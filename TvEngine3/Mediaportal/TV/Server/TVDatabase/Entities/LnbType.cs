using System.Collections.Generic;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class LnbType
  {
    public int LnbTypeId { get; set; }
    public string Name { get; set; }
    public int LowBandFrequency { get; set; }
    public int HighBandFrequency { get; set; }
    public int SwitchFrequency { get; set; }
    public bool IsBandStacked { get; set; }
    public bool IsToroidal { get; set; }

    public virtual ICollection<TuningDetail> TuningDetails { get; set; }

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
