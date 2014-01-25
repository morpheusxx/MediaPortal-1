rem set TARGET=Debug
set TARGET=Release

xcopy "TvService\bin\%TARGET%\Castle.Core.dll" "_MP2SlimTV\References\" /R /Y
xcopy "TvService\bin\%TARGET%\Castle.Windsor.dll" "_MP2SlimTV\References\" /R /Y
xcopy "TvService\bin\%TARGET%\Common.Utils.dll" "_MP2SlimTV\References\" /R /Y
xcopy "TvService\bin\%TARGET%\DirectShowLib.dll" "_MP2SlimTV\References\" /R /Y
xcopy "TvService\bin\%TARGET%\dxerr9.dll" "_MP2SlimTV\References\" /R /Y
xcopy "TvService\bin\%TARGET%\EntityFramework.dll" "_MP2SlimTV\References\" /R /Y
xcopy "TvService\bin\%TARGET%\Interop.SHDocVw.dll" "_MP2SlimTV\References\" /R /Y
xcopy "TvService\bin\%TARGET%\Ionic.Zip.dll" "_MP2SlimTV\References\" /R /Y
xcopy "TvService\bin\%TARGET%\log4net.dll" "_MP2SlimTV\References\" /R /Y
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.Plugins.Base.dll" "_MP2SlimTV\References\" /R /Y
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.SetupControls.dll" "_MP2SlimTV\References\" /R /Y
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVControl.dll" "_MP2SlimTV\References\" /R /Y
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVDatabase.Entities.dll" "_MP2SlimTV\References\" /R /Y
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVDatabase.EntityModel.dll" "_MP2SlimTV\References\" /R /Y
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVDatabase.Presentation.dll" "_MP2SlimTV\References\" /R /Y
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.dll" "_MP2SlimTV\References\" /R /Y
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVLibrary.dll" "_MP2SlimTV\References\" /R /Y
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVLibrary.IntegrationProvider.Interfaces.dll" "_MP2SlimTV\References\" /R /Y
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVLibrary.Interfaces.dll" "_MP2SlimTV\References\" /R /Y
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVLibrary.Services.dll" "_MP2SlimTV\References\" /R /Y
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TvLibrary.Utils.dll" "_MP2SlimTV\References\" /R /Y
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.TVService.Interfaces.dll" "_MP2SlimTV\References\" /R /Y
xcopy "TvService\bin\%TARGET%\StreamingServer.dll" "_MP2SlimTV\References\" /R /Y
xcopy "TvService\bin\%TARGET%\TsWriter.ax" "_MP2SlimTV\References\" /R /Y
xcopy "..\..\..\..\DirectShowFilters\MPWriter\bin\Release\MPFileWriter.ax" "_MP2SlimTV\References\" /R /Y
xcopy "..\..\..\..\DirectShowFilters\MPIPTVSource\bin\Release\MPIPTVSource.ax" "_MP2SlimTV\References\" /R /Y
xcopy "TVLibrary.Services\bin\%TARGET%\Mediaportal.TV.Server.TVLibrary.Services.dll.config" "_MP2SlimTV\References\" /R /Y
xcopy "SetupTv\bin\%TARGET%\Mediaportal.TV.Server.RuleBasedScheduler.dll" "_MP2SlimTV\References\" /R /Y

xcopy "Plugins\PluginBase\bin\%TARGET%\Mediaportal.TV.Server.Plugins.Base.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y
xcopy "Plugins\CustomDevices\Anysee\bin\%TARGET%\Mediaportal.TV.Server.Plugins.CustomDevices.Anysee.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y
REM xcopy "Plugins\CustomDevices\AVerMedia\bin\%TARGET%\Mediaportal.TV.Server.Plugins.CustomDevices.AVerMedia.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y
xcopy "Plugins\CustomDevices\Compro\bin\%TARGET%\Mediaportal.TV.Server.Plugins.CustomDevices.Compro.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y
xcopy "Plugins\CustomDevices\Conexant\bin\%TARGET%\Mediaportal.TV.Server.Plugins.CustomDevices.Conexant.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y
xcopy "Plugins\CustomDevices\DigitalDevices\bin\%TARGET%\Mediaportal.TV.Server.Plugins.CustomDevices.DigitalDevices.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y
xcopy "Plugins\CustomDevices\DigitalEverywhere\bin\%TARGET%\Mediaportal.TV.Server.Plugins.CustomDevices.DigitalEverywhere.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y
xcopy "Plugins\CustomDevices\DvbSky\bin\%TARGET%\Mediaportal.TV.Server.Plugins.CustomDevices.DvbSky.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y
xcopy "Plugins\CustomDevices\Geniatech\bin\%TARGET%\Mediaportal.TV.Server.Plugins.CustomDevices.Geniatech.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y
xcopy "Plugins\CustomDevices\Genpix\bin\%TARGET%\Mediaportal.TV.Server.Plugins.CustomDevices.Genpix.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y
xcopy "Plugins\CustomDevices\GenpixOpenSource\bin\%TARGET%\Mediaportal.TV.Server.Plugins.CustomDevices.GenpixOpenSource.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y
xcopy "Plugins\CustomDevices\Hauppauge\bin\%TARGET%\Mediaportal.TV.Server.Plugins.CustomDevices.Hauppauge.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y
xcopy "Plugins\CustomDevices\Knc\bin\%TARGET%\Mediaportal.TV.Server.Plugins.CustomDevices.Knc.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y
xcopy "Plugins\CustomDevices\MdPlugin\bin\%TARGET%\Mediaportal.TV.Server.Plugins.CustomDevices.MdPlugin.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y
xcopy "Plugins\CustomDevices\Microsoft\bin\%TARGET%\Mediaportal.TV.Server.Plugins.CustomDevices.Microsoft.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y
xcopy "Plugins\CustomDevices\NetUp\bin\%TARGET%\Mediaportal.TV.Server.Plugins.CustomDevices.NetUp.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y
xcopy "Plugins\CustomDevices\Omicom\bin\%TARGET%\Mediaportal.TV.Server.Plugins.CustomDevices.Omicom.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y
xcopy "Plugins\CustomDevices\Prof\bin\%TARGET%\Mediaportal.TV.Server.Plugins.CustomDevices.Prof.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y
xcopy "Plugins\CustomDevices\ProfUsb\bin\%TARGET%\Mediaportal.TV.Server.Plugins.CustomDevices.ProfUsb.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y
xcopy "Plugins\CustomDevices\SmarDtvUsbCi\bin\%TARGET%\Mediaportal.TV.Server.Plugins.CustomDevices.SmarDtvUsbCi.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y
xcopy "Plugins\CustomDevices\TechnoTrend\bin\%TARGET%\Mediaportal.TV.Server.Plugins.CustomDevices.TechnoTrend.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y
xcopy "Plugins\CustomDevices\TeVii\bin\%TARGET%\Mediaportal.TV.Server.Plugins.CustomDevices.TeVii.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y
xcopy "Plugins\CustomDevices\Turbosight\bin\%TARGET%\Mediaportal.TV.Server.Plugins.CustomDevices.Turbosight.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y
xcopy "Plugins\CustomDevices\Twinhan\bin\%TARGET%\Mediaportal.TV.Server.Plugins.CustomDevices.Twinhan.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y
xcopy "Plugins\CustomDevices\ViXS\bin\%TARGET%\Mediaportal.TV.Server.Plugins.CustomDevices.ViXS.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y
xcopy "TvService\bin\%TARGET%\Mediaportal.TV.Server.SetupControls.dll" "_MP2SlimTV\References\Plugins\CustomDevices\" /R /Y

xcopy "SetupTv\bin\%TARGET%\*.dll" "_MP2SlimTV\References\SetupTv\" /R /Y
xcopy "..\..\..\..\DirectShowFilters\TsReader\bin\Release\TsReader.ax" "_MP2SlimTV\References\SetupTv\" /R /Y
xcopy "SetupTv\bin\%TARGET%\log4net.config" "_MP2SlimTV\References\SetupTv\" /R /Y
xcopy "SetupTv\bin\%TARGET%\SetupTV.exe" "_MP2SlimTV\References\SetupTv\" /R /Y
xcopy "SetupTv\bin\%TARGET%\SetupTV.exe.config" "_MP2SlimTV\References\SetupTv\" /R /Y

