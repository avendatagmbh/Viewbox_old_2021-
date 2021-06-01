// --------------------------------------------------------------------------------
// author: Solueman Hussain
// since: 2012-01-05
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.ComponentModel;

namespace eBalanceKitBusiness.FederalGazette.Model {
    public class ClientsList : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        #region properties
        
        #region ClientId

        private string _clientId;

        public string ClientId {
            get { return _clientId; }
            set {
                if (value == _clientId) return;
                _clientId = value;
                OnPropertyChanged("ClientId");
            }
        }
        #endregion

        #region CompanySign
        
        private string _companySign;

        public string CompanySign
        {
            get { return _companySign; }
            set {
                if (value == _companySign) _companySign = value;
                OnPropertyChanged("CompanySign");
            }
        }
        #endregion

        #region CompanyName
        
        private string _companyName;

        public string CompanyName {
            get { return _companyName; }
            set {
                if (value == _companyName) return;
                _companyName = value;
                OnPropertyChanged("CompanyName");
            }
        }
        #endregion
        
        #region IsChecked
        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }
        #endregion

        #region IsSelected
        
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
        #endregion
        #endregion

        #region method

        public override string ToString() { return String.Format("{0} - {1}", ClientId, CompanyName); }
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion




    }
}