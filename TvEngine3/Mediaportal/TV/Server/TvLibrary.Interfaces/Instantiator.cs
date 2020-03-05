using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces
{
  public class Instantiator : Singleton<Instantiator>
  {
    /// <summary>
    /// Get or Create an IoC container
    /// </summary>
    /// <param name="configFile">Path to an external castle.config file.
    /// Can be <c>null</c> the force reading of the app.config.
    /// Can be <c>none</c> to construct WindsorContainer with defaults only.
    /// </param>
    /// <returns></returns>
    public IWindsorContainer Container(string configFile = null)
    {
      var container = GlobalServiceProvider.Instance.Get<IWindsorContainer>();
      if (container == null)
      {
        if (string.IsNullOrEmpty(configFile))
          container = new WindsorContainer(new XmlInterpreter());
        else if (configFile == "none")
          container = new WindsorContainer();
        else
          container = new WindsorContainer(configFile);
        GlobalServiceProvider.Instance.Add<IWindsorContainer>(container);
      }
      return container;
    }
  }
}
