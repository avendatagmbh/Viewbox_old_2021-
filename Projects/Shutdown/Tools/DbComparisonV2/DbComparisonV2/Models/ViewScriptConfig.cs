using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace DbComparisonV2.Models
{
    [DataContract]
    public class ViewScriptConfig:ICompareConfig, INotifyPropertyChanged
    {

        #region Fields
        ObservableCollection<string> _scriptFiles = new ObservableCollection<string>();
        #endregion
        #region Constructor
        public ViewScriptConfig() : base() { }
        #endregion


        #region Properties
        [DataMember]
        public ObservableCollection<string> ScriptFiles
        {
            get { return _scriptFiles; }
            set {
                _scriptFiles = value; 
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("ScriptFiles"));
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
