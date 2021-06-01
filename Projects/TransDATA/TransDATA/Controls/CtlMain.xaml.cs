// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-08-25
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using Base.Localisation;
using Logging;
using TransDATA.Models;
using TransDATA.Windows;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;
using Config;
using System.IO;

namespace TransDATA.Controls {
    /// <summary>
    /// Interaktionslogik für CtlMain.xaml
    /// </summary>
    public partial class CtlMain : UserControl {
        public CtlMain() {
            InitializeComponent();
        }

        private Window Owner {
            get { return UIHelpers.TryFindParent<Window>(this); }
        }

        private MainWindowModel Model { get { return DataContext as MainWindowModel; } }

        private void BtnInfoClick(object sender, RoutedEventArgs e) {
            FocusDummyButton.Focus();
            new DlgInfo {Owner = Owner}.ShowDialog();
        }

        private void BtnDisconnenctClick(object sender, RoutedEventArgs e) {
            new DlgLogin().Show();
            Owner.Close();
        }

        private void BtnProfilesClick(object sender, RoutedEventArgs e) {
            FocusDummyButton.Focus();
            Model.ShowProfileManagement();
            
        }

        private void BtnExportClick(object sender, RoutedEventArgs e) {
            FocusDummyButton.Focus();
            new DlgExport(Model.SelectedProfile) { Owner = Owner }.ShowDialog();
        }

        private void BtnTransferClick(object sender, RoutedEventArgs e) {
            FocusDummyButton.Focus();
            Model.StartTransfer();
        }

        private void BtnDocumentationClick(object sender, RoutedEventArgs e) {
            FocusDummyButton.Focus();
            Model.CreateDocumentation();
        }

        private void BtnEditProfileClick(object sender, RoutedEventArgs e) {
            FocusDummyButton.Focus();
            Model.EditProfile();
        }

        private void BtnReadDatabaseStructureClick(object sender, RoutedEventArgs e) {
            FocusDummyButton.Focus();
            Model.ReadDatabaseStructure();
        }

        private void btnMail_Click(object sender, RoutedEventArgs e) {
            FocusDummyButton.Focus();
            Model.EditMailSettings();
        }

        private void btnEditCurrentUser_Click(object sender, RoutedEventArgs e) {
            FocusDummyButton.Focus();
            Model.EditUserSettings();
        }

        private void btnOpenLogFolder_Click(object sender, RoutedEventArgs e) {
            FocusDummyButton.Focus();
            Model.OpenLogDirectory();
        }
    }
}