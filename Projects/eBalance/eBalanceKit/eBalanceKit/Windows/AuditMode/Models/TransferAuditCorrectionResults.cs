// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-05
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using Utils;
using eBalanceKitBusiness.AuditCorrections;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKit.Windows.AuditMode.Models {
    public class TransferAuditCorrectionResults : NotifyPropertyChangedBase {

        public TransferAuditCorrectionResults(Dictionary<Document, Dictionary<IAuditCorrection, IAuditCorrection>> newAuditCorrectionsDict) {
            var results = new Dictionary<IAuditCorrection, Dictionary<Document, IAuditCorrection>>();
            foreach (var pair1 in newAuditCorrectionsDict) {
                foreach (var pair2 in pair1.Value) {
                    var document = pair1.Key;
                    var origCorrection = pair2.Key;
                    var newCorrection = pair2.Value;
                    
                    if (!results.ContainsKey(origCorrection))
                        results[origCorrection] = new Dictionary<Document, IAuditCorrection>();

                    results[origCorrection][document] = newCorrection;
                }
            }

            TransferAuditCorrectionResultTables = new Dictionary<IAuditCorrection, TransferAuditCorrectionResultTable>();
            foreach (var pair in results) {
                var origCorrection = pair.Key;
                var correctionAssignments = pair.Value;
                TransferAuditCorrectionResultTables[origCorrection] = new TransferAuditCorrectionResultTable(correctionAssignments);
            }
        }

        public Dictionary<IAuditCorrection, TransferAuditCorrectionResultTable> TransferAuditCorrectionResultTables { get; private set; }
        
        #region SelectedAuditCorrectionTransaction
        private CheckableAuditCorrectionTransaction _selectedAuditCorrectionTransaction;

        public CheckableAuditCorrectionTransaction SelectedAuditCorrectionTransaction {
            get { return _selectedAuditCorrectionTransaction; }
            set {
                if (_selectedAuditCorrectionTransaction == value) return;
                _selectedAuditCorrectionTransaction = value;
                OnPropertyChanged("SelectedAuditCorrectionTransaction");
            }
        }
        #endregion // SelectedAuditCorrectionTransaction
    }

    public class TransferAuditCorrectionResultTable {
        public TransferAuditCorrectionResultTable(Dictionary<Document, IAuditCorrection> results) {
            Columns = new List<TransferAuditCorrectionResultColumn>();
            Rows = new List<TransferAuditCorrectionResultRow>();
            foreach (var pair in results) {
                var document = pair.Key;
                var newCorrection = pair.Value;

                Columns.Add(new TransferAuditCorrectionResultColumn(document.Name, document.FinancialYear.FYear));
                foreach(var transaction in newCorrection.Transactions) {
                    Rows.Add(new TransferAuditCorrectionResultRow(transaction.Label));

                    // TODO TODO TODO
                }
            }
        }

        public List<TransferAuditCorrectionResultColumn> Columns { get; private set; }
        public List<TransferAuditCorrectionResultRow> Rows { get; private set; }
    }

    public class TransferAuditCorrectionResultRow {
        public TransferAuditCorrectionResultRow(string header) {
            Values = new Dictionary<TransferAuditCorrectionResultColumn, IAuditCorrectionTransaction>();
            Header = header;
        }

        public string Header { get; private set; }
        public Dictionary<TransferAuditCorrectionResultColumn, IAuditCorrectionTransaction> Values { get; private set; }
    }

    public class TransferAuditCorrectionResultColumn {
        public TransferAuditCorrectionResultColumn(string header, int financialYear) {
            Header = header;
            FinancialYear = financialYear;
        }

        public string Header { get; private set; }
        public int FinancialYear { get; private set; }
    }


}