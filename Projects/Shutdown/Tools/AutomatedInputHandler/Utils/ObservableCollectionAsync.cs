using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;
using System.Collections.Specialized;
using System.Collections.Generic;
using log4net;

namespace Utils {
    public class ObservableCollectionAsync<T> : ObservableCollection<T>, IEnumerable<T> {


        public ObservableCollectionAsync(IEnumerable<T> collection)
            : base(collection)
        {
        
        }


        public ObservableCollectionAsync(List<T> list)
            : base(list)
        {

        }

        public ObservableCollectionAsync():base()
        {
            
        }

        public override event NotifyCollectionChangedEventHandler CollectionChanged;
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
            using (BlockReentrancy()) {

                    System.Collections.Specialized.NotifyCollectionChangedEventHandler eventHandler = CollectionChanged;
                    if (eventHandler == null)
                        return;
                
                    var delegates = eventHandler.GetInvocationList();
                    // Walk thru invocation list
                    foreach (NotifyCollectionChangedEventHandler handler in delegates)
                    {
                        var dispatcherObject = handler.Target as DispatcherObject;
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

    }
}
