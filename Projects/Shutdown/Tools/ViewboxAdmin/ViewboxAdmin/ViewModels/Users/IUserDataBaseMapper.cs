using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewboxAdmin.ViewModels.Users
{
    interface IUserDataBaseMapper {
        void Save(List<UserModel> users);
        void Delete(List<UserModel> users);
        void Update(List<UserModel> users);

    }
}
