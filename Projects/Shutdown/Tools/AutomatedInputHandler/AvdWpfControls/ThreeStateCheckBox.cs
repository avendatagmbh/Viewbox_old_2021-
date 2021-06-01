using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace AvdWpfControls {
    
    /// <summary>
    /// the class is little bit tricky. You have 2 options. If you add the checkbox on xaml file, than you can't highlight the checkbox
    /// (the IsHighlighted dependency property is useless). The other option is to create the instance programatically, but You have to add the element's
    /// MyBorder property in the container, not 'this'.
    /// </summary>
    public class ThreeStateCheckBox : CheckBox {
        public ThreeStateCheckBox() {
            // Always start in the 'Indeterminate' state
            //base.IsChecked = null;
            Initialized += OnInitialized;
        }

        private void OnInitialized(object sender, EventArgs eventArgs) {
            MyBorder = new Border {
                // default color of highlight.
                BorderBrush = Brushes.PowderBlue,
                SnapsToDevicePixels = true,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            // this is more safe implementation.
            // if we want to add IsHighlighted to not only programatically created ThreeStateCheckBoxes,
            // we can comment the next 3 line, and uncomment the rest, but we are not sure,
            // that all of the UIElement types are checked for children removal.
            // also we have to set some dependency property of the MyBorder, to have the same value as this's value.
            // we can't check if all the properties are counted.
            if (Parent == null) {
                MyBorder.Child = this;
            }
            //IAddChild addChild = Parent as IAddChild;
            //Panel panel = Parent as Panel;
            //if (Parent != null) {
            //    if (panel != null) {
            //        panel.Children.Remove(this);
            //    }
            //    Decorator decorator = Parent as Decorator;
            //    if (decorator != null) {
            //        decorator.Child = null;
            //    }
            //}
            //MyBorder.Child = this;
            //Grid.SetColumn(MyBorder, Grid.GetColumn(this));
            //Grid.SetRow(MyBorder, Grid.GetRow(this));
            //Grid.SetColumnSpan(MyBorder, Grid.GetColumnSpan(this));
            //Grid.SetRowSpan(MyBorder, Grid.GetRowSpan(this));
            //SetFlowDirection(MyBorder, GetFlowDirection(this));
            //Grid.SetIsSharedSizeScope(MyBorder, Grid.GetIsSharedSizeScope(this));
            //Panel.SetZIndex(MyBorder, Panel.GetZIndex(this));
            //if (addChild != null) {
            //    addChild.AddChild(MyBorder);
            //}

            if (Scale != 1.0)
            {
                LayoutTransform = new ScaleTransform(Scale, Scale);
            }
        }

        public static DependencyProperty ScaleProperty =
            DependencyProperty.Register(
                "Scale",
                typeof(double),
                typeof(ThreeStateCheckBox),
                new PropertyMetadata(1.0));

        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        public Border MyBorder { get; set; }

        private static readonly Thickness ThreeThick = new Thickness(3d);

        private static readonly Thickness NonThick = new Thickness(0d);

        public static readonly DependencyProperty IsHighlightedProperty =
            DependencyProperty.Register("IsHighlighted", typeof (bool), typeof (ThreeStateCheckBox), new PropertyMetadata(default(bool), PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
                if ((bool)dependencyPropertyChangedEventArgs.NewValue) {
                    ((ThreeStateCheckBox)dependencyObject).MyBorder.SetValue(Border.BorderThicknessProperty, ThreeThick);
                } else {
                    ((ThreeStateCheckBox)dependencyObject).MyBorder.SetValue(Border.BorderThicknessProperty, NonThick);
                }
            }

        public bool IsHighlighted { get { return (bool) GetValue(IsHighlightedProperty); } set { SetValue(IsHighlightedProperty, value); } }

        public event RoutedEventHandler Toggled;
        private void OnToggled() {
            if (Toggled != null) Toggled(this, new RoutedEventArgs());
        }

        protected override void OnToggle() {
            if (IsChecked == false) IsChecked = true;
            else if (IsChecked == true) IsChecked = null;
            else IsChecked = false;

            OnToggled();
        }
    }
}
