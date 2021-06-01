using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Controllers;

namespace Viewbox.Test.ControllerTests
{
    [TestClass]
    public class DocumentsControllerTest : BaseControllerTest<DocumentsController>
    {
        [TestMethod]
        public void IndexTest()
        {
            ActionResult result = Controller.Index();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void DownloadTest()
        {
            ActionResult result = Controller.Download("test", "test");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void DownloadNewTest()
        {
            ActionResult result = Controller.DownloadNew("test");
            Assert.IsNull(result);
        }

        [TestMethod]
        public void CallWCFMethodTest()
        {
            // In DownloadNewTest
        }

        [TestMethod]
        public void FilterTest()
        {
            ActionResult result = Controller.Filter(Context.TestTable.Id, Context.TestColumn.Id, "", false);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void SortTest()
        {
        }

        [TestMethod]
        public void ThumbnailTest()
        {
            ActionResult result = Controller.Thumbnail("test");
            Assert.IsNull(result);
        }

// GenerateCode
    }
}