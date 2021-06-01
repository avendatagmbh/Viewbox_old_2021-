using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTextSharp.text;

namespace BelegeArchivePDFGenerator.PdfGenerator
{
    public interface IPDFGenerator
    {
        void Generate();
    }
}
