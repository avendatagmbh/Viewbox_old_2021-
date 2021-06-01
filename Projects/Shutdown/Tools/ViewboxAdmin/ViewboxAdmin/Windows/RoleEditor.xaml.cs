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
using ViewboxAdmin.ViewModels;
using ViewboxAdmin.ViewModels.Roles;

namespace ViewboxAdmin.Windows
{
    /// <summary>
    /// Interaction logic for RoleEditor.xaml
    /// </summary>
    public partial class RoleEditor : UserControl
    {
        public RoleEditor() {
            InitializeComponent();
        }

        private void UserControl_Drop(object sender, DragEventArgs e) {
            try {
                var role = (e.OriginalSource as Control).DataContext as RoleModel;
                RoleEditViewModel VM = DataContext as RoleEditViewModel;
                VM.SelectedRole = role;
                VM.MarkAsDirty();
            }
            catch {
                MessageBox.Show("The drag and drop went wrong... try it again");
            }
            
            
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var VM = DataContext as RoleEditViewModel;
            VM.EditOrCreate += new RoleEditViewModel.editrole(VM_EditOrCreate);
            
        }

        void VM_EditOrCreate(Action yes, Action no, RoleModel role) {
            var RoleDialog = new RoleCreateDialog();
            RoleDialog.DataContext = role;
            var result = RoleDialog.ShowDialog();
            if (result == true) yes();
            else {
                no();
            }
        }

        
    }
}
