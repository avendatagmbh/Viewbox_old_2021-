using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using SystemDb;
using SystemDb.Internal;
using Utils;
using Viewbox.Exceptions;

namespace Viewbox.Models
{
	public class PasswordModel
	{
		private static PasswordPolicy _pwdConfig;

		private static bool? _isPolicyEnabled;

		private readonly IUser _user;

		public static string PolicyDescription
		{
			get
			{
				if (_pwdConfig == null)
				{
					_pwdConfig = new PasswordPolicy();
				}
				string description = "";
				if (IsPolicyEnabled() && ViewboxSession.Language != null)
				{
					description = _pwdConfig.GetType().GetProperty($"PasswordDescription_{ViewboxSession.Language.CountryCode.ToUpper()}").GetValue(_pwdConfig, null)
						.ToString();
				}
				if (string.IsNullOrEmpty(description))
				{
					description = _pwdConfig.PasswordDescription_EN;
				}
				return description;
			}
		}

		public static PasswordPolicy PwdConfig
		{
			get
			{
				return _pwdConfig;
			}
			set
			{
				_pwdConfig = value;
			}
		}

		public PasswordModel(IUser user)
		{
			_user = user;
			_pwdConfig = new PasswordPolicy();
		}

		public static bool HasPolicyChange()
		{
			return IsPolicyEnabled() && ViewboxSession.User.PasswordCreationDate == DateTime.MinValue;
		}

		public static bool IsPolicyEnabled()
		{
			if (!_isPolicyEnabled.HasValue)
			{
				PwdConfig = PasswordConfiguration.GetPasswordPolicy();
				_isPolicyEnabled = PwdConfig != null && PwdConfig.EnablePolicy;
			}
			return _isPolicyEnabled.Value;
		}

		public static bool IsValidPasswordPattern(string passwordText, IUser user)
		{
			if (!string.IsNullOrWhiteSpace(passwordText))
			{
				return Regex.IsMatch(passwordText, ViewboxApplication.PasswordRegex);
			}
			return false;
		}

		public static bool ValidatePasswordPattern(string passwordText, IUser user)
		{
			if (!IsValidPasswordPattern(passwordText, user))
			{
				return false;
			}
			return true;
		}

		public bool TrySaveNewPassword(string passwordText)
		{
			if (ValidatePasswordPattern(passwordText, _user))
			{
				if (IsPolicyEnabled() && PwdConfig.HistoryEnabled)
				{
					ArchivePassword(GetPasswordHash(_user.Password, _user.UserName), _user.PasswordCreationDate);
				}
				_user.Password = passwordText;
				_user.PasswordCreationDate = DateTime.Now;
				_user.PasswordTrials = 0;
				_user.FirstLogin = false;
				ViewboxApplication.Database.SaveUser(_user);
				return true;
			}
			return false;
		}

		public bool ChangePassword(string newPassword, string oldPassword = null)
		{
			try
			{
				if (oldPassword == null || ViewboxApplication.ByUserName(_user.UserName).CheckPassword(oldPassword))
				{
					return TrySaveNewPassword(newPassword);
				}
			}
			catch (BadPasswordException)
			{
				throw;
			}
			catch
			{
			}
			return false;
		}

		private void ArchivePassword(string passwordHash, DateTime creationDate)
		{
			IPassword pwd = ViewboxApplication.Database.AddPasswordToHistory(passwordHash, _user.Id, creationDate);
			_user.PasswordHistory.Insert(0, pwd);
			ViewboxApplication.Database.SystemDb.Passwords.Insert(0, pwd);
			if (_user.PasswordHistory.Count() >= PwdConfig.PasswordHistoryCount)
			{
				Password oldestPwd = (_user.PasswordHistory as List<Password>).Last();
				ViewboxApplication.Database.DeletePasswordFromHistory(oldestPwd);
				_user.PasswordHistory.Remove(oldestPwd);
				ViewboxApplication.Database.SystemDb.Passwords.Remove(oldestPwd);
			}
		}

		private static string GetPasswordHash(string password, string salt)
		{
			return BitConverter.ToString(new SHA1CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes(password + salt))).Replace("-", "");
		}
	}
}
