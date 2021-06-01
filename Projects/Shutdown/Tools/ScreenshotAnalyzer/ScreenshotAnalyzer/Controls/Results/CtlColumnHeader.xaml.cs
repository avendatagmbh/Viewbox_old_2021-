using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ScreenshotAnalyzer.Models.Results;

namespace ScreenshotAnalyzer.Controls.Results {
    /// <summary>
    /// Interaktionslogik für CtlColumnHeader.xaml
    /// </summary>
    public partial class CtlColumnHeader : UserControl {
        public CtlColumnHeader() {
            InitializeComponent();
        }

        ColumnHeaderModel Model { get { return DataContext as ColumnHeaderModel; } }

        private void btnEditHeader_Click(object sender, RoutedEventArgs e) {
            Model.EditHeader(true);
            txtHeader.Focus();
            txtHeader.SelectAll();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Return) {
                Model.EditHeader(false);
            }
        }

        private void txtHeader_LostFocus(object sender, RoutedEventArgs e) {
            Model.EditHeader(false);
        }

    }
}
