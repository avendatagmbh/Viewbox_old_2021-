using System.IO;
using DbAccess.Structures;
using System.Windows;
using Base.Localisation;
using Microsoft.Win32;
using System;
using System.Windows.Controls;

namespace ViewAssistant.Controls
{
    /// <summary>
    /// Interaction logic for CtlDatabaseConfig.xaml
    /// </summary>
    public partial class CtlDatabaseConfig
    {
        public CtlDatabaseConfig()
        {
            InitializeComponent();
        }

        private void TestButtonClick(object sender, RoutedEventArgs e)
        {
            var dc = DataContext as DbConfig;
            if (dc != null)
            {
                try
                {
                    dc.TestConnection();
                    MessageBox.Show(ResourcesCommon.DatabaseConnectionOk, "", MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ResourcesCommon.DatabaseConnectionFailed, "", MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    // TODO log
                }
            }
        }

        private DbConfig Model
        {
            get { return DataContext as DbConfig; }
        }

        private void DataTypeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InitFields();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            InitFields();
        }

        private void InitFields()
        {
            if((String)DataTypeList.SelectedValue == "Access")
            {
                BrowseButton.Visibility = Visibility.Visible;
                UsernameField.Visibility = Visibility.Collapsed;
                PasswordField.Visibility = Visibility.Collapsed;
                DatabaseField.Visibility = Visibility.Collapsed;
                PortField.Visibility = Visibility.Collapsed;
                return;
            }
            BrowseButton.Visibility = Visibility.Collapsed;
            UsernameField.Visibility = Visibility.Visible;
            PasswordField.Visibility = Visibility.Visible;
            DatabaseField.Visibility = Visibility.Visible;
            PortField.Visibility = Visibility.Visible;
        }

        private void BrowseButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            var result = dialog.ShowDialog();

            if (result.Value)
            {
                Model.Hostname = dialog.FileName;
            }
        }
    }
}
