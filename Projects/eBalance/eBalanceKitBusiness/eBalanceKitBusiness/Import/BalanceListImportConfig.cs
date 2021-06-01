/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2011-04-22      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using AvdCommon.DataGridHelper;
using AvdCommon.DataGridHelper.Interfaces;
using eBalanceKitResources.Localisation;
using System.Drawing;

namespace eBalanceKitBusiness.Import {

    public enum BalanceListImportType {
        SignedBalance,
        DebitCreditFlagOneColumn,
        DebitCreditFlagTwoColumns,
        BalanceInTwoColumns
    }

    /// <summary>
    /// Configuration for balance list importer.
    /// </summary>
    public class BalanceListImportConfig : INotifyPropertyChanged {

        #region constructor
        public BalanceListImportConfig() {
            this.Encoding = Encoding.UTF8;
            this.FirstLineIsHeadline = true;
            this.Seperator = ",";
            this.TextDelimiter = "\"";
            this.ImportType = BalanceListImportType.SignedBalance;
            this.TaxonomyColumnExists = false;

        }
        #endregion constructor

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion events

        #region properties

        #region BalanceListName
        private string _balanceListName = ResourcesBalanceList.DefaultBalanceListName;

        public string BalanceListName {
            get { return _balanceListName; }
            set {
                if (_balanceListName != value) {
                    _balanceListName = value;
                    OnPropertyChanged("BalanceListName");
                }
            }
        }
        #endregion

        #region CsvFileName
        public string CsvFileName {
            get { return _csvFileName; }
            set {
                if (_csvFileName != value) {
                    _csvFileName = value;
                    OnPropertyChanged("CsvFileName");
                }
            }
        }
        string _csvFileName;
        #endregion

        #region Encoding
        public Encoding Encoding {
            get { return _encoding; }
            set {
                if (_encoding != value) {
                    _encoding = value;
                    OnPropertyChanged("Encoding");
                }
            }
        }
        private Encoding _encoding;
        #endregion

        #region FirstLineIsHeadline
        public bool FirstLineIsHeadline {
            get { return _firstLineIsHeadline; }
            set {
                if (_firstLineIsHeadline != value) {
                    _firstLineIsHeadline = value;
                    OnPropertyChanged("FirstLineIsHeadline");
                }
            }
        }
        private bool _firstLineIsHeadline;
        #endregion

        #region TaxonomyColumnExists

        public bool TaxonomyColumnExists {
            get { return _taxonomyColumnExists; }
            set {
                if (_taxonomyColumnExists != value) {
                    _taxonomyColumnExists = value;
                    OnPropertyChanged("TaxonomyColumnExists");
                    OnPropertyChanged("ImportType");
                }
            }
        }
        private bool _taxonomyColumnExists;
        #endregion

        #region Comment

        public bool Comment {
            get { return _comment; }
            set {
                if (_comment != value) {
                    _comment = value;
                    OnPropertyChanged("Comment");
                    OnPropertyChanged("ImportType");
                }
            }
        }
        private bool _comment;
        #endregion

        #region Index
        public bool Index {
            get { return _index; }
            set {
                if (_index != value) {
                    _index = value;
                    OnPropertyChanged("Index");
                    OnPropertyChanged("ImportType");
                }
            }
        }
        
        private bool _index;
        #endregion

        #region Seperator
        public string Seperator {
            get { return _seperator; }
            set {
                if (_seperator != value) {
                    _seperator = value;
                    OnPropertyChanged("Seperator");
                }
            }
        }
        private string _seperator;
        #endregion

        #region TextDelimiter
        public string TextDelimiter {
            get { return _textDelimiter; }
            set {
                if (_textDelimiter != value) {
                    _textDelimiter = value;
                    OnPropertyChanged("TextDelimiter");
                }
            }
        }
        private string _textDelimiter;
        #endregion

        #region ImportType
        public BalanceListImportType ImportType {
            get { return _importType; }
            set {
                if (_importType != value) {
                    _importType = value;

                    // reset column assignments if import type has been changed
                    for (int i = 0; i < ColumnDict.Count; i++) {
                        ColumnDict[ColumnDict.ElementAt(i).Key] = string.Empty;
                    }
                    OnPropertyChanged("ImportType");
                }
            }
        }
        BalanceListImportType _importType;
        #endregion

        #region ColumnDict
        class MyDictionary<T1,T2> : Dictionary<T1, T2> {

            public new T2 this[T1 key] {
                get { return this.ContainsKey(key) ? base[key] : default(T2); }
                set { this[key] = value; }
            }
        }
        Dictionary<string, string> _columnDict = new MyDictionary<string,string>();
        public static Dictionary<string, string> ColumnToNameDict = new Dictionary<string, string>(){
            {"AccountNumberCol", ResourcesBalanceList.ReferenceListAccountNumber},
            //{"AccountNumberCol", "Kontonummer"},
            {"AccountNameCol", ResourcesBalanceList.ColumnToNameName},
            {"DebitBalanceCol", ResourcesBalanceList.ColumnToNameExpense},
            {"CreditBalanceCol", ResourcesBalanceList.ColumnToNameIncome},
            {"BalanceColSigned", ResourcesBalanceList.ColumnToNameSigned},
            {"DebitCreditFlagCol", ResourcesBalanceList.ColumnToNameMarkIncomeExpense},
            {"DebitFlagCol", ResourcesBalanceList.ColumnToNameMarkExpense},
            {"CreditFlagCol", ResourcesBalanceList.ColumnToNameMarkIncome},
            {"BalanceCol", ResourcesBalanceList.ColumnToNameBalance},
            {"Taxonomy", ResourcesBalanceList.ColumnToNameTaxonomy},
            {"Comment", ResourcesBalanceList.ColumnToNameComment},
            {"Index", ResourcesBalanceList.ColumnToNameIndex}
        };

        public Dictionary<string, string> ColumnDict {
            get { return _columnDict; }
            set { _columnDict = value; }
        }

        public readonly Dictionary<string, Color> ColorDict = new Dictionary<string, Color> {
            {"AccountNumberCol", Color.BurlyWood},
            {"AccountNameCol", Color.Gold},
            {"DebitBalanceCol", Color.LightYellow},
            {"CreditBalanceCol", Color.Coral},
            {"BalanceColSigned", Color.CornflowerBlue},
            {"DebitCreditFlagCol", Color.LightPink},
            {"DebitFlagCol", Color.DarkKhaki},
            {"CreditFlagCol", Color.LightBlue},
            {"BalanceCol", Color.Lavender},
            {"Taxonomy", Color.ForestGreen},
            {"Comment", Color.Bisque},
            {"Index", Color.IndianRed}
        };

        #region Warning
        private string _warning;

        public string Warning {
            get { return _warning; }
            set {
                _warning = value;
                OnPropertyChanged("Warning");
            }
        }
        #endregion

        #endregion

        #endregion properties
    }
}
