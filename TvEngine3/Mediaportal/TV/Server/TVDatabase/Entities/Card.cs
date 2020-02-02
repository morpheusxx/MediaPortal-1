using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(CardGroupMap))]
  [KnownType(typeof(DisEqcMotor))]
  [KnownType(typeof(ChannelMap))]
  [KnownType(typeof(Conflict))]
  public class Card
  {
    [DataMember]
    public int CardId { get; set; }
    [DataMember]
    public string DevicePath { get; set; }
    [DataMember]
    public string Name { get; set; }
    [DataMember]
    public int Priority { get; set; }
    [DataMember]
    public bool GrabEPG { get; set; }
    [DataMember]
    public DateTime? LastEpgGrab { get; set; }
    [DataMember]
    public string RecordingFolder { get; set; }
    [DataMember]
    public bool Enabled { get; set; }
    [DataMember]
    public int CamType { get; set; }
    [DataMember]
    public string TimeshiftingFolder { get; set; }
    [DataMember]
    public int DecryptLimit { get; set; }
    [DataMember]
    public bool PreloadCard { get; set; }
    [DataMember]
    public int NetProvider { get; set; }
    [DataMember]
    public int IdleMode { get; set; }
    [DataMember]
    public int MultiChannelDecryptMode { get; set; }
    [DataMember]
    public bool AlwaysSendDiseqcCommands { get; set; }
    [DataMember]
    public int DiseqcCommandRepeatCount { get; set; }
    [DataMember]
    public int PidFilterMode { get; set; }
    [DataMember]
    public bool UseCustomTuning { get; set; }
    [DataMember]
    public bool UseConditionalAccess { get; set; }

    public virtual ICollection<CardGroupMap> CardGroupMaps { get; set; }
    public virtual ICollection<DisEqcMotor> DisEqcMotors { get; set; }
    public virtual ICollection<ChannelMap> ChannelMaps { get; set; }
    public virtual ICollection<Conflict> Conflicts { get; set; }
  }
}
