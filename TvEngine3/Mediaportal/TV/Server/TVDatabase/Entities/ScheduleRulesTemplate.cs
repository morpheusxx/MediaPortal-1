namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class ScheduleRulesTemplate
  {
    public int ScheduleRulesTemplateId { get; set; }
    public string Name { get; set; }
    public string Rules { get; set; }
    public bool Enabled { get; set; }
    public int Usages { get; set; }
    public bool Editable { get; set; }
  }
}
