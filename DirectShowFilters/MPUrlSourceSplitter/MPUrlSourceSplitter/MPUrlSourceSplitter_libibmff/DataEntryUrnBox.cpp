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

#include "DataEntryUrnBox.h"
#include "BoxCollection.h"

CDataEntryUrnBox::CDataEntryUrnBox(HRESULT *result)
  : CDataEntryBox(result)
{
  this->type = NULL;
  this->name = NULL;
  this->location = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(DATA_ENTRY_URN_BOX_TYPE);
    this->name = Duplicate(L"");
    this->location = Duplicate(L"");

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->name, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->location, *result, E_OUTOFMEMORY);
  }
}

CDataEntryUrnBox::~CDataEntryUrnBox(void)
{
  FREE_MEM(this->name);
  FREE_MEM(this->location);
}

/* get methods */

const wchar_t *CDataEntryUrnBox::GetName(void)
{
  return this->name;
}

const wchar_t *CDataEntryUrnBox::GetLocation(void)
{
  return this->location;
}

/* set methods */

bool CDataEntryUrnBox::SetName(const wchar_t *name)
{
  SET_STRING_RETURN(this->name, name);
}

bool CDataEntryUrnBox::SetLocation(const wchar_t *location)
{
  SET_STRING_RETURN(this->location, location);
}

/* other methods */

wchar_t *CDataEntryUrnBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare finally human readable representation
    result = FormatString(
      L"%s\n" \
      L"%sName: %s\n" \
      L"%sLocation: %s"
      ,
      
      previousResult,
      indent, this->GetName(),
      indent, this->GetLocation()

      );
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CDataEntryUrnBox::GetBoxSize(void)
{
  size_t nameLength = this->GetStringSize(this->name);
  size_t locationLength = this->GetStringSize(this->location);

  uint64_t result = ((nameLength != 0) && (locationLength != 0)) ? (nameLength + locationLength) : 0;

  if (result != 0)
  {
    uint64_t boxSize = __super::GetBoxSize();
    result = (boxSize != 0) ? (result + boxSize) : 0; 
  }

  return result;
}

bool CDataEntryUrnBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  FREE_MEM(this->name);
  FREE_MEM(this->location);

  if (__super::ParseInternal(buffer, length, false))
  {
    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, DATA_ENTRY_URN_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;

    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is data entry urn box, parse all values
      uint32_t position = this->HasExtendedHeader() ? FULL_BOX_HEADER_LENGTH_SIZE64 : FULL_BOX_HEADER_LENGTH;
      HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_NOT_VALID_STATE;

      // self contained flag is valid only for DataEntryUrlBox
      if (SUCCEEDED(continueParsing))
      {
        size_t positionAfter = position;
        continueParsing = this->GetString(buffer, length, position, &this->name, &positionAfter);

        CHECK_CONDITION_EXECUTE(SUCCEEDED(continueParsing), position = positionAfter);
      }

      if (SUCCEEDED(continueParsing))
      {
        size_t positionAfter = position;
        continueParsing = this->GetString(buffer, length, position, &this->location, &positionAfter);

        CHECK_CONDITION_EXECUTE(SUCCEEDED(continueParsing), position = positionAfter);
      }

      if (SUCCEEDED(continueParsing) && processAdditionalBoxes)
      {
        this->ProcessAdditionalBoxes(buffer, length, position);
      }

      this->flags &= ~BOX_FLAG_PARSED;
      this->flags |= SUCCEEDED(continueParsing) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;
    }
  }

  return this->IsSetFlags(BOX_FLAG_PARSED);
}

uint32_t CDataEntryUrnBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    uint32_t res = this->SetString(buffer + result, length - result, this->GetName());
    result = (res != 0) ? (result + res) : 0;

    if (result != 0)
    {
      res = this->SetString(buffer + result, length - result, this->GetLocation());
      result = (res != 0) ? (result + res) : 0;
    }

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}