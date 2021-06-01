using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using eBalanceKitBusiness.Structures.DbMapping;
using Utils;
using eBalanceKitBusiness.Manager;
using System.Windows;
using eBalanceKitBusiness.Reconciliation;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures.DbMapping.ValueMappings;
using eBalanceKitBusiness.Structures.ValueTree;
using System.Collections.Generic;
using System.Linq;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitBusiness.Reconciliation.Enums;

namespace eBalanceKitBusiness.Test
{
    
    
    /// <summary>
    ///This is a test class for ReconciliationImportModelTest and is intended
    ///to contain all ReconciliationImportModelTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ReconciliationImportModelTest {

        private const string resourcePath = "Resources\\ReconciliationImport";
        private string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), resourcePath);
        
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for ExampleFile
        ///</summary>
        //[TestMethod()]
        public void ExampleFileTest() {
            //// Creation of the private accessor for 'Microsoft.VisualStudio.TestTools.TypesAndSymbols.Assembly' failed
            //Assert.Inconclusive("Creation of the private accessor for \'Microsoft.VisualStudio.TestTools.TypesAndSy" +
            //        "mbols.Assembly\' failed");
            ReconciliationImportModel model = new ReconciliationImportModel(null);
            PrivateObject x = new PrivateObject(model);
            Type[] paramTypes = new Type[1] { typeof(object) };
            Object[] paramObjects = new Object[1] { null };
            object o = x.Invoke("ExampleFile", paramTypes, paramObjects);
        }

        /// <summary>
        ///A test for reconciliation position import
        ///</summary>
        [TestMethod()]
        public void ReconciliationImport_PositionsTest() {

            string file1Path = System.IO.Path.Combine(path, "ReconciliationValueImportExample.csv");
            string file2PathWithErrors = System.IO.Path.Combine(path, "ReconciliationValueImportExample_error.csv");
            string file3PathInvalid = System.IO.Path.Combine(path, "ReconciliationValueImportExample_1text.csv");
            string file4PathInvalid = System.IO.Path.Combine(path, "ReconciliationValueImportExample_4text.csv");

            Structures.DbMapping.System system = null;
            User adminUser = TestHelper.LogonAsAdmin();
            Company company = null;
            Document document = null;

            try {

                #region [ Prepare ]

                system = TestHelper.CreateSystem();
                company = TestHelper.CreateCompany();
                document = TestHelper.CreateDocument();

                #endregion [ Prepare ]

                #region [ Init document ]

                //document.ReportRights = new ReportRights(document);
                //ValueTree valueTreeGcd = ValueTree.CreateValueTree(typeof(ValuesGCD), document, document.GcdTaxonomy, null);
                //ValueTree valueTreeMain = ValueTree.CreateValueTree(typeof(ValuesGAAP), document, document.MainTaxonomy, null);
                //ReconciliationManager reconciliationManager = new ReconciliationManager(document);
                //PrivateObject privateDocument = new PrivateObject(document);
                //privateDocument.SetFieldOrProperty("ReconciliationManager", reconciliationManager);
                //privateDocument.SetFieldOrProperty("ValueTreeGcd", valueTreeGcd);
                //privateDocument.SetFieldOrProperty("ValueTreeMain", valueTreeMain);
                //Type[] paramTypesDoc = new Type[] { };
                //Object[] paramObjectsDoc = new Object[] { };
                //object retValInvokeDoc = privateDocument.Invoke("InitValueTreeMain", paramTypesDoc, paramObjectsDoc);

                TestHelper.InitDocument(document);

                #endregion [ Init document ]

                ObjectWrapper<Document> documentWrapper = new ObjectWrapper<Document>();
                documentWrapper.Value = document;
                //document.ReconciliationManager = new ReconciliationManager(document);
                ReconciliationsModel reconciliationsModel = new ReconciliationsModel(documentWrapper.Value);
                ReconciliationImportModel importModel = new ReconciliationImportModel(reconciliationsModel);

                #region [ Step 1 add file / remove file / add file ]
                
                // check if everithing is in init state
                if (importModel.HasFiles || importModel.IsStepErrorsEnabled || importModel.IsStepPreviewEnabled)
                    Assert.Fail("Step 1");
                importModel.CsvFiles.Add(file1Path);
                if (!importModel.HasFiles || !importModel.IsStepErrorsEnabled || !importModel.IsStepPreviewEnabled)
                    Assert.Fail("Step 1");
                //TODO: select file, check HasSelectedFiles and remove selected
                importModel.CsvFiles.RemoveAt(0);
                // check if everithing is in init state
                if (importModel.HasFiles || importModel.IsStepErrorsEnabled || importModel.IsStepPreviewEnabled)
                    Assert.Fail("Step 1");
                importModel.CsvFiles.Add(file1Path);

                #endregion [ Step 1 add file / remove file / add file ]

                if (importModel.HasErrors.HasValue)
                    Assert.Fail("After Step 1");

                #region [ Step 2 ]

                int currentStep = 2;
                if (importModel.ValidateAndSetDialogPage(currentStep)) {
                    importModel.CreatePreviewData();
                } else {
                    Assert.Fail("Go to Step 2");
                }

                if (importModel.Preview == null)
                    Assert.Fail("Step 2: Preview is null");
                if (importModel.PreviewData != null || importModel.Importer != null)
                    Assert.Fail("Step 2: PreviewData or Importer failed");
                if (!importModel.HasFiles || !importModel.IsStepErrorsEnabled || !importModel.IsStepPreviewEnabled)
                    Assert.Fail("Step 2: property check");
                
                #endregion [ Step 2 ]

                #region [ Step 1 / 2: navigate back and add new file ]

                currentStep = 1;
                if (importModel.ValidateAndSetDialogPage(currentStep)) {
                    ;
                } else {
                    Assert.Fail("Go to Step 1");
                }
                importModel.CsvFiles.Add(file2PathWithErrors);
                importModel.CsvFiles.Add(file3PathInvalid);
                importModel.CsvFiles.Add(file4PathInvalid);
                if (!importModel.HasFiles || !importModel.IsStepErrorsEnabled || !importModel.IsStepPreviewEnabled)
                    Assert.Fail("Step 1");
                
                #endregion [ Step 1 / 2: navigate back and add new file ]

                #region [ Step 2 / 2 ]

                currentStep = 2;
                if (importModel.ValidateAndSetDialogPage(currentStep)) {
                    importModel.CreatePreviewData();
                } else {
                    Assert.Fail("Go to Step 2 / 2");
                }

                if (importModel.Preview == null)
                    Assert.Fail("Step 2: Preview is null");
                if (importModel.PreviewData != null || importModel.Importer != null)
                    Assert.Fail("Step 2: PreviewData or Importer failed");
                if (!importModel.HasFiles || !importModel.IsStepErrorsEnabled || !importModel.IsStepPreviewEnabled)
                    Assert.Fail("Step 2: property check");

                PrivateObject importPreview = new PrivateObject(importModel.Preview);
                Type[] paramTypes = new Type[] { };
                Object[] paramObjects = new Object[] { };
                object retValInvoke = importPreview.Invoke("UpdatePreview", paramTypes, paramObjects);
                if (importModel.PreviewData == null || importModel.PreviewData.TableName != file1Path)
                    Assert.Fail("PreviewData failed");

                // check Preview.IsPreviousAllowed and Preview.IsNextAllowed
                if (importModel.Preview.IsPreviousAllowed)
                    Assert.Fail("Step 2: property IsPreviousAllowed");
                if (!importModel.Preview.IsNextAllowed)
                    Assert.Fail("Step 2: property IsNextAllowed");
                importModel.Preview.Next();
                importModel.Preview.Next();
                importModel.Preview.Next();
                retValInvoke = importPreview.Invoke("UpdatePreview", paramTypes, paramObjects);
                if (importModel.PreviewData == null || importModel.PreviewData.TableName != file4PathInvalid)
                    Assert.Fail("PreviewData failed");
                if (!importModel.Preview.IsPreviousAllowed)
                    Assert.Fail("Step 2: property IsPreviousAllowed");
                if (importModel.Preview.IsNextAllowed)
                    Assert.Fail("Step 2: property IsNextAllowed");
                importModel.Preview.Previous();
                retValInvoke = importPreview.Invoke("UpdatePreview", paramTypes, paramObjects);
                if (importModel.PreviewData == null || importModel.PreviewData.TableName != file3PathInvalid)
                    Assert.Fail("PreviewData failed");
                importModel.Preview.Previous();
                importModel.Preview.Previous();
                importModel.Preview.Previous();
                importModel.Preview.Previous();
                retValInvoke = importPreview.Invoke("UpdatePreview", paramTypes, paramObjects);
                if (importModel.PreviewData == null || importModel.PreviewData.TableName != file1Path)
                    Assert.Fail("PreviewData failed");
                if (importModel.Preview.IsPreviousAllowed)
                    Assert.Fail("Step 2: property IsPreviousAllowed");
                if (!importModel.Preview.IsNextAllowed)
                    Assert.Fail("Step 2: property IsNextAllowed");
                
                #endregion [ Step 2 / 2 ]

                #region [ Step 3 ]

                currentStep = 3;
                if (importModel.ValidateAndSetDialogPage(currentStep)) {
                    //importModel.PreviewImport(new Window());
                    PrivateObject previewImport = new PrivateObject(importModel);
                    Object[] paramObjectsPreviewImport = new Object[] { };
                    object retValInvokePreviewImport = previewImport.Invoke("PreviewImport", paramObjectsPreviewImport);
                } else {
                    Assert.Fail("Go to Step 3");
                }

                if (importModel.Importer == null)
                    Assert.Fail("Step 3: Importer failed");

                PrivateObject importer = new PrivateObject(importModel.Importer);
                Dictionary<string, List<Tuple<string, string, TransferKinds, List<Tuple<Taxonomy.IElement, decimal?, decimal?>>>>> retValInvokeImporter = importer.GetFieldOrProperty("ImportedPositions") as Dictionary<string, List<Tuple<string, string, TransferKinds, List<Tuple<Taxonomy.IElement, decimal?, decimal?>>>>>;

                Assert.IsNotNull(retValInvokeImporter, "Step 3 retValInvokeImporter");

                Assert.IsTrue(retValInvokeImporter.Count == 2, "Step 3: Import failed, file number mismatch");
                Assert.IsTrue(importModel.Importer.TaxonomyIdErrors.Count == 1, "Step 3: Import failed, number of TaxonomyIdErrors mismatch");
                Assert.IsTrue(importModel.Importer.TaxonomyValueErrors.Count == 1, "Step 3: Import failed, number of TaxonomyValueErrors mismatch");
                Assert.IsTrue(importModel.Importer.ImportRowErrors.Count() == 2, "Step 3 ImportRowErrors");
                Assert.IsTrue(importModel.Importer.ImportRowErrors.Where(e => e.LineNumber == -1).Count() == 2, "Step 3 ImportRowErrors");
                Assert.IsTrue(retValInvokeImporter.ContainsKey(file1Path), "Step 3: Import failed, file name mismatch 1");
                Assert.IsTrue(retValInvokeImporter.ContainsKey(file2PathWithErrors), "Step 3: Import failed, file name mismatch 2");
                Assert.IsTrue(!retValInvokeImporter.ContainsKey(file3PathInvalid), "Step 3: Import failed, file name mismatch 3");
                Assert.IsTrue(!retValInvokeImporter.ContainsKey(file4PathInvalid), "Step 3: Import failed, file name mismatch 4");

                List<Tuple<string, string, TransferKinds, List<Tuple<Taxonomy.IElement, decimal?, decimal?>>>> file1ImportedValues = retValInvokeImporter[file1Path];
                List<Tuple<string, string, TransferKinds, List<Tuple<Taxonomy.IElement, decimal?, decimal?>>>> file2ImportedValues = retValInvokeImporter[file2PathWithErrors];
                Assert.IsTrue(file1ImportedValues.Count == 2, "Step 3 imported 1/1");
                Assert.IsTrue(file2ImportedValues.Count == 1, "Step 3 imported 1/2");
                // check transfer kind
                Assert.IsTrue(file1ImportedValues[0].Item3 == TransferKinds.ValueChange, "Step 3 transfer kind check 1");
                Assert.IsTrue(file1ImportedValues[1].Item3 == TransferKinds.ValueChange, "Step 3 transfer kind check 2");
                //Assert.IsTrue(file2ImportedValues[0].Item3 == TransferKinds.Reclassification, "Step 3 transfer kind check 3");
                Assert.IsTrue(file2ImportedValues[0].Item3 == TransferKinds.ValueChange, "Step 3 transfer kind check 3");
                //Assert.IsTrue(file2ImportedValues[1].Item3 == TransferKinds.ValueChange, "Step 3 transfer kind check 4");

                // check file 1
                List<Tuple<Taxonomy.IElement, decimal?, decimal?>> valueList1 = file1ImportedValues[0].Item4;
                List<Tuple<Taxonomy.IElement, decimal?, decimal?>> valueList2 = file1ImportedValues[1].Item4;
                Assert.IsTrue(valueList1.Count == 1, "Step 3 value list 1/1");
                Assert.IsTrue(valueList2.Count == 1, "Step 3 value list 1/2");
                Assert.IsTrue(valueList1[0].Item2 == (decimal)500.6, "Step 3 check 1");
                Assert.IsTrue(valueList2[0].Item2 == 1500, "Step 3 check 2");

                // check previous year values
                Assert.IsTrue(valueList1[0].Item3 == 100, "Step 3 check 3");
                Assert.IsTrue(valueList2[0].Item3 == null, "Step 3 check 4");
                                    
                // check file 2
                valueList1 = file2ImportedValues[0].Item4;
                //valueList2 = file2ImportedValues[1].Item4;
                Assert.IsTrue(valueList1.Count == 1, "Step 3 value list 2/1");
                //Assert.IsTrue(valueList2.Count == 1, "Step 3 value list 2/2");
                //Assert.IsTrue(valueList1[0].Item2 == 0, "Step 3 check 3");
                //Assert.IsTrue(valueList2[0].Item2 == 2734, "Step 3 check 4");
                Assert.IsTrue(valueList1[0].Item2 == 2734, "Step 3 check 4");

                // check previous year values
                Assert.IsTrue(valueList1[0].Item3 == null, "Step 3 check 5");

                Assert.IsTrue(importModel.HasErrors.HasValue && importModel.HasErrors.Value, "Step 3 HasErrors");

                #endregion [ Step 3 ]

                #region [ Step 4, do the import and check the imported values ]

                // CanImport is false because errors found, but these can be ignored
                Assert.IsFalse(importModel.CanImport, "Step 4 CanImport");

                importModel.Importer.CreateReconciliations();
                Assert.IsTrue(importModel.ReconciliationsModel.ImportedValues.Count() == 3, "Step 4, Imported transaction count mismatch");
                IImportedValues importedTransactions1 = importModel.ReconciliationsModel.ImportedValues.ElementAt(0);
                IImportedValues importedTransactions2 = importModel.ReconciliationsModel.ImportedValues.ElementAt(1);
                IImportedValues importedTransactions3 = importModel.ReconciliationsModel.ImportedValues.ElementAt(2);
                Assert.IsTrue(importedTransactions1.Transactions.Count() == 1, "Step 4 check 1");
                Assert.IsTrue(importedTransactions2.Transactions.Count() == 1, "Step 4 check 2");
                Assert.IsTrue(importedTransactions3.Transactions.Count() == 1, "Step 4 check 3");

                Assert.IsTrue(importedTransactions1.Transactions.ElementAt(0).Value == 2734, "Step 4 check 4");
                Assert.IsTrue(importedTransactions2.Transactions.ElementAt(0).Value == (decimal)500.6, "Step 4 check 5");
                Assert.IsTrue(importedTransactions3.Transactions.ElementAt(0).Value == 1500, "Step 4 check 6");
                // check transfer kind
                Assert.IsTrue(importedTransactions1.TransferKind == TransferKinds.ValueChange, "Step 4 transfer kind check 1");
                Assert.IsTrue(importedTransactions2.TransferKind == TransferKinds.ValueChange, "Step 4 transfer kind check 2");
                Assert.IsTrue(importedTransactions3.TransferKind == TransferKinds.ValueChange, "Step 4 transfer kind check 3");

                // check previous year values
                Assert.IsTrue(DocumentManager.Instance.CurrentDocument.ReconciliationManager.PreviousYearValues.Transactions.Count() == 1, "Step 4 previous values check 1");
                IReconciliationTransaction tran = DocumentManager.Instance.CurrentDocument.ReconciliationManager.PreviousYearValues.Transactions.ElementAt(0);
                Assert.IsTrue(tran.Value == 100, "Step 4 previous values check 2");

                #endregion [ Step 4, do the import and check the imported values ]

            } catch (Exception) {
                throw;
            }
            #region [ Cleanup ]
            finally {
                try {
                    if (UserManager.Instance.CurrentUser.UserName != adminUser.UserName)
                        TestHelper.LogonAsUser(adminUser);
                } catch {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }
                try {
                    if (document != null)
                        TestHelper.DeleteDocument(document);
                } catch {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }
                try {
                    if (company != null)
                        TestHelper.DeleteCompany(company);
                } catch (Exception) {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }
                try {
                    if (system != null)
                        TestHelper.DeleteSystem(system);
                } catch {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }
            }
            #endregion [ Cleanup ]   
        }

        /// <summary>
        ///A test for reconciliation account import
        ///</summary>
        [TestMethod()]
        public void ReconciliationImport_AccountsTest() {

            string file1Path = System.IO.Path.Combine(path, "AccountValueImportExample.csv");
            string file2PathError = System.IO.Path.Combine(path, "AccountValueImportExample_error.csv");
            string file3PathInvalid = System.IO.Path.Combine(path, "AccountValueImportExample_invalid.csv");

            Structures.DbMapping.System system = null;
            User adminUser = TestHelper.LogonAsAdmin();
            Company company = null;
            Document document = null;

            try {

                #region [ Prepare ]

                system = TestHelper.CreateSystem();
                company = TestHelper.CreateCompany();
                document = TestHelper.CreateDocument();

                #endregion [ Prepare ]

                #region [ Init document ]
                
                TestHelper.InitDocument(document);

                #endregion [ Init document ]

                ObjectWrapper<Document> documentWrapper = new ObjectWrapper<Document>();
                documentWrapper.Value = document;
                ReconciliationsModel reconciliationsModel = new ReconciliationsModel(documentWrapper.Value);
                ReconciliationImportModel importModel = new ReconciliationImportModel(reconciliationsModel);

                #region [ Step 1 add files ]

                importModel.CsvFiles.Add(file1Path);
                importModel.CsvFiles.Add(file2PathError);
                importModel.CsvFiles.Add(file3PathInvalid);
                if (!importModel.HasFiles || !importModel.IsStepErrorsEnabled || !importModel.IsStepPreviewEnabled)
                    Assert.Fail("Step 1");
                
                #endregion [ Step 1 add files ]

                #region [ Step 2 ]

                int currentStep = 2;
                if (importModel.ValidateAndSetDialogPage(currentStep)) {
                    importModel.CreatePreviewData();
                } else {
                    Assert.Fail("Go to Step 2");
                }

                if (importModel.Preview == null)
                    Assert.Fail("Step 2: Preview is null");
                if (importModel.PreviewData != null || importModel.Importer != null)
                    Assert.Fail("Step 2: PreviewData or Importer failed");
                if (!importModel.HasFiles || !importModel.IsStepErrorsEnabled || !importModel.IsStepPreviewEnabled)
                    Assert.Fail("Step 2: property check");

                PrivateObject importPreview = new PrivateObject(importModel.Preview);
                Type[] paramTypes = new Type[] { };
                Object[] paramObjects = new Object[] { };
                object retValInvoke = importPreview.Invoke("UpdatePreview", paramTypes, paramObjects);
                if (importModel.PreviewData == null || importModel.PreviewData.TableName != file1Path)
                    Assert.Fail("PreviewData failed");

                if (importModel.Preview.IsPreviousAllowed)
                    Assert.Fail("Step 2: property IsPreviousAllowed");
                if (!importModel.Preview.IsNextAllowed)
                    Assert.Fail("Step 2: property IsNextAllowed");
                importModel.Preview.Next();
                importModel.Preview.Next();
                retValInvoke = importPreview.Invoke("UpdatePreview", paramTypes, paramObjects);
                if (importModel.PreviewData == null || importModel.PreviewData.TableName != file3PathInvalid)
                    Assert.Fail("PreviewData failed 2");
                if (!importModel.Preview.IsPreviousAllowed)
                    Assert.Fail("Step 2: property IsPreviousAllowed 2");
                if (importModel.Preview.IsNextAllowed)
                    Assert.Fail("Step 2: property IsNextAllowed 2");

                #endregion [ Step 2 ]

                #region [ Step 3 ]

                currentStep = 3;
                if (importModel.ValidateAndSetDialogPage(currentStep)) {
                    PrivateObject previewImport = new PrivateObject(importModel);
                    Object[] paramObjectsPreviewImport = new Object[] { };
                    object retValInvokePreviewImport = previewImport.Invoke("PreviewImport", paramObjectsPreviewImport);
                } else {
                    Assert.Fail("Go to Step 3");
                }

                if (importModel.Importer == null)
                    Assert.Fail("Step 3: Importer failed");

                PrivateObject importer = new PrivateObject(importModel.Importer);
                Dictionary<string, List<Tuple<string, string, TransferKinds, List<Tuple<string, decimal?>>>>> retValInvokeImporter = importer.GetFieldOrProperty("ImportedAccounts") as Dictionary<string, List<Tuple<string, string, TransferKinds, List<Tuple<string, decimal?>>>>>;

                Assert.IsNotNull(retValInvokeImporter, "Step 3 retValInvokeImporter");

                #endregion [ Step 3 ]

            } catch (Exception) {
                throw;
            }
            #region [ Cleanup ]
            finally {
                try {
                    if (UserManager.Instance.CurrentUser.UserName != adminUser.UserName)
                        TestHelper.LogonAsUser(adminUser);
                } catch {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }
                try {
                    if (document != null)
                        TestHelper.DeleteDocument(document);
                } catch {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }
                try {
                    if (company != null)
                        TestHelper.DeleteCompany(company);
                } catch (Exception) {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }
                try {
                    if (system != null)
                        TestHelper.DeleteSystem(system);
                } catch {
                    //DEVNOTE: intentional, we do not care about this error because it is irrelevant for the purpose of this test
                }
            }
            #endregion [ Cleanup ]   
        }
    }
}
