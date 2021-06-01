using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business;
using Config.Interfaces.DbStructure;
using Config.Manager;

namespace TransDATA.Models {
    public class InitPasswordModel {

        public InitPasswordModel(IUser user)
        {
            User = user;
        }

        #region User
        public IUser User { get; private set; }
        #endregion User

        internal PasswordValidationState ValidatePasswords(string newPw, string newPwRepeat) {
            if (!newPw.Equals(newPwRepeat)) return PasswordValidationState.NewPasswordsNotMatch;
            if (newPw.Length < 5) return PasswordValidationState.PasswortNotMatchesSaftyRules;
            return PasswordValidationState.Ok;
        }

        internal void SavePassword(string pw) {
            UserManager.SetPassword(User, pw);
            User.IsInitialized = true;
            AppController.UserManager.UpdateUser(User);
        }
    }
}
