using EO.Pdf;

namespace ViewBuilderBusiness.Reports
{
    internal class EOPdfDocumentHelper
    {
        private PdfDocument _document;
        private string _fileName;
        private TOCHelper _toc;

        public EOPdfDocumentHelper(PdfDocument document, string fileName)
        {
            _document = document;
            _fileName = fileName;
        }
    }
}