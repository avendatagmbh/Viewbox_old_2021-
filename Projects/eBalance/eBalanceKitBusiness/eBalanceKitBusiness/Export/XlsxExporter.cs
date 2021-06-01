// --------------------------------------------------------------------------------
// author: Márton Garai
// since: 2012-10-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using Taxonomy;
using Taxonomy.Enums;
using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKitBusiness.Manager;
using eBalanceKitResources.Localisation;
using Application = Microsoft.Office.Interop.Excel.Application;
using Style = Microsoft.Office.Interop.Excel.Style;
#if DEBUG
using Utils.Commands;
#endif

namespace eBalanceKitBusiness.Export {
    internal class XlsxExporter {
        /// <summary>
        /// exports the visualization of a taxonomy into xlsx file. Needs excel to be installed for that function.
        /// </summary>
        /// <param name="path">output file name</param>
        /// <param name="taxonomyType">the given taxonomy type. The column order of visualization can be changed by default value if taxonomyType is not recognised</param>
        /// <param name="selectedVersion">the given version. The column order of visualization can be changed by default value if version is not recognised</param>
        /// <returns>errors</returns>
        public string Export(string path, TaxonomyType taxonomyType, string selectedVersion) {
            _summationHyperLinkList = new List<Tuple<int, string, string>>();
            _sameElementHyperLinkList = new List<Tuple<int, int, int, string>>();
            _elementMap = new Dictionary<string, Tuple<int, int>>();
            _thickColumnCount = 0;
            RegistryKey key = Registry.ClassesRoot;
            if (key.OpenSubKey("Excel.Application") == null)
                return ResourcesExport.NoExcelInstalled;
            if (MessageBox.Show(ResourcesExport.TakeSomeTime, ResourcesExport.TakeSomeTimeCaption, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return null;
            Exception exception;
            Utils.FileHelper.IsFileBeingUsed(path, out exception);
            if (exception != null) {
                return ResourcesCommon.FileInUse;
            }
            try {
                ExportToXlsx(path, taxonomyType, selectedVersion);
            } catch (Exception ex) {
                return ex.Message;
            }
            return null;
        }

#if DEBUG
        // testing purposes. Check the output with a clearly ok taxonomy visualization
        public XlsxExporter() {
            TestTaxonomiesCommand = new DelegateCommand(o => true, TestTaxonomies);
        }

        public DelegateCommand TestTaxonomiesCommand { get; set; }

        private const string FromColumn = "H";

        private const string ToColumn = "H";

        private List<IElement> _summationItemNotInTree;

        private void TestTaxonomies(object parameter) {
            OpenFileDialog open1 = new OpenFileDialog {Title = "The bigger file"};
            open1.ShowDialog();
            OpenFileDialog open2 = new OpenFileDialog {Title = "The smaller file"};
            bool? result = open2.ShowDialog();
            if (result.Value) {
                string fromExcelPath = open1.FileName;
                string toExcelPath = open2.FileName;
                Exception exception;
                Utils.FileHelper.IsFileBeingUsed(fromExcelPath, out exception);
                if (exception != null) {
                    MessageBox.Show("file in use");
                    return;
                }
                Utils.FileHelper.IsFileBeingUsed(toExcelPath, out exception);
                if (exception != null) {
                    MessageBox.Show("file in use");
                    return;
                }
                Application xlApp = new Application { ScreenUpdating = false };//, DisplayAlerts = false, Visible = false, UserControl = false, Interactive = false};
                Workbooks workbooks = xlApp.Workbooks;
                Workbook workbookFrom = workbooks.Open(fromExcelPath, 0, true, 5, string.Empty, string.Empty, true, XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                Workbook workbookTo = workbooks.Open(toExcelPath, 0, true, 5, string.Empty, string.Empty, true, XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                Sheets sheets = workbookFrom.Sheets;
                Worksheet sheet = sheets[1];
                Range usedRange = sheet.UsedRange;
                Range rows = usedRange.Rows;
                int rowCount = rows.Count;
                Range actualRange = usedRange.get_Range(FromColumn + 1, FromColumn + rowCount);
                Object[,] data = actualRange.Value2;
                List<string> fromList = new List<string>();
                foreach (string label in data) {
                    fromList.Add(label);
                }
                ReleaseObject(actualRange);
                ReleaseObject(rows);
                ReleaseObject(usedRange);
                ReleaseObject(sheet);
                ReleaseObject(sheets);
                sheets = workbookTo.Sheets;
                sheet = sheets[1];
                usedRange = sheet.UsedRange;
                rows = usedRange.Rows;
                rowCount = rows.Count;
                actualRange = usedRange.get_Range(ToColumn + 1, ToColumn + rowCount);
                data = actualRange.Value2;
                List<string> leftOver = new List<string>();
                foreach (string label in data) {
                    if (!fromList.Remove(label)) {
                        leftOver.Add(label);
                    }
                }
                ReleaseObject(actualRange);
                ReleaseObject(rows);
                ReleaseObject(usedRange);
                ReleaseObject(sheet);
                ReleaseObject(sheets);
                workbookTo.Close(true, Type.Missing, Type.Missing);
                ReleaseObject(workbookTo);
                workbookFrom.Close(true, Type.Missing, Type.Missing);
                ReleaseObject(workbookFrom);
                ReleaseObject(workbooks);
                xlApp.Quit();
                ReleaseObject(xlApp);
                // check fromList
                Debugger.Break();
            }
        }
#endif

        private const string DefaultVersion = "2012-06-01";

        private const TaxonomyType DefaultTaxonomyType = TaxonomyType.OtherBusinessClass;

        // first item in values is row count second is presentation tree column delta.
        private Dictionary<string, Tuple<int, int>> _elementMap;

        private int _thickColumnCount;

        // first item is version, second item is a mapping : taxonomy type to which column should be indented.
        private readonly Dictionary<string, Dictionary<TaxonomyType, int>> _indentedColumnIndexByTaxonomyByVersion =
            new Dictionary<string, Dictionary<TaxonomyType, int>> {
                {
                    "2012-06-01",
                    new Dictionary<TaxonomyType, int> {
                        {TaxonomyType.GCD, 8},
                        {TaxonomyType.GAAP, 8},
                        {TaxonomyType.Financial, 8},
                        {TaxonomyType.OtherBusinessClass, 8},
                        {TaxonomyType.Insurance, 8}
                    }
                    }
            };

        // first item is version, second item is taxonomy type, third item is mapping for which column should be where.
        // the value can be changed by CheckShifting, if we want to check if there are pasted columns in the table.
        private readonly Dictionary<string, Dictionary<TaxonomyType, Dictionary<string, int>>>
            _columnIndexByColumnNameByTaxonomyTypeByVersion =
                new Dictionary<string, Dictionary<TaxonomyType, Dictionary<string, int>>> {
                    {
                        "2012-06-01",
                        new Dictionary<TaxonomyType, Dictionary<string, int>>
                        {
                            {
                                TaxonomyType.GCD,
                                new Dictionary<string, int> {
                                    {"Comment", 5},
                                    {"Section", 6},
                                    {"Level", 7},
                                    {"LabelAlternative", 8},
                                    {"NameSpace", 9},
                                    {"Name", 10},
                                    {"IsAbstract", 11},
                                    {"XbrlType", 12},
                                    {"Restriction", 13},
                                    {"PeriodType", 14},
                                    {"Balance", 15},
                                    {"Nillable", 16},
                                    {"Item", 17},
                                    {"MinOccurs", 18},
                                    {"MaxOccurs", 19},
                                    {"Content", 20},
                                    {"TerseLabel (de)", 21},
                                    {"TerseLabel (en)", 22},
                                    {"StandardLabel (de)", 23},
                                    {"StandardLabel (en)", 24},
                                    {"Documentation (de)", 25},
                                    {"Documentation (en)", 26},
                                    {"FiscalRequierement", 27},
                                    {"NotPermittedFor", 28},
                                    {"ConsistencyCheck", 29},
                                    {"ReferenceName", 30},
                                    {"Number", 31},
                                    {"Paragraph", 32},
                                    {"Subparagraph", 33},
                                    {"Clause", 34}
                                }
                                },
                            {
                                TaxonomyType.GAAP,
                                new Dictionary<string, int> {
                                    {"Comment", 5},
                                    {"Section", 6},
                                    {"Level", 7},
                                    {"LabelAlternative", 8},
                                    {"NameSpace", 9},
                                    {"Name", 10},
                                    {"IsAbstract", 11},
                                    {"XbrlType", 12},
                                    {"Restriction", 13},
                                    {"PeriodType", 14},
                                    {"Balance", 15},
                                    {"Nillable", 16},
                                    {"Item", 17},
                                    {"MinOccurs", 18},
                                    {"MaxOccurs", 19},
                                    {"Content", 20},
                                    {"TerseLabel (de)", 21},
                                    {"TerseLabel (en)", 22},
                                    {"StandardLabel (de)", 23},
                                    {"StandardLabel (en)", 24},
                                    {"PositiveTerseLabel (de)", 25},
                                    {"PositiveTerseLabel (en)", 26},
                                    {"PositiveLabel (de)", 27},
                                    {"PositiveLabel (en)", 28},
                                    {"NegativeTerseLabel (de)", 29},
                                    {"NegativeTerseLabel (en)", 30},
                                    {"NegativeLabel (de)", 31},
                                    {"NegativeLabel (en)", 32},
                                    {"PeriodStartLabel (de)", 33},
                                    {"PeriodStartLabel (en)", 34},
                                    {"PeriodEndLabel (de)", 35},
                                    {"PeriodEndLabel (en)", 36},
                                    {"Documentation (de)", 37},
                                    {"Documentation (en)", 38},
                                    {"DefinitionGuidance (de)", 39},
                                    {"DefinitionGuidance (en)", 40},
                                    {"FiscalReference", 41},
                                    {"FiscalRequierement", 42},
                                    {"NotPermittedFor", 43},
                                    {"LegalFormEU", 44},
                                    {"LegalFormKSt", 45},
                                    {"LegalFormPG", 46},
                                    {"TypeOperatingResult", 47},
                                    {"ConsistencyCheck", 48},
                                    {"FiscalValidSince", 49},
                                    {"FiscalValidThrough", 50},
                                    {"ValidSince", 51},
                                    {"ValidThrough", 52},
                                    {"ReferenceName", 53},
                                    {"Number", 54},
                                    {"IssueDate", 55},
                                    {"Article", 56},
                                    {"Note", 57},
                                    {"ReferenceSection", 58},
                                    {"Paragraph", 59},
                                    {"Subparagraph", 60},
                                    {"Clause", 61},
                                    {"SubClause", 62}
                                }
                                },
                            {
                                TaxonomyType.Financial,
                                new Dictionary<string, int> {
                                    {"Comment", 5},
                                    {"Section", 6},
                                    {"Level", 7},
                                    {"LabelAlternative", 8},
                                    {"NameSpace", 9},
                                    {"Name", 10},
                                    {"IsAbstract", 11},
                                    {"XbrlType", 12},
                                    {"Restriction", 13},
                                    {"PeriodType", 14},
                                    {"Balance", 15},
                                    {"Nillable", 16},
                                    {"Item", 17},
                                    {"MinOccurs", 18},
                                    {"MaxOccurs", 19},
                                    {"Content", 20},
                                    {"TerseLabel (de)", 21},
                                    {"TerseLabel (en)", 22},
                                    {"StandardLabel (de)", 23},
                                    {"StandardLabel (en)", 24},
                                    {"PositiveTerseLabel (de)", 25},
                                    {"PositiveTerseLabel (en)", 26},
                                    {"PositiveLabel (de)", 27},
                                    {"PositiveLabel (en)", 28},
                                    {"NegativeTerseLabel (de)", 29},
                                    {"NegativeTerseLabel (en)", 30},
                                    {"NegativeLabel (de)", 31},
                                    {"NegativeLabel (en)", 32},
                                    {"PeriodStartLabel (de)", 33},
                                    {"PeriodStartLabel (en)", 34},
                                    {"PeriodEndLabel (de)", 35},
                                    {"PeriodEndLabel (en)", 36},
                                    {"Documentation (de)", 37},
                                    {"Documentation (en)", 38},
                                    {"FiscalReference", 39},
                                    {"FiscalRequierement", 40},
                                    {"NotPermittedFor", 41},
                                    // TODO: there are some true values which shouldn't be there. For example in taxonomy type fi, version 2012-06, line 164.
                                    {"LegalFormEU", 42},
                                    {"LegalFormKSt", 43},
                                    {"LegalFormPG", 44},
                                    {"TypeOperatingResult", 45},
                                    {"ConsistencyCheck", 46},
                                    {"FiscalValidSince", 47},
                                    {"FiscalValidThrough", 48},
                                    {"ValidSince", 49},
                                    {"ValidThrough", 50},
                                    {"Publisher", 51},
                                    {"ReferenceName", 52},
                                    {"Number", 53},
                                    {"IssueDate", 54},
                                    {"Chapter", 55},
                                    {"Article", 56},
                                    {"Note", 57},
                                    {"ReferenceSection", 58},
                                    {"SubSection", 59},
                                    {"Paragraph", 60},
                                    {"Subparagraph", 61},
                                    {"Clause", 62},
                                    {"SubClause", 63},
                                    {"KindOfFinancialInstitutionKreditgenossenschWarengeschaeft", 64},
                                    {"KindOfFinancialInstitutionFinanzdienstl", 65},
                                    {"KindOfFinancialInstitutionPfandbriefbanken", 66},
                                    {"KindOfFinancialInstitutionBauspar", 67},
                                    {"KindOfFinancialInstitutionKreditgenossensch", 68},
                                    {"KindOfFinancialInstitutionSparkassen", 69},
                                    {"KindOfFinancialInstitutionKapitalanlagegesellschaften", 70},
                                    {"KindOfFinancialInstitutionSkontrofuehrer", 71},
                                    {"KindOfFinancialInstitutionPfandbriefbankenOERA", 72},
                                    {"KindOfFinancialInstitutionGirozentralen", 73},
                                    {"KindOfFinancialInstitutiongenossZentrB", 74},
                                    {"KindOfFinancialInstitutionDtGenB", 75},
                                    {"KindOfFinancialInstitutionPfandBG", 76},
                                }
                                },
                            {
                                TaxonomyType.OtherBusinessClass,
                                new Dictionary<string, int> {
                                    {"Comment", 5},
                                    {"Section", 6},
                                    {"Level", 7},
                                    {"LabelAlternative", 8},
                                    {"NameSpace", 9},
                                    {"Name", 10},
                                    {"IsAbstract", 11},
                                    {"XbrlType", 12},
                                    {"Restriction", 13},
                                    {"PeriodType", 14},
                                    {"Balance", 15},
                                    {"Nillable", 16},
                                    {"Item", 17},
                                    {"MinOccurs", 18},
                                    {"MaxOccurs", 19},
                                    {"Content", 20},
                                    {"TerseLabel (de)", 21},
                                    {"TerseLabel (en)", 22},
                                    {"StandardLabel (de)", 23},
                                    {"StandardLabel (en)", 24},
                                    {"PositiveTerseLabel (de)", 25},
                                    {"PositiveTerseLabel (en)", 26},
                                    {"PositiveLabel (de)", 27},
                                    {"PositiveLabel (en)", 28},
                                    {"NegativeTerseLabel (de)", 29},
                                    {"NegativeTerseLabel (en)", 30},
                                    {"NegativeLabel (de)", 31},
                                    {"NegativeLabel (en)", 32},
                                    {"PeriodStartLabel (de)", 33},
                                    {"PeriodStartLabel (en)", 34},
                                    {"PeriodEndLabel (de)", 35},
                                    {"PeriodEndLabel (en)", 36},
                                    {"Documentation (de)", 37},
                                    {"Documentation (en)", 38},
                                    {"DefinitionGuidance (de)", 39},
                                    {"DefinitionGuidance (en)", 40},
                                    {"FiscalReference", 41},
                                    {"FiscalRequierement", 42},
                                    {"NotPermittedFor", 43},
                                    {"LegalFormEU", 44},
                                    {"LegalFormKSt", 45},
                                    {"LegalFormPG", 46},
                                    {"TypeOperatingResult", 47},
                                    {"ConsistencyCheck", 48},
                                    {"FiscalValidSince", 49},
                                    {"FiscalValidThrough", 50},
                                    {"KeineBranche", 51},
                                    {"PBV", 52},
                                    {"KHBV", 53},
                                    {"EBV", 54},
                                    {"WUV", 55},
                                    {"VUV", 56},
                                    {"LUF", 57},
                                    {"ValidSince", 58},
                                    {"ValidThrough", 59},
                                    {"ReferenceName", 60},
                                    {"Number", 61},
                                    {"IssueDate", 62},
                                    {"Article", 63},
                                    {"Note", 64},
                                    {"ReferenceSection", 65},
                                    {"Paragraph", 66},
                                    {"Subparagraph", 67},
                                    {"Clause", 68},
                                    {"SubClause", 69},
                                }
                                },
                            {
                                TaxonomyType.Insurance,
                                new Dictionary<string, int> {
                                    {"Comment", 5},
                                    {"Section", 6},
                                    {"Level", 7},
                                    {"LabelAlternative", 8},
                                    {"NameSpace", 9},
                                    {"Name", 10},
                                    {"IsAbstract", 11},
                                    {"XbrlType", 12},
                                    {"Restriction", 13},
                                    {"PeriodType", 14},
                                    {"Balance", 15},
                                    {"Nillable", 16},
                                    {"Item", 17},
                                    {"MinOccurs", 18},
                                    {"MaxOccurs", 19},
                                    {"Content", 20},
                                    {"TerseLabel (de)", 21},
                                    {"TerseLabel (en)", 22},
                                    {"StandardLabel (de)", 23},
                                    {"StandardLabel (en)", 24},
                                    {"PositiveTerseLabel (de)", 25},
                                    {"PositiveTerseLabel (en)", 26},
                                    {"PositiveLabel (de)", 27},
                                    {"PositiveLabel (en)", 28},
                                    {"NegativeTerseLabel (de)", 29},
                                    {"NegativeTerseLabel (en)", 30},
                                    {"NegativeLabel (de)", 31},
                                    {"NegativeLabel (en)", 32},
                                    {"PeriodStartLabel (de)", 33},
                                    {"PeriodStartLabel (en)", 34},
                                    {"PeriodEndLabel (de)", 35},
                                    {"PeriodEndLabel (en)", 36},
                                    {"Documentation (de)", 37},
                                    // TODO: check what is wrong in line 485. The en documentation is the same as de doc. Shouldn't be any there.
                                    {"Documentation (en)", 38},
                                    {"FiscalReference", 39},
                                    {"FiscalRequierement", 40},
                                    {"NotPermittedFor", 41},
                                    {"LegalFormEU", 42},
                                    {"LegalFormKSt", 43},
                                    {"LegalFormPG", 44},
                                    {"LegalFormSEAG", 45},
                                    {"LegalFormVVaG", 46},
                                    {"LegalFormOerV", 47},
                                    {"LegalFormBNaU", 48},
                                    {"TypeOperatingResult", 49},
                                    {"ConsistencyCheck", 50},
                                    {"FiscalValidSince", 51},
                                    {"FiscalValidThrough", 52},
                                    {"KeineBranche", 53},
                                    {"ValidSince", 54},
                                    {"ValidThrough", 55},
                                    {"ReferenceName", 56},
                                    {"Number", 57},
                                    {"IssueDate", 58},
                                    {"Article", 59},
                                    {"Note", 60},
                                    {"ReferenceSection", 61},
                                    {"Paragraph", 62},
                                    {"Subparagraph", 63},
                                    {"Clause", 64},
                                    {"SubClause", 65},
                                    {"KindOfBusinessSUV", 66},
                                    {"KindOfBusinessRV", 67},
                                    {"KindOfBusinessLKV", 68},
                                    {"KindOfBusinessPSK", 69},
                                    {"KindOfBusinessLUV", 70},
                                    {"KindOfBusinessSUK", 71},
                                    {"KindOfBusinessPF", 72},
                                }
                                }
                        }
                        }
                };

        private Range _destinationRange;

        private int _shiftCount;

        private void ExportToXlsx(string path, TaxonomyType taxonomyType, string selectedVersion) {
            Application xlApp = new Application { ScreenUpdating = false, AlertBeforeOverwriting = false };//, DisplayAlerts = false, Visible = false, UserControl = false, Interactive = false};
            Workbooks workbooks = xlApp.Workbooks;
            Workbook workbook = workbooks.Add(Type.Missing);
            Sheets sheets = workbook.Sheets;
            Worksheet sheet = sheets.Cast<Worksheet>().First();
            Style style = sheet.UsedRange.Style;
            Range shortenedColumnsCells = sheet.UsedRange.get_Range("A1", "E1");
            Range shortenedColumns = shortenedColumnsCells.Columns;
            shortenedColumns.ColumnWidth = 0;
            ReleaseObject(shortenedColumns);
            ReleaseObject(shortenedColumnsCells);
            Range shortenedCell = sheet.UsedRange.get_Range("H1", "H1");
            shortenedCell.ColumnWidth = 50;
            ReleaseObject(shortenedCell);
            shortenedCell = sheet.UsedRange.get_Range("G1", "G1");
            shortenedCell.ColumnWidth = 4;
            ReleaseObject(shortenedCell);

            string version = selectedVersion;
            Dictionary<TaxonomyType, Dictionary<string, int>> temp;
            TaxonomyType actualTaxonomyType = taxonomyType;
            if (!_columnIndexByColumnNameByTaxonomyTypeByVersion.TryGetValue(version, out temp)) {
                // warning level error.
                version = DefaultVersion;
            }
            Dictionary<string, int> innerTemp;
            if (!_columnIndexByColumnNameByTaxonomyTypeByVersion[version].TryGetValue(actualTaxonomyType, out innerTemp)) {
                if (_columnIndexByColumnNameByTaxonomyTypeByVersion[DefaultVersion].TryGetValue(actualTaxonomyType, out innerTemp)) {
                    // warning level error.
                    version = DefaultVersion;
                } else {
                    // error level error.
                    actualTaxonomyType = DefaultTaxonomyType;
                    if (MessageBox.Show(ResourcesExport.CantUseTaxonomy, ResourcesCommon.ErrorsCaption, MessageBoxButton.YesNo) != MessageBoxResult.Yes) {
                        ReleaseObject(style);
                        ReleaseObject(sheet);
                        ReleaseObject(sheets);
                        if (File.Exists(path)) {
                            // TODO: not working XlSaveConflictResolution.xlLocalSessionChanges.
                            workbook.SaveAs(path, Type.Missing, XlFileFormat.xlExcel12, Type.Missing, Type.Missing, Type.Missing,
                                            XlSaveAsAccessMode.xlExclusive, XlSaveConflictResolution.xlLocalSessionChanges, Type.Missing, Type.Missing,
                                            Type.Missing, Type.Missing);
                            workbook.Close(false, path, Type.Missing);
                        } else {
                            workbook.Close(true, path, Type.Missing);
                        }
                        ReleaseObject(workbook);
                        ReleaseObject(workbooks);
                        xlApp.ScreenUpdating = true;
                        xlApp.Quit();
                        ReleaseObject(xlApp);
                        return;
                    }
                }
            }

            IEnumerable<Taxonomy.Interfaces.PresentationTree.IPresentationTree> presentationTrees =
                TaxonomyManager.GetTaxonomy(TaxonomyManager.GetTaxonomyInfo(taxonomyType, selectedVersion)).
                    PresentationTrees;
            // first element is row count the second is the number of which link we use.
            // DEVNOTE: uncomment all references for store which link should be bold.
            // Dictionary<int, int> linkNumberByRowCount = new Dictionary<int, int>();
            Dictionary<string, List<int>> linksRowByElementId = new Dictionary<string, List<int>>();

            _rowCount = 2;
            // first iteration through the presentation trees. Now we count how many extra columns are there, and calculate the size of data.
            // And check how many doubled elements are there.
// ReSharper disable PossibleMultipleEnumeration
            foreach (Taxonomy.Interfaces.PresentationTree.IPresentationTree presentationTree in presentationTrees) {
// ReSharper restore PossibleMultipleEnumeration
                foreach (IPresentationTreeNode presentationTreeNode in presentationTree.RootEntries) {
                    BuildLinkNumbers(ref linksRowByElementId, presentationTreeNode);
                    // BuildLinkNumbers(ref linkNumberByRowCount, ref linksRowByElementId, presentationTreeNode);
                }
            }
            int maxRowCount = _rowCount;

            // scrolling won't scroll the first row and the first n column. n is calculated
            Range freezeCenter =
                sheet.UsedRange.Cells[
                    2,
                    _indentedColumnIndexByTaxonomyByVersion[version][taxonomyType] +
                    _thickColumnCount + 1];
            freezeCenter.Activate();
            freezeCenter.Application.ActiveWindow.FreezePanes = true;
            ReleaseObject(freezeCenter);

            // the extra columns are inserted into the table. The columns on the right of the extra columns are shifted right with that amount.
            // all shift count is the amount of presentation trees + maximum of "same id" elements.
            _shiftCount =
                TaxonomyManager.GetTaxonomy(TaxonomyManager.GetTaxonomyInfo(taxonomyType, selectedVersion)).
                    PresentationTrees.Count;
            string tillString =
                GetTillStringFromNumber(_thickColumnCount +
                                        _indentedColumnIndexByTaxonomyByVersion[version][actualTaxonomyType] +
                                        _shiftCount);
            shortenedColumnsCells = sheet.UsedRange.get_Range("I1", tillString + "1");
            shortenedColumns = shortenedColumnsCells.Columns;
            shortenedColumns.ColumnWidth = 2;
            ReleaseObject(shortenedColumns);
            ReleaseObject(shortenedColumnsCells);
            int maxColumnCount =
                _columnIndexByColumnNameByTaxonomyTypeByVersion[version][actualTaxonomyType].Values.Max() + _shiftCount +
                _thickColumnCount;
            Object[,] data = (Object[,]) Array.CreateInstance(typeof (object), new[] {maxRowCount, maxColumnCount}, new[] {1, 1});
            Object[,] indentLevels = (Object[,]) Array.CreateInstance(typeof (object), new[] {maxRowCount, maxColumnCount}, new[] {1, 1});
            foreach (
                var columnIndexColumnNamePair in
                    _columnIndexByColumnNameByTaxonomyTypeByVersion[version][actualTaxonomyType]) {
                int index = columnIndexColumnNamePair.Value;
                CheckShifting(ref index, version, actualTaxonomyType);
                data[1, index] = columnIndexColumnNamePair.Key;
            }
            Type elementType = typeof (IElement);
            tillString = GetTillStringFromNumber(maxColumnCount);
            _destinationRange = sheet.get_Range("A1", tillString + maxRowCount);
            style.Font.Name = "Verdana";
            style.Font.Size = 8;
            int presentationTreeColumn = _thickColumnCount +
                    _indentedColumnIndexByTaxonomyByVersion[version][actualTaxonomyType] + 1;
            // second iteration on the presentation tree. Add the name of the role uri.
// ReSharper disable PossibleMultipleEnumeration
            foreach (Taxonomy.PresentationTree.PresentationTree tree in presentationTrees) {
// ReSharper restore PossibleMultipleEnumeration
                data[1, presentationTreeColumn] = tree.Role.Name + " [" +
                                tree.Role.RoleUri + "]";
                presentationTreeColumn++;
            }

            foreach (PropertyInfo info in elementType.GetProperties()) {
                Type propertyType = info.PropertyType;
                int i;
                if (
                    !_columnIndexByColumnNameByTaxonomyTypeByVersion[version][actualTaxonomyType].TryGetValue(
                        info.Name, out i))
                    continue;
                Type enumBaseType = propertyType.BaseType;
                _rowCount = 2;
                int presentationTreeColumnDelta = 1;
                // second iteration on the whole tree (every column iterate on the whole tree once) :
                // do main job. Fill the 'data' object
// ReSharper disable PossibleMultipleEnumeration
                foreach (Taxonomy.PresentationTree.PresentationTree tree in presentationTrees) {
// ReSharper restore PossibleMultipleEnumeration
                    foreach (IPresentationTreeNode root in tree.RootEntries) {
                        ExportPresentationNodeColumn(root, ref data, version, actualTaxonomyType, propertyType,
                                                     enumBaseType, info, 1, ref indentLevels, presentationTreeColumnDelta);
                    }
                    presentationTreeColumnDelta++;
                }
            }

            // n.th iteration on the whole tree (after the n column) :
            // add links
            _rowCount = 2;
#if DEBUG
            _summationItemNotInTree = new List<IElement>();
#endif
// ReSharper disable PossibleMultipleEnumeration
            foreach (Taxonomy.PresentationTree.PresentationTree tree in presentationTrees) {
// ReSharper restore PossibleMultipleEnumeration
                foreach (IPresentationTreeNode root in tree.RootEntries) {
                    AddLinksToOutput(root, linksRowByElementId);
                    // AddLinksToOutput(root, linksRowByElementId, linkNumberByRowCount, linksRowByElementId);
                }
            }

            ReleaseObject(style);
            for (int i = 1; i <= maxRowCount; i++) {
                for (int j = 1; j <= maxColumnCount; j++) {
                    if (data[i, j] == null) {
                        data[i, j] = " ";
                    }
                    bool? boolValue = data[i, j] as bool?;
                    if (boolValue != null) {
                        data[i, j] = boolValue.Value ? "true " : "false ";
                    }
                }
            }
            _destinationRange.Value = data;
            _destinationRange.WrapText = false;
            _destinationRange.IndentLevel = indentLevels;

            Hyperlinks links = _destinationRange.Hyperlinks;
            // the _summationHyperLinkList, and _sameElementHyperLinkList are builded in the AddLinksToOutput.
            foreach (Tuple<int, string, string> hyperLinkDescription in _summationHyperLinkList) {
                string position =
                    GetTillStringFromNumber(_elementMap[hyperLinkDescription.Item2].Item2 +
                                            _indentedColumnIndexByTaxonomyByVersion[version][actualTaxonomyType] +
                                            _thickColumnCount) +
                    hyperLinkDescription.Item1;
                Range linkRange = _destinationRange.get_Range(position, position);
                Hyperlink link = links.Add(linkRange, string.Empty,
                                                 GetTillStringFromNumber(_indentedColumnIndexByTaxonomyByVersion[version][actualTaxonomyType]) +
                                                 _elementMap[hyperLinkDescription.Item2].Item1, Type.Missing,
                                                 hyperLinkDescription.Item3);
                ReleaseObject(link);
                ReleaseObject(linkRange);
            }
            foreach (var sameElementHyperLink in _sameElementHyperLinkList) {
                string position =
                    GetTillStringFromNumber(_indentedColumnIndexByTaxonomyByVersion[version][actualTaxonomyType] + sameElementHyperLink.Item1) +
                    sameElementHyperLink.Item2;
                Range linkRange = _destinationRange.get_Range(position, position);
                Hyperlink link = links.Add(linkRange, string.Empty,
                                                 GetTillStringFromNumber(_indentedColumnIndexByTaxonomyByVersion[version][actualTaxonomyType]) +
                                                 sameElementHyperLink.Item3, Type.Missing,
                                                 sameElementHyperLink.Item4);
                ReleaseObject(link);
                ReleaseObject(linkRange);
            }
            // finalization of COM objects
            ReleaseObject(links);

            ReleaseObject(_destinationRange);
            ReleaseObject(sheet);
            ReleaseObject(sheets);
            if (File.Exists(path)) {
                // TODO: not working XlSaveConflictResolution.xlLocalSessionChanges.
                workbook.SaveAs(path, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                                XlSaveAsAccessMode.xlExclusive, XlSaveConflictResolution.xlLocalSessionChanges, Type.Missing, Type.Missing,
                                Type.Missing, Type.Missing);
                workbook.Close(false, path, Type.Missing);
            } else {
                workbook.Close(true, path, Type.Missing);
            }
            ReleaseObject(workbook);
            ReleaseObject(workbooks);
            xlApp.ScreenUpdating = true;
            xlApp.Quit();
            ReleaseObject(xlApp);
        }

        private void BuildLinkNumbers(ref Dictionary<string, List<int>> linksRowByElementId, IPresentationTreeNode pnode) {
        // private void BuildLinkNumbers(ref Dictionary<int, int> linkNumberByRowCount, ref Dictionary<string, List<int>> linksRowByElementId, IPresentationTreeNode pnode) {
            if (linksRowByElementId.ContainsKey(pnode.Element.Id)) {
                linksRowByElementId[pnode.Element.Id].Add(_rowCount);
                // linkNumberByRowCount[_rowCount] = linksRowByElementId[pnode.Element.Id].Count;
                if (_thickColumnCount < linksRowByElementId[pnode.Element.Id].Count) {
                    _thickColumnCount = linksRowByElementId[pnode.Element.Id].Count;
                }
            } else {
                linksRowByElementId[pnode.Element.Id] = new List<int> {_rowCount};
                if (_thickColumnCount < 1) {
                    _thickColumnCount = 1;
                }
                // linkNumberByRowCount[_rowCount] = 1;
            }

            if (pnode.Element.References.Count > 1) {
                _rowCount += pnode.Element.References.Count - 1;
            }
            _rowCount++;

            if (pnode.Element.Children.Count == 1 && pnode.Element.Children[0].Id.EndsWith(".head")) {
                List<IPresentationTreeNode> children = new List<IPresentationTreeNode>();
                foreach (IPresentationTreeNode child in pnode.Children) {
                    children.Add(child);
                }
                foreach (IElement eChild in pnode.Element.Children[0].Children) {
                    foreach (IPresentationTreeNode presentationTreeNode in eChild.PresentationTreeNodes) {
                        if (children.Contains(presentationTreeNode))
                            continue;
                        children.Add(presentationTreeNode);
                    }
                }
                foreach (IPresentationTreeNode pchild in children.OrderBy(t => t.Order)) {
                    BuildLinkNumbers(ref linksRowByElementId, pchild);
                    // BuildLinkNumbers(ref linkNumberByRowCount, ref linksRowByElementId, pchild);
                }
                return;
            }
            foreach (IPresentationTreeNode child in pnode.Children.OrderBy(t => t.Order)) {
                BuildLinkNumbers(ref linksRowByElementId, child);
                // BuildLinkNumbers(ref linkNumberByRowCount, ref linksRowByElementId, child);
            }
        }

        // First item is row count second is position number for element, third is the display value for the link.
        private List<Tuple<int, string, string>> _summationHyperLinkList;

        // First item is from column second is from row third is to row fourth is display value.
        private List<Tuple<int, int, int, string>> _sameElementHyperLinkList; 

        private void AddLinksToOutput(IPresentationTreeNode pnode, Dictionary<string, List<int>> linksRowByElementId) {
        // private void AddLinksToOutput(IPresentationTreeNode pnode, Dictionary<int, int> linkNumberByRowCount, Dictionary<string, List<int>> linksRowByElementId) {
            foreach (IElement element in pnode.Element.SummationSources) {
                if (_elementMap.ContainsKey(element.PositionNumber ?? element.Id)) {
                    SummationItem actualSummationItem = null;
                    foreach (SummationItem summationTarget in element.SummationTargets) {
                        if (pnode.Element.PositionNumber != null) {
                            if (summationTarget.Element.PositionNumber == pnode.Element.PositionNumber) {
                                actualSummationItem = summationTarget;
                            }
                        } else {
                            if (summationTarget.Element.Id == pnode.Element.Id) {
                                actualSummationItem = summationTarget;
                            }
                        }
                    }
                    Debug.Assert(actualSummationItem != null);
                    _summationHyperLinkList.Add(new Tuple<int, string, string>(_rowCount,
                                                                               element.PositionNumber ?? element.Id,
                                                                               actualSummationItem.CalculationArc.Weight >
                                                                               0
                                                                                   ? "+"
                                                                                   : "-"));
                } else {
#if DEBUG
                    _summationItemNotInTree.Add(element);
#endif
                }
            }
            int i = 1;
            foreach (int row in linksRowByElementId[pnode.Element.Id]) {
                _sameElementHyperLinkList.Add(new Tuple<int, int, int, string>(i,
                                                                               _rowCount, row,
                                                                               i.ToString(
                                                                                   CultureInfo.InvariantCulture)));
                i++;
            }

            if (pnode.Element.References.Count > 1) {
                _rowCount += pnode.Element.References.Count - 1;
            }
            _rowCount++;

            if (pnode.Element.Children.Count == 1 && pnode.Element.Children[0].Id.EndsWith(".head")) {
                List<IPresentationTreeNode> children = new List<IPresentationTreeNode>();
                foreach (IPresentationTreeNode child in pnode.Children) {
                    children.Add(child);
                }
                foreach (IElement eChild in pnode.Element.Children[0].Children) {
                    foreach (IPresentationTreeNode presentationTreeNode in eChild.PresentationTreeNodes) {
                        if (children.Contains(presentationTreeNode))
                            continue;
                        children.Add(presentationTreeNode);
                    }
                }
                foreach (IPresentationTreeNode pchild in children.OrderBy(t => t.Order)) {
                    AddLinksToOutput(pchild, linksRowByElementId);
                    // AddLinksToOutput(pchild, linkNumberByRowCount, linksRowByElementId);
                }
                return;
            }
            foreach (IPresentationTreeNode child in pnode.Children.OrderBy(t => t.Order)) {
                AddLinksToOutput(child, linksRowByElementId);
                // AddLinksToOutput(child, linkNumberByRowCount, linksRowByElementId);
            }
        }

        /// <summary>
        /// gets the name of the column in excel from a number
        /// </summary>
        /// <param name="columnCount">the number</param>
        /// <returns>the column in string</returns>
        public string GetTillStringFromNumber(int columnCount) {
            string columnName = String.Empty;

            while (columnCount > 0)
            {
                int modulo = (columnCount - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString(CultureInfo.InvariantCulture) + columnName;
                columnCount = ((columnCount - modulo) / 26);
            }

            return columnName;
        }

        // fills the output
        private void ExportPresentationNodeColumn(IPresentationTreeNode pnode, ref object[,] outputData,
                                                  string selectedVersion, TaxonomyType taxonomyType,
                                                  Type propertyType, Type enumBaseType, PropertyInfo info, int depth, ref object[,] indentLevels, int presentationTreeColumnDelta) {
            // TODO: check if there is something like comment, change type, reviewer name, task, last change
            WriteToObjectOutput(selectedVersion, taxonomyType, ref outputData, "Section",
                                pnode.PresentationTree.Role.Name + " [" +
                                pnode.PresentationTree.Role.RoleUri + "]");
            ExportElement(pnode.Element, ref outputData, selectedVersion, taxonomyType, propertyType, enumBaseType, info,
                          depth, ref indentLevels, presentationTreeColumnDelta);

            if (pnode.Element.Children.Count == 1 && pnode.Element.Children[0].Id.EndsWith(".head")) {
                List<IPresentationTreeNode> children = new List<IPresentationTreeNode>();
                foreach (IPresentationTreeNode child in pnode.Children) {
                    children.Add(child);
                }
                foreach (IElement eChild in pnode.Element.Children[0].Children) {
                    foreach (IPresentationTreeNode presentationTreeNode in eChild.PresentationTreeNodes) {
                        if (children.Contains(presentationTreeNode))
                            continue;
                        children.Add(presentationTreeNode);
                    }
                }
                foreach (IPresentationTreeNode pchild in children.OrderBy(t => t.Order)) {
                    ExportPresentationNodeColumn(pchild, ref outputData, selectedVersion, taxonomyType, propertyType,
                                                 enumBaseType, info,
                                                 depth + 1, ref indentLevels, presentationTreeColumnDelta);
                }
                return;
            }
            foreach (IPresentationTreeNode child in pnode.Children.OrderBy(t => t.Order)) {
                ExportPresentationNodeColumn(child, ref outputData, selectedVersion, taxonomyType, propertyType,
                                             enumBaseType, info,
                                             depth + 1, ref indentLevels, presentationTreeColumnDelta);
            }
        }

        /// <summary>
        /// Checks if shifting is needed because of pasted columns or not. If shifting is needed, the index will change to the right value.
        /// </summary>
        /// <param name="index">input output parameter of the column's index</param>
        /// <param name="selectedVersion">the shifting is version dependant</param>
        /// <param name="taxonomyType">the shifting is taxonomy type dependant</param>
        private void CheckShifting(ref int index, string selectedVersion, TaxonomyType taxonomyType) {
            if (index > _indentedColumnIndexByTaxonomyByVersion[selectedVersion][taxonomyType]) {
                index += _shiftCount + _thickColumnCount;
            }
        }

        // export an Element to outputData
        private void ExportElement(IElement element, ref object[,] outputData, string selectedVersion,
                                   TaxonomyType taxonomyType, Type propertyType, Type enumBaseType, PropertyInfo info,
                                   int depth, ref object[,] indentLevels, int presentationTreeColumnDelta) {
            string id = element.PositionNumber ?? element.Id;
            if (!_elementMap.ContainsKey(id)) {
                _elementMap[id] = new Tuple<int, int>(_rowCount, presentationTreeColumnDelta);
            }
            Dictionary<string, object> valueDictionary = new Dictionary<string, object> {
                {"NameSpace", element.TargetNamespace.Namespace},
                {"XbrlType", element.Type.Replace("ItemType", string.Empty)},
                {"Level", depth},
                {"Item", element.SubstitutionGroup.EndsWith(".head") ? "item" : element.SubstitutionGroup},
                {"TerseLabel (de)", element.GetLabel(LabelRoles.TerseLabel, Languages.de)},
                {"TerseLabel (en)", element.GetLabel(LabelRoles.TerseLabel, Languages.en)},
                {"StandardLabel (de)", element.GetLabel(LabelRoles.Label, Languages.de)},
                {"StandardLabel (en)", element.GetLabel(LabelRoles.Label, Languages.en)},
                {"PositiveTerseLabel (de)", element.GetLabel(LabelRoles.PositiveTerseLabel, Languages.de)},
                {"PositiveTerseLabel (en)", element.GetLabel(LabelRoles.PositiveTerseLabel, Languages.en)},
                {"PositiveLabel (de)", element.GetLabel(LabelRoles.PositiveLabel, Languages.de)},
                {"PositiveLabel (en)", element.GetLabel(LabelRoles.PositiveLabel, Languages.en)},
                {"NegativeTerseLabel (de)", element.GetLabel(LabelRoles.NegativeTerseLabel, Languages.de)},
                {"NegativeTerseLabel (en)", element.GetLabel(LabelRoles.NegativeTerseLabel, Languages.en)},
                {"NegativeLabel (de)", element.GetLabel(LabelRoles.NegativeLabel, Languages.de)},
                {"NegativeLabel (en)", element.GetLabel(LabelRoles.NegativeLabel, Languages.en)},
                {"PeriodStartLabel (de)", element.GetLabel(LabelRoles.PeriodStartLabel, Languages.de)},
                {"PeriodStartLabel (en)", element.GetLabel(LabelRoles.PeriodStartLabel, Languages.en)},
                {"PeriodEndLabel (de)", element.GetLabel(LabelRoles.PeriodEndLabel, Languages.de)},
                {"PeriodEndLabel (en)", element.GetLabel(LabelRoles.PeriodEndLabel, Languages.en)},
                {"Documentation (de)", element.GetLabel(LabelRoles.Documentation, Languages.de)},
                {"Documentation (en)", element.GetLabel(LabelRoles.Documentation, Languages.en)},
                {"DefinitionGuidance (de)", element.GetLabel(LabelRoles.DefinitionGuidance, Languages.de)},
                {"DefinitionGuidance (en)", element.GetLabel(LabelRoles.DefinitionGuidance, Languages.en)},
            };
            foreach (KeyValuePair<string, object> keyValuePair in valueDictionary) {
                WriteToObjectOutput(selectedVersion, taxonomyType, ref outputData, keyValuePair.Key, keyValuePair.Value);
            }
            if (enumBaseType != null && enumBaseType.Name == "Enum") {
                WriteToObjectOutput(selectedVersion, taxonomyType, ref outputData, info.Name, Enum.GetName(propertyType, info.GetValue(element, null)));
            } else {
                if (info.Name == "LabelAlternative") {
                    int index;
                    if (_columnIndexByColumnNameByTaxonomyTypeByVersion[selectedVersion][taxonomyType].TryGetValue("LabelAlternative", out index)) {
                        CheckShifting(ref index, selectedVersion, taxonomyType);
                        outputData[_rowCount, index] = string.IsNullOrEmpty(element.LabelAlternative)
                                                           ? element.Label
                                                           : element.LabelAlternative;
                        indentLevels[_rowCount, _indentedColumnIndexByTaxonomyByVersion[selectedVersion][taxonomyType]] =
                            depth - 1;
                    }
                } else {
                    WriteToObjectOutput(selectedVersion, taxonomyType, ref outputData, info.Name, info.GetValue(element, null));
                }
            }
            if (element.References.Count > 0) {
                IReference reference = element.References.Last();
                ExportReference(reference, ref outputData, selectedVersion, taxonomyType);
            }
            _rowCount++;
            if (element.References.Count <= 1) return;
            foreach (IReference reference in element.References.Reverse().Skip(1)) {
                outputData[_rowCount, 8] = "[reference only]";
                indentLevels[_rowCount, _indentedColumnIndexByTaxonomyByVersion[selectedVersion][taxonomyType]] = depth;
                ExportReference(reference, ref outputData, selectedVersion, taxonomyType);
                _rowCount++;
            }
        }

        private void WriteToObjectOutput(string selectedVersion, TaxonomyType taxonomyType, ref object[,] outputData, string key, object value) {
            int index;
            if (_columnIndexByColumnNameByTaxonomyTypeByVersion[selectedVersion][taxonomyType].TryGetValue(key, out index)) {
                CheckShifting(ref index, selectedVersion, taxonomyType);
                outputData[_rowCount, index] = value;
            }
        }

        // export references of an Element to outputData
        private void ExportReference(IReference reference, ref object[,] outputData, string selectedVersion, TaxonomyType taxonomyType) {
            Dictionary<string, object> valueDictionary = new Dictionary<string, object> {
                {"FiscalReference", reference.FiscalReference},
                {"FiscalRequierement", reference.FiscalRequirement},
                {"NotPermittedFor", reference.NotPermittedFor},
                {"LegalFormEU", reference.LegalFormEU},
                {"LegalFormKSt", reference.LegalFormKSt},
                {"LegalFormPG", reference.LegalFormPG},
                {"TypeOperatingResult", reference.TypeOperatingResult},
                {"ConsistencyCheck", reference.ConsistencyCheck},
                {"FiscalValidSince", reference.FiscalValidSince},
                {"FiscalValidThrough", reference.FiscalValidThrough},
                {"KeineBranche", reference.keineBranche},
                {"PBV", reference.PBV},
                {"KHBV", reference.KHBV},
                {"EBV", reference.EBV},
                {"WUV", reference.WUV},
                {"VUV", reference.VUV},
                {"LUF", reference.LUF},
                {"ValidSince", reference.ValidSince},
                {"ValidThrough", reference.ValidThrough},
                {"Publisher", reference.Publisher},
                {"ReferenceName", reference.Name},
                {"Number", reference.Number},
                {"IssueDate", reference.IssueDate},
                {"Chapter", reference.Chapter},
                {"Article", reference.Article},
                {"Note", reference.Note},
                {"ReferenceSection", reference.Section},
                {"SubSection", reference.Subsection},
                {"Paragraph", reference.Paragraph},
                {"Subparagraph", reference.Subparagraph},
                {"Clause", reference.Clause},
                {"SubClause", reference.Subclause},
                {"KindOfFinancialInstitutionKreditgenossenschWarengeschaeft", reference.kindOfFinancialInstitutionKreditgenossenschWarengeschaeft},
                {"KindOfFinancialInstitutionFinanzdienstl", reference.kindOfFinancialInstitutionFinanzdienstl},
                {"KindOfFinancialInstitutionPfandbriefbanken", reference.kindOfFinancialInstitutionPfandbriefbanken},
                {"KindOfFinancialInstitutionBauspar", reference.kindOfFinancialInstitutionBauspar},
                {"KindOfFinancialInstitutionKreditgenossensch", reference.kindOfFinancialInstitutionKreditgenossensch},
                {"KindOfFinancialInstitutionSparkassen", reference.kindOfFinancialInstitutionSparkassen},
                {"KindOfFinancialInstitutionKapitalanlagegesellschaften", reference.kindOfFinancialInstitutionKapitalanlagegesellschaften},
                {"KindOfFinancialInstitutionSkontrofuehrer", reference.kindOfFinancialInstitutionSkontrofuehrer},
                {"KindOfFinancialInstitutionPfandbriefbankenOERA", reference.kindOfFinancialInstitutionPfandbriefbankenOERA},
                {"KindOfFinancialInstitutionGirozentralen", reference.kindOfFinancialInstitutionGirozentralen},
                {"KindOfFinancialInstitutiongenossZentrB", reference.kindOfFinancialInstitutiongenossZentrB},
                {"KindOfFinancialInstitutionDtGenB", reference.kindOfFinancialInstitutionDtGenB},
                {"KindOfFinancialInstitutionPfandBG", reference.kindOfFinancialInstitutionPfandBG},
                {"KindOfBusinessSUV", reference.kindOfBusinessSUV},
                {"KindOfBusinessRV", reference.kindOfBusinessRV},
                {"KindOfBusinessLKV", reference.kindOfBusinessLKV},
                {"KindOfBusinessPSK", reference.kindOfBusinessPSK},
                {"KindOfBusinessLUV", reference.kindOfBusinessLUV},
                {"KindOfBusinessSUK", reference.kindOfBusinessSUK},
                {"KindOfBusinessPF", reference.kindOfBusinessPF},
            };
            foreach (KeyValuePair<string, object> keyValuePair in valueDictionary) {
                WriteToObjectOutput(selectedVersion, taxonomyType, ref outputData, keyValuePair.Key, keyValuePair.Value);
            }
        }

        private int _rowCount;

        private static void ReleaseObject(object obj) {
            try {
                Marshal.ReleaseComObject(obj);
                // ReSharper disable RedundantAssignment
                obj = null;
                // ReSharper restore RedundantAssignment
            } finally {
                GC.Collect();
            }
        }
    }
}
