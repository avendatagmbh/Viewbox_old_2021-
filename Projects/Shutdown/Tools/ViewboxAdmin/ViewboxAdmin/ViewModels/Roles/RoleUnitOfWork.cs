using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewboxAdmin.ViewModels.Roles
{
    class RoleUnitOfWork : IUnitOfWork<RoleModel> {

        public RoleUnitOfWork() {
            this.NewRoles = new List<RoleModel>();
            this.DirtyRoles = new List<RoleModel>();
            this.DeletedRoles = new List<RoleModel>();
        }

       
        public List<RoleModel> NewRoles { get; private set; }
        public List<RoleModel> DirtyRoles { get; private set; }
        public List<RoleModel> DeletedRoles { get; private set; } 


        public void MarkAsDirty(RoleModel item) {
            if (ObjectCanBeRegisteredAsDirty(item))
            {
                DirtyRoles.Add(item);
            }
        }

        private bool ObjectCanBeRegisteredAsDirty(RoleModel item) { return !NewRoles.Contains(item) && !DirtyRoles.Contains(item) && !NewRoles.Contains(item); }

        public void MarkAsNew(RoleModel item) {
            NewRoles.Add(item);
        }

        public void MarkAsDeleted(RoleModel item) {
            DeletedRoles.Add(item);
        }

        public void Commit() {
            // call the database methods here :D
            throw new NotImplementedException();
        }

        public void RollBack() {
            throw new NotImplementedException();
        }
    }
}
