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
using AvdWpfControls;
using eBalanceKitBusiness.HyperCubes.Import;

namespace eBalanceKit.Windows.Import {
    /// <summary>
    /// Interaktionslogik für CtlTemplateSelection.xaml
    /// </summary>
    public partial class CtlTemplateSelection : UserControl {
        public CtlTemplateSelection() {
            InitializeComponent();
        }
        
        internal Importer Model { get { return DataContext as Importer; } }

        private void btnAddTemplate_Click(object sender, RoutedEventArgs e) {
            Model.CreateNewTemplate();
            var ctrTabPanel= UIHelpers.TryFindParent<AssistantControlTabPanel>(this);

            if (ctrTabPanel == null) return;
            ctrTabPanel.NavigateNext();
        }

        private void btnEditTemplate_Click(object sender, RoutedEventArgs e) {
            Model.ModifyTemplate(Model.Templates.SelectedTemplate);
            var dlgImport = UIHelpers.TryFindParent<DlgImport>(this);
            if (dlgImport == null) return;
            dlgImport.tabControl1.NavigateNext();
        }

        private void btnDeleteTemplate_Click(object sender, RoutedEventArgs e) {
            Model.Templates.DeleteSelectedTemplate();
        }
    }
}
