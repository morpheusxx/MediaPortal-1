rem @echo off
rem set TARGET=Debug
set TARGET=Release

xcopy "TvService\bin\%TARGET%\Castle.Core.dll" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Castle.Windsor.dll" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Common.Utils.dll" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\DirectShowLib.dll" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\EntityFramework.dll" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.Plugins.Base.dll" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.SetupControls.dll" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVControl.dll" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVDatabase.Entities.dll" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVDatabase.EntityModel.dll" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVDatabase.Presentation.dll" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.dll" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVLibrary.dll" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVLibrary.IntegrationProvider.Interfaces.dll" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVLibrary.Interfaces.dll" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVLibrary.Services.dll" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TvLibrary.Utils.dll" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVService.Interfaces.dll" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\TsWriter.ax" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Plugins\*.dll" "_MP2SlimTV\References\Plugins\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Plugins\CustomDevices\*.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Plugins\CustomDevices\Resources\*.*" "_MP2SlimTV\References\Plugins\CustomDevices\Resources\" /R /Y  || exit /b 1

REM #######################################
REM 	SetupTv
REM #######################################

xcopy "SetupTv\bin\%TARGET%\*.dll" "_MP2SlimTV\References\SetupTv\" /R /Y  || exit /b 1
xcopy "SetupTv\bin\%TARGET%\log4net.config" "_MP2SlimTV\References\SetupTv\" /R /Y  || exit /b 1
xcopy "SetupTv\bin\%TARGET%\SetupTV.exe" "_MP2SlimTV\References\SetupTv\" /R /Y  || exit /b 1
xcopy "SetupTv\bin\%TARGET%\SetupTV.exe.config" "_MP2SlimTV\References\SetupTv\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Castle.Core.dll" "_MP2SlimTV\References\SetupTv\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Castle.Windsor.dll" "_MP2SlimTV\References\SetupTv\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Castle.Facilities.Logging.dll" "_MP2SlimTV\References\SetupTv\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Castle.Services.Logging.Log4netIntegration.dll" "_MP2SlimTV\References\SetupTv\" /R /Y  || exit /b 1
rem TODO: where does this assembly come from? If there would be a dependency, it should be included in folder already?
rem xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.Plugins.PowerScheduler.Interfaces.dll" "_MP2SlimTV\References\SetupTv\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TvLibrary.Utils.dll" "_MP2SlimTV\References\SetupTv\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Plugins\*.*" "_MP2SlimTV\References\SetupTv\Plugins\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Plugins\CustomDevices\*.dll" "_MP2SlimTV\References\SetupTv\Plugins\CustomDevices\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Plugins\CustomDevices\Resources\*.*" "_MP2SlimTV\References\SetupTv\Plugins\CustomDevices\Resources\" /R /Y  || exit /b 1

REM #######################################
REM 	DirectShow filters
REM #######################################

rem xcopy "..\..\..\..\DirectShowFilters\MPWriter\bin\%TARGET%\MPFileWriter.ax" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "..\..\..\..\DirectShowFilters\TsMuxer\bin\%TARGET%\TsMuxer.ax" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "..\..\..\..\DirectShowFilters\StreamingServer\bin\%TARGET%\StreamingServer.dll" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "..\..\..\..\DirectShowFilters\MPIPTVSource\MPIPTVSource\bin\%TARGET%\MPIPTVSource.ax" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "..\..\..\..\DirectShowFilters\MPIPTVSource\MPIPTV_FILE\bin\%TARGET%\MPIPTV_FILE.dll" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "..\..\..\..\DirectShowFilters\MPIPTVSource\MPIPTV_HTTP\bin\%TARGET%\MPIPTV_HTTP.dll" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "..\..\..\..\DirectShowFilters\MPIPTVSource\MPIPTV_RTP\bin\%TARGET%\MPIPTV_RTP.dll" "_MP2SlimTV\References\" /R /Y  || exit /b 1
xcopy "..\..\..\..\DirectShowFilters\MPIPTVSource\MPIPTV_RTSP\bin\%TARGET%\MPIPTV_RTSP.dll" "_MP2SlimTV\References\" /R /Y  || exit /b 1

xcopy "..\..\..\..\DirectShowFilters\TsReader\bin\%TARGET%\TsReader.ax" "_MP2SlimTV\References\SetupTv\" /R /Y  || exit /b 1

REM #######################################
REM 	ProgramData base files
REM #######################################

cd TVServer.Base
"c:\Program Files\7-Zip\7z.exe" a -r ..\_MP2SlimTV\References\ProgramData\ProgramData.zip .
cd ..