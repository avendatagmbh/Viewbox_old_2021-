using System.Xml;

namespace eBalanceKitBusiness.HyperCubes.Import.Templates.TemplateLoader
{
    class XmlLoaderXmlReader : TempalteLoader {
        private XmlReader _xmlSource;
        public XmlLoaderXmlReader(XmlReader xmlReader) { _xmlSource = xmlReader; Load(); }
            
        public override void Load()
        {
            XmlAssignmentDoc.Load(_xmlSource);
        }
    }
}