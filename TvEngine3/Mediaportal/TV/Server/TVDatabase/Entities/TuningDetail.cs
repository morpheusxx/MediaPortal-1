namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class TuningDetail
  {
    public int TuningDetailId { get; set; }
    public int ChannelId { get; set; }
    public string Name { get; set; }
    public string Provider { get; set; }
    public int ChannelType { get; set; }
    public int ChannelNumber { get; set; }
    public int Frequency { get; set; }
    public int CountryId { get; set; }
    public int NetworkId { get; set; }
    public int TransportId { get; set; }
    public int ServiceId { get; set; }
    public int PmtPid { get; set; }
    public bool FreeToAir { get; set; }
    public int Modulation { get; set; }
    public int Polarisation { get; set; }
    public int Symbolrate { get; set; }
    public int DiSEqC { get; set; }
    public int Bandwidth { get; set; }
    public int MajorChannel { get; set; }
    public int MinorChannel { get; set; }
    public int VideoSource { get; set; }
    public int TuningSource { get; set; }
    public int Band { get; set; }
    public int SatIndex { get; set; }
    public int InnerFecRate { get; set; }
    public int Pilot { get; set; }
    public int RollOff { get; set; }
    public string Url { get; set; }
    public int Bitrate { get; set; }
    public int AudioSource { get; set; }
    public bool IsVCRSignal { get; set; }
    public int MediaType { get; set; }
    public int? LnbTypeId { get; set; }

    public Channel Channel { get; set; }
    public LnbType LnbType { get; set; }
  }
}
