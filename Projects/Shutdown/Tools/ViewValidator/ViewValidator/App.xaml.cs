using System;
using System.Windows;
using System.Windows.Threading;
using AvdCommon;

namespace ViewValidator
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public App() {
            Global.ApplicationName = "ViewValidator";
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            Exception ex = e.ExceptionObject as Exception;
            string message = "";
            if (ex != null)
                message = ex.Message;
            ShowMessage(message);
        }

        private void ShowMessage(string message) {
            string errorMessage = string.Format("Eine unbehandelte Ausnahme ist aufgetreten: {0}", message);
            MessageBox.Show(errorMessage, "", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            string errorMessage = string.Format("Eine unbehandelte Ausnahme ist aufgetreten: {0}", e.Exception.Message);
            MessageBox.Show(errorMessage, "", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void Application_Startup(object sender, StartupEventArgs e) {
            Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(AppDispatcherUnhandledException);
        }

        void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            ShowMessage(e.Exception.Message);
            e.Handled = true;
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            ShowMessage(e.Exception.Message);
            e.Handled = true;
        }
    }
}
