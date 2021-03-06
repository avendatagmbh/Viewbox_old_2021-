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
    /// Interaktionslogik für CtlErrorMessage.xaml
    /// </summary>
    public partial class CtlErrorMessage : UserControl {
        public CtlErrorMessage() {
            InitializeComponent();
            errMessage.DataContext = this;
        }

        public string Message {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Message.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(CtlErrorMessage), new UIPropertyMetadata(null));
    }
}
