using System.IO;

namespace eBalanceKitBusiness.HyperCubes.Import.Templates.TemplateLoader
{
    class TempalteLoaderStream : TempalteLoader {
        private Stream _xmlSource;
        public TempalteLoaderStream(Stream stream) { _xmlSource = stream; Load(); }
            
        public override void Load()
        {
            XmlAssignmentDoc.Load(_xmlSource);
        }
    }
}