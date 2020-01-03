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

#include "CleanApertureBox.h"
#include "BoxCollection.h"

CCleanApertureBox::CCleanApertureBox(HRESULT *result)
  : CBox(result)
{
  this->type = NULL;
  this->cleanApertureWidthN = 0;
  this->cleanApertureWidthD = 0;
  this->cleanApertureHeightN = 0;
  this->cleanApertureHeightD = 0;
  this->horizOffN = 0;
  this->horizOffD = 0;
  this->vertOffN = 0;
  this->vertOffD = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(CLEAN_APERTURE_BOX_TYPE);

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
  }
}

CCleanApertureBox::~CCleanApertureBox(void)
{
}

/* get methods */

uint32_t CCleanApertureBox::GetCleanApertureWidthN(void)
{
  return this->cleanApertureWidthN;
}

uint32_t CCleanApertureBox::GetCleanApertureWidthD(void)
{
  return this->cleanApertureWidthD;
}

uint32_t CCleanApertureBox::GetCleanApertureHeightN(void)
{
  return this->cleanApertureHeightN;
}

uint32_t CCleanApertureBox::GetCleanApertureHeightD(void)
{
  return this->cleanApertureHeightD;
}

uint32_t CCleanApertureBox::GetHorizontalOffsetN(void)
{
  return this->horizOffN;
}

uint32_t CCleanApertureBox::GetHorizontalOffsetD(void)
{
  return this->horizOffD;
}

uint32_t CCleanApertureBox::GetVerticalOffsetN(void)
{
  return this->vertOffN;
}

 uint32_t CCleanApertureBox::GetVerticalOffsetD(void)
 {
   return this->vertOffD;
 }

/* set methods */

/* other methods */

wchar_t *CCleanApertureBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare finally human readable representation
    result = FormatString(
      L"%s\n" \
      L"%sClean aperture width N: %u\n" \
      L"%sClean aperture width D: %u\n" \
      L"%sClean aperture height N: %u\n" \
      L"%sClean aperture height D: %u\n" \
      L"%sHorizontal offset N: %u\n" \
      L"%sHorizontal offset D: %u\n" \
      L"%sVetical offset N: %u\n" \
      L"%sVertical offset D: %u"
      ,
      
      previousResult,
      indent, this->GetCleanApertureWidthN(),
      indent, this->GetCleanApertureWidthD(),
      indent, this->GetCleanApertureHeightN(),
      indent, this->GetCleanApertureHeightD(),
      indent, this->GetHorizontalOffsetN(),
      indent, this->GetHorizontalOffsetD(),
      indent, this->GetVerticalOffsetN(),
      indent, this->GetVerticalOffsetD()

      );
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CCleanApertureBox::GetBoxSize(void)
{
  return __super::GetBoxSize();
}

bool CCleanApertureBox::ParseInternal(const unsigned char *buffer, size_t length, bool processAdditionalBoxes)
{
  if (__super::ParseInternal(buffer, length, false))
  {
    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, CLEAN_APERTURE_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;

    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is clean aperture box, parse all values
      uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;
      HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_NOT_VALID_STATE;

      if (SUCCEEDED(continueParsing))
      {
        RBE32INC(buffer, position, this->cleanApertureWidthN);
        RBE32INC(buffer, position, this->cleanApertureWidthD);
        RBE32INC(buffer, position, this->cleanApertureHeightN);
        RBE32INC(buffer, position, this->cleanApertureHeightD);
        RBE32INC(buffer, position, this->horizOffN);
        RBE32INC(buffer, position, this->horizOffD);
        RBE32INC(buffer, position, this->vertOffN);
        RBE32INC(buffer, position, this->vertOffD);
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

size_t CCleanApertureBox::GetBoxInternal(uint8_t *buffer, size_t length, bool processAdditionalBoxes)
{
  size_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}