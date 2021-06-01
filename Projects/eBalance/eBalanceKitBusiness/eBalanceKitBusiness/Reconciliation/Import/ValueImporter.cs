// --------------------------------------------------------------------------------
// author: Gábor Bauer
// since: 2012-05-03
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Taxonomy;
using Utils;
using eBalanceKitBusiness.Import.ImportCompanyDetails;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitBusiness.Reconciliation.ReconciliationTypes;
using eBalanceKitBusiness.Reconciliation.ViewModels;
using eBalanceKitBusiness.Reconciliation.Enums;
using System.Resources;
using eBalanceKitBusiness.Structures;
using eBalanceKitBase.Structures;
using eBalanceKitResources.Localisation;
using System.Globalization;
using eBalanceKitBusiness.Manager;

namespace eBalanceKitBusiness.Reconciliation.Import {
    public class ValueImporter : NotifyPropertyChangedBase, IValueImporter
    {
        #region [ Members ]

        private readonly string idPrefix = "de-gaap-ci_";
        private const int positionDescriptionColumnIndex = 1;
        private const int positionFirstPositionColumnIndex = 3;
        private const int positionValueColumnIndexShift = 3;
        private const int accountDescriptionColumnIndex = 0;
        private const int accountFirstAccountColumnIndex = 1;
        private const int accountValueColumnIndexShift = 3;
        private string prefixBalanceListAssets;
        private string prefixBalanceListLiabilities;
        private string prefixIncomeStatement;
        private const int sampleRate = 10;

        private enum ImportFileFormat { 
            Unknown = 0,
            PositionImport = 1,
            AccountImport = 2,
        }

        #endregion [ Members ]

        #region [ Constructor ]

        public ValueImporter(ReconciliationsModel reconciliationsModel, char textDelimiter, char separator, Encoding encoding)
        {
            this.ReconciliationsModel = reconciliationsModel;
            this.TextDelimiter = textDelimiter;
            this.Separator = separator;
            this.Encoding = encoding;
            if (reconciliationsModel.PresentationTreeBalanceSheetTotalAssets.RootEntries.Any())
                this.prefixBalanceListAssets = reconciliationsModel.PresentationTreeBalanceSheetTotalAssets.RootEntries.ElementAt(0).Element.Name;
            if (reconciliationsModel.PresentationTreeBalanceSheetLiabilities.RootEntries.Any())
                this.prefixBalanceListLiabilities = reconciliationsModel.PresentationTreeBalanceSheetLiabilities.RootEntries.ElementAt(0).Element.Name;
            if (reconciliationsModel.PresentationTreeIncomeStatement.RootEntries.Any())
                this.prefixIncomeStatement = reconciliationsModel.PresentationTreeIncomeStatement.RootEntries.ElementAt(0).Element.Name;
            this.ImportedPositions = new Dictionary<string, List<Tuple<string, string, TransferKinds, List<Tuple<IElement, decimal?, decimal?>>>>>();
            this.ImportedAccounts = new Dictionary<string, List<Tuple<string, string, TransferKinds, List<Tuple<string, decimal?>>>>>();
            this.TaxonomyIdErrors = new ObservableCollectionAsync<WrongTaxonomyIdError>();
            this.TaxonomyValueErrors = new ObservableCollectionAsync<InvalidValueError>();
            this.ImportRowErrors = new ObservableCollectionAsync<ImportRowError>();

            //if (ReconciliationsModel.Document.MainTaxonomy != null)
            //    if (ReconciliationsModel.Document.MainTaxonomy.Elements != null && ReconciliationsModel.Document.MainTaxonomy.Elements.Count > 0)
            //        idPrefix = ReconciliationsMdel.Document.MainTaxonomy.Elements.Values.ElementAt(0).Id.Substring(0, ReconciliationsModel.Document.MainTaxonomy.Elements.Values.ElementAt(0).Id.IndexOf("_") + 1);

            IElement idPrefixElement = null;
            if (reconciliationsModel.PresentationTreeBalanceSheetTotalAssets.RootEntries.Any())
                idPrefixElement = ReconciliationsModel.PresentationTreeBalanceSheetTotalAssets.RootEntries.ElementAt(0).Element;
            else if (reconciliationsModel.PresentationTreeBalanceSheetLiabilities.RootEntries.Any())
                idPrefixElement = ReconciliationsModel.PresentationTreeBalanceSheetLiabilities.RootEntries.ElementAt(0).Element;
            else if (reconciliationsModel.PresentationTreeIncomeStatement.RootEntries.Any())
                idPrefixElement = ReconciliationsModel.PresentationTreeIncomeStatement.RootEntries.ElementAt(0).Element;
            if (idPrefixElement != null)
                idPrefix = idPrefixElement.Id.Substring(0, idPrefixElement.Id.IndexOf("_") + 1);
        }

        #endregion [ Constructor ]

        #region [ Properties ]

        public char TextDelimiter { get; private set; }
        public char Separator { get; private set; }
        public Encoding Encoding { get; private set; }
        /// <summary>
        /// The imported taxonomy positions: FileName - List -> Name - Description - TransferKind - List -> Element, value pairs
        /// </summary>
        private Dictionary<string, List<Tuple<string, string, TransferKinds, List<Tuple<Taxonomy.IElement, decimal?, decimal?>>>>> ImportedPositions { get; set; }
        /// <summary>
        /// The imported account values: FileName - List -> Name - Description - TransferKind - List -> Account number, value pairs
        /// </summary>
        private Dictionary<string, List<Tuple<string, string, TransferKinds, List<Tuple<string, decimal?>>>>> ImportedAccounts { get; set; }

        #endregion [ Properties ]

        #region [ IValueImporter members ]

        /// <inheritdoc />
        public ObservableCollectionAsync<WrongTaxonomyIdError> TaxonomyIdErrors { get; private set; }
        /// <inheritdoc />
        public ObservableCollectionAsync<InvalidValueError> TaxonomyValueErrors { get; private set; }
        /// <inheritdoc />
        public ObservableCollectionAsync<ImportRowError> ImportRowErrors { get; private set; }
        /// <inheritdoc />
        public bool HasErrors { get { return TaxonomyIdErrors.Count > 0 || TaxonomyValueErrors.Count > 0 || ImportRowErrors.Count > 0; } }
        /// <inheritdoc />
        public bool HasTaxonomyIdErrors { get { return TaxonomyIdErrors.Count > 0; } }
        /// <inheritdoc />
        public bool HasTaxonomyValueErrors { get { return TaxonomyValueErrors.Count > 0; } }
        /// <inheritdoc />
        public bool HasImportRowErrors { get { return ImportRowErrors.Count > 0; } }
        /// <inheritdoc />
        public ReconciliationsModel ReconciliationsModel { get; private set; }
        /// <inheritdoc />
        public string ImportStatusMessage {
            get {
                string dynamicMessage = string.Empty;

                if (this.ImportedPositions != null) {
                    int files = this.ImportedPositions.Count;
                    int transactions = this.ImportedPositions.Sum(t => t.Value.Count());
                    int values = this.ImportedPositions.Values.Sum(t => t.Sum(v => v.Item4.Count()));
                    int previousYearValues = this.ImportedPositions.Sum(t => t.Value.Sum(i => i.Item4.Sum(v => v.Item3.HasValue ? 1 : 0)));
                    dynamicMessage = string.Format(ResourcesCommon.ImportDetailedMessage, values, transactions, files, previousYearValues);
                }

                if (this.HasErrors)
                    return dynamicMessage;
                else {
                    return ResourcesCommon.ImportNoErrorsFound + " " + dynamicMessage;
                }
            }
        }

        /// <summary>
        /// Imports data form CSV file
        /// </summary>
        /// <param name="fileName">The fully qualified file name to be imported.</param>
        public void ImportFile(string fileName) {
            CsvReader reader = new CsvReader(fileName) {
                Separator = Separator,
                HeadlineInFirstRow = true,
                StringsOptionallyEnclosedBy = TextDelimiter
            };
            DataTable data = reader.GetCsvData(500, Encoding);
            data.TableName = fileName;

            //ImportFileFormat format = GetFileFormat(data);
            //if (format == ImportFileFormat.PositionImport) {
            //    List<Tuple<string, string, TransferKinds, List<Tuple<Taxonomy.IElement, decimal?>>>> importedFromFile = ImportPositionsFromFile(data);
            //    if (importedFromFile != null)
            //        ImportedPositions.Add(fileName, importedFromFile);
            //// currently we do not handle account data import
            ////} else if (format == ImportFileFormat.AccountImport) {
            ////    List<Tuple<string, string, TransferKinds, List<Tuple<string, decimal?>>>> importedFromFile = ImportAccountsFromFile(data);
            ////    if (importedFromFile != null)
            ////        ImportedAccounts.Add(fileName, importedFromFile);
            //} else {
            //    ImportRowErrors.Add(new ImportRowError() {
            //        File = fileName,
            //        LineNumber = -1,
            //        Description = ResourcesReconciliation.UnknownFileFormat
            //    });
            //    OnPropertyChanged("ImportRowErrors");
            //    OnPropertyChanged("TaxonomyValueErrors");
            //    OnPropertyChanged("TaxonomyIdErrors");
            //    OnPropertyChanged("HasErrors");
            //    OnPropertyChanged("HasImportRowErrors");
            //    OnPropertyChanged("HasTaxonomyValueErrors");
            //    OnPropertyChanged("HasTaxonomyIdErrors");
            //}
            
            ImportFileFormat format = GetFileFormat(data);
            if (format == ImportFileFormat.PositionImport) {
                List<Tuple<string, string, TransferKinds, List<Tuple<IElement, decimal?, decimal?>>>> importedFromFile = ImportPositionsFromFile(data);
                if (importedFromFile != null)
                    ImportedPositions.Add(fileName, importedFromFile);
                // currently we do not handle account data import
                //} else if (format == ImportFileFormat.AccountImport) {
                //    List<Tuple<string, string, TransferKinds, List<Tuple<string, decimal?>>>> importedFromFile = ImportAccountsFromFile(data);
                //    if (importedFromFile != null)
                //        ImportedAccounts.Add(fileName, importedFromFile);
            } else {
                ImportRowErrors.Add(new ImportRowError() {
                    FilePath = fileName,
                    LineNumber = -1,
                    Description = ResourcesReconciliation.UnknownFileFormat
                });
                OnPropertyChanged("ImportRowErrors");
                OnPropertyChanged("TaxonomyValueErrors");
                OnPropertyChanged("TaxonomyIdErrors");
                OnPropertyChanged("HasErrors");
                OnPropertyChanged("HasImportRowErrors");
                OnPropertyChanged("HasTaxonomyValueErrors");
                OnPropertyChanged("HasTaxonomyIdErrors");
            }
        }

        /// <summary>
        /// Create reconciliation transactions based on imported data
        /// </summary>
        public void CreateReconciliations() {
            // delete all imported reconciliations
            ReconciliationsModel.DeleteImportedReconciliations();

            PreviousYearValues previousYearValues = new PreviousYearValues();

            foreach (KeyValuePair<string, List<Tuple<string, string, TransferKinds, List<Tuple<IElement, decimal?, decimal?>>>>> importedFile in ImportedPositions) {
                foreach (Tuple<string, string, TransferKinds, List<Tuple<IElement, decimal?, decimal?>>> reconciliationImportValues in importedFile.Value) {
                    foreach (Tuple<IElement, decimal?, decimal?> importedPosition in reconciliationImportValues.Item4) {
                        if (importedPosition.Item3.HasValue) {
                            previousYearValues.AddValue(importedPosition.Item3.Value, importedPosition.Item1);
                        }
                    }
                    if (reconciliationImportValues.Item4.Any(t => t.Item2.HasValue)) {
                        ImportedValues reconciliationImport = ReconciliationsModel.AddReconciliation(Enums.ReconciliationTypes.ImportedValues) as ImportedValues;
                        if (reconciliationImport != null) {
                            if (!string.IsNullOrEmpty(reconciliationImportValues.Item1)) {
                                string name = reconciliationImportValues.Item1;
                                int i = 0;
                                while (ReconciliationsModel.Document.ReconciliationManager.Reconciliations.Any(r => r.Name == name)) {
                                    i++;
                                    name = reconciliationImportValues.Item1 + " " + i;
                                }
                                reconciliationImport.Name = name;
                            }
                            reconciliationImport.Comment = reconciliationImportValues.Item2;
                            reconciliationImport.TransferKind = reconciliationImportValues.Item3;
                            foreach (Tuple<IElement, decimal?, decimal?> importedPosition in reconciliationImportValues.Item4) {
                                if (importedPosition.Item2.HasValue) {
                                    ReconciliationTransaction transaction = reconciliationImport.AddTransaction(importedPosition.Item1);
                                    if (transaction != null) {
                                        //DEVNOTE: if a transaction exists with the same element then should not be overwritten with the next value
                                        transaction.Value = transaction.Value.HasValue ? transaction.Value.Value + (importedPosition.Item2 ?? 0) : importedPosition.Item2;
                                    }
                                }
                                if (importedPosition.Item3.HasValue) {
                                    previousYearValues.AddValue(importedPosition.Item3.Value, importedPosition.Item1);
                                }
                            }
                        }
                    }
                }
            }

            if (previousYearValues.Values.Count() > 0)
                //DocumentManager.Instance.CurrentDocument.ReconciliationManager.ImportPreviousYearValues(previousYearValues);
                DocumentManager.Instance.CurrentDocument.ReconciliationManager.MergePreviousYearValues(previousYearValues);
        }

        #endregion [ IValueImporter members ]

        #region [ Methods ]

        private ImportFileFormat GetFileFormat(DataTable data) {            
            ImportFileFormat format = ImportFileFormat.Unknown;
            if (data == null || data.Rows.Count == 0)
                return format;

            //int positionsFileMinColCount = positionFirstPositionColumnIndex + positionValueColumnIndexShift;
            //int accountsFileMinColCount = accountFirstAccountColumnIndex + accountValueColumnIndexShift;

            //List<ImportFileFormat> determindeRowFormats = new List<ImportFileFormat>();

            //for (int i = 0; i < (data.Rows.Count < sampleRate ? data.Rows.Count : sampleRate); i++) {
            //    DataRow row = data.Rows[i];
            //    ImportFileFormat rowFormat = ImportFileFormat.Unknown;
            //    // check format for accounts import
            //    if (data.Columns.Count >= accountsFileMinColCount && ((data.Columns.Count - accountsFileMinColCount) % accountValueColumnIndexShift == 0)) {
            //        string description = row[accountDescriptionColumnIndex].ToString();
            //        string accountNumber = row[accountFirstAccountColumnIndex].ToString();
            //        string accountName = row[accountFirstAccountColumnIndex + 1].ToString();
            //        string accountValue = row[accountFirstAccountColumnIndex + 2].ToString();
            //        double result = 0;
            //        if (!string.IsNullOrEmpty(accountNumber) && !accountNumber.Contains(".") && Double.TryParse(accountValue, out result)) {
            //            if (Double.TryParse(accountNumber.Trim().Substring(0, 1), out result))
            //                rowFormat = ImportFileFormat.AccountImport;
            //        }
            //    }  
                
            //    // if format not defined, then check next format
            //    if (rowFormat == ImportFileFormat.Unknown) {
            //        // check format for positions import
            //        if (data.Columns.Count >= positionsFileMinColCount && ((data.Columns.Count - positionsFileMinColCount) % positionValueColumnIndexShift == 0)) {
            //            string name = row[positionDescriptionColumnIndex - 1].ToString();
            //            string description = row[positionDescriptionColumnIndex].ToString();
            //            string transferKindString = row[positionDescriptionColumnIndex + 1].ToString();
            //            string positionId = row[positionFirstPositionColumnIndex].ToString();
            //            string positionName = row[positionFirstPositionColumnIndex + 1].ToString();
            //            string positionValue = row[positionFirstPositionColumnIndex + 2].ToString();
            //            // name, description is allowed to be null or empty
            //            double result = 0;
            //            if (!string.IsNullOrEmpty(positionId) && positionId.Contains(".") && Double.TryParse(positionValue, out result))
            //                rowFormat = ImportFileFormat.PositionImport;
            //        }     
            //    }
            //    determindeRowFormats.Add(rowFormat);
            //}

            //IEnumerable<ImportFileFormat> distinctFormats = determindeRowFormats.Distinct();
            //if (distinctFormats.Count() == 1)
            //    return distinctFormats.ElementAt(0);
            //else
            //    return ImportFileFormat.Unknown;

            int positionsFileMinColCount = 7;

            List<ImportFileFormat> determinedRowFormats = new List<ImportFileFormat>();

            for (int i = 0; i < (data.Rows.Count < sampleRate ? data.Rows.Count : sampleRate); i++) {
                DataRow row = data.Rows[i];
                ImportFileFormat rowFormat = ImportFileFormat.Unknown;
                // if format not defined, then check next format
                if (rowFormat == ImportFileFormat.Unknown) {
                    // check format for positions import
                    if (data.Columns.Count == positionsFileMinColCount || data.Columns.Count == positionsFileMinColCount + 1) {
                        string positionId = row[0].ToString();
                        // name, description is allowed to be null or empty
                        if (!string.IsNullOrEmpty(positionId) && positionId.Contains("."))
                            rowFormat = ImportFileFormat.PositionImport;
                    }
                }
                determinedRowFormats.Add(rowFormat);
            }

            IEnumerable<ImportFileFormat> distinctFormats = determinedRowFormats.Distinct();
            if (distinctFormats.Count() == 1)
                return distinctFormats.ElementAt(0);
            else
                return ImportFileFormat.Unknown;
        }

        /// <summary>
        /// Imports taxonomy positions data from a csv file (DataTable)
        /// </summary>
        /// <param name="data">The data to be imported</param>
        /// <returns>The imported data</returns>
        private List<Tuple<string, string, TransferKinds, List<Tuple<IElement, decimal?, decimal?>>>> ImportPositionsFromFile(DataTable data) {

            List<Tuple<string, string, TransferKinds, List<Tuple<IElement, decimal?, decimal?>>>> importedTransactions = new List<Tuple<string, string, TransferKinds, List<Tuple<IElement, decimal?, decimal?>>>>();
            
            //int rowNumber = 1;
            //foreach (DataRow row in data.Rows) {
            //    Tuple<string, string, TransferKinds, List<Tuple<IElement, decimal?>>> importedRow = null;
            //    if (TryImportTaxonomyPositionRow(row, data.TableName, rowNumber, out importedRow)) {
            //        if (importedRow != null)
            //            importedRows.Add(importedRow);
            //    }
            //    rowNumber++;
            //}

            List<Tuple<IElement, decimal?, TransferKinds, string, string, decimal?>> importedRows = new List<Tuple<IElement, decimal?, TransferKinds, string, string, decimal?>>();
            int rowNumber = 1;
            foreach (DataRow row in data.Rows) {
                Tuple<IElement, decimal?, TransferKinds, string, string, decimal?> importedRow = null;
                if (TryImportTaxonomyPositionRow_New(row, data.TableName, rowNumber, out importedRow)) {
                    if (importedRow != null)
                        importedRows.Add(importedRow);
                }
                rowNumber++;
            }

            importedRows = Merge(importedRows);
            importedTransactions = MergeTransactions(importedRows);

            OnPropertyChanged("ImportRowErrors");
            OnPropertyChanged("TaxonomyValueErrors");
            OnPropertyChanged("TaxonomyIdErrors");
            OnPropertyChanged("HasErrors");
            OnPropertyChanged("HasImportRowErrors");
            OnPropertyChanged("HasTaxonomyValueErrors");
            OnPropertyChanged("HasTaxonomyIdErrors");

            return importedTransactions.Count == 0 ? null : importedTransactions;
        }

        /// <summary>
        /// Merges a list of imported rows into transactions
        /// </summary>
        /// <param name="importedRows">The imported rows</param>
        /// <returns>The imported rows converted to transactions</returns>
        private List<Tuple<string, string, TransferKinds, List<Tuple<IElement, decimal?, decimal?>>>> MergeTransactions(IEnumerable<Tuple<IElement, decimal?, TransferKinds, string, string, decimal?>> importedRows) {
            List<Tuple<string, string, TransferKinds, List<Tuple<IElement, decimal?, decimal?>>>> importedTransactions = new List<Tuple<string, string, TransferKinds, List<Tuple<IElement, decimal?, decimal?>>>>();

            // order by element id and merge the values
            importedRows = importedRows.OrderBy(r => r.Item4);
            List<Tuple<IElement, decimal?, TransferKinds, string, string, decimal?>> tmpRowsToMerge = new List<Tuple<IElement, decimal?, TransferKinds, string, string, decimal?>>();
            string lastTransaction = null;
            foreach (Tuple<IElement, decimal?, TransferKinds, string, string, decimal?> importedRow in importedRows) {
                if (lastTransaction != null && lastTransaction != importedRow.Item4) {
                    // merge
                    if (tmpRowsToMerge.Count == 0)
                        tmpRowsToMerge.Add(importedRow);
                    MergeTransaction(tmpRowsToMerge, importedTransactions);
                    tmpRowsToMerge = new List<Tuple<IElement, decimal?, TransferKinds, string, string, decimal?>>();
                    tmpRowsToMerge.Add(importedRow);
                } else {
                    tmpRowsToMerge.Add(importedRow);
                }
                lastTransaction = importedRow.Item4;
            }
            MergeTransaction(tmpRowsToMerge, importedTransactions);

            return importedTransactions;
        }

        /// <summary>
        /// Merges a list of imported rows into a transaction
        /// </summary>
        /// <param name="rowsToMerge">The list of imported roews</param>
        /// <param name="importedTransactions">The transactions to which the new transaction will be added</param>
        private void MergeTransaction(List<Tuple<IElement, decimal?, TransferKinds, string, string, decimal?>> rowsToMerge, List<Tuple<string, string, TransferKinds, List<Tuple<IElement, decimal?, decimal?>>>> importedTransactions) {
            //rowsToMerge = new List<Tuple<IElement, decimal?, TransferKinds, string, string>>(rowsToMerge.OrderByDescending(r => r.Item3));
            if (rowsToMerge.Count == 0) return;

            int index = rowsToMerge.Count - 1;
            string name = rowsToMerge[index].Item4;
            string description = rowsToMerge[index].Item5;
            TransferKinds kind = rowsToMerge[index].Item3;
            List<Tuple<IElement, decimal?, decimal?>> transaction = new List<Tuple<IElement, decimal?, decimal?>>(rowsToMerge.Select(r => new Tuple<IElement, decimal?, decimal?>(r.Item1, r.Item2, r.Item6)));
            importedTransactions.Add(new Tuple<string, string, TransferKinds, List<Tuple<IElement, decimal?, decimal?>>>(name, description, kind, transaction));
        }

        /// <summary>
        /// Merges a list of imported rows by element id
        /// </summary>
        /// <param name="importedRows">The imported rows</param>
        /// <returns>Merged rows</returns>
        private List<Tuple<IElement, decimal?, TransferKinds, string, string, decimal?>> Merge(IEnumerable<Tuple<IElement, decimal?, TransferKinds, string, string, decimal?>> importedRows) {
            List<Tuple<IElement, decimal?, TransferKinds, string, string, decimal?>> importedRowsMerged = new List<Tuple<IElement, decimal?, TransferKinds, string, string, decimal?>>();

            // order by element id and merge the values
            importedRows = importedRows.OrderBy(r => r.Item1.Id);
            List<Tuple<IElement, decimal?, TransferKinds, string, string, decimal?>> tmpRowsToMerge = new List<Tuple<IElement, decimal?, TransferKinds, string, string, decimal?>>();
            string lastId = null;
            foreach (Tuple<IElement, decimal?, TransferKinds, string, string, decimal?> importedRow in importedRows) {
                if (lastId != null && lastId != importedRow.Item1.Id) {
                    // merge
                    if (tmpRowsToMerge.Count == 0)
                        tmpRowsToMerge.Add(importedRow);
                    MergeRow(tmpRowsToMerge, importedRowsMerged);
                    tmpRowsToMerge = new List<Tuple<IElement, decimal?, TransferKinds, string, string, decimal?>>();
                    tmpRowsToMerge.Add(importedRow);
                } else {
                    tmpRowsToMerge.Add(importedRow);
                }
                lastId = importedRow.Item1.Id;
            }
            MergeRow(tmpRowsToMerge, importedRowsMerged);

            return importedRowsMerged;
        }

        /// <summary>
        /// Merges a list of imported rows into one row (the value will be added up)
        /// </summary>
        /// <param name="rowsToMerge">The list of rows to merge</param>
        /// <param name="rowsMerged">The merged rows to which the new row will be added</param>
        private void MergeRow(List<Tuple<IElement, decimal?, TransferKinds, string, string, decimal?>> rowsToMerge, List<Tuple<IElement, decimal?, TransferKinds, string, string, decimal?>> rowsMerged) {
            if (rowsToMerge.Count == 0) return;

            rowsToMerge = new List<Tuple<IElement, decimal?, TransferKinds, string, string, decimal?>>(rowsToMerge.OrderByDescending(r => r.Item4));
            string name = rowsToMerge[0].Item4;
            string description = rowsToMerge[0].Item5;
            TransferKinds kind = rowsToMerge[0].Item3;
            IElement element = rowsToMerge[0].Item1;
            //decimal value = rowsToMerge.Sum(row => row.Item2.HasValue ? row.Item2.Value : 0);
            //decimal previousYearValue = rowsToMerge.Sum(row => row.Item6.HasValue ? row.Item6.Value : 0);
            decimal? value = null;
            if (rowsToMerge.Any(row => row.Item2.HasValue)) 
                value = rowsToMerge.Sum(row => row.Item2.HasValue ? row.Item2.Value : 0);
            decimal? previousYearValue = null;
            if (rowsToMerge.Any(row => row.Item6.HasValue))
                previousYearValue = rowsToMerge.Sum(row => row.Item6.HasValue ? row.Item6.Value : 0);
            rowsMerged.Add(new Tuple<IElement, decimal?, TransferKinds, string, string, decimal?>(element, value, kind, name, description, previousYearValue));
        }

        /// <summary>
        /// Imports account data from a csv file (DataTable)
        /// </summary>
        /// <param name="data">The data to be imported</param>
        /// <returns>The imported data</returns>
        private List<Tuple<string, string, TransferKinds, List<Tuple<string, decimal?>>>> ImportAccountsFromFile(DataTable data) {

            List<Tuple<string, string, TransferKinds, List<Tuple<string, decimal?>>>> importedRows = new List<Tuple<string, string, TransferKinds, List<Tuple<string, decimal?>>>>();

            int rowNumber = 1;
            foreach (DataRow row in data.Rows) {
                Tuple<string, string, TransferKinds, List<Tuple<string, decimal?>>> importedRow = null;
                if (TryImportAccountRow(row, data.TableName, rowNumber, out importedRow)) {
                    if (importedRow != null)
                        importedRows.Add(importedRow);
                }
                rowNumber++;
            }

            OnPropertyChanged("ImportRowErrors");
            OnPropertyChanged("TaxonomyValueErrors");
            OnPropertyChanged("TaxonomyIdErrors");
            OnPropertyChanged("HasErrors");
            OnPropertyChanged("HasImportRowErrors");
            OnPropertyChanged("HasTaxonomyValueErrors");
            OnPropertyChanged("HasTaxonomyIdErrors");

            return importedRows;
        }

        /// <summary>
        /// Imports data from a datarow from the csv file
        /// </summary>
        /// <param name="row">The datarow which will be processed</param>
        /// <param name="fileName">The name of the file which is being processed</param>
        /// <param name="rowNumber">The sequence number of the row which will be processed</param>
        /// <param name="importedRow">The imported data (transaction / account number, value pairs)</param>
        /// <returns>True if successful otherwise false</returns>
        private bool TryImportAccountRow(DataRow row, string fileName, int rowNumber, out Tuple<string, string, TransferKinds, List<Tuple<string, decimal?>>> importedRow) {
             Tuple<string, string, TransferKinds, List<Tuple<string, decimal?>>> importedRowTmp = null;

            try {
                string description = row[accountDescriptionColumnIndex].ToString();
                string name = string.Empty;
                importedRowTmp = new Tuple<string, string, TransferKinds, List<Tuple<string, decimal?>>>(name, description, TransferKinds.Reclassification, new List<Tuple<string, decimal?>>());

                int colIndex = accountFirstAccountColumnIndex;
                while (colIndex < row.ItemArray.Count()) {
                    decimal? value = null;

                    // try to get value
                    int valueIndex = colIndex + accountValueColumnIndexShift - 1;
                    bool? valueResult = TryGetValue(row, valueIndex, out value);
                    if (!valueResult.HasValue) {
                        // generate an error when value is not valid
                        TaxonomyValueErrors.Add(new InvalidValueError() {
                            FilePath = fileName,
                            TaxonomyName = row[colIndex + 1].ToString(),
                            TaxonomyId = row[colIndex].ToString(),
                            Value = row[valueIndex].ToString(),
                            //DEVNOTE: rowNumber + 1 because of the header row
                            LineNumber = rowNumber + 1
                        });
                    }

                    string accountNumber = row[colIndex].ToString();
                    // if accountNumber is an empty string it means the id value pair in that position was not presented (no error generated, no import occures)
                    if (!string.IsNullOrEmpty(accountNumber)) {
                        if (!TryFindAccount(accountNumber)) {
                            // generate an error when account not found
                            TaxonomyIdErrors.Add(new WrongTaxonomyIdError() {
                                FilePath = fileName,
                                TaxonomyName = row[colIndex + 1].ToString(),
                                TaxonomyId = row[colIndex].ToString(),
                                //DEVNOTE: rowNumber + 1 because of the header row
                                LineNumber = rowNumber + 1
                            });
                        } else {
                            importedRowTmp.Item4.Add(new Tuple<string, decimal?>(accountNumber, value));
                        }
                    }
                    colIndex += accountValueColumnIndexShift;
                }

                importedRow = importedRowTmp;
                return true;
            } catch (IndexOutOfRangeException) {
                // generate an error when invalid format
                ImportRowErrors.Add(new ImportRowError() {
                    FilePath = fileName,
                    //DEVNOTE: rowNumber + 1 because of the header row
                    LineNumber = rowNumber + 1,
                    Description = ResourcesReconciliation.InvalidFormatError
                });
                importedRow = importedRowTmp;
                return false;
            }
        }

        /*
        /// <summary>
        /// Imports data from a datarow from the csv file
        /// </summary>
        /// <param name="row">The datarow which will be processed</param>
        /// <param name="fileName">The name of the file which is being processed</param>
        /// <param name="rowNumber">The sequence number of the row which will be processed</param>
        /// <param name="importedRow">The imported data (transaction / element value pairs)</param>
        /// <returns>True if successful otherwise false</returns>
        private bool TryImportTaxonomyPositionRow(DataRow row, string fileName, int rowNumber, out Tuple<string, string, TransferKinds, List<Tuple<IElement, decimal?>>> importedRow) {
            Tuple<string, string, TransferKinds, List<Tuple<IElement, decimal?>>> importedRowTmp = null;

            try {
                string name = row[positionDescriptionColumnIndex - 1].ToString();
                string description = row[positionDescriptionColumnIndex].ToString();
                string transferKindString = row[positionDescriptionColumnIndex + 1].ToString();

                TransferKinds transferKind = TransferKinds.ValueChange;
                if (!TryParseTransferKind(transferKindString, out transferKind)) {
                    ImportRowErrors.Add(new ImportRowError() {
                        FilePath = fileName,
                        //DEVNOTE: rowNumber + 1 because of the header row
                        LineNumber = rowNumber + 1,
                        Description = ResourcesReconciliation.TransferKindError
                    });
                    importedRow = importedRowTmp;
                    return false;
                }

                importedRowTmp = new Tuple<string, string, TransferKinds, List<Tuple<IElement, decimal?>>>(name, description, transferKind, new List<Tuple<IElement, decimal?>>());

                int colIndex = positionFirstPositionColumnIndex;
                while (colIndex < row.ItemArray.Count()) {
                    decimal? value = null;

                    // try to get value
                    int valueIndex = colIndex + positionValueColumnIndexShift - 1;
                    bool? valueResult = TryGetValue(row, valueIndex, out value);
                    if (!valueResult.HasValue) {
                        // generate an error when value is not valid
                        TaxonomyValueErrors.Add(new InvalidValueError() {
                            FilePath = fileName,
                            TaxonomyName = row[colIndex + 1].ToString(),
                            TaxonomyId = row[colIndex].ToString(),
                            Value = row[valueIndex].ToString(),
                            //DEVNOTE: rowNumber + 1 because of the header row
                            LineNumber = rowNumber + 1
                        });
                    }
                    // try to find element, no matter if value is null
                    // null values will generate warnings after import when checking with Validate method
                    string elementName = row[colIndex].ToString();
                    // if elementName is an empty string it means the id value pair in that position was not presented (no error generated, no import occures)
                    if (!string.IsNullOrEmpty(elementName)) {
                        Taxonomy.IElement element;
                        if (!TryFindElement(elementName, out element)) {
                            // generate an error when element not found
                            TaxonomyIdErrors.Add(new WrongTaxonomyIdError() {
                                FilePath = fileName,
                                TaxonomyName = row[colIndex + 1].ToString(),
                                TaxonomyId = row[colIndex].ToString(),
                                //DEVNOTE: rowNumber + 1 because of the header row
                                LineNumber = rowNumber + 1
                            });
                        } else {
                            importedRowTmp.Item4.Add(new Tuple<IElement, decimal?>(element, value));
                        }
                    }
                    colIndex += positionValueColumnIndexShift;
                }
                importedRow = importedRowTmp;
                return true;
            } catch (IndexOutOfRangeException ex) {
                // generate an error when invalid format
                ImportRowErrors.Add(new ImportRowError() {
                    FilePath = fileName,
                    //DEVNOTE: rowNumber + 1 because of the header row
                    LineNumber = rowNumber + 1,
                    Description = ResourcesReconciliation.InvalidFormatError
                });
                importedRow = importedRowTmp;
                return false;
            }
        }*/

        private bool TryImportTaxonomyPositionRow_New(DataRow row, string fileName, int rowNumber, out Tuple<IElement, decimal?, TransferKinds, string, string, decimal?> importedRow) {
            Tuple<IElement, decimal?, TransferKinds, string, string, decimal?> importedRowTmp = null;

            bool isBalanceList = row.ItemArray.Count() > 7;
            int positionFirstPositionColumnIndex = 0;
            int positionValueColumnIndexShift = isBalanceList ? 4 : 3;
            string name = null;
            // used for error description
            try {
                name = row[positionFirstPositionColumnIndex + positionValueColumnIndexShift + 2].ToString();
            } catch { }

            try {
                TransferKinds transferKind = TransferKinds.ValueChange;
                
                // try to find element
                IElement element;
                string elementName = row[positionFirstPositionColumnIndex].ToString();
                // if elementName is an empty string it means the id value pair in that position was not presented (import row error will be generated)
                if (!string.IsNullOrEmpty(elementName)) {
                    if (!TryFindElement(elementName, out element)) {
                        // generate an error when element not found
                        TaxonomyIdErrors.Add(new WrongTaxonomyIdError() {
                            FilePath = fileName,
                            TaxonomyName = row[positionFirstPositionColumnIndex + 1].ToString(),
                            TaxonomyId = row[positionFirstPositionColumnIndex].ToString(),
                            //DEVNOTE: rowNumber + 1 because of the header row
                            LineNumber = rowNumber + 1,
                            ReconciliationName = name
                        });
                        importedRow = importedRowTmp;
                        return false;
                    }
                } else {
                    ImportRowErrors.Add(new ImportRowError() {
                        FilePath = fileName,
                        //DEVNOTE: rowNumber + 1 because of the header row
                        LineNumber = rowNumber + 1,
                        Description = ResourcesReconciliation.InvalidFormatError,
                        ReconciliationName = name
                    });
                    importedRow = importedRowTmp;
                    return false;
                }
                
                // try to get value
                decimal? value = null;
                int valueIndex = positionFirstPositionColumnIndex + positionValueColumnIndexShift;
                bool? valueResult = TryGetValue(row, valueIndex, out value);
                // missing value is error only in the case when the reconciliation name is entered
                if ((!valueResult.HasValue || (valueResult.HasValue && value == null)) && (name != null && !string.IsNullOrEmpty(name))) {
                    // generate an error when value is not valid
                    TaxonomyValueErrors.Add(new InvalidValueError() {
                        FilePath = fileName,
                        TaxonomyName = row[positionFirstPositionColumnIndex + 1].ToString(),
                        TaxonomyId = row[positionFirstPositionColumnIndex].ToString(),
                        Value = row[valueIndex].ToString(),
                        //DEVNOTE: rowNumber + 1 because of the header row
                        LineNumber = rowNumber + 1,
                        ReconciliationName = name
                    });
                    importedRow = importedRowTmp;
                    return false;
                }

                // try to get previous year value
                decimal? previousYearValue = null;
                bool? previousYearValueResult = null;
                if (isBalanceList) {
                    int previousYearValueIndex = positionFirstPositionColumnIndex + positionValueColumnIndexShift - 1;
                    previousYearValueResult = TryGetValue(row, previousYearValueIndex, out previousYearValue);
                }

                // if no value and previous year value and reconciliation name then skipp this row
                if (!value.HasValue && !previousYearValue.HasValue && string.IsNullOrEmpty(name)) {
                    importedRow = importedRowTmp;
                    return false;
                }

                string description = row[positionFirstPositionColumnIndex + positionValueColumnIndexShift + 3].ToString();

                importedRowTmp = new Tuple<IElement, decimal?, TransferKinds, string, string, decimal?>(element, value, transferKind, name, description, previousYearValue);
                importedRow = importedRowTmp;
                return true;
            } catch (IndexOutOfRangeException) {
                // generate an error when invalid format
                ImportRowErrors.Add(new ImportRowError() {
                    FilePath = fileName,
                    //DEVNOTE: rowNumber + 1 because of the header row
                    LineNumber = rowNumber + 1,
                    Description = ResourcesReconciliation.InvalidFormatError,
                    ReconciliationName = name
                });
                importedRow = importedRowTmp;
                return false;
            }
        }

        /// <summary>
        /// Try to parse reconciliation transferkind from string (from all language representations and from TransferKinds enum ToString() as well)
        /// </summary>
        /// <param name="transferKindString">The string representation of transfer kind imported</param>
        /// <param name="transferKind">The determined transfer kind if success</param>
        /// <returns>True if transfer kind could be determined otherwise false</returns>
        private bool TryParseTransferKind(string transferKindString, out TransferKinds transferKind) {
                        
            bool isTransferKindIdentified = false;
            if (!Enum.TryParse<TransferKinds>(transferKindString, true, out transferKind)) {
                if (TransferKindCaptionHelper.Instance.LngRepValueChange.Any(lr => string.Compare(lr, transferKindString, StringComparison.InvariantCultureIgnoreCase) == 0)) {
                    transferKind = TransferKinds.ValueChange;
                    return true;
                }
                if (TransferKindCaptionHelper.Instance.LngRepReclassification.Any(lr => string.Compare(lr, transferKindString, StringComparison.InvariantCultureIgnoreCase) == 0)) {
                    transferKind = TransferKinds.Reclassification;
                    return true;
                }
                if (TransferKindCaptionHelper.Instance.LngRepReclassificationWithValueChange.Any(lr => string.Compare(lr, transferKindString, StringComparison.InvariantCultureIgnoreCase) == 0)) {
                    transferKind = TransferKinds.ReclassificationWithValueChange;
                    return true;
                }
            } else isTransferKindIdentified = true;

            return isTransferKindIdentified;
        }

        /// <summary>
        /// Gets the value from the column with colIndex
        /// </summary>
        /// <param name="row">The data row</param>
        /// <param name="valueIndex">The coulumn index to get data from</param>
        /// <param name="value">The value in the column</param>
        /// <returns>Returns true when the value is a valid number, false when the value is provided but invalid number and returns null if value not provided</returns>
        private bool? TryGetValue(DataRow row, int valueIndex, out decimal? value) {
            if (valueIndex < row.ItemArray.Count()) {
                Decimal decimalValue;
                if (string.IsNullOrEmpty(row[valueIndex].ToString().Trim())) {
                    value = null;
                    return false;
                }
                NumberStyles style = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
                CultureInfo culture = LocalisationUtils.GermanCulture;
                if (Decimal.TryParse(row[valueIndex].ToString(), style, culture, out decimalValue)) {
                    value = decimalValue;
                    return true;
                }
            }
            value = null;
            return null;
        }

        /// <summary>
        /// Finds an element by name in the presentation trees
        /// </summary>
        /// <param name="elementName">The element name to be found</param>
        /// <param name="element">The element found</param>
        /// <returns>True if element with the given elementName is found</returns>
        private bool TryFindElement(string elementName, out Taxonomy.IElement element) {
            string id = elementName.StartsWith(idPrefix) ? elementName : idPrefix + elementName;
            IReconciliationTreeNode node = null;
            //DEVNOTE: try to avoid iterating trough all trees
            if (elementName.StartsWith(prefixBalanceListAssets)) {
                node = ReconciliationsModel.PresentationTreeBalanceSheetTotalAssets.GetNode(id);
                if (node == null)
                    node = ReconciliationsModel.PresentationTreeBalanceSheetTotalAssets.GetNodeWithoutPrefix(elementName);
            } else if (elementName.StartsWith(prefixBalanceListLiabilities)) {
                node = ReconciliationsModel.PresentationTreeBalanceSheetLiabilities.GetNode(id);
                if (node == null)
                    node = ReconciliationsModel.PresentationTreeBalanceSheetLiabilities.GetNodeWithoutPrefix(elementName);
            } else if (elementName.StartsWith(prefixIncomeStatement)) {
                node = ReconciliationsModel.PresentationTreeIncomeStatement.GetNode(id);
                if (node == null)
                    node = ReconciliationsModel.PresentationTreeIncomeStatement.GetNodeWithoutPrefix(elementName);
            } 
            if (node == null) {
                node = ReconciliationsModel.PresentationTreeBalanceSheetTotalAssets.GetNode(id);
                if (node == null)
                    node = ReconciliationsModel.PresentationTreeBalanceSheetTotalAssets.GetNodeWithoutPrefix(elementName);
                if (node == null)
                    node = ReconciliationsModel.PresentationTreeBalanceSheetLiabilities.GetNode(id);
                if (node == null)
                    node = ReconciliationsModel.PresentationTreeBalanceSheetLiabilities.GetNodeWithoutPrefix(elementName);
                if (node == null)
                    node = ReconciliationsModel.PresentationTreeIncomeStatement.GetNode(id);
                if (node == null)
                    node = ReconciliationsModel.PresentationTreeIncomeStatement.GetNodeWithoutPrefix(elementName);
            }
            element = node == null ? null : node.Element;
            return element != null;
        }

        /// <summary>
        /// Finds an account by numnber
        /// </summary>
        /// <param name="accountNumber">The account number to be found</param>
        /// <returns>True if account with the given account number is found</returns>
        private bool TryFindAccount(string accountNumber) {
            foreach (IBalanceList balanceList in ReconciliationsModel.Document.BalanceLists) {
                if (balanceList.Accounts.Any(a => a.Number == accountNumber)) return true;

                if (balanceList.AccountGroups.Any(a => a.Number == accountNumber)) return true;

                if (balanceList.SplitAccountGroups.Any(a => a.Items.Any(s => s.Number == accountNumber))) return true;
            }
            return false;
        }
        #endregion [ Methods ]
    }
}
