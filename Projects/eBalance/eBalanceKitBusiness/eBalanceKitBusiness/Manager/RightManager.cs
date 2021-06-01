using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DbAccess;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping.Rights;

namespace eBalanceKitBusiness.Manager {

    /// <summary>
    /// 
    /// </summary>
    /// <author>Mirko Dibbert / Benjamin Held</author>
    /// <since>2011-02-23</since>
    public static class RightManager {

        public static RightDeducer RightDeducer { get; private set; }

        public static void Init() {

            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {

                try {
                    
                    foreach(DbUserRole dbUserRole in conn.DbMapping.Load<DbUserRole>()){
                        if (RoleManager.GetRole(dbUserRole.RoleId) == null) {
                            // if role no longer exists for some reason, then delete the assignment
                            string sql = "DELETE FROM " + conn.Enquote(conn.DbMapping.GetTableName(typeof(DbUserRole))) +
                                " WHERE role_id = " + dbUserRole.RoleId +
                                " AND user_id = " + dbUserRole.UserId;
                            conn.ExecuteNonQuery(sql);
                        } else {
                            RightManager.AddAssignedRole(dbUserRole.User, dbUserRole.Role, false);
                        }
                    }

                } catch (Exception ex) {
                    throw new Exception("Could not load user priviledges: " + ex.Message);
                } finally {
                    LogManager.Instance.Log = true;
                }
            }
        }

        public static void InitAllowedDetails() {
            //RightDeducer = new RightDeducer(UserManager.Instance.CurrentUser);
            RightDeducer = UserManager.Instance.RightDeducer;
            CompanyManager.Instance.InitAllowedCompanies(RightDeducer);
            DocumentManager.Instance.InitAllowedDocuments(RightDeducer);
            //RoleManager.InitAllowedRoles(RightDeducer);
        }

        #region RoleDeleted
        public static void RoleDeleted(IDatabase conn, Role role) {
            DbRole dbRole = role.DbRole;
            string sql = "DELETE FROM " + conn.Enquote(conn.DbMapping.GetTableName(typeof (DbUserRole))) +
                         " WHERE "+conn.Enquote("role_id")+" = " + dbRole.Id;
            conn.ExecuteNonQuery(sql);

            sql = "DELETE FROM " + conn.Enquote(conn.DbMapping.GetTableName(typeof (DbRight))) +
                  " WHERE "+conn.Enquote("role_id")+" = " + dbRole.Id;
            conn.ExecuteNonQuery(sql);
        }
        #endregion

        
        #region RoleAdded
        public static void RoleAdded(Role role) {
            //foreach (var keyValuePair in UserRoleAssignments) {
            //    keyValuePair.Value.Add(
            //            new UserRoleAssignment(
            //                new DbUserRole() { RoleId = role.DbRole.Id, UserId = keyValuePair.Key }, false
            //            )
            //        );
            //}
            //foreach (var keyValuePair in VisibleUserRoleAssignments) {
            //    keyValuePair.Value.Add(
            //            new UserRoleAssignment(
            //                new DbUserRole() { RoleId = role.DbRole.Id, UserId = keyValuePair.Key }, false
            //            )
            //        );
            //}
        }
        #endregion

        
        #region CompanyAdded
        public static void CompanyAdded(Company company) {
            //RoleManager.GetUserRole(UserManager.Instance.CurrentUser).GetRight(company).SetFullRights();
            //RoleManager.GetUserRole(UserManager.Instance.CurrentUser).Save();
            RightDeducer.CompanyAdded(company);
            // TODO mid - why did you remove this auto assignment of rights?
            RoleManager.GetUserRole(UserManager.Instance.CurrentUser).GetRight(company).SetFullRights();
            RoleManager.GetUserRole(UserManager.Instance.CurrentUser).Save();
        }
        #endregion


        #region DocumentAdded
        public static void DocumentAdded(Document document) {
            //RoleManager.GetUserRole(UserManager.Instance.CurrentUser).GetRight(document).SetFullRights();
            //RoleManager.GetUserRole(UserManager.Instance.CurrentUser).Save();
            RightDeducer.DocumentAdded(document);
        }
        #endregion


        #region UserDeleted
        public static void UserDeleted(IDatabase conn, User user) {
            string sql = "DELETE FROM " + conn.Enquote(conn.DbMapping.GetTableName(typeof(DbUserRole))) +
                " WHERE "+conn.Enquote("user_id")+" = " + user.Id;
            conn.ExecuteNonQuery(sql);
        }
        #endregion


        #region DocumentDeleted
        public static void DocumentDeleted(Document document) {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                string sql = "DELETE FROM " + conn.Enquote(conn.DbMapping.GetTableName(typeof(DbRight))) +
                    " WHERE "+conn.Enquote("content_type")+" = " + (int)DbRight.ContentTypes.Document +
                    " AND "+conn.Enquote("ref_id")+" = " + document.Id;
                conn.ExecuteNonQuery(sql);

                foreach (DbRight.ContentTypes type in Enum.GetValues(typeof(DbRight.ContentTypes))) {
                    if (DbRight.IsSpecialRight(type)) {
                        sql = "DELETE FROM " + conn.Enquote(conn.DbMapping.GetTableName(typeof(DbRight))) +
                            " WHERE "+conn.Enquote("content_type")+" = " + (int)type +
                            " AND "+conn.Enquote("ref_id")+ " = " + document.Id;
                        conn.ExecuteNonQuery(sql);
                    }
                }
                
            }
        }
        #endregion


        #region CompanyDeleted
        public static void CompanyDeleted(Company company) {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                string sql = "DELETE FROM " + conn.Enquote(conn.DbMapping.GetTableName(typeof(DbRight))) +
                    " WHERE " + conn.Enquote("content_type") + " = " + (int)DbRight.ContentTypes.Company +
                    " AND " + conn.Enquote("ref_id") +" = " + company.Id;
                conn.ExecuteNonQuery(sql);
            }
        }
        #endregion

        public static bool ExportAllowed(Document document) {
            return RightDeducer.GetRight(document).IsExportAllowed;
        }

        public static bool SendAllowed(Document document) {
            return RightDeducer.GetRight(document).IsSendAllowed;
        }

        public static bool ReadRestDocumentAllowed(Document document) {
            if (document == null) return true;
            return RightDeducer.GetSpecialRight(document, DbRight.ContentTypes.DocumentSpecialRight_Rest).IsReadAllowed;
        }

        public static bool WriteRestDocumentAllowed(Document document) {
            if (document == null) return true;
            return RightDeducer.GetSpecialRight(document, DbRight.ContentTypes.DocumentSpecialRight_Rest).IsWriteAllowed;
        }

        public static bool ReadTransferValuesAllowed(Document document) {
            if (document == null) return true;
            return RightDeducer.GetSpecialRight(document,DbRight.ContentTypes.DocumentSpecialRight_TransferValueCalculation).IsReadAllowed;
        }

        public static bool WriteTransferValuesAllowed(Document document) {
            if (document == null) return true;
            return RightDeducer.GetSpecialRight(document, DbRight.ContentTypes.DocumentSpecialRight_TransferValueCalculation).IsWriteAllowed;
        }

        public static bool CompanyWriteAllowed(Company company) {
            if (company == null) return true;
            return RightDeducer.CompanyEditable(company);
            //return RightDeducer.GetRight(company).IsWriteAllowed;
        }

        public static bool HasDocumentRestWriteRight(Document document, System.Windows.Window parent) {
            if (WriteRestDocumentAllowed(document)) return true;
            MessageBox.Show(parent, "Sie habe keine Berechtigung diesen Teil des Reports zu verändern.");
            return false;
        }
        
        public static void AddAssignedRole(User user, Role role, bool persistate = true) {
            //if (user == null || role == null || user.AssignedRoles.Contains(role)) return;
            //user.AssignedRoles.Add(role);
            //role.AssignedUsers.Add(user);
            if (user != null && role != null && !user.AssignedRoles.Any(r => r.DbRole.Id == role.DbRole.Id)) {
                user.AssignedRoles.Add(role);
                //for (int i = 0; i < UserManager.Instance.EditableUsers.Count; i++) {
                //    if (UserManager.Instance.EditableUsers[i].Id == user.Id) {
                //        UserManager.Instance.EditableUsers.RemoveAt(i);
                //        UserManager.Instance.EditableUsers.Add(user);
                //        break;
                //    }
                //}
            }
            if (user != null && role != null && !role.AssignedUsers.Any(u => u.Id == user.Id))
                role.AssignedUsers.Add(user);

            if (persistate) {
                using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                    conn.DbMapping.Save(
                        new eBalanceKitBusiness.Structures.DbMapping.Rights.DbUserRole { RoleId = role.DbRole.Id, UserId = user.Id });
                }

                //ToDo add log entry for new assigned role
            }
        }

        public static void RemoveAssignedRole(User user, Role role) {
            if (role == null) return;
            user.AssignedRoles.Remove(role);
            role.AssignedUsers.Remove(user);

            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                string sql = "DELETE FROM " + conn.Enquote(conn.DbMapping.GetTableName(typeof(DbUserRole))) +
                    " WHERE "+conn.Enquote("role_id")+" = " + role.DbRole.Id + 
                    " AND "+conn.Enquote("user_id")+ " = " + user.Id;
                conn.ExecuteNonQuery(sql);
            }
        }

        public static EffectiveUserRightTreeNode GetEffectiveUserRightTree(User user) {
            RightDeducer tmp = new RightDeducer(user);
            return tmp.GetEffectiveUserRightTree(); 
        }
    }
}
