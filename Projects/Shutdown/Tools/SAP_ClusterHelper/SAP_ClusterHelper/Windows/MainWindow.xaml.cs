using System.Windows;

namespace SAP_ClusterHelper.Windows {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void BtnGenerateSourceDataClick(object sender, RoutedEventArgs e) { new DlgGenerateSourceData {Owner = this}.ShowDialog(); }
    }
}
