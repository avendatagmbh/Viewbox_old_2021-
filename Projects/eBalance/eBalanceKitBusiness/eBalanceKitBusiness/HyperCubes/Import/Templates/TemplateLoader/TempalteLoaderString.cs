namespace eBalanceKitBusiness.HyperCubes.Import.Templates.TemplateLoader
{
   public class TempalteLoaderString : TempalteLoader {
        private string _xmlString;
        public TempalteLoaderString(string xml)
        {
            _xmlString = xml;
            Load();
        }
            
        public override void Load()
        {
            XmlAssignmentDoc.LoadXml(_xmlString);
        }
    }
}