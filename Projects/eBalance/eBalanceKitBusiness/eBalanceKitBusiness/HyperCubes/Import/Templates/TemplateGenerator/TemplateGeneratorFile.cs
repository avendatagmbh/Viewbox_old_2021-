namespace eBalanceKitBusiness.HyperCubes.Import.Templates.TemplateGenerator
{
    public class TemplateGeneratorFile : TemplateGeneratorBase
    {
        private string _destination;
        public TemplateGeneratorFile(string destinationFile) { _destination = destinationFile; }


        public override void SaveXml() { XmlAssignmentDoc.Save(_destination); }
    }
}
