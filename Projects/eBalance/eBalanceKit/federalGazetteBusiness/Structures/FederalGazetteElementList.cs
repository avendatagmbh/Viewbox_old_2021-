// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-12-14
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using Utils;
using federalGazetteBusiness.Structures.ValueTypes;

namespace federalGazetteBusiness.Structures {
    /// <summary>
    /// A class that contains a list of different FederalGazette <see cref="Items"/>.
    /// Has to be <see cref="IFederalGazetteElementInfo"/>.
    /// </summary>
    public class FederalGazetteElementList : Utils.NotifyPropertyChangedBase {

        public FederalGazetteElementList() {
            ValidationErrors = new ObservableCollectionAsync<string>();
            _items = new List<IFederalGazetteElementInfo>();

        }
        public FederalGazetteElementList(IEnumerable<IFederalGazetteElementInfo> items) {
            ValidationErrors = new ObservableCollectionAsync<string>();
            _items = items.ToList();

            foreach (var elementInfo in _items) {
                elementInfo.PropertyChanged += (sender, args) => OnPropertyChanged(args.PropertyName);
            }
        }

        #region Items
        protected List<IFederalGazetteElementInfo> _items;

        //public IEnumerable<IFederalGazetteElementInfo> Items {
        //    get { return _items; }
        //    set {
        //        if (_items != value) {
        //            _items = value.ToList();
        //            //OnPropertyChanged("Items");
        //        }
        //    }
        //}
        #endregion // Items
        //public List<object> Items { get; set; } 
 //set { _items = value.ToList(); }
        public IEnumerable<IFederalGazetteElementInfo> Items { get { return _items; } }

        public void Add(IFederalGazetteElementInfo item) {
            item.PropertyChanged += (sender, args) => OnPropertyChanged(args.PropertyName);
            _items.Add(item);
            OnPropertyChanged("Items");
        }

        public void AddError(string message) {
            ValidationErrors.Add(message);
            OnPropertyChanged("IsValid");
        }

        public void ClearAllErrors() {
            ValidationErrors.Clear();
        }

        #region IsValid

        public bool IsValid {
            get { return !ValidationErrors.Any(); }
        }
        #endregion // IsValid

        public ObservableCollectionAsync<string> ValidationErrors { get; set; }
    }
}