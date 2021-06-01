// --------------------------------------------------------------------------------
// author: Márton Garai
// since: 2012-08-23
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;

namespace PivotCsvConverter {
    internal class Program {
        //private static readonly List<string> ComboBoxNames = new List<string>
        //                                                     {"BUS_UNIT", "M4DESC", "TYPE", "ACTORPLAN"};
        private static bool _firstWrite;

        private static List<string> _fieldNames;

        private static Dictionary<string, List<string>> _notWorkingPivtoItemsByField;

        private static List<string> _lastRowPrefixValues;

        private static bool _isYeared;

        private static PivotFields _fields;

        private static void ExportInner(int level, PivotTable table, StreamWriter writerSemicolon,
                                        StreamWriter writerComma, string prefixSemicolon = "", string prefixComma = "") {
            int i = 0;
            foreach (PivotField pivotField in _fields) {
                if (i == level) {
                    PivotItems items = pivotField.PivotItems(Type.Missing);
                    if (!_notWorkingPivtoItemsByField.ContainsKey(pivotField.Name)) {
                        _notWorkingPivtoItemsByField[pivotField.Name] = new List<string>();
                    }
                    foreach (PivotItem pivotItem in items) {
                        if (pivotItem.Name == "(blank)") {
                            ReleaseObject(pivotItem);
                            continue;
                        }
                        bool canContinue = !_notWorkingPivtoItemsByField[pivotField.Name].Contains(pivotItem.Name);
                        if (!canContinue) {
                            ReleaseObject(pivotItem);
                            continue;
                        }
                        try {
                            pivotField.CurrentPage = pivotItem.Name;
                        } catch (Exception) {
                            canContinue = false;
                            _notWorkingPivtoItemsByField[pivotField.Name].Add(pivotItem.Name);
                        }
                        if (!canContinue) {
                            ReleaseObject(pivotItem);
                            continue;
                        }
                        if (pivotItem.RecordCount <= 0) {
                            ReleaseObject(pivotItem);
                            continue;
                        }
                        if (level + 1 != _fields.Count) {
                            string newName = GetUniqueName(pivotField.Name, _fieldNames);
                            _fieldNames.Add(newName);
                            ExportInner(level + 1, table, writerSemicolon, writerComma,
                                        prefixSemicolon + ";\"" + pivotItem.Name + "\"",
                                        prefixComma + "," + pivotItem.Name);
                            ReleaseObject(pivotItem);
                            continue;
                        }
                        Range usedRange = table.DataBodyRange;
                        if (_firstWrite) {
                            writerComma.Write("Year,Column header,");
                            foreach (string fieldName in _fieldNames) {
                                writerComma.Write(fieldName);
                                writerComma.Write(',');
                            }
                            string newName = GetUniqueName(pivotField.Name, _fieldNames);
                            writerComma.Write(newName + ",");
                            for (int l = 1; l < usedRange.Column; l++) {
                                Range cell = _worksheet.Cells[usedRange.Row - 1, l];
                                writerComma.Write(cell.Value.ToString());
                                writerComma.Write(',');
                                ReleaseObject(cell);
                            }
                            writerComma.WriteLine("Value");
                            _firstWrite = false;
                        }
                        if (usedRange.Value == null) {
                            Console.WriteLine("there was null value");
                            ReleaseObject(usedRange);
                            ReleaseObject(pivotItem);
                            continue;
                        }
                        Range columns = usedRange.Columns;
                        Range rows = usedRange.Rows;
                        int width = columns.Count;
                        int height = rows.Count;
                        ReleaseObject(columns);
                        ReleaseObject(rows);
                        int j = 0;
                        int k = 1;
                        bool tillEndOfLine = false;
                        string publicPrefixSemicolon = _publicPrefixComma;
                        string publicPrefixComma = _publicPrefixSemicolon;
                        foreach (object value in usedRange.Value) {
                            if (k == height) {
                                break;
                            }
                            if (tillEndOfLine) {
                                if (j == width - 1) {
                                    j = -1;
                                    k++;
                                    tillEndOfLine = false;
                                }
                                j++;
                                continue;
                            }
                            if (_isYeared) {
                                Range yearCell = _worksheet.Cells[usedRange.Row - 2, usedRange.Column + j];
                                if (yearCell.Value2 != null) {
                                    StringBuilder stringB = new StringBuilder();
                                    foreach (char c in yearCell.Value2.ToString()) {
                                        if (c != ',') {
                                            stringB.Append(c);
                                        }
                                    }
                                    stringB.Append(',');
                                    publicPrefixComma = stringB.ToString().Replace(Environment.NewLine, string.Empty);
                                    StringBuilder sb = new StringBuilder();
                                    sb.Append('"');
                                    foreach (char c in yearCell.Value2.ToString()) {
                                        if (c != ';' && c != '"') {
                                            sb.Append(c);
                                        }
                                        if (c == '"') {
                                            sb.Append(@"''");
                                        }
                                    }
                                    sb.Append("\";\"");
                                    publicPrefixSemicolon = sb.ToString().Replace(Environment.NewLine, string.Empty);
                                }
                            }
                            if (value == null) {
                                if (j == width - 1) {
                                    j = -1;
                                    k++;
                                }
                                j++;
                                continue;
                            }
                            Range monthCell = _worksheet.Cells[usedRange.Row - 1, usedRange.Column + j];
                            if (monthCell.Value == null) {
                                if (j == width - 1) {
                                    j = -1;
                                    k++;
                                }
                                j++;
                                ReleaseObject(monthCell);
                                continue;
                            }
                            string monthString = monthCell.Value.ToString();
                            ReleaseObject(monthCell);
                            if (monthString == "Végösszeg" || monthString == "Gesamtergebnis" ||
                                monthString == "Total" || string.IsNullOrEmpty(monthString)) {
                                if (j == width - 1) {
                                    j = -1;
                                    k++;
                                }
                                j++;
                                Console.WriteLine("'Total' in one of the columns");
                                continue;
                            }
                            StringBuilder rowPrefixSemicolon = new StringBuilder();
                            StringBuilder rowPrefixComma = new StringBuilder();
                            bool isTotal = false;
                            for (int l = 1; l < usedRange.Column; l++) {
                                Range cell = _worksheet.Cells[k + usedRange.Row - 1, l];
                                string prefixCellValue;
                                if (cell.Value == null) {
                                    prefixCellValue = _lastRowPrefixValues[l - 1];
                                    isTotal = true;
                                } else {
                                    if (_lastRowPrefixValues.Count < l) {
                                        _lastRowPrefixValues.Add(cell.Value.ToString());
                                    } else {
                                        _lastRowPrefixValues[l - 1] = cell.Value.ToString();
                                    }
                                    prefixCellValue = _lastRowPrefixValues[l - 1];
                                    isTotal = false;
                                }
                                rowPrefixComma.Append(prefixCellValue);
                                rowPrefixComma.Append(',');
                                rowPrefixSemicolon.Append('"');
                                rowPrefixSemicolon.Append(prefixCellValue);
                                rowPrefixSemicolon.Append("\";");
                                ReleaseObject(cell);
                            }
                            if (isTotal) {
                                tillEndOfLine = true;
                                j++;
                                continue;
                            }
                            if (value is double) {
                                writerSemicolon.Write(publicPrefixSemicolon + monthString + "\"" + prefixSemicolon +
                                                      ";\"" + pivotItem.Name + "\";" + rowPrefixSemicolon + "\"");
                                writerSemicolon.Write(
                                    ((double) value).ToString(
                                        "#,0.###############################################################", German) +
                                    "\"");
                                writerSemicolon.WriteLine();
                            } else {
                                StringBuilder sb = new StringBuilder();
                                sb.Append('"');
                                foreach (char c in value.ToString()) {
                                    if (c != ';' && c != '"') {
                                        sb.Append(c);
                                    }
                                    if (c == '"') {
                                        sb.Append(@"''");
                                    }
                                }
                                sb.Append('"');
                                writerSemicolon.Write(publicPrefixSemicolon + monthString + "\"" + prefixSemicolon +
                                                      ";\"" + pivotItem.Name + "\";" + rowPrefixSemicolon + "\"");
                                writerSemicolon.Write(sb.ToString().Replace(Environment.NewLine, string.Empty));
                                writerSemicolon.WriteLine();
                            }
                            StringBuilder stringBuilder = new StringBuilder();
                            foreach (char c in value.ToString()) {
                                if (c != ',') {
                                    stringBuilder.Append(c);
                                }
                            }
                            writerComma.Write(publicPrefixComma + monthString + prefixComma + "," + pivotItem.Name + "," +
                                              rowPrefixComma);
                            writerComma.Write(stringBuilder.ToString().Replace(Environment.NewLine, string.Empty));
                            writerComma.WriteLine();
                            if (j == width - 1) {
                                j = -1;
                                k++;
                            }
                            j++;
                        }
                        ReleaseObject(usedRange);
                        ReleaseObject(pivotItem);
                    }
                    ReleaseObject(items);
                    ReleaseObject(pivotField);
                    break;
                }
                ReleaseObject(pivotField);
                i++;
            }
        }

        private static string _publicPrefixSemicolon;
        private static string _publicPrefixComma;

        private static Worksheet _worksheet;
        private static readonly CultureInfo German = new CultureInfo("de-DE");

        private static void Main(string[] args) {
            #region task
            // iterate through the given pivot tables' filters, and populate a csv file from the filtered data.
            #endregion

            DateTime startingTime = DateTime.Now;
            JapaneseCalendar japaneseCalendar = new JapaneseCalendar();
            Console.WriteLine("This is the " + japaneseCalendar.GetEra(DateTime.Now) + ".th era in japanese calendar");
            // most likely excel won't use that culture setting.
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            string[] yearDirectories = Directory.GetDirectories(args[0]);
            // starts an excel. If Visibility = true, the excel will be visible.
            Application xlApp = new Application();

            foreach (string yearDirectory in yearDirectories) {
                DirectoryInfo monthDirInfoLast = new DirectoryInfo(yearDirectory);
                DirectoryInfo monthDirInfoLastPrev = monthDirInfoLast.Parent;
                Debug.Assert(monthDirInfoLastPrev != null && monthDirInfoLastPrev.Parent != null);
                // get the company/year/semicolon/month folder hierarchy
                string newPathSemicolon = Path.Combine(monthDirInfoLastPrev.Parent.Name, monthDirInfoLastPrev.Name,
                                                       "semicolon", monthDirInfoLast.Name);
                // get the company/year/semicolon folder hierarchy
                //string newPathSemicolonYearly = Path.Combine(monthDirInfoLastPrev.Parent.Name, monthDirInfoLastPrev.Name,
                //                                             "semicolon");
                // get the company/year/comma/month folder hierarchy
                string newPathComma = Path.Combine(monthDirInfoLastPrev.Parent.Name, monthDirInfoLastPrev.Name, "comma",
                                                   monthDirInfoLast.Name);
                // get the company/year/comma folder hierarchy
                //string newPathCommaYearly = Path.Combine(monthDirInfoLastPrev.Parent.Name, monthDirInfoLastPrev.Name,
                //                                         "comma");
                foreach (string filePath in Directory.GetFiles(yearDirectory)) {
                    //if (!filePath.EndsWith(".xls")) continue;
                    bool longName = filePath.ToLower().EndsWith(".xlsx");
                    if (!filePath.ToLower().EndsWith(".xls") && !longName) {
                        Console.WriteLine("skipping " + filePath);
                        continue;
                    }
                    Console.WriteLine("starting " + filePath);
                    string[] pathElement = filePath.Split('\\');
                    // if I found the file, and processed it, continue the foreach.
                    Workbooks workbooks = xlApp.Workbooks;
                    // read the old file.
                    Workbook oldMonthlyWorkbook = workbooks.Open(filePath, Type.Missing, Type.Missing,
                                                                 Type.Missing, Type.Missing, Type.Missing,
                                                                 Type.Missing, Type.Missing, Type.Missing,
                                                                 Type.Missing, false, Type.Missing,
                                                                 Type.Missing, Type.Missing, true);
                    Sheets sheets = oldMonthlyWorkbook.Sheets;
                    foreach (Worksheet worksheet in sheets) {
                        int howMuchRemove = longName
                                                ? pathElement[pathElement.Length - 1].Length - 5
                                                : pathElement[pathElement.Length - 1].Length - 4;
                        string path = Path.Combine(newPathComma,
                                                   pathElement[pathElement.Length - 1].Remove(howMuchRemove));
                        Directory.CreateDirectory(path);
                        string fullFileName = new FileInfo(Path.Combine(path, worksheet.Name + ".csv")).FullName;
                        //Range columns = usedRange.Columns;
                        //int maxColumn = columns.Count;
                        Range range = worksheet.Range["A1", "A1"];
                        PivotTables pivotTables = worksheet.PivotTables(Type.Missing);
                        Range entireRow = range.EntireRow;
                        _worksheet = worksheet;
                        _firstWrite = true;
                        path = Path.Combine(newPathSemicolon,
                                            pathElement[pathElement.Length - 1].Remove(howMuchRemove));
                        Directory.CreateDirectory(path);
                        string fullFileName2 = new FileInfo(Path.Combine(path, worksheet.Name + ".csv")).FullName;
                        File.Delete(fullFileName2);
                        try {
                            // delete the new file if exists, and create a new one.
                            File.Delete(fullFileName);
                            _publicPrefixComma = monthDirInfoLast.Name + ",";
                            using (StreamWriter newSheetCommaWriter = new StreamWriter(fullFileName, false, Encoding.GetEncoding("latin1"))) {
                                using (StreamWriter newSheetSemicolonWriter = new StreamWriter(fullFileName2, false, Encoding.GetEncoding("latin1"))) {
                                    Console.WriteLine("creating " + fullFileName + " and " + fullFileName2);
                                    _notWorkingPivtoItemsByField = new Dictionary<string, List<string>>();
                                    _publicPrefixSemicolon = "\"" + monthDirInfoLast.Name + "\";\"";
                                    foreach (PivotTable pivotTable in pivotTables) {
                                        _fieldNames = new List<string>();
                                        _lastRowPrefixValues = new List<string>();
                                        _isYeared = false;
                                        PivotFields fields = pivotTable.ColumnFields;
                                        foreach (PivotField pivotField in fields) {
                                            if (pivotField.Name == "Years") {
                                                _isYeared = true;
                                                ReleaseObject(pivotField);
                                                break;
                                            }
                                            ReleaseObject(pivotField);
                                        }
                                        ReleaseObject(fields);
                                        fields = pivotTable.RowFields;
                                        foreach (PivotField pivotField in fields) {
                                            try {
                                                pivotField.ShowDetail = true;
// ReSharper disable EmptyGeneralCatchClause
                                            } catch (Exception) {
// ReSharper restore EmptyGeneralCatchClause
                                            }
                                            ReleaseObject(pivotField);
                                        }
                                        ReleaseObject(fields);
                                        Range bodyRange = pivotTable.DataBodyRange;
                                        if (bodyRange == null) {
// ReSharper disable ExpressionIsAlwaysNull
                                            ReleaseObject(bodyRange);
// ReSharper restore ExpressionIsAlwaysNull
                                            ReleaseObject(pivotTable);
                                            Console.WriteLine("DataBodyRange null");
                                            continue;
                                        }
                                        _fields = pivotTable.PageFields;
                                        ExportInner(0, pivotTable, newSheetSemicolonWriter, newSheetCommaWriter);
                                        ReleaseObject(_fields);
                                        ReleaseObject(pivotTable);
                                    }
                                    Console.WriteLine("finished creating");
                                }
                            }
                        } finally {
                            // every COM object have to be released.
                            //ReleaseObject(columns);
                            ReleaseObject(entireRow);
                            ReleaseObject(range);
                            ReleaseObject(worksheet);
                        }
                    }
                    ReleaseObject(sheets);
                    try {
                        // try to close normally.
                        oldMonthlyWorkbook.Close(false, Type.Missing, Type.Missing);
                    } finally {
                        ReleaseObject(oldMonthlyWorkbook);
                    }

                    break;
                }
            }
            xlApp.Quit();
            ReleaseObject(xlApp);
            Console.WriteLine();
            Console.Write("finished. The program run for : " + DateTime.Now.Subtract(startingTime).Hours + ":" +
                          DateTime.Now.Subtract(startingTime).Minutes + ":" +
                          DateTime.Now.Subtract(startingTime).Seconds + " hour:minute:second");
            Console.ReadLine();
        }

        internal static string GetUniqueName(string newName, List<string> oldNames) {
            string ret = newName;
            while (oldNames.Contains(ret)) {
                int lastNumberIndex = 0;
                bool prevNumber = false;
                for (int i = 0; i < ret.Length; i++) {
                    switch (ret[i]) {
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            if (!prevNumber) {
                                lastNumberIndex = i;
                            }
                            prevNumber = true;
                            break;
                        default:
                            prevNumber = false;
                            break;
                    }
                }
                if (prevNumber) {
                    int number = int.Parse(ret.Substring(lastNumberIndex));
                    ret = ret.Remove(lastNumberIndex) + (number + 1);
                } else {
                    ret += 2;
                }
            }
            return ret;
        }


        private static void ReleaseObject(object obj) {
            try {
                Marshal.ReleaseComObject(obj);
// ReSharper disable RedundantAssignment
                obj = null;
// ReSharper restore RedundantAssignment
            } catch (Exception ex) {
// ReSharper disable RedundantAssignment
                obj = null;
// ReSharper restore RedundantAssignment
                Console.Write("Unable to release the Object " + ex);
            } finally {
                GC.Collect();
            }
        }
    }
}