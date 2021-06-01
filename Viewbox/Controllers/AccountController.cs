using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using SystemDb;
using Viewbox.Exceptions;
using Viewbox.Models;
using Viewbox.Properties;

namespace Viewbox.Controllers
{
	[ViewboxFilter]
	public class AccountController : BaseController
	{
		public IFormsAuthenticationService FormsService { get; set; }

		public IMembershipService MembershipService { get; set; }

		public override void BeforeInitialize(RequestContext requestContext)
		{
			if (FormsService == null)
			{
				FormsService = new FormsAuthenticationService();
			}
			if (MembershipService == null)
			{
				MembershipService = new AccountMembershipService();
			}
			base.BeforeInitialize(requestContext);
		}

		public ActionResult LogOn(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				LogOnModel model = new LogOnModel();
				return View(model);
			}
			IUser user = ViewboxApplication.GetInvitedUser(key);
			if (user != null)
			{
				if (HttpContextFactory.Current.Session["UnitTesting"] == null || !(bool)HttpContextFactory.Current.Session["UnitTesting"])
				{
					FormsAuthentication.SignOut();
					base.Request.Cookies.Remove(FormsAuthentication.FormsCookieName);
				}
				LogOnModel model2 = new LogOnModel
				{
					UserName = user.Name,
					Dialog = GetChangePasswordDialogModel(user.UserName, key)
				};
				return View(model2);
			}
			return RedirectToAction("LogOn");
		}

		[HttpPost]
		public bool SaveNewPassword(string newPassword, string key, string userName = "", string password = "")
		{
			if (string.IsNullOrEmpty(key))
			{
				if (ViewboxSession.User != null)
				{
					return new PasswordModel(ViewboxSession.User).TrySaveNewPassword(newPassword);
				}
				if (MembershipService.ValidateUser(userName, password))
				{
					return new PasswordModel(ViewboxApplication.ByUserName(userName)).TrySaveNewPassword(newPassword);
				}
				return false;
			}
			return new PasswordModel(ViewboxApplication.GetInvitedUser(key)).TrySaveNewPassword(newPassword);
		}

		[JsonExceptionHandler]
		public bool ValidateNewPassword(string userName, string newPassword, string confirmNewPassword = null)
		{
			if (!newPassword.Equals(confirmNewPassword))
			{
				throw new BadPasswordException(Resources.PasswordDoesNotMatch);
			}
			try
			{
				if (ViewboxApplication.ByUserName(userName).Password.Equals(confirmNewPassword))
				{
					throw new BadPasswordException(Resources.PasswordNewNotEqualOld);
				}
			}
			catch (InvalidOperationException)
			{
			}
			if (!PasswordModel.ValidatePasswordPattern(newPassword, ViewboxApplication.ByUserName(userName)))
			{
				throw new BadPasswordException(ViewboxApplication.PasswordPolicyDescription);
			}
			return MembershipService.ValidateNewPassword(userName, newPassword);
		}

		public bool HasPasswordExpired(string userName)
		{
			return ViewboxApplication.ByUserName(userName).HasPasswordExpired();
		}

		public bool IsFirstLogin(string userName)
		{
			return ViewboxApplication.ByUserName(userName).FirstLogin;
		}

		[HttpPost]
		public bool LogUser(LogOnModel model, string returnUrl)
		{
			if (base.ModelState.IsValid && MembershipService.ValidateUser(model.UserName, model.Password))
			{
				FormsService.SignIn(model.UserName, model.RememberMe);
				return true;
			}
			return false;
		}

		private DialogModel GetNewPasswordDialogModel(string userName, string passWord)
		{
			return new DialogModel
			{
				DialogTemplate = "_ExtendedDialogPartial",
				Title = Resources.CreatePassword,
				Content = $"{Resources.Password_Expired_Choose_New}<br/><br/>",
				Inputs = new List<Tuple<string, string, string, string, int, int>>
				{
					new Tuple<string, string, string, string, int, int>(Resources.CreatePassword, "password", "newPassword", "", 30, 0),
					new Tuple<string, string, string, string, int, int>(Resources.Confirm_Password, "password", "confirmNewPassword", "", 30, 0)
				},
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.Cancel
					},
					new DialogModel.Button
					{
						Data = "NewPassword",
						Caption = Resources.Submit
					}
				},
				HiddenFields = new List<Tuple<string, string, string>>
				{
					new Tuple<string, string, string>("userName", userName, null),
					new Tuple<string, string, string>("passWord", passWord, null)
				}
			};
		}

		private DialogModel FirstLogin(string userName, string oldPassword)
		{
			return new DialogModel
			{
				DialogTemplate = "_ExtendedDialogPartial",
				Title = Resources.PasswordModification,
				Content = $"{Resources.PasswordChangeFirst} ",
				Inputs = new List<Tuple<string, string, string, string, int, int>>
				{
					new Tuple<string, string, string, string, int, int>(Resources.Password, "password", "newPassword", "", 30, 0),
					new Tuple<string, string, string, string, int, int>(Resources.Confirm_Password, "password", "confirmNewPassword", "", 30, 0)
				},
				HiddenFields = new List<Tuple<string, string, string>>
				{
					new Tuple<string, string, string>("userName", userName, null),
					new Tuple<string, string, string>("passWord", oldPassword, null)
				},
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.Cancel
					},
					new DialogModel.Button
					{
						Data = "NewPassword",
						Caption = Resources.Submit
					}
				}
			};
		}

		private DialogModel GetChangePasswordDialogModel(string userName, string key)
		{
			return new DialogModel
			{
				DialogTemplate = "_ExtendedDialogPartial",
				Title = Resources.PasswordModification,
				Content = $"{Resources.ChangePasswordSubject}<br/><br/><br/>",
				Inputs = new List<Tuple<string, string, string, string, int, int>>
				{
					new Tuple<string, string, string, string, int, int>(Resources.Password, "password", "newPassword", "", 30, 0),
					new Tuple<string, string, string, string, int, int>(Resources.Confirm_Password, "password", "confirmNewPassword", "", 30, 0)
				},
				HiddenFields = new List<Tuple<string, string, string>>
				{
					new Tuple<string, string, string>("userName", userName, null),
					new Tuple<string, string, string>("key", key, null)
				},
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.Cancel
					},
					new DialogModel.Button
					{
						Data = "NewPassword",
						Caption = Resources.Submit
					}
				}
			};
		}

		[HttpPost]
		public ActionResult ForceLogOn(LogOnModel model, string returnUrl)
		{
			if (base.ModelState.IsValid && MembershipService.ValidateUser(model.UserName, model.Password))
			{
				FormsService.SignIn(model.UserName, model.RememberMe);
				ViewboxApplication.ByUserName(model.UserName).SessionId = ViewboxSession.Key;
				ViewboxApplication.ByUserName(model.UserName).CurrentLanguage = ViewboxSession.Language.CountryCode;
				if (ViewboxSession.LastPositionUserViewList != null && ViewboxSession.LastPositionUserViewList.ContainsKey(ViewboxSession.Key))
				{
					ViewboxSession.LastPositionUserViewList.Remove(ViewboxSession.Key);
				}
				if (ViewboxSession.LastPositionUserTableList != null && ViewboxSession.LastPositionUserTableList.ContainsKey(ViewboxSession.Key))
				{
					ViewboxSession.LastPositionUserTableList.Remove(ViewboxSession.Key);
				}
			}
			return RedirectToAction("Index", "Home");
		}

		[HttpPost]
		public ActionResult LogOn(LogOnModel model, string returnUrl)
		{
			if (base.ModelState.IsValid)
			{
				bool canAccess = true;
				if (int.TryParse(ViewboxSession.GetValueFromInfo("UpdateMode"), out var um))
				{
					ViewboxSession.updMode = um;
				}
				else
				{
					ViewboxSession.updMode = null;
				}
				if (ViewboxSession.updMode == 1)
				{
					if (!(model.UserName == "avendata_admin") && !(model.UserName == "avendata_qs"))
					{
						canAccess = false;
						return RedirectToAction("UpdateMode", "Error");
					}
					canAccess = true;
				}
				if (canAccess)
				{
					if (MembershipService.ValidateUser(model.UserName, model.Password))
					{
						if (!string.IsNullOrEmpty(ViewboxApplication.ByUserName(model.UserName).SessionId) && ViewboxApplication.ByUserName(model.UserName).SessionId != ViewboxSession.Key)
						{
							return View("/Views/Error/UserAlreadyLoggedIn.cshtml", new UserAlreadyLoggedInModel());
						}
						if (model.UserName != "avendata_admin" && model.UserName != "avendata_qs" && HasPasswordExpired(model.UserName))
						{
							model.Dialog = GetNewPasswordDialogModel(model.UserName, model.Password);
							return View(model);
						}
						if (IsFirstLogin(model.UserName))
						{
							model.Dialog = FirstLogin(model.UserName, model.Password);
							return View(model);
						}
						FormsService.SignIn(model.UserName, model.RememberMe);
						ViewboxApplication.ByUserName(model.UserName).CurrentLanguage = ViewboxSession.Language.CountryCode;
						if (HttpContextFactory.Current.Session["UnitTesting"] == null || !(bool)HttpContextFactory.Current.Session["UnitTesting"])
						{
							if (base.Url.IsLocalUrl(returnUrl))
							{
								return Redirect(returnUrl);
							}
							return RedirectToAction("Index", "Home");
						}
						return RedirectToAction("Index", "Home");
					}
					model.Dialog = new DialogModel
					{
						Title = Resources.WrongPassword,
						Class = "user-name",
						Buttons = new List<DialogModel.Button>
						{
							new DialogModel.Button
							{
								Caption = Resources.OK,
								Data = false.ToString()
							}
						}
					};
				}
			}
			return View(model);
		}

		public ActionResult LogOff(string usercontroller)
		{
			try
			{
				if (ViewboxSession.User != null && ViewboxSession.LastPositionUserTableList != null && ViewboxSession.LastPositionUserViewList != null)
				{
					if (ViewboxSession.LastPositionUserViewList.ContainsKey(ViewboxSession.User.SessionId))
					{
						ViewboxSession.LastPositionUserViewList.Remove(ViewboxSession.User.SessionId);
					}
					if (ViewboxSession.LastPositionUserTableList.ContainsKey(ViewboxSession.User.SessionId))
					{
						ViewboxSession.LastPositionUserTableList.Remove(ViewboxSession.User.SessionId);
					}
				}
				FormsService.SignOut();
				if (ViewboxSession.User != null && ViewboxSession.User.SessionId == ViewboxSession.Key)
				{
					ViewboxSession.User.SessionId = null;
					ViewboxApplication.Database.SystemDb.UserLastIssueSettingsCollection.Remove(ViewboxApplication.Database.SystemDb.UserLastIssueSettingsCollection[ViewboxSession.User]);
					ViewboxSession.UserInitialized = false;
				}
				if (HttpContextFactory.Current.Session["UnitTesting"] == null || !(bool)HttpContextFactory.Current.Session["UnitTesting"])
				{
					base.Request.Cookies.Remove(FormsAuthentication.FormsCookieName);
				}
				ViewboxSession.ClearSavedDocumentSettings("DataGrid");
				ViewboxSession.Logoff();
			}
			catch
			{
			}
			return RedirectToAction("Index", "Home");
		}

		[ActionName("BackgroundLogOff")]
		[AcceptVerbs(HttpVerbs.Get)]
		public void LogOff()
		{
			FormsService.SignOut();
			base.Request.Cookies.Remove(FormsAuthentication.FormsCookieName);
			ViewboxSession.Logoff();
		}

		public ActionResult ForgotPassword(string email, string url)
		{
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = DialogModel.Type.Mail,
				Title = Resources.ForgotPassword,
				Content = Resources.GiveUsernameDetails,
				InputName = "userName",
				InputValue = null,
				HiddenFields = new List<Tuple<string, string, string>>
				{
					new Tuple<string, string, string>("url", url, "")
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
						Caption = Resources.Send,
						Data = true.ToString()
					}
				}
			});
		}

		[HttpPost]
		public ActionResult SendInvitation(string userName, string url)
		{
			bool isUserSpecial = userName == "avendata_admin" || userName == "avendata_qs";
			bool hasEmailAddress = false;
			bool doesUserExist = false;
			if (!isUserSpecial)
			{
				IUser user = ViewboxApplication.ByUserName(userName);
				if (user != null)
				{
					doesUserExist = true;
					hasEmailAddress = !string.IsNullOrEmpty(user.Email);
					if (hasEmailAddress)
					{
						ViewboxApplication.SendInvitation(user, url);
					}
				}
			}
			return PartialView("_DialogPartial", new DialogModel
			{
				DialogType = (hasEmailAddress ? DialogModel.Type.Info : DialogModel.Type.Warning),
				Title = Resources.ActionFinished,
				Content = (isUserSpecial ? Resources.WarningForgetPasswordEmailRequestForSpecialUser : ((!doesUserExist) ? Resources.NoSpecifiedUserFound : (hasEmailAddress ? Resources.LinkToPasswordHasBeenSent : Resources.WarningNoEmailIsSet))),
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = "OK"
					}
				}
			});
		}
	}
}
