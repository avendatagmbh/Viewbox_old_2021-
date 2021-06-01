using System.Linq;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Controllers;
using ViewboxDb;

namespace Viewbox.Test.ControllerTests
{
    [TestClass]
    public class HeaderControllerTest : BaseControllerTest<HeaderController>
    {
        [TestMethod]
        public void ShowEditOptimizationTextDialogTest()
        {
            var opt = ViewboxApplication.Database.SystemDb.Optimizations.FirstOrDefault();
            if (opt != null)
            {
                ActionResult result = Controller.ShowEditOptimizationTextDialog(opt.Id);
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        public void ChangedOptimizationTextsTest()
        {
            var opt = ViewboxApplication.Database.SystemDb.Optimizations.FirstOrDefault();
            if (opt != null)
            {
                ActionResult result = Controller.ChangedOptimizationTexts(opt.Id, new DescriptionCollection());
                Assert.IsNotNull(result);
            }
        }

// GenerateCode
    }
}