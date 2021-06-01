using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace eBalanceKit.Windows.TaxonomyUpgrade {
    /// <summary>
    /// Interaktionslogik für DlgTaxonomyUpgradeInfo.xaml
    /// </summary>
    public partial class DlgTaxonomyUpgradeInfo : Window {
        public DlgTaxonomyUpgradeInfo() {
            InitializeComponent();
        }

        private void PdfInfoClick(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start(@"Documents\Hinweis zum Aenderungsnachweis Taxonomie 5.0 zu 5.1.pdf");
        }


        private void WebInfoClick(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start(@"http://www.esteuer.de/");
        }
    }
}
