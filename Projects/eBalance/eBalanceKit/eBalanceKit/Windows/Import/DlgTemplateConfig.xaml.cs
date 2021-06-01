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

namespace eBalanceKit.Windows.Import {
    /// <summary>
    /// Interaktionslogik für DlgTemplateConfig.xaml
    /// </summary>
    public partial class DlgTemplateConfig : Window {

        public DlgTemplateConfig() {
            InitializeComponent();
        }

        public DlgTemplateConfig(eBalanceKitBusiness.HyperCubes.Interfaces.Structure.IHyperCube cube) {
            InitializeComponent();
            // TODO: Complete member initialization
            DataContext = new ImportModel(cube, tabControl1, ctlAssignmentGrid1.dataGrid1, ctlAssignmentGrid2.dataGrid1, ctlAssignmentSummaryGrid.ctlAssignmentSummary.dataGrid1);
        }
        
        internal ImportModel Model { get { return DataContext as ImportModel; } }

        private void tabControl1_Ok(object sender, RoutedEventArgs e) {
            Model.SaveTemplate();
            this.Close();
        }
        
        private void tabControl1_Next(object sender, RoutedEventArgs e) {
            Model.NextPage();
        }

        private void BtSaveAndImport_OnClick(object sender, RoutedEventArgs e) {
            Model.SaveTemplate();
            new DlgImportHyperCube(Model).ShowDialog();
            this.Close();
        }

        private void btSave_Click(object sender, RoutedEventArgs e) {
            Model.SaveTemplate();
            this.Close();
        }

        private void tabControl1_Back(object sender, RoutedEventArgs e) {
            Model.PreviousPage();
        }
    }
}
