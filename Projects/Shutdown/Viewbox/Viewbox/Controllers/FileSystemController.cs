using System.Linq;
using System.Web.Mvc;
using SystemDb;
using SystemDb.Internal;
using Viewbox.Models;

namespace Viewbox.Controllers
{
	[Authorize]
	[ViewboxFilter]
	public class FileSystemController : TableObjectControllerBase
	{
		public ActionResult Index(string fileFilter = null)
		{
			FileSystemModel model = new FileSystemModel();
			if (!ViewboxApplication.IsFilesSystemInitialized)
			{
				if (ViewboxApplication.FileSystems.Any((IFileSys x) => x.Database == ViewboxSession.SelectedSystem))
				{
					if (!ViewboxApplication.LoadInFileSystem(ViewboxSession.SelectedSystem))
					{
						return View(new EmptyResult());
					}
					model.Directories = new DirectoryObjectCollection { ViewboxApplication.DirectoryObjects.First() };
				}
			}
			else
			{
				if (!ViewboxApplication.IsFilesSystemInitialized)
				{
					return View(new EmptyResult());
				}
				model.Directories = new DirectoryObjectCollection { ViewboxApplication.DirectoryObjects.First() };
			}
			return View(model);
		}

		[HttpGet]
		public ActionResult ShowFiles(int id = -1)
		{
			FileSystemModel model = new FileSystemModel();
			if (id != -1)
			{
				model.Files = ViewboxApplication.FilterForFiles(id);
			}
			return PartialView("_FilesPartial", model);
		}

		[HttpGet]
		public ActionResult FilterDirectories(string filter = null)
		{
			FileSystemModel model = new FileSystemModel();
			if (filter != null && filter != string.Empty)
			{
				model.Directories = ViewboxApplication.FilterForDirectories(filter);
			}
			else
			{
				model.Directories = new DirectoryObjectCollection { ViewboxApplication.DirectoryObjects.First() };
			}
			return PartialView("_DirectoriesPartial", model);
		}

		[HttpGet]
		public ActionResult FilterFiles(string filter = null)
		{
			FileSystemModel model = new FileSystemModel();
			if (filter != null && filter != string.Empty)
			{
				model.Files = ViewboxApplication.FilterForFiles(filter);
			}
			else
			{
				model.Files = new FileObjectCollection();
			}
			return PartialView("_FilesPartial", model);
		}

		[HttpGet]
		public ActionResult FilterAll(string filter = null)
		{
			FileSystemModel model = new FileSystemModel();
			if (filter != null && filter != string.Empty)
			{
				model.Directories = ViewboxApplication.FilterForDirectories(filter);
				model.Files = ViewboxApplication.FilterForFiles(filter);
			}
			else
			{
				model.Directories = new DirectoryObjectCollection { ViewboxApplication.DirectoryObjects.First() };
				model.Files = new FileObjectCollection();
			}
			return PartialView("_FileSystemPartial", model);
		}
	}
}
