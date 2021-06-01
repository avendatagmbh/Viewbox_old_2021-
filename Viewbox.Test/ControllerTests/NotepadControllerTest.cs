using System.Linq;
using System.Web.Mvc;
using SystemDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Controllers;

namespace Viewbox.Test.ControllerTests
{
    [TestClass]
    public class NotepadControllerTest : BaseControllerTest<NotepadController>
    {
        [TestMethod]
        public void GetNoteTextTest()
        {
            if (ViewboxApplication.Notes[ViewboxSession.User] != null)
            {
                INote note = ViewboxApplication.Notes[ViewboxSession.User].FirstOrDefault();
                if (note != null)
                {
                    ActionResult result = Controller.GetNoteText(note.Id);
                    Assert.IsNotNull(result);
                }
            }
        }

        [TestMethod]
        public void GetNoteTitleTest()
        {
            if (ViewboxApplication.Notes[ViewboxSession.User] != null)
            {
                INote note = ViewboxApplication.Notes[ViewboxSession.User].FirstOrDefault();
                if (note != null)
                {
                    ActionResult result = Controller.GetNoteTitle(note.Id);
                    Assert.IsNotNull(result);
                }
            }
        }

        [TestMethod]
        public void SaveNoteTest()
        {
            if (ViewboxApplication.Notes[ViewboxSession.User] != null)
            {
                INote note = ViewboxApplication.Notes[ViewboxSession.User].FirstOrDefault();
                if (note != null)
                {
                    ActionResult result = Controller.SaveNote(note.Id, note.Text, note.Title);
                    Assert.IsNotNull(result);
                }
            }
        }

        [TestMethod]
        public void DeleteNoteTest()
        {
            //Controller.DeleteNote();
        }

// GenerateCode
    }
}