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

namespace eBalanceKit.Controls {
    /// <summary>
    /// Interaktionslogik für CtlNavTab.xaml
    /// </summary>
    public partial class CtlNavTab : UserControl {
        public CtlNavTab() {
            InitializeComponent();
            this.DataContext = this;
        }
        
        public string Caption {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Caption.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof(string), typeof(CtlNavTab), new UIPropertyMetadata("-"));

        
    }
}
