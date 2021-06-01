using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Controllers;

namespace Viewbox.Test.ControllerTests
{
    [TestClass]
    public class IssueListControllerTest : BaseControllerTest<IssueListController>
    {
        [TestMethod]
        public void IndexTest()
        {
            ActionResult result = Controller.Index();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void IndexListTest()
        {
            ActionResult result = Controller.IndexList();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ShowOneTest()
        {
            ActionResult result = Controller.ShowOne(Context.TestTable.Id);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CategoryTabsTest()
        {
            ActionResult result = Controller.CategoryTabs(Context.TestTable.CategoryId);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void SortTest()
        {
            ActionResult result = Controller.Sort(Context.TestTable.Id);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void DeleteIssueTest()
        {
            //Controller.DeleteIssue();
        }

        [TestMethod]
        public void HideTableObjectTest()
        {
            ActionResult result = Controller.HideTableObject(Context.TestTable.Id);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ChangeTableOrderTest()
        {
            Controller.ChangeTableOrder(Context.TestTable.Id, 2);
        }

        [TestMethod]
        public void SaveFavoriteTest()
        {
            Controller.SaveFavorite(Context.TestTable.Id);
            Controller.DeleteFavorite(Context.TestTable.Id);
        }

        /*
		[TestMethod]
		public void DeleteFavoriteTest() {
            // In SaveFavoriteTest
		}
        */

        [TestMethod]
        public void GetConfirmationDialogTest()
        {
            ActionResult result = Controller.GetConfirmationDialog();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetValidationDialogTest()
        {
            ActionResult result = Controller.GetValidationDialog();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void DeleteConfirmationDialogTest()
        {
            ActionResult result = Controller.DeleteConfirmationDialog();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void EditIssueDialogTest()
        {
            ActionResult result = Controller.EditIssueDialog(Context.TestTable.Id);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void EditIssueTest()
        {
            //Controller.EditIssue();
        }

// GenerateCode
    }
}