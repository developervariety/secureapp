using System.Linq;
using SecureApp.Utilities.Model.Interface;
using SecureApp.Utilities.Network.Server;

namespace SecureApp.Core
{
    public class Global
    {
        public static SecureSocketServer Socket { get; set; }
        public static RemoteFunctions RemoteFunctions { get; set; }
        
        public static IPlugin Logger { get; set; }

        public static void Init()
        {
            Socket = new SecureSocketServer();
            RemoteFunctions = new RemoteFunctions();
            new Callbacks();

            Plugins.LoadPlugins();
            foreach (IPlugin plugin in Settings.Plugins)
            {
                plugin.Init();
            }
            
            Logger = Settings.Plugins.FirstOrDefault(p => p.Name == "Logger");
            
            Logger?.Execute("Successfully initiated.");

            Socket.StartServer(100);
            Logger?.Execute("Initiated server.");
        }
    }
}