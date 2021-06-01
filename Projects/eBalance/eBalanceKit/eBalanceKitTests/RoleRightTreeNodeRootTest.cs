// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-08
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Microsoft.VisualStudio.TestTools.UnitTesting;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures.DbMapping.Rights;

namespace eBalanceKitTests {
    [TestClass]
    public class RoleRightTreeNodeRootTest {
        #region Init/Cleanup
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext) { }

        [ClassCleanup]
        public static void MyClassCleanup() { }

        [TestInitialize]
        public void MyTestInitialize() { }

        [TestCleanup]
        public void MyTestCleanup() { }
        #endregion

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void IsRightAllowedTest() {
            Role role = new Role(new DbRole());
            var target = new RoleRightTreeNodeRoot(role);
            int i = 0;
            bool expected = false;
            bool actual;
            //actual = target.IsRightAllowed(i);
            //Assert.AreEqual(expected, actual);
            Assert.AreEqual(true, false, "Fehler!!!");
        }
    }
}