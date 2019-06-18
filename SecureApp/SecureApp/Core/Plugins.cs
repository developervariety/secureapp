using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SecureApp.Model;
using SecureAppUtil.Model.Interface;

namespace SecureApp.Core
{
    public class Plugins
    {
        public static void LoadPlugins()
        {
            Settings.Plugins = new List<IPlugin>();

            if (Directory.Exists(Location.Plugins))
            {
                string[] plugins = Directory.GetFiles(Location.Plugins, "*.dll", SearchOption.AllDirectories);

                foreach (string plugin in plugins)
                {
                    Assembly.LoadFile(Path.GetFullPath(plugin));
                }
            }
            
            Type interfaceType = typeof(IPlugin);
            Type[] types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(p => interfaceType.IsAssignableFrom(p) && p.IsClass)
                .ToArray();
            
            foreach (Type type in types)
            {
                Settings.Plugins.Add((IPlugin)Activator.CreateInstance(type));
            }
        }
    }
}