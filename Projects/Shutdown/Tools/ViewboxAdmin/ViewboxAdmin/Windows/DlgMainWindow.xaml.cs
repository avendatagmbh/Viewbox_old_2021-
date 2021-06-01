using System;
using System.Windows;
using AV.Log;
using ViewboxAdmin.Models;
using log4net;

namespace ViewboxAdmin.Windows {
    
    public partial class DlgMainWindow : Window {

        public DlgMainWindow() {
            InitializeComponent();
        }

        private void OnExceptionHandler(object sender, System.IO.ErrorEventArgs ex) {
            string message = "Exception has been caught in mainwindow: " + ex.GetException().Message;
            _log.Error(message,ex.GetException());
            MessageBox.Show(message,"Error",MessageBoxButton.OK,MessageBoxImage.Error);
        }


        private ILog _log = LogHelper.GetLogger();
       
        
        public MainWindow_ViewModel viewModel {get { return DataContext as MainWindow_ViewModel; }}
       
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            try {
                viewModel.SaveApplicationAndProfile();
            }
            catch(Exception ex) {
                string message = "Cannot save profile and application settings: " + ex.Message;
                _log.Error(message,ex);
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void btnUpgradeDatabase_Click(object sender, RoutedEventArgs e) {
            try {
                viewModel.UpGradeDatabase();
            }
            catch(Exception ex) {
                string message = "Database upgrade failed: " + ex.Message;
                _log.Error(message, ex);
            }
        }

        private void Window_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            try {
                viewModel.ExceptionEvent += OnExceptionHandler;
            }
            catch(Exception ex) {
                string message = "Error during Datacontextchange..." + ex.Message;
                _log.Error(message, ex);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            try {
                viewModel.SetProfile();
            }
            catch(Exception ex) {
                string message = "Error during loading: " + ex.Message;
                _log.Error(message, ex);
            }
        }
    }
}
