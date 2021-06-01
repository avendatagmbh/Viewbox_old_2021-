using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using AvdCommon;
using AvdCommon.Logging;
using DbSearch.Localisation;
using DbSearchLogic.Manager;
using AV.Log;
using log4net;

namespace DbSearch {
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary> 
    public partial class App : Application {

        internal ILog _log = LogHelper.GetLogger();

        public App() {
            _log.InfoWithCheck("Program started");
            Global.ApplicationName = "DbSearch";
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            Exception ex = e.ExceptionObject as Exception;
            string message = "";
            if (ex != null)
                message = ex.Message;
            _log.ErrorWithCheck(message, ex);
            ShowMessage(message, ex);
        }

        private void ShowMessage(string message, Exception ex) {
            string errorMessage = string.Format("Eine unbehandelte Ausnahme ist aufgetreten: {0}", Environment.NewLine + message);
            //LogManager.Error(message, ex);
            _log.Error(message, ex); 
            MessageBox.Show(errorMessage, "", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            //string errorMessage = string.Format("Eine unbehandelte Ausnahme ist aufgetreten: {0}", Environment.NewLine + e.Exception.Message);
            //MessageBox.Show(errorMessage, "", MessageBoxButton.OK, MessageBoxImage.Error);
            ShowMessage(e.Exception.Message, e.Exception);
            if(!(e.Exception is OutOfMemoryException))
                e.Handled = true;
        }

        private void Application_Startup(object sender, StartupEventArgs e) {
            Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(AppDispatcherUnhandledException);

            try {
                ApplicationManager.Load(ProfileManager.Profiles, App.Current.Dispatcher);
            } catch (Exception ex) {
                MessageBox.Show(ResourcesGui.Error + Environment.NewLine + ex.Message, "", MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            ShowMessage(e.Exception.Message, e.Exception);
            if (!(e.Exception is OutOfMemoryException))
                e.Handled = true;
        }

    }
}
