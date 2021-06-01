using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Controllers;

namespace Viewbox.Test.ControllerTests
{
    [TestClass]
    public class ControlCenterControllerTest : BaseControllerTest<ControlCenterController>
    {
        [TestMethod]
        public void IndexTest()
        {
            ActionResult result = Controller.Index();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void OtherUserTest()
        {
            //Controller.OtherUser();
        }

        [TestMethod]
        public void RemoveTest()
        {
            ActionResult result = Controller.Remove(0);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void HideTest()
        {
            //Controller.Hide();
        }

        [TestMethod]
        public void RunningJobsTest()
        {
            ActionResult result = Controller.RunningJobs();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void RunningJTest()
        {
            ActionResult result = Controller.RunningJ();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void RefreshJobsTest()
        {
            ActionResult result = Controller.RefreshJobs("");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CancelJobTest()
        {
            ActionResult result = Controller.CancelJob("");
            Assert.IsNotNull(result);
            Assert.IsTrue(result is RedirectToRouteResult);
            result = Controller.CancelJob("", true);
            Assert.IsNotNull(result);
            Assert.IsTrue(result is JsonResult);
        }

        [TestMethod]
        public void CancelOtherUserJobTest()
        {
            ActionResult result = Controller.CancelOtherUserJob("");
            Assert.IsNotNull(result);
            Assert.IsTrue(result is RedirectToRouteResult);
            result = Controller.CancelOtherUserJob("", true);
            Assert.IsNotNull(result);
            Assert.IsTrue(result is JsonResult);
        }

        [TestMethod]
        public void ResultTest()
        {
            //Controller.Result();
        }

        [TestMethod]
        public void DeleteTransformationTest()
        {
            //Controller.DeleteTransformation();
        }

// GenerateCode
    }
}