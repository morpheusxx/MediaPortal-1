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

namespace Mediaportal.TV.Server.TVLibrary.Interfaces
{
  /// <summary>
  /// Class for definition of globally used constants
  /// </summary>
  public class TvConstants
  {
    /// <summary>
    /// all constant Tv channel group names
    /// </summary>
    public static class TvGroupNames
    {
      /// <summary>
      /// Name of group where all (new) channels are stored
      /// </summary>
      public static string AllChannels = "All Channels";

      /// <summary>
      /// Name of group where all analog channels are stored
      /// </summary>
      public static string Analog = "Analog";

      /// <summary>
      /// Name of group where all DVB-T channels are stored
      /// </summary>
      public static string DVBT = "Digital terrestrial";

      /// <summary>
      /// Name of group where all DVB-C channels are stored
      /// </summary>
      public static string DVBC = "Digital cable";

      /// <summary>
      /// Name of group where all DVB-S channels are stored
      /// </summary>
      public static string DVBS = "Digital satellite";
    }

    /// <summary>
    /// all constant Radio channel group names
    /// </summary>
    public static class RadioGroupNames
    {
      /// <summary>
      /// Name of group where all (new) channels are stored
      /// </summary>
      public static string AllChannels = "All Channels";

      /// <summary>
      /// Name of group where all analog channels are stored
      /// </summary>
      public static string Analog = "Analog";

      /// <summary>
      /// Name of group where all DVB-T channels are stored
      /// </summary>
      public static string DVBT = "Digital terrestrial";

      /// <summary>
      /// Name of group where all DVB-C channels are stored
      /// </summary>
      public static string DVBC = "Digital cable";

      /// <summary>
      /// Name of group where all DVB-S channels are stored
      /// </summary>
      public static string DVBS = "Digital satellite";
    }
  }

  public class Consts
  {
    public const string SETTINGS_KEY_HOSTNAME = "hostname";
    public const string SETTINGS_DEFAULTS_HOSTNAME = "localhost";
    public const string SETTINGS_KEY_RTSPPORT = "rtspport";
    public const string SETTINGS_KEY_PRE_RECORD_INTERVAL = "preRecordInterval";
    public const int SETTINGS_DEFAULTS_PRE_RECORD_INTERVAL = 7;
    public const string SETTINGS_KEY_POST_RECORD_INTERVAL = "postRecordInterval";
    public const int SETTINGS_DEFAULTS_POST_RECORD_INTERVAL = 10;

    /// <summary>
    /// {0} contains card number.
    /// </summary>
    public const string SETTINGS_KEY_CARD_ANALOG_COUNTRY = "analog{0}Country";
    /// <summary>
    /// {0} contains card number.
    /// </summary>
    public const string SETTINGS_KEY_CARD_ANALOG_SOURCE = "analog{0}Source";
    /// <summary>
    /// {0} contains card number.
    /// </summary>
    public const string SETTINGS_KEY_CARD_ANALOG_SIGNAL_GROUP = "analog{0}createsignalgroup";
  }
}