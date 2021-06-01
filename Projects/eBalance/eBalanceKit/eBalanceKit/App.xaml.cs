using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace eBalanceKit {
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application {

        public App() {            
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            Exception ex = e.ExceptionObject as Exception;
            string message = "";
            if (ex != null)
                message = ex.Message;
            eBalanceKitBase.Structures.ExceptionLogging.LogException(ex);
            ShowMessage(message);
        }

        private void ShowMessage(string message) {
            string errorMessage = string.Format("Eine unbehandelte Ausnahme ist aufgetreten: {0}", message);
            MessageBox.Show(errorMessage, "", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            string errorMessage = string.Format("Eine unbehandelte Ausnahme ist aufgetreten: {0}", e.Exception.Message); 
            eBalanceKitBase.Structures.ExceptionLogging.LogException(e.Exception);
            MessageBox.Show(errorMessage, "", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void Application_Startup(object sender, StartupEventArgs e) {
            Application.Current.DispatcherUnhandledException += AppDispatcherUnhandledException;
        }

        void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            if (!e.Handled) {
                eBalanceKitBase.Structures.ExceptionLogging.LogException(e.Exception);
                ShowMessage(e.Exception.Message);
                e.Handled = true;
            } 
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            ShowMessage(e.Exception.Message); 
            e.Handled = true;
        }
    }
}
