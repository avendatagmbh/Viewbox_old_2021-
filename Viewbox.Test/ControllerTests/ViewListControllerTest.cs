using System.Web.Mvc;
using SystemDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Controllers;
using ViewboxDb;

namespace Viewbox.Test.ControllerTests
{
    [TestClass]
    public class ViewListControllerTest : BaseControllerTest<ViewListController>
    {
        [TestMethod]
        public void IndexTest()
        {
            ActionResult result = Controller.Index();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CategoryTabsTest()
        {
            ActionResult result = Controller.CategoryTabs();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void SortTest()
        {
            ActionResult result = Controller.Sort();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void DeleteViewTest()
        {
            //Controller.DeleteView();
        }

        [TestMethod]
        public void ArchiveTest()
        {
            ActionResult result = Controller.Archive(Context.TestTable.Id, ArchiveType.Archive);
            Assert.IsNotNull(result);
            Controller.Archive(Context.TestTable.Id, ArchiveType.Restore);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ArchiveSelectedTest()
        {
            ActionResult result = Controller.ArchiveSelected(TableType.Table, "Archive");
            Assert.IsNotNull(result);
            result = Controller.ArchiveSelected(TableType.Table, "Restore");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void SelectTest()
        {
            Controller.Select();
        }

        [TestMethod]
        public void SelectNoneTest()
        {
            Controller.SelectNone();
        }

        [TestMethod]
        public void GetTableObjectsTest()
        {
            ActionResult result = Controller.GetTableObjects(null, "Table");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetConfirmationDialogTest()
        {
            ActionResult result = Controller.GetConfirmationDialog("Archive");
            Assert.IsNotNull(result);
            result = Controller.GetConfirmationDialog("Restore");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ChangeTableOrderTest()
        {
            Controller.ChangeTableOrder(Context.TestTable.Id, Context.TestTable.Ordinal);
        }

        [TestMethod]
        public void DeleteConfirmationDialogTest()
        {
            ActionResult result = Controller.DeleteConfirmationDialog();
            Assert.IsNotNull(result);
        }

// GenerateCode
    }
}