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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace eBalanceKit.Windows.Import {
    /// <summary>
    /// Interaktionslogik für CtlCsvConfigLimiterCharset.xaml
    /// </summary>
    public partial class CtlCsvConfigLimiterCharset : UserControl {
        public CtlCsvConfigLimiterCharset() {
            InitializeComponent();
        }


        internal eBalanceKitBusiness.HyperCubes.Import.Importer Model { get { return DataContext as eBalanceKitBusiness.HyperCubes.Import.Importer; } }

        private void CheckBox_Click(object sender, RoutedEventArgs e) { Model.RemoveAllAssignments(); }


    }
}
