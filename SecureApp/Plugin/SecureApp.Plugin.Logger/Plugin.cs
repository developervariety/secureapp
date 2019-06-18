using NLog;
using NLog.Config;
using NLog.Targets;
using SecureAppUtil.Model.Interface;

namespace SecureApp.Plugin.Logger
{
    public class Plugin : IPlugin
    {
        public string Version => "1.0.0";
        public string Author => "SecureApp";
        public string Description => "Logs information sent to server using NLog.";
        public string Name => "Logger";

        private static LoggingConfiguration _config;
        private static NLog.Logger _logger;

        public void Init()
        {
            _config = new LoggingConfiguration();

            FileTarget logfile = new FileTarget("logFile") {FileName = "logs.txt"};
            ConsoleTarget logConsole = new ConsoleTarget("logConsole");

            _config.AddRule(LogLevel.Info, LogLevel.Fatal, logConsole);
            _config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            LogManager.Configuration = _config;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public void Execute(params object[] args)
        {
            _logger.Info(args);
        }
    }
}
