using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using SystemDb;
using SystemDb.Enums;
using SystemDb.Helper;
using AV.Log;
using DbAccess;
//using Newtonsoft.Json;
using Utils;
using Viewbox.Job;
using Viewbox.Models;
using Viewbox.Properties;
using ViewboxDb;

namespace Viewbox.Controllers
{
	[Authorize]
	[ViewboxFilter]
	public class UserManagementController : BaseController
	{
		private class CategoryList : ICategoryList
		{
			public int SelectedId { get; set; }

			public TableType Type { get; set; }

			public bool RightsMode => ViewboxSession.RightsMode;

			public ICategoryCollection Categories => Type switch
			{
				TableType.Issue => ViewboxSession.IssueCategories, 
				TableType.Table => ViewboxSession.TableCategories, 
				TableType.View => ViewboxSession.ViewCategories, 
				_ => null, 
			};

			public ICategory SelectedCategory => Categories[SelectedId];
		}

		internal static List<string> UserOptions = new List<string> { "normal", "canGrant" };

		internal static List<string> ExportOptions = new List<string> { "enabled", "disabled" };

		public static List<string> UserOptionsLang => new List<string>
		{
			Resources.NormalUser,
			Resources.CanGrant
		};

		public static List<string> ExportLang => new List<string>
		{
			Resources.Enabled,
			Resources.Disabled
		};

		public ActionResult Index()
		{
			UserManagementModel model = new UserManagementModel();
			return View(model);
		}

		private DialogModel CopyTableTypeRightSwitchDialogModel(string resTableType)
		{
			DialogModel DM = new DialogModel();
			DM.Title = string.Format(Resources.DoYouWantToSwitchRightsOfSelectedTableTypes, resTableType);
			DM.DialogType = DialogModel.Type.Info;
			DM.Buttons = new List<DialogModel.Button>();
			DM.Buttons = new List<DialogModel.Button>
			{
				new DialogModel.Button
				{
					Caption = Resources.Cancel,
					Data = false.ToString()
				},
				new DialogModel.Button
				{
					Caption = Resources.Yes,
					Data = true.ToString()
				}
			};
			return DM;
		}

		public ActionResult CreateTableTypeRightSwitchDialog(string tableType)
		{
			string resTableType = string.Empty;
			switch (tableType)
			{
			case "View":
				resTableType = Resources.Views;
				break;
			case "Table":
				resTableType = Resources.Tables;
				break;
			case "Issue":
				resTableType = Resources.Issues;
				break;
			case "Archive":
				resTableType = Resources.Archive;
				break;
			}
			return PartialView("_ExtendedDialogPartial", CopyTableTypeRightSwitchDialogModel(resTableType));
		}

		private void ApplyDisplaySettingsForAll()
		{
			IUser currentuser = ViewboxSession.User;
			foreach (IUser user in ViewboxApplication.Users)
			{
				if (currentuser.CanGrant(user))
				{
					CopyUserSettings(currentuser, user);
				}
			}
		}

		private void CopyUserSettings(IUser fromUser, IUser toUser)
		{
			IPropertiesCollection propertylist = ViewboxApplication.Database.SystemDb.GetProperties(fromUser);
			foreach (IProperty property in propertylist)
			{
				ViewboxApplication.Database.SystemDb.UpdateProperty(toUser, property);
			}
		}

		public ActionResult CreateUser()
		{
			return PartialView("_ExtendedDialogPartial", new DialogModel
			{
				Title = Resources.CreateUserTitle,
				Content = Resources.CreateUserText,
				DialogType = DialogModel.Type.Info,
				Inputs = new List<Tuple<string, string, string, string, int, int>>
				{
					new Tuple<string, string, string, string, int, int>(Resources.Username, "text", "userName", "", 256, 0),
					new Tuple<string, string, string, string, int, int>(Resources.Password, "password", "password", "", 256, 0),
					new Tuple<string, string, string, string, int, int>(Resources.RepeatPassword, "password", "repeatPassword", "", 256, 0),
					new Tuple<string, string, string, string, int, int>(Resources.Name + " (" + Resources.Optional + ")", "text", "name", "", 256, 0),
					new Tuple<string, string, string, string, int, int>(Resources.EMail + " (" + Resources.Optional + ")", "text", "email", "", 256, 0)
				},
				Select = new List<Tuple<string, string, List<string>, List<string>, int>>
				{
					new Tuple<string, string, List<string>, List<string>, int>(Resources.SpecialRights, "specialRights", UserOptionsLang, UserOptions, 0),
					new Tuple<string, string, List<string>, List<string>, int>(Resources.ExportRights, "exportRights", ExportLang, ExportOptions, 0)
				},
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.Cancel,
						Data = false.ToString()
					},
					new DialogModel.Button
					{
						Caption = Resources.Submit,
						Data = true.ToString()
					}
				}
			});
		}

		[HttpPost]
		public ActionResult CreateUser(string userName, string password, string repeatPassword, string name = "", string email = "", string specialRights = "", string exportRights = "")
		{
			bool check = true;
			bool wrongEmail = false;
			string errorMsg = "";
			if (ViewboxApplication.ByUserName(userName) != null)
			{
				return RedirectToAction("AlreadyExisting", "Settings", new { password, repeatPassword, name, email });
			}
			if (string.IsNullOrEmpty(userName))
			{
				check = false;
			}
			if (string.IsNullOrEmpty(password) || password != repeatPassword)
			{
				check = false;
			}
			if (!PasswordModel.IsValidPasswordPattern(password, ViewboxApplication.ByUserName(userName)))
			{
				errorMsg = "Password policy: " + ViewboxApplication.PasswordPolicyDescription;
			}
			if (!Regex.IsMatch(password, ViewboxApplication.PasswordRegex))
			{
				errorMsg = ViewboxApplication.PasswordPolicyDescription;
			}
			Regex rxmail = new Regex("^[-a-zA-Z0-9][-.a-zA-Z0-9]*@[-.a-zA-Z0-9]+(\\.[-.a-zA-Z0-9]+)*\\.[a-zA-Z]{2,10}$", RegexOptions.Compiled);
			if (!string.IsNullOrEmpty(email) && !rxmail.IsMatch(email))
			{
				check = false;
				wrongEmail = true;
				email = "";
			}
			if (check && string.IsNullOrEmpty(errorMsg))
			{
				SpecialRights specialRightsEnum = SpecialRightsEnumFromString(specialRights);
				ExportRights exportRightsEnum = ExportRightsEnumFromString(exportRights);
				ViewboxSession.CreateUser(userName, name, specialRightsEnum, exportRightsEnum, password, email, 0);
				return RedirectToAction("Rights", "Settings", new
				{
					json = true
				});
			}
			return RedirectToAction("InvalidValue", "Settings", new
			{
				userName = userName,
				password = password,
				repeatPassword = repeatPassword,
				name = name,
				email = email,
				wrongEmail = wrongEmail,
				message = errorMsg
			});
		}

		private static SpecialRights SpecialRightsEnumFromString(string specialRights)
		{
			SpecialRights specialRightsEnum = SpecialRights.None;
			if (specialRights == "canGrant")
			{
				specialRightsEnum |= SpecialRights.Grant;
			}
			return specialRightsEnum;
		}

		private static ExportRights ExportRightsEnumFromString(string exportRights)
		{
			ExportRights exportRightsEnum = ExportRights.Enabled;
			if (string.CompareOrdinal(exportRights, "disabled") == 0)
			{
				exportRightsEnum = ExportRights.Disabled;
			}
			return exportRightsEnum;
		}

		public ActionResult UpdateUser(int id, string userName, string name = "", SpecialRights flags = SpecialRights.None, ExportRights exportAllowed = ExportRights.Enabled, string password = "", string email = "")
		{
			ViewboxSession.CreateUser(userName, name, flags, exportAllowed, password, email, id);
			return RedirectToAction("Rights", "Settings", new
			{
				json = true
			});
		}

		public ActionResult DeleteUser(int id)
		{
			ViewboxSession.DeleteUser(id);
			Credential c = ViewboxSession.RightsModeCredential;
			if (c != null && c.Type == CredentialType.User && c.Id == id)
			{
				ViewboxSession.DisableRightsMode();
			}
			return RedirectToAction("Rights", "Settings", new
			{
				json = true
			});
		}

		public static DialogModel CreateRoleDialogModel(string title = "", string content = "", DialogModel.Type type = DialogModel.Type.Info)
		{
			List<string> RoleOptionsLang = new List<string>
			{
				Resources.NormalRole,
				Resources.CanGrantRole
			};
			if (string.IsNullOrEmpty(title))
			{
				title = Resources.CreateRole;
			}
			if (string.IsNullOrEmpty(content))
			{
				content = Resources.EnterRoleName;
			}
			return new DialogModel
			{
				Title = title,
				Content = content,
				InputName = "name",
				DialogType = type,
				Inputs = new List<Tuple<string, string, string, string, int, int>>
				{
					new Tuple<string, string, string, string, int, int>(Resources.Name, "text", "name", "", 256, 0)
				},
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.Cancel,
						Data = false.ToString()
					},
					new DialogModel.Button
					{
						Caption = Resources.Add,
						Data = true.ToString()
					}
				},
				Select = new List<Tuple<string, string, List<string>, List<string>, int>>
				{
					new Tuple<string, string, List<string>, List<string>, int>(Resources.RoleSpecialStatus, "specialRights", RoleOptionsLang, UserOptions, 0),
					new Tuple<string, string, List<string>, List<string>, int>(Resources.ExportRights, "exportRights", ExportLang, ExportOptions, 0)
				}
			};
		}

		public ActionResult CreateRoleDialog()
		{
			return PartialView("_ExtendedDialogPartial", CreateRoleDialogModel());
		}

		public static DialogModel CreateUploadDialogModel(string title = "", string content = "", DialogModel.Type type = DialogModel.Type.Info)
		{
			List<string> RoleOptionsLang = new List<string>
			{
				Resources.NormalRole,
				Resources.CanGrantRole
			};
			if (string.IsNullOrEmpty(title))
			{
				title = "Role upload";
			}
			if (string.IsNullOrEmpty(content))
			{
				content = "Upload the csv-s";
			}
			return new DialogModel
			{
				Title = title,
				Content = content,
				InputName = "1",
				DialogType = type,
				FormAction = "/Upload",
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.Cancel,
						Data = false.ToString()
					},
					new DialogModel.Button
					{
						Caption = Resources.CsvLoadFile,
						Data = true.ToString()
					}
				},
				Upload = new List<Tuple<string, string, string>>
				{
					Tuple.Create("Role CSV", "File", "RoleFile"),
					Tuple.Create("Optimization CSV", "File", "OptFile"),
					Tuple.Create("Tables CSV", "File", "TableFile"),
					Tuple.Create("Rows CSV", "File", "RowFile")
				}
			};
		}

		public ActionResult CreateUploadDialog()
		{
			return PartialView("_ExtendedDialogPartial", CreateUploadDialogModel());
		}

		private DialogModel OptimizationDeletionDialogModel()
		{
			DialogModel DM = new DialogModel();
			DM.Title = Resources.OptimizationDelete;
			DM.DialogType = DialogModel.Type.Warning;
			DM.Buttons = new List<DialogModel.Button>();
			DM.Buttons = new List<DialogModel.Button>
			{
				new DialogModel.Button
				{
					Caption = Resources.Cancel,
					Data = false.ToString()
				},
				new DialogModel.Button
				{
					Caption = Resources.OK,
					Data = true.ToString()
				}
			};
			return DM;
		}

		public ActionResult CreateOptimizationDeleteDialog()
		{
			return PartialView("_ExtendedDialogPartial", OptimizationDeletionDialogModel());
		}

		public DialogModel CreateInfoDialogModel()
		{
			DialogModel DM = new DialogModel();
			DM.Title = Resources.ApplyForUsers;
			DM.DialogType = DialogModel.Type.Info;
			DM.Buttons = new List<DialogModel.Button>();
			DM.Buttons = new List<DialogModel.Button>
			{
				new DialogModel.Button
				{
					Caption = Resources.Cancel,
					Data = false.ToString()
				},
				new DialogModel.Button
				{
					Caption = Resources.OK,
					Data = true.ToString()
				}
			};
			return DM;
		}

		public ActionResult CreateInfoDialog()
		{
			return PartialView("_ExtendedDialogPartial", CreateInfoDialogModel());
		}

		public ActionResult CreateInfo()
		{
			return RedirectToAction("Rights", "Settings", new
			{
				json = true
			});
		}

		public ActionResult CreateRole(string name, string specialRights, string exportRights = "")
		{
			if (string.IsNullOrEmpty(name))
			{
				return RedirectToAction("InvalidValue", "Settings", new
				{
					type = CredentialType.Role
				});
			}
			SpecialRights flags = SpecialRightsEnumFromString(specialRights);
			ExportRights exportRightsEnum = ExportRightsEnumFromString(exportRights);
			RoleType type = RoleType.None;
			int count = 0;
			using (DatabaseBase db = ViewboxApplication.Database.ConnectionManager.GetConnection())
			{
				string sql = $"SELECT count(*) FROM {db.DbConfig.DbName}.`roles` WHERE `name` = '{name}';";
				if (int.Parse(db.ExecuteScalar(sql).ToString()) != 0)
				{
					return RedirectToAction("InvalidValue", "Settings", new
					{
						type = CredentialType.Role
					});
				}
			}
			ViewboxSession.CreateRole(name, flags, exportRightsEnum, type, 0);
			return RedirectToAction("Rights", "Settings", new
			{
				json = true
			});
		}

		public ActionResult UpdateRole(int id, string name, SpecialRights flags = SpecialRights.None, ExportRights exportRights = ExportRights.Enabled, RoleType type = RoleType.None)
		{
			ViewboxSession.CreateRole(name, flags, exportRights, type, id);
			return RedirectToAction("Rights", "Settings", new
			{
				json = true
			});
		}

		public ActionResult DeleteRole(int id)
		{
			ChangeRoleRight(id);
			ViewboxSession.DeleteRole(id);
			Credential c = ViewboxSession.RightsModeCredential;
			if (c != null && c.Type == CredentialType.Role && c.Id == id)
			{
				ViewboxSession.DisableRightsMode();
			}
			return RedirectToAction("Rights", "Settings", new
			{
				json = true
			});
		}

		public ActionResult SwitchMode(int id, CredentialType type)
		{
			ViewboxSession.EnableRightsMode(id, type);
			return null;
		}

		public ActionResult UpdateOptimizationRight(int id, RightType right, string returnUrl)
		{
			ViewboxSession.UpdateRight(id, UpdateRightType.Optimization, right);
			return PartialView("_BoxHeaderPartial", new BoxHeaderModel
			{
				ReturnUrl = returnUrl
			});
		}

		public ActionResult UpdateOptimizationRightNavigationBar(int id, RightType right, string returnUrl)
		{
			ViewboxSession.UpdateGroupRight(id, UpdateRightType.Optimization, right);
			return PartialView("_BoxHeaderPartial", new BoxHeaderModel
			{
				ReturnUrl = returnUrl
			});
		}

		public ActionResult UpdateCategoryRight(int id, RightType right, TableType type, string search = null, bool showEmpty = true, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool json = false, int page = 0, int size = 25)
		{
			ViewboxSession.UpdateRight(id, UpdateRightType.Category, right);
			return type switch
			{
				TableType.Issue => RedirectToAction("Index", "IssueList", new
				{
					id = id,
					search = search,
					sortColumn = sortColumn,
					direction = direction,
					json = json,
					page = page,
					size = size,
					catRefresh = true
				}), 
				TableType.View => RedirectToAction("Index", "ViewList", new
				{
					id = id,
					search = search,
					showEmpty = showEmpty,
					sortColumn = sortColumn,
					direction = direction,
					json = json,
					page = page,
					size = size,
					catRefresh = true
				}), 
				TableType.Table => RedirectToAction("Index", "TableList", new
				{
					id = id,
					search = search,
					showEmpty = showEmpty,
					sortColumn = sortColumn,
					direction = direction,
					json = json,
					page = page,
					size = size,
					catRefresh = true
				}), 
				_ => null, 
			};
		}

		public ActionResult UpdateRoleSettingDialog(RightType right, RoleSettingsType type, int tableId)
		{
			ITableObject tobj = ((tableId < 0) ? ViewboxSession.TempTableObjects[tableId].Table : ViewboxSession.TableObjects[tableId]);
			Transformation trans = Transformation.Create(right, type, tobj);
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Title = Resources.RoleSettingTitle,
				Key = trans.Key,
				Content = string.Format(Resources.RoleSettingText, type.ToString()),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.LongRunningDialogCaption
					}
				}
			});
		}

		public ActionResult UpdateTableTypeRightNavigationBar(RightType right, TableType type)
		{
			Transformation trans = Transformation.Create(right, type);
			string tableType = string.Empty;
			switch (type)
			{
			case TableType.Table:
				tableType = Resources.Tables;
				break;
			case TableType.Issue:
				tableType = Resources.Issues;
				break;
			case TableType.View:
				tableType = Resources.Views;
				break;
			case TableType.Archive:
				tableType = Resources.Archive;
				break;
			}
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Title = string.Format(Resources.ExecuteTableTypeRightsChangeTitle, tableType),
				Key = trans.Key,
				Content = string.Format(Resources.LongRunningDialogText, string.Format(Resources.ExecuteTableTypeRightsChangeTitle, tableType)),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.LongRunningDialogCaption
					}
				}
			});
		}

		public ActionResult UpdateTableTypeRightJob(int id, RightType right, TableType type, string search = null, bool showEmpty = true, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool json = false, int page = 0, int size = 25)
		{
			Transformation trans = Transformation.Create(right, type);
			return type switch
			{
				TableType.Issue => RedirectToAction("Index", "IssueList", new
				{
					id = id,
					search = search,
					sortColumn = sortColumn,
					direction = direction,
					json = json,
					page = page,
					size = size,
					catRefresh = true
				}), 
				TableType.View => RedirectToAction("Index", "ViewList", new
				{
					id = id,
					search = search,
					showEmpty = showEmpty,
					sortColumn = sortColumn,
					direction = direction,
					json = json,
					page = page,
					size = size,
					catRefresh = true
				}), 
				TableType.Table => RedirectToAction("Index", "TableList", new
				{
					id = id,
					search = search,
					showEmpty = showEmpty,
					sortColumn = sortColumn,
					direction = direction,
					json = json,
					page = page,
					size = size,
					catRefresh = true
				}), 
				_ => null, 
			};
		}

		public ActionResult UpdateTableTypeRight(int id, RightType right, TableType type, string search = null, bool showEmpty = true, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool json = false, int page = 0, int size = 25)
		{
			int[] idList = ViewboxApplication.Database.SystemDb.ReadTableTypeIds(type, ViewboxSession.SelectedSystem, ViewboxSession.User.Id);
			int[] array = idList;
			foreach (int tableId in array)
			{
				ViewboxSession.UpdateRight(tableId, UpdateRightType.TableObject, right);
			}
			return type switch
			{
				TableType.Issue => RedirectToAction("Index", "IssueList", new
				{
					id = id,
					search = search,
					sortColumn = sortColumn,
					direction = direction,
					json = json,
					page = page,
					size = size,
					catRefresh = true
				}), 
				TableType.View => RedirectToAction("Index", "ViewList", new
				{
					id = id,
					search = search,
					showEmpty = showEmpty,
					sortColumn = sortColumn,
					direction = direction,
					json = json,
					page = page,
					size = size,
					catRefresh = true
				}), 
				TableType.Table => RedirectToAction("Index", "TableList", new
				{
					id = id,
					search = search,
					showEmpty = showEmpty,
					sortColumn = sortColumn,
					direction = direction,
					json = json,
					page = page,
					size = size,
					catRefresh = true
				}), 
				_ => null, 
			};
		}

		public ActionResult UpdateTableObjectRight(int id, RightType right, string search = null, bool showEmpty = true, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool json = false, int page = 0, int size = 25, bool catRefresh = true, bool fullRefresh = false, bool showHidden = false, bool showArchived = false)
		{
			ViewboxSession.UpdateRight(id, UpdateRightType.TableObject, right);
			ITableObject tobj = ViewboxSession.TableObjects[id];
			return tobj.Type switch
			{
				TableType.Issue => RedirectToAction("ShowOne", "IssueList", new
				{
					id = id,
					showHidden = !tobj.IsVisible,
					catRefresh = catRefresh
				}), 
				TableType.View => RedirectToAction("Index", "ViewList", new
				{
					id = tobj.Category.Id,
					search = search,
					showEmpty = showEmpty,
					sortColumn = sortColumn,
					direction = direction,
					json = json,
					page = page,
					size = size,
					catRefresh = catRefresh,
					fullRefresh = fullRefresh,
					showHidden = showHidden,
					showArchived = showArchived
				}), 
				TableType.Table => RedirectToAction("Index", "TableList", new
				{
					id = tobj.Category.Id,
					search = search,
					showEmpty = showEmpty,
					sortColumn = sortColumn,
					direction = direction,
					json = json,
					page = page,
					size = size,
					catRefresh = catRefresh,
					fullRefresh = fullRefresh
				}), 
				_ => null, 
			};
		}

		public ActionResult UpdateReportListObjectRight(int id, RightType right, string search = null, bool showEmpty = true, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool json = false, int page = 0, int size = 25, bool catRefresh = true)
		{
			ViewboxSession.UpdateRight(id, UpdateRightType.TableObject, right);
			ITableObject tobj = ViewboxSession.TableObjects[id];
			return RedirectToAction("IndexList", "IssueList", new
			{
				id = tobj.Category.Id,
				search = search,
				showEmpty = showEmpty,
				sortColumn = sortColumn,
				direction = direction,
				json = json,
				page = page,
				size = size,
				catRefresh = catRefresh
			});
		}

		public ActionResult UpdateColumnRight(int id, RightType right)
		{
			ViewboxSession.UpdateRight(id, UpdateRightType.Column, right);
			return PartialView("_ViewOptionsPartial", ViewboxSession.Columns[id].Table);
		}

		public ActionResult UpdateLogRights(int id, RightType right)
		{
			ViewboxApplication.Database.SystemDb.UpdateUserLogRights(ViewboxSession.RightsModeCredential.Id, id, right);
			return RedirectToAction("Rights", "Settings");
		}

		public ActionResult UserAddRole(int user, int role)
		{
			ChangeUserRight(user);
			ViewboxSession.AddUserRoleMapping(user, role);
			return RedirectToAction("Rights", "Settings", new
			{
				json = true
			});
		}

		public ActionResult UserRemoveRole(int user, int role)
		{
			ChangeUserRight(user);
			ViewboxSession.RemoveUserRoleMapping(user, role);
			return RedirectToAction("Rights", "Settings", new
			{
				json = true
			});
		}

		public ContentResult CheckPassword(string password)
		{
			bool result = false;
			IUser user = ViewboxSession.User;
			if (ViewboxSession.RightsMode && ViewboxSession.RightsModeCredential.Type == CredentialType.User)
			{
				user = ViewboxApplication.Users[ViewboxSession.RightsModeCredential.Id];
			}
			if (user != ViewboxSession.User)
			{
				user.Password = password;
				result = true;
			}
			else
			{
				result = user.CheckPassword(password);
			}
			return Content(result.ToString(), "text/plain");
		}

		public ActionResult ImportUserFromADDialog(int currentDomain = 0, string filter = null)
		{
			List<string> domains = (from d in ActiveDirectory.GetDomains()
				select (d)).ToList();
			string title = Resources.AdTitle;
			string content = Resources.AdContent;
			ADImportModel model = new ADImportModel
			{
				Title = title,
				Content = content,
				Class = "active-directory",
				DialogType = ADImportModel.Type.Info,
				Inputs = new List<Tuple<string, string, string, string, int>>
				{
					new Tuple<string, string, string, string, int>(Resources.Filter + ":", "text", "filter", "", 256)
				},
				Buttons = new List<ADImportModel.Button>
				{
					new ADImportModel.Button
					{
						Caption = Resources.Cancel,
						Data = false.ToString()
					},
					new ADImportModel.Button
					{
						Caption = Resources.Import,
						Data = true.ToString()
					}
				},
				Select = new Tuple<string, string, List<string>, List<string>, int>(Resources.Domain + ":", "domains", domains, domains, currentDomain),
				Users = new List<Tuple<string, string, string>>()
			};
			return PartialView("_ADImportPartial", model);
		}

		public PartialViewResult GetADUserListPartial(string domainName, string filter)
		{
			List<Tuple<string, string>> users = ActiveDirectory.GetActiveDirectoryUsers(domainName, filter, useStar: true);
			List<Tuple<string, string, string>> list = new List<Tuple<string, string, string>>();
			foreach (Tuple<string, string> u2 in users.OrderBy((Tuple<string, string> u) => u.Item1))
			{
				list.Add(new Tuple<string, string, string>(u2.Item1, u2.Item2, domainName));
			}
			return PartialView("_ADUserListPartial", list);
		}

		[HttpPost]
		public ActionResult ImportUsersFromAD(ADUserCollection adUsers)
		{
			if (adUsers != null && adUsers.Count <= 50)
			{
				foreach (ADUser u in adUsers)
				{
					string shortName2 = ((u.ShortName.Length > 2) ? u.ShortName.Substring(1, u.ShortName.Length - 2) : string.Empty);
					if (ViewboxApplication.ByUserName(shortName2) != null)
					{
						throw new DuplicateNameException(string.Format(Resources.UserAlreadyExists, shortName2));
					}
				}
				foreach (ADUser u2 in adUsers)
				{
					string shortName = ((u2.ShortName.Length > 2) ? u2.ShortName.Substring(1, u2.ShortName.Length - 2) : string.Empty);
					if (ViewboxApplication.ByUserName(shortName) == null && !string.IsNullOrEmpty(shortName) && !string.IsNullOrEmpty(u2.Domain))
					{
						ViewboxSession.CreateUser(shortName, u2.Name, SpecialRights.None, ExportRights.Enabled, "", "", 0, isAdUser: true, u2.Domain);
					}
				}
				return RedirectToAction("Rights", "Settings", new
				{
					json = true
				});
			}
			throw new Exception(Resources.ADImportMaxLimit);
		}

		public ActionResult UploadDocument()
		{
			return View();
		}

		public PartialViewResult ShowErrorMessage(string error)
		{
			DialogModel dialog = new DialogModel
			{
				Title = Resources.Error,
				Content = error,
				DialogType = DialogModel.Type.Warning,
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.OK
					}
				}
			};
			return PartialView("_DialogPartial", dialog);
		}

		[HttpGet]
		public JsonResult SetRoleBasedFilter(int tableId, int columnId, string filterValue)
		{
			try
			{
				RowVisibilityHelper.SetRoleBasedFilter(tableId, columnId, filterValue);
				int roleId = ViewboxApplication.Roles[ViewboxSession.RightsModeCredential.Id].Id;
				ChangeRoleRight(roleId);
				return Json(true, JsonRequestBehavior.AllowGet);
			}
			catch (Exception)
			{
				return Json(false, JsonRequestBehavior.AllowGet);
			}
		}

		[HttpPost]
		public ActionResult SearchForTables(string search = "")
		{
			try
			{
				IEnumerable<ITable> tables = (from t in ViewboxApplication.Database.SystemDb.Tables
					orderby t.Name
					where t.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) != -1 || t.TransactionNumber.IndexOf(search, StringComparison.OrdinalIgnoreCase) != -1
					select t).Take(100);
				return PartialView("_TableListPartial", tables);
			}
			catch
			{
				return new EmptyResult();
			}
		}

		[HttpPost]
		public ActionResult SetRightsForTables(bool visible)
		{
			try
			{
				if (ViewboxSession.RightsMode && ViewboxSession.RightsModeCredential.Type == CredentialType.Role)
				{
					IRole role = ViewboxApplication.Database.SystemDb.Roles.FirstOrDefault((IRole r) => r.Id == ViewboxSession.RightsModeCredential.Id);
					if (role != null)
					{
						ChangeRoleRight(role.Id);
						RoleManagement.UpdateTablesVisibility(role, visible);
					}
				}
				return new EmptyResult();
			}
			catch (Exception ex)
			{
				LogHelper.GetLogger().ErrorWithCheck(ex.Message, ex);
			}
			return ShowErrorMessage(Resources.UnhandledExceptionMessage);
		}

		[HttpPost]
		public ActionResult SetRightForOneTable(int id, bool visible)
		{
			try
			{
				if (ViewboxSession.RightsMode && ViewboxSession.RightsModeCredential.Type == CredentialType.Role)
				{
					IRole role = ViewboxApplication.Database.SystemDb.Roles.FirstOrDefault((IRole r) => r.Id == ViewboxSession.RightsModeCredential.Id);
					if (role != null)
					{
						ChangeRoleRight(role.Id);
						RoleManagement.UpdateOneTableVisibility(id, role, visible);
					}
				}
				return new EmptyResult();
			}
			catch (Exception ex)
			{
				LogHelper.GetLogger().ErrorWithCheck(ex.Message, ex);
			}
			return ShowErrorMessage(Resources.UnhandledExceptionMessage);
		}

		[ValidateInput(false)]
		public ActionResult UpdateTableObjectRoles(Dictionary<int, bool> settings)
		{
			try
			{
				if (ViewboxSession.RightsMode && ViewboxSession.RightsModeCredential.Type == CredentialType.Role)
				{
					IRole role = ViewboxApplication.Database.SystemDb.Roles.FirstOrDefault((IRole r) => r.Id == ViewboxSession.RightsModeCredential.Id);
					if (role != null)
					{
						ChangeRoleRight(role.Id);
						UpdateTableObjectRolesTask t = new UpdateTableObjectRolesTask(role, settings);
						t.StartJob();
						return PartialView("_DialogPartial", new DialogModel
						{
							DialogType = DialogModel.Type.Info,
							Title = Resources.RightsChange,
							Key = t.Key,
							Content = string.Format(Resources.LongRunningDialogText, Resources.RightsChange),
							Buttons = new List<DialogModel.Button>
							{
								new DialogModel.Button
								{
									Caption = Resources.LongRunningDialogCaption
								}
							}
						});
					}
				}
			}
			catch (Exception ex)
			{
				LogHelper.GetLogger().ErrorWithCheck(ex.Message, ex);
			}
			return ShowErrorMessage(Resources.UnhandledExceptionMessage);
		}

		[ValidateInput(false)]
		public ActionResult UpdateOptimizationRoles(string settings)
		{
			try
			{
				if (ViewboxSession.RightsMode && ViewboxSession.RightsModeCredential.Type == CredentialType.Role)
				{
					IRole role = ViewboxApplication.Database.SystemDb.Roles.FirstOrDefault((IRole r) => r.Id == ViewboxSession.RightsModeCredential.Id);
					if (role != null)
					{
						ChangeRoleRight(role.Id);
						List<KeyValuePair<int, bool>> helper = new List<KeyValuePair<int, bool>>();//JsonConvert.DeserializeObject<List<KeyValuePair<int, bool>>>(settings);
						Dictionary<int, bool> modifiedSettings = helper.ToDictionary((KeyValuePair<int, bool> h) => h.Key, (KeyValuePair<int, bool> h) => h.Value);
						UpdateOptimizationRolesTask t = new UpdateOptimizationRolesTask(role, modifiedSettings);
						t.StartJob();
						return PartialView("_DialogPartial", new DialogModel
						{
							DialogType = DialogModel.Type.Info,
							Title = Resources.RightsChange,
							Key = t.Key,
							Content = string.Format(Resources.LongRunningDialogText, Resources.RightsChange),
							Buttons = new List<DialogModel.Button>
							{
								new DialogModel.Button
								{
									Caption = Resources.LongRunningDialogCaption
								}
							}
						});
					}
				}
			}
			catch (Exception ex)
			{
				LogHelper.GetLogger().ErrorWithCheck(ex.Message, ex);
			}
			return ShowErrorMessage(Resources.UnhandledExceptionMessage);
		}

		private void ChangeRoleRight(int id)
		{
			if (ViewboxApplication.Roles[id] == null || ViewboxApplication.Roles[id].Users == null || !ViewboxApplication.Roles[id].Users.Any())
			{
				return;
			}
			foreach (IUser user in ViewboxApplication.Roles[id].Users)
			{
				ChangeUserRight(user.Id);
			}
		}

		private void ChangeUserRight(int userId)
		{
			if (ViewboxApplication.Users[userId] != null)
			{
				ViewboxApplication.Users[userId].UserRightHelper = new RightHelper
				{
					ChangeRight = ChangeRightType.Role,
					ModifyUser = ViewboxSession.User
				};
			}
		}
	}
}
