/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2011-04-22      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AvdCommon.DataGridHelper;
using AvdCommon.DataGridHelper.Interfaces;
using Utils;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitBusiness.Interfaces.PresentationTree;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Structures.DbMapping;
using System.ComponentModel;
using System.Globalization;
using eBalanceKitBusiness.Manager;
using System.Windows;
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;
using eBalanceKitBusiness.Structures;
using System.IO;
using eBalanceKitResources.Localisation;
using DataRow = System.Data.DataRow;
using DataTable = System.Data.DataTable;

namespace eBalanceKitBusiness.Import {
 
    /// <summary>
    /// 
    /// </summary>
    public class BalanceListImporter : INotifyPropertyChanged {

        #region constructor
        public BalanceListImporter(Document document, Window owner) {
            this.Document = document;
            this.Config = new BalanceListImportConfig();
            this.SummaryConfig = new BalanceListImportSummaryConfig(Config.ColorDict);
            this.Config.PropertyChanged += new PropertyChangedEventHandler(Config_PropertyChanged);
            //balanceListModel.PropertyChanged += new PropertyChangedEventHandler(Config_PropertyChanged);
            this.CsvReader = new Utils.CsvReader(this.Config.CsvFileName);
            this.Owner = owner;
        }
        #endregion constructor

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }       
        #endregion events

        #region eventHandler
        void Config_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            
            if (e.PropertyName != "Warning")
                Config.Warning = string.Empty;

            try {
                switch (e.PropertyName) {
                    case "ColumnDict":
                        OnPropertyChanged("PreviewData");
                        break;
                    case "Encoding":
                        LoadPreview();
                        break;

                    case "FirstLineIsHeadline":
                        this.CsvReader.HeadlineInFirstRow = Config.FirstLineIsHeadline;
                        LoadPreview();
                        break;

                    case "Seperator":
                        if (!string.IsNullOrEmpty(Config.Seperator)) {
                            CsvReader.Separator = Config.Seperator[0];
                            LoadPreview();
                        }
                        break;

                    case "TextDelimiter":
                        if (!string.IsNullOrEmpty(this.Config.TextDelimiter)) {
                            this.CsvReader.StringsOptionallyEnclosedBy = this.Config.TextDelimiter[0];
                        } else {
                            this.CsvReader.StringsOptionallyEnclosedBy = '\0';
                        }
                        LoadPreview();
                        break;

                    case "CsvFileName": {
                        if (!File.Exists(Config.CsvFileName)) {
                            Config.Warning = "Die angegebene Datei existiert nicht.";
                            return;
                        }


                        this.CsvReader.Filename = this.Config.CsvFileName;

                        //Check which is the most likely seperator
                        StreamReader reader = null;
                        try {
                            reader = new StreamReader(this.Config.CsvFileName, this.Config.Encoding);

                            if (!reader.EndOfStream) {
                                string csvLine = reader.ReadLine().Trim();
                                this.Config.Seperator = FindMostOftenCharacter(csvLine, ",;|");
                                this.Config.TextDelimiter = FindMostOftenCharacter(csvLine, "\"'");
                            }
                        }catch(IOException ioex) {
                            this.LastError = ioex.Message;
                            Config.Warning = ioex.Message;
                            //throw;
                        }
                        catch (Exception ex) {
                            this.LastError = ex.Message;
                        }
                        finally {
                            if (reader != null) 
                                reader.Close();
                        }

                        if (reader != null)
                            LoadPreview();
                        //else
                            //Config.Warning = "Zugriff verweigert";
                        break;
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show(this.Owner, ex.Message);
            }
        }

        #region FindMostOftenCharacter
        string FindMostOftenCharacter(string line, string characters) {
            int max = -1; string maxSeperator = Convert.ToString(characters[0]);
            foreach (char seperator in characters) {
                int occurences = line.Split(seperator).Length - 1;
                if (occurences > max) {
                    max = occurences;
                    maxSeperator = Convert.ToString(seperator);
                }
            }
            return maxSeperator;
        }
        #endregion
        #endregion eventHandler

        #region properties

        #region Document
        /// <summary>
        /// Gets or sets the document for which the data should be imported.
        /// </summary>
        public Document Document { get; private set; }
        #endregion

        #region Config
        /// <summary>
        /// Gets or sets the configuration which is used to import the data.
        /// </summary>
        public BalanceListImportConfig Config { get; private set; }



        #region TestConfig
        private BalanceListImportConfig _testConfig;

        private BalanceListImportConfig TestConfig {
            get {
                if (_testConfig == null) {
                    var x = new ExampleCSV();
                    _testConfig = x.ImportConfig;
                    _testConfig.CsvFileName = x.CsvFilePath;
                }

                _testConfig.BalanceListName = UserConfig == null
                                                  ? ResourcesBalanceList.DefaultBalanceListName
                                                  : UserConfig.BalanceListName;

                return _testConfig;
            }
            set {
                if (_testConfig != value) {
                    _testConfig = value;
                    OnPropertyChanged("TestConfig");
                }
            }
        }
        #endregion TestConfig

        #region UserConfig
        private BalanceListImportConfig _userConfig;

        private BalanceListImportConfig UserConfig {
            get { return _userConfig; }
            set {
                if (_userConfig != value) {
                    _userConfig = value;
                    OnPropertyChanged("UserConfig");
                }
            }
        }
        #endregion UserConfig

        #region UseTestConfig
        private bool _useTestConfig;

        public bool UseTestConfig {
            get { return _useTestConfig; }
            set {
                if (_useTestConfig != value) {
                    _useTestConfig = value;
                    OnPropertyChanged("UseTestConfig");

                    if (_useTestConfig) {
                        if (UserConfig == null) {
                            UserConfig = new BalanceListImportConfig();
                        }

                        UserConfig.CsvFileName = Config.CsvFileName;

                        UserConfig.Comment = Config.Comment;
                        UserConfig.Encoding = Config.Encoding;
                        UserConfig.FirstLineIsHeadline = Config.FirstLineIsHeadline;
                        UserConfig.ImportType = Config.ImportType;
                        UserConfig.Index = Config.Index;
                        UserConfig.Seperator = Config.Seperator;
                        UserConfig.TaxonomyColumnExists = Config.TaxonomyColumnExists;
                        UserConfig.TextDelimiter = Config.TextDelimiter;

                        Config.CsvFileName = TestConfig.CsvFileName;
                        
                        Config.Comment = TestConfig.Comment;
                        Config.Encoding = TestConfig.Encoding;
                        Config.FirstLineIsHeadline = TestConfig.FirstLineIsHeadline;
                        Config.ImportType = TestConfig.ImportType;
                        Config.Index = TestConfig.Index;
                        Config.Seperator = TestConfig.Seperator;
                        Config.TaxonomyColumnExists = TestConfig.TaxonomyColumnExists;
                        Config.TextDelimiter = TestConfig.TextDelimiter;

                    } else {
                        //Config = UserConfig;
                        Config.CsvFileName = UserConfig.CsvFileName;

                        Config.Comment = UserConfig.Comment;
                        Config.Encoding = UserConfig.Encoding;
                        Config.FirstLineIsHeadline = UserConfig.FirstLineIsHeadline;
                        Config.ImportType = UserConfig.ImportType;
                        Config.Index = UserConfig.Index;
                        Config.Seperator = UserConfig.Seperator;
                        Config.TaxonomyColumnExists = UserConfig.TaxonomyColumnExists;
                        Config.TextDelimiter = UserConfig.TextDelimiter;
                    }
                    //OnPropertyChanged("Config.CsvFileName");
                    try {
                        LoadPreview();
                    } catch (Exception e) {
                        eBalanceKitBase.Structures.ExceptionLogging.LogException(e);
                    }
                    
                    OnPropertyChanged("Config");
                }
            }
        }
        #endregion UseTestConfig

        #endregion
        #region SummaryConfig
        /// <summary>
        /// Gets or sets the configuration which is used to import the data.
        /// </summary>
        public BalanceListImportSummaryConfig SummaryConfig { get; private set; }
        #endregion

        #region LastError
        /// <summary>
        /// Gets the last error message if an import process has been failed.
        /// </summary>
        public string LastError {
            get { return _lastError; }
            private set {
                if (_lastError != value) {
                    _lastError = value;
                    OnPropertyChanged("LastError");
                }
            }
        }
        private string _lastError;
        #endregion

        #region PreviewData
        /// <summary>
        /// Gets or sets a data table, which could be used to show a preview of the imported data in the gui.
        /// </summary>
        public AvdCommon.DataGridHelper.DataTable PreviewData {
            get { return _previewData; }
            private set {
                _previewData = value;
                OnPropertyChanged("PreviewData");
            }
        }
        private AvdCommon.DataGridHelper.DataTable _previewData;
        #endregion

        public void RefreshPreviewData() { OnPropertyChanged("PreviewData"); }

        #endregion properties

        public void LastPageReached() {
            DlgProgress progress = new DlgProgress(Owner) {
                ProgressInfo = {
                    IsIndeterminate = true,
                    Caption = ResourcesReconciliation.ProgressImportData
                }
            };
            try {
                progress.ExecuteModal(() => SummaryConfig.Update(this));
            } catch (ExceptionBase ex) {
                MessageBox.Show(ex.Message, ex.Header);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            } finally {
                try {
                    progress.Close();
                } catch (Exception exc) {
                    Debug.WriteLine(exc);
                }
            }
            //BalanceList balanceList = new BalanceList();
            //if (ImportAccounts(false, balanceList)) {

            //}

        }

        /// <summary>
        /// Imports a balance list into the assigned document.
        /// </summary>
        /// <returns>True, if the import succeed, false otherwise. If the import fails, the error message could be received from LastError.</returns>
        public bool Import(Document document = null) {
            if (document == null) {
                document = Document;
            }
            if (document == null) {
                return true;
            }
            BalanceList balanceList = new BalanceList {Document = document};
            if (ImportAccounts(true, balanceList)) {
                BalanceListManager.AddBalanceList(document, balanceList);
                return true;
            }
            return false;
        }

        public bool ImportAccounts(bool addAssignmentsToDocument, IBalanceList balanceList) {
            try {
                balanceList.ImportedFrom = UserManager.Instance.CurrentUser;

                DataTable csvData = this.CsvReader.GetCsvData(0, Config.Encoding);
                if (CsvReader.LastError != null) throw new Exception(CsvReader.LastError);
                int curRow = 0;

                Dictionary<string, Account> accounts = new Dictionary<string, Account>();

                foreach (DataRow dr in csvData.Rows) {
                    
                    curRow++;

                    // if there is a line without any values (only delimiters)
                    if (string.IsNullOrEmpty(string.Join(string.Empty, dr.ItemArray))) {
                        continue;
                    }

                    Account account = new Account { BalanceList = balanceList as BalanceList };

                    try {
                        account.Number = dr[Config.ColumnDict["AccountNumberCol"]].ToString().Trim();
                    } catch (Exception) {
                        LastError = string.Format(ResourcesBalanceList.InvalidAccountNumber, curRow, dr[Config.ColumnDict["AccountNumberCol"]]);
                        return false;
                    }

                    account.Name = dr[Config.ColumnDict["AccountNameCol"]].ToString();

                    string value;

                    switch (Config.ImportType) {
                        case BalanceListImportType.BalanceInTwoColumns:
                            string valueDebit = NormalizeValue(dr[Config.ColumnDict["DebitBalanceCol"]].ToString());
                            string valueCredit = NormalizeValue(dr[Config.ColumnDict["CreditBalanceCol"]].ToString());
                            decimal dvalueDebit, dvalueCredit;
                            try {
                                dvalueDebit = Convert.ToDecimal(valueDebit, CultureInfo.CreateSpecificCulture("en-US"));
                            } catch (Exception) {
                                LastError = string.Format(ResourcesBalanceList.InvalidDebitRow, curRow, valueDebit);
                                return false;
                            }

                            try {
                                dvalueCredit = Convert.ToDecimal(valueCredit, CultureInfo.CreateSpecificCulture("en-US"));
                            } catch (Exception) {
                                LastError = string.Format(ResourcesBalanceList.InvalidCreditRow, curRow, valueCredit);
                                return false;
                            }

                            account.Amount = (dvalueDebit - dvalueCredit);

                            break;

                        case BalanceListImportType.SignedBalance:
                            value = NormalizeValue(dr[Config.ColumnDict["BalanceColSigned"]].ToString());
                            try {
                                account.Amount = (Convert.ToDecimal(value, CultureInfo.CreateSpecificCulture("en-US")));
                            } catch (Exception) {
                                LastError = string.Format(ResourcesBalanceList.InvalidValue, curRow, value);
                                return false;
                            }
                            break;

                        case BalanceListImportType.DebitCreditFlagOneColumn:
                            value = NormalizeValue(dr[Config.ColumnDict["BalanceCol"]].ToString());
                            try {
                                account.Amount = (Convert.ToDecimal(value, CultureInfo.CreateSpecificCulture("en-US")));
                            } catch (Exception) {
                                LastError = string.Format(ResourcesBalanceList.InvalidValue, curRow, value);
                                return false;
                            }

                            string debitCreditFlag = dr[Config.ColumnDict["DebitCreditFlagCol"]].ToString();

                            if (debitCreditFlag != null) {
                                Regex regex = new Regex(ResourcesBalanceList.CreditRegularExpression);
                                if (regex.IsMatch(debitCreditFlag)) {
                                    // credit/Haben
                                    account.Amount *= -1;
                                }
                            }
                            break;

                        case BalanceListImportType.DebitCreditFlagTwoColumns:
                            value = NormalizeValue(dr[Config.ColumnDict["BalanceCol"]].ToString());
                            try {
                                account.Amount = (Convert.ToDecimal(value, CultureInfo.CreateSpecificCulture("en-US")));
                            } catch (Exception) {
                                LastError = string.Format(ResourcesBalanceList.InvalidValue, curRow, value);
                                return false;
                            }

                            // debitFlag is not needed, since valid balance lists could not have a flag in both columns 
                            // (except for zero values, which does not matter since the flag does not affect the amount in this case).
                            //string debitFlag = dr[Config.ColumnDict["DebitFlagCol"]].ToString();
                            
                            string creditFlag = dr[Config.ColumnDict["CreditFlagCol"]].ToString();
                            if (!string.IsNullOrEmpty(creditFlag)) account.Amount *= -1;
                            break;
                    }

                    if (accounts.ContainsKey(account.Number)) {
                        throw new Exception(string.Format(ResourcesBalanceList.NotUnique, account.Number));
                    }

                    if (Config.TaxonomyColumnExists) {
                        string elementId = dr[Config.ColumnDict["Taxonomy"]] as string;

                        if (!string.IsNullOrEmpty(elementId)) {
                            try {
                                elementId = Document.TaxonomyIdManager.TryToConvertToElementId(elementId);
                            } catch {}
                        }

                        if (elementId != null && Document.MainTaxonomy.Elements.ContainsKey(elementId)) {
                            if (addAssignmentsToDocument) {
                                try {
                                    LogManager.Instance.Log = false;
                                    Document.AddAccountAssignment(account, Document.MainTaxonomy.Elements[elementId].Id, balanceList);
                                } finally { LogManager.Instance.Log = true; }

                                // add assignment to PresentationTree, if the document is currenctly displayed
                                if (Document.GaapPresentationTrees != null) {
                                    foreach (IPresentationTree ptree in Document.GaapPresentationTrees.Values) {
                                        if (ptree.HasNode(elementId)) {
                                            IPresentationTreeNode node = ptree.GetNode(elementId) as IPresentationTreeNode;
                                            node.IsExpanded = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                                SummaryConfig.NewAssignment(false, account, elementId);

                        } else if (!addAssignmentsToDocument && !string.IsNullOrEmpty(elementId)) {
                            SummaryConfig.NewAssignment(true, account, elementId);
                        }
                    }
                    if (Config.Comment) {
                        string info = dr[Config.ColumnDict["Comment"]] as string;
                        if (!string.IsNullOrEmpty(info))
                            account.Comment = info;
                    }

                    if (Config.Index) {
                        string index = dr[Config.ColumnDict["Index"]] as string;
                        if (!string.IsNullOrEmpty(index))
                            account.SortIndex = index;
                    } else {
                        account.SortIndex = accounts.Count.ToString();
                    }
                    
                    accounts.Add(account.Number, account);
                    account.DoDbUpdate = true;
                }

                balanceList.Name = Config.BalanceListName;
                balanceList.SetEntries(accounts.Values, null, null);
                balanceList.Source = this.CsvReader.Filename;
                balanceList.ImportDate = System.DateTime.Now;
                balanceList.ImportedFrom = UserManager.Instance.CurrentUser;
                balanceList.Document = Document;
                balanceList.DoDbUpdate = true;

                return true;

            } catch (Exception ex) {
                LastError = ex.Message;
                return false;
            }

        }

        internal string NormalizeValue(string tmpValue) {
            //string value;
            //int posDot = tmpValue.LastIndexOf('.');
            //int posComma = tmpValue.LastIndexOf(',');

            //char decimalChar = posDot > posComma ? '.' : ',';

            //if (decimalChar.Equals('.')) {
            //    value = tmpValue.Trim().Replace(",", string.Empty);
            //} else if (decimalChar.Equals(',')) {
            //    value = tmpValue.Trim().Replace(".", string.Empty).Replace(",", ".");
            //} else {
            //    value = tmpValue.Trim();
            //}

            Match match = Regex.Match(tmpValue,
                                      @"^[+-]?[0-9]{1,3}(?:(?:(?:,?[0-9]{3})+(?:\.[0-9]*)?)|(?:(?:,?[0-9]{3}){0}(?:\.[0-9]{1,2})?))");
            string value = (match.Length == tmpValue.Length)
                               ? tmpValue.Trim().Replace(",", string.Empty)
                               : tmpValue.Trim().Replace(".", string.Empty).Replace(",", ".");

            if (string.IsNullOrEmpty(value)) value = "0";
            return value;
        }


        /// <summary>
        /// Loads the preview data from the csv-file.
        /// </summary>
        private void LoadPreview() {
            try {
                PreviewData = DataGridCreater.CreateDataTable(CsvReader.GetCsvData(200, Config.Encoding));
                OnPropertyChanged("PreviewData");
            } catch (Exception ex) {
                this.PreviewData = new AvdCommon.DataGridHelper.DataTable();
                this.LastError = ex.Message;
                throw new Exception(ex.Message);
            }
        }

        public Utils.CsvReader CsvReader { get; set; }

        public void ClearColumns() {
            if (PreviewData == null) {
                return;
            }
            foreach (IDataColumn dataColumn in PreviewData.Columns) {
                dataColumn.Color = System.Drawing.Color.White;
            }
        }

        public Window Owner { get; set; }
    }
}
