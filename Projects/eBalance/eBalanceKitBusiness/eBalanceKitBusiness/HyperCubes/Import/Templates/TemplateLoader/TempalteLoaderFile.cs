namespace eBalanceKitBusiness.HyperCubes.Import.Templates.TemplateLoader
{
    public class TempalteLoaderFile : TempalteLoader {
        private string _xmlSource;
        public TempalteLoaderFile(string filename)
        {
            _xmlSource = filename;
            Load();
        }
            
        public override void Load()
        {
            XmlAssignmentDoc.Load(_xmlSource);
        }
    }
}
