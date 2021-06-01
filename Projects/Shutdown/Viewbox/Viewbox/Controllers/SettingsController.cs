using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using SystemDb;
using SystemDb.Internal;
using DbAccess;
using Viewbox.Job;
using Viewbox.Models;
using Viewbox.Properties;

namespace Viewbox.Controllers
{
	[Authorize]
	[ViewboxFilter]
	public class SettingsController : BaseController
	{
		public ActionResult Index()
		{
			ViewboxSession.AllowedOpts = null;
			ViewboxSession.IssueOptimizationFilter.Clear();
			ViewboxSession.ClearSavedDocumentSettings("Settings");
			return View(new PropertySettingsModel
			{
				Properties = ViewboxApplication.GetProperties()
			});
		}

		[HttpPost]
		public ActionResult Index(IEnumerable<PropertySettingsModel.IdValuePair> properties)
		{
			ViewboxSession.AllowedOpts = null;
			ViewboxSession.IssueOptimizationFilter.Clear();
			IPropertiesCollection props = ViewboxApplication.GetProperties();
			PropertySettingsModel model = new PropertySettingsModel();
			if (properties != null)
			{
				foreach (PropertySettingsModel.IdValuePair pair in properties)
				{
					if (props[pair.Id] == null)
					{
						continue;
					}
					if (props[pair.Id].Key == "defaultPDFfontSize")
					{
						int.TryParse(pair.Value, out var tmp);
						if (tmp == 0)
						{
							pair.Value = "12";
						}
						else if (tmp < 5)
						{
							pair.Value = "5";
						}
						else if (tmp > 20)
						{
							pair.Value = "20";
						}
					}
					props[pair.Id].Value = pair.Value;
					ViewboxApplication.Database.SystemDb.UpdateProperty(ViewboxSession.User, props[pair.Id]);
				}
			}
			model.Properties = props;
			return View("Index", model);
		}

		public ActionResult Personal()
		{
			return View("Index", new UserSettingsModel
			{
				Properties = new List<IProperty>(from p in ViewboxApplication.GetProperties()
					where p.Key.StartsWith("user.")
					select p)
			});
		}

		[HttpPost]
		public ActionResult Personal(string userName, string name, string email, string oldpassword, string newpassword, string newpassword2, int displayrowcount = 0)
		{
			UserSettingsModel model = new UserSettingsModel
			{
				Properties = new List<IProperty>(from p in ViewboxApplication.GetProperties()
					where p.Key.StartsWith("user.")
					select p)
			};
			IUser user = ((ViewboxSession.RightsMode && ViewboxSession.RightsModeCredential.Type == CredentialType.User) ? ViewboxApplication.Users[ViewboxSession.RightsModeCredential.Id] : ViewboxSession.User);
			if (!(user.UserName == "avendata_admin") && !(user.UserName == "avendata_qs"))
			{
				if (!string.IsNullOrWhiteSpace(oldpassword) || !string.IsNullOrWhiteSpace(newpassword) || !string.IsNullOrWhiteSpace(newpassword2))
				{
					if (user == ViewboxSession.User)
					{
						if (!user.CheckPassword(oldpassword))
						{
							return OldPasswordNotValid();
						}
						if (newpassword != newpassword2)
						{
							return ApprovalPassword();
						}
						if (string.IsNullOrWhiteSpace(newpassword) || !Regex.IsMatch(newpassword, ViewboxApplication.PasswordRegex))
						{
							return InvalidPassword();
						}
					}
					if ((!ViewboxSession.RightsMode || ViewboxSession.RightsModeCredential.Type != CredentialType.User) && !(user.Password != newpassword))
					{
						return SamePassword();
					}
					user.Password = newpassword;
					model.Dialog = new DialogModel
					{
						Title = Resources.PasswordModification,
						Content = Resources.PasswordModificationSucceeded,
						Class = "user-name"
					};
					model.Dialog.Buttons = new List<DialogModel.Button>
					{
						new DialogModel.Button
						{
							Caption = "OK",
							Data = ""
						}
					};
				}
				user.Name = name;
				user.Email = email;
				if (!string.IsNullOrWhiteSpace(userName) && userName != user.UserName)
				{
					if (ViewboxApplication.ByUserName(userName) != null)
					{
						return RedirectToAction("AlreadyExisting", "Settings", new
						{
							changedValue = true
						});
					}
					user.UserName = userName;
					HttpCookie cookie = base.Request.Cookies.Get(FormsAuthentication.FormsCookieName);
					FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
					FormsAuthentication.SignOut();
					base.Request.Cookies.Remove(FormsAuthentication.FormsCookieName);
					FormsAuthentication.SetAuthCookie(user.UserName, ticket.IsPersistent);
					ViewboxSession.SaveUser(user);
					return RedirectToAction("Personal");
				}
			}
			if (displayrowcount > 100)
			{
				displayrowcount = 100;
			}
			user.DisplayRowCount = ((displayrowcount == 0) ? ViewboxSession.User.DisplayRowCount : displayrowcount);
			ViewboxSession.SaveUser(user);
			return View("Index", model);
		}

		public ActionResult SamePassword()
		{
			UserSettingsModel model = new UserSettingsModel
			{
				Properties = new List<IProperty>(from p in ViewboxApplication.GetProperties()
					where p.Key.StartsWith("user.")
					select p)
			};
			model.Dialog = new DialogModel
			{
				Title = Resources.InvalidPassword,
				Content = Resources.SamePasswordError,
				Class = "user-name"
			};
			model.Dialog.Buttons = new List<DialogModel.Button>
			{
				new DialogModel.Button
				{
					Caption = "OK",
					Data = ""
				}
			};
			return View("Index", model);
		}

		public ActionResult InvalidPassword()
		{
			UserSettingsModel model = new UserSettingsModel
			{
				Properties = new List<IProperty>(from p in ViewboxApplication.GetProperties()
					where p.Key.StartsWith("user.")
					select p)
			};
			model.Dialog = new DialogModel
			{
				Title = Resources.InvalidPassword,
				Content = ViewboxApplication.PasswordPolicyDescription,
				Class = "user-name"
			};
			model.Dialog.Buttons = new List<DialogModel.Button>
			{
				new DialogModel.Button
				{
					Caption = "OK",
					Data = ""
				}
			};
			return View("Index", model);
		}

		public ActionResult ApprovalPassword()
		{
			UserSettingsModel model = new UserSettingsModel
			{
				Properties = new List<IProperty>(from p in ViewboxApplication.GetProperties()
					where p.Key.StartsWith("user.")
					select p)
			};
			model.Dialog = new DialogModel
			{
				Title = Resources.InvalidPassword,
				Content = Resources.PasswordDoesNotMatch,
				Class = "user-name"
			};
			model.Dialog.Buttons = new List<DialogModel.Button>
			{
				new DialogModel.Button
				{
					Caption = "OK",
					Data = ""
				}
			};
			return View("Index", model);
		}

		public ActionResult OldPasswordNotValid()
		{
			UserSettingsModel model = new UserSettingsModel
			{
				Properties = new List<IProperty>(from p in ViewboxApplication.GetProperties()
					where p.Key.StartsWith("user.")
					select p)
			};
			model.Dialog = new DialogModel
			{
				Title = Resources.InsertOldPassword,
				Content = Resources.WrongPassword,
				Class = "user-name"
			};
			model.Dialog.Buttons = new List<DialogModel.Button>
			{
				new DialogModel.Button
				{
					Caption = "OK",
					Data = ""
				}
			};
			return View("Index", model);
		}

		public ActionResult LogRead()
		{
			LogReadModel model = new LogReadModel();
			return View("Index", model);
		}

		public ActionResult Rights(bool json = false)
		{
			RightSettingsModel model = new RightSettingsModel();
			if (json)
			{
				return PartialView("_RightsPartial", model);
			}
			return View("Index", model);
		}

		public ActionResult AdminTasks()
		{
			AdminTasksModel model = new AdminTasksModel();
			model.GeneratedTable = ViewboxApplication.Database.SystemDb.IsEmptyDistinctGenerated();
			model.AreIndexesPopulated = ViewboxApplication.Database.SystemDb.AreIndexesPopulated();
			model.ExtendedColumnsInformationGenerated = ViewboxApplication.Database.SystemDb.IsExtendedGenerated();
			return View("Index", model);
		}

		public ActionResult CopyArchiveFiles()
		{
			CreateArchiveFilesJob job = CreateArchiveFilesJob.Create(ViewboxSession.SelectedSystem);
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Title = "Copy archive files that are in the table.",
				Key = job.Key,
				Content = string.Format(Resources.VeryLongRunningDialogText, "Copy archive files that are in the table."),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.LongRunningDialogCaption
					}
				}
			});
		}

		public ActionResult CheckArchiveFiles()
		{
			CreateArchiveFilesJob job = CreateArchiveFilesJob.Create(0, ViewboxSession.SelectedSystem);
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Title = "Check which files are missing.",
				Key = job.Key,
				Content = string.Format(Resources.VeryLongRunningDialogText, "The missing file names will be written in the log file."),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.LongRunningDialogCaption
					}
				}
			});
		}

		public ActionResult CheckAndCreateThumbnailFiles()
		{
			CreateArchiveFilesJob job = CreateArchiveFilesJob.Create(0, 0);
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Title = "Check which thumbnails are missing.",
				Key = job.Key,
				Content = string.Format(Resources.VeryLongRunningDialogText, "The missing thumbnail files will be generated."),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.LongRunningDialogCaption
					}
				}
			});
		}

		public ActionResult RenameThumbnailFiles()
		{
			CreateArchiveFilesJob job = CreateArchiveFilesJob.Create(0, 0, ViewboxSession.SelectedSystem);
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Title = "Rename thumbnails are missing.",
				Key = job.Key,
				Content = string.Format(Resources.VeryLongRunningDialogText, "Renaming thumbnail files."),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.LongRunningDialogCaption
					}
				}
			});
		}

		public PartialViewResult InvalidValue(CredentialType type = CredentialType.User, string userName = "", string password = "", string repeatPassword = "", string name = "", string email = "", bool wrongEmail = false, string message = null)
		{
			RightSettingsModel model = new RightSettingsModel();
			model.Type = type;
			switch (type)
			{
			case CredentialType.User:
			{
				List<string> invalidValues = new List<string>();
				if (string.IsNullOrEmpty(userName))
				{
					invalidValues.Add(Resources.Username);
				}
				if (string.IsNullOrEmpty(password) || password != repeatPassword)
				{
					invalidValues.Add(Resources.Password);
				}
				if (wrongEmail)
				{
					invalidValues.Add(Resources.EMail);
				}
				model.Dialog = new DialogModel
				{
					Title = ((invalidValues.Count > 1) ? Resources.InvalidValues : Resources.InvalidValue),
					Content = string.Format((invalidValues.Count > 1) ? Resources.InvalidValuesText : Resources.InvalidValueText, string.Join(", ", invalidValues)),
					InputMessages = new Dictionary<string, string> { 
					{
						"password",
						(!string.IsNullOrEmpty(message)) ? ("<span style='color:yellow;font-weight: bold;display:inline-block;text-align:left;'>" + message + "</span>") : ""
					} },
					Inputs = new List<Tuple<string, string, string, string, int, int>>
					{
						new Tuple<string, string, string, string, int, int>(Resources.Username, "text", "userName", userName, 256, 0),
						new Tuple<string, string, string, string, int, int>(Resources.Password, "password", "password", (password != repeatPassword) ? "" : password, 256, 0),
						new Tuple<string, string, string, string, int, int>(Resources.RepeatPassword, "password", "repeatPassword", (string.IsNullOrEmpty(password) || password != repeatPassword) ? "" : repeatPassword, 256, 0),
						new Tuple<string, string, string, string, int, int>(Resources.Name + " (" + Resources.Optional + ")", "text", "name", name, 256, 0),
						new Tuple<string, string, string, string, int, int>(Resources.EMail + " (" + Resources.Optional + ")", "text", "email", email, 256, 0)
					},
					Select = new List<Tuple<string, string, List<string>, List<string>, int>>
					{
						new Tuple<string, string, List<string>, List<string>, int>(Resources.SpecialRights, "specialRights", UserManagementController.UserOptionsLang, UserManagementController.UserOptions, 0)
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
				};
				break;
			}
			case CredentialType.Role:
				model.Dialog = new DialogModel
				{
					Title = Resources.InvalidValue,
					Content = string.Format("Bitte geben Sie {0} ein.", "einen gültigen Namen"),
					InputName = "name",
					InputLength = 24,
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
				};
				break;
			default:
				model.Dialog = new DialogModel
				{
					Title = Resources.InvalidValue,
					Content = string.Format("Bitte geben Sie {0} ein.", (type == CredentialType.User) ? "eine gültige Emailadresse" : "einen gültigen Namen"),
					InputName = ((type == CredentialType.User) ? "email" : "name"),
					InputLength = ((type == CredentialType.User) ? 64 : 24),
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
				};
				break;
			}
			return PartialView("_RightsPartial", model);
		}

		public ActionResult AlreadyExisting(CredentialType type = CredentialType.User, string password = "", string repeatPassword = "", string name = "", string email = "", bool changedValue = false)
		{
			if (type == CredentialType.User)
			{
				if (changedValue)
				{
					UserSettingsModel model3 = new UserSettingsModel
					{
						Properties = new List<IProperty>(from p in ViewboxApplication.GetProperties()
							where p.Key.StartsWith("user.")
							select p)
					};
					model3.Dialog = new DialogModel
					{
						Title = string.Format(Resources.AlreadyExisting, Resources.Username),
						Content = string.Format(Resources.AlreadyExistingText, Resources.Username),
						Buttons = new List<DialogModel.Button>
						{
							new DialogModel.Button
							{
								Caption = Resources.OK
							}
						}
					};
					return View("Index", model3);
				}
				RightSettingsModel model2 = new RightSettingsModel();
				model2.Type = type;
				model2.Dialog = new DialogModel
				{
					Title = string.Format(Resources.AlreadyExisting, Resources.Username),
					Content = string.Format(Resources.AlreadyExistingText, Resources.Username),
					Inputs = new List<Tuple<string, string, string, string, int, int>>
					{
						new Tuple<string, string, string, string, int, int>(Resources.Username, "text", "userName", "", 256, 0),
						new Tuple<string, string, string, string, int, int>(Resources.Password, "password", "password", password, 256, 0),
						new Tuple<string, string, string, string, int, int>(Resources.RepeatPassword, "password", "repeatPassword", repeatPassword, 256, 0),
						new Tuple<string, string, string, string, int, int>(Resources.Name + " (" + Resources.Optional + ")", "text", "name", name, 256, 0),
						new Tuple<string, string, string, string, int, int>(Resources.EMail + " (" + Resources.Optional + ")", "text", "email", email, 256, 0)
					},
					Select = new List<Tuple<string, string, List<string>, List<string>, int>>
					{
						new Tuple<string, string, List<string>, List<string>, int>(Resources.SpecialRights, "specialRights", UserManagementController.UserOptionsLang, UserManagementController.UserOptions, 0)
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
				};
				return PartialView("_RightsPartial", model2);
			}
			RightSettingsModel model = new RightSettingsModel();
			model.Type = type;
			model.Dialog = UserManagementController.CreateRoleDialogModel(string.Format(Resources.RoleAlreadyExisting, name), Resources.RoleAlreadyExistingText, DialogModel.Type.Warning);
			return PartialView("_RightsPartial", model);
		}

		public ActionResult HideEveryUnreachableTables()
		{
			Transformation trans = Transformation.Create(ViewboxApplication.Database.SystemDb.Objects, ViewboxApplication.Database.SystemDb.Issues);
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Info,
				Title = Resources.HideEveryUnreachableTables,
				Key = trans.Key,
				Content = string.Format(Resources.VeryLongRunningDialogText, Resources.HideEveryUnreachableTables),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.LongRunningDialogCaption
					}
				}
			});
		}

		public ActionResult UpdateMode(int id)
		{
			using (DatabaseBase connection = ViewboxApplication.Database.ConnectionManager.GetConnection())
			{
				if (ViewboxSession.updMode != id)
				{
					Info.SetValue("UpdateMode", id.ToString(), connection);
				}
			}
			ViewboxSession.updMode = id;
			return RedirectToAction("AdminTasks");
		}

		public string GetValueFromInfo(string key)
		{
			using DatabaseBase connection = ViewboxApplication.Database.ConnectionManager.GetConnection();
			return Info.GetValue("UpdateMode", connection);
		}

		public ActionResult Information()
		{
			InformationModel model = new InformationModel();
			return View("Index", model);
		}

		public JsonResult BenchMark()
		{
			long time = ControlCenterModel.RunBenchmark(1000000000L);
			double seconds = (double)time / 1000.0;
			return Json(seconds, JsonRequestBehavior.AllowGet);
		}

		public ActionResult TableAndColumnSettings()
		{
			TableAndColumnSettingsModel model = TableAndColumnSettingsModel.Instance;
			return View("Index", model);
		}

		[HttpGet]
		public JsonResult GetColumns(int tableId)
		{
			TableAndColumnSettingsModel model = TableAndColumnSettingsModel.Instance;
			var columns = from c in model.GetTableColumns(tableId)
				select new
				{
					Id = c.Id,
					Description = c.GetDescription()
				};
			return Json(columns.ToArray(), JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public ActionResult UpdateColumn(int columnId, string newName)
		{
			TableAndColumnSettingsModel model = TableAndColumnSettingsModel.Instance;
			if (model.UpdateColumn(columnId, newName))
			{
				return Json(true, JsonRequestBehavior.AllowGet);
			}
			return Json(false, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public ActionResult UpdateTable(int tableId, string newName)
		{
			TableAndColumnSettingsModel model = TableAndColumnSettingsModel.Instance;
			if (model.UpdateTable(tableId, newName))
			{
				return Json(true, JsonRequestBehavior.AllowGet);
			}
			return Json(false, JsonRequestBehavior.AllowGet);
		}

		public ActionResult RolesConfig()
		{
			if (ViewboxSession.RightsMode)
			{
				RoleSettingsModel model = new RoleSettingsModel();
				return View("Index", model);
			}
			return Rights();
		}

		public PartialViewResult LoadOptimizationChildren(int id)
		{
			IOptimization opt = ViewboxApplication.Database.SystemDb.Optimizations.SingleOrDefault((IOptimization o) => o.Id == id);
			return PartialView("_OptimizationChildrenPartial", opt);
		}

		public ActionResult RoleTree()
		{
			if (ViewboxSession.RightsMode)
			{
				RoleTreeModel model = new RoleTreeModel();
				return View("Index", model);
			}
			return Rights();
		}

		public ActionResult ModifyStartScreen()
		{
			ModifyStartScreenModel model = new ModifyStartScreenModel();
			return View("Index", model);
		}

		public ActionResult DeletePictures(IEnumerable<string> pictures)
		{
			ModifyStartScreenModel model = new ModifyStartScreenModel();
			if (pictures != null)
			{
				try
				{
					foreach (string pic in pictures)
					{
						ViewboxApplication.Database.SystemDb.StartScreens.Remove(ViewboxApplication.Database.SystemDb.StartScreens.First((StartScreen s) => s.Name == pic));
						using DatabaseBase conn = ViewboxApplication.Database.SystemDb.ConnectionManager.GetConnection();
						conn.ExecuteNonQuery("DELETE FROM `" + conn.DbConfig.DbName + "`.`startscreens` WHERE name ='" + pic + "';");
					}
				}
				catch
				{
					model.Dialog = new DialogModel
					{
						DialogType = DialogModel.Type.Warning,
						Title = Resources.Error,
						Content = Resources.ErrorPictureDelete,
						Buttons = new List<DialogModel.Button>
						{
							new DialogModel.Button
							{
								Caption = Resources.OK
							}
						}
					};
				}
				model.UpdatePicsList();
			}
			return View("Index", model);
		}

		[HttpPost]
		public ActionResult UploadPicture()
		{
			bool notImg = false;
			ModifyStartScreenModel model = new ModifyStartScreenModel();
			if (base.Request.Files.Count > 0)
			{
				try
				{
					HttpPostedFileBase file = base.Request.Files["file"];
					if (file != null && file.ContentLength > 0)
					{
						try
						{
							Image img = Image.FromStream(file.InputStream, useEmbeddedColorManagement: true);
							if (!file.FileName.ToLower().Contains(".png"))
							{
								throw new FileLoadException();
							}
							if (file.ContentLength >= 5242880)
							{
								throw new IOException();
							}
						}
						catch (FileLoadException)
						{
							notImg = true;
							model.Dialog = new DialogModel
							{
								DialogType = DialogModel.Type.Warning,
								Title = Resources.Error,
								Content = Resources.NotImageFile,
								Buttons = new List<DialogModel.Button>
								{
									new DialogModel.Button
									{
										Caption = Resources.OK
									}
								}
							};
						}
						catch (IOException)
						{
							notImg = true;
							model.Dialog = new DialogModel
							{
								DialogType = DialogModel.Type.Warning,
								Title = Resources.Error,
								Content = Resources.ImageFileSizeError,
								Buttons = new List<DialogModel.Button>
								{
									new DialogModel.Button
									{
										Caption = Resources.OK
									}
								}
							};
						}
						if (!notImg)
						{
							using DatabaseBase conn = ViewboxApplication.Database.SystemDb.ConnectionManager.GetConnection();
							using MemoryStream mS = new MemoryStream();
							string[] tmpFileNameList = file.FileName.Split('\\');
							Image img = Image.FromStream(file.InputStream, useEmbeddedColorManagement: true);
							img.Save(mS, ImageFormat.Png);
							string base64 = Convert.ToBase64String(mS.ToArray());
							ViewboxApplication.Database.SystemDb.StartScreens.Add(new StartScreen
							{
								Id = 0,
								Name = tmpFileNameList.Last(),
								ImgBase64 = base64,
								IsDefault = true
							});
							conn.ExecuteNonQuery("INSERT INTO `" + conn.DbConfig.DbName + "`.`startscreens` (`name`,`value`,`default`) VALUE ('" + tmpFileNameList.Last() + "','" + base64 + "','1');");
							conn.ExecuteNonQuery("UPDATE `" + conn.DbConfig.DbName + "`.`startscreens` SET `default` = 0 WHERE `name` != '" + tmpFileNameList.Last() + "';");
						}
					}
				}
				catch (Exception)
				{
					model.Dialog = new DialogModel
					{
						DialogType = DialogModel.Type.Warning,
						Title = Resources.Error,
						Content = Resources.ErrorPictureUpload,
						Buttons = new List<DialogModel.Button>
						{
							new DialogModel.Button
							{
								Caption = Resources.OK
							}
						}
					};
				}
				model.UpdatePicsList();
			}
			return View("Index", model);
		}

		[HttpGet]
		public ActionResult SetDeafultPicture(string pic)
		{
			ModifyStartScreenModel model = new ModifyStartScreenModel();
			if (ViewboxApplication.Database.SystemDb.StartScreens.Count > 0)
			{
				IStartScreen defaultPic = ViewboxApplication.Database.SystemDb.StartScreens.FirstOrDefault((StartScreen p) => p.IsDefault);
				IStartScreen toBeDefPic = ViewboxApplication.Database.SystemDb.StartScreens.FirstOrDefault((StartScreen p) => p.Name == pic);
				using DatabaseBase conn = ViewboxApplication.Database.SystemDb.ConnectionManager.GetConnection();
				conn.ExecuteNonQuery("UPDATE `" + conn.DbConfig.DbName + "`.`startscreens` SET `default`='1' WHERE `id`=" + toBeDefPic.Id + ";");
				if (defaultPic != null)
				{
					conn.ExecuteNonQuery("UPDATE `" + conn.DbConfig.DbName + "`.`startscreens` SET `default`='0' WHERE `id`=" + defaultPic.Id + ";");
				}
			}
			model.UpdatePicsList();
			return View("Index", model);
		}
	}
}
