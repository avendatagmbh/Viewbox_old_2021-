using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;
using SystemDb.Internal;
using DbAccess;
using DbAccess.Structures;

namespace ViewboxAdmin.ViewModels.Users
{
    class UserDataBaseMapper : IUserDataBaseMapper
    {
        public UserDataBaseMapper(ISystemDb SystemDb) {
            this.SystemDb = SystemDb;
            UserRelatedTableNames = new List<string>() {
                "category_users",
                "column_users",
                "optimization_users",
                "table_users",
                "userlog_users",
                "user_column_order_settings",
                "user_column_settings",
                "user_controller_settings",
                "user_optimization_settings",
                "user_property_settings",
                "user_roles",
                "user_table_column_width_settings",
                "user_table_object_order_settings",
                "user_table_settings",
                "user_table_transactionid_settings",
                "user_userlog_settings"
            };
        }

        public ISystemDb SystemDb { get; private set; }

        public void Save(List<UserModel> users) {
            
                using (var db = ConnectionManager.CreateConnection(SystemDb.ConnectionManager.DbConfig))
                {
                    db.Open();
                    foreach (var userWrapper in users){
                    User user = CloneUserWithoutId(userWrapper);
                        db.DbMapping.Save(user);
                        userWrapper.Id = user.Id;
                    }
            }
        }

        private List<string> UserRelatedTableNames;

        public void Delete(List<UserModel> users) {
            using (var db = ConnectionManager.CreateConnection(SystemDb.ConnectionManager.DbConfig))
            {
                db.Open();
                foreach (var userWrapper in users) {
                    User user = CloneUserWithoutId(userWrapper);
                    user.Id = userWrapper.Id;
                    user.Password = "placeholder";
                    db.DbMapping.Delete(user);


                    foreach (var userRelatedTableName in UserRelatedTableNames) {
                        var val = new DbColumnValues();
                        val["user_id"] = user.Id;
                        db.Delete(userRelatedTableName, val);
                    }
                }
            }
        }

        private User CloneUserWithoutId(UserModel user) { 
            var result = new User();
            result.UserName = user.UserName;
            result.Name = user.Name;
            result.Flags = user.Flags;
            result.Email = user.Email;
            result.IsADUser = user.IsADUser;
            //prevent argument exception:
            if (user.Password!=null) result.Password = user.Password;
            result.DisplayRowCount = user.DisplayRowCount;
            result.Domain = user.Domain;
            return result;
        }


        public void Update(List<UserModel> users) {
            using (var db = ConnectionManager.CreateConnection(SystemDb.ConnectionManager.DbConfig))
            {
                db.Open();
                foreach (var userWrapper in users)
                {
                    User user = CloneUserWithoutId(userWrapper);
                    user.Id = userWrapper.Id;
                    db.DbMapping.Save(user);
                }
            }
        }
    }
}
