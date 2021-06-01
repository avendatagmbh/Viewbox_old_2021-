using System;
using System.Web.Security;
using SystemDb;

namespace Viewbox.Models
{
	public class FormsAuthenticationService : IFormsAuthenticationService
	{
		public void SignIn(string userName, bool createPersistentCookie)
		{
			if (string.IsNullOrEmpty(userName))
			{
				throw new ArgumentException("Der Wert darf nicht NULL oder leer sein.", "userName");
			}
			if (HttpContextFactory.Current.Session["UnitTesting"] == null || !(bool)HttpContextFactory.Current.Session["UnitTesting"])
			{
				FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
			}
			IUser user = ViewboxApplication.ByUserName(userName);
			user.UserRightHelper = null;
		}

		public void SignOut()
		{
			if (HttpContextFactory.Current.Session["UnitTesting"] == null || !(bool)HttpContextFactory.Current.Session["UnitTesting"])
			{
				FormsAuthentication.SignOut();
			}
		}
	}
}
