// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-25
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Taxonomy;
using eBalanceKitBase.Windows;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Controls.Document {
    /// <summary>
    /// Interaktionslogik für CtlDocumentProperties.xaml
    /// </summary>
    public partial class CtlDocumentProperties : UserControl {
        private IElement CbSpecialAccountingStandardLastValue;

        public CtlDocumentProperties() {
            InitializeComponent();
            DataContextChanged += CtlEditDocument_DataContextChanged;
        }

        private void CtlEditDocument_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            cbSpecialAccountingStandard.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateSource();
        }

        private void ComboBoxSpecialAccountingStandardSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (cbSpecialAccountingStandard.SelectedIndex < 0) {
                CbSpecialAccountingStandardLastValue = null;
                return; // combo box resetted due to document change => ignore
            }

            // combo box initialized => ignore
            if (CbSpecialAccountingStandardLastValue == null) {
                CbSpecialAccountingStandardLastValue = cbSpecialAccountingStandard.SelectedItem as IElement;
                return;
            }

            var document = ((eBalanceKitBusiness.Structures.DbMapping.Document) DataContext);

            var oldTaxonomyInfo = document.MainTaxonomyInfo;
            string selectedAccountingStandard = ((IElement)cbSpecialAccountingStandard.SelectedItem).Name;
            
            if (oldTaxonomyInfo == eBalanceKitBusiness.Structures.DbMapping.Document.GetTaxonomyInfoFromBusinessSector(selectedAccountingStandard)) return;

            var owner = UIHelpers.TryFindParent<Window>(this) ?? GlobalResources.MainWindow;
            MessageBoxResult result = MessageBox.Show(
                owner,
                ResourcesCommon.SpecialAccountingStandardChanged,
                ResourcesCommon.SpecialAccountingStandardChangedHeadline,
                MessageBoxButton.YesNo, MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (result == MessageBoxResult.Yes) {
                // update taxonomy depending data (presentation and value trees)
                var progress = new DlgProgress(GlobalResources.MainWindow) { ProgressInfo = { IsIndeterminate = true, Caption = "Taxonomie wird geändert..." } };
                progress.ExecuteModal(
                    () => document.ChangeAssignedTaxonomy(oldTaxonomyInfo, selectedAccountingStandard, progress.ProgressInfo));

                cbSpecialAccountingStandard.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateSource();
            } else {
                cbSpecialAccountingStandard.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateTarget();
            }
        }
    }
}