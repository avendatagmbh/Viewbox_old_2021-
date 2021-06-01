using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web.Mvc;
using SystemDb;
using Viewbox.Job;
using Viewbox.Models;
using ViewboxDb;

namespace Viewbox.Controllers
{
	[Authorize]
	[ViewboxFilter]
	public class ControlCenterController : BaseController
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private class MEMORYSTATUSEX
		{
			public uint dwLength;

			public ulong ullTotalPhys;

			public ulong ullAvailPhys;

			public MEMORYSTATUSEX()
			{
				dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
			}
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		private static extern int GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GlobalMemoryStatusEx([In][Out] MEMORYSTATUSEX lpBuffer);

		public ActionResult Index()
		{
			ControlCenterModel model = new ControlCenterModel();
			MEMORYSTATUSEX msex = new MEMORYSTATUSEX();
			GlobalMemoryStatusEx(msex);
			model.FreeMem = msex.ullAvailPhys;
			model.AvailableMem = msex.ullTotalPhys;
			model.CpuLoad = ViewboxApplication.ProcessorTime.NextValue();
			model.DiscSpeed = ViewboxApplication.HardDiskTransferSpeed.NextValue();
			model.DiscSpeed /= 10f;
			model.UserList = new List<SelectListItem>();
			foreach (IUser user in ViewboxApplication.Database.SystemDb.Users)
			{
				model.UserList.Add(new SelectListItem
				{
					Value = user.Id.ToString(),
					Text = user.UserName
				});
			}
			return View(model);
		}

		public ActionResult OtherUser(int id = 0, string filterdate = null, bool showHidden = false)
		{
			ControlCenterModel model = new ControlCenterModel();
			MEMORYSTATUSEX msex = new MEMORYSTATUSEX();
			GlobalMemoryStatusEx(msex);
			model.FreeMem = msex.ullAvailPhys;
			model.AvailableMem = msex.ullTotalPhys;
			model.CpuLoad = ViewboxApplication.ProcessorTime.NextValue();
			model.DiscSpeed = ViewboxApplication.HardDiskTransferSpeed.NextValue();
			model.DiscSpeed /= 10f;
			model.UserList = new List<SelectListItem>();
			foreach (IUser user in ViewboxApplication.Database.SystemDb.GetLoggedUsers(ViewboxSession.User))
			{
				model.UserList.Add(new SelectListItem
				{
					Value = user.Id.ToString(),
					Text = user.UserName
				});
			}
			DateTime datetime = ((filterdate != null) ? DateTime.Parse(filterdate) : DateTime.Now);
			base.ViewBag.filterDate = datetime.ToShortDateString();
			base.ViewBag.showHidden = showHidden;
			if (id == 0 && model.UserList.Count > 0)
			{
				int.TryParse(model.UserList[0].Value, out id);
			}
			model.UserId = id;
			model.LogDate = datetime.ToShortDateString();
			model.ShowHidden = showHidden;
			model.NewLogActions = new NewLogActionCollection();
			model.NewLogActions = ViewboxApplication.Database.GetUserActions(id, datetime, showHidden);
			return View("Index", model);
		}

		public ActionResult Remove(int id)
		{
			return RedirectToAction("Index");
		}

		public ActionResult Hide(int id, int userId, string filterDate, bool showHidden)
		{
			ViewboxApplication.Database.SystemDb.ShowHideLog(id, userId, showHidden);
			return RedirectToAction("OtherUser", "ControlCenter", new
			{
				id = userId,
				filterDate = filterDate,
				showHidden = showHidden
			});
		}

		public ActionResult RunningJobs()
		{
			var list = Transformation.List.Select((Transformation job) => new
			{
				key = job.Key,
				user = ViewboxSession.User.Name,
				start = $"{job.StartTime}",
				end = ((job.EndTime != DateTime.MinValue) ? $"{job.EndTime}" : null),
				time = $"{job.Runtime.Hours}:{job.Runtime.Minutes:00}:{job.Runtime.Seconds:00}",
				status = job.Status.ToString()
			});
			return PartialView("_JobListPartial");
		}

		public ActionResult RunningJ()
		{
			var list = Transformation.List.Select((Transformation job) => new
			{
				key = job.Key,
				user = ((job.Owners[0] != null) ? job.Owners[0].Name : "Unknown"),
				start = $"{job.StartTime}",
				end = ((job.EndTime != DateTime.MinValue) ? $"{job.EndTime}" : null),
				time = $"{job.Runtime.Hours}:{job.Runtime.Minutes:00}:{job.Runtime.Seconds:00}",
				status = job.Status.ToString()
			});
			return Json(list, JsonRequestBehavior.AllowGet);
		}

		public PartialViewResult RefreshJobs(string search = "")
		{
			ControlCenterModel model = new ControlCenterModel(search);
			return PartialView("_JobListPartial", model);
		}

		[HttpPost]
		public ActionResult CancelJob(string key, bool json = false)
		{
			Transformation job = Transformation.Find(key);
			if (job != null)
			{
				if (job.Owners.Count == 1)
				{
					job.Cancel();
				}
				else
				{
					job.Owners.Remove(ViewboxSession.User);
				}
			}
			if (json)
			{
				return Json(job == null, JsonRequestBehavior.AllowGet);
			}
			return RedirectToAction("Index");
		}

		[HttpPost]
		public ActionResult CancelOtherUserJob(string key, bool json = false)
		{
			Transformation job = Transformation.Find(key);
			if (job != null)
			{
				job.Owners.Clear();
				job.Cancel();
			}
			if (json)
			{
				return Json(job == null, JsonRequestBehavior.AllowGet);
			}
			return RedirectToAction("Index");
		}

		public ActionResult Result(string key)
		{
			Transformation job = Transformation.Find(key);
			ViewboxSession.RemoveJob(key);
			Transformation.TableListParams pParams = null;
			if (job.notificationData != null && job.notificationData.Params != null)
			{
				pParams = new Transformation.TableListParams
				{
					Search = job.notificationData.Params.Search,
					ShowArchived = job.notificationData.Params.ShowArchived,
					ShowHidden = job.notificationData.Params.ShowHidden,
					ShowEmpty = job.notificationData.Params.ShowEmpty
				};
			}
			if (job.TransformationObject == null && job.notificationData != null)
			{
				if (job.notificationData.Controller == "ArchiveDocuments" && job.notificationData.Method == "OpenDocument" && pParams != null)
				{
					return RedirectToAction(job.notificationData.Method, job.notificationData.Controller, new
					{
						search = pParams.Search
					});
				}
				return RedirectToAction(job.notificationData.Method, job.notificationData.Controller, pParams);
			}
			if (job.TransformationObject.OriginalTable.Type == TableType.Archive)
			{
				return RedirectToAction("Index", "Documents", new
				{
					id = job.TransformationObject.Table.Id
				});
			}
			if (job.TransformationObject.OriginalTable.Type == TableType.ArchiveDocument)
			{
				return RedirectToAction("Index", "ArchiveDocuments", new
				{
					id = job.TransformationObject.Table.Id
				});
			}
			return RedirectToAction("Index", "DataGrid", new
			{
				id = job.TransformationObject.Table.Id
			});
		}

		public void DeleteTransformation(string key)
		{
			Transformation trans = Transformation.Find(key);
			if (trans.Owners.Contains(ViewboxSession.User))
			{
				trans.Owners.Remove(ViewboxSession.User);
			}
			if (trans.Listeners.Contains(ViewboxSession.User))
			{
				trans.Listeners.Remove(ViewboxSession.User);
			}
			if (trans.JobShown.Contains(ViewboxSession.User))
			{
				trans.JobShown.Remove(ViewboxSession.User);
			}
			if (ViewboxSession.User.IsSuper)
			{
				trans.Owners.Clear();
				trans.Listeners.Clear();
				trans.JobShown.Clear();
			}
		}
	}
}
