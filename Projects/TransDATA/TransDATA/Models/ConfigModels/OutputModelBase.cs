using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Config.Interfaces.Config;
using Config.Interfaces.DbStructure;
using Utils;

namespace TransDATA.Models.ConfigModels {
    public class OutputModelBase<T> : NotifyPropertyChangedBase where T:class,IConfig {
        #region Constructor
        public OutputModelBase(IProfile profile) {
            _profile = profile;
            OutputConfig = profile.OutputConfig.Config as T;
            _profile.OutputConfig.PropertyChanged += OutputConfig_PropertyChanged;
        }
        #endregion Constructor

        void OutputConfig_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "Config")
                OutputConfig = _profile.OutputConfig.Config as T;
        }

        #region Properties
        protected IProfile _profile;
        #region OutputConfig
        private T _outputConfig;

        public T OutputConfig {
            get { return _outputConfig; }
            set {
                _outputConfig = value;
                OnPropertyChanged("OutputConfig");
            }
        }
        #endregion OutputConfig
        #endregion Properties

        #region Methods
        #endregion Methods
    }
}
