using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ScreenshotAnalyzer.Models.ProfileRelated;
using Utils;

namespace ScreenshotAnalyzer.Controls.ProfileRelated {

    /// <summary>
    /// Interaktionslogik für CtlProfileDetails.xaml
    /// </summary>
    public partial class CtlProfileDetails : UserControl {
        ProfileModel Model { get { return DataContext as ProfileModel; } }

        public CtlProfileDetails() {
            InitializeComponent();

            
        }

        private void UserControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e) {
            if (Model != null) {
                //ctlMysqlDatabase.DataContext = new DatabaseModel(Model.Profile.DbConfigView) { IsReadOnly = true };    
            }
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e) {
            SaveFileDialog dlg = new SaveFileDialog() {
                DefaultExt = ".mdb",
                Filter = "Access Datenbank|*.mdb"
            };
            bool? result = dlg.ShowDialog();
            if (result.Value) {
                Model.Profile.AccessPath = dlg.FileName;
            }

        }
    }
}
