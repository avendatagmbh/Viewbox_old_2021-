using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Microsoft.Win32;
using eBalanceKit.Models.Import;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;

namespace eBalanceKit.Windows.Import {
    /// <summary>
    /// Interaktionslogik für DlgImport.xaml
    /// </summary>
    public partial class DlgImport : Window {
        public DlgImport(IHyperCube cube) {
            InitializeComponent();

            DataContext = new ImportModel(cube, tabControl1, ctlAssignmentGrid1.dataGrid1, ctlAssignmentGrid2.dataGrid1, ctlAssignmentSummaryGrid.ctlAssignmentSummary.dataGrid1); 
            
        }

        internal ImportModel Model { get { return DataContext as ImportModel; } }

        
        private void btNext_Click(object sender, RoutedEventArgs e) {
            //tabControl1.NavigateNext();
            Model.NextPage();
        }

        private void btBack_Click(object sender, RoutedEventArgs e) {
            //tabControl1.NavigateBack();
            Model.PreviousPage();
        }

        private void OkClick(object sender, RoutedEventArgs e) { }

    }
}
