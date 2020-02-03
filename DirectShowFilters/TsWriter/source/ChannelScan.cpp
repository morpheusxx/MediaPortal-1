/* 
 *  Copyright (C) 2006-2008 Team MediaPortal
 *  http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#pragma warning(disable : 4995)
#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <streams.h>
#include <algorithm>

#include "ChannelScan.h"
#include "..\..\shared\ChannelInfo.h"
#include "TsWriter.h"

extern void LogDebug(const char *fmt, ...) ;

CChannelScan::CChannelScan(LPUNKNOWN pUnk, HRESULT *phr, CMpTsFilter* filter) 
  : CUnknown(NAME("MpTsChannelScan"), pUnk)
{
  m_bIsScanning = false;
  m_bIsScanningNetwork = false;
  m_pFilter = filter;
  m_pCallBack = NULL;
  m_pEncryptionAnalyser = new CEncryptionAnalyser(GetOwner(), phr);
  if (m_pEncryptionAnalyser == NULL)
  {
    *phr = E_OUTOFMEMORY;
  }
}

CChannelScan::~CChannelScan(void)
{
  CleanUp();
  delete m_pEncryptionAnalyser;
  m_pEncryptionAnalyser = NULL;
}

void CChannelScan::CleanUp()
{
  map<int, CChannelInfo*>::iterator serviceIt = m_mServices.begin();
  while (serviceIt != m_mServices.end())
  {
    CChannelInfo* info = serviceIt->second;
    // The CChannelInfo destructor takes care of the char* members.
    delete info;
    info = NULL;
    serviceIt++;
  }
  m_mServices.clear();

  vector<CPmtParser*>::iterator pmtIt = m_vPmtParsers.begin();
  while (pmtIt != m_vPmtParsers.end())
  {
    CPmtParser* parser = *pmtIt;
    delete parser;
    parser = NULL;
    pmtIt++;
  }
  m_vPmtParsers.clear();
}

STDMETHODIMP CChannelScan::SetCallBack(IChannelScanCallBack* callBack)
{
  LogDebug("ChannelScan: set callback 0x%x", callBack);
  m_pCallBack = callBack;
  return S_OK;
}

STDMETHODIMP CChannelScan::ScanStream(BroadcastStandard broadcastStandard)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    CleanUp();
    m_mPids.clear();

    LogDebug("ChannelScan: start scanning stream, broadcast standard = %d", broadcastStandard);
    m_broadcastStandard = broadcastStandard;
    m_bIsScanningNetwork = false;
    m_bIsScanning = true;
    m_patParser.Reset();
    m_pEncryptionAnalyser->Reset();
    m_sdtParser.Reset(false);
    m_nitParser.Reset();
    m_batParser.Reset();
    m_lvctParser.Reset();
    m_patParser.SetCallBack(this);
    m_pEncryptionAnalyser->SetCallBack(this);
    m_sdtParser.SetCallBack(this);
    m_lvctParser.SetCallBack(this);
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in ScanStream()");
    return S_FALSE;
  }
  return S_OK;
}

STDMETHODIMP CChannelScan::StopStreamScan()
{
  CEnterCriticalSection enter(m_section);
  try
  {
    LogDebug("ChannelScan: stop scanning stream");
    m_bIsScanning = false;
    m_pCallBack = NULL;
    m_patParser.SetCallBack(NULL);
    m_pEncryptionAnalyser->SetCallBack(NULL);
    m_sdtParser.SetCallBack(NULL);
    m_lvctParser.SetCallBack(NULL);
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in StopStreamScan()");
    return S_FALSE;
  }
  return S_OK;
}

STDMETHODIMP CChannelScan::GetServiceCount(int* serviceCount)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    *serviceCount = (int)m_mServices.size();
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in GetServiceCount()");
    *serviceCount = 0;
    return S_FALSE;
  }
  return S_OK;
}

STDMETHODIMP CChannelScan::GetServiceDetail(int index,
                                            int* originalNetworkId,
                                            int* transportStreamId,
                                            int* serviceId,
                                            char** serviceName,
                                            char** providerName,
                                            char** logicalChannelNumber,
                                            int* serviceType,
                                            int* videoStreamCount,
                                            int* audioStreamCount,
                                            bool* isHighDefinition,
                                            bool* isEncrypted,
                                            bool* isRunning,
                                            int* pmtPid,
                                            int* previousOriginalNetworkId,
                                            int* previousTransportStreamId,
                                            int* previousServiceId,
                                            int* networkIdCount,
                                            unsigned short** networkIds,
                                            int* bouquetIdCount,
                                            unsigned short** bouquetIds,
                                            int* languageCount,
                                            unsigned long** languages,
                                            int* availableInCellCount,
                                            unsigned long** availableInCells,
                                            int* unavailableInCellCount,
                                            unsigned long** unavailableInCells,
                                            int* targetRegionCount,
                                            __int64** targetRegions,
                                            int* availableInCountryCount,
                                            unsigned long** availableInCountries,
                                            int* unavailableInCountryCount,
                                            unsigned long** unavailableInCountries)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    if (index < 0 || index >= (int)m_mServices.size())
    {
      LogDebug("ChannelScan: attempted to retrieve service details with invalid index %d, service count = %d", index, m_mServices.size());
      return S_FALSE;
    }

    int originalIndex = index + 1;
    map<int, CChannelInfo*>::iterator it = m_mServices.begin();
    while (index > 0)
    {
      it++;
      index--;
    }

    CChannelInfo* info = it->second;
    *originalNetworkId = info->OriginalNetworkId;
    *transportStreamId = info->TransportStreamId;
    *serviceId = info->ServiceId;
    *serviceName = info->ServiceName;
    *providerName = info->ProviderName;

    if (m_broadcastStandard == Dvb)
    {
      int tempLcn = m_nitParser.GetLogicialChannelNumber(info->OriginalNetworkId, info->TransportStreamId, info->ServiceId);
      if (tempLcn <= 0 || tempLcn == 10000)
      {
        tempLcn = m_batParser.GetLogicialChannelNumber(info->OriginalNetworkId, info->TransportStreamId, info->ServiceId);
      }
      info->LogicalChannelNumber = new char[10];
      if (info->LogicalChannelNumber != NULL)
      {
        if (tempLcn == 0)
        {
          strcpy(info->LogicalChannelNumber, "");
        }
        else
        {
          sprintf(info->LogicalChannelNumber, "%d", tempLcn);
        }
      }
    }
    *logicalChannelNumber = info->LogicalChannelNumber;

    *serviceType = info->ServiceType;
    *videoStreamCount = info->VideoStreamCount;
    *audioStreamCount = info->AudioStreamCount;
    *isHighDefinition = info->IsHighDefinition;
    *isEncrypted = info->IsEncrypted;
    *isRunning = info->IsRunning;
    *pmtPid = info->PmtPid;
    *previousOriginalNetworkId = info->PreviousOriginalNetworkId;
    *previousTransportStreamId = info->PreviousTransportStreamId;
    *previousServiceId = info->PreviousServiceId;

    if (m_broadcastStandard == Dvb)
    {
      // Add network and bouquet IDs from the NIT and BAT.
      m_nitParser.GetNetworkIds(info->OriginalNetworkId, info->TransportStreamId, info->ServiceId, &(info->NetworkIds));
      m_batParser.GetBouquetIds(info->OriginalNetworkId, info->TransportStreamId, info->ServiceId, &(info->BouquetIds));

      // Add available-in-cell information from the NIT.
      vector<int> nitAvailableInCells;
      m_nitParser.GetAvailableInCells(info->OriginalNetworkId, info->TransportStreamId, info->ServiceId, &nitAvailableInCells);
      vector<int>::iterator cellIt = nitAvailableInCells.begin();
      while (cellIt != nitAvailableInCells.end())
      {
        if (find((info->AvailableInCells).begin(), (info->AvailableInCells).end(), *cellIt) == (info->AvailableInCells).end())
        {
          (info->AvailableInCells).push_back(*cellIt);
        }
        cellIt++;
      }

      // The target region information is scoped. Currently the channel info only contains information from the SDT.
      // If there is no information from the SDT, try the BAT; if there is no information from the BAT, try the NIT.
      if (info->TargetRegions.size() == 0)
      {
        m_batParser.GetTargetRegionIds(info->OriginalNetworkId, info->TransportStreamId, info->ServiceId, &(info->TargetRegions));
        if (info->TargetRegions.size() == 0)
        {
          m_nitParser.GetTargetRegionIds(info->OriginalNetworkId, info->TransportStreamId, info->ServiceId, &(info->TargetRegions));
        }
      }

      // Add country availability from the BAT.
      vector<unsigned int> batCountries;
      m_batParser.GetAvailableInCountries(info->OriginalNetworkId, info->TransportStreamId, info->ServiceId, &batCountries);
      vector<unsigned int>::iterator countryIt = batCountries.begin();
      while (countryIt != batCountries.end())
      {
        if (find((info->AvailableInCountries).begin(), (info->AvailableInCountries).end(), *countryIt) == (info->AvailableInCountries).end())
        {
          (info->AvailableInCountries).push_back(*countryIt);
        }
        countryIt++;
      }
      batCountries.clear();
      m_batParser.GetUnavailableInCountries(info->OriginalNetworkId, info->TransportStreamId, info->ServiceId, &batCountries);
      countryIt = batCountries.begin();
      while (countryIt != batCountries.end())
      {
        if (find((info->UnavailableInCountries).begin(), (info->UnavailableInCountries).end(), *countryIt) == (info->UnavailableInCountries).end())
        {
          (info->UnavailableInCountries).push_back(*countryIt);
        }
        countryIt++;
      }
    }

    // Make the language list distinct.
    map<unsigned long, bool> tempLangs;
    vector<unsigned long>::iterator langIt = (info->Languages).begin();
    while (langIt != (info->Languages).end())
    {
      tempLangs[*langIt] = true;
      langIt++;
    }
    (info->Languages).clear();
    map<unsigned long, bool>::iterator langIt2 = tempLangs.begin();
    while (langIt2 != tempLangs.end())
    {
      LogDebug("ChannelScan: add language '%s' for service 0x%x", (char*)&(langIt2->first), info->ServiceId);
      (info->Languages).push_back(langIt2->first);
      langIt2++;
    }

    *networkIdCount = min((long)(info->NetworkIds).size(), *networkIdCount);
    if (*networkIdCount != 0)
    {
      copy(info->NetworkIds.begin(), info->NetworkIds.begin() + *networkIdCount, *networkIds);
    }
    *bouquetIdCount = min((long)(info->BouquetIds).size(), *bouquetIdCount);
    if (*bouquetIdCount != 0)
    {
      copy(info->BouquetIds.begin(), info->BouquetIds.begin() + *bouquetIdCount, *bouquetIds);
    }
    *languageCount = min((long)(info->Languages).size(), *languageCount);
    if (*languageCount != 0)
    {
      copy(info->Languages.begin(), info->Languages.begin() + *languageCount, *languages);
    }
    *availableInCellCount = min((long)(info->AvailableInCells).size(), *availableInCellCount);
    if (*availableInCellCount != 0)
    {
      copy(info->AvailableInCells.begin(), info->AvailableInCells.begin() + *availableInCellCount, *availableInCells);
    }
    *unavailableInCellCount = min((long)(info->UnavailableInCells).size(), *unavailableInCellCount);
    if (*unavailableInCellCount != 0)
    {
      copy(info->UnavailableInCells.begin(), info->UnavailableInCells.begin() + *unavailableInCellCount, *unavailableInCells);
    }
    *targetRegionCount = min((long)(info->TargetRegions).size(), *targetRegionCount);
    if (*targetRegionCount != 0)
    {
      copy(info->TargetRegions.begin(), info->TargetRegions.begin() + *targetRegionCount, *targetRegions);
    }
    *availableInCountryCount = min((long)(info->AvailableInCountries).size(), *availableInCountryCount);
    if (*availableInCountryCount != 0)
    {
      copy(info->AvailableInCountries.begin(), info->AvailableInCountries.begin() + *availableInCountryCount, *availableInCountries);
    }
    *unavailableInCountryCount = min((long)(info->UnavailableInCountries).size(), *unavailableInCountryCount);
    if (*unavailableInCountryCount != 0)
    {
      copy(info->UnavailableInCountries.begin(), info->UnavailableInCountries.begin() + *unavailableInCountryCount, *unavailableInCountries);
    }

    LogDebug("%4d) %-25s provider = %-15s, ONID = 0x%-4x, TSID = 0x%-4x, SID = 0x%-4x, PMT PID = 0x%-4x, LCN = %-7s",
            originalIndex, info->ServiceName, info->ProviderName, info->OriginalNetworkId, info->TransportStreamId, info->ServiceId, info->PmtPid, info->LogicalChannelNumber);
    LogDebug("       type = %-3d, video stream count = %1d, audio stream count = %1d, is high definition = %1d, is encrypted = %1d, is running = %1d, is other mux = %1d",
            info->ServiceType, info->VideoStreamCount, info->AudioStreamCount, info->IsHighDefinition, info->IsEncrypted, info->IsRunning, info->IsOtherTransportStream);
    LogDebug("       is PMT received = %1d, is SDT/VCT received = %1d, is PID received = %1d",
            info->IsPmtReceived, info->IsServiceInfoReceived, info->IsPidReceived);
    if (info->PreviousServiceId != 0)
    {
      LogDebug("***[Moved from SID 0x%4x, TSID 0x%4x, ONID 0x%4x]***",
              info->PreviousServiceId, info->PreviousTransportStreamId, info->PreviousOriginalNetworkId);
    }
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in GetServiceDetail()");
    return S_FALSE;
  }
  return S_OK;
}

STDMETHODIMP CChannelScan::ScanNetwork()
{
  CEnterCriticalSection enter(m_section);
  try
  {
    CleanUp();
    m_mPids.clear();

    LogDebug("ChannelScan: start scanning network");
    m_bIsScanning = true;
    m_bIsScanningNetwork = true;
    m_broadcastStandard = Dvb;   // Only DVB network scanning is supported. Other standards don't seem to have network information tables.
    m_bIsOtherMuxServiceInfoSeen = false;
    m_patParser.Reset();
    m_pEncryptionAnalyser->Reset();
    m_sdtParser.Reset(true);
    m_nitParser.Reset();
    m_batParser.Reset();
    m_patParser.SetCallBack(this);
    m_pEncryptionAnalyser->SetCallBack(this);
    m_sdtParser.SetCallBack(this);
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in ScanNetwork()");
    return S_FALSE;
  }
  return S_OK;
}

STDMETHODIMP CChannelScan::StopNetworkScan(bool* isOtherMuxServiceInfoAvailable)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    LogDebug("ChannelScan: stop scanning network");
    m_bIsScanning = false;
    m_bIsScanningNetwork = false;
    m_pCallBack = NULL;
    m_patParser.SetCallBack(NULL);
    m_pEncryptionAnalyser->SetCallBack(NULL);
    m_sdtParser.SetCallBack(NULL);
    m_lvctParser.SetCallBack(NULL);

    *isOtherMuxServiceInfoAvailable = m_bIsOtherMuxServiceInfoSeen;
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in StopNetworkScan()");
    return S_FALSE;
  }
  return S_OK;
}

STDMETHODIMP CChannelScan::GetMultiplexCount(int* multiplexCount)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    *multiplexCount = m_nitParser.GetMultiplexCount();
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in GetMultiplexCount()");
    *multiplexCount = 0;
    return S_FALSE;
  }
  return S_OK;
}

STDMETHODIMP CChannelScan::GetMultiplexDetail(int index,
                                              int* networkId,
                                              int* transportStreamId,
                                              int* type,
                                              int* frequency,
                                              int *polarisation,
                                              int* modulation,
                                              int* symbolRate,
                                              int* bandwidth,
                                              int* innerFecRate,
                                              int* rollOff,
                                              int* longitude,
                                              int* cellId,
                                              int* cellIdExtension,
                                              int* plpId)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    int multiplexCount = m_nitParser.GetMultiplexCount();
    if (index < 0 || index >= multiplexCount)
    {
      LogDebug("ChannelScan: attempted to retrieve multiplex details with invalid index %d, multiplex count = %d", index, multiplexCount);
      return S_FALSE;
    }
    NitMultiplexDetail* mux = m_nitParser.GetMultiplexDetail(index);
    if (mux == NULL)
    {
      LogDebug("ChannelScan: multiplex is NULL, index = %d, multiplex count = %d", index, multiplexCount);
      return S_FALSE;
    }

    *networkId = mux->OriginalNetworkId;
    *transportStreamId = mux->TransportStreamId;

    NitCableMultiplexDetail* cableMux = dynamic_cast<NitCableMultiplexDetail*>(mux);
    if (cableMux != NULL)
    {
      *frequency = cableMux->Frequency;
      *modulation = cableMux->Modulation;
      *symbolRate = cableMux->SymbolRate;
      *innerFecRate = cableMux->InnerFecRate;
      *type = 2;  // This is as-per the TV Server database channel types.

      *polarisation = BDA_POLARISATION_NOT_SET;
      *bandwidth = 0;
      *rollOff = BDA_ROLL_OFF_NOT_SET;
      *longitude = 0;
      *cellId = 0;
      *cellIdExtension = 0;
      *plpId = 0;
      return S_OK;
    }

    NitSatelliteMultiplexDetail* satelliteMux = dynamic_cast<NitSatelliteMultiplexDetail*>(mux);
    if (satelliteMux != NULL)
    {
      *frequency = satelliteMux->Frequency;
      *polarisation = satelliteMux->Polarisation;
      *modulation = satelliteMux->Modulation;
      *symbolRate = satelliteMux->SymbolRate;
      *innerFecRate = satelliteMux->InnerFecRate;
      *rollOff = satelliteMux->RollOff;
      *longitude = ((satelliteMux->WestEastFlag == 1 ? 1 : -1) * satelliteMux->OrbitalPosition);
      *plpId = satelliteMux->InputStreamIdentifier;
      *type = 3;  // This is as-per the TV Server database channel types.

      *bandwidth = 0;
      *cellId = 0;
      *cellIdExtension = 0;
      return S_OK;
    }

    NitTerrestrialMultiplexDetail* terrestrialMux = dynamic_cast<NitTerrestrialMultiplexDetail*>(mux);
    if (terrestrialMux != NULL)
    {
      *frequency = terrestrialMux->CentreFrequency;
      *bandwidth = terrestrialMux->Bandwidth;
      *cellId = terrestrialMux->CellId;
      *cellIdExtension = terrestrialMux->CellIdExtension;
      *plpId = terrestrialMux->PlpId;
      *type = 4;  // This is as-per the TV Server database channel types.

      *polarisation = BDA_POLARISATION_NOT_SET;
      *modulation = BDA_MOD_NOT_SET;
      *symbolRate = 0;
      *innerFecRate = BDA_BCC_RATE_NOT_SET;
      *rollOff = BDA_ROLL_OFF_NOT_SET;
      *longitude = 0;
      return S_OK;
    }

    LogDebug("ChannelScan: unhandled multiplex type in GetMultiplexDetail()");
    return S_FALSE;
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in GetMultiplexDetail()");
    return S_FALSE;
  }
  return S_OK;
}

STDMETHODIMP CChannelScan::GetTargetRegionName(__int64 targetRegionId, char** name)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    // According to EN 300 468, BAT names take precidence over NIT names.
    unsigned int lang;
    m_batParser.GetTargetRegionName(targetRegionId, 0, &lang, name);
    if (*name == NULL)
    {
      m_nitParser.GetTargetRegionName(targetRegionId, 0, &lang, name);
    }
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in GetTargetRegionName()");
    *name = NULL;
    return S_FALSE;
  }
  return S_OK;
}

STDMETHODIMP CChannelScan::GetBouquetName(int bouquetId, char** name)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    *name = m_sdtParser.GetBouquetName(bouquetId);
    if (*name == NULL)
    {
      unsigned int lang;
      m_batParser.GetBouquetName(bouquetId, 0, &lang, name);
    }
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in GetBouquetName()");
    *name = NULL;
    return S_FALSE;
  }
  return S_OK;
}

STDMETHODIMP CChannelScan::GetNetworkName(int networkId, char** name)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    unsigned int lang;
    m_nitParser.GetNetworkName(networkId, 0, &lang, name);
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in GetNetworkName()");
    *name = NULL;
    return S_FALSE;
  }
  return S_OK;
}

void CChannelScan::OnTsPacket(byte* tsPacket)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    if (!m_bIsScanning)
    {
      return;
    }

    // Note: we continue parsing even when we think scanning is complete. The user
    // has the option to set a minimum scan time...
    bool isReady = true;
    if (m_broadcastStandard == Atsc || m_broadcastStandard == Scte)
    {
      isReady = m_lvctParser.IsReady();
    }
    else
    {
      isReady = m_nitParser.IsReady() && m_batParser.IsReady() && m_sdtParser.IsReady();
    }
    if (isReady)
    {
      isReady = m_patParser.IsReady();
      if (isReady)
      {
        map<int, CChannelInfo*>::iterator it = m_mServices.begin();
        while (it != m_mServices.end())
        {
          // If the information from the encryption analyser hasn't been received and the
          // service is meant to be in this stream and is apparently running, then we're not ready.
          if (!(*it->second).IsPidReceived && !(*it->second).IsOtherTransportStream && (*it->second).IsRunning)
          {
            isReady = false;
            break;
          }
          it++;
        }
      }
    }
    if (isReady)
    {
      LogDebug("ChannelScan: scanner finished");
      if (m_pCallBack != NULL)
      {
        LogDebug("ChannelScan: triggering callback");
        m_pCallBack->OnScannerDone();
      }
      m_pCallBack = NULL;
    }

    if (m_broadcastStandard == Atsc || m_broadcastStandard == Scte)
    {
      m_lvctParser.OnTsPacket(tsPacket);
    }
    else
    {
      m_nitParser.OnTsPacket(tsPacket);
      m_batParser.OnTsPacket(tsPacket);
      m_sdtParser.OnTsPacket(tsPacket);
    }

    m_patParser.OnTsPacket(tsPacket);
    vector<CPmtParser*>::iterator it2 = m_vPmtParsers.begin();
    while (it2 != m_vPmtParsers.end())
    {
      CPmtParser* parser = *it2;
      parser->OnTsPacket(tsPacket);
      it2++;
    }
    m_pEncryptionAnalyser->OnTsPacket(tsPacket);
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in OnTsPacket()");
  }
}

void CChannelScan::OnPatReceived(int serviceId, int pmtPid)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    if (m_pCallBack == NULL)
    {
      return;
    }

    map<int, CChannelInfo*>::iterator it = m_mServices.find(serviceId);
    if (it == m_mServices.end())
    {
      CChannelInfo* info = new CChannelInfo();
      info->ServiceId = serviceId;
      info->PmtPid = pmtPid;
      m_mServices[serviceId] = info;
    }
    else
    {
      it->second->PmtPid = pmtPid;
    }

    // We're not expecting there to be a PMT parser yet, but double check just in case.
    bool foundParser = false;
    vector<CPmtParser*>::iterator it2 = m_vPmtParsers.begin();
    CPmtParser* parser = NULL;
    while (it2 != m_vPmtParsers.end())
    {
      parser = *it2;
      unsigned short sid;
      unsigned short pid;
      parser->GetFilter(pid, sid);
      if (pid == pmtPid && sid == serviceId)
      {
        LogDebug("ChannelScan: PMT parser already exists");
        return;
      }
      it2++;
    }
    parser = new CPmtParser();
    parser->SetFilter(pmtPid, serviceId);
    parser->SetCallBack(this);
    m_vPmtParsers.push_back(parser);
    LogDebug("ChannelScan: added PMT parser for service 0x%x, PMT PID = 0x%x", serviceId, pmtPid);
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in OnPatReceived()");
  }
}

void CChannelScan::OnSdtReceived(const CChannelInfo& sdtInfo)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    map<int, CChannelInfo*>::iterator it = m_mServices.find(sdtInfo.ServiceId);

    // Do we have a channel with this service ID?
    CChannelInfo* info = NULL;
    if (it != m_mServices.end())
    {
      info = it->second;
      if (info->IsServiceInfoReceived)
      {
        LogDebug("ChannelScan: SDT information for service 0x%x received multiple times", sdtInfo.ServiceId);
      }
    }
    else
    {
      info = new CChannelInfo();
      m_mServices[sdtInfo.ServiceId] = info;
    }

    info->OriginalNetworkId = sdtInfo.OriginalNetworkId;
    info->TransportStreamId = sdtInfo.TransportStreamId;
    info->ServiceId = sdtInfo.ServiceId;
    info->ServiceType = sdtInfo.ServiceType;
    // We trust PMT information more than we trust component descriptors
    // in the SDT.
    if (!info->IsPmtReceived)
    {
      info->VideoStreamCount = sdtInfo.VideoStreamCount;
      info->AudioStreamCount = sdtInfo.AudioStreamCount;
    }
    info->IsHighDefinition |= sdtInfo.IsHighDefinition;
    // We trust running_status and free_ca_mode over PMT being
    // received (or not) and CA descriptors being found in the PMT
    // (or not). However, information from the encryption analyser
    // takes higher precedence.
    if (!info->IsPidReceived)
    {
      info->IsEncrypted = sdtInfo.IsEncrypted;
      info->IsRunning = sdtInfo.IsRunning;
    }
    info->IsOtherTransportStream = sdtInfo.IsOtherTransportStream;
    if (sdtInfo.IsOtherTransportStream)
    {
      m_bIsOtherMuxServiceInfoSeen = true;
    }

    // Be careful not to wipe these details as they're really important.
    if (sdtInfo.PreviousServiceId != 0)
    {
      info->PreviousServiceId = sdtInfo.PreviousServiceId;
      info->PreviousTransportStreamId = sdtInfo.PreviousTransportStreamId;
      info->PreviousOriginalNetworkId = sdtInfo.PreviousOriginalNetworkId;
    }

    info->ReplaceServiceName(sdtInfo.ServiceName);
    info->ReplaceProviderName(sdtInfo.ProviderName);

    // The SDT is the primary source of information for the following fields.
    // We suppliment with details from the NIT and BAT when the channel is
    // retrieved.
    info->BouquetIds = sdtInfo.BouquetIds;
    info->AvailableInCells = sdtInfo.AvailableInCells;
    info->UnavailableInCells = sdtInfo.UnavailableInCells;
    info->TargetRegions = sdtInfo.TargetRegions;
    info->AvailableInCountries = sdtInfo.AvailableInCountries;
    info->UnavailableInCountries = sdtInfo.UnavailableInCountries;

    // Add the languages. We don't want to overwrite languages found in
    // the PMT. We'll take care of making the list distinct later.
    vector<unsigned long>::const_iterator langIt = sdtInfo.Languages.begin();
    while (langIt != sdtInfo.Languages.end())
    {
      //LogDebug("ChannelScan:   add component language '%s'", (char*)&(*langIt));
      info->Languages.push_back(*langIt);
      langIt++;
    }

    info->IsServiceInfoReceived = true;
    LogDebug("ChannelScan: received SDT information for service 0x%x", sdtInfo.ServiceId);
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in OnSdtReceived()");
  }
}

void CChannelScan::OnLvctReceived(const CChannelInfo& vctInfo)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    map<int, CChannelInfo*>::iterator it = m_mServices.find(vctInfo.ServiceId);

    // Do we have a channel with this service ID?
    CChannelInfo* info = NULL;
    if (it != m_mServices.end())
    {
      info = it->second;
      if (info->IsServiceInfoReceived)
      {
        LogDebug("ChannelScan: L-VCT information for service 0x%x received multiple times", vctInfo.ServiceId);
      }
    }
    else
    {
      info = new CChannelInfo();
      m_mServices[vctInfo.ServiceId] = info;
    }

    info->OriginalNetworkId = vctInfo.OriginalNetworkId;
    info->TransportStreamId = vctInfo.TransportStreamId;
    info->ServiceId = vctInfo.ServiceId;
    info->ServiceType = vctInfo.ServiceType;
    // We trust PMT information over anything else for A/V present
    // status.
    if (!info->IsPmtReceived)
    {
      info->VideoStreamCount = vctInfo.VideoStreamCount;
      info->AudioStreamCount = vctInfo.AudioStreamCount;
    }
    // We trust L-VCT info and access_controlled over PMT being
    // received (or not) and CA descriptors being found in the PMT
    // (or not). However, information from the encryption analyser
    // takes higher precedence.
    if (!info->IsPidReceived)
    {
      info->IsEncrypted = vctInfo.IsEncrypted;
      info->IsRunning = vctInfo.IsRunning;
    }
    info->IsOtherTransportStream = vctInfo.IsOtherTransportStream;
    if (vctInfo.IsOtherTransportStream)
    {
      m_bIsOtherMuxServiceInfoSeen = true;
    }

    info->ReplaceServiceName(vctInfo.ServiceName);
    info->ReplaceProviderName(vctInfo.ProviderName);
    info->ReplaceLogicalChannelNumber(vctInfo.LogicalChannelNumber);

    // Add the languages. We don't want to overwrite languages found in
    // the PMT. We'll take care of making the list distinct later.
    vector<unsigned long>::const_iterator langIt = vctInfo.Languages.begin();
    while (langIt != vctInfo.Languages.end())
    {
      //LogDebug("ChannelScan:   add component language '%s'", (char*)&(*langIt));
      info->Languages.push_back(*langIt);
      langIt++;
    }

    info->IsServiceInfoReceived = true;
    LogDebug("ChannelScan: received L-VCT information for service 0x%x", vctInfo.ServiceId);
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in OnLvctReceived()");
  }
}

void CChannelScan::OnPmtReceived(const CPidTable& pidTable)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    map<int, CChannelInfo*>::iterator it = m_mServices.find(pidTable.ProgramNumber);

    // Do we have a channel with this service ID?
    CChannelInfo* info = NULL;
    if (it != m_mServices.end())
    {
      info = it->second;
      if (info->IsPmtReceived)
      {
        LogDebug("ChannelScan: PMT information for service 0x%x received multiple times", pidTable.ProgramNumber);
      }
    }
    else
    {
      info = new CChannelInfo();
      m_mServices[pidTable.ProgramNumber] = info;
    }

    info->ServiceId = pidTable.ProgramNumber;
    // We trust PMT information over anything else for A/V present
    // status.
    info->VideoStreamCount = (unsigned short)pidTable.VideoPids.size();
    info->AudioStreamCount = (unsigned short)pidTable.AudioPids.size();
    // We trust service and encryption analyser information over the PMT information
    // for encryption and running status.
    if (!info->IsServiceInfoReceived && !info->IsPidReceived)
    {
      //info->IsEncrypted = pidTable.ConditionalAccessDescriptorCount > 0;
      info->IsRunning = true;
    }
    info->IsOtherTransportStream = false;
    info->IsPmtReceived = true;

    LogDebug("ChannelScan: PMT information for service 0x%x received from PID 0x%x", pidTable.ProgramNumber, pidTable.PmtPid);

    // Register each video and audio PID with our encryption analyser.
    // The analyser takes care of avoiding double-registry.
    // At the same time, add languages from the audio and subtitle PIDs.
    vector<VideoPid*>::const_iterator vPidIt = pidTable.VideoPids.begin();
    while (vPidIt != pidTable.VideoPids.end())
    {
      m_pEncryptionAnalyser->AddPid((*vPidIt)->Pid);
      m_mPids[(*vPidIt)->Pid] = pidTable.ProgramNumber;
      vPidIt++;
    }
    vector<AudioPid*>::const_iterator aPidIt = pidTable.AudioPids.begin();
    while (aPidIt != pidTable.AudioPids.end())
    {
      m_pEncryptionAnalyser->AddPid((*aPidIt)->Pid);
      m_mPids[(*aPidIt)->Pid] = pidTable.ProgramNumber;

      unsigned long lang = (*aPidIt)->Lang[0] + ((*aPidIt)->Lang[1] << 8) + ((*aPidIt)->Lang[2] << 16);
      (info->Languages).push_back(lang);
      //LogDebug("ChannelScan:   add audio language '%s'", (char*)&lang);
      if ((*aPidIt)->Lang[3] != NULL)
      {
        lang = (*aPidIt)->Lang[3] + ((*aPidIt)->Lang[4] << 8) + ((*aPidIt)->Lang[5] << 16);
        //LogDebug("ChannelScan:   add dual-mono language '%s'", (char*)&lang);
        (info->Languages).push_back(lang);
      }

      aPidIt++;
    }

    vector<SubtitlePid*>::const_iterator sPidIt = pidTable.SubtitlePids.begin();
    while (sPidIt != pidTable.SubtitlePids.end())
    {
      unsigned long lang = (*sPidIt)->Lang[0] + ((*sPidIt)->Lang[1] << 8) + ((*sPidIt)->Lang[2] << 16);
      //LogDebug("ChannelScan:   add subtitle language '%s'", (char*)&lang);
      (info->Languages).push_back(lang);
      sPidIt++;
    }
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in OnPmtReceived()");
  }
}

HRESULT CChannelScan::OnEncryptionStateChange(int pid, EncryptionState encryptionState)
{
  CEnterCriticalSection enter(m_section);
  try
  {
    // Find the service that the PID is associated with (note the limitation that
    // a PID may only be associated with one service).
    map<int, int>::iterator it = m_mPids.find(pid);
    if (it == m_mPids.end())
    {
      LogDebug("ChannelScan: encryption state received for PID 0x%x that we don't know about", pid);
      return S_OK;
    }
    map<int, CChannelInfo*>::iterator it2 = m_mServices.find(it->second);
    if (it2 == m_mServices.end())
    {
      LogDebug("ChannelScan: encryption state received for PID 0x%x associated with service 0x%x that we don't know about", pid, it->second);
      return S_OK;
    }

    CChannelInfo* info = it2->second;
    info->IsRunning = true;
    // Have we already seen one or more of the PIDs for this service?
    // If yes, set the encrypted flag to true if any of the elementary streams are encrypted.
    // If no, assume that the encryption state for this elementary stream reflects the encryption state for the service.
    if (!info->IsPidReceived || !info->IsEncrypted || encryptionState == Encrypted)
    {
      info->IsEncrypted = (encryptionState == Encrypted);
    }
    info->IsPidReceived = true;
  }
  catch (...)
  {
    LogDebug("ChannelScan: unhandled exception in OnEncryptionStateChange()");
  }
  return S_OK;
}

void CChannelScan::OnPatChanged(int programNumber, int oldPmtPid, int newPmtPid)
{
}
void CChannelScan::OnPatRemoved(int programNumber, int pmtPid)
{
}