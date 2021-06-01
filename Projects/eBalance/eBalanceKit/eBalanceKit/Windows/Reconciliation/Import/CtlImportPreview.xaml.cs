using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using eBalanceKitBase.Interfaces;
using eBalanceKitBusiness.Reconciliation.ViewModels;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Reconciliation.Import {
    /// <summary>
    /// Interaction logic for CtlImportPreview.xaml
    /// </summary>
    public partial class CtlImportPreview : UserControl {
        public CtlImportPreview() {
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs) {
            if (cboFiles.Items.Count > 0)
                cboFiles.SelectedIndex = 0;
            if (Model != null && Model.Preview != null) {
                Model.Preview.PreviewFile(cboFiles.SelectedItem.ToString());
                ctlPreview.DataContext = Model.Preview;
            }
        }

        IAssistedImport Model { get { return DataContext as IAssistedImport; } } 

        private void CboFiles_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count > 0)
                Model.Preview.PreviewFile(e.AddedItems[0].ToString());
            if (cboFiles.SelectedItem == null)
                lblCurrentFile.Content = ResourcesCommon.Filename + ": ";
            else
                lblCurrentFile.Content = ResourcesCommon.Filename + ": " + Model.Preview.CsvFiles.First(f => f.EndsWith(cboFiles.SelectedItem.ToString()));
            ctlPreview.DataContext = Model.Preview;
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e) {
            if (Model.Preview.Previous())
                cboFiles.SelectedIndex -= 1;
            ctlPreview.DataContext = Model.Preview;
        }

        private void btnNext_Click(object sender, RoutedEventArgs e) {
            if (Model.Preview.Next())
                cboFiles.SelectedIndex += 1;
            ctlPreview.DataContext = Model.Preview;
        }
    }
}
