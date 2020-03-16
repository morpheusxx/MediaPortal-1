#region Copyright (C) 2007-2018 Team MediaPortal

/*
    Copyright (C) 2007-2018 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System.Runtime.InteropServices;

namespace MediaPortal.Common.UI
{
  public static class FormDpiAwarenessExtension
  {
    [DllImport("SHCore.dll")]
    private static extern bool SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

    private enum PROCESS_DPI_AWARENESS
    {
      Process_DPI_Unaware = 0,
      Process_System_DPI_Aware = 1,
      Process_Per_Monitor_DPI_Aware = 2
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetProcessDPIAware();

    /// <summary>
    /// Sets the current process to DPI aware. This allows the Form to scale fonts properly.
    /// </summary>
    public static void TryEnableDPIAwareness()
    {
      try
      {
        SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.Process_Per_Monitor_DPI_Aware);
      }
      catch
      {
        try
        {
          // fallback, use (simpler) internal function
          SetProcessDPIAware();
        }
        catch { }
      }
    }
  }
}
