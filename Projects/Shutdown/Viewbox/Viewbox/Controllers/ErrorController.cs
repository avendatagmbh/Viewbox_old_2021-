using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using SystemDb.Enums;
using Viewbox.Models;
using Viewbox.Properties;

namespace Viewbox.Controllers
{
	public class ErrorController : BaseController
	{
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult NotReady(bool json = false, int retry = 1000)
		{
			NotReadyModel model = new NotReadyModel
			{
				Retry = retry
			};
			if (base.Request.Params["ReturnUrl"] != null)
			{
				model.ReturnUrl = base.Request.Params["ReturnUrl"];
			}
			if (json)
			{
				return Json(model);
			}
			return View(model);
		}

		[OutputCache(Duration = 0, Location = OutputCacheLocation.Client, VaryByParam = "*")]
		public ActionResult DatabaseOutOfDate()
		{
			return View(new DatabaseOutOfDateModel(ViewboxApplication.DatabaseOutOfDateInformation));
		}

		public ActionResult UpgradeDatabase()
		{
			try
			{
				ViewboxApplication.UpgradeDatabase();
				return View(new DatabaseUpgradeFinishedModel(string.Empty));
			}
			catch (Exception ex)
			{
				return View(new DatabaseUpgradeFinishedModel(ex.Message));
			}
		}

		public ActionResult NotEnoughFreeSpaceInformation()
		{
			return View(new NotEnoughFreeSpaceModel(AppDomain.CurrentDomain.BaseDirectory.Substring(0, 3)));
		}

		[HttpGet]
		public JsonResult UpdateMySqlConfig()
		{
			MySqlAutoConfigModel model = new MySqlAutoConfigModel();
			try
			{
				model.ConfigureMySqlServer();
				return Json(Resources.mySQLConfSuccessText, JsonRequestBehavior.AllowGet);
			}
			catch (Exception)
			{
				return Json(Resources.mySQLConfUnsuccessText, JsonRequestBehavior.AllowGet);
			}
		}

		public ActionResult MySqlAutoConfig()
		{
			MySqlAutoConfigModel model = new MySqlAutoConfigModel();
			if (model.IsMySqlConfigurationNeeded())
			{
				model.InitDialog();
				return View(model);
			}
			return RedirectToAction("NotReady");
		}

		public ActionResult UpgradeDatabaseSuccessful()
		{
			ViewboxApplication.Init();
			return RedirectToAction("NotReady");
		}

		[OutputCache(Duration = 0, Location = OutputCacheLocation.Client, VaryByParam = "*")]
		public ActionResult DeadUrl()
		{
			return View(new DeadUrlModel());
		}

		[OutputCache(Duration = 0, Location = OutputCacheLocation.Client, VaryByParam = "*")]
		public ActionResult Maintenance()
		{
			return View(new MaintenanceModel());
		}

		public ActionResult HandleAppRestart()
		{
			if (ViewboxApplication.Initialized)
			{
				throw new HttpException(404, "Not Found");
			}
			HttpResponseBase hResp = base.HttpContext.Response;
			HttpRuntime.UnloadAppDomain();
			hResp.Clear();
			hResp.Redirect("/", endResponse: true);
			return new EmptyResult();
		}

		public ActionResult UserAlreadyLoggedIn()
		{
			return View(new UserAlreadyLoggedInModel());
		}

		public ActionResult ForcedToQuit()
		{
			if (base.Request.IsAjaxRequest())
			{
				return PartialView(new ForcedToQuitModel());
			}
			return View(new ForcedToQuitModel());
		}

		public ActionResult RoleChangedQuit()
		{
			if (ViewboxSession.UserRightHelper != null)
			{
				ErrorModel model = new ErrorModel();
				model.Dialog = new DialogModel
				{
					DialogTemplate = "_ExtendedDialogPartial",
					Title = Resources.ForcedToQuitTitle,
					DialogType = DialogModel.Type.Info,
					Buttons = new List<DialogModel.Button>
					{
						new DialogModel.Button
						{
							Caption = Resources.OK
						}
					}
				};
				if (ViewboxSession.UserRightHelper.ChangeRight == ChangeRightType.Role)
				{
					model.Dialog.Content = string.Format(Resources.RoleRightChangedQuit, "<span style=\"color: red;\">" + ViewboxSession.UserRightHelper.ModifyUser.GetName() + "</span>");
				}
				else
				{
					model.Dialog.Content = string.Format(Resources.UserRightChangedQuit, "<span style=\"color: red;\">" + ViewboxSession.UserRightHelper.ModifyUser.GetName() + "</span>");
				}
				if (base.Request.IsAjaxRequest())
				{
					return PartialView(model);
				}
				return View(model);
			}
			return ForcedToQuit();
		}

		public ActionResult UpdateMode()
		{
			if (ViewboxSession.updMode == 1)
			{
				return View(new UpdateModel());
			}
			if (ViewboxSession.User != null)
			{
				return RedirectToAction("Index", ViewboxApplication.Database.SystemDb.UserControllerSettings[ViewboxSession.User].Controller);
			}
			return Redirect("http://" + base.HttpContext.Request.Url.Authority + "/Account/LogOn");
		}

		public ActionResult NoRightForOpt()
		{
			return View(new NoRightForOptModel());
		}

		[OutputCache(Duration = 0, Location = OutputCacheLocation.Client, VaryByParam = "*")]
		public ActionResult NoRightsForContent()
		{
			return View(new NoRightsForContentModel());
		}
	}
}
