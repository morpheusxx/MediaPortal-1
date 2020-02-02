using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVLibrary.IntegrationProvider.Interfaces;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces
{
  public class PathManager
  {
    /// <summary>
    /// Returns the path to the Application Data location
    /// </summary>
    /// <returns>Application data path of TvServer</returns>
    public static string GetDataPath
    {
      get
      {
        return GlobalServiceProvider.Instance.Get<IIntegrationProvider>().PathManager.GetPath("<TVCORE>");
      }
    }

    /// <summary>
    /// Builds a full path for a given <paramref name="fileName"/> that is located in the same folder as the <see cref="Assembly.GetCallingAssembly"/>.
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <returns>Combined path</returns>
    public static string BuildAssemblyRelativePath(string fileName)
    {
      string executingPath = Assembly.GetCallingAssembly().Location;
      return Path.Combine(Path.GetDirectoryName(executingPath), fileName);
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
    public static extern bool SetDefaultDllDirectories(uint flags);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool AddDllDirectory(string path);

    #region LoadLibraryEx Flags

    public const uint LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR = 0x00000100;
    public const uint LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000;
    public const uint LOAD_LIBRARY_SEARCH_USER_DIRS = 0x00000400;

    #endregion

    /// <summary>
    /// Helper method to set the native Dll search path to a subfolder relative to calling assembly.
    /// <example>
    /// BassPlugin.dll with following subfolders for platform specific binaries:
    ///   \x86\
    ///   \x64\
    /// </example>
    /// </summary>
    /// <returns><c>true</c> if path could be set successfully.</returns>
    public static bool SetPlatformSearchDirectories(out string selectedPath)
    {
      string platformDir = IntPtr.Size > 4 ? "x64" : "x86";
      string executingPath = Assembly.GetCallingAssembly().Location;
      string absolutePlatformDir = Path.Combine(Path.GetDirectoryName(executingPath), platformDir);
      SetDefaultDllDirectories(LOAD_LIBRARY_SEARCH_USER_DIRS | LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);
      selectedPath = absolutePlatformDir;
      return AddDllDirectory(absolutePlatformDir);
    }

    /// <summary>
    /// Builds a full path for a given <paramref name="fileName"/> sorted by subfolder architecture (x64 or x86) that is located in the same folder as the <see cref="Assembly.GetCallingAssembly"/>.
    /// /// </summary>
    /// <param name="fileName">File name</param>
    /// <returns>Combined path</returns>
    public static string BuildAssemblyRelativePathForArchitecture(string fileName)
    {
      string executingPath = Assembly.GetCallingAssembly().Location;
      string architecture = Environment.Is64BitOperatingSystem ? "x64" : "x86";
      return Path.Combine(Path.GetDirectoryName(executingPath), architecture, fileName);
    }

  }
}
