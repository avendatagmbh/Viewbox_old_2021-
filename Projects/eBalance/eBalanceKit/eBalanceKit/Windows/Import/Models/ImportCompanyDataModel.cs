// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-25
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Utils;
using eBalanceKit.Models.Import;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Import.Models {
    public class ImportCompanyDataModel {

        public ImportCompanyDataModel(Window owner) { Owner = owner; }
        
        protected Window Owner { get; set; }

        #region ImportItem

        public void CreateCompanies(object sender, CustomCreateCompanyArgs args) {
            var companyTableList = args.CompanyTableList;
            foreach (Tuple<DataTable, List<int>> companyTuple in companyTableList) {
                Company existingCompany = null;
                foreach (Company company in CompanyManager.Instance.AllowedCompanies) {
                    foreach (DataRow row in companyTuple.Item1.Rows) {
                        if (row[1].ToString() != "genInfo.company.id.name" ||
                            (company.ValueTree.Root.Values["de-gcd_genInfo.company.id.name"].Value != null &&
                            company.ValueTree.Root.Values["de-gcd_genInfo.company.id.name"].Value.ToString() !=
                            row[2].ToString())) continue;
                        existingCompany = company;
                        break;
                    }
                    if (existingCompany != null) {
                        break;
                    }
                }
                if (existingCompany == null) {
                    Company company = new Company();
                    company.SetFinancialYearIntervall(2009, 2030);
                    company.SetVisibleFinancialYearIntervall(DateTime.Now.Year, DateTime.Now.Year);
                    try {
                        CompanyManager.Instance.AddCompany(company);
                        SetCompany(companyTuple.Item1, companyTuple.Item2, company);
                        CompanyManager.Instance.SaveCompany(company);
                    } catch (Exception e) {
                        e.Data["detail"] = companyTuple;
                        args.ErrorList.Add(e);
                    }
                } else {
                    try {
                        // deleting existing contacts
                        var existingContacts =
                            existingCompany.ValueTree.Root.Values["de-gcd_genInfo.company.id.contactAddress"] as
                            XbrlElementValue_List;
                        Debug.Assert(existingContacts != null);
                        for (int i = existingContacts.Items.Count - 1; i > -1; i--) {
                            existingContacts.DeleteValue(existingContacts.Items[i]);
                        }
                        // deleting existing shareholders
                        var existingShareholders =
                            existingCompany.ValueTree.Root.Values["de-gcd_genInfo.company.id.shareholder"] as
                            XbrlElementValue_List;
                        Debug.Assert(existingShareholders != null);
                        for (int i = existingShareholders.Items.Count - 1; i > -1; i--) {
                            existingShareholders.DeleteValue(existingShareholders.Items[i]);
                        }
                        SetCompany(companyTuple.Item1, companyTuple.Item2, existingCompany);
                        CompanyManager.Instance.SaveCompany(existingCompany);
                    } catch (Exception e) {
                        e.Data["detail"] = companyTuple;
                        args.ErrorList.Add(e);
                    }
                }
            }
        }

        private void SetCompany(DataTable csvData, List<int> badRows, Company company) {
            const string longPrefix = "de-gcd_genInfo.company.id.";
            const string prefix = "genInfo.company.id.";
            int iCount = csvData.Rows.Count;
            //string gcdUri = "http://www.xbrl.de/taxonomies/de-gcd-2011-09-14:";
            for (int i = iCount - 1; i >= 0; i--) {
                if (badRows.IndexOf(i) != -1) { continue; }
                DataRow row = csvData.Rows[i];
                var key = row[1].ToString();

                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(row[2].ToString())) {
                    if ((key.Length > 30) && (key.Substring(0, 30) == prefix + "shareholder")) {
                        continue;
                    } else if ((key.Length > 32) && (key.Substring(0, 33) == prefix + "contactAddress")) {
                        continue;
                    } else if ((key.Length > 27) && (key.Substring(0, 27) == prefix + "industry")) {

                        if ((key.Length > 34) && (key.Substring(0, 35) == prefix + "industry.keyType")) {
                            var industry = company.ValueTree.Root.Values[longPrefix + "industry"];
                            var industryAsTuple = (industry as XbrlElementValue_Tuple);
                            Debug.Assert(industryAsTuple != null);
                            var keyType =
                                industryAsTuple.Items[0].Values[longPrefix + "industry.keyType"] as
                                XbrlElementValue_SingleChoice;
                            Debug.Assert(keyType != null);
                            if (company.ValueTree.Root.Elements.ContainsKey("de-gcd_" + row[2])) {
                                keyType.SelectedValue = company.ValueTree.Root.Elements["de-gcd_" + row[2]];
                            }

                            industryAsTuple.Items[0].Values[longPrefix + "industry.keyEntry"].Value =
                                csvData.Rows[i + 1][2].ToString();

                            industryAsTuple.Items[0].Values[longPrefix + "industry.name"].Value =
                                csvData.Rows[i + 2][2].ToString();
                        }
                    } else if (company.ValueTree.Root.Values.ContainsKey("de-gcd_" + key) &&
                               company.ValueTree.Root.Values["de-gcd_" + key] is XbrlElementValue_SingleChoice) {
                        var singleChoiceElement =
                            company.ValueTree.Root.Values["de-gcd_" + key] as
                            XbrlElementValue_SingleChoice;
                        Debug.Assert(singleChoiceElement != null);
                        if (company.ValueTree.Root.Elements.ContainsKey("de-gcd_" + row[2])) {
                            singleChoiceElement.SelectedValue = company.ValueTree.Root.Elements["de-gcd_" + row[2]];
                        }
                    } else if (company.ValueTree.Root.Values.ContainsKey("de-gcd_" + key) &&
                               company.ValueTree.Root.Values["de-gcd_" + key] is XbrlElementValue_Date) {
                        DateTime outTime;
                        if (DateTime.TryParse(row[2].ToString(), out outTime))
                            company.ValueTree.Root.Values["de-gcd_" + key].Value = outTime;
                    } else if ((key.Length > 37) && (key.Substring(0, 38) == prefix + "idNo.type.companyId")) {
                        var idNumbers = company.ValueTree.Root.Values[longPrefix + "idNo"] as XbrlElementValue_Tuple;
                        Debug.Assert(idNumbers != null);
                        var idNumbersAsTuple = (idNumbers).Items[0];
                        var firstId = idNumbersAsTuple.Values["de-gcd_" + key];
                        firstId.Value = row[2];
                    } else if ((key.Length > 27) && (key.Substring(0, 28) == prefix + "stockExch")) {
                        var stockExch = company.ValueTree.Root.Values[longPrefix + "stockExch"] as XbrlElementValue_Tuple;
                        Debug.Assert(stockExch != null);
                        var stockExchAsTuple = stockExch.Items[0];
                        var first = stockExchAsTuple.Values["de-gcd_" + key];
                        first.Value = row[2];
                    } else if (key.StartsWith(prefix + "parent")) {
                        var tupleElement =
                            company.ValueTree.Root.Values[longPrefix + "parent"] as XbrlElementValue_Tuple;
                        Debug.Assert(tupleElement != null);
                        if (key.StartsWith(prefix + "parent.idNo")) {
                            string tempKey = "de-gcd_genInfo.company.id.idNo.type.companyId." + key.Substring(46);
                            var idTupleElement =
                                tupleElement.Items[0].Values["de-gcd_genInfo.company.id.parent.idNo"] as
                                XbrlElementValue_Tuple;
                            Debug.Assert(idTupleElement != null);
                            idTupleElement.Items[0].Values[tempKey].Value = row[2];
                        } else {
                            if (key == "genInfo.company.id.parent.name.dateOfLastChange" ||
                                key == "genInfo.company.id.parent.legalStatus.dateOfLastChange") {
                                DateTime outTime;
                                if (DateTime.TryParse(row[2].ToString(), out outTime)) {
                                    tupleElement.Items[0].Values["de-gcd_" + key].Value = outTime;
                                }
                            } else {
                                tupleElement.Items[0].Values["de-gcd_" + key].Value = row[2];
                            }
                        }
                    } else {
                        company.ValueTree.Root.Values["de-gcd_" + key].Value = row[2].ToString();
                    }

                } else if ((row[0].ToString() == "Stichtag") && (csvData.Rows[i + 1][0].ToString() == "Beginn") &&
                           (csvData.Rows[i + 2][0].ToString() == "Ende")) {
                    SetFinancialYears(row, csvData.Rows[i + 1], csvData.Rows[i + 2], company, csvData.Columns.Count, csvData.Rows[i - 1]);
                }
            }
            company.Name = company.ValueTree.Root.Values["de-gcd_genInfo.company.id.name"].Value.ToString();
            // previous contacts and shareholders are deleted.
            AddContactOrShareholder(csvData, company, badRows);
        }

        private void AddContactOrShareholder(DataTable csvData, Company company, List<int> badRows) {
            int iCount = csvData.Rows.Count;

            var shareholderRoot = company.ValueTree.Root.Values["de-gcd_genInfo.company.id.shareholder"];
            XbrlElementValue_List shareholderRootList = shareholderRoot as XbrlElementValue_List;

            var contactRoot = company.ValueTree.Root.Values["de-gcd_genInfo.company.id.contactAddress"];
            XbrlElementValue_List contactRootList = contactRoot as XbrlElementValue_List;

            for (int j = 2; j < csvData.Columns.Count; j++) {

                bool isNewShareholder = false;
                var shareholder = shareholderRootList.AddValue();
                shareholderRootList.SelectedItem = shareholder;

                bool isNewContact = false;
                var contact = contactRootList.AddValue();
                contactRootList.SelectedItem = contact;

                for (int i = iCount - 1; i >= 0; i--) {
                    if (badRows.IndexOf(i) != -1) { continue; }
                    DataRow row = csvData.Rows[i];
                    var key = row[1].ToString();
                    var value = csvData.Rows[i][j].ToString();
                    if ((key.Length > 30) &&
                        (key.Substring(0, 30) == "genInfo.company.id.shareholder") &&
                        (!string.IsNullOrEmpty(value))) {
                        if ((key.Length > 67) && (key.Substring(31, 37) == "ProfitDivideKey.dateOfunderyearChange")) {
                            DateTime outTime;
                            if (DateTime.TryParse(value, out outTime)) {
                                shareholder.Values["de-gcd_" + key].Value = outTime;
                            }
                        } else if ((key.Length > 47) &&
                            ((key.Substring(31, 17) == "extensionRequired") || (key.Substring(31, 17) == "SpecialBalanceReq")) &&
                            (!string.IsNullOrEmpty(value))) {
                            bool outBool;
                            if (bool.TryParse(value, out outBool))
                                shareholder.Values["de-gcd_" + key].Value = outBool;
                        } else if ((key.Length > 46) &&
                            (key.Substring(31, 14) == "ShareDivideKey")) {
                            var shareDivideKey =
                                shareholder.Values["de-gcd_genInfo.company.id.shareholder.ShareDivideKey"] as
                                XbrlElementValue_Tuple;
                            Debug.Assert(shareDivideKey != null);
                            var numerator = shareDivideKey.Items[0].Values["de-gcd_" + key];
                            numerator.Value = value;
                        } else if ((key.Length == 42) &&
                            (key.Substring(31, 11) == "legalStatus")) {
                            var singleChoiceElement = shareholder.Values["de-gcd_" + key] as XbrlElementValue_SingleChoice;
                            //string gcdUri = "http://www.xbrl.de/taxonomies/de-gcd-2011-09-14:";
                            Debug.Assert(singleChoiceElement != null);
                            if (company.ValueTree.Root.Elements.ContainsKey("de-gcd_" + value))
                                singleChoiceElement.SelectedValue = company.ValueTree.Root.Elements["de-gcd_" + value];
                        } else {
                            shareholder.Values["de-gcd_" + csvData.Rows[i][1]].Value = value;
                        }
                        isNewShareholder = true;
                    }
                    if (key.Length > 33 && key.Substring(0, 33) == "genInfo.company.id.contactAddress" && !string.IsNullOrEmpty(row[j].ToString())) {
                        contact.Values["de-gcd_" + key].Value = value;
                        isNewContact = true;
                    }
                }
                if (!isNewShareholder) {
                    shareholderRootList.DeleteSelectedValue();
                }
                if (!isNewContact) {
                    contactRootList.DeleteSelectedValue();
                }
            }
        }

        private void SetFinancialYears(DataRow deadlineRow, DataRow beginRow, DataRow endRow, Company company, int columnCount, DataRow yearRow) {
            int curYear = -1;
            for (int j = 2; j < columnCount; j++) {
                if (string.IsNullOrEmpty(deadlineRow[j].ToString())) continue;
                curYear = Int32.Parse(yearRow[j].ToString());
                FinancialYear year = company.FinancialYears.First(f => f.FYear == curYear && f.Company.Id == company.Id);
                //if (year == null) {
                //    year = new FinancialYear {Company = company, FYear = curYear};
                //    SetFinancialYear(ref year, curYear, deadlineRow, beginRow, endRow, j);
                //    company.FinancialYears.Add(year);
                //} else {
                SetFinancialYear(ref year, curYear, deadlineRow, beginRow, endRow, j);
                //}
            }
            int firstYear = Int32.Parse(yearRow[2].ToString());
            if (curYear == -1) {
                throw new Exception(ResourcesCompanyImport.NoFinancialYears);
            }
            company.SetVisibleFinancialYearIntervall(firstYear, curYear);
        }

        private void SetFinancialYear(ref FinancialYear year, int curYear, DataRow deadlineRow, DataRow beginRow, DataRow endRow, int j) {
            DateTime outTime;
            if (DateTime.TryParse(beginRow[j].ToString(), out outTime)) {
                year.FiscalYearBegin = outTime;
            }
            if (DateTime.TryParse(endRow[j].ToString(), out outTime)) {
                year.FiscalYearEnd = outTime;
            }
            if (DateTime.TryParse(deadlineRow[j].ToString(), out outTime)) {
                year.BalSheetClosingDate = outTime;
            }
            year.Save();
            // TODO : delete the relating not needed code.
            //year.FiscalYearBeginPrevious = new DateTime(curYear - 1, 1, 1);
            //year.FiscalYearEndPrevious = new DateTime(curYear - 1, 12, 31);
            //year.BalSheetClosingDatePreviousYear = new DateTime(curYear - 1, 12, 31);
        }

        public CsvReader CsvReader { get; set; }
        #endregion ImportItem         
    }
}