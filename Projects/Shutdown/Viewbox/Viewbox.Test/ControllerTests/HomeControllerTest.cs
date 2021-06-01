using System;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Controllers;

namespace Viewbox.Test.ControllerTests
{
    [TestClass]
    public class HomeControllerTest : BaseControllerTest<HomeController>
    {
        [TestMethod]
        public void IndexTest()
        {
            ActionResult result = Controller.Index();
            Assert.IsNotNull(result);
        }

        [TestMethod,
         ExpectedException(typeof (Exception))]
        public void TestTest()
        {
            ActionResult result = Controller.Test();
            Assert.IsNotNull(result);
        }

// GenerateCode
    }
}