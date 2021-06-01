using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Config.DbStructure;
using Config.Interfaces.Config;
using Config.Interfaces.DbStructure;
using Utils;

namespace TransDATA.Models.ConfigModels {
    //public class SelectGdpduOutputModel : NotifyPropertyChangedBase{
    //    #region Constructor
    //    public SelectGdpduOutputModel(IProfile profile) {
    //        _profile = profile;
    //        OutputConfig = profile.OutputConfig.Config as IGdpduConfig;
    //        _profile.OutputConfig.PropertyChanged += OutputConfig_PropertyChanged;
    //    }
    //    #endregion Constructor

    //    void OutputConfig_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
    //        if (e.PropertyName == "Config")
    //            OutputConfig = _profile.OutputConfig.Config as IGdpduConfig;
    //    }

    //    #region Properties
    //    private IProfile _profile;
    //    #region OutputConfig
    //    private IGdpduConfig _outputConfig;

    //    public IGdpduConfig OutputConfig {
    //        get { return _outputConfig; }
    //        set {
    //            if (_outputConfig != value) {
    //                _outputConfig = value;
    //                OnPropertyChanged("OutputConfig");
    //            }
    //        }
    //    }
    //    #endregion OutputConfig
    //    #endregion Properties

    //    #region Methods
    //    #endregion Methods
    public class SelectGdpduOutputModel : OutputModelBase<IGdpduConfig> {
        public SelectGdpduOutputModel(IProfile profile) :base(profile) {
        }
        
    }
    
}
