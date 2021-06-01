using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using Taxonomy;
using Taxonomy.Enums;
using Utils;
using eBalanceKitBase.Interfaces;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Import.ImportCompanyDetails {
    public class CompanyDetailsImporter {

        public CompanyDetailsImporter(IImportCompany owner) {
            TaxonomyIdErrors = new ObservableCollectionAsync<WrongTaxonomyIdError>();
            ValueErrors = new ObservableCollectionAsync<WrongValueError>();
            DateErrors = new ObservableCollectionAsync<WrongDateError>();
            VisibleValueErrors = new ObservableCollectionAsync<WrongValueError>();
            BadRows = new List<int>();
            CsvData = new DataTable(ResourcesCommon.Import);
            _owner = owner;
        }

        //public void ValueInErrorChanged() {
        //    List<WrongValueError> temp = new List<WrongValueError>();
        //    foreach (WrongValueError wrongValueError in ValueErrors) {
        //        if (string.IsNullOrEmpty(wrongValueError.Value)) {
        //            temp.Add(wrongValueError);
        //        }
        //        //if (row[1].ToString().StartsWith("genInfo.company", StringComparison.InvariantCulture) && company.ValueTree.Root.Values.ContainsKey("de-gcd_" + row[1])) {
        //        //if (company.ValueTree.Root.Values.ContainsKey("de-gcd_" + row[1].ToString())) {
        //        //if (SingleChoiceChecker(row, iColumn, company, filepath, i)) {
        //        DataTable tempRow = new DataTable();
        //        tempRow.Columns.Add("Name");
        //        tempRow.Columns.Add("TaxonomieId");
        //        tempRow.Columns.Add("wert");
        //        tempRow.Rows.Add(wrongValueError.TaxonomyName, wrongValueError.TaxonomyId,
        //                         wrongValueError.SelectedValueId.Key == null
        //                             ? wrongValueError.Value
        //                             : wrongValueError.SelectedValueId.Key.Substring(7));
        //        if (SingleChoiceChecker(tempRow.Rows[0], wrongValueError.Column, new Company(), wrongValueError.Path,
        //                                wrongValueError.RowNumber, false)) {
        //            temp.Add(wrongValueError);
        //            continue;
        //        }
        //        if (!IsValidDate(tempRow.Rows[0], wrongValueError.Column, wrongValueError.File)) {
        //            temp.Add(wrongValueError);
        //            continue;
        //        }
        //        if (
        //            !IsValidBool(tempRow.Rows[0], wrongValueError.Column, wrongValueError.Path,
        //                         wrongValueError.RowNumber, false)) {
        //            temp.Add(wrongValueError);
        //            continue;
        //        }
        //        BadRows.Remove(wrongValueError.RowNumber);
        //    }
        //    ValueErrors.Clear();
        //    foreach (WrongValueError wrongValueError in temp) {
        //        ValueErrors.Add(wrongValueError);
        //    }
        //}

        #region Properties
        public ObservableCollectionAsync<WrongTaxonomyIdError> TaxonomyIdErrors { get; private set; }
        public ObservableCollectionAsync<WrongValueError> ValueErrors { get; private set; }
        public ObservableCollectionAsync<WrongValueError> VisibleValueErrors { get; private set; }
        public ObservableCollectionAsync<WrongDateError> DateErrors { get; private set; }
        public List<int> BadRows { get; private set; }
        public DataTable CsvData { get; private set; }
        public bool HasErrors { get { return TaxonomyIdErrors.Count != 0 || ValueErrors.Count(item => item.SelectedValueId.Key == null) != 0; } }
        #endregion Properties

        private int _shareholderCount;

        /// <summary>
        /// Import values into the class. This class'es properties will hold the information about the import. CsvData contains the DataTable
        /// BadRows, ValueErrors, TaxonomyIdErrors contains information about possible errors in import.
        /// </summary>
        /// <param name="filepath">full path of CSV file</param>
        /// <param name="separator"></param>
        /// <param name="textDelimiter"></param>
        /// <param name="encoding"></param>
        public bool ImportDetails(string filepath, char separator, char textDelimiter, Encoding encoding) {
            CsvReader csvReader = new CsvReader(filepath) {Separator = separator};
            DataTable csvData = csvReader.GetCsvData(0, encoding);
            csvData.TableName = filepath;
            List<int> badRows = new List<int>();
            var company = new Company();
            int iColumn = csvData.Columns.Count;
            int iCount = csvData.Rows.Count;
            if (iColumn < 4) {
                MessageBox.Show("There are less column than expected. Please check the delimiter");
                return false;
            }
            for (int i = 0; i < iCount; i++) {
                DataRow row = csvData.Rows[i];
                if (string.IsNullOrEmpty(row[1].ToString())) {
                    int temp;
                    // if the first column is not empty, the second column is empty, the third column is not empty, and not int, than it's a line in fiscal data.
                    if (!string.IsNullOrEmpty(row[0].ToString()) && !string.IsNullOrEmpty(row[2].ToString()) && !Int32.TryParse(row[2].ToString(), out temp)) {
                        for (int j = 2; j < iColumn && !string.IsNullOrEmpty(row[j].ToString()); j++) {
                            if (!IsValidFiscalDate(row, i, j, filepath)) {
                                badRows.Add(i);
                            }
                        }
                    }
                    continue;
                }
                // hacked elements are skipped from normal validation
                // we can add the .StartsWith("genInfo.company.id.idNo") to the if, than we can validate the keys if we want.
                if (row[1].ToString().StartsWith("genInfo.company.id.parent.idNo")) {
                    continue;
                }
                if (row[1].ToString().StartsWith("genInfo.company.id.shareholder.name") ) {
                    _shareholderCount = 0;
                    for (int j = 2; j < iColumn; j++) {
                        if (!string.IsNullOrEmpty(row[j].ToString())) {
                            _shareholderCount++;
                        }
                    }
                }
                if (row[1].ToString().StartsWith("genInfo.company") && company.ValueTree.Root.Elements.ContainsKey("de-gcd_" + row[1])) {
                    // check for errors in single choices. Adds the error in value errors, or taxonomy id errors. Value errors can be corrected later.
                    if (SingleChoiceChecker(row, company, filepath, i)) {
                        badRows.Add(i);
                    // checks the dates if they can be parsed. No correction, just adding to badRows.
                    } else if (!IsValidDate(row, iColumn, filepath, i)) {
                        badRows.Add(i);
                    // checks for invalid bools. 
                    } else if (!IsValidBool(row, iColumn, filepath, i)) {
                        badRows.Add(i);
                    }
                } else {
                    TaxonomyIdErrors.Add(new WrongTaxonomyIdError {
                        FilePath = FilenameShorter(filepath),
                        TaxonomyName = row[0].ToString(),
                        TaxonomyId = row[1].ToString(),
                        LineNumber = i
                    });
                    badRows.Add(i);
                }
            }
            CsvData = csvData;
            BadRows = badRows;
            return true;
        }

        private bool IsValidFiscalDate(DataRow row, int i, int j, string filepath) { 
            DateTime temp;
            if (DateTime.TryParse(row[j].ToString(), out temp)) {
                return true;
            }
            string filename = FilenameShorter(filepath);
            DateErrors.Add(new WrongDateError(this) {
                Column = j,
                LineNumber = i,
                File = filename,
                Path = filepath,
                Value = row[j].ToString(),
                TaxonomyName = row[0].ToString()
            });
            return false;
        }

        private bool SingleChoiceChecker(DataRow row, Company company, string filepath, int rowNumber, bool appendError = true) {
            var filename = FilenameShorter(filepath);
            var key = row[1].ToString();
            var value = row[2].ToString();
            bool isSingleChoice = company.ValueTree.Root.Values.ContainsKey("de-gcd_" + key) &&
                                  company.ValueTree.Root.Values["de-gcd_" + key] is XbrlElementValue_SingleChoice;
            bool isBadKey = false;
            bool isTheSingleChoiceInsideTuple = key == "genInfo.company.id.industry.keyType";
            if (isSingleChoice || isTheSingleChoiceInsideTuple) {
                List<IElement> realValues = isTheSingleChoiceInsideTuple
                                                ? ((company.ValueTree.Root.Values["de-gcd_genInfo.company.id.industry"]
                                                    as XbrlElementValue_Tuple).Items[0].Values["de-gcd_" + key] as
                                                   XbrlElementValue_SingleChoice).Elements
                                                : (company.ValueTree.Root.Values["de-gcd_" + key] as
                                                   XbrlElementValue_SingleChoice).Elements;
                isBadKey = IsBadKey(realValues, value);
                if (!string.IsNullOrEmpty(value) && isBadKey) {
                    if (appendError) {
                        WrongValueError wrongValueError = new WrongValueError(this) {
                            File = filename,
                            Path = filepath,
                            RowNumber = rowNumber,
                            TaxonomyName = row[0].ToString(),
                            TaxonomyId = row[1].ToString(),
                            Column = 2,
                            Value = value,
                            ValueList = realValues.Zip(realValues, (element, element1) =>
                                                                   new KeyValuePair<string, string>(element.Id,
                                                                                                    element1.Label))
                        };
                        if (ValueErrors.All(valueError => valueError.Value != value)) {
                            VisibleValueErrors.Add(wrongValueError);
                        }
                        ValueErrors.Add(wrongValueError);
                    }
                    return true;
                }
            } else if (key == "genInfo.company.id.shareholder.legalStatus") {
                var realValues = company.ValueTree.Root.Values["de-gcd_genInfo.company.id.shareholder"].Element.Children[9].Children[0].Children;
                for (int k = 2; k < _shareholderCount + 2; k++) {
                    value = row[k].ToString();
                    isBadKey = IsBadKey(realValues, value);
                    if (!string.IsNullOrEmpty(value) && isBadKey && appendError) {
                        WrongValueError wrongValueError = new WrongValueError(this) {
                            File = filename,
                            Path = filepath,
                            RowNumber = rowNumber,
                            TaxonomyName = row[0].ToString(),
                            TaxonomyId = row[1].ToString(),
                            Column = k,
                            Value = value,
                            ValueList =
                                realValues.Zip(realValues,
                                               (element, element1) =>
                                               new KeyValuePair<string, string>(element.Id, element1.Label))
                        };
                        if (ValueErrors.All(valueError => valueError.Value != value)) {
                            VisibleValueErrors.Add(wrongValueError);
                        }
                        ValueErrors.Add(wrongValueError);
                    }
                }
                return isBadKey;
            } 
            return false;
        }

        private bool IsBadKey(IEnumerable<IElement> realValues, string value) { return realValues.All(realValue => realValue.Id != "de-gcd_" + value); }

        private bool IsValidDate(DataRow row, int iColumn, string filePath, int i) {
            var shortKey = row[1].ToString().Substring(19);
            DateTime temp = new DateTime();
            if (shortKey == "name.dateOfLastChange" || shortKey == "legalStatus.dateOfLastChange" || shortKey == "shareholder.ProfitDivideKey.dateOfunderyearChange" ||
                shortKey == "Incorporation.dateOfFirstRegistration" || shortKey == "FoundationDate" || shortKey == "parent.legalStatus.dateOfLastChange" || shortKey == "parent.name.dateOfLastChange") {
                for (int k = 2; k < iColumn; k++) {
                    if (!string.IsNullOrEmpty(row[k].ToString()) && !DateTime.TryParse(row[k].ToString(), out temp)) {
                        DateErrors.Add(new WrongDateError(this) {
                            Column = k,
                            File = FilenameShorter(filePath),
                            LineNumber = i,
                            Path = filePath,
                            TaxonomyName = row[0].ToString(),
                            Value = row[k].ToString()
                        });
                        return false;
                    }
                }
            }
            return true;
        }

        private bool IsValidBool(DataRow row, int iColumn, string filepath, int rowNumber, bool appendError = true) {
            var shortKey = row[1].ToString().Substring(19);
            IEnumerable<KeyValuePair<string, string>> valueList = new Dictionary<string, string> {
                // in ImportCompanyDetailsModel in CorrectEntries the first 7 char will be cut. So the keys are "false", "true"
                {"de-gcd_false", ResourcesCommon.No},
                {"de-gcd_true", ResourcesCommon.Yes}
            };
            if (shortKey == "shareholder.SpecialBalanceRequired" || shortKey == "shareholder.extensionRequired") {
                for (int i = 2; i < iColumn; i++) {
                    bool value;
                    if (string.IsNullOrEmpty(row[i].ToString()) || bool.TryParse(row[i].ToString(), out value)) continue;
                    if (appendError) {
                        WrongValueError wrongValueError = new WrongValueError(this) {
                            File = FilenameShorter(filepath),
                            Path = filepath,
                            RowNumber = rowNumber,
                            TaxonomyName = row[0].ToString(),
                            TaxonomyId = row[1].ToString(),
                            Column = i,
                            Value = row[i].ToString(),
                            ValueList = valueList
                        };
                        ValueErrors.Add(wrongValueError);
                        VisibleValueErrors.Add(wrongValueError);
                    }
                    return false;
                }
            }
            return true;
        }

        private string FilenameShorter(string filepath) {
            string[] words = filepath.Split('\\');
            string filename = words[words.Count() - 1];
            return filename;
        }

        private readonly IImportCompany _owner;
        internal void UpdateHasValueErrors() {
            _owner.UpdateCanImport();
        }
    }

}
