using System.Windows;
using AvdWpfControls;
using eBalanceKit.Models;
using federalGazetteBusiness;
using eBalanceKitBusiness.Export;
using System;
using System.Text.RegularExpressions;
using System.IO;
using federalGazetteBusiness.Structures.Enum;

namespace eBalanceKit.Windows.FederalGazette {
    /// <summary>
    /// Interaktionslogik für DlgAssistant.xaml
    /// </summary>
    public partial class DlgAssistant {
        /// <summary>
        /// Initializes a new instance of the <see cref="DlgAssistant"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        public DlgAssistant(eBalanceKitBusiness.Structures.DbMapping.Document document) {

            _currentDocument = document;
            InitializeComponent();

            //new eBalanceKitBase.Windows.DlgProgress(GlobalResources.MainWindow) {
            //    ProgressInfo = {
            //        IsIndeterminate = true,
            //        Caption = eBalanceKitResources.Localisation.ResourcesFederalGazette.ProgressCaptionLoading
            //    }
            //}.ExecuteModal(() => _currentModel = new FederalGazetteModel(_currentDocument, this));
            _currentModel = new FederalGazetteModel(_currentDocument, this);
            //_currentModel = new FederalGazetteModel(_currentDocument);
            DataContext = _currentModel;
        }

        private eBalanceKitBusiness.Structures.DbMapping.Document _currentDocument;
        private FederalGazetteModel _currentModel;

        #region Events
        private void MyDlgAssistantLoaded(object sender, RoutedEventArgs e) {
            SetCurrentTab();
            _currentModel.RefreshPreview(GetEnumTag());
        }

        private void RbSmallChecked(object sender, RoutedEventArgs e) { SetAccountsByCompanySize(CompanySize.Small); }

        private void RbMediumChecked(object sender, RoutedEventArgs e) { SetAccountsByCompanySize(CompanySize.Midsize); }

        private void RbBigChecked(object sender, RoutedEventArgs e) { SetAccountsByCompanySize(CompanySize.Big); }

        private void PreviewExpanderExpanded(object sender, RoutedEventArgs e) { RightColumn.Width = GridLength.Auto; }

        private void PreviewExpanderCollapsed(object sender, RoutedEventArgs e) { RightColumn.Width = GridLength.Auto; }

        /// <summary>
        /// PDFs the export click.
        /// Currently its not a final version! In progress...
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void PdfExportClick(object sender, RoutedEventArgs e) {
            if (!String.IsNullOrEmpty(avdBrowser.GetHtmlText())) {
                string filename =
                    _currentDocument.Company.Name + "_"
                    + _currentDocument.FinancialYear.FYear + "_"
                    + eBalanceKitResources.Localisation.ResourcesFederalGazette.FederalGazette;
                string regex = "[" + String.Join(",", Path.GetInvalidFileNameChars()) + "]";
                filename = Regex.Replace(filename, @"" + regex + "", "_");

                HtmlToPdfExporter exp = new HtmlToPdfExporter();
                exp.ConvertHtmlToPdf(avdBrowser.GetHtmlText(), filename, false, 30f, 10f, 15f, 15f);
            }
        }

        private void ReportPartChanged(object sender, RoutedEventArgs e) { _currentModel.RefreshPreview(FederalGazetteAssistantTabs.ReportOptions); }

        private void OnTabIndexChanges(object sender, RoutedEventArgs e) { SetCurrentTab(); }
        #endregion

        #region Methodes
        /// <summary>
        /// Sets the current tab and if browser has no content and the current tab is Login then refreshing preview.
        /// </summary>
        private void SetCurrentTab() {
            _currentModel.FederalGazetteAssistantCurrentTab = GetEnumTag();
            if (!_currentModel.BrowserHasContent
                && _currentModel.FederalGazetteAssistantCurrentTab == FederalGazetteAssistantTabs.Login)
                _currentModel.RefreshPreview(GetEnumTag());
        }

        /// <summary>
        /// Sets the size of the accounts by company.
        /// </summary>
        /// <param name="pSize">Size of the p.</param>
        private void SetAccountsByCompanySize(CompanySize pSize) {
            if (_currentModel == null || _currentModel.TreeViewBalanceModel == null || _currentModel.TreeViewIncomeStatementModel == null) {
                return;
            }

            _currentModel.TreeViewBalanceModel.SetCheckedItemsByCompanySize(pSize, true, true);
            _currentModel.TreeViewIncomeStatementModel.SetCheckedItemsByCompanySize(pSize, true, false);

            SetPartOfReport();
            _currentModel.RefreshPreview(FederalGazetteAssistantTabs.CompanySize);
        }

        /// <summary>
        /// Gets the enum by the current TabItem's tag.
        /// </summary>
        /// <returns></returns>
        private FederalGazetteAssistantTabs GetEnumTag() {
            if (assistantControl.SelectedItem == null)
                return FederalGazetteAssistantTabs.NotDefined;

            AssistantTabItem selectedItem = (assistantControl.SelectedItem as AssistantTabItem);
            if (selectedItem == null || selectedItem.Tag == null)
                return FederalGazetteAssistantTabs.NotDefined;

            return (FederalGazetteAssistantTabs)selectedItem.Tag;
        }

        /// <summary>
        /// Set all part of the report by company size.
        /// The role:
        ///               S    M    L
        /// Bilanz        x    x    x
        /// GuV                x    x
        /// Anhang        x    x    x
        /// Lagebericht        x    x
        /// </summary>
        private void SetPartOfReport() {
            if (rbSmall.IsChecked == true) {
                chkBilanz.IsChecked = true;
                chkGuV.IsChecked = false;
                chkAnhang.IsChecked = true;
                chkLagebericht.IsChecked = false;
            } else {
                chkBilanz.IsChecked = true;
                chkGuV.IsChecked = true;
                chkAnhang.IsChecked = true;
                chkLagebericht.IsChecked = true;
            }

            chkBilanz.IsEnabled = (bool) !chkBilanz.IsChecked;
            chkAnhang.IsEnabled = (bool) !chkAnhang.IsChecked;
            chkGuV.IsEnabled = (bool) !chkGuV.IsChecked;
            chkLagebericht.IsEnabled = (bool) !chkLagebericht.IsChecked;
        }
        #endregion
    }
}

