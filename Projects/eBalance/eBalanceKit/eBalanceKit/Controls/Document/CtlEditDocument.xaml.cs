// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-01-13
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Taxonomy;
using eBalanceKit.Models.Document;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.Manager;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Controls.Document {
    /// <summary>
    /// Interaktionslogik für CtlEditDocument.xaml
    /// </summary>
    public partial class CtlEditDocument : UserControl {
        private IElement CbSpecialAccountingStandardLastValue;

        public CtlEditDocument() {
            InitializeComponent();
            DataContextChanged += CtlEditDocument_DataContextChanged;
        }

        void CtlEditDocument_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) { CbSpecialAccountingStandardLastValue = null;
            // fix the problem with visible error message because visibility is not set per binding
            Validate();
        }

        #region properties
        /// <summary>
        /// Gets the document.
        /// </summary>
        /// <value>The document.</value>
        private eBalanceKitBusiness.Structures.DbMapping.Document Document {
            get {
                //if (this.DataContext is eBalanceKitBusiness.Structures.DbMapping.Document) {
                //    return this.DataContext as eBalanceKitBusiness.Structures.DbMapping.Document;
                //} else {
                //    return null;
                //}
                return Model.Document;
            }
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>The model.</value>
        private EditDocumentModel Model { get { return DataContext as EditDocumentModel; } }
        #endregion properties

        #region methods
        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <returns></returns>
        public bool Validate() {
            bool result = true;

            // validate name
            txtNameWarning.Visibility = Visibility.Collapsed;
            txtNameWarning1.Visibility = Visibility.Collapsed;

            if (string.IsNullOrEmpty(txtName.Text)) {
                txtNameWarning.Visibility = Visibility.Visible;
                result = false;
            } else {
                if (Model.SelectedSystem != null &&
                    Model.SelectedCompany != null &&
                    Model.SelectedFinancialYear != null) {
                    // check if the name already exists for the selected system/company/financial year combination
                    bool nameExists = DocumentManager.Instance.Exists(Model.SelectedSystem, Model.SelectedCompany,
                                                             Model.SelectedFinancialYear, txtName.Text);
                    bool newOrChangedName = !Model.Document.Name.ToLower().Equals(txtName.Text.ToLower());
                    if (newOrChangedName && nameExists) {
                        //txtNameWarning1.Visibility = Visibility.Visible;
                        result = false;
                    }
                }
            }

            // validate system
            if (Model.SelectedSystem == null) {
                cboSystemWarning.Visibility = Visibility.Visible;
                result = false;
            } else {
                cboSystemWarning.Visibility = Visibility.Collapsed;
            }

            // validate company
            if (Model.SelectedCompany == null) {
                cboCompanyWarning.Visibility = Visibility.Visible;
                result = false;
            } else {
                cboCompanyWarning.Visibility = Visibility.Collapsed;
            }

            // validate financial year
            if (Model.SelectedFinancialYear == null) {
                cboPeriodWarning.Visibility = Visibility.Visible;
                result = false;
            } else {
                cboPeriodWarning.Visibility = Visibility.Collapsed;
            }

            return result;
        }

        /// <summary>
        /// Updates the sources.
        /// </summary>
        public bool UpdateSources(bool select) {
            //txtName.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            //txtComment.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            if (Validate()) {
                if (txtName.Text != Model.Document.Name || txtComment.Text != Model.Document.Comment ||
                    Model.SelectedSystem != Model.Document.System ||
                    Model.SelectedCompany != Model.Document.Company ||
                    Model.SelectedFinancialYear != Model.Document.FinancialYear) {
                    txtName.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                    txtComment.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                    //this.Model.Document.Name = txtName.Text;
                    //this.Model.Document.Comment = txtComment.Text;
                    Model.Document.System = Model.SelectedSystem;
                    Model.Document.Company = Model.SelectedCompany;
                    Model.Document.FinancialYear = Model.SelectedFinancialYear;
                    DocumentManager.Instance.SaveDocument(Model.Document);
                }
                return true;
            }
            return false;
        }
        #endregion methods

        private void financialYear_changed(object sender, SelectionChangedEventArgs e) { UpdateSources(true); }

        private void company_changed(object sender, SelectionChangedEventArgs e) { UpdateSources(true); }

        private void System_changed(object sender, SelectionChangedEventArgs e) { UpdateSources(true); }

        private void txtName_LostFocus(object sender, RoutedEventArgs e) { UpdateSources(false); }

        private void txtComment_LostFocus(object sender, RoutedEventArgs e) { UpdateSources(false); }

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

            var oldTaxonomyInfo = Model.Document.MainTaxonomyInfo;

            var document = Model.Document;
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

        private void txtName_TextChanged(object sender, TextChangedEventArgs e) { Validate(); }

        private void txtComment_TextChanged(object sender, TextChangedEventArgs e) { Validate(); }

    }
}