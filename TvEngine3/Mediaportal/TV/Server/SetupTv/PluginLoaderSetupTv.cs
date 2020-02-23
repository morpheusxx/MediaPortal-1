#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.IO;
using System.IO.Compression;
using Mediaportal.TV.Server.Plugins.Base;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.SetupTV
{
  public class PluginLoaderSetupTv : PluginLoader
  {
    /// <summary>
    /// Loads all plugins.
    /// </summary>
    public override void Load()
    {
      try
      {
        RetrievePluginsFromServer();
        base.Load();
      }
      catch (Exception ex)
      {
        this.LogError(ex, "PluginLoaderSetupTv.Load - could not load plugins");
      }
    }

    private void RetrievePluginsFromServer()
    {
      if (File.Exists(@"tvservice.exe"))
      {
        return;
      }

      byte[] pluginsZip = ServiceAgents.Instance.ControllerServiceAgent.GetPluginBinariesZipped();
      if (pluginsZip == null)
        return;

      string pluginsFolder = PathManager.BuildAssemblyRelativePath("plugins");
      if (!Directory.Exists(pluginsFolder))
        Directory.CreateDirectory(pluginsFolder);

      using (var ms = new MemoryStream(pluginsZip))
      using (var zip = new ZipArchive(ms))
      {
        foreach (ZipArchiveEntry entry in zip.Entries)
        {
          var targetFile = pluginsFolder + entry.FullName; // No Path.Combine here, because the entry contains a leading backslash, which makes the result root based.
          var targetFolder = Path.GetDirectoryName(targetFile);
          if (!Directory.Exists(targetFolder))
            Directory.CreateDirectory(targetFolder);

          if (File.Exists(targetFile))
            continue;
          using (var fsStream = new FileStream(targetFile, FileMode.Create))
          using (var stream = entry.Open())
          {
            stream.CopyTo(fsStream);
          }
        }
      }
    }
  }
}