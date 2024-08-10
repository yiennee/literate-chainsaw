using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using ZephyrInnovations;

namespace LiterateChainsaw
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        Mutex mutex;
        static string basedir = AppDomain.CurrentDomain.BaseDirectory;
        public App()
        {
            ShutdownMode = ShutdownMode.OnLastWindowClose;
            SetupUnhandledExceptionHandling();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            bool success = false;
            string errMsg = "";

            #region Single_Instance_Control

            bool isNewInstance = false;

            string assemblyName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;

            mutex = new Mutex(true, assemblyName, out isNewInstance);

            if (!isNewInstance)
            {
                MessageBox.Show("A QVS application instance is running already", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                //App.Current.Shutdown();
                Environment.Exit(0);
            }
            #endregion

            if (File.Exists(qGlobal.MainConfigurationFile) == false)
                MessageBox.Show("Main Config file not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                qGlobal.LoadMainConfigurationFile(qGlobal.MainConfigurationFile);

            success = qGlobal.LoadDefectCodeSettingFile(out errMsg);
            qGlobal.WriteRunLog(true, "End: Load Defect Code Setting", "INFO");

            if (success == true)
                success = qGlobal.LoadDefectGroupSettingFile(out errMsg);
            qGlobal.WriteRunLog(true, "End: Load Defect Group Setting", "INFO");

            if (success == true)
                success = qGlobal.LoadVisionRecipeSettingFile(out errMsg);
            qGlobal.WriteRunLog(true, "End: Load Vision Recipe Setting", "INFO");

            if (success == false)
            {
                MessageBox.Show(errMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //Application.Current.Shutdown();
                Environment.Exit(0);
            }
        }

        private void SetupUnhandledExceptionHandling()
        {
            // Catch exceptions from all threads in the AppDomain.
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                ShowUnhandledException(args.ExceptionObject as Exception, "AppDomain.CurrentDomain.UnhandledException", false);

            // Catch exceptions from each AppDomain that uses a task scheduler for async operations.
            TaskScheduler.UnobservedTaskException += (sender, args) =>
                ShowUnhandledException(args.Exception, "TaskScheduler.UnobservedTaskException", false);

            // Catch exceptions from a single specific UI dispatcher thread.
            Dispatcher.UnhandledException += (sender, args) =>
            {
                // If we are debugging, let Visual Studio handle the exception and take us to the code that threw it.
                if (!Debugger.IsAttached)
                {
                    args.Handled = true;
                    ShowUnhandledException(args.Exception, "Dispatcher.UnhandledException", true);
                }
            };
        }
        void ShowUnhandledException(Exception e, string unhandledExceptionType, bool promptUserForShutdown)
        {
            var messageBoxTitle = $"Unexpected Error Occurred: {unhandledExceptionType}";
            var messageBoxMessage = $"The following exception occurred:\n\n{e}";
            var messageBoxButtons = MessageBoxButton.OK;

            if (promptUserForShutdown)
            {
                messageBoxMessage += "\n\nNormally the app would die now. Should we let it die?";
                messageBoxButtons = MessageBoxButton.YesNo;
            }

            // Let the user decide if the app should die or not (if applicable).
            if (MessageBox.Show(messageBoxMessage, messageBoxTitle, messageBoxButtons) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
