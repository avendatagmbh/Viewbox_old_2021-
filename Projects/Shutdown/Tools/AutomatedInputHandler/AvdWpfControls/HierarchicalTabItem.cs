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
using System.Collections.ObjectModel;

namespace AvdWpfControls {
    public class HierarchicalTabItem : TreeViewItem {
        static HierarchicalTabItem() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HierarchicalTabItem), new FrameworkPropertyMetadata(typeof(HierarchicalTabItem)));
        }


        #region Content
        //--------------------------------------------------------------------------------
        public object Content {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(object), typeof(HierarchicalTabItem), new UIPropertyMetadata(null));
        //--------------------------------------------------------------------------------
        #endregion Content
        
    }
}
