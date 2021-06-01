using System.IO;

namespace eBalanceKitBusiness.HyperCubes.Import.Templates.TemplateGenerator
{
    public class TemplateGeneratorStream : TemplateGeneratorBase {
        private Stream _destination;
        public TemplateGeneratorStream(Stream destinationStream) { _destination = destinationStream; }


        public override void SaveXml() { XmlAssignmentDoc.Save(_destination); }
    }
}
