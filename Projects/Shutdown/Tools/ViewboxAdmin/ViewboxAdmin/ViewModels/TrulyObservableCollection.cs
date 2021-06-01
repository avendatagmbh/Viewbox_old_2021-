using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ViewboxAdmin.ViewModels
{
    /// <summary>
    /// http://stackoverflow.com/questions/1427471/observablecollection-not-noticing-when-item-in-it-changes-even-with-inotifyprop
    /// this subclass report collection change as one of the items changed. The default observable collection report collectionchanged only if an item added or deleted...
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TrulyObservableCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        public TrulyObservableCollection()
            : base() {
            CollectionChanged += new NotifyCollectionChangedEventHandler(TrulyObservableCollection_CollectionChanged);
        }

        void TrulyObservableCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null)
            {
                foreach (Object item in e.NewItems) {
                    INotifyPropertyChanged iItem = item as INotifyPropertyChanged;
                    if (iItem != null)
                        iItem.PropertyChanged += new PropertyChangedEventHandler(item_PropertyChanged);
                }
            }
            if (e.OldItems != null)
            {
                foreach (Object item in e.OldItems) {
                    INotifyPropertyChanged iItem = item as INotifyPropertyChanged;
                    if (iItem != null)
                        iItem.PropertyChanged -= new PropertyChangedEventHandler(item_PropertyChanged);
                }
            }
        }

        void item_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            NotifyCollectionChangedEventArgs a = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(a);
        }
    }

}
