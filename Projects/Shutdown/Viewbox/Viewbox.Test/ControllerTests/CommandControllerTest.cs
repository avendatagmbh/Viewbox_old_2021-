using System.Linq;
using System.Web.Mvc;
using SystemDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Controllers;
using ViewboxDb;

namespace Viewbox.Test.ControllerTests
{
    [TestClass]
    public class CommandControllerTest : BaseControllerTest<CommandController>
    {
        [TestMethod]
        public void GetUpdatesTest()
        {
            ActionResult result = Controller.GetUpdates();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void RemoveJobTest()
        {
            //Controller.RemoveJob();
        }

        [TestMethod]
        public void IndexTest()
        {
            ActionResult result = Controller.Index();
            Assert.IsNotNull(result);
            Assert.IsTrue(result is ContentResult);
        }

        [TestMethod]
        public void SearchOptimizationTest()
        {
            ActionResult result = Controller.SearchOptimization("", 0, "");
            Assert.IsNotNull(result);
            ViewboxSession.Init();
        }

        [TestMethod]
        public void RefreshBoxHeaderTest()
        {
            ActionResult result = Controller.RefreshBoxHeader("");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void RefreshTableObjectsTest()
        {
            ActionResult result = Controller.RefreshTableObjects(null, true, 0, SortDirection.Ascending, false, 0, 25,
                                                                 TableType.Issue, true);
            Assert.IsNotNull(result);
            result = Controller.RefreshTableObjects(null, true, 0, SortDirection.Ascending, true, 0, 25, TableType.Issue,
                                                    true);
            Assert.IsNotNull(result);
            result = Controller.RefreshTableObjects(null, true, 0, SortDirection.Ascending, false, 0, 25,
                                                    TableType.Table, true);
            Assert.IsNotNull(result);
            result = Controller.RefreshTableObjects(null, true, 0, SortDirection.Ascending, true, 0, 25, TableType.Table,
                                                    true);
            Assert.IsNotNull(result);
            result = Controller.RefreshTableObjects(null, true, 0, SortDirection.Ascending, false, 0, 25, TableType.View,
                                                    true);
            Assert.IsNotNull(result);
            result = Controller.RefreshTableObjects(null, true, 0, SortDirection.Ascending, true, 0, 25, TableType.View,
                                                    true);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void RefreshViewOptionsTest()
        {
            ActionResult result = Controller.RefreshViewOptions(ViewboxSession.TableObjects.FirstOrDefault().Id);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void RefreshExtendedSortTest()
        {
            ActionResult result = Controller.RefreshExtendedSort(ViewboxSession.TableObjects.FirstOrDefault().Id);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void RefreshExtendedFilterTest()
        {
            ActionResult result = Controller.RefreshExtendedFilter(ViewboxSession.TableObjects.FirstOrDefault().Id);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetNoteTextTest()
        {
            if (ViewboxApplication.Notes[ViewboxSession.User] != null)
            {
                ActionResult result =
                    Controller.GetNoteText(ViewboxApplication.Notes[ViewboxSession.User].FirstOrDefault().Id);
                Assert.IsNotNull(result);
                Assert.IsTrue(result is ContentResult);
            }
        }

        [TestMethod]
        public void GetNoteTitleTest()
        {
            if (ViewboxApplication.Notes[ViewboxSession.User] != null)
            {
                ActionResult result =
                    Controller.GetNoteTitle(ViewboxApplication.Notes[ViewboxSession.User].FirstOrDefault().Id);
                Assert.IsNotNull(result);
                Assert.IsTrue(result is ContentResult);
            }
        }

        [TestMethod]
        public void SaveNoteTest()
        {
            if (ViewboxApplication.Notes[ViewboxSession.User] != null)
            {
                var note = ViewboxApplication.Notes[ViewboxSession.User].FirstOrDefault();
                if (note == null)
                    return;
                int oldId = note.Id;
                string oldText = note.Text;
                string oldTitle = note.Title;
                ActionResult result =
                    Controller.SaveNote(note.Id, note.Text, note.Title);
                Assert.IsNotNull(result);
                Assert.IsTrue(result is ContentResult);
                var changedNote = ViewboxApplication.Notes[ViewboxSession.User].FirstOrDefault(w => w.Id == oldId);
                Assert.IsNotNull(changedNote);
                Assert.AreEqual(changedNote.Text, oldText);
                Assert.AreEqual(changedNote.Title, oldTitle);
            }
        }

        [TestMethod]
        public void UpdateTableTest()
        {
            //Controller.UpdateTable();
        }

// GenerateCode
    }
}