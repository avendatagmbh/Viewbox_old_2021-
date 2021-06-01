using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewboxAdmin.ViewModels.Users
{
    class UserUnitOfWork : IUnitOfWork<UserModel> {

        public UserUnitOfWork(IUserDataBaseMapper userDataBaseMapper) {
            this.UserDataBaseMapper = userDataBaseMapper;
            this.newUsers = new List<UserModel>();
            this.dirtyUsers = new List<UserModel>();
            this.deletedUsers = new List<UserModel>();
        }

        public IUserDataBaseMapper UserDataBaseMapper { get; private set; }
        public List<UserModel> newUsers { get; private set; }
        public List<UserModel> dirtyUsers { get; private set; }
        public List<UserModel> deletedUsers { get;private set; } 

        public void MarkAsDirty(UserModel item) {
            if (ObjectCanBeRegisteredAsDirty(item))
            {
                dirtyUsers.Add(item);
            }
        }

        private bool ObjectCanBeRegisteredAsDirty(UserModel item) { return !newUsers.Contains(item) && !dirtyUsers.Contains(item) && !newUsers.Contains(item); }

        public void MarkAsNew(UserModel item) {
            newUsers.Add(item);
        }

        public void MarkAsDeleted(UserModel item) {
            deletedUsers.Add(item);
        }

        public void Commit() {
            UserDataBaseMapper.Save(newUsers);
            UserDataBaseMapper.Delete(deletedUsers);
            UserDataBaseMapper.Update(dirtyUsers);
            ClearElements();
        }

        private void ClearElements() {
            newUsers.Clear();
            deletedUsers.Clear();
            dirtyUsers.Clear();
        }

        public void RollBack() {
            throw new NotImplementedException();
        }
    }
}
