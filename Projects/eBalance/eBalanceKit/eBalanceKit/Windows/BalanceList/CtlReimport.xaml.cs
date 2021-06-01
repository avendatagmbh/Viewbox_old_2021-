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
using AvdWpfControls;

namespace eBalanceKit.Windows.BalanceList {
    /// <summary>
    /// Interaktionslogik für CtlReimport.xaml
    /// </summary>
    public partial class CtlReimport : UserControl {
        public CtlReimport() {
            InitializeComponent();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e) {
            var slideControl = UIHelpers.TryFindParent<SlideControlBase>(sender as Button);
            if (slideControl != null) slideControl.Hide();
        }
    }
}
