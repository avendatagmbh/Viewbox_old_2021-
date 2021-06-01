using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AV.Log;
using Base.Localisation;
using TransDATA.Models;
using Config.Interfaces.DbStructure;
using log4net;

namespace TransDATA.Controls {
    /// <summary>
    /// Interaktionslogik für CtlEditUser.xaml
    /// </summary>
    public partial class CtlEditUser : UserControl {
        internal ILog _log = LogHelper.GetLogger();

        public EditUserModel Model { get { return DataContext as EditUserModel; } }

        public CtlEditUser() {
            InitializeComponent();
        }

        private Window Owner {
            get { return UIHelpers.TryFindParent<Window>(this); }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            Owner.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            switch (Model.ValidatePasswords(oldPw.Password, newPw.Password, newPwRepeat.Password))
            {
                case PasswordValidationState.IncorrectOldPasswort:
                    MessageBox.Show(ExceptionMessages.InvalidPassword, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
                    oldPw.Focus();
                    oldPw.SelectAll();
                    break;
                case PasswordValidationState.NewPasswordsNotMatch:
                    MessageBox.Show(ExceptionMessages.PasswordsNotMatch, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
                    newPw.Focus();
                    newPw.SelectAll();
                    break;
                case PasswordValidationState.Ok:
                    Model.SavePassword(newPw.Password);
                    Owner.Close();
                    break;
                case PasswordValidationState.PasswortNotMatchesSaftyRules:
                    MessageBox.Show(ExceptionMessages.PasswortNotMatchesSaftyRules, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
                    newPw.Focus();
                    newPw.SelectAll();
                    break;
                default:
                    NotImplementedException ex=new NotImplementedException();
                    using (NDC.Push(LogHelper.GetNamespaceContext()))
                    {
                        _log.Error(ex.Message, ex);
                    }

                    throw ex;
            }
        }
    }
}
