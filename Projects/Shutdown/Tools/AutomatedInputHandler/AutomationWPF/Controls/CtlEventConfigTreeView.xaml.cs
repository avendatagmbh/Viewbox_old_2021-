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

namespace AutomationWPF.Controls
{
    /// <summary>
    /// Interaction logic for CtlEventConfigTreeView.xaml
    /// </summary>
    public partial class CtlEventConfigTreeView : UserControl
    {
        public CtlEventConfigTreeView()
        {
            InitializeComponent();
        }

        private void nav_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }
    }
}
