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

    public class ExcelViewCompareResultGenerator : ExcelResultGeneratorBase
    {

        IViewComparisonResult _comparisonResult;

        #region constructors
        public ExcelViewCompareResultGenerator(IViewComparisonResult result)
            : base()
        {
            _comparisonResult = result;
        }
        #endregion

        protected override void  CreateContent()
        {
            AddSheets(4);

            AddContentToSheet(CreatePage1Content(), _workbook.Worksheets[1], "overview");
            AddContentToSheet(CreatePage2Content(), _workbook.Worksheets[2], "missing tables");
            AddContentToSheet(CreatePage3Content(), _workbook.Worksheets[3], "missing columns");
            AddContentToSheet(CreatePage4Content(), _workbook.Worksheets[4], "SQL errors");
        }
        
        private FileContent CreatePage4Content() {
            FileContent titleContent = new FileContent()
                .AddContent("ViewScript", addLine: 0, addColumn: 0)
                .AddContent("Complete SQL statement", addLine: 0, addColumn: 1)
                .AddContent("Parser error", addLine: 0, addColumn: 2);
            titleContent.AddLine();
            foreach (ViewScriptInfo script in _comparisonResult.ViewScriptInfos) {

                titleContent.AddContent(script.ViewScriptName, addLine: 0, addColumn: 0);
                titleContent.AddContent(script.ViewScript.ViewInfo.CompleteStatement, addLine: 0, addColumn: 1);
                IEnumerable<string> syntaxErrors = script.Errors.SyntaxErrors;

                foreach (string syntaxError in syntaxErrors) {
                    titleContent.AddContent(syntaxError, addLine: 0, addColumn: 2);
                    titleContent.AddLine();
                }
            }

            return titleContent;
        }

        private FileContent CreatePage3Content()
        {
            var titleContent = new FileContent()
                .AddContent("Database", addLine: 0, addColumn: 0)
                .AddContent("ViewScripts", addLine: 0, addColumn: 1)
                .AddContent(_comparisonResult.DBName);

            //overview information
            foreach (var script in _comparisonResult.ViewScriptInfos)
            {
                IEnumerable<string> allColumnNames = 
                    (from colList in _comparisonResult.GetMissingColumns(script).Values
                                  from colName in colList 
                                  select colName);

                titleContent.AddContent(string.Format("{0} (missing : {1} on {2})",
                        script.ViewScriptName,
                        allColumnNames.Distinct().Count(),
                        script.ObjectTree.Columns.Count), addLine: 0, addColumn: 1);
            }
            titleContent.AddLine();

            // show missing columns
            int colIndex = 0;
            int topLineIndex = (int)titleContent.GetCurrentPosition().Y;
            foreach (ViewScriptInfo viewInfo in _comparisonResult.ViewScriptInfos)
            {
                if (viewInfo.ObjectTree == null) continue;

                titleContent.SetCurrentLine(topLineIndex);
                colIndex++;
                foreach (var columnDico in _comparisonResult.GetMissingColumns(viewInfo))
                {
                    foreach (var colName in columnDico.Value.Distinct())
                    {
                        // display the column name + the "likely" belonging tables
                        titleContent.AddContent(colName + " ( " + string.Join(",", columnDico.Key) + " ) ", 0, colIndex).AddLine();
                    }
                }
            }

            return titleContent;
        }

        private FileContent CreatePage2Content()
        {
            var titleContent = new FileContent()
                .AddContent("Database", addLine: 0, addColumn: 0)
                .AddContent("ViewScripts", addLine: 0, addColumn: 1)
                .AddContent(_comparisonResult.DBName);

            //overview information
            foreach(var script in _comparisonResult.ViewScriptInfos)
                if (script.ObjectTree != null)
                titleContent.AddContent(string.Format("{0} (missing : {1} on {2})",
                        script.ViewScriptName, 
                        _comparisonResult.GetMissingTables(script).Count , 
                        script.ObjectTree.Tables.Count) , 
                    addLine: 0, addColumn: 1);

            titleContent.AddLine();

            // show missing tables
            int colIndex = 0;
            int topLineIndex = (int)titleContent.GetCurrentPosition().Y;
            foreach (ViewScriptInfo viewInfo in _comparisonResult.ViewScriptInfos)
            {
                if (viewInfo.ObjectTree == null) continue;

                titleContent.SetCurrentLine(topLineIndex);
                colIndex++;
                foreach (var tableName in _comparisonResult.GetMissingTables(viewInfo))
                {
                    titleContent.AddContent(tableName, 0, colIndex).AddLine();
                }
            }

            return titleContent;
        }

        private FileContent CreatePage1Content()
        {
            var titleContent = new FileContent()
                .AddContent("Database:", addLine: 0, addColumn: 0, border: Border.Left & Border.Bottom & Border.Top)
                .AddContent(_comparisonResult.DBName, addLine: 0, addColumn: 1, border: Border.Right & Border.Bottom & Border.Top);

            var pageContent = new FileContent()
                .AddContent("Script View", addLine: 1, addColumn: 0);

            foreach (var script in _comparisonResult.ViewScriptInfos)
            {
                bool isScriptCompatible = script.ObjectTree!= null && _comparisonResult.GetMissingTables(script).Count == 0
                    && _comparisonResult.GetMissingColumns(script).Count == 0;

                var style = new Style() { BackgroundColor = (isScriptCompatible) ? Color.Green : Color.Red, Name = (isScriptCompatible) ? "OK" : "NOTOK" };

                pageContent.AddContent(script.ViewScriptName, addLine: 1, addColumn: 0, style: style)
                    .AddContent((isScriptCompatible) ? "OK" : "Not OK", addLine: 0, addColumn: 1, style: style);

                if (script.Errors != null && (!string.IsNullOrEmpty(script.Errors.ErrorMessage)) || (script.Errors.SyntaxErrors != null && script.Errors.SyntaxErrors.Count > 0))
                {
                    pageContent.AddContent("Error: " + script.Errors.ErrorMessage + " Syntax error at : " + string.Join(",", script.Errors.SyntaxErrors) , addLine:0,addColumn:1);
                }
            }
            
            return pageContent;

        }
    }
}
