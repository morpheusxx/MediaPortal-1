using System;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [AttributeUsage(AttributeTargets.All)]
  public class ProgramAttribute : Attribute
  {
    public string DisplayName { get; }
    public int LanguageId { get; }

    public ProgramAttribute(string displayName, int languageId)
    {
      DisplayName = displayName;
      LanguageId = languageId;
    }
  }
}
