using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Controllers;

namespace Viewbox.Test.ControllerTests
{
    [TestClass]
    public class HelpControllerTest : BaseControllerTest<HelpController>
    {
        [TestMethod]
        public void IndexTest()
        {
            ActionResult result = Controller.Index();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TableListTest()
        {
            ActionResult result = Controller.TableList();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void IssueListTest()
        {
            ActionResult result = Controller.IssueList();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ViewListTest()
        {
            ActionResult result = Controller.ViewList();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void DocumentsTest()
        {
            ActionResult result = Controller.Documents();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ControlCenterTest()
        {
            ActionResult result = Controller.ControlCenter();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ExportTest()
        {
            ActionResult result = Controller.Export();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void SettingsTest()
        {
            ActionResult result = Controller.Settings();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void AboutTest()
        {
            ActionResult result = Controller.About();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void FaqTest()
        {
            ActionResult result = Controller.Faq();
            Assert.IsNotNull(result);
        }

// GenerateCode
    }
}