using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SystemDb;
using ViewboxAdmin.ViewModels.Users;

namespace ViewboxAdmin.ViewModels.Roles
{
    class RoleLoader
    {
        public ISystemDb SystemDb { get; private set; }
        public IEnumerable<UserModel> Users { get; private set;} 
        public RoleLoader(ISystemDb systemdb, IEnumerable<UserModel> Users ) {
            this.SystemDb = systemdb;
            this.Users = Users;
        }

        public ObservableCollection<RoleModel> GetRoles() {
            ObservableCollection<RoleModel> result = new ObservableCollection<RoleModel>();
            foreach (var role in SystemDb.Roles) {
                var rolemodel = new RoleModel();
                rolemodel.Id = role.Id;
                rolemodel.Name = role.Name;
                rolemodel.Flags = role.Flags;
                foreach (var user in role.Users) {
                    var nuser = Users.FirstOrDefault(x => x.Id == user.Id);
                    rolemodel.Users.Add(nuser);
                }
                result.Add(rolemodel);
            }
            return result;
        } 
    }
}
