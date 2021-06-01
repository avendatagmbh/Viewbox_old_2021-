using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BelegeArchivePDFGenerator.DataModels;
using iTextSharp.text;

namespace BelegeArchivePDFGenerator.PdfGenerator
{
    public abstract class APDFGenerator : IPDFGenerator
    {
        protected DataModel row;

        public APDFGenerator(DataModel row)
        {
            this.row = row;
        }

        public virtual void Generate() { }
    }
}
