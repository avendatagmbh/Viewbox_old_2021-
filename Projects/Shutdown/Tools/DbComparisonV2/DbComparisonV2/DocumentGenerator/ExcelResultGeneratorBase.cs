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

    public abstract class ExcelResultGeneratorBase : IDocument
    {

        protected MSExcel.Workbook _workbook;
        protected MSExcel.Application _app;
        protected List<dynamic> _sheets;// keeps track of COM objects that should be disposed
        protected bool disposed = false;
        protected static List<string> _excelStyles = new List<string>();

        #region constructors

        public ExcelResultGeneratorBase()
        {
            _app = new MSExcel.Application();
            _workbook = _app.Workbooks.Add();
        }
        #endregion


        #region Methods
        protected abstract void CreateContent();

        protected void AddSheets(short sheetsNumber)
        {
            _sheets = new List<dynamic>(sheetsNumber);
            while (_workbook.Worksheets.Count < sheetsNumber) _sheets.Add(_workbook.Worksheets.Add());
        }

        protected void AddContentToSheet(FileContent pageContent, MSExcel.Worksheet worksheet) {
            AddContentToSheet(pageContent, worksheet, "");
        }
        protected void AddContentToSheet(FileContent pageContent, MSExcel.Worksheet worksheet, string sheetName)
        {
            if (pageContent == null || worksheet == null) throw new ArgumentNullException("pageContent or worksheet are null.");

            worksheet.Name = sheetName;

            foreach (var content in pageContent.SheetContent)
            {
                content.Location.Row++;
                content.Location.Column++;
                string contentText = content.Content.ToString();

                if(content.Style != null && content.Style.BackgroundColor != System.Drawing.Color.Empty )
                    worksheet.Cells[content.Location.Row, content.Location.Column].Interior.Color = System.Drawing.ColorTranslator.ToOle(content.Style.BackgroundColor);
                worksheet.Cells[content.Location.Row, content.Location.Column].Value2 = contentText;
                
                worksheet.Cells[content.Location.Row, content.Location.Column].Columns.AutoFit();
            }
            pageContent.ClearAll();
        }
        protected MSExcel.Style GetStyle(Style fileContentntStyle) {

            if (fileContentntStyle != null) {
                if (!_excelStyles.Contains(fileContentntStyle.Name))
                    _excelStyles.Add(fileContentntStyle.Name);
            }
            return null;
        }
        /// <summary>
        /// Full path file name without extension.
        /// </summary>
        /// <param name="filename"></param>
        public void WriteToFile(string filename)
        {
            CreateContent();

            _workbook.SaveAs(filename + ".xls");
        }
        /// <summary>
        /// disposes all excel objects
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    if (_sheets != null)
                        _sheets.ForEach(s => Marshal.ReleaseComObject(s));
                    if (_workbook != null)
                    {
                        _workbook.Close();
                        Marshal.ReleaseComObject(_workbook);
                    }
                    if (_app != null)
                    {
                        _app.Quit();
                        Marshal.ReleaseComObject(_app);
                    }
                }
                disposed = true;
            }
        }
        //~ExcelDocumentGenerator() 
        //{
        //    Dispose(false);
        //}
        public void Dispose()
        {
            Dispose(true);
            //GC.SuppressFinalize(this);
        }
        #endregion
    }
}
