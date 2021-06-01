using System.Linq;
using System.Web.Mvc;
using SystemDb;

namespace Viewbox.Controllers
{
	public class NotepadController : BaseController
	{
		public ActionResult GetNoteText(int id)
		{
			if (ViewboxApplication.Notes[ViewboxSession.User] != null)
			{
				INote note = ViewboxApplication.Notes[ViewboxSession.User].FirstOrDefault((INote n) => n.Id == id);
				return Content((note != null) ? note.Text : "", "text/plain");
			}
			return Content("", "text/plain");
		}

		public ActionResult GetNoteTitle(int id)
		{
			if (ViewboxApplication.Notes[ViewboxSession.User] != null)
			{
				INote note = ViewboxApplication.Notes[ViewboxSession.User].FirstOrDefault((INote n) => n.Id == id);
				return Content((note != null) ? note.Title : "", "text/plain");
			}
			return Content("", "text/plain");
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult SaveNote(int id, string text, string title)
		{
			INote note = ViewboxApplication.SaveNote(id, text, title);
			return Content(note.Id.ToString(), "text/plain");
		}

		public void DeleteNote(int id)
		{
			ViewboxApplication.Database.DeleteNote(id, ViewboxSession.User);
		}
	}
}
