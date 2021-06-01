using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBComparisonBusiness;
using MSExcel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.Drawing;
using DBComparisonBusiness.Business;

namespace DbComparisonV2.DocumentGenerator
{

    public class ExcelDBCompareResultGenerator : ExcelResultGeneratorBase
    {

        IComparisonResult _comparisonResult;

        #region constructors
        public ExcelDBCompareResultGenerator(IComparisonResult result): base()
        {
            _comparisonResult = result;
        }
        #endregion

        protected override void  CreateContent()
        {
            AddSheets(3);

            AddContentToSheet(CreatePage1Content(), _workbook.Worksheets[1]);
            AddContentToSheet(CreatePage2Content(), _workbook.Worksheets[2]);
            AddContentToSheet(CreatePage3Content(), _workbook.Worksheets[3]);
        }
        private FileContent CreatePage3Content()
        {
            // add DB name of both databases & headers of columns + add style
            var titleContent = new FileContent()
                .AddContent(_comparisonResult.DbName[0], addLine: 0, addColumn: 0, border: Border.Left & Border.Bottom & Border.Top)
                .AddContent(_comparisonResult.DbName[1], addLine: 0, addColumn: 3, border: Border.Right & Border.Bottom & Border.Top)
                .AddContent("Table", border: Border.Bottom)
                .AddContent("ColumnName", addLine: 0, addColumn: 1, border: Border.Bottom)
                .AddContent("ColumnType", addLine: 0, addColumn: 1, border: Border.Bottom)
                .AddContent("Table",        addLine: 0, addColumn: 1, border: Border.Bottom)
                .AddContent("ColumnName", addLine: 0, addColumn: 1, border: Border.Bottom)
                .AddContent("ColumnType", addLine: 0, addColumn: 1, border: Border.Bottom);
            var tableCount = 0;
            // for all tables existing in both DB
            foreach (var table in _comparisonResult.Common.AllDistinctTables)
            {
                var colCount = 0;
                var style = new Style() { BackgroundColor = (++tableCount % 2 == 0) ? Color.LightBlue : Color.White, Name = (tableCount % 2 == 0) ? "Even" : "Odd" };

                // for all columns existing in both tables with same name
                table.Columns.ForEach(colInfo =>
                {
                    // Func that selects the column (if any) in the table that matches the current table in AllDistinctTables (if any)
                    Func<ComparisonResult.TableInfoCollection, ComparisonResult.ColumnInfo> GetColumnOfTableFromDB = (Tables) =>
                         (from t in Tables
                          let foundTable = t.Name == table.Name
                          from c in t.Columns
                          where foundTable && c.Name == colInfo.Name
                          select c).FirstOrDefault();
                    var tableDb1 = (from t in _comparisonResult.Tables[0] where t.Name == table.Name select t).FirstOrDefault();
                    var tableDb2 = (from t in _comparisonResult.Tables[0] where t.Name == table.Name select t).FirstOrDefault();

                    var colInfoTableDb1 = GetColumnOfTableFromDB(_comparisonResult.Tables[0]);
                    var colInfoTableDb2 = GetColumnOfTableFromDB(_comparisonResult.Tables[1]);
                    colCount++;
                    // foreach colums found (in both tables) writes the table name (if any table) then the column index of the current loop within the table
                    titleContent.AddContent(colInfoTableDb1 != null ? table.Name : "", style: style)
                        .AddContent(colInfoTableDb1 != null ? colInfoTableDb1.Name : "", addLine: 0, addColumn: 1, style: style)
                        .AddContent(colInfoTableDb1 != null ? colInfoTableDb1.DataType : "", addLine: 0, addColumn: 1, border: Border.Right, style: style)
                        .AddContent(colInfoTableDb2 != null ? table.Name : "", addLine: 0, addColumn: 1, style: style)
                        .AddContent(colInfoTableDb2 != null ? colInfoTableDb2.Name : "", addLine: 0, addColumn: 1, style: style)
                        .AddContent(colInfoTableDb2 != null ? colInfoTableDb2.DataType : "", addLine: 0, addColumn: 1, style: style);

                });
            }
            return titleContent;
        }

        private FileContent CreatePage2Content()
        {
            // add DB name of both databases + add style
            var titleContent = new FileContent()
                .AddContent(_comparisonResult.DbName[0], addLine: 0, addColumn: 0, border: Border.Left & Border.Bottom & Border.Top)
                .AddContent(_comparisonResult.DbName[1], addLine: 0, addColumn: 3, border: Border.Right & Border.Bottom & Border.Top);

            // add headers for the columns
            var pageContent = new FileContent()
                .AddContent("Table", addLine: 1, addColumn: 0)
                .AddContent("RowCount", addLine: 0, addColumn: 1)
                .AddContent("ColCount", border: Border.Right, addLine: 0, addColumn: 1) // defines a border on the right cell
                .AddContent("Table", addLine: 0, addColumn: 1)
                .AddContent("RowCount", addLine: 0, addColumn: 1)
                .AddContent("ColCount", addLine: 0, addColumn: 1);

            _comparisonResult.Common.AllDistinctTables.ForEach(tableInfo =>
            {
                var db1TableInfo = _comparisonResult.Tables[0].Find(t => t.Name == tableInfo.Name);
                var db2TableInfo = _comparisonResult.Tables[1].Find(t => t.Name == tableInfo.Name);

                // in current table of DB1 on a new line
                pageContent.AddContent(db1TableInfo != null ? db1TableInfo.Name : "", addLine: 1, addColumn: 0)               // writes table name if any
                    .AddContent(db1TableInfo != null ? db1TableInfo.RowCount.ToString() : "", addLine: 0, addColumn: 1) // writes row counts
                    .AddContent(db1TableInfo != null ? db1TableInfo.Columns.Count.ToString() : "", addLine: 0, addColumn: 1, border: Border.Right); // writes column count

                // in current table of DB2 on the same line
                pageContent.AddContent(db2TableInfo != null ? db2TableInfo.Name : "", addLine: 0, addColumn: 1)               // writes table name if any
                    .AddContent(db2TableInfo != null ? db2TableInfo.RowCount.ToString() : "", addLine: 0, addColumn: 1) // writes row counts
                    .AddContent(db2TableInfo != null ? db2TableInfo.Columns.Count.ToString() : "", addLine: 0, addColumn: 1); // writes column count
            });

            return pageContent;
        }

        private FileContent CreatePage1Content()
        {
            var titleContent = new FileContent()
                .AddContent(_comparisonResult.DbName[0], addLine: 0, addColumn: 1, border: Border.Left & Border.Bottom & Border.Top)
                .AddContent(_comparisonResult.DbName[1], addLine: 0, addColumn: 1, border: Border.Right & Border.Bottom & Border.Top);

            var pageContent = new FileContent()
                .AddContent("# Tables",                         addLine: 1, addColumn: 0)
                .AddContent(_comparisonResult.TableCount[0], addLine: 0, addColumn: 1)
                .AddContent(_comparisonResult.TableCount[1], addLine: 0, addColumn: 1)
                .AddContent("# Columns",                    addLine: 1, addColumn:0)
                .AddContent(_comparisonResult.ColumnCount[0], addLine: 0, addColumn: 1)
                .AddContent(_comparisonResult.ColumnCount[1], addLine: 0, addColumn: 1)
                .AddContent("# Empty Tables",               addLine: 1, addColumn: 0)
                .AddContent(_comparisonResult.EmptyTablesCount[0], addLine: 0, addColumn: 1)
                .AddContent(_comparisonResult.EmptyTablesCount[1], addLine: 0, addColumn: 1)

                .AddContent("# Non-Empty Tables", addLine: 1, addColumn: 0)
                .AddContent(_comparisonResult.TableCount[0], addLine: 0, addColumn: 1)
                .AddContent(_comparisonResult.TableCount[1], addLine: 0, addColumn: 1);
            return pageContent;

        }
    }
}
