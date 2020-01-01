using NLog;
using System.Windows;
using wKpr.Properties;

namespace wKpr
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly NLog.Logger _sLog = NLog.LogManager.GetCurrentClassLogger();
        protected override void OnExit(ExitEventArgs e)
        {
            Settings.Default.Save();
            _sLog.Debug("Exit");
            NLog.LogManager.Shutdown(); // Flush and close down internal threads and timers
            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            #region Logging
            if (e.Args != null && e.Args.Length > 0 && e.Args[0] == "log")
            {
                var config = new NLog.Config.LoggingConfiguration();
                var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "wKpr.log" , Layout= "${longdate}|${level}|${callsite}|${message}  ${exception}" };

                config.AddTarget(logfile); 
                config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

                // Apply config           
                NLog.LogManager.Configuration = config;
                _sLog.Debug("startup");
            }
            #endregion


            base.OnStartup(e);
            View.MainWindow mainWindow = new View.MainWindow();
            mainWindow.Show();
        }
    }
}
