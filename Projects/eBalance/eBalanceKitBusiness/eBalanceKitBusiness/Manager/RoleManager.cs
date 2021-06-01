// -----------------------------------------------------------
// Created by Benjamin Held - 14.07.2011 15:21:29
// Copyright AvenDATA 2011
// -----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using eBalanceKitBusiness.Structures.DbMapping.Rights;
using DbAccess;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Manager {
    public static class RoleManager {

        #region properties

        #region Roles
        public static ObservableCollection<Role> Roles {
            get { return RoleManager._roles; }
        }
        private static ObservableCollection<Role> _roles = new ObservableCollection<Role>();
        #endregion

        #region AllowedRoles
        //public static ObservableCollection<Role> AllowedRoles {
        //    get { return RoleManager._allowedRoles; }
        //}
        //private static ObservableCollection<Role> _allowedRoles = new ObservableCollection<Role>();
        #endregion

        private static Dictionary<int, Role> _userRoles = new Dictionary<int,Role>();

        public static Dictionary<int, Role> UserRoles{
          get { return RoleManager._userRoles; }
        }
        #endregion properties

        #region methods

        #region Init
        public static void Init() {

            Roles.Clear();

            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                List<DbRole> tmp = conn.DbMapping.Load<DbRole>();
                
                foreach (DbRole role in tmp) {
                    //LogManager.Instance.NewRole(role, true);
                    if (role.UserId.HasValue && role.UserId.Value != 0) {
                        _userRoles[role.UserId.Value] = new Role(role);
                        //_userRoles[role.UserId.Value].LoadDetails();
                    } else {
                        Role newRole = new Role(role);
                        Roles.Add(newRole);
                        //newRole.LoadDetails();
                    }
                }
            }
        }
        #endregion

        #region AddRole
        public static void AddRole(Role role) {
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.BeginTransaction();
                    try {
                        conn.DbMapping.Save(role.DbRole);
                        Logs.LogManager.Instance.NewRole(role);
                        conn.CommitTransaction();
                    } catch (Exception ex) {
                        conn.RollbackTransaction();
                        throw new Exception(ex.Message, ex);
                    }

                    Roles.Add(role);
                    RightManager.RoleAdded(role);

                    //New role is always allowed
                    //AllowedRoles.Add(role);
                } catch (Exception ex) {
                    throw new Exception(Localisation.ExceptionMessages.RoleManagerAdd + ex.Message);
                }
            }
        }
        #endregion

        #region DeleteRole
        public static void DeleteRole(Role role) {

            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                conn.BeginTransaction();
                try {
                    Logs.LogManager.Instance.DeleteRole(role);
                    RightManager.RoleDeleted(conn, role);
                    conn.DbMapping.Delete(role.DbRole);
                    conn.CommitTransaction();
                } catch (Exception ex) {
                    conn.RollbackTransaction();
                    throw new Exception(Localisation.ExceptionMessages.RoleManagerDelete + ex.Message);
                }

                Roles.Remove(role);
                //AllowedRoles.Remove(role);
            }
        }
        #endregion

        #endregion methods

        public static Role GetUserRole(User user) {
            if (!UserRoles.ContainsKey(user.Id)) {
                UserRoles[user.Id] = new Role(new DbRole());
                UserRoles[user.Id].DbRole.DoDbUpdate = false;
                UserRoles[user.Id].DbRole.UserId = user.Id;
                UserRoles[user.Id].DbRole.Name = "userrole";
                UserRoles[user.Id].DbRole.Comment = null;
                UserRoles[user.Id].DbRole.DoDbUpdate = true;
                UserRoles[user.Id].DbRole.Save();
            }
            return UserRoles[user.Id];
        }

        public static Role GetRole(int roleId) {
            foreach (var role in Roles)
                if (role.DbRole.Id == roleId) return role;
            foreach (var role in UserRoles.Values)
                if (role.DbRole.Id == roleId) return role;
            return null;
        }


        //public static void InitAllowedRoles(RightDeducer rightDeducer) {
        //    AllowedRoles.Clear();
        //    foreach (var role in Roles) {
        //        if (role.IsAllowed(rightDeducer))
        //            AllowedRoles.Add(role);
        //    }
        //}

        /// <summary>
        /// Returns true, if the specified user name exists.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public static bool Exists(string userName) {
            foreach (Role role in Roles) {
                if (string.IsNullOrEmpty(role.Name) && string.IsNullOrEmpty(userName))
                    return true;
                else if (string.IsNullOrEmpty(role.Name) || string.IsNullOrEmpty(userName))
                    continue;
                if (role.Name.ToLower().Equals(userName.ToLower())) {
                    return true;
                }
            }

            return false;
        }


        public static void UpdateRole(Role role) {
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.DbMapping.Save(role.DbRole);
                    Logs.LogManager.Instance.UpdateRole(role);
                } catch (Exception ex) {
                    throw new Exception(Localisation.ExceptionMessages.RoleManagerUpdate + ex.Message);
                }
            }
        }
    }
}
