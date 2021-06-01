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
using eBalanceKit.Models.Import;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;

namespace eBalanceKit.Windows.Import {
    /// <summary>
    /// Interaktionslogik für DlgImportHyperCube.xaml
    /// </summary>
    public partial class DlgImportHyperCube : Window {
        public DlgImportHyperCube(IHyperCube cube) {
            InitializeComponent();

            DataContext = new ImportModel(cube, tabControl1, ctlAssignmentSummaryGrid.ctlAssignmentSummary.dataGrid1); 
            
        }

        public DlgImportHyperCube(ImportModel model) {
            InitializeComponent();
            DataContext = model;
            Model.Import();
            tabControl1.SelectedIndex = tabControl1.Items.Count - 1;
        }


        internal ImportModel Model { get { return DataContext as ImportModel; } }

        private void OkClick(object sender, RoutedEventArgs e) {
            Model.Import();
            tabControl1.SelectedIndex = tabControl1.SelectedIndex + 1;
        }

        private void tabControl1_Next(object sender, RoutedEventArgs e) {
            Model.NextImportPage();
        }
    }
}
