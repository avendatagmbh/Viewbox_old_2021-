using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Utils;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Import {
    public class BalanceListImportSummaryConfig : INotifyPropertyChanged {
        public class ColumnAssignment{
            public string ColumnName { get; set; }
            public Color Color { get; set; }
            public string AssignmentName { get; set; }
        }

        public BalanceListImportSummaryConfig(Dictionary<string, System.Drawing.Color> colorDict) {
            this.ColumnAssignments = new ObservableCollectionAsync<ColumnAssignment>();
            AssignmentInfos = new ObservableCollectionAsync<string>();
            _colorDict = colorDict;
        }

        #region fields
        private readonly Dictionary<string, System.Drawing.Color> _colorDict;
        #endregion

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion events
        
        #region Properties

        public ObservableCollectionAsync<ColumnAssignment> ColumnAssignments { get; set; }
        #endregion

        internal void Update(BalanceListImporter importer) {
            ColumnAssignments.Clear();
            foreach (var pair in importer.Config.ColumnDict) {
                if (string.IsNullOrEmpty(pair.Value)) {
                    continue;
                }
                Color color = Color.FromArgb(_colorDict[pair.Key].A, _colorDict[pair.Key].R, _colorDict[pair.Key].G,
                                             _colorDict[pair.Key].B);
                ColumnAssignments.Add(new ColumnAssignment {
                    ColumnName = BalanceListImportConfig.ColumnToNameDict[pair.Key],
                    Color = color,
                    AssignmentName = pair.Value
                });
            }

            BalanceList balanceList = new BalanceList();
            AssignmentInfos.Clear();
            assignmentsCorrect = 0;
            assignmentsFailed = 0;
            if (importer.ImportAccounts(false, balanceList)) {
                numberOfAccounts = balanceList.Accounts.Count();
                SumOfAccountsDecimal = 0;
                foreach (var account in balanceList.Accounts)
                    SumOfAccountsDecimal += account.Amount;
                
                OnPropertyChanged("NumberOfAccountsString");
                OnPropertyChanged("SumOfAccountsString");
                OnPropertyChanged("NumberOfAccounts");
                OnPropertyChanged("SumOfAccounts");
                ShowErrorString = false;
            } else {
                ErrorString = ResourcesBalanceList.ImportError + importer.LastError;
                ShowErrorString = true;
            }
            OnPropertyChanged("ErrorString");
            OnPropertyChanged("ShowErrorString");
            OnPropertyChanged("AssignmentInfoString");
            OnPropertyChanged("ShowAssignmentErrors");
        }

        #region NumberOfAccountsString
        public string NumberOfAccountsString {
            get{ return ResourcesBalanceList.NumberOfAccountsCaption + numberOfAccounts; }
        }
        int numberOfAccounts;
        #endregion
        
        #region NumberOfAccounts
        public int NumberOfAccounts {
            get { return numberOfAccounts; }
        }
        #endregion
        
        #region SumOfAccountsString

        public string SumOfAccountsString {
            get { return ResourcesBalanceList.SumOfAccountLabel + SumOfAccounts; }
        }
        public decimal SumOfAccountsDecimal;
        #endregion
        
        #region SumOfAccounts

        public string SumOfAccounts { get { return LocalisationUtils.CurrencyToString(SumOfAccountsDecimal); } }
        #endregion

        public ObservableCollectionAsync<string> AssignmentInfos { get; set; }

        public string AssignmentInfoString {
            get { return string.Format(ResourcesBalanceList.AssignmentInfo, assignmentsCorrect, assignmentsFailed); }
        }
        internal void NewAssignment(bool failed, Account account, string elementString) {
            if (failed) {
                AssignmentInfos.Add(
                    string.Format(
                        ResourcesBalanceList.UnknownTaxonomyPosition,
                        account.Label, elementString));
                assignmentsFailed++;
            } else
                assignmentsCorrect++;
        }
        int assignmentsFailed, assignmentsCorrect;

        public bool ShowAssignmentErrors { get { return assignmentsCorrect != 0 || assignmentsFailed != 0; } }

        public string ErrorString { get; set; }
        public bool ShowErrorString { get; set; }
    }
}
