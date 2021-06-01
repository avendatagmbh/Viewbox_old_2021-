// --------------------------------------------------------------------------------
// author: Márton Garai
// since: 2012-08-16
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Office.Interop.Excel;

namespace ExcelToCsvConverter {
    /// <summary>
    /// usage of the program. First delete the program created folder in the executing folder. The argument of the program must be the year's folder. Run the application.
    /// </summary>
    class Program {
        static void Main(string[] args) {
            #region task
            //read in the excel files, perhaps Helene needs to copy them to another location where you have access
            
            //write them to a local path on your computer for testing and copy them when you are finished

            //after you converted one file, give it to Helene to let her test it, if the format is correct.
            #endregion

            // most likely excel won't use that culture setting.
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo german = new CultureInfo("de-DE");
            string[] monthDirectories = Directory.GetDirectories(args[0]);
            // starts an excel. If Visibility = true, the excel will be visible.
            Application xlApp = new Application();

            // the monthly files have to be parsed monthly, and not joined.
            foreach (string monthDirectory in monthDirectories) {
                DirectoryInfo monthDirInfoLast = new DirectoryInfo(monthDirectory);
                DirectoryInfo monthDirInfoLastPrev = monthDirInfoLast.Parent;
                Debug.Assert(monthDirInfoLastPrev != null && monthDirInfoLastPrev.Parent != null);
                // get the company/year/semicolon/month folder hierarchy
                string newPathSemicolon = Path.Combine(monthDirInfoLastPrev.Parent.Name, monthDirInfoLastPrev.Name,
                                                       "semicolon", monthDirInfoLast.Name);
                // get the company/year/semicolon folder hierarchy
                string newPathSemicolonYearly = Path.Combine(monthDirInfoLastPrev.Parent.Name, monthDirInfoLastPrev.Name,
                                                             "semicolon");
                Directory.CreateDirectory(newPathSemicolon);
                // get the company/year/comma/month folder hierarchy
                string newPathComma = Path.Combine(monthDirInfoLastPrev.Parent.Name, monthDirInfoLastPrev.Name,
                                                   "comma", monthDirInfoLast.Name);
                // get the company/year/comma folder hierarchy
                string newPathCommaYearly = Path.Combine(monthDirInfoLastPrev.Parent.Name, monthDirInfoLastPrev.Name,
                                                         "comma");
                Directory.CreateDirectory(newPathComma);
                int i = 0;
                foreach (string filePath in Directory.GetFiles(monthDirectory)) {
                    if (!filePath.ToLower().EndsWith(".xls") && !filePath.ToLower().EndsWith(".xlsx")) continue;
                    bool longName = filePath.ToLower().EndsWith(".xlsx");
                    Console.WriteLine("started to read " + filePath);
                    string[] pathElement = filePath.Split('\\');
                    // delete the xls ending, and in the new file replace with csv.
                    string file =
                        pathElement[pathElement.Length - 1].Remove(longName
                                                                       ? pathElement[pathElement.Length - 1].Length - 4
                                                                       : pathElement[pathElement.Length - 1].Length - 3) +
                        "csv";
                    StringBuilder newFileName = new StringBuilder();
                    string[] splitteds = file.Split('-');
                    for (int j = 0; j < splitteds.Length; j++) {
                        if (j != 0 && j != 2) {
                            newFileName.Append(splitteds[j]);
                            newFileName.Append('-');
                        }
                    }
                    newFileName.Remove(newFileName.Length - 1, 1);
                    file = newFileName.ToString();
                    Workbooks workbooks = xlApp.Workbooks;
                    // if I found the file, and processed it, continue the foreach.
                    bool canContinue = false;
                    foreach (string monthStringPart in Monthly) {
                        if (file.Replace(" ", "").Replace("_", "").Contains(monthStringPart)) {
                            // read the old file.
                            Workbook oldMonthlyWorkbook = workbooks.Open(filePath, Type.Missing, Type.Missing,
                                                                         Type.Missing, Type.Missing, Type.Missing,
                                                                         Type.Missing, Type.Missing, Type.Missing,
                                                                         Type.Missing, false, Type.Missing,
                                                                         Type.Missing, Type.Missing, true);
                            Sheets sheets = oldMonthlyWorkbook.Sheets;
                            Worksheet oldSheet = oldMonthlyWorkbook.Sheets[1];
                            string fullFileName = new FileInfo(Path.Combine(newPathComma, file)).FullName;
                            Range usedRange = oldSheet.UsedRange;
                            if (usedRange.Value != null) {
                                Range columns = usedRange.Columns;
                                int maxColumn = columns.Count;
                                Range range = oldSheet.Range["A1", "A1"];
                                Range entireRow = range.EntireRow;
                                try {
                                    // delete the new file if exists, and create a new one.
                                    File.Delete(fullFileName);
                                    xlApp.DefaultFilePath = fullFileName;
                                    bool headerLine = true;
                                    List<string> headerNames = new List<string>();
                                    using (StreamWriter newSheetCommaWriter = new StreamWriter(fullFileName)) {
                                        foreach (object value in usedRange.Value) {
                                            if (value != null) {
                                                // copy the same value, just skip the ',' character because that will be the separator.
                                                StringBuilder sb = new StringBuilder();
                                                foreach (char c in value.ToString()) {
                                                    if (c != ',') {
                                                        sb.Append(c);
                                                    }
                                                }
                                                // sb.ToString() will take care about the ',' decimal, or thousand separator, if the excel parses it as decimal.
                                                string parsedData = sb.ToString().Replace(Environment.NewLine, "");
                                                if (headerLine) {
                                                    parsedData = GetUniqueName(parsedData, headerNames);
                                                    headerNames.Add(parsedData);
                                                }
                                                newSheetCommaWriter.Write(parsedData);
                                            }

                                            if (i == maxColumn - 1) {
                                                newSheetCommaWriter.WriteLine();
                                                i = -1;
                                                headerLine = false;
                                            } else {
                                                newSheetCommaWriter.Write(",");
                                            }
                                            i++;
                                        }
                                        Console.WriteLine("finished writing (comma, monthly )");
                                    }
                                    // after processing the comma separated values delete the header row.
                                    entireRow.Delete(XlDeleteShiftDirection.xlShiftUp);
                                    fullFileName = new FileInfo(Path.Combine(newPathSemicolon, file)).FullName;
                                    File.Delete(fullFileName);
                                    xlApp.DefaultFilePath = fullFileName;
                                    // file access problems with the two below tries.
                                    //oldMonthlyWorkbook.SaveCopyAs(fullFileName);
                                    //oldMonthlyWorkbook.SaveAs(fullFileName, XlFileFormat.xlCSVWindows, Type.Missing,
                                    //                          Type.Missing, false, false,
                                    //                          XlSaveAsAccessMode.xlShared, Type.Missing, Type.Missing,
                                    //                          Type.Missing, Type.Missing, false);
                                    i = 0;
                                    using (StreamWriter newSheetSemicolonWriter = new StreamWriter(fullFileName)) {
                                        if (usedRange.Value == null) {
                                            Console.WriteLine("value is null");
                                            continue;
                                        }
                                        foreach (object value in usedRange.Value) {
                                            if (value != null) {
                                                if (value is double) {
                                                    //double temp = (double)value;
                                                    //newSheetSemicolonWriter.Write(((double)value).ToString("0#,#.##", english));
                                                    newSheetSemicolonWriter.Write("\"" +
                                                                                  ((double) value).ToString(
                                                                                      "#,0.###############################################################",
                                                                                      german) + "\"");
                                                } else {
                                                    StringBuilder sb = new StringBuilder();
                                                    sb.Append("\"");
                                                    foreach (char c in value.ToString()) {
                                                        if (c != ';' && c != '"') {
                                                            sb.Append(c);
                                                        }
                                                        if (c == '"') {
                                                            sb.Append('\'');
                                                            sb.Append('\'');
                                                        }
                                                    }
                                                    sb.Append("\"");
                                                    newSheetSemicolonWriter.Write(sb.ToString().Replace(
                                                        Environment.NewLine, ""));
                                                }
                                            }

                                            if (i == maxColumn - 1) {
                                                newSheetSemicolonWriter.WriteLine();
                                                i = -1;
                                            } else {
                                                newSheetSemicolonWriter.Write(";");
                                            }
                                            i++;
                                        }
                                        Console.WriteLine("finished writing (semicolon, monthly)");
                                    }
                                } finally {
                                    // every COM object have to be released.
                                    ReleaseObject(entireRow);
                                    ReleaseObject(range);
                                    ReleaseObject(usedRange);
                                    ReleaseObject(columns);
                                    ReleaseObject(oldSheet);
                                    ReleaseObject(sheets);
                                    try {
                                        // try to close normally.
                                        oldMonthlyWorkbook.Close(false, Type.Missing, Type.Missing);
                                    } finally {
                                        ReleaseObject(oldMonthlyWorkbook);
                                    }
                                }

                                canContinue = true;
                                break;
                                // ReSharper disable RedundantIfElseBlock
                            } else {
                                Console.WriteLine("value is null");
                                // ReSharper restore RedundantIfElseBlock
                                File.Delete(fullFileName);
                                xlApp.DefaultFilePath = fullFileName;
                                using (StreamWriter newSheetCommaWriter = new StreamWriter(fullFileName)) {
                                    newSheetCommaWriter.Write(" ");
                                }
                                ReleaseObject(usedRange);
                                ReleaseObject(oldSheet);
                                ReleaseObject(sheets);
                                try {
                                    // try to close normally.
                                    oldMonthlyWorkbook.Close(false, Type.Missing, Type.Missing);
                                } finally {
                                    ReleaseObject(oldMonthlyWorkbook);
                                }
                            }
                        }
                    }
                    if (canContinue) {
                        continue;
                    }
                    // do the same with yearly values.
                    foreach (string yearStringPart in Yearly) {
                        if (file.Replace(" ","").Replace("_", "").Contains(yearStringPart)) {
                            Workbook oldMonthlyWorkbook = workbooks.Open(filePath, Type.Missing, Type.Missing,
                                                                         Type.Missing, Type.Missing, Type.Missing,
                                                                         Type.Missing, Type.Missing, Type.Missing,
                                                                         Type.Missing, false, Type.Missing,
                                                                         Type.Missing, Type.Missing, true);
                            Sheets sheets = oldMonthlyWorkbook.Sheets;
                            Worksheet oldSheet = oldMonthlyWorkbook.Sheets[1];
                            // we don't need the month in the file name.
                            FileInfo actualYearlyFileInfo =
                                new FileInfo(Path.Combine(newPathCommaYearly, file.Remove(0, 4)));
                            string fullFileName = actualYearlyFileInfo.FullName;
                            Range usedRange = oldSheet.UsedRange;
                            if (usedRange.Value != null) {
                                Range columns = usedRange.Columns;
                                int maxColumn = columns.Count;
                                Range range = oldSheet.Range["A1", "A1"];
                                Range entireRow = range.EntireRow;
                                try {
                                    // if FileInfo sais the file exists, we only append to that file. 
                                    if (actualYearlyFileInfo.Exists) entireRow.Delete(XlDeleteShiftDirection.xlShiftUp);
                                    xlApp.DefaultFilePath = fullFileName;
                                    bool headerLine = true;
                                    List<string> headerNames = new List<string>();
                                    using (
                                        StreamWriter yearlyCsvWriter = new StreamWriter(fullFileName,
                                                                                        actualYearlyFileInfo.Exists)) {
                                        foreach (object value in usedRange.Value) {
                                            if (value != null) {
                                                StringBuilder sb = new StringBuilder();
                                                foreach (char c in value.ToString()) {
                                                    if (c != ',') {
                                                        sb.Append(c);
                                                    }
                                                }
                                                string parsedData = sb.ToString().Replace(Environment.NewLine, "");
                                                if (headerLine) {
                                                    parsedData = GetUniqueName(parsedData, headerNames);
                                                    headerNames.Add(parsedData);
                                                }
                                                yearlyCsvWriter.Write(parsedData);
                                            }

                                            if (i == maxColumn - 1) {
                                                yearlyCsvWriter.WriteLine();
                                                i = -1;
                                                headerLine = false;
                                            } else {
                                                yearlyCsvWriter.Write(",");
                                            }
                                            i++;
                                        }
                                        Console.WriteLine("finished writing (comma, yearly)");
                                    }
                                    if (!actualYearlyFileInfo.Exists)
                                        entireRow.Delete(XlDeleteShiftDirection.xlShiftUp);
                                    actualYearlyFileInfo =
                                        new FileInfo(Path.Combine(newPathSemicolonYearly, file.Remove(0, 4)));
                                    fullFileName = actualYearlyFileInfo.FullName;
                                    xlApp.DefaultFilePath = fullFileName;
                                    //oldMonthlyWorkbook.SaveCopyAs(fullFileName);
                                    //oldMonthlyWorkbook.SaveAs(fullFileName, XlFileFormat.xlCSVWindows, Type.Missing,
                                    //                          Type.Missing, false, false,
                                    //                          XlSaveAsAccessMode.xlShared, Type.Missing, Type.Missing,
                                    //                          Type.Missing, Type.Missing, false);
                                    i = 0;
                                    using (
                                        StreamWriter yearlyCsvWriter = new StreamWriter(fullFileName,
                                                                                        actualYearlyFileInfo.Exists)) {
                                        foreach (object value in usedRange.Value) {
                                            if (value != null) {
                                                if (value is double) {
                                                    //yearlyCsvWriter.Write(((double)value).ToString("0#,#.##", english));
                                                    //string temp = temp.ToString("n0", german) +
                                                    //                    (((temp%1).ToString(german).Remove(0, 1)));
                                                    //string temp2 = value.ToString();
                                                    //string temp3 = ((Range) usedRange.Cells[j + 1, i + 1]).Text;
                                                    //yearlyCsvWriter.Write(temp);
                                                    yearlyCsvWriter.Write("\"" +
                                                                          ((double) value).ToString(
                                                                              "#,0.###############################################################",
                                                                              german) + "\"");
                                                } else {
                                                    StringBuilder sb = new StringBuilder();
                                                    sb.Append("\"");
                                                    foreach (char c in value.ToString()) {
                                                        if (c != ';' && c != '"') {
                                                            sb.Append(c);
                                                        }
                                                        if (c == '"') {
                                                            sb.Append('\'');
                                                            sb.Append('\'');
                                                        }
                                                    }
                                                    sb.Append("\"");
                                                    yearlyCsvWriter.Write(sb.ToString().Replace(Environment.NewLine, ""));
                                                }
                                            }

                                            if (i == maxColumn - 1) {
                                                yearlyCsvWriter.WriteLine();
                                                i = -1;
                                            } else {
                                                yearlyCsvWriter.Write(";");
                                            }
                                            i++;
                                        }
                                    }
                                } finally {
                                    ReleaseObject(columns);
                                    ReleaseObject(usedRange);
                                    ReleaseObject(entireRow);
                                    ReleaseObject(range);
                                    ReleaseObject(oldSheet);
                                    ReleaseObject(sheets);
                                    try {
                                        oldMonthlyWorkbook.Close(false, Type.Missing, Type.Missing);
                                    } finally {
                                        ReleaseObject(oldMonthlyWorkbook);
                                    }
                                }

                                ReleaseObject(workbooks);
                                Console.WriteLine("Finished writing (semicolon, yearly)");
                                break;
// ReSharper disable RedundantIfElseBlock
                            } else {
// ReSharper restore RedundantIfElseBlock
                                Console.WriteLine("value is null");
                                ReleaseObject(usedRange);
                                ReleaseObject(oldSheet);
                                ReleaseObject(sheets);
                                try {
                                    // try to close normally.
                                    oldMonthlyWorkbook.Close(false, Type.Missing, Type.Missing);
                                    break;
                                } finally {
                                    ReleaseObject(oldMonthlyWorkbook);
                                }
                            }
                        }
                    }
                }
            }
            xlApp.Quit();
            ReleaseObject(xlApp);
            Console.WriteLine();
            Console.WriteLine("Finished running");
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
                }
                else {
                    ret += 2;
                }
            }
            return ret;
        }


        private static void ReleaseObject(object obj)
        {
            try
            {
                Marshal.ReleaseComObject(obj);
// ReSharper disable RedundantAssignment
                obj = null;
// ReSharper restore RedundantAssignment
            }
            catch (Exception ex)
            {
// ReSharper disable RedundantAssignment
                obj = null;
// ReSharper restore RedundantAssignment
                Console.Write("Unable to release the Object " + ex);
            }
            finally
            {
                GC.Collect();
            }
        }

        private static readonly string[] Yearly = {
            "AdditionByAccount", "ClosedWOLaborExpenseReport",
            "InventoryMovementbyAccountbyTransacationisscogs",
            "InventoryMovementbyAccountbyTransacationissinv",
            "InventoryMovementbyAccountbyTransacationrctcogs",
            "InventoryMovementbyAccountbyTransacationrctinv",
            "InventoryMovementbyAccountbyTransacationtrncogs",
            "InventoryMovementbyAccountbyTransacationtrninv", "InventoryMovementbyAccountcogs",
            "InventoryMovementbyAccountinv", "LaborConsumableReportforClosedWO", "MovementDetailsReport",
            "ReceivednotPaidAccuralReport", "ToolRetirementByAccount"
        };

        // because the type in the file name Open Work Order Labor Consumable I cut the ending of that file.
        private static readonly string[] Monthly = {
            "AssetStatusValueReport", "ClosedWorkOrderConsumableReport",
            "FixedAssetDeprReport", "InventoryStatusDetailReport", "LaborConsumableReportforCompletedWO",
            "OpenWorkOrderLabor", "PurchaseOrderReport", "ToolAssetDepreciationReport",
            "WorkOrderConsumableAcctRptbyWO", "WorkOrderConsumableInvAcctRpt"
        };
    }
}
