using System;
using System.Windows;
using System.Windows.Controls;

namespace AvdWpfControls {

    /// <summary>
    /// class for a column definition that can be hided with the Visible property set to False.
    /// </summary>
    public class HidableColumnDefinition : ColumnDefinition {

        #region dependecyProperty
        public static DependencyProperty VisibleProperty;

        public Boolean Visible {
            get { return (Boolean)GetValue(VisibleProperty); }
            set { SetValue(VisibleProperty, value); }
        }
        #endregion

        #region constructor
        static HidableColumnDefinition() {
            VisibleProperty = DependencyProperty.Register("Visible",
                typeof(Boolean),
                typeof(HidableColumnDefinition),
                new PropertyMetadata(true, OnVisibleChanged));
            
            WidthProperty.OverrideMetadata(typeof(HidableColumnDefinition),
                new FrameworkPropertyMetadata(new GridLength(1, GridUnitType.Star), null,
                    CoerceWidth));
        }
        #endregion

        #region Get/Set
        public static void SetVisible(DependencyObject obj, Boolean nVisible) {
            obj.SetValue(VisibleProperty, nVisible);
        }

        public static Boolean GetVisible(DependencyObject obj) {
            return (Boolean)obj.GetValue(VisibleProperty);
        }

        static void OnVisibleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            obj.CoerceValue(WidthProperty);
        }

        static Object CoerceWidth(DependencyObject obj, Object nValue) {
            return (((HidableColumnDefinition)obj).Visible) ? nValue : new GridLength(0);
        }
        #endregion
    }
}
