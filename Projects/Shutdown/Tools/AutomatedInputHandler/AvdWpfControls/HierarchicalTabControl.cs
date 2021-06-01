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

namespace AvdWpfControls {

    public class HierarchicalTabControl : TreeView {
        static HierarchicalTabControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HierarchicalTabControl), new FrameworkPropertyMetadata(typeof(HierarchicalTabControl)));
        }

        protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e) {
            base.OnSelectedItemChanged(e);

            if (ContentMemberPath != null) {
                if (e.NewValue != null) {
                    Type type = e.NewValue.GetType();
                    var pi = type.GetProperty(ContentMemberPath);
                    if (pi != null) SelectedContent = pi.GetValue(e.NewValue, null);
                }
            } else if (e.NewValue is HierarchicalTabItem) {
                var item = e.NewValue as HierarchicalTabItem;
                SelectedContent = item.Content;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            if (e.Key == Key.PageDown) return;
            if (e.Key == Key.PageUp) return;
            base.OnKeyDown(e);
        }

        #region SelectedContent
        //--------------------------------------------------------------------------------
        public object SelectedContent {
            get { return (object)GetValue(SelectedContentProperty);}
            set { SetValue(SelectedContentProperty, value); }
        }

        public static readonly DependencyProperty SelectedContentProperty =
            DependencyProperty.Register("SelectedContent", typeof(object), typeof(HierarchicalTabControl), new UIPropertyMetadata(null));
        //--------------------------------------------------------------------------------
        #endregion SelectedContent

        #region NavAreaWidth
        //--------------------------------------------------------------------------------
        public GridLength NavAreaWidth {
            get { return (GridLength)GetValue(NavAreaWidthProperty); }
            set { SetValue(NavAreaWidthProperty, value); }
        }

        public static readonly DependencyProperty NavAreaWidthProperty =
            DependencyProperty.Register("NavAreaWidth", typeof(GridLength), typeof(HierarchicalTabControl), new UIPropertyMetadata(new GridLength(250)));
        //--------------------------------------------------------------------------------
        #endregion NavAreaWidth

        #region NavAreaMaxWidth
        //--------------------------------------------------------------------------------
        public double NavAreaMaxWidth {
            get { return (double)GetValue(NavAreaMaxWidthProperty); }
            set { SetValue(NavAreaMaxWidthProperty, value); }
        }

        public static readonly DependencyProperty NavAreaMaxWidthProperty =
            DependencyProperty.Register("NavAreaMaxWidth", typeof(double), typeof(HierarchicalTabControl), new UIPropertyMetadata((double)800));
        //--------------------------------------------------------------------------------
        #endregion NavAreaMaxWidth

        #region NavAreaExpanded
        //--------------------------------------------------------------------------------
        public bool NavAreaExpanded {
            get { return (bool)GetValue(NavAreaExpandedProperty); }
            set { SetValue(NavAreaExpandedProperty, value); }
        }

        public static readonly DependencyProperty NavAreaExpandedProperty =
            DependencyProperty.Register("NavAreaExpanded", typeof(bool), typeof(HierarchicalTabControl), new UIPropertyMetadata(true));

        //--------------------------------------------------------------------------------
        #endregion NavAreaExpanded

        #region HideExpanderButton
        public static readonly DependencyProperty HideExpanderButtonProperty =
            DependencyProperty.Register("HideExpanderButton", typeof(bool), typeof(HierarchicalTabControl), new PropertyMetadata(default(bool)));

        public bool HideExpanderButton {
            get {
                return (bool)GetValue(HideExpanderButtonProperty);
            }
            set {
                SetValue(HideExpanderButtonProperty, value);
            }
        }
        #endregion HideExpanderButton

        #region ContentMemberPath
        //--------------------------------------------------------------------------------
        public string ContentMemberPath {
            get { return (string)GetValue(ContentMemberPathProperty); }
            set { SetValue(ContentMemberPathProperty, value); }
        }

        public static readonly DependencyProperty ContentMemberPathProperty =
            DependencyProperty.Register("ContentMemberPath", typeof(string), typeof(HierarchicalTabControl), new UIPropertyMetadata(null));
       
        //--------------------------------------------------------------------------------
        #endregion ContentMemberPath
       

    }
}
