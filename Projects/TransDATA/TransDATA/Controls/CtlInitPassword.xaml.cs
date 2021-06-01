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
using Base.Localisation;
using TransDATA.Models;
using Config.Interfaces.DbStructure;

namespace TransDATA.Controls {
    /// <summary>
    /// Interaktionslogik für CtlInitPassword.xaml
    /// </summary>
    public partial class CtlInitPassword : UserControl {

        InitPasswordModel Model { get { return DataContext as InitPasswordModel; } }

        public CtlInitPassword() {
            InitializeComponent();
        }

        private Window Owner {
            get { return UIHelpers.TryFindParent<Window>(this); }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            switch (Model.ValidatePasswords(newPw.Password, newPwRepeat.Password)) {
                case PasswordValidationState.NewPasswordsNotMatch:
                    MessageBox.Show(ExceptionMessages.PasswordsNotMatch, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
                    newPw.Focus();
                    newPw.SelectAll();
                    break;
                case PasswordValidationState.PasswortNotMatchesSaftyRules:
                    MessageBox.Show(ExceptionMessages.PasswortNotMatchesSaftyRules, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
                    newPw.Focus();
                    newPw.SelectAll();
                    break;
                case PasswordValidationState.Ok:
                    Model.SavePassword(newPw.Password);
                    Owner.Close();
                    break;
            }
        }
    }
}
