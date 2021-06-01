using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using ViewAssistantBusiness.Models;

namespace ViewAssistant.Windows
{
    public enum AccessOperationType
    {
        Merge,
        Copy,
        Linking
    }
    /// <summary>
    /// Interaction logic for DlgAccessOperations.xaml
    /// </summary>
    public partial class DlgAccessOperations
    {
        public AccessMergerModel Model
        {
            get { return DataContext as AccessMergerModel; }
        }

        public AccessOperationType AccessOperationType { get; set; }

        public DlgAccessOperations(AccessOperationType accessOperationType)
        {
            InitializeComponent();

            AccessOperationType = accessOperationType;
            InitWindow();
        }

        private void InitWindow()
        {
            btnClose.Visibility = Visibility.Visible;
            ProgressbarField.Visibility = Visibility.Collapsed;
            IsStarted = false; 
            switch (AccessOperationType)
            {
                case AccessOperationType.Merge:
                    btnMergeFile.Visibility = Visibility.Visible;
                    OutputDirectoryField.Visibility = Visibility.Collapsed;
                    OutputFileLabelField.Visibility = Visibility.Collapsed;
                    break;
                case AccessOperationType.Copy:
                    btCopyAccessFiles.Visibility = Visibility.Visible;
                    OutputFileField.Visibility = Visibility.Collapsed;
                    break;
                case AccessOperationType.Linking:
                    btnLinkingFile.Visibility = Visibility.Visible;
                    OutputDirectoryField.Visibility = Visibility.Collapsed;
                    OutputFileLabelField.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void WindowPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        public String Content
        {
            get { return Title; }
            set
            {
                Title = value;
                TitleMessage.Content = value;
            }
        }

        private void BtnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private Boolean GetDirectory(out String result)
        {
            var dialog = new FolderBrowserDialog();
            var dr = dialog.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                result = dialog.SelectedPath;
                return true;
            }
            result = null;
            return false;
        }

        private void SetSourceDirectory(object sender, RoutedEventArgs e)
        {
            String result;
            if (GetDirectory(out result))
            {
                Model.InputDirectory = result;
            }
        }

        private void SetOutputDirectory(object sender, RoutedEventArgs e)
        {
            String result;
            if (GetDirectory(out result))
            {
                Model.OutputDirectory = result;
            }
        }

        private void SetOutputFileName(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "Microsoft Access|*.accdb;*.mdb";
            dialog.DefaultExt = "accdb";

            var result = dialog.ShowDialog();

            if (result.Value)
            {
                Model.OutputFileName = dialog.FileName;
            }
        }

        private void WindowDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var dataContext = e.NewValue as AccessMergerModel;
            if (dataContext != null)
            {
                dataContext.StartUp += StartUp;
                dataContext.InfoShownEvent += ShowInfo;
                dataContext.ErrorShownEvent += ShowError;
            }
        }

        private void StartUp(object sender, EventArgs e)
        {
            if (Dispatcher.CheckAccess())
            {
                IsStarted = true;
                SourceDirectoryField.IsEnabled = false;
                OutputDirectoryField.IsEnabled = false;
                OutputFileField.IsEnabled = false;
                ProgressbarField.Visibility = Visibility.Visible;
                btnMergeFile.Visibility = Visibility.Collapsed;
                btCopyAccessFiles.Visibility = Visibility.Collapsed;
                btnLinkingFile.Visibility = Visibility.Collapsed;
                btnLinkingFile.Visibility = Visibility.Collapsed;
                btnClose.Visibility = Visibility.Collapsed;
            }
            else
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => StartUp(sender, e)));
            }
        }

        private void ShowError(string message)
        {
            if (Dispatcher.CheckAccess())
            {
                System.Windows.MessageBox.Show(message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                InitWindow();
            }
            else
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => ShowError(message)));
            }
        }

        private void ShowInfo(string message)
        {
            if (Dispatcher.CheckAccess())
            {
                System.Windows.MessageBox.Show(message, "", MessageBoxButton.OK, MessageBoxImage.Information);
                InitWindow();
                Close();
            }
            else
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => ShowInfo(message)));
            }
        }

        public bool IsStarted { get; set; }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = IsStarted;
        }
    }
}
