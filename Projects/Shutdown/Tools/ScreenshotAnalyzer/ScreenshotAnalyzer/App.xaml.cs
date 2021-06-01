using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using AvdCommon;
using ScreenshotAnalyzer.Resources.Localisation;
using ScreenshotAnalyzerBusiness.Manager;

namespace ScreenshotAnalyzer {
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application {
        public App() {
            Global.ApplicationName = "ScreenshotAnalyzer";
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            Exception ex = e.ExceptionObject as Exception;
            string message = "";
            if (ex != null)
                message = ex.Message + Environment.NewLine + ex.StackTrace;
            ShowMessage(message);
        }

        private void ShowMessage(string message) {
            string errorMessage = string.Format("Eine unbehandelte Ausnahme ist aufgetreten: {0}", Environment.NewLine + message);
            MessageBox.Show(errorMessage, "", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            //string errorMessage = string.Format("Eine unbehandelte Ausnahme ist aufgetreten: {0}", Environment.NewLine + e.Exception.Message);
            //MessageBox.Show(errorMessage, "", MessageBoxButton.OK, MessageBoxImage.Error);
            ShowMessage(e.Exception.Message + Environment.NewLine + e.Exception.StackTrace);
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
            ShowMessage(e.Exception.Message + Environment.NewLine + e.Exception.StackTrace);
            e.Handled = true;
        }
    }
}
