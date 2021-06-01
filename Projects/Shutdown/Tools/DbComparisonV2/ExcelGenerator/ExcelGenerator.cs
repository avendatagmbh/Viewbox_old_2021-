using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;

namespace DocumentGenerator
{
    public class ExcelDocumentGenerator
    {
        Workbook _workbook;
        public ExcelDocumentGenerator(short workSheetNumber)
        {
            _workbook = new Workbook();
            while (_workbook.Worksheets.Count < workSheetNumber) _workbook.Worksheets.Add(); 
        }
        public Worksheet GetSheet(int index) { 
            Worksheet sheet= null;
            while (index > -1) {
                foreach (Worksheet item in _workbook.Worksheets) sheet = item;
                index--;
            }
            return sheet;
        }
    }
}
