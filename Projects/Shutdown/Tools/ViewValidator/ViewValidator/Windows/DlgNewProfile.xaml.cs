using System;
using System.Windows;
using System.Windows.Input;
using ViewValidatorLogic.Config;
using ViewValidatorLogic.Manager;

namespace ViewValidator.Windows {
    /// <summary>
    /// Interaktionslogik für DlgNewProfile.xaml
    /// </summary>
    public partial class DlgNewProfile : Window {
        ProfileConfig Profile { get; set; }

        public DlgNewProfile(ProfileConfig profile) {
            InitializeComponent();

            this.Profile = profile;
            txtProfileName.Focus();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            try {
                if (ProfileManager.ProfileExists(txtProfileName.Text)) throw new ArgumentException("Das Profil existiert bereits");
                Profile.Name = txtProfileName.Text;
                DialogResult = true;
                this.Close();
            } catch (ArgumentException ex) {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            this.Close();
        }

        private void txtProfileName_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter || e.Key == Key.Return)
                btnOk_Click(null, null);
        }

    }
}
