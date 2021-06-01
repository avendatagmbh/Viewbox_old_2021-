// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-04-23
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Taxonomy;
using Utils;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Reconciliation.DbMapping;
using eBalanceKitBusiness.Reconciliation.Import;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures;

namespace eBalanceKitBusiness.Reconciliation.ViewModels {
    public class ImportPreviousYearValuesModel : NotifyPropertyChangedBase {

        public ImportPreviousYearValuesModel() {
            _previousYearReports = new List<ImportPreviousYearValuesReport>();
            foreach (var report in DocumentManager.Instance.GetPreviousYearReports()) {

                var previousYearValues = new PreviousYearValues();
                var rights = new ReportRights(report);
                
                // check report.Document.IsCommercialBalanceSheet not possible because value tree is not loaded!
                if (rights.ReadTransferValuesAllowed /* && report.Document.IsCommercialBalanceSheet */) {
                    using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                        try {
                            Dictionary<IElement, decimal> values = new Dictionary<IElement, decimal>();
                            var taxIdMgr = DocumentManager.Instance.CurrentDocument.TaxonomyIdManager;
                            foreach (var reconciliation in conn.DbMapping.Load<DbEntityReconciliation>(
                                conn.Enquote("document_id") + "=" + report.Id)) {
                                foreach (var transaction in reconciliation.Transactions) {
                                    if (transaction.ElementId <= 0) continue;

                                    var element = taxIdMgr.GetElement(transaction.ElementId);
                                    if (!values.ContainsKey(element)) values[element] = 0;
                                    values[element] += transaction.Value;
                                }
                            }

                            foreach (var pair in values)
                                previousYearValues.AddValue(pair.Value, pair.Key);
                        } catch (Exception) {
                        }
                    }

                }

                if (previousYearValues.HasTransactions)
                    _previousYearReports.Add(new ImportPreviousYearValuesReport(report, previousYearValues));
            }

            if (_previousYearReports.Count > 0) {
                SelectedPreviousReport = _previousYearReports.First();
                SelectedPreviousReport.IsSelected = true;
            }

            if (!HasTwoOrMorePreviousYearReports) SelectedAssistantPage = 1;
        }

        #region PreviousYearReports
        private readonly List<ImportPreviousYearValuesReport> _previousYearReports;
        public IEnumerable<ImportPreviousYearValuesReport> PreviousYearReports { get { return _previousYearReports; } }
        #endregion // PreviousYearReports

        #region SelectedPreviousReport
        private ImportPreviousYearValuesReport _selectedPreviousReport;

        public ImportPreviousYearValuesReport SelectedPreviousReport {
            get { return _selectedPreviousReport; }
            set {
                if (_selectedPreviousReport == value) return;
                _selectedPreviousReport = value;

                OnPropertyChanged("SelectedPreviousYearValues");
                OnPropertyChanged("SelectedPreviousReport");
            }
        }
        #endregion // SelectedPreviousReport

        public PreviousYearValues SelectedPreviousYearValues {
            get {
                //return _selectedPreviousReport.PreviousYearValues;
                if (_selectedPreviousReport == null) return null;
                return _selectedPreviousReport.PreviousYearValues;
            }
        }

        public bool HasTwoOrMorePreviousYearReports { get { return _previousYearReports.Count >= 2; } }
        
        #region SelectedAssistantPage
        private int _selectedAssistantPage;

        public int SelectedAssistantPage {
            get { return _selectedAssistantPage; }
            set {
                if (_selectedAssistantPage == value) return;
                _selectedAssistantPage = value;
                OnPropertyChanged("SelectedAssistantPage");
            }
        }
        #endregion // SelectedAssistantPage

        public bool HasPreviousYearValues { get { return DocumentManager.Instance.CurrentDocument.ReconciliationManager.PreviousYearValues.HasTransaction; } }

        public void Import() {
            DocumentManager.Instance.CurrentDocument.ReconciliationManager.ImportPreviousYearValues(SelectedPreviousYearValues);
        }
    }
}