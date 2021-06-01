using System;
using System.Web.Security;

namespace Viewbox.Models
{
	public class AccountMembershipService : IMembershipService
	{
		private readonly MembershipProvider _provider;

		public int MinPasswordLength => _provider.MinRequiredPasswordLength;

		public AccountMembershipService()
			: this(null)
		{
		}

		public AccountMembershipService(MembershipProvider provider)
		{
			_provider = provider ?? Membership.Provider;
		}

		public bool ValidateUser(string userName, string password)
		{
			if (string.IsNullOrEmpty(userName))
			{
				throw new ArgumentException("Der Wert darf nicht NULL oder leer sein.", "userName");
			}
			return _provider.ValidateUser(userName, password);
		}

		public MembershipCreateStatus CreateUser(string userName, string password, string email)
		{
			if (string.IsNullOrEmpty(userName))
			{
				throw new ArgumentException("Der Wert darf nicht NULL oder leer sein.", "userName");
			}
			if (string.IsNullOrEmpty(password))
			{
				throw new ArgumentException("Der Wert darf nicht NULL oder leer sein.", "password");
			}
			if (string.IsNullOrEmpty(email))
			{
				throw new ArgumentException("Der Wert darf nicht NULL oder leer sein.", "email");
			}
			_provider.CreateUser(userName, password, email, null, null, isApproved: true, null, out var status);
			return status;
		}

		public bool ValidateNewPassword(string userName, string newPassword)
		{
			return PasswordModel.ValidatePasswordPattern(newPassword, ViewboxApplication.ByUserName(userName));
		}

		public bool ChangePassword(string userName, string oldPassword, string newPassword)
		{
			if (string.IsNullOrEmpty(userName))
			{
				throw new ArgumentException("Der Wert darf nicht NULL oder leer sein.", "userName");
			}
			if (string.IsNullOrEmpty(oldPassword))
			{
				throw new ArgumentException("Der Wert darf nicht NULL oder leer sein.", "oldPassword");
			}
			if (string.IsNullOrEmpty(newPassword))
			{
				throw new ArgumentException("Der Wert darf nicht NULL oder leer sein.", "newPassword");
			}
			try
			{
				MembershipUser currentUser = _provider.GetUser(userName, userIsOnline: true);
				return currentUser.ChangePassword(oldPassword, newPassword);
			}
			catch (ArgumentException)
			{
				return false;
			}
			catch (MembershipPasswordException)
			{
				return false;
			}
		}
	}
}
