using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Controllers;

namespace Viewbox.Test.ControllerTests
{
    [TestClass]
    public class ErrorControllerTest : BaseControllerTest<ErrorController>
    {
        [TestMethod]
        public void IndexTest()
        {
            ActionResult result = Controller.Index();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void NotReadyTest()
        {
            ActionResult result = Controller.NotReady();
            Assert.IsNotNull(result);
            result = Controller.NotReady(true);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void WrongBrowserTest()
        {
            ActionResult result = Controller.WrongBrowser();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void DatabaseOutOfDateTest()
        {
            ActionResult result = Controller.DatabaseOutOfDate();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void UpgradeDatabaseTest()
        {
            ActionResult result = Controller.UpgradeDatabase();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void UpgradeDatabaseSuccessfulTest()
        {
            ActionResult result = Controller.UpgradeDatabaseSuccessful();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void DeadUrlTest()
        {
            ActionResult result = Controller.DeadUrl();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CheckBrowserCompatibilityTest()
        {
            ActionResult result = Controller.CheckBrowserCompatibility();
            Assert.IsNotNull(result);
        }

// GenerateCode
    }
}