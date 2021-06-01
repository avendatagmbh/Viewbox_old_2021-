using ExcelToCsvConverter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace ExcelToCsvTest
{
    
    
    /// <summary>
    ///This is a test class for ProgramTest and is intended
    ///to contain all ProgramTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ProgramTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
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
        ///A test for GetUniqueName
        ///</summary>
        [TestMethod()]
        public void GetUniqueNameTest() {
            string newName = "maki";
            List<string> oldNames = new List<string> {"maki"};
            string expected = "maki2";
            string actual = Program.GetUniqueName(newName, oldNames);
            Assert.AreEqual(expected, actual);
            oldNames = new List<string> {"maki", "maki2"};
            expected = "maki3";
            actual = Program.GetUniqueName(newName, oldNames);
            Assert.AreEqual(expected, actual);
            oldNames = new List<string>();
            expected = "maki";
            actual = Program.GetUniqueName(newName, oldNames);
            Assert.AreEqual(expected, actual);
        }
    }
}
