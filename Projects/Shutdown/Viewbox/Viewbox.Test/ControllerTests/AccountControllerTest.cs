using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Controllers;
using Viewbox.Models;

namespace Viewbox.Test.ControllerTests
{
    [TestClass]
    public class AccountControllerTest : BaseControllerTest<AccountController>
    {
        [TestMethod]
        public void get_FormsServiceTest()
        {
            //Controller.get_FormsService();
        }

        [TestMethod]
        public void set_FormsServiceTest()
        {
            //Controller.set_FormsService();
        }

        [TestMethod]
        public void get_MembershipServiceTest()
        {
            //Controller.get_MembershipService();
        }

        [TestMethod]
        public void set_MembershipServiceTest()
        {
            //Controller.set_MembershipService();
        }

        [TestMethod]
        public void LogOnTest()
        {
            ActionResult result = Controller.LogOff("");
            Assert.IsNotNull(result);

            ViewResult view = Controller.LogOn() as ViewResult;
            Assert.IsNotNull(view);
            LogOnModel model = view.Model as LogOnModel;
            Assert.IsNotNull(model);
            model.UserName = "1";
            model.Password = "2";
            model.RememberMe = true;
            result = Controller.LogOn(model, "");
            Assert.IsTrue(result is ViewResult);
            view = Controller.LogOn() as ViewResult;
            Assert.IsNotNull(view);
            model = view.Model as LogOnModel;
            Assert.IsNotNull(model);
            Controller.ModelState.Clear();
            model.UserName = "avendata_admin";
            model.Password = "avendata_admin";
            model.RememberMe = true;
            result = Controller.LogOn(model, "");
            Assert.IsTrue(result is RedirectResult || result is RedirectToRouteResult);
        }

        [TestMethod]
        public void LogOffTest()
        {
            // In LogOnTest
        }

        [TestMethod]
        public void PassTest()
        {
            // in SendInvitationTest
        }

        [TestMethod]
        public void ForgotPasswordTest()
        {
            Controller.ForgotPassword("avendata_admin");
        }

        [TestMethod]
        public void SendInvitationTest()
        {
            Controller.SendInvitation("avendata_admin");
            string key = ViewboxApplication.GetFirstAccessKey();
            Assert.IsNotNull(key);
            Controller.Pass(key);
        }

        [TestMethod]
        public void BeforeInitializeTest()
        {
            //Controller.BeforeInitialize();
        }

// GenerateCode
    }
}