using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using SystemDb;
using Viewbox.Controllers;
using ViewboxDb;

namespace Viewbox
{
	public class ViewboxFilterAttribute : ActionFilterAttribute
	{
		private enum ValidityCheckResult
		{
			Valid,
			RedirectHome,
			DeadUrl,
			NoRights
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
			HttpContext.Current.Response.Cache.SetNoStore();
			if (ViewboxSession.User != null)
			{
				if (ViewboxSession.User.UserRightHelper != null)
				{
					ViewboxSession.UserRightHelper = ViewboxSession.User.UserRightHelper;
					FormsAuthentication.SignOut();
					HttpContext.Current.Request.Cookies.Remove(FormsAuthentication.FormsCookieName);
					ViewboxSession.Logoff();
					context.Result = new RedirectToRouteResult(new RouteValueDictionary
					{
						{ "controller", "Error" },
						{ "action", "RoleChangedQuit" }
					});
					return;
				}
				if (!ViewboxSession.UserInitialized)
				{
					ViewboxApplication.Database.SystemDb.UpdateUserController(ViewboxSession.User, "IssueList");
					try
					{
						ViewboxApplication.Database.SystemDb.UserLastIssueSettingsCollection.Remove(ViewboxApplication.Database.SystemDb.UserLastIssueSettingsCollection[ViewboxSession.User]);
					}
					catch (Exception)
					{
					}
					ViewboxSession.UserInitialized = true;
				}
				if (string.IsNullOrEmpty(ViewboxSession.User.SessionId))
				{
					ViewboxSession.User.SessionId = ViewboxSession.Key;
				}
				else if (ViewboxSession.User.SessionId != ViewboxSession.Key)
				{
					try
					{
						FormsAuthentication.SignOut();
						HttpContext.Current.Request.Cookies.Remove(FormsAuthentication.FormsCookieName);
						ViewboxSession.Logoff();
						context.Result = new RedirectToRouteResult(new RouteValueDictionary
						{
							{ "controller", "Error" },
							{ "action", "ForcedToQuit" }
						});
					}
					catch
					{
					}
					return;
				}
				if (ViewboxSession.User.UserName.ToLower() == "avendata_admin" && !ViewboxSession.User.IsLogRead)
				{
					ViewboxSession.User.IsLogRead = true;
				}
				if (ViewboxSession.User.IsSuper && ViewboxSession.RightsMode && context.HttpContext.Request.Params["rm_type"] == "User" && context.ActionDescriptor.ControllerDescriptor.ControllerName == "Settings")
				{
					context.Result = new RedirectToRouteResult(new RouteValueDictionary
					{
						{ "controller", "Settings" },
						{ "action", "Personal" }
					});
				}
			}
			if (ViewboxApplication.DatabaseOutOfDateInformation != null)
			{
				context.Result = new RedirectToRouteResult(new RouteValueDictionary
				{
					{ "controller", "Error" },
					{ "action", "DatabaseOutOfDate" }
				});
				return;
			}
			if (!ViewboxApplication.Initialized)
			{
				if (ViewboxApplication.Initializing)
				{
					context.Result = new RedirectToRouteResult(new RouteValueDictionary
					{
						{ "controller", "Error" },
						{ "action", "NotReady" },
						{
							"ReturnUrl",
							context.HttpContext.Request.RawUrl
						}
					});
				}
				else
				{
					context.Result = new RedirectToRouteResult(new RouteValueDictionary
					{
						{ "controller", "Error" },
						{ "action", "Maintenance" },
						{
							"ReturnUrl",
							context.HttpContext.Request.RawUrl
						}
					});
				}
				return;
			}
			if (!ViewboxSession.updMode.HasValue)
			{
				if (int.TryParse(ViewboxSession.GetValueFromInfo("UpdateMode"), out var um))
				{
					ViewboxSession.updMode = um;
				}
				else
				{
					ViewboxSession.updMode = null;
				}
			}
			if (ViewboxSession.User != null && ViewboxSession.updMode == 1 && !ViewboxApplication.Initializing && ViewboxApplication.Initialized && ViewboxSession.User.UserName.ToLower() != "avendata_admin" && ViewboxSession.User.UserName.ToLower() != "avendata_qs")
			{
				try
				{
					context.Result = new RedirectToRouteResult(new RouteValueDictionary
					{
						{ "controller", "Error" },
						{ "action", "UpdateMode" }
					});
				}
				catch
				{
				}
				return;
			}
			if (ViewboxSession.isFreeSpaceChecked++ == 0)
			{
				DriveInfo[] drives = DriveInfo.GetDrives();
				foreach (DriveInfo drive in drives)
				{
					if (drive.IsReady && drive.Name == "C:\\")
					{
						long freeSpace = drive.TotalFreeSpace / 1048576;
						long freeSpaceRequirement = Convert.ToInt64(ConfigurationManager.AppSettings["FreeDiskSpaceRequirement"]);
						if (freeSpace < freeSpaceRequirement && freeSpaceRequirement != 0)
						{
							context.Result = new RedirectToRouteResult(new RouteValueDictionary
							{
								{ "controller", "Error" },
								{ "action", "NotEnoughFreeSpaceInformation" }
							});
							return;
						}
					}
				}
			}
			if (context.ActionDescriptor.ControllerDescriptor.ControllerName != "Error")
			{
				string sRightsMode = context.HttpContext.Request.Params["rights_mode"];
				bool.TryParse(sRightsMode, out var rightsMode);
				ViewboxSession.SetAction(context.ActionDescriptor.ActionName, context.ActionDescriptor.ControllerDescriptor.ControllerName, DateTime.Now, context.ActionParameters, rightsMode);
				ViewboxSession.NewLogSetAction(context.ActionDescriptor.ActionName, context.ActionDescriptor.ControllerDescriptor.ControllerName, DateTime.Now, context.ActionParameters, rightsMode);
			}
			string sRightsMode2 = context.HttpContext.Request.Params["rights_mode"];
			if (bool.TryParse(sRightsMode2, out var rightsMode2))
			{
				if (rightsMode2)
				{
					string sId = context.HttpContext.Request.Params["rm_id"];
					string sType = context.HttpContext.Request.Params["rm_type"];
					if (int.TryParse(sId, out var id2) && Enum.TryParse<CredentialType>(sType, out var type))
					{
						ViewboxSession.EnableRightsMode(id2, type);
					}
				}
				else
				{
					ViewboxSession.DisableRightsMode();
				}
			}
			if (ViewboxSession.User != null && ViewboxApplication.UserSessions.Current)
			{
				ViewboxApplication.UserSessions.Current = false;
				ViewboxSession.SetupObjects();
			}
			OptimizationManager.Check(context);
			string candc = context.HttpContext.Request.Params["candc"];
			if (!string.IsNullOrEmpty(candc))
			{
				if (bool.TryParse(candc, out var tempCandC))
				{
					ViewboxSession.RedirectionByCandCHappened = tempCandC;
				}
				else
				{
					ViewboxSession.RedirectionByCandCHappened = false;
				}
			}
			string cc = context.HttpContext.Request.Params["lang"];
			if (cc != null)
			{
				ViewboxSession.Language = ViewboxApplication.GetLanguageByCountryCode(cc);
			}
			else if (ViewboxApplication.OnlyGermanLanguageEnabled && ViewboxSession.Language != null && ViewboxSession.Language.CountryCode != "de")
			{
				ViewboxSession.Language = ViewboxApplication.GetLanguageByCountryCode("de");
			}
			string controllerName = context.ActionDescriptor.ControllerDescriptor.ControllerName;
			string actionName = context.ActionDescriptor.ActionName;
			ValidityCheckResult result = ValidityCheckResult.Valid;
			if (controllerName == "DataGrid" && actionName == "Index")
			{
				if (context.ActionParameters.ContainsKey("id") && context.ActionParameters["id"] != null)
				{
					int id = (int)context.ActionParameters["id"];
					bool check;
					if (id < 0)
					{
						TableObject tobj2 = ViewboxSession.TempTableObjects[id];
						check = tobj2 != null;
					}
					else
					{
						ITableObject tobj = ViewboxSession.TableObjects[id];
						check = tobj != null;
					}
					if (!check)
					{
						result = ((id == 0 || ViewboxSession.User.Id == 0 || !(context.Controller is DataGridController) || ViewboxApplication.Database.SystemDb.GetUserRightsToTable(id, ViewboxSession.User.Id)) ? ValidityCheckResult.DeadUrl : ValidityCheckResult.NoRights);
					}
				}
				else
				{
					result = ValidityCheckResult.DeadUrl;
				}
			}
			else if (((controllerName == "Settings" && actionName == "RolesConfig") || (controllerName == "UserManagement" && (actionName == "UpdateTableObjectRoles" || actionName == "UpdateOptimizationRoles"))) && (ViewboxSession.User == null || !ViewboxSession.User.CanGrant || !ViewboxSession.RightsMode || ViewboxSession.RightsModeCredential.Type != 0))
			{
				result = ValidityCheckResult.RedirectHome;
			}
			switch (result)
			{
			case ValidityCheckResult.RedirectHome:
				context.Result = new RedirectToRouteResult(new RouteValueDictionary
				{
					{ "controller", "Home" },
					{ "action", "Index" }
				});
				break;
			case ValidityCheckResult.DeadUrl:
				context.Result = new RedirectToRouteResult(new RouteValueDictionary
				{
					{ "controller", "Error" },
					{ "action", "DeadUrl" }
				});
				break;
			case ValidityCheckResult.NoRights:
				context.Result = new RedirectToRouteResult(new RouteValueDictionary
				{
					{ "controller", "Error" },
					{ "action", "NoRightsForContent" }
				});
				break;
			}
		}
	}
}
