// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-08-25
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Windows;
using System.Windows.Threading;
using AV.Log;
using Base.EventArgs;
using Base.Localisation;
using Business;
using log4net;

namespace TransDATA {
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application {
        internal ILog _log = LogHelper.GetLogger();

        public App() {
            _log.ContextLog(LogLevelEnum.Info,"Application started: {0} v{1}", System.Reflection.Assembly.GetEntryAssembly().GetName().Name, System.Reflection.Assembly.GetEntryAssembly().GetName().Version);
            AvdCommon.Global.ApplicationName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            string senderString = "";
            try {
                senderString = sender.ToString();
            } catch (Exception) {
            }

            Exception ex = e.ExceptionObject as Exception;
            string message = "";
            if (ex != null)
                message = ex.Message;

            //_log.ContextLog( LogLevelEnum.Warn,"UnhandledException: {0} Sender: {1}", message, senderString);
            using (NDC.Push(LogHelper.GetNamespaceContext())) {
                _log.Error(message, ex);
            }
            ShowMessage(message);
        }

        private void ShowMessage(string message) {
            string errorMessage = string.Format(ExceptionMessages.UnhandledException, message);
            _log.ContextLog(LogLevelEnum.Error, "ShowMessage: {0}", message);
            MessageBox.Show(errorMessage, "", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            string errorMessage = string.Format(ExceptionMessages.UnhandledException, e.Exception.Message);
            //_log.ContextLog(LogLevelEnum.Error, "OnDispatcherUnhandledException: {0}", errorMessage);
            using (NDC.Push(LogHelper.GetNamespaceContext())) {
                _log.Error(errorMessage, e.Exception);
            }
            MessageBox.Show(errorMessage, "", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void ApplicationStartup(object sender, StartupEventArgs e) {
            Current.DispatcherUnhandledException += AppDispatcherUnhandledException;
            AppController.Error += AppController_Error;
            AppController.Init();
            AppController.Error -= AppController_Error;
        }

        private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            ShowMessage(e.Exception.Message);
            e.Handled = true;
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            ShowMessage(e.Exception.Message);
            e.Handled = true;
        }

        private void ApplicationExit(object sender, ExitEventArgs e) {
            _log.ContextLog( LogLevelEnum.Info,"ApplicationExit");

            AppController.Error += AppController_Error;
            AppController.Shutdown();
            AppController.Error -= AppController_Error;
        }

        private void AppController_Error(object sender, MessageEventArgs e) {
            string senderString = "";
            try
            {
                senderString = sender.ToString();
            }
            catch (Exception)
            {
            } 
            string errorMessage = e.Message;
            _log.ContextLog(LogLevelEnum.Error, "AppController_Error: {0} Sender: {1}", errorMessage, senderString);
            MessageBox.Show(errorMessage, ResourcesCommon.Error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}