using System.Drawing;
using System.Linq;
using EO.Pdf.Acm;
using Resx = ViewBuilderBusiness.Resources.Resource;

namespace ViewBuilderBusiness.Reports
{
    internal class EOPdfTableHelper
    {
        private readonly float[] _colWidths;
        private readonly AcmHorizontalAlign[] _hAlign;
        private readonly AcmTable table;
        private int _rowCount;

        public EOPdfTableHelper(float[] sizes)
        {
            _colWidths = sizes;
            table = new AcmTable(sizes);
            table.Style.HorizontalAlign = AcmHorizontalAlign.Left;
//            table.Style.Padding = null;
            table.CellPadding = null;
            table.CellSpacing = null;
//            table.Style.Padding = new AcmPadding(0f);
            table.Style.Padding.Top = 0.2f;
            table.Style.Padding.Bottom = 0.2f;
            table.Style.Margin = null;
            //table.Style.Border = new AcmBorder(0f);
        }

        public EOPdfTableHelper(string[] headers, float[] sizes, object[] hAlign) : this(sizes)
        {
            _hAlign = hAlign.OfType<AcmHorizontalAlign>().ToArray();
            table.Style.HorizontalAlign = AcmHorizontalAlign.Left;
            table.Style.Padding.Top = 0.2f;
            table.Style.Padding.Bottom = 0.2f;
            //table.Style.Padding = null;
//            table.Style.Padding = new AcmPadding(0f);
//            table.Style.Padding.Top = 0.2f;
//            table.Style.Padding.Bottom = 0.2f;
            table.Style.Margin = null;
            //Create the first row and cells
            AcmTableRow row = new AcmTableRow();
            row.Style.BackgroundColor = Color.LightSkyBlue;
            row.Style.Border = null;
            row.Style.Padding = null;
//            row.Style.Border = new AcmBorder(0.01f);
//            row.Style.Padding = new AcmPadding(0.03f);
            row.Style.Margin = null;
            table.Rows.Add(row);
            ShowBorder = true;
            for (int i = 0; i < headers.Length; i++)
            {
                row.Cells.Add(CreateCell(headers[i], _hAlign[i]));
            }
        }

        public bool ShowBorder { get; set; }

        private AcmTableCell CreateCell(string text, AcmHorizontalAlign pos)
        {
            return new AcmTableCell(new AcmText(text))
                       .SetProperty("Style.HorizontalAlign", pos)
                       .SetProperty("Style.Padding", new AcmPadding(0.05f, 0, 0, 0))
                       .SetProperty("Style.Margin", null)
                       .SetProperty("Style.Border",
                                    ShowBorder
                                        ? new AcmBorder(new AcmLineInfo(AcmLineStyle.Solid, Color.Black, 0.001f))
                                        : null) as AcmTableCell;
        }

        public void AddRow(string[] rowItems)
        {
            var row = new AcmTableRow();
//            if(ShowBorder)
//                row.Style.Border = new AcmBorder(0.01f);
//            else
            row.Style.Border = null;
            row.Style.Padding = null; ////new AcmPadding(0.03f);
            row.Style.Margin = null;
            table.Rows.Add(row);

            for (int i = 0; i < rowItems.Length; i++)
            {
                var text = rowItems[i];
                // for the last column text : replaces the _ and . with space to allow the text to wrap
                if (!string.IsNullOrEmpty(text) && i == rowItems.Length - 1 && text.Length > 25)
                    text = string.Join(" ", text.Split('_', '.'));
                row.Cells.Add(CreateCell(text,
                                         ((_hAlign != null && _hAlign.Length > i)
                                              ? _hAlign[i]
                                              : AcmHorizontalAlign.Left)));
            }
            _rowCount++;
        }

        internal AcmTable GetTable()
        {
            if (_rowCount == 0) // display empty rows
            {
                var row = new AcmTableRow();
                row.Style.Border = new AcmBorder(0.01f);
                var cell = new AcmTableCell(new AcmText(Resx.NoRows)) {ColSpan = _colWidths.Length};
                cell.Style.HorizontalAlign = AcmHorizontalAlign.Center;
                row.Cells.Add(cell);
                table.Rows.Add(row);
            }
            return table;
        }
    }
}