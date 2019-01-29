rem @echo off
rem set TARGET=Debug
set TARGET=Release
set PKG=MediaPortal.TvEngine.Core

rem "lib" (direct references)
xcopy "TvService\bin\%TARGET%\Common.Utils.dll" "_MP2SlimTV\%PKG%\lib" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.Plugins.Base.dll" "_MP2SlimTV\%PKG%\lib" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.SetupControls.dll" "_MP2SlimTV\%PKG%\lib" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVControl.dll" "_MP2SlimTV\%PKG%\lib" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVDatabase.Entities.dll" "_MP2SlimTV\%PKG%\lib" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVDatabase.EntityModel.dll" "_MP2SlimTV\%PKG%\lib" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVDatabase.Presentation.dll" "_MP2SlimTV\%PKG%\lib" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.dll" "_MP2SlimTV\%PKG%\lib" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVLibrary.dll" "_MP2SlimTV\%PKG%\lib" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVLibrary.Interfaces.dll" "_MP2SlimTV\%PKG%\lib" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVLibrary.Services.dll" "_MP2SlimTV\%PKG%\lib" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TvLibrary.Utils.dll" "_MP2SlimTV\%PKG%\lib" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVService.Interfaces.dll" "_MP2SlimTV\%PKG%\lib" /R /Y  || exit /b 1

rem "references"
xcopy "TvService\bin\%TARGET%\Plugins\*.dll" "_MP2SlimTV\%PKG%\references\Plugins\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Plugins\CustomDevices\*.dll" "_MP2SlimTV\%PKG%\references\Plugins\CustomDevices\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Plugins\CustomDevices\Resources\*.*" "_MP2SlimTV\%PKG%\references\Plugins\CustomDevices\Resources\" /R /Y  || exit /b 1

xcopy "TvService\bin\%TARGET%\Castle.Core.dll" "_MP2SlimTV\%PKG%\references" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Castle.Windsor.dll" "_MP2SlimTV\%PKG%\references" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\DirectShowLib.dll" "_MP2SlimTV\%PKG%\references" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\EntityFramework.dll" "_MP2SlimTV\%PKG%\references" /R /Y  || exit /b 1
xcopy "..\..\..\..\DirectShowFilters\MPIPTVSource\MPIPTVSource\bin\%TARGET%\MPIPTVSource.ax" "_MP2SlimTV\%PKG%\references" /R /Y  || exit /b 1
xcopy "..\..\..\..\DirectShowFilters\MPIPTVSource\MPIPTV_FILE\bin\%TARGET%\MPIPTV_FILE.dll" "_MP2SlimTV\%PKG%\references" /R /Y  || exit /b 1
xcopy "..\..\..\..\DirectShowFilters\MPIPTVSource\MPIPTV_HTTP\bin\%TARGET%\MPIPTV_HTTP.dll" "_MP2SlimTV\%PKG%\references" /R /Y  || exit /b 1
xcopy "..\..\..\..\DirectShowFilters\MPIPTVSource\MPIPTV_RTP\bin\%TARGET%\MPIPTV_RTP.dll" "_MP2SlimTV\%PKG%\references" /R /Y  || exit /b 1
xcopy "..\..\..\..\DirectShowFilters\MPIPTVSource\MPIPTV_RTSP\bin\%TARGET%\MPIPTV_RTSP.dll" "_MP2SlimTV\%PKG%\references" /R /Y  || exit /b 1
xcopy "..\..\..\..\DirectShowFilters\MPIPTVSource\MPIPTV_UDP\bin\%TARGET%\MPIPTV_UDP.dll" "_MP2SlimTV\%PKG%\references" /R /Y  || exit /b 1
xcopy "..\..\..\..\DirectShowFilters\StreamingServer\bin\%TARGET%\StreamingServer.dll" "_MP2SlimTV\%PKG%\references" /R /Y  || exit /b 1
xcopy "..\..\..\..\DirectShowFilters\TsMuxer\bin\%TARGET%\TsMuxer.ax" "_MP2SlimTV\%PKG%\references" /R /Y  || exit /b 1
xcopy "..\..\..\..\DirectShowFilters\TsWriter\bin\%TARGET%\TsWriter.ax" "_MP2SlimTV\%PKG%\references" /R /Y  || exit /b 1

cd TVServer.Base
"c:\Program Files\7-Zip\7z.exe" a -r ..\_MP2SlimTV\%PKG%\references\ProgramData\ProgramData.zip .
cd ..

