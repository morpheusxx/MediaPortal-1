#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Castle.DynamicProxy;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.Plugins.Base.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.Base
{
  public class PluginLoader
  {
    private List<ITvServerPlugin> _plugins = new List<ITvServerPlugin>();
    private readonly List<Type> _incompatiblePlugins = new List<Type>();

    /// <summary>
    /// returns a list of all plugins loaded.
    /// </summary>
    /// <value>The plugins.</value>
    public List<ITvServerPlugin> Plugins
    {
      get { return _plugins; }
    }

    /// <summary>
    /// returns a list of plugins not loaded as incompatible.
    /// </summary>
    /// <value>The plugins.</value>
    public List<Type> IncompatiblePlugins
    {
      get { return _incompatiblePlugins; }
    }

    /// <summary>
    /// Loads all plugins.
    /// </summary>
    public virtual void Load()
    {
      _plugins.Clear();
      _incompatiblePlugins.Clear();
      try
      {
        // Load plugins from "plugins" subfolder, relative to calling assembly's location
        string pluginFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), "Plugins");
        var assemblyFilter = new AssemblyFilter(pluginFolder);
        IWindsorContainer container = Instantiator.Instance.Container();

        container.Register(Component.For<IInterceptor>().ImplementedBy<PluginExceptionInterceptor>().IsDefault().Named("PluginExceptionInterceptor"));

        container.Register(
          Classes.FromAssemblyInDirectory(assemblyFilter).
            BasedOn<ITvServerPlugin>().
            If(IsPluginCompatible).
            WithServiceBase().
            LifestyleSingleton()
            );

        assemblyFilter = new AssemblyFilter(Path.Combine(pluginFolder, "CustomDevices"));
        container.Register(
          Classes.FromAssemblyInDirectory(assemblyFilter).
            BasedOn<ITvServerPlugin>().
            If(IsPluginCompatible).
            WithServiceBase().
            LifestyleSingleton().
            Configure(c => c.Named(c.Implementation.Name + "Plugin"))
            );

        _plugins = new List<ITvServerPlugin>(container.ResolveAll<ITvServerPlugin>());

        foreach (ITvServerPlugin plugin in _plugins)
        {
          this.LogDebug("PluginManager: Loaded {0} version:{1} author:{2}", plugin.Name, plugin.Version, plugin.Author);
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "PluginManager: Error while loading DLLs.");
      }
    }

    private bool IsPluginCompatible(Type type)
    {
      bool isPluginCompatible = CompatibilityManager.IsPluginCompatible(type, true);
      if (!isPluginCompatible)
      {
        _incompatiblePlugins.Add(type);
        this.LogDebug("PluginManager: {0} is incompatible with the current tvserver version and won't be loaded!", type.FullName);
      }
      return isPluginCompatible;
    }
  }
}