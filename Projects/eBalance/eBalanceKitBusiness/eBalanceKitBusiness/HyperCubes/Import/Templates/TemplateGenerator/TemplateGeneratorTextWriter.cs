using System.IO;

namespace eBalanceKitBusiness.HyperCubes.Import.Templates.TemplateGenerator
{
    public class TemplateGeneratorTextWriter : TemplateGeneratorBase
    {
        private TextWriter _destination;
        public TemplateGeneratorTextWriter(TextWriter destinationTextWriter) { _destination = destinationTextWriter; }


        public override void SaveXml() { XmlAssignmentDoc.Save(_destination); }
    }
}
