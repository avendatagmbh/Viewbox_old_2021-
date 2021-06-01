// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2011-01-03
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using eBalanceKit.Models;

namespace eBalanceKit.Controls.XbrlVisualisation {
    
    public partial class XbrlBasePanel {
        public XbrlBasePanel() {

            InitializeComponent();

            borderHeader.DataContext = this;
            txtHeader.SetBinding(TextBlock.TextProperty, new Binding("Header"));
        }

        public string Header {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        
        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(XbrlBasePanel), new UIPropertyMetadata(""));
        
        public Visibility HeaderVisibility {
            get { return (Visibility)GetValue(HeaderVisibilityProperty); }
            set { SetValue(HeaderVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderVisibilityProperty =
            DependencyProperty.Register("HeaderVisibility", typeof(Visibility), typeof(XbrlBasePanel), new UIPropertyMetadata(Visibility.Visible));
        
        internal DisplayValueTreeModel Model { get { return DataContext as DisplayValueTreeModel; } }
    }
}