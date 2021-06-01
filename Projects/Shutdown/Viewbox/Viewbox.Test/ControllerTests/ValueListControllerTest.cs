using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Controllers;

namespace Viewbox.Test.ControllerTests
{
    [TestClass]
    public class ValueListControllerTest : BaseControllerTest<ValueListController>
    {
        [TestMethod]
        public void IndexTest()
        {
            if (Context.TestParameter != null)
            {
                ActionResult result = Controller.Index(Context.TestParameter.Id, 0);
                Assert.IsNotNull(result);
            }
        }

// GenerateCode
    }
}