// --------------------------------------------------------------------------------
// author: Solueman Hussain
// since: 2011-12-30
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.ComponentModel;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKit.Windows.FederalGazette.Model {
    public class DlgFederalGazetteClientModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }

        #region NewFederalGazette
        private FederalGazetteInfo _newFederalGazette;
        public FederalGazetteInfo NewFederalGazette {
            get { return _newFederalGazette; }
            set {
                _newFederalGazette = value;
                OnPropertyChanged("NewFederalGazette");
            }
        }
        #endregion

        #region EditFederalGazette

        public FederalGazetteInfo EditFederalGazette {
            get { return NewFederalGazette ?? SelectedFederalGazette; }
        }
        #endregion

        #region SelectedFederalGazette
        public FederalGazetteInfo SelectedFederalGazette {
            get
            {
                if (FederalGazetteListSelectedItem == null || !(FederalGazetteListSelectedItem is FederalGazetteInfo)) return null;
                return FederalGazetteListSelectedItem as FederalGazetteInfo;
            }
        }
        #endregion

        #region FederalGazetteListSelectedItem
        private object _federalGazetteListSelectedItem;

        public object FederalGazetteListSelectedItem {
            get { return _federalGazetteListSelectedItem; }
            set {
                _federalGazetteListSelectedItem = value;
                OnPropertyChanged("FederalGazetteListSelectedItem");
                OnPropertyChanged("SelectedFederalGazette");
            }
        }
        #endregion



        //**********************************************************

        #region SaveEditedFederalGazette
        public void SaveEditedFederalGazette() {
            if (NewFederalGazette != null) {
                FederalGazetteInfoManager.AddFederalGazette(NewFederalGazette);
                FederalGazetteListSelectedItem = NewFederalGazette;
                NewFederalGazette = null;
            } else FederalGazetteInfoManager.UpdateFederalGazette(EditFederalGazette);
        }
        #endregion

        #region CancelFederalGazetteEdit
        public void CancelFederalGazetteEdit() { NewFederalGazette = null; }
        #endregion

        #region DeleteFederalGazette
        public void DeleteFederalGazette(FederalGazetteInfo federalGazetteInfo) {
            if (federalGazetteInfo != null) 
                FederalGazetteInfoManager.DeleteFederalGazette(federalGazetteInfo);
        }
        #endregion

    }
}