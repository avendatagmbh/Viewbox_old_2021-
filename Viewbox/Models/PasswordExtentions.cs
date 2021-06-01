using System;
using SystemDb;

namespace Viewbox.Models
{
	public static class PasswordExtentions
	{
		public static bool HasPasswordExpired(this IUser user)
		{
			return ViewboxApplication.PasswordExpiredInDays > -1 && DateTime.Now.Subtract(user.PasswordCreationDate).TotalDays >= (double)ViewboxApplication.PasswordExpiredInDays;
		}
	}
}
