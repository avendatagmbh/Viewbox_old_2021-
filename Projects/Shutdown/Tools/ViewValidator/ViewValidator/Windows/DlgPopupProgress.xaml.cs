using System.Windows;

namespace ViewValidator.Windows {
    /// <summary>
    /// Interaktionslogik für DlgPopupProgress.xaml
    /// </summary>
    public partial class DlgPopupProgress : Window {
        public DlgPopupProgress() {
            InitializeComponent();
        }

        private void btnAbort_Click(object sender, RoutedEventArgs e) {
            if (MessageBox.Show(this, "Möchten Sie wirklich abbrechen?", "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes) {
                this.DialogResult = false;
                Close();
            }
        }
    }
}
