// --------------------------------------------------------------------------------
// author: Marcus Gerlach
// since: 2012-01-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Config.Interfaces.Config;
using Config.Interfaces.DbStructure;
using Utils;

namespace TransDATA.Models.ConfigModels {
    public class SelectCsvOutputModel : OutputModelBase<ICsvOutputConfig> {
        //private IProfile _profile;

        public SelectCsvOutputModel(IProfile profile) : base(profile){
            //_profile = profile;
            //OutputConfig = profile.OutputConfig.Config as ICsvOutputConfig;
            //_profile.OutputConfig.PropertyChanged += OutputConfig_PropertyChanged;
        }

        //void OutputConfig_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
        //    if (e.PropertyName == "Config")
        //        OutputConfig = _profile.OutputConfig.Config as ICsvOutputConfig;
        //}

        //#region OutputConfig
        //private ICsvOutputConfig _outputConfig;

        //public ICsvOutputConfig OutputConfig {
        //    get { return _outputConfig; }
        //    set {
        //        if (_outputConfig != value) {
        //            _outputConfig = value;
        //            OnPropertyChanged("OutputConfig");
        //        }
        //    }
        //}
        //#endregion OutputConfig
    }
}