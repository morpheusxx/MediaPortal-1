rem @echo off
rem set TARGET=Debug
set TARGET=Release
set PKG=MediaPortal.TvEngine.Core
set                   MB="%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
if not exist %MB% set MB="%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBUILD.exe"
rem set BUILD_OPTS=" /t:Rebuild"
set BUILD_OPTS=

%MB% Nuget.targets /target:DownloadNuGet 

%MB% ..\..\..\..\DirectShowFilters\Filters.sln /P:Configuration=%TARGET% /P:Platform=Win32 %BUILD_OPTS% || exit /b 1
%MB% ..\..\..\..\DirectShowFilters\Filters.sln /P:Configuration=%TARGET% /P:Platform=x64 %BUILD_OPTS% || exit /b 1
nuget update -self
nuget restore Mediaportal.TV.Server.sln || exit /b 2
%MB% Mediaportal.TV.Server.sln /p:Configuration=%TARGET% %BUILD_OPTS% || exit /b 2

rem "lib" (direct references)
xcopy "TvService\bin\%TARGET%\Common.Utils.dll" "_MP2SlimTV\%PKG%\lib\" /R /Y  || exit /b 1
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.Plugins.Base.dll" "_MP2SlimTV\%PKG%\lib\" /R /Y  || exit /b 2
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.SetupControls.dll" "_MP2SlimTV\%PKG%\lib\" /R /Y  || exit /b 3
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVControl.dll" "_MP2SlimTV\%PKG%\lib\" /R /Y  || exit /b 4
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVDatabase.Entities.dll" "_MP2SlimTV\%PKG%\lib\" /R /Y  || exit /b 5
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVDatabase.EntityModel.dll" "_MP2SlimTV\%PKG%\lib\" /R /Y  || exit /b 6
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVDatabase.Presentation.dll" "_MP2SlimTV\%PKG%\lib\" /R /Y  || exit /b 7
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.dll" "_MP2SlimTV\%PKG%\lib\" /R /Y  || exit /b 8
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVLibrary.dll" "_MP2SlimTV\%PKG%\lib\" /R /Y  || exit /b 9
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVLibrary.Interfaces.dll" "_MP2SlimTV\%PKG%\lib\" /R /Y  || exit /b 10
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVLibrary.Services.dll" "_MP2SlimTV\%PKG%\lib\" /R /Y  || exit /b 11
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TvLibrary.Utils.dll" "_MP2SlimTV\%PKG%\lib\" /R /Y  || exit /b 12
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVService.Interfaces.dll" "_MP2SlimTV\%PKG%\lib\" /R /Y  || exit /b 13
xcopy /E "TvService\bin\%TARGET%\runtimes\*.dll" "_MP2SlimTV\%PKG%\references\runtimes\" /R /Y  || exit /b 13

rem "references\"
xcopy "TvService\bin\%TARGET%\AutoMapper.dll" "_MP2SlimTV\%PKG%\references\" /R /Y  || exit /b 14
xcopy "TvService\bin\%TARGET%\Microsoft.*.dll" "_MP2SlimTV\%PKG%\references\" /R /Y  || exit /b 14
xcopy "TvService\bin\%TARGET%\Castle.*.dll" "_MP2SlimTV\%PKG%\references\" /R /Y  || exit /b 21
xcopy "TvService\bin\%TARGET%\DirectShowLib.dll" "_MP2SlimTV\%PKG%\references\" /R /Y  || exit /b 14
xcopy "TvService\bin\%TARGET%\SQLite*.dll" "_MP2SlimTV\%PKG%\references\" /R /Y  || exit /b 15
xcopy "TvService\bin\%TARGET%\System.*.dll" "_MP2SlimTV\%PKG%\references\" /R /Y  || exit /b 16
xcopy "TvService\bin\%TARGET%\Plugins\*.dll" "_MP2SlimTV\%PKG%\references\Plugins\" /R /Y  || exit /b 18
xcopy "TvService\bin\%TARGET%\Plugins\*.exe" "_MP2SlimTV\%PKG%\references\Plugins\" /R /Y  || exit /b 19
xcopy /E "TvService\bin\%TARGET%\Plugins\CustomDevices\*.dll" "_MP2SlimTV\%PKG%\references\Plugins\CustomDevices\" /R /Y  || exit /b 20

rem "references\SetupTv"
xcopy "SetupTv\bin\%TARGET%\SetupTv.exe" "_MP2SlimTV\%PKG%\references\SetupTv\" /R /Y  || exit /b 14
xcopy "SetupTv\bin\%TARGET%\Mediaportal.TV.Server.Plugins.Base.dll" "_MP2SlimTV\%PKG%\references\SetupTv\" /R /Y  || exit /b 14
xcopy "SetupTv\bin\%TARGET%\Mediaportal.TV.Server.RuleBasedScheduler.dll" "_MP2SlimTV\%PKG%\references\SetupTv\" /R /Y  || exit /b 14
xcopy "SetupTv\bin\%TARGET%\Mediaportal.TV.Server.SetupControls.dll" "_MP2SlimTV\%PKG%\references\SetupTv\" /R /Y  || exit /b 14
xcopy "SetupTv\bin\%TARGET%\Mediaportal.TV.Server.TVControl.dll" "_MP2SlimTV\%PKG%\references\SetupTv\" /R /Y  || exit /b 14
xcopy "SetupTv\bin\%TARGET%\Mediaportal.TV.Server.TVDatabase.Entities.dll" "_MP2SlimTV\%PKG%\references\SetupTv\" /R /Y  || exit /b 14
xcopy "SetupTv\bin\%TARGET%\Mediaportal.TV.Server.TVDatabase.EntityModel.dll" "_MP2SlimTV\%PKG%\references\SetupTv\" /R /Y  || exit /b 14
xcopy "SetupTv\bin\%TARGET%\Mediaportal.TV.Server.TVDatabase.Presentation.dll" "_MP2SlimTV\%PKG%\references\SetupTv\" /R /Y  || exit /b 14
xcopy "SetupTv\bin\%TARGET%\Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.dll" "_MP2SlimTV\%PKG%\references\SetupTv\" /R /Y  || exit /b 14
xcopy "SetupTv\bin\%TARGET%\Mediaportal.TV.Server.TVLibrary.dll" "_MP2SlimTV\%PKG%\references\SetupTv\" /R /Y  || exit /b 14
xcopy "SetupTv\bin\%TARGET%\Mediaportal.TV.Server.TVLibrary.IntegrationProvider.Interfaces.dll" "_MP2SlimTV\%PKG%\references\SetupTv\" /R /Y  || exit /b 14
xcopy "SetupTv\bin\%TARGET%\Mediaportal.TV.Server.TVLibrary.Interfaces.dll" "_MP2SlimTV\%PKG%\references\SetupTv\" /R /Y  || exit /b 14
xcopy "SetupTv\bin\%TARGET%\Mediaportal.TV.Server.TvLibrary.Utils.dll" "_MP2SlimTV\%PKG%\references\SetupTv\" /R /Y  || exit /b 14
xcopy "SetupTv\bin\%TARGET%\Mediaportal.TV.Server.TVService.Interfaces.dll" "_MP2SlimTV\%PKG%\references\SetupTv\" /R /Y  || exit /b 14
xcopy "SetupTv\bin\%TARGET%\Microsoft.*.dll" "_MP2SlimTV\%PKG%\references\SetupTv\" /R /Y  || exit /b 14
xcopy "SetupTv\bin\%TARGET%\Castle.Core.dll" "_MP2SlimTV\%PKG%\references\SetupTv\" /R /Y  || exit /b 21
xcopy "SetupTv\bin\%TARGET%\Castle.Windsor.dll" "_MP2SlimTV\%PKG%\references\SetupTv\" /R /Y  || exit /b 21
xcopy "SetupTv\bin\%TARGET%\Common.Utils.dll" "_MP2SlimTV\%PKG%\references\SetupTv\" /R /Y  || exit /b 14
xcopy "SetupTv\bin\%TARGET%\DirectShowLib.dll" "_MP2SlimTV\%PKG%\references\SetupTv\" /R /Y  || exit /b 14
xcopy "SetupTv\bin\%TARGET%\SQLite*.dll" "_MP2SlimTV\%PKG%\references\SetupTv\" /R /Y  || exit /b 15
xcopy "SetupTv\bin\%TARGET%\System.*.dll" "_MP2SlimTV\%PKG%\references\SetupTv\" /R /Y  || exit /b 16
xcopy "SetupTv\bin\%TARGET%\UPnP.dll" "_MP2SlimTV\%PKG%\references\SetupTv\" /R /Y  || exit /b 16
xcopy "SetupTv\bin\%TARGET%\log4net.dll" "_MP2SlimTV\%PKG%\references\SetupTv\" /R /Y  || exit /b 16
xcopy "SetupTv\bin\%TARGET%\*.config" "_MP2SlimTV\%PKG%\references\SetupTv\" /R /Y  || exit /b 16
xcopy /E "SetupTv\bin\%TARGET%\x64\*.*" "_MP2SlimTV\%PKG%\references\SetupTv\x64\" /R /Y  || exit /b 16
xcopy /E "SetupTv\bin\%TARGET%\x86\*.*" "_MP2SlimTV\%PKG%\references\SetupTv\x86\" /R /Y  || exit /b 16

xcopy "..\..\..\..\DirectShowFilters\Win32\%TARGET%\MPIPTVSource.ax" "_MP2SlimTV\%PKG%\references\x86\" /R /Y  || exit /b 22
xcopy "..\..\..\..\DirectShowFilters\Win32\%TARGET%\MPIPTV_FILE.dll" "_MP2SlimTV\%PKG%\references\x86\" /R /Y  || exit /b 23
xcopy "..\..\..\..\DirectShowFilters\Win32\%TARGET%\MPIPTV_HTTP.dll" "_MP2SlimTV\%PKG%\references\x86\" /R /Y  || exit /b 24
xcopy "..\..\..\..\DirectShowFilters\Win32\%TARGET%\MPIPTV_RTP.dll" "_MP2SlimTV\%PKG%\references\x86\" /R /Y  || exit /b 25
xcopy "..\..\..\..\DirectShowFilters\Win32\%TARGET%\MPIPTV_RTSP.dll" "_MP2SlimTV\%PKG%\references\x86\" /R /Y  || exit /b 26
xcopy "..\..\..\..\DirectShowFilters\Win32\%TARGET%\MPIPTV_UDP.dll" "_MP2SlimTV\%PKG%\references\x86\" /R /Y  || exit /b 27
xcopy "..\..\..\..\DirectShowFilters\Win32\%TARGET%\TsMuxer.ax" "_MP2SlimTV\%PKG%\references\x86\" /R /Y  || exit /b 29
xcopy "..\..\..\..\DirectShowFilters\Win32\%TARGET%\TsWriter.ax" "_MP2SlimTV\%PKG%\references\x86\" /R /Y  || exit /b 30

xcopy "..\..\..\..\DirectShowFilters\x64\%TARGET%\MPIPTVSource.ax" "_MP2SlimTV\%PKG%\references\x64\" /R /Y  || exit /b 32
xcopy "..\..\..\..\DirectShowFilters\x64\%TARGET%\MPIPTV_FILE.dll" "_MP2SlimTV\%PKG%\references\x64\" /R /Y  || exit /b 33
xcopy "..\..\..\..\DirectShowFilters\x64\%TARGET%\MPIPTV_HTTP.dll" "_MP2SlimTV\%PKG%\references\x64\" /R /Y  || exit /b 34
xcopy "..\..\..\..\DirectShowFilters\x64\%TARGET%\MPIPTV_RTP.dll" "_MP2SlimTV\%PKG%\references\x64\" /R /Y  || exit /b 35
xcopy "..\..\..\..\DirectShowFilters\x64\%TARGET%\MPIPTV_RTSP.dll" "_MP2SlimTV\%PKG%\references\x64\" /R /Y  || exit /b 36
xcopy "..\..\..\..\DirectShowFilters\x64\%TARGET%\MPIPTV_UDP.dll" "_MP2SlimTV\%PKG%\references\x64\" /R /Y  || exit /b 37
xcopy "..\..\..\..\DirectShowFilters\x64\%TARGET%\TsMuxer.ax" "_MP2SlimTV\%PKG%\references\x64\" /R /Y  || exit /b 39
xcopy "..\..\..\..\DirectShowFilters\x64\%TARGET%\TsWriter.ax" "_MP2SlimTV\%PKG%\references\x64\" /R /Y  || exit /b 40

xcopy "..\..\..\..\DirectShowFilters\Win32\%TARGET%\StreamingServer.dll" "_MP2SlimTV\%PKG%\references\x86\" /R /Y  || exit /b 28
xcopy "..\..\..\..\DirectShowFilters\x64\%TARGET%\StreamingServer.dll" "_MP2SlimTV\%PKG%\references\x64\" /R /Y  || exit /b 38

cd TVServer.Base
"c:\Program Files\7-Zip\7z.exe" a -r ..\_MP2SlimTV\%PKG%\references\ProgramData\ProgramData.zip .
cd ..

nuget pack _MP2SlimTV\MediaPortal.TvEngine.Core\MediaPortal.TvEngine.Core.nuspec -Version %1