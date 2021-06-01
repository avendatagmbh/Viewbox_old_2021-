using System.Windows;

namespace Utils.DataBinding {
    public class ReadOnlyBindingManager {
        public static DependencyProperty ReadOnlyBindingsInternalProperty =
            DependencyProperty.RegisterAttached("ReadOnlyBindingsInternal", typeof (ReadOnlyBindingCollection),
                                                typeof (ReadOnlyBindingManager), new UIPropertyMetadata(null));

        public static ReadOnlyBindingCollection GetBindings(FrameworkElement obj) {
            if (obj.GetValue(ReadOnlyBindingsInternalProperty) == null) {
                obj.SetValue(ReadOnlyBindingsInternalProperty, new ReadOnlyBindingCollection(obj));
            }
            return (ReadOnlyBindingCollection)obj.GetValue(ReadOnlyBindingsInternalProperty);
        }

        public static void SetBindings(FrameworkElement obj, ReadOnlyBindingCollection value) { obj.SetValue(ReadOnlyBindingsInternalProperty, value); }
    }
}