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
using ViewValidator.Models.Datagrid;

namespace ViewValidator.Controls.Datagrid {
    /// <summary>
    /// Interaktionslogik für CtlColumnHeader.xaml
    /// </summary>
    public partial class CtlColumnHeader : UserControl {
        public CtlColumnHeader() {
            InitializeComponent();
        }

        ColumnHeaderModel Model { get { return DataContext as ColumnHeaderModel; } }

        private void btnHide_Click(object sender, RoutedEventArgs e) {
            Model.Hide();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {

        }
    }
}
