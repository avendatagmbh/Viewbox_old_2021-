namespace ViewBuilderBusiness.Structures.Pdf
{
    public class PdfLogWriter
    {
        #region Constructor

        public PdfLogWriter(string filename)
        {
            FileName = filename;
        }

        #endregion Constructor

        #region Properties

        protected string FileName { get; set; }

        #endregion Properties

        #region Methods

        public void WritePdf()
        {
        }

        #endregion Methods
    }
}