using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Security.Principal;

namespace Utils
{
	public static class ActiveDirectory
	{
		public static string GetUserAbbr(string defaultString = "Unknown")
		{
			WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
			if (windowsIdentity != null)
			{
				string currentUser = windowsIdentity.Name;
				if (currentUser.LastIndexOf('\\') != -1)
				{
					currentUser = currentUser.Substring(currentUser.LastIndexOf('\\') + 1);
				}
				return currentUser;
			}
			return defaultString;
		}

		private static UserPrincipal GetCurrentUser()
		{
			WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
			if (windowsIdentity != null)
			{
				string currentUser = windowsIdentity.Name;
				if (currentUser.LastIndexOf('\\') != -1)
				{
					currentUser = windowsIdentity.Name.Substring(windowsIdentity.Name.LastIndexOf('\\') + 1);
					string domain = windowsIdentity.Name.Substring(0, windowsIdentity.Name.LastIndexOf('\\'));
					return UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain, domain), currentUser);
				}
			}
			return null;
		}

		public static string GetEmailOfUser()
		{
			UserPrincipal user = GetCurrentUser();
			if (user != null)
			{
				return user.EmailAddress;
			}
			return "";
		}

		public static string GetUserName(string defaultString = "Unknown")
		{
			UserPrincipal user = GetCurrentUser();
			if (user != null)
			{
				return user.DisplayName;
			}
			return defaultString;
		}

		public static List<Tuple<string, string>> GetActiveDirectoryUsers(string domainName, string filterUsername = null, bool useStar = false)
		{
			List<Tuple<string, string>> userList = new List<Tuple<string, string>>();
			if (domainName != string.Empty && domainName != null)
			{
				DirectoryEntry searchRoot = new DirectoryEntry("LDAP://" + domainName);
				string filter = (string.IsNullOrEmpty(filterUsername) ? "objectClass=user" : string.Format("(&(objectClass=user)(|(sAMAccountName={0}{1}{0})(CN={0}{1}{0})))", useStar ? "*" : "", filterUsername));
				DirectorySearcher ldapSearch = new DirectorySearcher(searchRoot)
				{
					Filter = filter,
					PageSize = int.MaxValue,
					SizeLimit = int.MaxValue
				};
				userList.AddRange(from SearchResult result in ldapSearch.FindAll()
					select result.GetDirectoryEntry() into directoryEntry
					where directoryEntry.Properties["sAMAccountName"].Value != null && directoryEntry.Properties["CN"].Value != null && (Convert.ToInt32(directoryEntry.Properties["userAccountControl"].Value) & 2) <= 0
					select new Tuple<string, string>(directoryEntry.Properties["CN"].Value.ToString(), directoryEntry.Properties["sAMAccountName"].Value.ToString()));
			}
			return userList;
		}

		public static ObservableCollectionAsync<string> GetDomains()
		{
			ObservableCollectionAsync<string> result = new ObservableCollectionAsync<string>();
			foreach (object dom in Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain)).Forest.Domains)
			{
				result.Add(dom.ToString());
			}
			return result;
		}
	}
}
