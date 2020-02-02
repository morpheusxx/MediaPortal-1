using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(Channel))]
  [KnownType(typeof(LnbType))]
  public class TuningDetail
  {
    [DataMember]
    public int TuningDetailId { get; set; }
    [DataMember]
    public int ChannelId { get; set; }
    [DataMember]
    public string Name { get; set; }
    [DataMember]
    public string Provider { get; set; }
    [DataMember]
    public int ChannelType { get; set; }
    [DataMember]
    public int ChannelNumber { get; set; }
    [DataMember]
    public int Frequency { get; set; }
    [DataMember]
    public int CountryId { get; set; }
    [DataMember]
    public int NetworkId { get; set; }
    [DataMember]
    public int TransportId { get; set; }
    [DataMember]
    public int ServiceId { get; set; }
    [DataMember]
    public int PmtPid { get; set; }
    [DataMember]
    public bool FreeToAir { get; set; }
    [DataMember]
    public int Modulation { get; set; }
    [DataMember]
    public int Polarisation { get; set; }
    [DataMember]
    public int Symbolrate { get; set; }
    [DataMember]
    public int DiSEqC { get; set; }
    [DataMember]
    public int Bandwidth { get; set; }
    [DataMember]
    public int MajorChannel { get; set; }
    [DataMember]
    public int MinorChannel { get; set; }
    [DataMember]
    public int VideoSource { get; set; }
    [DataMember]
    public int TuningSource { get; set; }
    [DataMember]
    public int Band { get; set; }
    [DataMember]
    public int SatIndex { get; set; }
    [DataMember]
    public int InnerFecRate { get; set; }
    [DataMember]
    public int Pilot { get; set; }
    [DataMember]
    public int RollOff { get; set; }
    [DataMember]
    public string Url { get; set; }
    [DataMember]
    public int Bitrate { get; set; }
    [DataMember]
    public int AudioSource { get; set; }
    [DataMember]
    public bool IsVCRSignal { get; set; }
    [DataMember]
    public int MediaType { get; set; }
    [DataMember]
    public int? LnbTypeId { get; set; }

    [DataMember]
    public Channel Channel { get; set; }
    [DataMember]
    public LnbType LnbType { get; set; }
  }
}
