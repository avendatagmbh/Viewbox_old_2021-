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
using ViewboxAdmin.ViewModels.Roles;
using ViewboxAdmin.ViewModels.Users;

namespace ViewboxAdmin.Windows
{
    /// <summary>
    /// Interaction logic for Role.xaml
    /// </summary>
    public partial class Role : UserControl
    {
        public Role() {
            InitializeComponent();
            this.listBox1.AllowDrop = true;
        }

        protected override void OnDrop(DragEventArgs e) {
            base.OnDrop(e);
            var usermodel = e.Data.GetData(typeof (UserModel)) as UserModel;
            RoleModel role = DataContext as RoleModel;
            if (role != null && !role.Users.Contains(usermodel))
            {
            role.Users.Add(usermodel);
            }
        }
    }
}
