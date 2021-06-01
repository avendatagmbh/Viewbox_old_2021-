using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AV.Log;
using AvdCommon;
using AvdCommon.Logging;
using ViewBuilderBusiness.Manager;
using ViewBuilderBusiness.Structures.Config;
using log4net;
using Utils;

namespace ViewBuilder
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        private static ILog log = LogHelper.GetLogger();

        private const string NETWORK_DRIVE_N_PATH = @"\\lenny\Netztausch";
        private const string NETWORK_DRIVE_Q_PATH = @"\\lenny\Projekte";

        /// <summary>
        /// Map the network drives: N(\\lenny\Netztausch),
        ///                         Q(\\lenny\Projekte).
        /// </summary>
        public static void MapNetworkDrives_N_Q()
        {
            try
            {
                NetworkDriveMapper.MapNetworkDrive("N", NETWORK_DRIVE_N_PATH);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("App.MapNetworkDrives_N_Q exception (N drive): {0}", ex);
            }

            try
            {
                NetworkDriveMapper.MapNetworkDrive("Q", NETWORK_DRIVE_Q_PATH);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("App.MapNetworkDrives_N_Q exception (Q drive): {0}", ex);
            }
        }

        public App()
        {
            LogHelper.PerformanceLogging = false;

            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Global.ApplicationName = "ViewBuilder";

            MapNetworkDrives_N_Q();
            ProfileConfig.GetProcessorCoreNumber();
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            string message = "";
            string stackTrace = "";
            if (ex != null)
            {
                message = ex.Message;
                stackTrace = ex.StackTrace;
                if (ex.InnerException != null)
                {
                    message += Environment.NewLine + ex.InnerException.Message;
                }
            }
            ShowMessage(message, stackTrace);
        }

        private void ShowMessage(string message, string stackTrace)
        {
            string errorMessage = string.Format("Eine unbehandelte Ausnahme ist aufgetreten: {0}, Stacktrace: {1}", message, stackTrace);
            log.Error(errorMessage);
            MessageBox.Show(errorMessage, "", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            //string errorMessage = string.Format("Eine unbehandelte Ausnahme ist aufgetreten: {0}, Stacktrace: {1}", e.Exception.Message, e.Exception.StackTrace);
            //MessageBox.Show(errorMessage, "", MessageBoxButton.OK, MessageBoxImage.Error);
            ShowMessage(e.Exception.Message + (e.Exception.InnerException == null ? "" : System.Environment.NewLine + e.Exception.InnerException.Message), e.Exception.StackTrace);
            e.Handled = true;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Application.Current.DispatcherUnhandledException += AppDispatcherUnhandledException;
        }

        void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowMessage(e.Exception.Message, e.Exception.StackTrace);
            e.Handled = true;
        }



    }

}
