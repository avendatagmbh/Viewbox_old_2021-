using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Job.PdfExport;

namespace Viewbox.Test.PdfExport
{
    ///<summary>
    ///  This is a test class for PdfExporterConfig and is intended to contain all PdfExporterConfig Unit Tests
    ///</summary>
    [TestClass]
    public class PdfExporterConfigTest
    {
        private const string ConfigFileName = "PdfExporter.config";
        /*
		 * [bas] This method must be the first during the test run. It is not a nice pattern, but 
		 * the test uses static class with static member, which cannot be reset/destroy during the run.
		 * A solution can be to refactor the class to be a singleton. But it seems to me too much effort
		 * for this little task. Further information:
		 * http://stackoverflow.com/questions/531199/how-to-destroy-a-static-class-in-c-sharp
		 * http://csharpindepth.com/Articles/General/Singleton.aspx
		 * 
		 */

        [TestMethod]
        public void GetExporterNameWithoutConfigLoad()
        {
            Assert.AreEqual(
                "Viewbox.Job.PdfExport.General.LegacyPdfExporter",
                PdfExporterConfig.GetExporter("diaprod_final", "rechnungsnachdruck_dyn_view").Exporter,
                "The name of the exporter is wrong!");
        }

        [TestMethod]
        public void GetExporterNameIfExistTest()
        {
            PdfExporterConfig.Load(ConfigFileName);

            Assert.AreEqual(
                "SimplePdfExporter",
                PdfExporterConfig.GetExporter("diaprod_final", "rechnungsnachdruck_dyn_view").Exporter,
                "The name of the exporter is wrong!");

            Assert.AreEqual(
                "ActebisFacturaPdfExporter",
                PdfExporterConfig.GetExporter("actebis_sap_pe6", "rechnungsnachdruck_dyn_view").Exporter,
                "The name of the exporter is wrong!");
            Assert.AreEqual(
                "ActebisKreditnotePdfExporter",
                PdfExporterConfig.GetExporter("actebis_sap_pe6", "rechnungsnachdruck_2_dyn_view").Exporter,
                "The name of the exporter is wrong!");
        }

        [TestMethod]
        public void GetExporterNameIfNotExistTest()
        {
            PdfExporterConfig.Load(ConfigFileName);
            Assert.AreEqual(
                "Viewbox.Job.PdfExport.General.LegacyPdfExporter", PdfExporterConfig.GetExporter("", "").Exporter,
                "The name of the exporter is wrong!");
        }
    }
}