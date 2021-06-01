using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business;
using Config.Interfaces.DbStructure;
using System.Windows;
using Config.Manager;

namespace TransDATA.Models {
    public class EditUserModel {

        public EditUserModel(IUser user)
        {
            User = user;
        }

        #region User
        public IUser User { get; private set; }
        #endregion User

        internal PasswordValidationState ValidatePasswords(string oldPw, string newPw, string newPwRepeat) {
            if(!newPw.Equals(newPwRepeat)) return PasswordValidationState.NewPasswordsNotMatch;
            if (!AppController.UserManager.CanLogon(User, oldPw)) return PasswordValidationState.IncorrectOldPasswort;
            if(newPw.Length < 5) return PasswordValidationState.PasswortNotMatchesSaftyRules;
            return PasswordValidationState.Ok;
        }

        internal void SavePassword(string pw) {
            UserManager.SetPassword(User, pw);
            AppController.UserManager.UpdateUser(User);
        }
    }

    public enum PasswordValidationState
    {
        NewPasswordsNotMatch,
        IncorrectOldPasswort,
        PasswortNotMatchesSaftyRules,
        Ok
    }
}
