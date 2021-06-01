// -----------------------------------------------------------
// Created by Benjamin Held - 07.09.2011 13:43:51
// Copyright AvenDATA 2011
// -----------------------------------------------------------

using System;
using System.Windows;
using ScreenshotAnalyzer.Models;
using ScreenshotAnalyzerBusiness.Manager;
using Utils;

namespace ScreenshotAnalyzer.Windows {
    /// <summary>
    /// Interaktionslogik für DlgMainWindow.xaml
    /// </summary>
    public partial class DlgMainWindow : Window {
        public DlgMainWindow() {
            InitializeComponent();

            Model = new MainWindowModel(this);
            ctlScreenshotList.DataContext = Model.ScreenshotListModel;
            DataContext = Model;
        }

        public MainWindowModel Model { get; set; }

        private void btnExit_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (Model.SelectedProfile != null) {
                ApplicationManager.ApplicationConfig.LastProfile = Model.SelectedProfile.Name;
            }

            ApplicationManager.Save();
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e) {
            dummyButtonControl.Focus();
            DlgInfo dlgInfo = new DlgInfo();
            dlgInfo.ShowDialog();
        }

        private void btnExtractTextForAll_Click(object sender, RoutedEventArgs e) {
            dummyButtonControl.Focus();
            try {
                Model.SelectOcrAreasModel.ExtractTextForAll();
            } catch (Exception ex) {
                MessageBox.Show(this,
                                "Es ist ein Fehler aufgetreten:" + Environment.NewLine + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnExtractText_Click(object sender, RoutedEventArgs e) {
            dummyButtonControl.Focus();
            try {
                Model.SelectOcrAreasModel.ExtractText();
            } catch (Exception ex) {
                MessageBox.Show(this,
                                "Es ist ein Fehler aufgetreten:" + Environment.NewLine + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnUseSelectionForAll_Click(object sender, RoutedEventArgs e) {
            dummyButtonControl.Focus();
            try {
                Model.SelectOcrAreasModel.UseSelectionForAll();
                MessageBox.Show(this, "Rechtecke erfolgreich übertragen", "", MessageBoxButton.OK, MessageBoxImage.Information);
            } catch (Exception ex) {
                MessageBox.Show(this,
                                "Es ist ein Fehler aufgetreten:" + Environment.NewLine + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnUseExportAccess_Click(object sender, RoutedEventArgs e) {
            dummyButtonControl.Focus();
            try {
                Model.ExportAccessDatabase();
                MessageBox.Show(this, "Export erfolgreich durchgeführt", "", MessageBoxButton.OK, MessageBoxImage.Information);
            } catch (Exception ex) {
                MessageBox.Show(this,
                                "Es ist ein Fehler beim Exportieren aufgetreten:" + Environment.NewLine + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //private void btnUpdateDatabase_Click(object sender, RoutedEventArgs e) {
        //    try {
        //        var profile = Model.SelectedProfile;
        //        Model.SelectedProfile.UpdateDatabase();
        //        Model.SelectedProfile = null;
        //        Model.SelectedProfile = profile;
        //    } catch (Exception ex) {
        //        MessageBox.Show(this,
        //                        "Es gab einen Fehler beim Update der Datenbank: " + Environment.NewLine + ex.Message, "",
        //                        MessageBoxButton.OK, MessageBoxImage.Error);
        //        return;
        //    }
        //    MessageBox.Show(this,"Update war erfolgreich", "", MessageBoxButton.OK, MessageBoxImage.Information);
        //}

    }
}
