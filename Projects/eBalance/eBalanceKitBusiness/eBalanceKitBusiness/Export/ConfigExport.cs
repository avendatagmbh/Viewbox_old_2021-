// --------------------------------------------------------------------------------
// author: Sebastian Vetter, Mirko Dibbert
// since:  2012-02-09
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Utils;
using eBalanceKitBusiness.Localisation;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Export
{
    #region enum ExportTypes
    public enum ExportTypes
    {
        Pdf,
        Csv,
        Xbrl
    };
    #endregion enum ExportTypes

    public class ConfigExport : NotifyPropertyChangedBase, IExportConfig
    {

        /// <summary>
        /// At Pdf export should be same configs as Xbrl export
        /// </summary>
        #region ExportAsXbrl
        private bool _exportAsXbrl;

        public bool ExportAsXbrl
        {
            get { return _exportAsXbrl; }
            set
            {
                if (_exportAsXbrl != value)
                {
                    _exportAsXbrl = value;
                    OnPropertyChanged("ExportAsXbrl");
                }
            }
        }
        #endregion ExportAsXbrl

        #region class ExportTypeInfo
        public class ExportTypeInfo
        {

            public enum ExportDestinationTypes { File, Directory }

            public ExportTypeInfo(
                ExportTypes exportType,
                string label,
                string desciption,
                ExportDestinationTypes exportDestinationType,
                string fileTypeFilter)
            {

                ExportType = exportType;
                Label = label;
                Description = desciption;
                ExportDestinationType = exportDestinationType;
                FileTypeFilter = fileTypeFilter;
            }

            /// <summary>
            /// Assigned export type.
            /// </summary>
            public ExportTypes ExportType { get; private set; }

            /// <summary>
            /// Label for the export type.
            /// </summary>
            public string Label { get; private set; }

            /// <summary>
            /// Description for the export type.
            /// </summary>
            public string Description { get; private set; }

            /// <summary>
            /// Assigned export destination type (file or directory).
            /// </summary>
            public ExportDestinationTypes ExportDestinationType { get; private set; }

            /// <summary>
            /// Filter for save file dialog.
            /// </summary>
            public string FileTypeFilter { get; private set; }

            /// <summary>
            /// Label for destination selection.
            /// </summary>
            public string SelectDestinationLabel
            {
                get
                {
                    switch (ExportDestinationType)
                    {
                        case ExportDestinationTypes.File:
                            return ResourcesCommon.SelectFilename;

                        case ExportDestinationTypes.Directory:
                            return ResourcesCommon.SelectFolder;

                        default:
                            return ResourcesCommon.SelectDestination;
                    }
                }
            }
        }

        #endregion class ExportReportType

        public void InitConfig(Document document)
        {
            Document = document;

            var value =
                Document.ValueTreeGcd.Root.Values["de-gcd_genInfo.report.id.reportElement"] as XbrlElementValue_MultipleChoice;

            // init balance standard
            IsCommercialBalanceSheet = Document.IsCommercialBalanceSheet;

            {
                BoolWithNotifyChanged isChecked;
                value.IsChecked.TryGetValue("de-gcd_genInfo.report.id.reportElement.reportElements.KS", out isChecked);
                ExportAccountBalances = (isChecked != null && isChecked.BoolValue == true);
            }

            // init default values
            ExportCompanyInformation = true;
            ExportReportInformation = true;
            ExportDocumentInformation = true;
            ExportUnassignedAccounts = false;
            //ExportNotes = false;

            if (value == null) throw new Exception("Invalid type for genInfo.report.id.reportElement");

            // balance sheet
            {
                bool isPart = document.MainTaxonomy.RoleExists(ExportHelper.GetUri("balanceSheet", document));
                if (isPart)
                {
                    ShowBalanceSheet = true;
                    BoolWithNotifyChanged isChecked;
                    value.IsChecked.TryGetValue("de-gcd_genInfo.report.id.reportElement.reportElements.B", out isChecked);
                    ExportBalanceSheet = (isChecked != null && isChecked.BoolValue == true);

                }
                else
                {
                    ShowBalanceSheet = false;
                    ExportBalanceSheet = false;
                }
            }

            // income statement
            {
                bool isPart = document.MainTaxonomy.RoleExists(ExportHelper.GetUri("incomeStatement", document));
                if (isPart)
                {
                    ShowIncomeStatement = true;
                    BoolWithNotifyChanged isChecked;
                    value.IsChecked.TryGetValue("de-gcd_genInfo.report.id.reportElement.reportElements.GuV", out isChecked);
                    ExportIncomeStatement = (isChecked != null && isChecked.BoolValue == true);

                }
                else
                {
                    ShowIncomeStatement = false;
                    ExportIncomeStatement = false;
                }
            }

            // appropriation profits (Ergebnisverwendung)
            {
                bool isPart = document.MainTaxonomy.RoleExists(ExportHelper.GetUri("appropriationProfits", document));
                if (isPart)
                {
                    ShowAppropriationProfit = true;
                    BoolWithNotifyChanged isChecked;
                    value.IsChecked.TryGetValue("de-gcd_genInfo.report.id.reportElement.reportElements.EV", out isChecked);
                    ExportAppropriationProfit = (isChecked != null && isChecked.BoolValue == true);

                }
                else
                {
                    ShowAppropriationProfit = false;
                    ExportAppropriationProfit = false;
                }
            }

            // contingent liabilities (Haftungsverhältnisse)
            {
                bool isPart = document.MainTaxonomy.RoleExists(ExportHelper.GetUri("contingentLiabilities", document));
                if (isPart)
                {
                    ShowContingentLiabilities = true;
                    BoolWithNotifyChanged isChecked;
                    value.IsChecked.TryGetValue("de-gcd_genInfo.report.id.reportElement.reportElements.H", out isChecked);
                    ExportContingentLiabilities = (isChecked != null && isChecked.BoolValue == true);

                }
                else
                {
                    ShowContingentLiabilities = false;
                    ExportContingentLiabilities = false;
                }
            }

            // cash flow statement (Kapitalflussrechnung)
            {
                bool isPart = document.MainTaxonomy.RoleExists(ExportHelper.GetUri("cashFlowStatement", document));
                if (isPart)
                {
                    ShowCashFlowStatement = true;
                    BoolWithNotifyChanged isChecked;
                    value.IsChecked.TryGetValue("de-gcd_genInfo.report.id.reportElement.reportElements.CFS", out isChecked);
                    ExportCashFlowStatement = (isChecked != null && isChecked.BoolValue == true);

                }
                else
                {
                    ShowCashFlowStatement = false;
                    ExportCashFlowStatement = false;
                }
            }

            // notes (Anhang)
            {
                // TODO: change to look like other parts. Use the multiChoice value, and the checker value.
                try
                {
                    bool isPart = document.MainTaxonomy.RoleExists(ExportHelper.GetUri("notes", document));

                    if (isPart)
                    {
                        ShowNotes = true;
                        ExportNotes = false;
                    }
                    else
                    {
                        ShowNotes = false;
                        ExportNotes = false;
                    }

                }
                catch (Exception)
                {
                    ShowNotes = false;
                    ExportNotes = false;
                }
            }

            // Other Report Elements(andere berichtsbestandteile) 
            {
                // TODO: change to look like other parts. Use the multiChoice value, and the checker value.
                try
                {
                    bool isPart = document.MainTaxonomy.RoleExists(ExportHelper.GetUri("OtherReportElements", document));

                    if (isPart)
                    {
                        ShowOtherReportElements = true;
                        ExportOtherReportElements = false;
                    }
                    else
                    {
                        ShowOtherReportElements = false;
                        ExportOtherReportElements = false;
                    }

                }
                catch (Exception)
                {
                    ShowOtherReportElements = false;
                    ExportOtherReportElements = false;
                }
            }

            // management report (Lagebericht)
            {
                bool isPart = document.MainTaxonomy.RoleExists(ExportHelper.GetUri("managementReport", document));
                if (isPart)
                {
                    ShowManagementReport = true;
                    BoolWithNotifyChanged isChecked;
                    value.IsChecked.TryGetValue("de-gcd_genInfo.report.id.reportElement.reportElements.L", out isChecked);
                    ExportManagementReport = (isChecked != null && isChecked.BoolValue == true);

                }
                else
                {
                    ShowManagementReport = false;
                }
            }

            // adjustment of income (Berichtigung des Bewinns bei Wechsel der Gewinnermittlungsart)
            {
                bool isPart = document.MainTaxonomy.RoleExists(ExportHelper.GetUri("adjustmentOfIncome", document));
                if (isPart)
                {
                    ShowAdjustmentOfIncome = true;
                    //BoolWithNotifyChanged isChecked;
                    //value.IsChecked.TryGetValue("de-gcd_genInfo.report.id.reportElement.reportElements.???", out isChecked);
                    //ExportAdjustmentOfIncome = (isChecked != null && isChecked.BoolValue == true);
                    ExportAdjustmentOfIncome = false;

                }
                else
                {
                    ShowAdjustmentOfIncome = false;
                }
            }
            // (Steuerliche Gewinnermittlung)
            {
                bool isPart = document.MainTaxonomy.RoleExists(ExportHelper.GetUri("determinationOfTaxableIncome", document));
                if (isPart)
                {
                    ShowDeterminationOfTaxableIncome = true;
                    BoolWithNotifyChanged isChecked;
                    value.IsChecked.TryGetValue("de-gcd_genInfo.report.id.reportElement.reportElements.SGE", out isChecked);
                    ExportDeterminationOfTaxableIncome = (isChecked != null && isChecked.BoolValue == true);

                }
                else
                {
                    ShowDeterminationOfTaxableIncome = false;
                }
            }

            // (Steuerliche Gewinnermittlung bei Personengesellschaften)
            {
                bool isPart = document.MainTaxonomy.RoleExists(ExportHelper.GetUri("determinationOfTaxableIncomeBusinessPartnership", document));
                if (isPart)
                {
                    ShowDeterminationOfTaxableIncomeBusinessPartnership = true;
                    BoolWithNotifyChanged isChecked;
                    value.IsChecked.TryGetValue("de-gcd_genInfo.report.id.reportElement.reportElements.SGEP", out isChecked);
                    ExportDeterminationOfTaxableIncomeBusinessPartnership = (isChecked != null && isChecked.BoolValue == true);

                }
                else
                {
                    ShowDeterminationOfTaxableIncomeBusinessPartnership = false;
                }
            }

            // (Steuerliche Gewinnermittlung für besondere Fälle)
            {
                bool isPart = document.MainTaxonomy.RoleExists(ExportHelper.GetUri("determinationOfTaxableIncomeSpecialCases", document));
                if (isPart)
                {
                    ShowDeterminationOfTaxableIncomeSpecialCases = true;
                    BoolWithNotifyChanged isChecked;
                    value.IsChecked.TryGetValue("de-gcd_genInfo.report.id.reportElement.reportElements.SGEB", out isChecked);
                    ExportDeterminationOfTaxableIncomeSpecialCases = (isChecked != null && isChecked.BoolValue == true);

                }
                else
                {
                    ShowDeterminationOfTaxableIncomeSpecialCases = false;
                }
            }

            // Eigenkapitalspiegel           
            if (document.ExistsHyperCube("de-gaap-ci_table.eqCh"))
            {
                BoolWithNotifyChanged isChecked;
                value.IsChecked.TryGetValue("de-gcd_genInfo.report.id.reportElement.reportElements.EKE", out isChecked);
                HasHypercubes = true;
                ShowEquityStatement = true;
                ExportEquityStatement = (isChecked != null && isChecked.BoolValue == true);
            }
            else
            {
                ShowEquityStatement = false;
                ExportEquityStatement = false;
            }

            // Anlagespiegel (brutto)
            if (document.ExistsHyperCube("de-gaap-ci_table.nt.ass.gross"))
            {
                BoolWithNotifyChanged isChecked1;
                BoolWithNotifyChanged isChecked2;
                value.IsChecked.TryGetValue("de-gcd_genInfo.report.id.reportElement.reportElements.BAL", out isChecked1);
                value.IsChecked.TryGetValue("de-gcd_genInfo.report.id.reportElement.reportElements.BAL", out isChecked2);
                HasHypercubes = true;
                ShowNtAssGross = true;
                ExportNtAssGross = (isChecked1 != null && isChecked1.BoolValue == true) ||
                                   (isChecked2 != null && isChecked2.BoolValue == true);
            }
            else
            {
                ShowNtAssGross = false;
                ExportNtAssGross = false;
            }

            // Anlagespiegel (brutto), Kurzform
            if (document.ExistsHyperCube("de-gaap-ci_table.nt.ass.gross_short"))
            {
                BoolWithNotifyChanged isChecked1;
                BoolWithNotifyChanged isChecked2;
                value.IsChecked.TryGetValue("de-gcd_genInfo.report.id.reportElement.reportElements.BAL", out isChecked1);
                value.IsChecked.TryGetValue("de-gcd_genInfo.report.id.reportElement.reportElements.BAL", out isChecked2);
                HasHypercubes = true;
                ShowNtAssGrossShort = true;
                ExportNtAssGrossShort = (isChecked1 != null && isChecked1.BoolValue == true) ||
                                        (isChecked2 != null && isChecked2.BoolValue == true);
            }
            else
            {
                ShowNtAssGrossShort = false;
                ExportNtAssGrossShort = false;
            }

            // Anlagespiegel (netto)
            if (document.ExistsHyperCube("de-gaap-ci_table.nt.ass.net"))
            {
                BoolWithNotifyChanged isChecked;
                value.IsChecked.TryGetValue("de-gcd_genInfo.report.id.reportElement.reportElements.NA", out isChecked);
                HasHypercubes = true;
                ShowNtAssNet = true;
                ExportNtAssNet = (isChecked != null && isChecked.BoolValue == true);
            }
            else
            {
                ShowNtAssNet = false;
                ExportNtAssNet = false;
            }
        }

        public ConfigExport(Document document)
        {
            InitConfig(document);
            UserManager.Instance.CurrentUser.Options.GetPdfExportOptionsFromDatabase(this);
        }

        #region ExportTypeInfos
        private static readonly Dictionary<ExportTypes, ExportTypeInfo> ExportTypeInfos =
            new Dictionary<ExportTypes, ExportTypeInfo> {
                {
                    ExportTypes.Pdf,
                    new ExportTypeInfo(
                        ExportTypes.Pdf,
                        ResourcesExport.ExportLabelPdf,
                        ResourcesExport.ExportDescriptionPdf,
                        ExportTypeInfo.ExportDestinationTypes.File,
                        ResourcesCommon.FileFilterPdf)
                    },
                {
                    ExportTypes.Csv,
                    new ExportTypeInfo(
                        ExportTypes.Csv, 
                        ResourcesExport.ExportLabelCsv,
                        ResourcesExport.ExportDescriptionCsv,
                        ExportTypeInfo.ExportDestinationTypes.Directory, 
                        ResourcesCommon.FileFilterCsv)
                    },
                {
                    ExportTypes.Xbrl,
                    new ExportTypeInfo(
                        ExportTypes.Xbrl, 
                        ResourcesExport.ExportLabelXbrl,
                        ResourcesExport.ExportDescriptionXbrl, 
                        ExportTypeInfo.ExportDestinationTypes.File,
                        ResourcesCommon.FileFilterXbrl)
                    }
            };

        public static ExportTypeInfo ExportTypeInfoPdf { get { return ExportTypeInfos[ExportTypes.Pdf]; } }
        public static ExportTypeInfo ExportTypeInfoCsv { get { return ExportTypeInfos[ExportTypes.Csv]; } }
        public static ExportTypeInfo ExportTypeInfoXbrl { get { return ExportTypeInfos[ExportTypes.Xbrl]; } }
        #endregion ExportTypeInfos

        #region properties

        public Document Document { get; private set; }

        #region ExportType
        private ExportTypes _exportType;

        public ExportTypes ExportType
        {
            get { return _exportType; }
            set
            {
                _exportType = value;
                switch (ExportType)
                {
                    case ExportTypes.Pdf:
                        Filename = FilePath + "\\" + DefaultFilename;
                        break;

                    case ExportTypes.Csv:
                        Filename = FilePath + "\\";
                        break;

                    case ExportTypes.Xbrl:
                        Filename = FilePath + "\\" + DefaultFilename.Replace(".pdf", ".xml");
                        break;
                }

                OnPropertyChanged("ExportType");
                ExportInfo = ExportTypeInfos[ExportType];
            }
        }
        #endregion ExportType

        #region ExportInfo
        private ExportTypeInfo _exportInfo;

        public ExportTypeInfo ExportInfo
        {
            get { return _exportInfo; }
            private set
            {
                _exportInfo = value;
                OnPropertyChanged("ExportInfo");
            }
        }
        #endregion ExportInfo

        #region DefaultFilename
        public string DefaultFilename
        {
            get {
                //string filename =
                //    Document.Company.Name + "_" +
                //    Document.FinancialYear.FYear + "_" +
                //    Document.Name +
                //    (ExportType == ExportTypes.Pdf
                //        ? ".pdf"
                //        : ExportType == ExportTypes.Xbrl ? ".xml" : "");

                //string regex = "[" + String.Join(",", Path.GetInvalidFileNameChars()) + "]";
                //filename = Regex.Replace(filename, @"" + regex + "", "_");
                //return filename;
                return GetDefaultBasename(Document) + (ExportType == ExportTypes.Pdf
                        ? ".pdf"
                        : ExportType == ExportTypes.Xbrl ? ".xml" : "");
            }
        }
        #endregion DefaultFilename

        #region Filename
        private string _filename;

        public string Filename
        {
            get { return _filename; }
            set {
                //System.IO.Path.CheckInvalidPathChars(value);
                //Path.GetFileNameWithoutExtension(value)
                //var chars = value.ToCharArray();
                //var cleanedChars = from c in chars where !Path.GetInvalidFileNameChars().Contains(c) select c;
                //value = cleanedChars.ToString();

                //FileInfo fileInfo;
                //try {
                //    value = Export.ExportHelper.GetValidFilePath(value) + @"\" + Export.ExportHelper.GetValidFileName(value);
                //    fileInfo = new FileInfo(value);
                //_filename = fileInfo.FullName;
                //FilePath = fileInfo.DirectoryName;

                //if (ExportHelper.IsValidFileDestination(value)) {
                //    var fileInfo = new FileInfo(value);
                //    _filename = fileInfo.FullName;
                //    FilePath = fileInfo.DirectoryName;
                //} else {
                //    _filename = value;
                //    FilePath = value.Substring(0, value.LastIndexOf('\\'));
                //}

                FilePath = ExportHelper.GetValidFilePath(value, true);
                _filename = ExportHelper.GetFileDestination(value);
                //var fileName = ExportHelper.GetValidFileName(value);
                //if (string.IsNullOrEmpty(fileName) || fileName.Equals(FilePath)) {
                //    _filename = FilePath;
                //}
                //else if (string.IsNullOrEmpty(FilePath)) {
                //    _filename = fileName;
                //}
                //else {
                //    _filename = FilePath + @"\" + fileName;
                //}
                OnPropertyChanged("Filename");
                OnPropertyChanged("FileExists");
                //}
                //catch (Exception) {
                //    char[] disallowedCharsDirectory = {
                //        ':', '+', '#', '\'', '/', ';', '!', '?', '%', '^', '`', '=', '~', '<',
                //        '>', '|', ',', '"'
                //    };
                //    string regexSearch = new string(disallowedCharsDirectory);
                //    Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                //    var validFileName = r.Replace(value, "_");
                //    Filename = validFileName;
                //}

            }
        }

        #endregion Filename

        #region FilePath
        private string _filePath;

        public string FilePath
        {
            get
            {
                if (_filePath == null)
                    FilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                return _filePath;
            }
            set
            {
                _filePath = value.EndsWith("\\") ? value.Substring(0, value.LastIndexOf('\\')) : value;
                OnPropertyChanged("FilePath");
                OnPropertyChanged("FileExists");
            }
        }
        #endregion FilePath

        #region FileExists
        public bool FileExists
        {
            get
            {
                return ExportInfo.ExportDestinationType == ExportTypeInfo.ExportDestinationTypes.File &&
                       File.Exists(Filename);
            }
        }
        #endregion FileExists

        #region report parts config

        public bool ExportCompanyInformation { get; set; }
        public bool ExportReportInformation { get; set; }
        public bool ExportDocumentInformation { get; set; }
        public bool ExportAccountBalances { get; set; }
        public bool ExportUnassignedAccounts { get; set; }

        #region OtherReportElements
        public bool ExportOtherReportElements { get; set; }
        public bool ShowOtherReportElements { get; set; }
        #endregion

        #region BalanceSheet

        public bool ExportBalanceSheet { get; set; }
        public bool ShowBalanceSheet { get; set; }
        #endregion BalanceSheet

        #region Notes
        public bool ExportNotes { get; set; }
        public bool ShowNotes { get; set; }
        #endregion

        #region IncomeStatement
        public bool ExportIncomeStatement { get; set; }
        public bool ShowIncomeStatement { get; set; }
        #endregion IncomeStatement

        #region AppropriationProfit
        public bool ExportAppropriationProfit { get; set; }
        public bool ShowAppropriationProfit { get; set; }
        #endregion AppropriationProfit

        #region ContingentLiabilities
        public bool ShowContingentLiabilities { get; set; }
        public bool ExportContingentLiabilities { get; set; }
        #endregion ContingentLiabilities

        #region CashFlowStatement
        public bool ShowCashFlowStatement { get; set; }
        public bool ExportCashFlowStatement { get; set; }
        #endregion CashFlowStatement

        #region ManagementReport
        public bool ShowManagementReport { get; set; }
        public bool ExportManagementReport { get; set; }
        #endregion ManagementReport

        #region AdjustmentOfIncome
        public bool ExportAdjustmentOfIncome { get; set; }
        public bool ShowAdjustmentOfIncome { get; set; }
        #endregion AdjustmentOfIncome

        #region DeterminationOfTaxableIncome
        public bool ExportDeterminationOfTaxableIncome { get; set; }
        public bool ShowDeterminationOfTaxableIncome { get; set; }
        #endregion DeterminationOfTaxableIncome

        #region DeterminationOfTaxableIncomeBusinessPartnership
        public bool ExportDeterminationOfTaxableIncomeBusinessPartnership { get; set; }
        public bool ShowDeterminationOfTaxableIncomeBusinessPartnership { get; set; }
        #endregion DeterminationOfTaxableIncomeBusinessPartnership

        #region DeterminationOfTaxableIncomeSpecialCases
        public bool ExportDeterminationOfTaxableIncomeSpecialCases { get; set; }
        public bool ShowDeterminationOfTaxableIncomeSpecialCases { get; set; }
        #endregion DeterminationOfTaxableIncomeSpecialCases

        #region Hypercubes
        public bool ExportNtAssNet { get; set; }
        public bool ShowNtAssNet { get; set; }

        public bool ExportNtAssGrossShort { get; set; }
        public bool ShowNtAssGrossShort { get; set; }

        public bool ExportNtAssGross { get; set; }
        public bool ShowNtAssGross { get; set; }

        public bool ExportEquityStatement { get; set; }
        public bool ShowEquityStatement { get; set; }
        #endregion Hypercubes

        #endregion report parts config

        #region common config
        public bool ExportComments { get; set; }
        public bool ExportNILValues { get; set; }
        public bool ExportMandatoryOnly { get; set; }
        public bool ExportReconciliationInfo { get; set; }
        public bool ExportAccounts { get; set; }
        #endregion common config

        public bool IsCommercialBalanceSheet { get; private set; }
        public bool HasHypercubes { get; private set; }

        #endregion properties

        public static string GetDefaultBasename(Document document) {
            string filename =
                document.Company.Name + "_" +
                document.FinancialYear.FYear + "_" +
                document.Name;
            string regex = "[" + String.Join(",", Path.GetInvalidFileNameChars()) + "]";
            filename = Regex.Replace(filename, @"" + regex + "", "_");
            return filename;
        }


        //// unsused?!
        //public bool ExportChangesEquityStatement { get; set; }
        //public bool ExportChangesEquityAccounts { get; set; }

        //public bool ExportNotes { get; set; }
        //public bool ShowNotes { get; set; }

        //public bool ExportTransfersCommercialCodeToTax { get; set; }
        //public bool ExportOtherReportElements { get; set; }
        //public bool ExportDetailedInformation { get; set; }


        //    /*
        //    try {
        //        bool isPart = document.MainTaxonomy.RoleExists(ExportHelper.GetUri("appropriationProfits", document));
        //        if (isPart) {
        //            PdfConfig.ShowAppropriationProfit = System.Windows.Visibility.Visible;
        //            PdfConfig.ExportAppropriationProfit = true;
        //        } else {
        //            PdfConfig.ShowAppropriationProfit = System.Windows.Visibility.Collapsed;
        //            PdfConfig.ExportAppropriationProfit = false;
        //    }

        //    } catch (Exception) {

        //        PdfConfig.ShowAppropriationProfit = System.Windows.Visibility.Collapsed;
        //        PdfConfig.ExportAppropriationProfit = false;
        //    }
        //    */


        //    /*
        //    try {
        //        bool isPart = document.MainTaxonomy.RoleExists(ExportHelper.GetUri("contingentLiabilities", document));
        //        if (isPart) {
        //            PdfConfig.ShowContingentLiabilities = System.Windows.Visibility.Visible;
        //            PdfConfig.ExportContingentLiabilities = true;
        //        } else {
        //            PdfConfig.ShowContingentLiabilities = System.Windows.Visibility.Collapsed;
        //            PdfConfig.ExportContingentLiabilities = false;
        //    }

        //    } catch (Exception) {

        //        PdfConfig.ShowContingentLiabilities = System.Windows.Visibility.Collapsed;
        //        PdfConfig.ExportContingentLiabilities = false;
        //    }
        //    */
        //    // To be continued (converting)

        //    try {
        //        bool isPart = document.MainTaxonomy.RoleExists(ExportHelper.GetUri("notes", document));

        //        if (isPart) {
        //            Config.ShowNotes = System.Windows.Visibility.Visible;
        //            Config.ExportNotes = true;
        //        } else {
        //            Config.ShowNotes = System.Windows.Visibility.Collapsed;
        //            Config.ExportNotes = false;
        //        }


        //    } catch (Exception) {

        //        Config.ShowNotes = System.Windows.Visibility.Collapsed;
        //        Config.ExportNotes = false;
        //    }

    }

}