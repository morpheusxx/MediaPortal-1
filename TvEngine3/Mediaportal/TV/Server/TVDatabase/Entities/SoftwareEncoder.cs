namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class SoftwareEncoder
  {
    public int SoftwareEncoderId { get; set; }
    public int Priority { get; set; }
    public string Name { get; set; }
    public int Type { get; set; }
    public bool Reusable { get; set; }
  }
}
