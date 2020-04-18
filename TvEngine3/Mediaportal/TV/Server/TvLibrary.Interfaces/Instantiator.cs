using Castle.Windsor;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces
{
  public class Instantiator : Singleton<Instantiator>
  {
    /// <summary>
    /// Get or Create an IoC container
    /// </summary>
    /// <param name="configFile">Path to an external castle.config file.
    /// Can be <c>null</c> to construct WindsorContainer with defaults only.
    /// </param>
    /// <returns></returns>
    public IWindsorContainer Container(string configFile = null)
    {
      var container = GlobalServiceProvider.Instance.Get<IWindsorContainer>();
      if (container == null)
      {
        container = string.IsNullOrEmpty(configFile) ? new WindsorContainer() : new WindsorContainer(configFile);
        GlobalServiceProvider.Instance.Add<IWindsorContainer>(container);
      }
      return container;
    }
  }
}
