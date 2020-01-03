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

#pragma once

#ifndef __MPURLSOURCESPLITTER_PARSER_M3U8_DEFINED
#define __MPURLSOURCESPLITTER_PARSER_M3U8_DEFINED

#include "ParserPlugin.h"

#define PARSER_NAME                                                   L"PARSER_M3U8"

#define PARSER_STORE_FILE_NAME_PART                                   L"mpurlsourcesplitter_parser_m3u8"

class CMPUrlSourceSplitter_Parser_M3U8 : public CParserPlugin
{
public:
  // constructor
  // create instance of CMPUrlSourceSplitter_Parser_M3U8 class
  CMPUrlSourceSplitter_Parser_M3U8(HRESULT *result, CLogger *logger, CParameterCollection *configuration);
  // destructor
  virtual ~CMPUrlSourceSplitter_Parser_M3U8(void);

  // CParserPlugin

  // gets parser result about current stream
  // @return : one of ParserResult values
  virtual HRESULT GetParserResult(void);

  // gets parser score if parser result is Known
  // @return : parser score (parser with highest score is set as active parser)
  virtual unsigned int GetParserScore(void);

  // gets parser action after parser recognizes stream
  // @return : one of Action values
  virtual Action GetAction(void);

  // CPlugin

  // return reference to null-terminated string which represents plugin name
  // errors should be logged to log file and returned NULL
  // @return : reference to null-terminated string
  virtual const wchar_t *GetName(void);

  // ISeeking interface

  // IDemuxerOwner interface

  // ISimpleProtocol interface

  // clears current session
  virtual void ClearSession(void);

  // IProtocol interface

protected:
  // holds last received length
  size_t lastReceivedLength;

  /* methods */

  // get module name for Initialize() method
  // @return : module name
  virtual const wchar_t *GetModuleName(void);

  // gets store file name part
  // @return : store file name part or NULL if error
  virtual const wchar_t *GetStoreFileNamePart(void);
};

#endif
