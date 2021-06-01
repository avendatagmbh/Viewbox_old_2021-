using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Office.Interop.Excel;
using Utils;
using ViewValidator.Manager;
using ViewValidatorLogic.Interfaces;
using ViewValidatorLogic.Logic;
using ViewValidatorLogic.Structures.Results;
using Excel = Microsoft.Office.Interop.Excel;

namespace ViewValidator.Utils {
    public static class DataGridHelper {
        private static string GetLastColumnName(int lastColumnIndex) {
            string lastColumn = "";

            // check whether the column count is > 26
            if (lastColumnIndex > 26) {
                // If the column count is > 26, the the last column index will be something
                // like "AA", "DE", "BC" etc

                // Get the first letter
                // ASCII index 65 represent char. 'A'. So, we use 64 in this calculation as a starting point
                char first = Convert.ToChar(64 + ((lastColumnIndex - 1) / 26));

                // Get the second letter
                char second = Convert.ToChar(64 + (lastColumnIndex % 26 == 0 ? 26 : lastColumnIndex % 26));

                // Concat. them
                lastColumn = first.ToString() + second.ToString();
            } else {
                // ASCII index 65 represent char. 'A'. So, we use 64 in this calculation as a starting point
                lastColumn = Convert.ToChar(64 + lastColumnIndex).ToString();
            }
            return lastColumn;
        }

        private static void ExportRowsToWorksheet(IEnumerable<IRow> rows, Excel.Worksheet sheet, ProgressCalculator calculator, int currentChild) {
            int rowCount = rows.Count();
            calculator[currentChild].SetWorkSteps(rowCount, false);
            //No items -> return
            if (rowCount == 0) return;
            int columnCount = rows.First().ColumnCount;

            string rangeString = "A1:" + GetLastColumnName(columnCount) + (rowCount + 1);
            Range excelRange = sheet.Range[rangeString];
            object[,] valueArray = (object[,])excelRange.get_Value(
                XlRangeValueDataType.xlRangeValueDefault);
            string[,] values = new string[rowCount+1, columnCount];

            for (int colIndex = 0; colIndex < columnCount; ++colIndex) {
                values[0, colIndex] = rows.First().HeaderOfColumn(colIndex);
                sheet.Cells[1, colIndex + 1].Interior.Color =
                    ColorManager.BrushToExcelColor(Brushes.Gainsboro);
            }

            int rowIndex = 1;
            foreach (var row in rows) {
                for (int colIndex = 0; colIndex < columnCount; ++colIndex) {
                    values[rowIndex, colIndex] = row[colIndex].RuleDisplayString;
                    if (row[colIndex].RowEntryType != RowEntryType.Normal) {
                        sheet.Cells[rowIndex + 1, colIndex + 1].Interior.Color =
                            ColorManager.ExcelColorFromRowEntryType(row[colIndex].RowEntryType);
                    }
                }
                rowIndex++;
                calculator[currentChild].StepDone();
            }
            excelRange.set_Value(XlRangeValueDataType.xlRangeValueDefault, values);

            foreach (var row in rows) {
                for (int colIndex = 0; colIndex < row.ColumnCount; ++colIndex) {
                    ((Range)sheet.Cells[1, colIndex+1]).EntireColumn.AutoFit();
                }
                break;
            }
        }

        public static void ExportResultsToExcel(TableValidationResult result, string filename, ProgressCalculator calculator) {
            dynamic excelWorkBook = null;
            Excel.Application excelApplication = null;
            try {
                excelApplication = new Excel.Application { Visible = false, DisplayAlerts = false };
                var workBooks = excelApplication.Workbooks;
                excelWorkBook = workBooks.Add();
                calculator.SetWorkSteps(3, true);

                Excel.Worksheet sheet = excelWorkBook.Worksheets[1];
                sheet.Name = "Verschiedene Zeilen";
                ExportRowsToWorksheet(result.RowDifferences, sheet, calculator, 0);
                calculator.StepDone();

                sheet = excelWorkBook.Worksheets[2];
                sheet.Name = "Zeilen zuviel in View";
                ExportRowsToWorksheet(result.ResultValidationTable.MissingRows, sheet, calculator, 1);
                calculator.StepDone();

                sheet = excelWorkBook.Worksheets[3];
                sheet.Name = "Fehlende Zeilen in View";
                ExportRowsToWorksheet(result.ResultViewTable.MissingRows, sheet, calculator, 2);

                calculator.StepDone();

                excelWorkBook.SaveAs(
                    filename,
                    AccessMode: Excel.XlSaveAsAccessMode.xlShared);

            } catch (Exception) {
            } finally {
                if (excelWorkBook != null)
                    excelWorkBook.Close();
                if (excelApplication != null)
                    excelApplication.Workbooks.Close();
            }
        }
    }
}
