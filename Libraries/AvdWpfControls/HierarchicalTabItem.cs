using System.Windows;
using System.Windows.Controls;

namespace AvdWpfControls
{
    public class HierarchicalTabItem : TreeViewItem
    {
        static HierarchicalTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (HierarchicalTabItem),
                                                     new FrameworkPropertyMetadata(typeof (HierarchicalTabItem)));
        }

        #region Content

        //--------------------------------------------------------------------------------

        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof (object), typeof (HierarchicalTabItem),
                                        new UIPropertyMetadata(null));

        public object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        //--------------------------------------------------------------------------------

        #endregion Content
    }
}