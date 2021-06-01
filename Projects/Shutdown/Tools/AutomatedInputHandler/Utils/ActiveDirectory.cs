/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Sebestyén Muráncsik  2012-11-05      New function: +GetCurrentDomain() : ObservableCollectionAsync<String>
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Security.Principal;


namespace Utils {
    public static class ActiveDirectory {
        #region Methods
        public static string GetUserAbbr(string defaultString = "Unknown") {
            var windowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent();
            if (windowsIdentity != null) {
                string currentUser = windowsIdentity.Name;
                if (currentUser.LastIndexOf('\\') != -1) {
                    currentUser = currentUser.Substring(currentUser.LastIndexOf('\\') + 1);
                }
                return currentUser;
            }
            return defaultString;

        }
        private static UserPrincipal GetCurrentUser() {
            var windowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent();
            if (windowsIdentity != null) {
                string currentUser = windowsIdentity.Name;
                if (currentUser.LastIndexOf('\\') != -1) {
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
        public static string GetEmailOfUser() {
            UserPrincipal user = GetCurrentUser();
            if(user != null)
                return user.EmailAddress;
            return "";
        }

        public static string GetUserName(string defaultString = "Unknown") {
            UserPrincipal user = GetCurrentUser();
            if (user != null)
                return user.DisplayName;
            return defaultString;
        }

        /// <summary>
        /// Get a list of all active users in the specified domain. But careful, can be slow.
        /// </summary>
        /// <param name="domainName">The domain name.</param>
        /// <param name="filterUsername">The username to search for at the AD</param>
        /// <param name="useStar">Defines if using an asterisk as placeholder. </param>
        /// <returns>List of Tuples with CN [Name], sAMAccountName [LoginName]</returns>
        /// <remarks>
        /// If filterUsername is ma and useStar=false it will find only users with the name ma.
        /// If filterUsername is ma and useStar=true it will also find users with the name marc, manni or malogi. 
        /// </remarks>
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
        public static List<Tuple<string, string>> GetActiveDirectoryUsers(string domainName, string filterUsername = null, bool useStar = false) {
            List<Tuple<string, string>> userList = new List<Tuple<string, string>>();
            if (domainName != string.Empty)
            {
                DirectoryEntry ldapConnection = new DirectoryEntry("LDAP://" + domainName);
                String filter = String.Empty;

                if (String.IsNullOrEmpty(filterUsername))
                {
                    filter = "(&(objectCategory=Person)(objectClass=user))";
                }
                else
                {
                    filter = String.Format("(&(objectCategory=Person)(objectClass=user)(|(sAMAccountName={0}{1}{0})(CN={0}{1}{0})))", useStar ? "*" : "", filterUsername);
                }

                DirectorySearcher ldapSearch = new DirectorySearcher(ldapConnection)
                {
                    Filter = filter,
                    PageSize = Int32.MaxValue,
                    SizeLimit = Int32.MaxValue
                };

                SearchResultCollection resultCollection = ldapSearch.FindAll();

                userList.AddRange(from SearchResult result in resultCollection
                                  select result.GetDirectoryEntry()
                                      into directoryEntry
                                      where
                                          directoryEntry.Properties["sAMAccountName"].Value != null &&
                                          directoryEntry.Properties["CN"].Value != null &&
                                          !((Convert.ToInt32(directoryEntry.Properties["userAccountControl"].Value) & 0x00000002) > 0) //0x00000002 means disabled-account
                                      select new Tuple<string, string>(directoryEntry.Properties["CN"].Value.ToString(),
                                          directoryEntry.Properties["sAMAccountName"].Value.ToString()));

            }

            return userList;
        }


        /// <summary>
        /// Get a list of all available domains. (Using forest and trusts)
        /// </summary>
        /// <returns>ObservableCollectionAsync of all domain names.</returns>
        public static ObservableCollectionAsync<string> GetDomains() {
            ObservableCollectionAsync<String> result = new ObservableCollectionAsync<string>();
            Domain domain = Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain));
            Forest forest = domain.Forest;
            DomainCollection domains = forest.Domains;

            foreach (var dom in domains)
            {
                result.Add(dom.ToString());
            }

            var domainTrusts = domain.GetAllTrustRelationships();
            foreach (TrustRelationshipInformation domainTrust in domainTrusts)
            {
                if (!result.Contains(domainTrust.SourceName))
                {
                    result.Add(domainTrust.SourceName);
                }
                if (!result.Contains(domainTrust.TargetName))
                {
                    result.Add(domainTrust.TargetName);
                }
            }

            return result;
        }

        /// <summary>
        /// Get the domain of the current user.
        /// Similar than
        /// http://stackoverflow.com/questions/349520/net-built-in-helper-to-parse-domain-username-in-user-identity-name
        /// </summary>
        /// <returns>ObservableCollectionAsync with the actual domain name.</returns>
        public static ObservableCollectionAsync<String> GetCurrentDomain()
        {
            ObservableCollectionAsync<String> result = new ObservableCollectionAsync<string>();
            WindowsIdentity user = WindowsIdentity.GetCurrent();
            string s = user.Name;
            int stop = s.IndexOf("\\");

            if (stop > -1)
            {
                result.Add(s.Substring(0, stop));
            }
            return result;
        }

        #endregion Methods

    }
}
