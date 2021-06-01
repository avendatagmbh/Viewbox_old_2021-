using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewboxAdmin.ViewModels.Users
{
    class NewUserEventArg: EventArgs
    {

        public NewUserEventArg(UserModel user, Action Yes ,Action No) { 
            this.User = user;
            this.Yes = Yes;
            this.No = No;
        }

        public Action Yes { get; private set; }
        public Action No { get; private set; }
        public UserModel User { get; private set; }
    }
}
