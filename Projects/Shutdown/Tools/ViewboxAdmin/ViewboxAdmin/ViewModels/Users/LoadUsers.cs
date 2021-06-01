using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;


namespace ViewboxAdmin.ViewModels.Users
{
    class LoadUsers
    {
        public LoadUsers(ISystemDb systemDb) {
            this.SystemDb = systemDb;
        }

        public ISystemDb SystemDb { get; private set; }

        public TrulyObservableCollection<UserModel> GetUsers() {
            TrulyObservableCollection<UserModel> result = new TrulyObservableCollection<UserModel>();
            foreach (var user in SystemDb.Users) {
                result.Add(new UserModel(user));
            }
            return result;
        }
        
    }
}
