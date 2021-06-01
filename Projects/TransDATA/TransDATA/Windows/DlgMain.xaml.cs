// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-08-25
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using Business;
using Config;
using Config.Interfaces.DbStructure;
using TransDATA.Models;

namespace TransDATA.Windows {
    /// <summary>
    /// Interaktionslogik für DlgMain.xaml
    /// </summary>
    public partial class DlgMain : Window {
        public DlgMain(IUser user) {
            InitializeComponent();
            Model = new MainWindowModel(this, user);
            DataContext = Model;
        }

        private MainWindowModel Model { get; set; }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            AppController.ProfileManager.LastProfile = Model.SelectedProfile;
            ConfigDb.Cleanup();
        }
    }
}