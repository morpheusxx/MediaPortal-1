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

#include "stdafx.h"

#include "LinearBuffer.h"

CLinearBuffer::CLinearBuffer(HRESULT *result)
{
  this->buffer = NULL;
  this->DeleteBuffer();
}

CLinearBuffer::CLinearBuffer(HRESULT *result, unsigned int size)
{
  this->buffer = NULL;
  this->DeleteBuffer();

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    // create internal buffer
    CHECK_CONDITION_HRESULT(*result, this->InitializeBuffer(size), *result, E_OUTOFMEMORY);
  }
}

CLinearBuffer::~CLinearBuffer(void)
{
  this->DeleteBuffer();
}

unsigned char *CLinearBuffer::GetInternalBuffer(void)
{
  return this->buffer;
}

CLinearBuffer *CLinearBuffer::Clone(void)
{
  HRESULT result = S_OK;
  CLinearBuffer *clone = new CLinearBuffer(&result);
  CHECK_POINTER_HRESULT(result, clone, result, E_OUTOFMEMORY);

  CHECK_CONDITION_HRESULT(result, clone->InitializeBuffer(this->GetBufferSize()), result, E_OUTOFMEMORY);
  CHECK_CONDITION_HRESULT(result, clone->AddToBuffer(this->dataStart, this->GetBufferOccupiedSpace()) == this->GetBufferOccupiedSpace(), result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(clone));

  return clone;
}

bool CLinearBuffer::InitializeBuffer(size_t size)
{
  // remove current buffer (if any)
  this->DeleteBuffer();

  this->buffer = ALLOC_MEM(unsigned char, size);

  if (this->buffer != NULL)
  {
    this->bufferSize = size;
    this->ClearBuffer();
  }

  return (this->buffer != NULL);
}

bool CLinearBuffer::InitializeBuffer(size_t size, char value)
{
  bool result = this->InitializeBuffer(size);
  if (result)
  {
    memset(this->buffer, value, this->bufferSize);
  }
  return result;
}

void CLinearBuffer::ClearBuffer(void)
{
  this->dataStart = this->buffer;
  this->dataEnd = this->buffer;
}

void CLinearBuffer::DeleteBuffer(void)
{
  FREE_MEM(this->buffer);
  this->bufferSize = 0;
  this->ClearBuffer();
}

size_t CLinearBuffer::GetBufferSize()
{
  return this->bufferSize;
}

size_t CLinearBuffer::GetBufferFreeSpace()
{
  return ((size_t)this->bufferSize - (size_t)this->dataEnd + (size_t)this->buffer);
}

size_t CLinearBuffer::GetBufferOccupiedSpace(void)
{
  return ((size_t)this->dataEnd - (size_t)this->dataStart);
}

void CLinearBuffer::RemoveFromBuffer(size_t length)
{
  // the length to remove from buffer cannot be greater than occupied space
  length = min(length, this->GetBufferOccupiedSpace());

  if (length > 0)
  {
    if (length == this->GetBufferOccupiedSpace())
    {
      // removing all data from buffer
      this->ClearBuffer();
    }
    else
    {
      this->dataStart += length;
    }
  }
}

void CLinearBuffer::RemoveFromBufferAndMove(size_t length)
{
  // the length to remove from buffer cannot be greater than occupied space
  length = min(length, this->GetBufferOccupiedSpace());

  if (length > 0)
  {
    size_t occupiedSpace = this->GetBufferOccupiedSpace();
    if (length == occupiedSpace)
    {
      // removing all data from buffer
      this->ClearBuffer();
    }
    else
    {
      this->dataStart += length;

      size_t remainingDataLength = occupiedSpace - length;
      memmove(this->buffer, this->dataStart, remainingDataLength);
      this->dataStart = this->buffer;
      this->dataEnd = this->buffer + remainingDataLength;
    }
  }
}

void CLinearBuffer::RemoveFromBufferEnd(size_t length)
{
  // the length to remove from buffer cannot be greater than occupied space
  length = min(length, this->GetBufferOccupiedSpace());

  if (length > 0)
  {
    if (length == this->GetBufferOccupiedSpace())
    {
      // removing all data from buffer
      this->ClearBuffer();
    }
    else
    {
      this->dataEnd -= length;
    }
  }
}

size_t CLinearBuffer::AddToBuffer(const unsigned char *source, size_t length)
{
  size_t returnValue = 0;
  if ((length > 0) && (length <= this->GetBufferFreeSpace()))
  {
    memcpy(this->dataEnd, source, length);
    this->dataEnd += length;
    returnValue = length;
  }

  return returnValue;
}

size_t CLinearBuffer::AddToBufferWithResize(const unsigned char *source, size_t length)
{
  size_t returnValue = 0;

  if (this->GetBufferFreeSpace() < length)
  {
    if (this->ResizeBuffer(this->GetBufferSize() + length - this->GetBufferFreeSpace()))
    {
      returnValue = this->AddToBuffer(source, length);
    }
  }
  else
  {
    returnValue = this->AddToBuffer(source, length);
  }

  return returnValue;
}

size_t CLinearBuffer::AddToBufferWithResize(const unsigned char *source, size_t length, size_t minBufferSize)
{
  size_t returnValue = 0;

  if (this->GetBufferFreeSpace() < length)
  {
    size_t decidedLength = max(this->GetBufferSize() + length - this->GetBufferFreeSpace(), minBufferSize);
    if (this->ResizeBuffer(decidedLength))
    {
      returnValue = this->AddToBuffer(source, length);
    }
  }
  else
  {
    returnValue = this->AddToBuffer(source, length);
  }

  return returnValue;
}

size_t CLinearBuffer::AddToBufferWithResize(CLinearBuffer *buffer)
{
  return this->AddToBufferWithResize(buffer, 0);
}

size_t CLinearBuffer::AddToBufferWithResize(CLinearBuffer *buffer, size_t minBufferSize)
{
  return this->AddToBufferWithResize(buffer, 0, (buffer != NULL) ? buffer->GetBufferOccupiedSpace() : 0, minBufferSize);
}

size_t CLinearBuffer::AddToBufferWithResize(CLinearBuffer *buffer, size_t start, size_t length)
{
  return this->AddToBufferWithResize(buffer, start, length, 0);
}

size_t CLinearBuffer::AddToBufferWithResize(CLinearBuffer *buffer, size_t start, size_t length, size_t minBufferSize)
{
  size_t returnValue = 0;

  const unsigned char *dataStart = ((buffer != NULL) && (start < buffer->GetBufferOccupiedSpace())) ? (buffer->dataStart + start) : NULL;
  size_t dataLength = ((dataStart != NULL) && (length != 0) && ((start + length) <= buffer->GetBufferOccupiedSpace())) ? length : 0;

  if ((dataStart != NULL) && (dataLength != 0))
  {
    returnValue = this->AddToBufferWithResize(dataStart, dataLength, minBufferSize);
  }

  return returnValue;
}

size_t CLinearBuffer::CopyFromBuffer(unsigned char *destination, size_t length)
{
  return this->CopyFromBuffer(destination, length, 0);
}

size_t CLinearBuffer::CopyFromBuffer(unsigned char *destination, size_t length, size_t start)
{
  // length cannot be greater than buffer occupied space
  length = min(length, this->GetBufferOccupiedSpace() - start);
  if (length > 0)
  {
    memcpy(destination, this->dataStart + start, length);
  }

  return length;
}

size_t CLinearBuffer::GetFirstPosition(size_t start, char c)
{
  size_t result = UINT_MAX;

  for(size_t i = start; i < this->GetBufferOccupiedSpace(); i++)
  {
    if (this->buffer[i] == c)
    {
      result = i;
      break;
    }
  }

  return result;
}

bool CLinearBuffer::ResizeBuffer(size_t size)
{
  size_t occupiedSize = this->GetBufferOccupiedSpace();
  bool result = (size >= occupiedSize);

  if (result)
  {
    // requested buffer size is bigger than current occupied space
    // create new buffer
    unsigned char *tempBuffer = ALLOC_MEM(unsigned char, size);
    result = (tempBuffer != NULL);

    if (result)
    {
      // copy content from current buffer to new buffer
      this->CopyFromBuffer(tempBuffer, occupiedSize);
      // delete current buffer
      this->DeleteBuffer();
      // set new buffer pointer
      this->buffer = tempBuffer;
      // set new start, end of buffer
      this->dataStart = tempBuffer;
      this->dataEnd = tempBuffer + occupiedSize;
      this->bufferSize = size;
    }
  }

  return result;
}

bool CLinearBuffer::CompareBuffer(const unsigned char *buffer, size_t length)
{
  return this->CompareBuffer(buffer, length, 0);
}

bool CLinearBuffer::CompareBuffer(const unsigned char *buffer, size_t length, size_t start)
{
  size_t bufferOccupiedSpace = this->GetBufferOccupiedSpace();
  bool result = ((buffer != NULL) && (length > 0) && (bufferOccupiedSpace > start) && ((bufferOccupiedSpace - start) == length));

  if (result)
  {
    result = (memcmp(this->dataStart + start, buffer, length) == 0);
  }

  return result;
}

bool CLinearBuffer::CompareBuffer(CLinearBuffer *buffer)
{
  return this->CompareBuffer(buffer, 0);
}

bool CLinearBuffer::CompareBuffer(CLinearBuffer *buffer, size_t start)
{
  const unsigned char *dataStart = ((buffer != NULL) && (start < buffer->GetBufferOccupiedSpace())) ? (buffer->dataStart + start) : NULL;
  size_t dataLength = (dataStart != NULL) ? (buffer->GetBufferOccupiedSpace() - start) : 0;

  return this->CompareBuffer(dataStart, dataLength, 0);
}