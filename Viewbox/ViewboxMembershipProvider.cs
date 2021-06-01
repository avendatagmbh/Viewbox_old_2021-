using System;
using System.DirectoryServices.AccountManagement;
using System.Web.Security;
using SystemDb;
using AV.Log;
using log4net;
using Viewbox.Models;

namespace Viewbox
{
	public class ViewboxMembershipProvider : MembershipProvider
	{
		private ILog _logger = LogHelper.GetLogger();

		public override string ApplicationName
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public override bool EnablePasswordReset
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override bool EnablePasswordRetrieval
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override int MaxInvalidPasswordAttempts
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override int MinRequiredNonAlphanumericCharacters
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override int MinRequiredPasswordLength
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override int PasswordAttemptWindow
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override MembershipPasswordFormat PasswordFormat => MembershipPasswordFormat.Hashed;

		public override string PasswordStrengthRegularExpression
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override bool RequiresQuestionAndAnswer
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override bool RequiresUniqueEmail => true;

		public override bool ValidateUser(string userName, string password)
		{
			IUser user = ViewboxApplication.ByUserName(userName);
			if (user?.IsADUser ?? false)
			{
				return AuthenticateUser(user.Domain, userName, password);
			}
			return ViewboxApplication.Login(userName, password) != null;
		}

		public bool AuthenticateUser(string domain, string username, string password)
		{
			using PrincipalContext ctx = new PrincipalContext(ContextType.Domain, domain);
			try
			{
				return ctx.ValidateCredentials(username, password);
			}
			catch (Exception ex)
			{
				_logger.Error(ex.Message);
				return false;
			}
		}

		public override bool ChangePassword(string userName, string oldPassword, string newPassword)
		{
			return new PasswordModel(ViewboxApplication.ByUserName(userName)).ChangePassword(newPassword, oldPassword);
		}

		public override bool ChangePasswordQuestionAndAnswer(string userName, string password, string newPasswordQuestion, string newPasswordAnswer)
		{
			throw new NotImplementedException();
		}

		public override MembershipUser CreateUser(string userName, string password, string name, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
		{
			throw new NotImplementedException();
		}

		public override bool DeleteUser(string userName, bool deleteAllRelatedData)
		{
			throw new NotImplementedException();
		}

		public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override MembershipUserCollection FindUsersByName(string nameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override int GetNumberOfUsersOnline()
		{
			throw new NotImplementedException();
		}

		public override string GetPassword(string email, string answer)
		{
			throw new NotImplementedException();
		}

		public override MembershipUser GetUser(string email, bool userIsOnline)
		{
			throw new NotImplementedException();
		}

		public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
		{
			throw new NotImplementedException();
		}

		public override string GetUserNameByEmail(string email)
		{
			throw new NotImplementedException();
		}

		public override string ResetPassword(string email, string answer)
		{
			throw new NotImplementedException();
		}

		public override bool UnlockUser(string email)
		{
			throw new NotImplementedException();
		}

		public override void UpdateUser(MembershipUser user)
		{
			throw new NotImplementedException();
		}
	}
}
