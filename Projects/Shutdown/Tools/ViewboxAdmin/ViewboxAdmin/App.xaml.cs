using System;
using System.Windows;
using System.Windows.Threading;
using AV.Log;
using AvdCommon;
using ViewboxAdmin.Models;
using ViewboxAdmin.Windows;
using Autofac;
using log4net;


namespace ViewboxAdmin {
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application {
        public App() {
            Global.ApplicationName = "ViewboxAdmin";
            _log.Info(string.Format("Application started {0}", Global.ApplicationName));
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            try {
                IoCContainerBuilder inversionOfControlContainerBuilder = new IoCContainerBuilder();
                var container = inversionOfControlContainerBuilder.GetDependencyInjectionContainer();
                var MainWindow = container.Resolve<DlgMainWindow>();
                MainWindow.DataContext = container.Resolve<MainWindow_ViewModel>(new NamedParameter("container", container));
                MainWindow.Show();
            }
            catch(Exception ex) {
                string message = "Cannot create the main window: " + ex.Message;
                _log.Error(message, ex);
                MessageBox.Show(message);
            }
        }

        internal ILog _log = LogHelper.GetLogger();

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            Exception ex = e.ExceptionObject as Exception;
            string message = String.Empty;
            if (ex != null) {
                message = "currentdomain unhandle" + ex.Message + ", " + ex.StackTrace;
                _log.Error(message, ex);
            }
            ShowMessage(message);
        }

        private void ShowMessage(string message) {
            string errorMessage = string.Format("Unhandled exception: {0}", message);
            MessageBox.Show(errorMessage, "", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            string message = string.Format("Unhandled exception: {0}, {1}", e.Exception.Message, e.Exception.StackTrace);
            _log.Error(message, e.Exception);
            MessageBox.Show(message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void Application_Exit(object sender, ExitEventArgs e) {
            _log.Info("Application exit");
        }

       

        
    }
}
