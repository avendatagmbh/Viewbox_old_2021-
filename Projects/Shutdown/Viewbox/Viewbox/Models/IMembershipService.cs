using System.Web.Security;

namespace Viewbox.Models
{
	public interface IMembershipService
	{
		int MinPasswordLength { get; }

		bool ValidateUser(string userName, string password);

		MembershipCreateStatus CreateUser(string userName, string password, string email);

		bool ChangePassword(string userName, string oldPassword, string newPassword);

		bool ValidateNewPassword(string userName, string newPassword);
	}
}
