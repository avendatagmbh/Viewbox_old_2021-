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
        #region Methods

        public static string GetUserAbbr(string defaultString = "Unknown")
        {
            var windowsIdentity = WindowsIdentity.GetCurrent();
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
            var windowsIdentity = WindowsIdentity.GetCurrent();
            if (windowsIdentity != null)
            {
                string currentUser = windowsIdentity.Name;
                if (currentUser.LastIndexOf('\\') != -1)
                {
                    currentUser = windowsIdentity.Name.Substring(windowsIdentity.Name.LastIndexOf('\\') + 1);
                    string domain = windowsIdentity.Name.Substring(0, windowsIdentity.Name.LastIndexOf('\\'));
                    PrincipalContext domainContext = new PrincipalContext(ContextType.Domain, domain);
                    UserPrincipal user = UserPrincipal.FindByIdentity(domainContext, currentUser);
                    return user;
                }
                //return currentUser;
            }
            return null;
        }

        public static string GetEmailOfUser()
        {
            UserPrincipal user = GetCurrentUser();
            if (user != null)
                return user.EmailAddress;
            return "";
        }

        public static string GetUserName(string defaultString = "Unknown")
        {
            UserPrincipal user = GetCurrentUser();
            if (user != null)
                return user.DisplayName;
            return defaultString;
        }

        /// <summary>
        ///   Get a list of all active users in the specified domain. But careful, can be slow.
        /// </summary>
        /// <param name="domainName"> The domain name. </param>
        /// <returns> List of Tuples with CN [Name], sAMAccountName [LoginName] </returns>
        //public static List<Tuple<string, string>> GetActiveDirectoryUsers(string domainName) {
        //    //string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
        //    List<Tuple<string, string>> userList = new List<Tuple<string, string>>();
        //    if (domainName != string.Empty) {
        //        var ldapConnection = new DirectoryEntry("LDAP://" + domainName);
        //        var ldapSearch = new DirectorySearcher(ldapConnection) { Filter = "objectClass=user", PageSize = Int32.MaxValue, SizeLimit = Int32.MaxValue };
        //        userList.AddRange(from SearchResult result in ldapSearch.FindAll()
        //               select result.GetDirectoryEntry()
        //                   into directoryEntry
        //                   where
        //                       directoryEntry.Properties["sAMAccountName"].Value != null &&
        //                       directoryEntry.Properties["CN"].Value != null &&
        //                       !((Convert.ToInt32(directoryEntry.Properties["userAccountControl"].Value) & 0x00000002) > 0) //0x00000002 means disabled-account
        //                   select new Tuple<string, string> (directoryEntry.Properties["CN"].Value.ToString(),
        //                       directoryEntry.Properties["sAMAccountName"].Value.ToString()));
        //    }
        //    return userList;
        //}
        public static List<Tuple<string, string>> GetActiveDirectoryUsers(string domainName,
                                                                          string filterUsername = null,
                                                                          bool useStar = false)
        {
            List<Tuple<string, string>> userList = new List<Tuple<string, string>>();
            if (domainName != string.Empty)
            {
                var ldapConnection = new DirectoryEntry("LDAP://" + domainName);
                var filter = String.Empty;
                if (String.IsNullOrEmpty(filterUsername)) filter = "objectClass=user";
                else
                    filter = String.Format("(&(objectClass=user)(|(sAMAccountName={0}{1}{0})(CN={0}{1}{0})))",
                                           useStar ? "*" : "", filterUsername);
                var ldapSearch = new DirectorySearcher(ldapConnection)
                                     {Filter = filter, PageSize = Int32.MaxValue, SizeLimit = Int32.MaxValue};
                userList.AddRange(from SearchResult result in ldapSearch.FindAll()
                                  select result.GetDirectoryEntry()
                                  into directoryEntry
                                  where
                                      directoryEntry.Properties["sAMAccountName"].Value != null &&
                                      directoryEntry.Properties["CN"].Value != null &&
                                      !((Convert.ToInt32(directoryEntry.Properties["userAccountControl"].Value) &
                                         0x00000002) > 0) //0x00000002 means disabled-account
                                  select new Tuple<string, string>(directoryEntry.Properties["CN"].Value.ToString(),
                                                                   directoryEntry.Properties["sAMAccountName"].Value.
                                                                       ToString()));
            }
            return userList;
        }

        /// <summary>
        ///   Get a list of all available domains.
        /// </summary>
        /// <returns> ObservableCollectionAsync of all domain names. </returns>
        public static ObservableCollectionAsync<string> GetDomains()
        {
            var result = new ObservableCollectionAsync<string>();
            var domain = Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain));
            var forest = domain.Forest;
            var domains = forest.Domains;
            foreach (var dom in domains)
                result.Add(dom.ToString());
            return result;
        }

        #endregion Methods
    }
}