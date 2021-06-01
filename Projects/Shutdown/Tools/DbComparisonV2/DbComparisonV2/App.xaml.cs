using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using AV.Log;
using AvdCommon;
using System.Windows.Threading;
using DbComparisonV2.Models;
using log4net;
using LogManager = AvdCommon.Logging.LogManager;

namespace DbComparisonV2
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application {
        private ILog log = LogHelper.GetLogger();

        public App() {
            Global.ApplicationName = "DbComparisonV2";
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            string message = "";
            if (ex != null)
                message = ex.Message;
            ShowMessage(message, ex);
        }
        private void ShowMessage(string message, Exception ex)
        {
            string errorMessage = string.Format("Eine unbehandelte Ausnahme ist aufgetreten: {0}", Environment.NewLine + message);
            //LogManager.Error(message, ex);
            log.Error(message, ex);
            MessageBox.Show(errorMessage, "", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            //string errorMessage = string.Format("Eine unbehandelte Ausnahme ist aufgetreten: {0}", Environment.NewLine + e.Exception.Message);
            //MessageBox.Show(errorMessage, "", MessageBoxButton.OK, MessageBoxImage.Error);
            ShowMessage(e.Exception.Message, e.Exception);
            if (!(e.Exception is OutOfMemoryException))
                e.Handled = true;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(AppDispatcherUnhandledException);

        }

        void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowMessage(e.Exception.Message, e.Exception);
            if (!(e.Exception is OutOfMemoryException))
                e.Handled = true;
        }
    }
}
