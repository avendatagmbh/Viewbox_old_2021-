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

namespace eBalanceKit.Windows.MappingTemplates {
    /// <summary>
    /// Interaktionslogik für CtlEditTemplate.xaml
    /// </summary>
    public partial class CtlEditTemplate : UserControl {
        public CtlEditTemplate() {
            InitializeComponent();
        }

        private void BtnNextClick(object sender, RoutedEventArgs e) {
            UIHelpers.TryFindParent<Window>(this).DialogResult = true;
        }

        private void BtnCancelClick(object sender, RoutedEventArgs e) {
            UIHelpers.TryFindParent<Window>(this).DialogResult = false;
        }
    }
}
