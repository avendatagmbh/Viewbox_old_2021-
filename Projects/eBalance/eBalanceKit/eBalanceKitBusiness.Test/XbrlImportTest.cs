using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utils;
using eBalanceKit.Models.Import;
using eBalanceKit.Windows.Import;
using eBalanceKitBusiness.Import;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitBusiness.Reconciliation.ViewModels;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Test
{
    [TestClass]
    public class XbrlImportTest
    {
        private const string ResourcePath = "Resources\\XbrlImport";
        private string path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), ResourcePath);
        private XbrlImporter _importer;
        private XbrlImporter _importer2;
        private XbrlImporter _importer4;
        private XbrlImporter _importer3;

        /// <summary>
        ///A test for Xbrl import
        ///</summary>

        [TestMethod]
        public void TestImport() {

            string file1Path = Path.Combine(path, "Trade-Off GmbH_2011_Jahresabschluss 2011.xml");
            string file2Path = Path.Combine(path, "Avendata_2012_Kerntaxonomie gültig.xml");
            string file4Path = Path.Combine(path, "TextFile.txt");
            string file3PathError = Path.Combine(path, "error_file.xml");

            User adminUser = TestHelper.LogonAsAdmin();
            Structures.DbMapping.System system = TestHelper.CreateSystem();

            try {

                FileStream stream1 = new FileStream(file1Path, FileMode.Open, FileAccess.ReadWrite);
                FileStream stream2 = new FileStream(file2Path, FileMode.Open, FileAccess.ReadWrite);
                FileStream stream4 = new FileStream(file4Path, FileMode.Open, FileAccess.ReadWrite);
                FileStream stream3 = new FileStream(file3PathError, FileMode.Open, FileAccess.ReadWrite);

                #region [ Prepare ]

                string FinancialYear;
                string Company;
                Tuple<string, string> valuesForDialog = null;

                #endregion [ Prepare ]

                #region [ Step 1 ]

                _importer = new XbrlImporter(stream1);
                valuesForDialog = _importer.GetXbrlDetailsForDialog();
                FinancialYear = valuesForDialog.Item2;
                Company = valuesForDialog.Item1;

                if(FinancialYear == null || Company == null) {
                    Assert.Fail("No Company or Financial Year info");
                }

                _importer.Import("Testing", system);

                if (_importer.HasErrors) {
                    Assert.Fail(String.Join(", ", _importer.XbrlImportErrors));
                }

                #endregion [ Step 1 ]

                #region [ Step 2 ]

                _importer2 = new XbrlImporter(stream2);
                valuesForDialog = _importer2.GetXbrlDetailsForDialog();
                FinancialYear = valuesForDialog.Item2;
                Company = valuesForDialog.Item1;

                if (FinancialYear == null || Company == null) {
                    Assert.Fail("No Company or Financial Year info");
                }

                _importer2.Import("Testing2", system);

                if (_importer2.HasErrors) {
                    Assert.Fail(String.Join(", ", _importer2.XbrlImportErrors));
                }

                #endregion [ Step 2 ]

                #region [ Step 4 ]

                _importer4 = new XbrlImporter(stream4);
                valuesForDialog = _importer4.GetXbrlDetailsForDialog();
                FinancialYear = valuesForDialog.Item2;
                Company = valuesForDialog.Item1;

                if (FinancialYear == null || Company == null)
                {
                    Assert.Fail("No Company or Financial Year info");
                }

                _importer4.Import("Testing4", system);

                if (_importer4.HasErrors) {
                    Assert.Fail(String.Join(", ", _importer4.XbrlImportErrors));
                }

                #endregion [ Step 4 ]

                #region [ Step 3 ]

                _importer3 = new XbrlImporter(stream3);
                valuesForDialog = _importer.GetXbrlDetailsForDialog();
                FinancialYear = valuesForDialog.Item2;
                Company = valuesForDialog.Item1;

                if (FinancialYear == null || Company == null) {
                    Assert.Fail("No Company or Financial Year info");
                }

                _importer3.Import("Testing3", system);

                if (_importer3.HasErrors) {
                    Assert.Fail(String.Join(", ", _importer3.XbrlImportErrors));
                }

                #endregion [ Step 3 ]
            }
            catch (Exception ex) {
                Assert.Fail(ex.Message);
            }

            #region [ Cleanup ]

            finally {
                try {
                    if (_importer != null)
                        TestHelper.DeleteDocument(_importer.Document);
                }
                catch {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }

                try {
                    if (_importer2 != null)
                        TestHelper.DeleteDocument(_importer2.Document);
                }
                catch {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }

                try {
                    if (_importer4 != null)
                        TestHelper.DeleteDocument(_importer4.Document);
                }
                catch {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }

                try
                {
                    if (_importer3 != null)
                        TestHelper.DeleteDocument(_importer3.Document);
                }
                catch
                {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }

                try {
                    if (_importer != null)
                        TestHelper.DeleteCompany(_importer.Company);
                }
                catch {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }

                try {
                    if (_importer2 != null)
                        TestHelper.DeleteCompany(_importer2.Company);
                }
                catch {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }

                try {
                    if (_importer4 != null)
                        TestHelper.DeleteCompany(_importer4.Company);
                }
                catch {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }

                try {
                    if (_importer3 != null)
                        TestHelper.DeleteCompany(_importer3.Company);
                }
                catch {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }
                
                try {
                    if (system != null)
                        TestHelper.DeleteSystem(system);
                }
                catch {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }
            }

            #endregion [ Cleanup ]   
        }
    }
}
