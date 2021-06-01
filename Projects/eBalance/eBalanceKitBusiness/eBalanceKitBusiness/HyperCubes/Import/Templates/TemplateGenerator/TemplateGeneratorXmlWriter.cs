using System.Xml;

namespace eBalanceKitBusiness.HyperCubes.Import.Templates.TemplateGenerator
{
    public class TemplateGeneratorXmlWriter : TemplateGeneratorBase
    {
        private XmlWriter _destination;
        public TemplateGeneratorXmlWriter(XmlWriter destinationXmlWriter) { _destination = destinationXmlWriter; }


        public override void SaveXml() { XmlAssignmentDoc.Save(_destination); }
    }
}