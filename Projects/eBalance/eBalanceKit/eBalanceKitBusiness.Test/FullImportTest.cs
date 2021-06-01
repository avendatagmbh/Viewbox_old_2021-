using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utils;
using eBalanceKit.Models.Import;
using eBalanceKit.Windows.Import;
using eBalanceKitBusiness.Export;
using eBalanceKitBusiness.Import;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitBusiness.Reconciliation.ViewModels;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Test
{
    [TestClass]
    public class FullImportTest
    {
        private const string ResourcePath = "Resources\\FullImport";

        private string path =
            Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), ResourcePath);

        private FullImporter _fullImporter1;
        private XbrlImporter _xbrlImporter1;
        private FullImporter _fullImporter2;
        private XbrlImporter _xbrlImporter2;
        private FullImporter _fullImporter3;
        private XbrlImporter _xbrlImporter3;

        private XbrlImporter _importer3;

        public string FinancialYear { get; set; }
        public XmlNode Company { get; set; }
        public XmlNode MySystem { get; set; }
        public XmlNode Report { get; set; }

        /// <summary>
        ///A test for full import
        ///</summary>

        [TestMethod]
        public void TestFullImport() {

            string file1Path = Path.Combine(path, "erweitert_aa_2012_Version interesting.xml");
            string file2Path = Path.Combine(path, "TextFile.txt");
            string file3PathError = Path.Combine(path, "error_file.xml");

            User adminUser = TestHelper.LogonAsAdmin();
            Structures.DbMapping.System system = TestHelper.CreateSystem();

            
            try {

                FileStream stream1 = new FileStream(file1Path, FileMode.Open, FileAccess.ReadWrite);
                FileStream stream2 = new FileStream(file2Path, FileMode.Open, FileAccess.ReadWrite);
                FileStream stream3 = new FileStream(file3PathError, FileMode.Open, FileAccess.ReadWrite);

                #region [ Prepare ]

                Tuple<XmlNode, string, XmlNode, XmlNode> valuesForDialog;

                #endregion [ Prepare ]

                #region [ Step 1 ]

                _xbrlImporter1 = new XbrlImporter(stream1);

                valuesForDialog = _xbrlImporter1.GetFullDetailsForDialog();

                if (valuesForDialog == null) {
                    Assert.Fail("Missing root in xml: " + stream1.Name);
                }

                if (valuesForDialog.Item2 != null) {
                    FinancialYear = valuesForDialog.Item2;
                }
                if (valuesForDialog.Item1 != null) {
                    Company = valuesForDialog.Item1;
                }
                if (valuesForDialog.Item3 != null) {
                    MySystem = valuesForDialog.Item3;
                }
                if (valuesForDialog.Item4 != null && valuesForDialog.Item4.Attributes != null) {
                    Report = valuesForDialog.Item4;
                }

                if (FinancialYear == null || Company == null || MySystem == null || Report == null) {
                    Assert.Fail("Missing data in xml!");
                }

                _fullImporter1 = new FullImporter(stream1);
                _fullImporter1.Import(Report, MySystem, Company, FinancialYear, stream1);

                if (_fullImporter1.HasErrors) {
                    Assert.Fail(String.Join(", ", _fullImporter1.XbrlImportErrors));
                }


                //export
                string exportPath = path + "\\testFullExport.xml";

                XbrlExporter.ExportXbrl(exportPath, _fullImporter1.XbrlImporter.Document, null, true);
                FullExporter exporter = new FullExporter();
                exporter.FullExport(exportPath, _fullImporter1.XbrlImporter.Document);

                #endregion [ Step 1 ]


                /*
                #region [ Step 2 ]
                
                _xbrlImporter2 = new XbrlImporter(stream2);

                valuesForDialog = _xbrlImporter2.GetFullDetailsForDialog();

                if (valuesForDialog == null) {
                    Assert.Fail("Missing root in xml: " + stream2.Name);
                }
                
                if (valuesForDialog.Item2 != null) {
                    FinancialYear = valuesForDialog.Item2;
                }
                if (valuesForDialog.Item1 != null) {
                    Company = valuesForDialog.Item1;
                }
                if (valuesForDialog.Item3 != null) {
                    MySystem = valuesForDialog.Item3;
                }
                if (valuesForDialog.Item4 != null && valuesForDialog.Item4.Attributes != null) {
                    Report = valuesForDialog.Item4;
                }

                if (FinancialYear == null || Company == null || MySystem == null || Report == null) {
                    Assert.Fail("Missing data in xml!");
                }

                _fullImporter2 = new FullImporter(stream2);
                _fullImporter2.Import(Report, MySystem, Company, FinancialYear, stream2);

                if (_fullImporter2.HasErrors) {
                    Assert.Fail(String.Join(", ", _fullImporter2.XbrlImportErrors));
                }

                #endregion [ Step 2 ]


                #region [ Step 3 ]

                _xbrlImporter3 = new XbrlImporter(stream2);

                valuesForDialog = _xbrlImporter3.GetFullDetailsForDialog();

                if (valuesForDialog == null) {
                    Assert.Fail("Missing root in xml: " + stream3.Name);
                }

                if (valuesForDialog.Item2 != null) {
                    FinancialYear = valuesForDialog.Item2;
                }
                if (valuesForDialog.Item1 != null) {
                    Company = valuesForDialog.Item1;
                }
                if (valuesForDialog.Item3 != null) {
                    MySystem = valuesForDialog.Item3;
                }
                if (valuesForDialog.Item4 != null && valuesForDialog.Item4.Attributes != null) {
                    Report = valuesForDialog.Item4;
                }

                if (FinancialYear == null || Company == null || MySystem == null || Report == null) {
                    Assert.Fail("Missing data in xml!");
                }

                _fullImporter3 = new FullImporter(stream3);
                _fullImporter3.Import(Report, MySystem, Company, FinancialYear, stream2);

                if (_fullImporter3.HasErrors) {
                    Assert.Fail(String.Join(", ", _fullImporter3.XbrlImportErrors));
                }

                #endregion [ Step 3 ]
                */

            } catch (Exception ex) {
                Assert.Fail(ex.Message);
            }

                #region [ Cleanup ]

            finally {
                try {
                    if (_fullImporter1 != null)
                        TestHelper.DeleteDocument(_fullImporter1.XbrlImporter.Document);
                } catch {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }

                try {
                    if (_fullImporter2 != null)
                        TestHelper.DeleteDocument(_fullImporter2.XbrlImporter.Document);
                }
                catch {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }

                try {
                    if (_fullImporter3 != null)
                        TestHelper.DeleteDocument(_fullImporter3.XbrlImporter.Document);
                }
                catch {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }

                try
                {
                    if (system != null)
                        TestHelper.DeleteSystem(system);
                }
                catch
                {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }
            }

            #endregion [ Cleanup ]
        }
    }
}
