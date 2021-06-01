using System.IO;

namespace eBalanceKitBusiness.HyperCubes.Import.Templates.TemplateLoader
{
    class TempalteLoaderTextReader : TempalteLoader {
        private TextReader _xmlSource;
        public TempalteLoaderTextReader(TextReader textReader) { _xmlSource = textReader; Load();}
            
        public override void Load()
        {
            XmlAssignmentDoc.Load(_xmlSource);
        }
    }
}