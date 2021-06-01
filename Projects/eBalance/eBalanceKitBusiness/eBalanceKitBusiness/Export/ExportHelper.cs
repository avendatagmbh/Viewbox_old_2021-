using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Taxonomy.Enums;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Export {
    public class ExportHelper {
        private static char[] DisallowedCharsDirectory {
            get {
                return new[] {
                    ':', '+', '#', '\'', '/', ';', '!', '?', '%', '^', '`', '=', '~', '<',
                    '>', '|', ',', '"'
                };
            }
        }


        private static char[] DisallowedChars {
            get {
                return new[] {
                    ':', '\\', '+', '#', '\'', '/', ';', '!',
                    '?', '%', '^', '`', '=', '~', '<', '>', '|', ','
                };
            }
        }


        private static char[] DisallowedCharsGeneral {
            get {
                return new[] {
                    //'+', '#', '/', ';', '!', '?', '%', '^', '`', '=', '~', '<',
                    //'>', '|', ',', 
                    //'"'
                    '"', '\'', '>', '<'
                };
            }
        }

        public static string GetValidFilePath2(string filename) {
            string tmpDrive = string.Empty;
            string tmpDirectory = string.Empty;
            FileInfo fi = new FileInfo(filename);

            if (filename.StartsWith(@"\\")) {
                //It's a network drive
                tmpDirectory = fi.DirectoryName;
            } else {
                tmpDrive = fi.Directory.Root.Name;
                // Directory without drive
                tmpDirectory = fi.DirectoryName.Remove(0, 3);
            }


            foreach (var s in DisallowedCharsDirectory) {
                tmpDirectory = tmpDirectory.Replace(s.ToString(), "_");
            }

            return tmpDrive + tmpDirectory;
        }

        public static string GetValidFileName(string file) {
            if (string.IsNullOrEmpty(file)) {
                return string.Empty;
            }
            var filename = GetFileDestination(file);
            var fi = new FileInfo(filename);
            filename = fi.Name;
            if (string.IsNullOrEmpty(fi.Extension) || !fi.Extension.Equals(".pdf")) {
                filename += ".pdf";
            }
            return filename;

            //var tmpFileName = file;
            //tmpFileName = tmpFileName.Substring(tmpFileName.LastIndexOf("\\") + 2,
            //                                    tmpFileName.Length - tmpFileName.LastIndexOf("\\") - 2);

            //string validFileName;
            //try {
            //FileInfo fi = new FileInfo(file);
            //var filename = fi.Name;
            //    if (string.IsNullOrEmpty(filename)) {
            //        filename = tmpFileName;
            //    }
            //string regexSearch = new string(DisallowedChars);
            //Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            //validFileName = r.Replace(filename, "_");
            //} catch (Exception e) {

            //    validFileName = GetValidFileInput(file);
            //    if (validFileName.Equals(file)) {
            //        // Seems like the user has not yet finished writing the path
            //        return string.Empty;
            //    }
            //    validFileName = GetValidFileName(validFileName);
            //}
            //return validFileName;
        }

        public static string GetValidFilePath(string filename, bool stillTyping = false) {
            if (string.IsNullOrEmpty(filename))
                return string.Empty;
            

            var fi = new FileInfo(GetFileDestination(filename));

            return fi.DirectoryName;

            //string validFilePath;
            //try {
            //    FileInfo fi = new FileInfo(filename);
            //    //filename = fi.DirectoryName ?? string.Empty;
            //    if (fi.Directory == null) {
            //        return string.Empty;
            //    }
            //    string tmpDrive = string.Empty;
            //    string tmpDirectory;
            //    if (filename.StartsWith(@"\\")) {
            //        //It's a network drive
            //        tmpDirectory = fi.DirectoryName ?? string.Empty;
            //    }
            //    else {
            //        tmpDrive = fi.Directory.Root.Name;
            //        // Directory without drive
            //        tmpDirectory = fi.DirectoryName.Remove(0, tmpDrive.Length);
            //    }

            //    string regexSearch = new string(DisallowedCharsDirectory);
            //    Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            //    validFilePath = r.Replace(tmpDirectory, "_");
            //    validFilePath = tmpDrive + validFilePath;
            //} catch (Exception e) {
            //    validFilePath = GetValidFileInput(filename);
            //    if (validFilePath.Equals(filename)) {
            //        // Seems like the user has not yet finished writing the path
            //        return filename;
            //    }
            //    validFilePath = GetValidFilePath(validFilePath);
            //}
            //return (stillTyping && !filename.Contains(validFilePath)) ? filename : validFilePath;
        }

        public static string GetValidFileName2(string filename) {

            FileInfo fi = new FileInfo(filename);
               var tmpFileName = fi.Name;

            foreach (var disallowedChar in DisallowedChars) {
                tmpFileName = tmpFileName.Replace(disallowedChar.ToString(), "_");
            }
            if (tmpFileName.LastIndexOf(".") == -1) tmpFileName = tmpFileName + ".pdf";
            if (tmpFileName.LastIndexOf(".") == tmpFileName.Length - 1) tmpFileName = tmpFileName + "pdf";

            return tmpFileName;
        }


        public static string GetFileDestination(string file) {

            file = GetValidFileInput(file);

            var tmpDrive = file.StartsWith(@"\\") ? string.Empty : file.Substring(0, file.IndexOf(":") + 2);

            var tmpFileName = file.Substring(file.LastIndexOf("\\") + 1,
                                                file.Length - file.LastIndexOf("\\") - 1);
            var tmpDirectory = file.Substring(tmpDrive.Length, file.LastIndexOf("\\") + 1 - tmpDrive.Length);

            
            try {
                FileInfo fi = new FileInfo(file);
                if (file.StartsWith(@"\\")) {
                    //It's a UNC path
                    tmpDirectory = fi.DirectoryName;
                    tmpDrive = string.Empty;
                }
                else {
                    tmpDrive = fi.Directory.Root.Name;
                    // Directory without drive
                    tmpDirectory = fi.DirectoryName.Remove(0, tmpDrive.Length);
                }
                tmpFileName = fi.Name;
            } catch (Exception) {
                
            }


            string regexSearchPath = new string(DisallowedCharsDirectory);
            Regex rPath = new Regex(string.Format("[{0}]", Regex.Escape(regexSearchPath)));
            var validFilePath = rPath.Replace(tmpDirectory, "_");

            rPath = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidPathChars()))));
            validFilePath = rPath.Replace(validFilePath, "_");
            //validFilePath = tmpDrive + validFilePath;
            if (!validFilePath.Substring(validFilePath.Length - 1).Equals("\\")) {
                validFilePath += "\\";
            }

            string regexSearchFile = new string(DisallowedChars);
            Regex rFile = new Regex(string.Format("[{0}]", Regex.Escape(regexSearchFile)));
            var validFileName = rFile.Replace(tmpFileName, "_");


            rFile = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))));
            validFileName = rFile.Replace(validFileName, "_");

            return string.Concat(tmpDrive, validFilePath, validFileName);
        }

        public static bool IsValidFileDestination(string filedestination) {

            if (string.IsNullOrEmpty(filedestination)) {
                return true;
            }
            
            try {
                FileInfo fi = new FileInfo(filedestination);

                if (fi.Directory == null || string.IsNullOrEmpty(fi.DirectoryName)) {
                    return true;
                }

                string tmpDrive = string.Empty;
                string tmpDirectory;
                if (filedestination.StartsWith(@"\\")) {
                    //It's a network drive
                    tmpDirectory = fi.DirectoryName ?? string.Empty;
                }
                else {
                    tmpDrive = fi.Directory.Root.Name;
                    // Directory without drive
                    tmpDirectory = fi.DirectoryName.Remove(0, tmpDrive.Length);
                }


                string regexSearchPath = new string(DisallowedCharsDirectory);
                Regex rPath = new Regex(string.Format("[{0}]", Regex.Escape(regexSearchPath)));
                var validFilePath = rPath.Replace(tmpDirectory, "_");
                validFilePath = tmpDrive + validFilePath;

                var filename = fi.Name;
                string regexSearchFile = new string(DisallowedChars);
                Regex rFile = new Regex(string.Format("[{0}]", Regex.Escape(regexSearchFile)));
                var validFileName = rFile.Replace(filename, "_");

                var validDestination = string.Concat(tmpDrive, validFilePath, validFileName);

                if (validDestination.Equals(filedestination)) {
                    return true;
                }

                return false;

            } catch (Exception) {

                return false;
            }
        }

        private static string GetValidFileInput(string filename) {
            
            string regexSearch = new string(DisallowedCharsGeneral);
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            var validFileInput = r.Replace(filename, "_");

            r = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidPathChars()))));
            validFileInput = r.Replace(filename, "_");

            return validFileInput;
        }


        public static string GetUri(string what, Document doc)
        {
            string uri;
            _uriList.TryGetValue(doc.MainTaxonomyInfo.Type, out uri);

            if (what.Equals("incomeStatement") && doc.MainTaxonomyInfo.Type == TaxonomyType.Financial)
                what = "incomeStatementStf";
            uri = uri + what;
            if (!doc.MainTaxonomy.RoleExists(uri)) {
                var validRole = doc.MainTaxonomy.Roles.FirstOrDefault(role => role.Id.Equals("role_" + what));
                uri = validRole != null ? validRole.RoleUri : string.Empty;
            }

            return doc.GaapPresentationTrees.ContainsKey(uri) ? uri : string.Empty;
        }


        private static readonly Dictionary<TaxonomyType, string> _uriList = new Dictionary<TaxonomyType, string> {
            {TaxonomyType.GAAP, "http://www.xbrl.de/taxonomies/de-gaap-ci/role/"}, 
            {TaxonomyType.OtherBusinessClass, "http://www.xbrl.de/taxonomies/de-gaap-ci/role/"}, 
            {TaxonomyType.Financial, "http://www.xbrl.de/taxonomies/de-fi/role/"}, 
            {TaxonomyType.Insurance, "http://www.xbrl.de/taxonomies/de-ins/role/"} };

        public struct ExportTypeNames
        {
            public ExportTypeNames(string name, string headline)
            {
                Name = name;
                Headline = headline;
                tv = new PdfTreeView();
                Uri = string.Empty;
            }
            public string Name;
            public string Headline;
            internal PdfTreeView tv;
            public string Uri;

        }


        public struct ExportInformations
        {
            public ExportInformations(Document doc) {

                string uri;
                _uriList.TryGetValue(doc.MainTaxonomyInfo.Type, out uri);
                
                BalanceSheet.Uri = uri + BalanceSheet.Name;
                IncomeStatement.Uri = uri +
                                      (doc.MainTaxonomyInfo.Type == TaxonomyType.Financial
                                           ? "incomeStatementStf"
                                           : IncomeStatement.Name);
                AppropriationProfits.Uri = uri + AppropriationProfits.Name;
                ContingentLiabilities.Uri = uri + ContingentLiabilities.Name;
                CashFlowStatement.Uri = uri + CashFlowStatement.Name;
                AdjustmentOfIncome.Uri = uri + AdjustmentOfIncome.Name;
                DeterminationOfTaxableIncome.Uri = uri + DeterminationOfTaxableIncome.Name;
                DeterminationOfTaxableIncomeBusinessPartnership.Uri = uri + DeterminationOfTaxableIncomeBusinessPartnership.Name;
                DeterminationOfTaxableIncomeSpecialCases.Uri = uri + DeterminationOfTaxableIncomeSpecialCases.Name;

            }
            public static ExportTypeNames BalanceSheet = new ExportTypeNames("balanceSheet", "Bilanz");
            public static ExportTypeNames IncomeStatement = new ExportTypeNames("incomeStatement", "GuV");
            public static ExportTypeNames AppropriationProfits = new ExportTypeNames("appropriationProfits", "Ergebnisverwendung");
            public static ExportTypeNames ContingentLiabilities = new ExportTypeNames("contingentLiabilities", "Haftungsverhältnisse");
            public static ExportTypeNames CashFlowStatement = new ExportTypeNames("cashFlowStatement", "Kapitalflussrechnung");

            public static ExportTypeNames AdjustmentOfIncome = new ExportTypeNames("adjustmentOfIncome", "Berichtigung des Gewinns bei Wechsel der Gewinnermittlungsart");
            public static ExportTypeNames DeterminationOfTaxableIncome = new ExportTypeNames("determinationOfTaxableIncome", "Steuerliche Gewinnermittlung");
            public static ExportTypeNames DeterminationOfTaxableIncomeBusinessPartnership = new ExportTypeNames("determinationOfTaxableIncomeBusinessPartnership", "Steuerliche Gewinnermittlung bei Personengesellschaften");
            public static ExportTypeNames DeterminationOfTaxableIncomeSpecialCases = new ExportTypeNames("determinationOfTaxableIncomeSpecialCases", "Steuerliche Gewinnermittlung für besondere Fälle");
        }
    }
}
