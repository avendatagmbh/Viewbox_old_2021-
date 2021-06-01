using System.Diagnostics;
using System.Threading;
using System.Xml;
using DbAccess;
using ViewBuilderBusiness.Manager;
using ViewBuilderBusiness.SapBillSchemaImport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using ViewBuilderBusiness.Structures.Config;
using Utils;

namespace ViewBuilder.Test
{
    
    
    /// <summary>
    ///This is a test class for SapBillSchemaImportTest and is intended
    ///to contain all SapBillSchemaImportTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SapBillSchemaImportTest
    {


        #region Properties

        private ProfileConfig _config;

        #endregion

        #region Init, Cleanup

        [TestInitialize()]
        public void MyTestInitialize()
        {
            _config = ConfigLoadHelper.ConfigLoad();
        }


        [TestCleanup()]
        public void MyTestCleanup()
        {
            _config.Dispose();
        }

        #endregion

        #region TestMethods

        /// <summary>
        ///A test for DoWork
        ///</summary>
        [TestMethod()]
        public void DoWorkTest()
        {
            IList<SapBillSchemaImportFile> files = new List<SapBillSchemaImportFile>();
            files.Add(new SapBillSchemaImportFile() {AccountsStructure = "SCDE", FilePath = @"Resources\HGB.txt"});
            //files.Add(new SapBillSchemaImportFile() { AccountsStructure = "BAY", FilePath = @"Resources\ZAB1.txt" });
            ProfileConfig profile = _config;
            SapBillSchemaImport target = new SapBillSchemaImport(files, profile, null);
            try {
                target.DoWork2();
            }
            catch (Exception ex) {
                Assert.Fail("Exception:" +ex.Message);
                throw;
            }
            
            //Assert.Inconclusive("A method that does not return a value cannot be verified.");
            using (IDatabase conn = profile.ConnectionManager.GetConnection()) {
                int c= Convert.ToInt32(conn.ExecuteScalar("SELECT count(*) from _bgv_Bilanzstruktur WHERE bilstr like 'HGB' "));
                
                Assert.IsTrue(c>0,"Insertion failed");
            }
        }

        /// <summary>
        ///A test for DoWork2
        ///</summary>
        [TestMethod()]
        public void DoWork2Test()
        {
            IList<SapBillSchemaImportFile> files = new List<SapBillSchemaImportFile>();
            //files.Add(new SapBillSchemaImportFile() { AccountsStructure = "SCDE", FilePath = @"Resources\HGB.txt" });
            files.Add(new SapBillSchemaImportFile() { AccountsStructure = "BAY", FilePath = @"Resources\ZAB1.txt" });
            ProfileConfig profile = _config;
            SapBillSchemaImport target = new SapBillSchemaImport(files, profile, null);
            try
            {
                target.DoWork2();
            }
            catch (Exception ex)
            {
                Assert.Fail("Exception:" + ex.Message);
                throw;
            }

            //Assert.Inconclusive("A method that does not return a value cannot be verified.");
            using (IDatabase conn = profile.ConnectionManager.GetConnection())
            {
                int c = Convert.ToInt32(conn.ExecuteScalar("SELECT count(*) from _bgv_Bilanzstruktur WHERE bilstr like 'ZAB1' "));

                Assert.IsTrue(c > 0, "Insertion failed");
            }
        }
        #endregion


        /// <summary>
        ///A test for FunctionEnabled
        ///</summary>
        [TestMethod()]
        public void FunctionEnabledTest()
        {

            ProfileConfig profile = _config;
            bool expected;
            bool actual;

            using (IDatabase conn = profile.ViewboxDb.ConnectionManager.GetConnection())
            {
                int c = Convert.ToInt32(conn.ExecuteScalar("SELECT count(*) FROM " + conn.Enquote("issue_extensions") + " WHERE " + conn.Enquote("command") + " like 'sap_balance'"));
                expected =c > 0;
            }
            actual = SapBillSchemaImport.FunctionEnabled(profile);

            Assert.AreEqual(expected, actual);
        }
    }
}
