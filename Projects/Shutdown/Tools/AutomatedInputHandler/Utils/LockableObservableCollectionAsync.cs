using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace Utils
{
    /// <summary>
    /// Extends ObservableCollectionAsync<T>
    /// The OnCollectionChanged event can be enabled/disabled by locking/unlocking the collection
    /// The elements of the collection must also be ILockable in order to be able to lock/unlock them
    /// The locking mechanism of the elements can be anything, for example disabling/enabling the OnPropertyChanged event on them
    /// </summary>
    /// <typeparam name="T">The type of the elements of the collection</typeparam>
    public class LockableObservableCollectionAsync<T> : ObservableCollectionAsync<T>, ILockable where T : ILockable
    {
        public LockableObservableCollectionAsync(IEnumerable<T> collection)
            : base(collection)
        {
            Locked = false;
        }


        public LockableObservableCollectionAsync(List<T> list)
            : base(list)
        {
            Locked = false;
        }

        public LockableObservableCollectionAsync()
            : base()
        {
            Locked = false;
        }

        private bool _locked;
        public bool Locked
        {
            get { return _locked; }
            set
            {
                _locked = value;
                Items.ToList().ForEach(item => item.Locked = value);
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!Locked)
            {
                base.OnCollectionChanged(e);
            }
        }
    }
}
