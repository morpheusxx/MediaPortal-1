using System;
using System.Collections.Generic;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class Card
  {
    public int CardId { get; set; }
    public string DevicePath { get; set; }
    public string Name { get; set; }
    public int Priority { get; set; }
    public bool GrabEPG { get; set; }
    public DateTime? LastEpgGrab { get; set; }
    public string RecordingFolder { get; set; }
    public bool Enabled { get; set; }
    public int CamType { get; set; }
    public string TimeshiftingFolder { get; set; }
    public int DecryptLimit { get; set; }
    public bool PreloadCard { get; set; }
    public int NetProvider { get; set; }
    public int IdleMode { get; set; }
    public int MultiChannelDecryptMode { get; set; }
    public bool AlwaysSendDiseqcCommands { get; set; }
    public int DiseqcCommandRepeatCount { get; set; }
    public int PidFilterMode { get; set; }
    public bool UseCustomTuning { get; set; }
    public bool UseConditionalAccess { get; set; }

    public virtual ICollection<CardGroupMap> CardGroupMaps { get; set; }
    public virtual ICollection<DisEqcMotor> DisEqcMotors { get; set; }
    public virtual ICollection<ChannelMap> ChannelMaps { get; set; }
    public virtual ICollection<Conflict> Conflicts { get; set; }
  }
}
