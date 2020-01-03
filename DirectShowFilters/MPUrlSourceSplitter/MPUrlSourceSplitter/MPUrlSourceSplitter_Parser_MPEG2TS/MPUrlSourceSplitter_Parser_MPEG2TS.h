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

#ifndef __MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_DEFINED
#define __MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_DEFINED

#include "ParserPlugin.h"
#include "CacheFile.h"
#include "Mpeg2tsStreamFragmentCollection.h"
#include "DiscontinuityParser.h"
#include "ProgramAssociationParserContext.h"
#include "TransportStreamProgramMapParserContextCollection.h"
#include "SectionCollection.h"
#include "ConditionalAccessParserContext.h"
#include "SectionMultiplexerCollection.h"

#define MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_NONE               PARSER_PLUGIN_FLAG_NONE

#define MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_RECEIVE_DATA                     (1 << (PARSER_PLUGIN_FLAG_LAST + 0))

#define MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_DETECT_DISCONTINUITY             (1 << (PARSER_PLUGIN_FLAG_LAST + 1))
#define MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_ALIGN_TO_MPEG2TS_PACKET          (1 << (PARSER_PLUGIN_FLAG_LAST + 2))

#define MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_TRANSPORT_STREAM_ID       (1 << (PARSER_PLUGIN_FLAG_LAST + 3))
#define MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_NUMBER            (1 << (PARSER_PLUGIN_FLAG_LAST + 4))
#define MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_CHANGE_PROGRAM_MAP_PID           (1 << (PARSER_PLUGIN_FLAG_LAST + 5))

#define MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_SET_NOT_SCRAMBLED                (1 << (PARSER_PLUGIN_FLAG_LAST + 6))

#define MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_FILTER_PROGRAM_ELEMENTS          (1 << (PARSER_PLUGIN_FLAG_LAST + 7))

#define MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_STREAM_ANALYSIS                  (1 << (PARSER_PLUGIN_FLAG_LAST + 8))

#define MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_FOUND_PAT                        (1 << (PARSER_PLUGIN_FLAG_LAST + 9))
#define MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_FOUND_PMT                        (1 << (PARSER_PLUGIN_FLAG_LAST + 10))

#define MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_NO_MORE_PROTOCOL_DATA                 (1 << (PARSER_PLUGIN_FLAG_LAST + 11))

#define MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_LAST               (PARSER_PLUGIN_FLAG_LAST + 12)

#define PARSER_NAME                                                   L"PARSER_MPEG2TS"

#define PARSER_STORE_FILE_NAME_PART                                   L"mpurlsourcesplitter_parser_mpeg2ts"

class CMPUrlSourceSplitter_Parser_Mpeg2TS : public CParserPlugin
{
public:
  // constructor
  // create instance of CMPUrlSourceSplitter_Parser_Mpeg2TS class
  CMPUrlSourceSplitter_Parser_Mpeg2TS(HRESULT *result, CLogger *logger, CParameterCollection *configuration);
  // destructor
  virtual ~CMPUrlSourceSplitter_Parser_Mpeg2TS(void);

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

  // sets current connection url and parameters
  // @param parameters : the collection of url and connection parameters
  // @return : S_OK if successful
  virtual HRESULT SetConnectionParameters(const CParameterCollection *parameters);

  // tests if stream length was set
  // @return : true if stream length was set, false otherwise
  virtual bool IsSetStreamLength(void);

  // tests if stream length is estimated
  // @return : true if stream length is estimated, false otherwise
  virtual bool IsStreamLengthEstimated(void);

  // tests if whole stream is downloaded (no gaps)
  // @return : true if whole stream is downloaded
  virtual bool IsWholeStreamDownloaded(void);

  // tests if end of stream is reached (but it can be with gaps)
  // @return : true if end of stream reached, false otherwise
  virtual bool IsEndOfStreamReached(void);

  // tests if connection was lost and can't be opened again
  // @return : true if connection was lost and can't be opened again, false otherwise
  virtual bool IsConnectionLostCannotReopen(void);

  // tests if stream is IPTV compatible
  // @return : true if stream is IPTV compatible, false otherwise
  virtual bool IsStreamIptvCompatible(void);

  // gets IPTV section count
  // @return : IPTV section count
  virtual unsigned int GetIptvSectionCount(void);

  // gets IPTV section with specified index
  // @param index : the index of IPTV section to get
  // @param section : the reference to string which holds section data in BASE64 encoding
  // @return : S_OK if successful
  virtual HRESULT GetIptvSection(unsigned int index, wchar_t **section);

  // CPlugin

  // return reference to null-terminated string which represents plugin name
  // errors should be logged to log file and returned NULL
  // @return : reference to null-terminated string
  virtual const wchar_t *GetName(void);

  // ISeeking interface

  // request protocol implementation to receive data from specified time (in ms) for specified stream
  // this method is called with same time for each stream in protocols with multiple streams
  // @param streamId : the stream ID to receive data from specified time
  // @param time : the requested time (zero is start of stream)
  // @return : time (in ms) where seek finished or lower than zero if error
  virtual int64_t SeekToTime(unsigned int streamId, int64_t time);

  // set pause, seek or stop mode
  // in such mode are reading operations disabled
  // @param pauseSeekStopMode : one of PAUSE_SEEK_STOP_MODE values
  virtual void SetPauseSeekStopMode(unsigned int pauseSeekStopMode);

  // IDemuxerOwner interface

  // process stream package request
  // @param streamPackage : the stream package request to process
  // @return : S_OK if successful, error code only in case when error is not related to processing request
  virtual HRESULT ProcessStreamPackage(CStreamPackage *streamPackage);

  // retrieves the progress of the stream reading operation
  // @param streamProgress : reference to instance of class that receives the stream progress
  // @return : S_OK if successful, VFW_S_ESTIMATED if returned values are estimates, E_INVALIDARG if stream ID is unknown, E_UNEXPECTED if unexpected error
  virtual HRESULT QueryStreamProgress(CStreamProgress *streamProgress);

  // ISimpleProtocol interface

  // starts receiving data from specified url and configuration parameters
  // @param parameters : the url and parameters used for connection
  // @return : S_OK if url is loaded, false otherwise
  virtual HRESULT StartReceivingData(CParameterCollection *parameters);

  // request protocol implementation to cancel the stream reading operation
  // @return : S_OK if successful
  virtual HRESULT StopReceivingData(void);
  
  // clears current session
  virtual void ClearSession(void);

  // IProtocol interface

protected:
  // holds last received length of data when requesting parser result
  size_t lastReceivedLength;
  // mutex for locking access to file, buffer, ...
  HANDLE mutex;

  // holds stream fragments
  CMpeg2tsStreamFragmentCollection *streamFragments;
  // holds last store time to cache file
  unsigned int lastStoreTime;
  // holds cache file
  CCacheFile *cacheFile;
  // holds last processed size from last store time
  size_t lastProcessedSize;
  size_t currentProcessedSize;
  // holds which fragment is currently downloading (UINT_MAX means none)
  unsigned int streamFragmentDownloading;
  // holds which fragment have to be downloaded
  // (UINT_MAX means next fragment, always reset after started download of fragment)
  unsigned int streamFragmentToDownload;

  // the length of stream
  int64_t streamLength;

  // holds stream package for processing (only reference, not deep clone)
  CStreamPackage *streamPackage;

  // holds pause, seek or stop mode
  volatile unsigned int pauseSeekStopMode;

  // holds position offset added to stream length 
  int64_t positionOffset;

  // holds discontinuity parser
  CDiscontinuityParser *discontinuityParser;

  // holds program association (PAT) parser
  CProgramAssociationParserContext *programAssociationParserContext;
  // holds collection of transport stream program map (PMT) parser
  CTransportStreamProgramMapParserContextCollection *transportStreamProgramMapParserContextCollection;
  // holds conditional access (CA) parser
  CConditionalAccessParserContext *conditionalAccessParserContext;
  // holds mutliplexers for reconstructing stream
  CSectionMultiplexerCollection *multiplexers;

  // holds new stream identification (transport stream ID, program number and program map PID)
  unsigned int transportStreamId;
  unsigned int programNumber;
  unsigned int programMapPID;

  // holds sections
  CSectionCollection *sections;

  /* received data worker */

  HANDLE receiveDataWorkerThread;
  volatile bool receiveDataWorkerShouldExit;

  /* methods */

  // get module name for Initialize() method
  // @return : module name
  virtual const wchar_t *GetModuleName(void);

  // gets store file name part
  // @return : store file name part or NULL if error
  virtual const wchar_t *GetStoreFileNamePart(void);

  // gets byte position in buffer
  // it is always reset on seek
  // @return : byte position in buffer
  int64_t GetBytePosition(void);

  /* receive data worker */

  // creates receive data worker
  // @return : S_OK if successful
  HRESULT CreateReceiveDataWorker(void);

  // destroys receive data worker
  // @return : S_OK if successful
  HRESULT DestroyReceiveDataWorker(void);

  static unsigned int WINAPI ReceiveDataWorker(LPVOID lpParam);
};

#endif
