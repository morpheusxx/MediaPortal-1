using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [DataContract(IsReference = true)]
  [KnownType(typeof(TuningDetail))]
  public class LnbType
  {
    [DataMember]
    public int LnbTypeId { get; set; }
    [DataMember]
    public string Name { get; set; }
    [DataMember]
    public int LowBandFrequency { get; set; }
    [DataMember]
    public int HighBandFrequency { get; set; }
    [DataMember]
    public int SwitchFrequency { get; set; }
    [DataMember]
    public bool IsBandStacked { get; set; }
    [DataMember]
    public bool IsToroidal { get; set; }

    [DataMember]
    public virtual List<TuningDetail> TuningDetails { get; set; } = new List<TuningDetail>();

    public LnbType Clone()
    {
      var l = new LnbType
      {
        LnbTypeId = LnbTypeId,
        IsBandStacked = IsBandStacked,
        IsToroidal = IsToroidal,
        LowBandFrequency = LowBandFrequency,
        Name = Name,
        SwitchFrequency = SwitchFrequency,
        HighBandFrequency = HighBandFrequency
      };
      return l;
    }

    public override string ToString()
    {
      return Name;
    }
  }
}
