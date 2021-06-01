using System.Windows;
using Base.Localisation;
using System;
using System.Windows.Threading;
using ViewAssistantBusiness.Config;

namespace ViewAssistant
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            ConfigDb.Init();
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(string.Format(ExceptionMessages.UnhandledException, e.Exception.Message), "", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
