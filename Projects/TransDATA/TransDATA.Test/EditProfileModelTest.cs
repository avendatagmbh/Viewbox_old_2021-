using TransDATA.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Config.Interfaces.DbStructure;
using System.Windows;

namespace TransDATA.Test
{
    
    
    /// <summary>
    ///This is a test class for EditProfileModelTest and is intended
    ///to contain all EditProfileModelTest Unit Tests
    ///</summary>
    [TestClass()]
    public class EditProfileModelTest
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
        ///A test for Profile
        ///</summary>
        [TestMethod()]
        public void ProfileTest()
        {
            IProfile profile = null; // TODO: Initialize to an appropriate value
            Window owner = null; // TODO: Initialize to an appropriate value
            EditProfileModel target = new EditProfileModel(profile, owner); // TODO: Initialize to an appropriate value
            IProfile expected = null; // TODO: Initialize to an appropriate value
            IProfile actual;
            target.Profile = expected;
            actual = target.Profile;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
