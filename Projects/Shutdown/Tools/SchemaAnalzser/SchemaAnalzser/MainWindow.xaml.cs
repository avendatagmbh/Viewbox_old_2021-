// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-01-07
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Threading;
using System.Windows;

namespace SchemaAnalzser {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        public MainWindow() { InitializeComponent(); }

        private void BtnAnalyseSchemaClick(object sender, RoutedEventArgs e) {
            var analyzer = new SchemaAnalyzerLib.SchemaAnalyzer("localhost", "root", "avendata", "dbsearch_generated_data_test");
            var progress = new Progress {Owner = this, DataContext = analyzer.Progress};
            new Thread(analyzer.Start).Start();
            progress.ShowDialog();
        }
    }
}