using System.Collections.Specialized;
using System.Windows;

namespace Utils.DataBinding {
    public class ReadOnlyBindingCollection : FreezableCollection<ReadOnlyBinding> {
        public ReadOnlyBindingCollection(FrameworkElement targetObject) {
            TargetObject = targetObject;
            ((INotifyCollectionChanged) this).CollectionChanged += CollectionChanged;
        }

        public FrameworkElement TargetObject { get; private set; }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.Action != NotifyCollectionChangedAction.Add) return;
            foreach (ReadOnlyBinding pushBinding in e.NewItems) {
                pushBinding.SetupTargetBinding(TargetObject);
            }
        }
    }
}