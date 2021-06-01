// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-08
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System.Windows;
using System.Windows.Input;
using ScreenshotAnalyzerBusiness.Structures.Config;

namespace ScreenshotAnalyzer.Windows {
    /// <summary>
    /// Interaktionslogik für DlgNewProfile.xaml
    /// </summary>
    public partial class DlgNewProfile : Window {
        public DlgNewProfile() {
            InitializeComponent();
            DataContext = this;
            txtName.Focus();
        }

        //public Profile Profile { get; set; }
        public string ProfileName { get; set; }
        //private void btnSelectDir_Click(object sender, RoutedEventArgs e) {
        //    var dialog = new System.Windows.Forms.FolderBrowserDialog();
        //    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
        //    if (result == System.Windows.Forms.DialogResult.OK) {
        //        txtPath.Text = dialog.SelectedPath;
        //    }
        //}

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            Finish();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            this.Close();
        }

        private void txtName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Key == Key.Return)
                Finish();
        }

        private void Finish() {
            if (string.IsNullOrEmpty(txtName.Text)) {
                MessageBox.Show("Bitte einen Profilnamen eingeben");
                return;
            }
            DialogResult = true;
            this.Close();
        }
    }
}
