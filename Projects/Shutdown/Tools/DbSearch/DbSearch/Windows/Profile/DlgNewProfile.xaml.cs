// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-08
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using DbSearch.Models.Profile;
using DbSearchBase.Interfaces;


namespace DbSearch.Windows.Profile {
    /// <summary>
    /// Interaktionslogik für DlgNewProfile.xaml
    /// </summary>
    public partial class DlgNewProfile : Window {
        public DlgNewProfile(DbSearchLogic.Config.Profile profile) {
            InitializeComponent();
            Profile = profile;
            DataContext = profile;

            ctlMysqlDatabase.DataContext = new DatabaseModel(profile.DbConfigView);
        }

        public DbSearchLogic.Config.Profile Profile { get; set; }

        //private void btnSelectDir_Click(object sender, RoutedEventArgs e) {
        //    var dialog = new System.Windows.Forms.FolderBrowserDialog();
        //    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
        //    if (result == System.Windows.Forms.DialogResult.OK) {
        //        txtPath.Text = dialog.SelectedPath;
        //    }
        //}

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            //Profile.Path = txtPath.Text;
            if (string.IsNullOrEmpty(txtName.Text)) {
                MessageBox.Show("Bitte einen Profilnamen eingeben");
                return;
            }
            Profile.Name = txtName.Text;
            DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            this.Close();
        }
    }
}
