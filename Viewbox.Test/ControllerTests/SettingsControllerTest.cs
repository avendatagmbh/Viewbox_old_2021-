using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Controllers;
using Viewbox.Models;

namespace Viewbox.Test.ControllerTests
{
    [TestClass]
    public class SettingsControllerTest : BaseControllerTest<SettingsController>
    {
        [TestMethod]
        public void PersonalTest()
        {
            ViewResult result = Controller.Personal() as ViewResult;
            Assert.IsNotNull(result);
            UserSettingsModel model = result.Model as UserSettingsModel;
            Assert.IsNotNull(model);
            ActionResult result2 = Controller.Personal(model.User.UserName, model.User.Name, model.User.Email, "",
                                                       model.User.DisplayRowCount, false, false);
            Assert.IsNotNull(result2);
            result2 = Controller.CheckPassword(model.User.UserName, model.User.Name, model.User.Email, "");
            Assert.IsNotNull(result2);
        }

        [TestMethod]
        public void CheckPasswordTest()
        {
            // In PersonalTest
        }

        [TestMethod]
        public void RightsTest()
        {
            ActionResult result = Controller.Rights();
            Assert.IsNotNull(result);
            result = Controller.Rights(true);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void AdminTasksTest()
        {
            ActionResult result = Controller.AdminTasks();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void PopulateIndexesTest()
        {
            /*ActionResult result = Controller.PopulateIndexes();
            Assert.IsNotNull(result);*/
        }

        [TestMethod]
        public void GenerateEmptyDistinctTableTest()
        {
            //Controller.GenerateEmptyDistinctTable();
        }

        [TestMethod]
        public void InvalidValueTest()
        {
            ActionResult result = Controller.InvalidValue();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void AlreadyExistingTest()
        {
            ActionResult result = Controller.AlreadyExisting();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void IndexTest()
        {
            ActionResult result = Controller.Index();
            Assert.IsNotNull(result);
        }

// GenerateCode
    }
}