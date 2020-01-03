/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#include "StdAfx.h"

#include "UdpCurlInstance.h"
#include "UdpServer.h"
#include "UdpSocketContext.h"
#include "MulticastUdpServer.h"
#include "MulticastUdpRawServer.h"
#include "LockMutex.h"
#include "conversions.h"
#include "Dns.h"
#include "IpAddress.h"
#include "RtpPacket.h"
#include "UdpDumpBox.h"
#include "ErrorCodes.h"

#define SLEEP_MODE_NO                                                 0
#define SLEEP_MODE_SHORT                                              1
#define SLEEP_MODE_LONG                                               2

CUdpCurlInstance::CUdpCurlInstance(HRESULT *result, CLogger *logger, HANDLE mutex, const wchar_t *protocolName, const wchar_t *instanceName)
  : CCurlInstance(result, logger, mutex, protocolName, instanceName)
{
  this->localAddress = NULL;
  this->sourceAddress = NULL;
  this->localPort = PORT_UNSPECIFIED;
  this->sourcePort = PORT_UNSPECIFIED;

  this->udpDownloadRequest = dynamic_cast<CUdpDownloadRequest *>(this->downloadRequest);
  this->udpDownloadResponse = dynamic_cast<CUdpDownloadResponse *>(this->downloadResponse);

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
  }
}

CUdpCurlInstance::~CUdpCurlInstance(void)
{
  this->StopReceivingData();

  FREE_MEM(this->localAddress);
  FREE_MEM(this->sourceAddress);
}

/* get methods */

CUdpDownloadResponse *CUdpCurlInstance::GetUdpDownloadResponse(void)
{
  return this->udpDownloadResponse;
}

/* set methods */

/* other methods */

HRESULT CUdpCurlInstance::Initialize(CDownloadRequest *downloadRequest)
{
  HRESULT result = __super::Initialize(downloadRequest);
  this->state = CURL_STATE_CREATED;

  this->udpDownloadRequest = dynamic_cast<CUdpDownloadRequest  *>(this->downloadRequest);
  this->udpDownloadResponse = dynamic_cast<CUdpDownloadResponse *>(this->downloadResponse);
  CHECK_POINTER_HRESULT(result, this->udpDownloadRequest, result, E_NOT_VALID_STATE);
  CHECK_POINTER_HRESULT(result, this->udpDownloadResponse, result, E_NOT_VALID_STATE);

  if (SUCCEEDED(result))
  {
    ALLOC_MEM_DEFINE_SET(urlComponents, URL_COMPONENTS, 1, 0);
    CHECK_POINTER_HRESULT(result, urlComponents, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      ZeroURL(urlComponents);
      urlComponents->dwStructSize = sizeof(URL_COMPONENTS);

      CHECK_CONDITION_HRESULT(result, InternetCrackUrl(downloadRequest->GetUrl(), 0, 0, urlComponents) == TRUE, result, E_FAIL);

      if (SUCCEEDED(result))
      {
        this->localAddress = Substring(urlComponents->lpszHostName, 0, urlComponents->dwHostNameLength);
        CHECK_POINTER_HRESULT(result, this->localAddress, result, E_OUTOFMEMORY);

        this->localPort = urlComponents->nPort;

        if (urlComponents->dwUserNameLength > 0)
        {
          this->sourceAddress = Substring(urlComponents->lpszUserName, 0, urlComponents->dwUserNameLength);
          CHECK_POINTER_HRESULT(result, this->sourceAddress, result, E_OUTOFMEMORY);
        }

        if (urlComponents->dwPasswordLength > 0)
        {
          // its port for source address
          this->sourcePort = GetValueUint(urlComponents->lpszPassword, PORT_UNSPECIFIED);
        }

        if (SUCCEEDED(result))
        {
          this->logger->Log(LOGGER_INFO, L"%s: %s: local address '%s', local port %u, source address '%s', source port %u", this->protocolName, METHOD_INITIALIZE_NAME,
            (this->localAddress == NULL) ? L"NULL" : this->localAddress,
            this->localPort,
            (this->sourceAddress == NULL) ? L"NULL" : this->sourceAddress,
            this->sourcePort);
        }
      }
    }

    FREE_MEM(urlComponents);
  }

  this->state = SUCCEEDED(result) ? CURL_STATE_INITIALIZED : CURL_STATE_CREATED;
  return result;
}

/* protected methods */

CDownloadResponse *CUdpCurlInstance::CreateDownloadResponse(void)
{
  HRESULT result = S_OK;
  CUdpDownloadResponse *response = new CUdpDownloadResponse(&result);
  CHECK_POINTER_HRESULT(result, response, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(response));
  return response;
}

unsigned int CUdpCurlInstance::CurlWorker(void)
{
  this->logger->Log(LOGGER_INFO, L"%s: %s: Start, url: '%s'", this->protocolName, METHOD_CURL_WORKER_NAME, this->downloadRequest->GetUrl());
  this->startReceivingTicks = GetTickCount();
  this->stopReceivingTicks = 0;
  this->totalReceivedBytes = 0;

  HRESULT result = S_OK;
  ALLOC_MEM_DEFINE_SET(buffer, unsigned char, BUFFER_LENGTH_DEFAULT, 0);
  CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

  // local address can be address of some network interface, can be localhost or can be mutlicast address
  // check if local address is mutlicast or unicast address

  CIpAddressCollection *localIpAddresses = new CIpAddressCollection(&result);
  CIpAddressCollection *sourceIpAddresses = new CIpAddressCollection(&result);
  CRtpPacket *rtpPacket = new CRtpPacket(&result);

  CHECK_POINTER_HRESULT(result, localIpAddresses, result, E_OUTOFMEMORY);
  CHECK_POINTER_HRESULT(result, sourceIpAddresses, result, E_OUTOFMEMORY);
  CHECK_POINTER_HRESULT(result, rtpPacket, result, E_OUTOFMEMORY);

  // local address collection must contain only one IP address - local unicast IP address or multicast address
  CUdpServer *server = NULL;

  if (SUCCEEDED(result))
  {
    result = CDns::GetIpAddresses(this->localAddress, this->localPort, AF_UNSPEC, SOCK_DGRAM, IPPROTO_UDP, 0, localIpAddresses);
    CHECK_CONDITION_HRESULT(result, localIpAddresses->Count() == 1, result, E_FAIL);

    if (SUCCEEDED(result) && (this->sourceAddress != NULL))
    {
      result = CDns::GetIpAddresses(this->sourceAddress, this->sourcePort, AF_UNSPEC, SOCK_DGRAM, IPPROTO_UDP, 0, sourceIpAddresses);
      CHECK_CONDITION_HRESULT(result, sourceIpAddresses->Count() == 1, result, E_FAIL);
    }

    if (SUCCEEDED(result))
    {
      CIpAddress *localIpAddress = localIpAddresses->GetItem(0);

      server = localIpAddress->IsMulticast() ? (this->udpDownloadRequest->IsRawSocket() ? new CMulticastUdpRawServer(&result) : new CMulticastUdpServer(&result)) : new CUdpServer(&result);
      CHECK_POINTER_HRESULT(result, server, result, E_OUTOFMEMORY);

      CNetworkInterfaceCollection *interfaces = new CNetworkInterfaceCollection(&result);
      CHECK_POINTER_HRESULT(result, interfaces, result, E_OUTOFMEMORY);

      CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), CNetworkInterface::GetAllNetworkInterfaces(interfaces, AF_UNSPEC), result);

      if (SUCCEEDED(result))
      {
        if (!IsNullOrEmpty(this->udpDownloadRequest->GetNetworkInterfaceName()))
        {
          // in request is set network interface name, leave only interface with specified name
          unsigned int i = 0;
          while (SUCCEEDED(result) && (i < interfaces->Count()))
          {
            CNetworkInterface *networkInterface = interfaces->GetItem(i);
            
            if (CompareWithNull(networkInterface->GetFriendlyName(), this->udpDownloadRequest->GetNetworkInterfaceName()) != 0)
            {
              interfaces->Remove(i);
            }
            else
            {
              i++;
            }
          }
        }
      }

      if (SUCCEEDED(result) && (localIpAddress->IsMulticast()))
      {
        if (this->udpDownloadRequest->IsRawSocket())
        {
          CMulticastUdpRawServer *multicastServer = dynamic_cast<CMulticastUdpRawServer *>(server);

          result = multicastServer->Initialize(AF_UNSPEC, localIpAddress, (this->sourceAddress != NULL) ? sourceIpAddresses->GetItem(0) : NULL, interfaces, this->udpDownloadRequest->GetIpv4Header(), this->udpDownloadRequest->GetIgmpInterval());
        }
        else
        {
          CMulticastUdpServer *multicastServer = dynamic_cast<CMulticastUdpServer *>(server);

          result = multicastServer->Initialize(AF_UNSPEC, localIpAddress, (this->sourceAddress != NULL) ? sourceIpAddresses->GetItem(0) : NULL, interfaces);
        }
      }
      else if (SUCCEEDED(result) && (!localIpAddress->IsMulticast()))
      {
        // if not multicast address, then binding to local address
        // we need to find correct network interface (with same IP address) and bind to it

        if (SUCCEEDED(result))
        {
          unsigned int i = 0;
          while (SUCCEEDED(result) && (i < interfaces->Count()))
          {
            CNetworkInterface *networkInterface = interfaces->GetItem(i);

            unsigned int j = 0;
            while (SUCCEEDED(result) && (j < networkInterface->GetUnicastAddresses()->Count()))
            {
              CIpAddress *ipAddress = networkInterface->GetUnicastAddresses()->GetItem(j);

              if (SUCCEEDED(result))
              {
                ipAddress->SetPort(localIpAddress->GetPort());

                if ((ipAddress->GetAddressLength() == localIpAddress->GetAddressLength()) &&
                  (memcmp(ipAddress->GetAddress(), localIpAddress->GetAddress(), ipAddress->GetAddressLength()) == 0))
                {
                  j++;
                }
                else
                {
                  networkInterface->GetUnicastAddresses()->Remove(j);
                }
              }
            }

            if (SUCCEEDED(result))
            {
              if (networkInterface->GetUnicastAddresses()->Count() != 0)
              {
                i++;
              }
              else
              {
                interfaces->Remove(i);
              }
            }
          }
        }

        CHECK_CONDITION_HRESULT(result, interfaces->Count() != 0, result, E_FAIL);
        CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), server->Initialize(AF_UNSPEC, this->localPort, interfaces), result);
      }
      else
      {
        result = E_FAIL;
      }

      FREE_MEM_CLASS(interfaces);
      CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), server->StartListening(), result);

      unsigned int endTicks = (this->downloadRequest->GetFinishTime() == FINISH_TIME_NOT_SPECIFIED) ? (GetTickCount() + this->downloadRequest->GetReceiveDataTimeout()) : this->downloadRequest->GetFinishTime();
      unsigned int sleepMode = SLEEP_MODE_LONG;

      while (SUCCEEDED(result) && (!this->curlWorkerShouldExit))
      {
        sleepMode = SLEEP_MODE_LONG;

        if (SUCCEEDED(result))
        {
          // only one thread can work with UDP data in one time
          LOCK_MUTEX(this->mutex, INFINITE)

          // maintain connections (if needed)
          server->MaintainConnections();

          for (unsigned int i = 0; (SUCCEEDED(result) && (i < server->GetSockets()->Count())); i++)
          {
            CUdpSocketContext *udpContext = (CUdpSocketContext *)(server->GetSockets()->GetItem(i));

            size_t pendingIncomingDataLength = 0;
            HRESULT res = S_OK;
            do
            {
              res = udpContext->GetPendingIncomingDataLength(&pendingIncomingDataLength);

              if (SUCCEEDED(res) && (pendingIncomingDataLength != 0))
              {
                // allocate buffer and receive data
                size_t receivedLength = 0;

                CHECK_CONDITION_EXECUTE(SUCCEEDED(res), res = udpContext->Receive((char *)buffer, pendingIncomingDataLength, &receivedLength));
                CHECK_CONDITION_HRESULT(res, pendingIncomingDataLength == receivedLength, res, E_NOT_VALID_STATE);

                if (SUCCEEDED(res))
                {
                  sleepMode = SLEEP_MODE_SHORT;

                  this->totalReceivedBytes += receivedLength;
                  this->lastReceiveTime = GetTickCount();

                  CDumpBox *dumpBox = NULL;
                  CHECK_CONDITION_NOT_NULL_EXECUTE(this->dumpFile->GetDumpFile(), dumpBox = this->CreateDumpBox());

                  if (dumpBox != NULL)
                  {
                    dumpBox->SetInputData(true);
                    dumpBox->SetTimeWithLocalTime();
                    dumpBox->SetPayload(buffer, receivedLength);
                  }

                  CHECK_CONDITION_EXECUTE((dumpBox != NULL) && (!this->dumpFile->AddDumpBox(dumpBox)), FREE_MEM_CLASS(dumpBox));

                  if (this->IsSetFlags(UDP_CURL_INSTANCE_FLAG_TRANSPORT_RTP))
                  {
                    rtpPacket->Clear();

                    if (rtpPacket->Parse(buffer, receivedLength))
                    {
                      CHECK_CONDITION_HRESULT(res, this->udpDownloadResponse->GetReceivedData()->AddToBufferWithResize(rtpPacket->GetPayload(), rtpPacket->GetPayloadSize()) == rtpPacket->GetPayloadSize(), res, E_OUTOFMEMORY);
                    }
                  }
                  else if (this->IsSetFlags(UDP_CURL_INSTANCE_FLAG_TRANSPORT_UDP))
                  {
                    CHECK_CONDITION_HRESULT(res, this->udpDownloadResponse->GetReceivedData()->AddToBufferWithResize(buffer, pendingIncomingDataLength) == pendingIncomingDataLength, res, E_OUTOFMEMORY);
                  }
                  else
                  {
                    // transport type is not resolved, try first RTP
                    rtpPacket->Clear();

                    if (rtpPacket->Parse(buffer, receivedLength))
                    {
                      CHECK_CONDITION_HRESULT(res, this->udpDownloadResponse->GetReceivedData()->AddToBufferWithResize(rtpPacket->GetPayload(), rtpPacket->GetPayloadSize()) == rtpPacket->GetPayloadSize(), res, E_OUTOFMEMORY);
                      this->flags |= UDP_CURL_INSTANCE_FLAG_TRANSPORT_RTP;
                    }
                    else
                    {
                      CHECK_CONDITION_HRESULT(res, this->udpDownloadResponse->GetReceivedData()->AddToBufferWithResize(buffer, pendingIncomingDataLength) == pendingIncomingDataLength, res, E_OUTOFMEMORY);
                      this->flags |= UDP_CURL_INSTANCE_FLAG_TRANSPORT_UDP;
                    }
                  }
                }
              }
            }
            while (SUCCEEDED(res) && (!this->curlWorkerShouldExit) && (pendingIncomingDataLength != 0));

            if (FAILED(res))
            {
              this->logger->Log(LOGGER_ERROR, L"%s: %s: error while receiving data: 0x%08X", this->protocolName, METHOD_CURL_WORKER_NAME, res);
              result = res;
            }
          }

          UNLOCK_MUTEX(this->mutex)
        }

        if (SUCCEEDED(result))
        {
          // check timeout if no data received until now
          if ((this->totalReceivedBytes == 0) && (GetTickCount() > endTicks))
          {
            this->logger->Log(LOGGER_ERROR, L"%s: %s: no data received", this->protocolName, METHOD_CURL_WORKER_NAME);
            result = E_UDP_NO_DATA_RECEIVED;
          }

          // check last received data time
          if ((this->totalReceivedBytes != 0) && (GetTickCount() > (this->lastReceiveTime + this->udpDownloadRequest->GetCheckInterval())))
          {
            this->logger->Log(LOGGER_ERROR, L"%s: %s: no data received last %u ms", this->protocolName, METHOD_CURL_WORKER_NAME, this->udpDownloadRequest->GetCheckInterval());
            result = E_UDP_NO_DATA_RECEIVED;
          }
        }

        if (FAILED(result) && (this->state != CURL_STATE_RECEIVED_ALL_DATA))
        {
          // we have some error, we can't do more
          // report error code and wait for destroying CURL instance

          LOCK_MUTEX(this->mutex, INFINITE)

          this->udpDownloadResponse->SetResultError(result);
          this->state = CURL_STATE_RECEIVED_ALL_DATA;

          UNLOCK_MUTEX(this->mutex)
        }

        switch (sleepMode)
        {
        case SLEEP_MODE_SHORT:
          Sleep(1);
          break;
        case SLEEP_MODE_LONG:
          Sleep(20);
          break;
        default:
          break;
        }
      }
    }
  }

  FREE_MEM_CLASS(localIpAddresses);
  FREE_MEM_CLASS(sourceIpAddresses);
  FREE_MEM_CLASS(rtpPacket);
  FREE_MEM(buffer);

  CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while sending, receiving or processing data: 0x%08X", this->protocolName, METHOD_CURL_WORKER_NAME, result));
  this->udpDownloadResponse->SetResultError(result);

  unsigned int count = 0;
  {
    LOCK_MUTEX(this->mutex, INFINITE)

    for (unsigned int i = 0; ((server != NULL) && (i < server->GetSockets()->Count())); i++)
    {
      CSocketContext *context = server->GetSockets()->GetItem(i);

      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: address: %s, received bytes: %lld, sent bytes: %lld", this->protocolName, METHOD_CURL_WORKER_NAME, (context->GetIpAddress()->GetAddressString() == NULL) ? L"unknown" : context->GetIpAddress()->GetAddressString(), context->GetReceivedDataLength(), context->GetSentDataLength());
    }

    UNLOCK_MUTEX(this->mutex)
  }

  this->state = CURL_STATE_RECEIVED_ALL_DATA;
  this->stopReceivingTicks = GetTickCount();

  FREE_MEM_CLASS(server);

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, this->protocolName, METHOD_CURL_WORKER_NAME);
  return S_OK;
}

CDumpBox *CUdpCurlInstance::CreateDumpBox(void)
{
  HRESULT result = S_OK;
  CUdpDumpBox *box = new CUdpDumpBox(&result);
  CHECK_POINTER_HRESULT(result, box, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(box));
  return box;
}