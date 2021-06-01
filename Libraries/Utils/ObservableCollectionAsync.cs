using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;

namespace Utils
{
    public class ObservableCollectionAsync<T> : ObservableCollection<T>
    {
        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            using (BlockReentrancy())
            {
                NotifyCollectionChangedEventHandler eventHandler = CollectionChanged;
                if (eventHandler == null)
                    return;
                Delegate[] delegates = eventHandler.GetInvocationList();
                // Walk thru invocation list
                foreach (NotifyCollectionChangedEventHandler handler in delegates)
                {
                    DispatcherObject dispatcherObject = handler.Target as DispatcherObject;
                    // If the subscriber is a DispatcherObject and different thread
                    if (dispatcherObject != null && dispatcherObject.CheckAccess() == false)
                    {
                        // Invoke handler in the target dispatcher's thread
                        dispatcherObject.Dispatcher.Invoke(DispatcherPriority.DataBind, handler, this, e);
                    }
                    else // Execute handler as is
                        handler(this, e);
                }
            }
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
        }

        public void AddRange(IEnumerable<T> dataToAdd)
        {
            CheckReentrancy();
            //
            // We need the starting index later
            //
            int startingIndex = Count;
            //
            // Add the items directly to the inner collection
            //
            foreach (var data in dataToAdd)
            {
                Items.Add(data);
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}