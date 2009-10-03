using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using log4net;
using log4net.Core;
using WooYek.Common.Logging;

namespace HDBrowser.Client {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private static readonly ILog log = LogManager.GetLogger(typeof (App));

        protected override void OnStartup(StartupEventArgs e) {
            StartLogging(Level.Debug);
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args) {
            log.Error("OnDispatcherUnhandledException: ", args.Exception);
            this.Shutdown();
        }

        private static void StartLogging(Level level) {
            bool useAppConfigSettins = true;
            if (useAppConfigSettins) {
                log4net.Config.XmlConfigurator.Configure();
            } else {
                LoggingConfig.RollingAppendToFile = false;
                LoggingConfig.RollingFileBackups = 0;
                LoggingConfig.ConfigureLogging(level, Assembly.GetExecutingAssembly());
            }
            log.InfoFormat("Environment.OSVersion: {0}", Environment.OSVersion);
            log.InfoFormat("Environment.Version: {0}", Environment.Version);
            Assembly assembly = Assembly.GetExecutingAssembly();
            log.InfoFormat("Assembly: {0}", assembly.FullName);
            log.InfoFormat("Version: {0}", assembly.GetName().Version);
            AssemblyDescriptionAttribute assemblyDescriptionAttribute = (AssemblyDescriptionAttribute) assembly.GetCustomAttributes(typeof (AssemblyDescriptionAttribute), false)[0];
            log.InfoFormat("Description: {0}",assemblyDescriptionAttribute.Description);
            AssemblyConfigurationAttribute assemblyConfigurationAttribute = (AssemblyConfigurationAttribute) assembly.GetCustomAttributes(typeof (AssemblyConfigurationAttribute), false)[0];
            log.InfoFormat("Configuration: {0}",assemblyConfigurationAttribute.Configuration);
        }
    }
}