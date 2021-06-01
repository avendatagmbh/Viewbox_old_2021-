using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Business;
using Config.Config;
using Config.Enums;
using Config.Interfaces.Config;
using Config.Interfaces.DbStructure;
using Utils;
using System.Windows;

namespace TransDATA.Models {
    internal class HistorySelectorModel : NotifyPropertyChangedBase {
        public enum HistorySelectorModeEnum {
            InputMode,
            OutputMode
        };

        private HistorySelectorModeEnum _historySelectorMode;

        public HistorySelectorModel(HistorySelectorModeEnum historySelectorMode, IProfile profile) {
            _profile = profile;
            _historySelectorMode = historySelectorMode;
        }

        #region Items
        public IEnumerable<IProfile> Items {
            get {
                IEnumerable<IProfile> ret = AppController.ProfileManager.VisibleProfiles.Where(
                    itm => itm.Id != _profile.Id);

                if (_historySelectorMode == HistorySelectorModeEnum.OutputMode)
                    return ret.Where(
                        itm => itm.Id != _profile.Id &&
                               (_profile.OutputConfig.Type.ToString().ToLower() ==
                                itm.OutputConfig.Type.ToString().ToLower()
                               ));


                if (_historySelectorMode == HistorySelectorModeEnum.InputMode)
                    return ret.Where(
                        itm => itm.Id != _profile.Id &&
                               (_profile.InputConfig.Type.ToString().ToLower() ==
                                itm.InputConfig.Type.ToString().ToLower()
                               ));

                return null;
            }
        }
        #endregion Items

        private IProfile _profile;

        #region SelectedItem
        private IProfile _selectedItem;

        public IProfile SelectedItem {
            get { return _selectedItem; }
            set {
                _selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }
        #endregion SelectedItem

        public void CopyProfile() {
            if (_selectedItem == null || _profile==null)
                return;

            if (_historySelectorMode == HistorySelectorModeEnum.OutputMode) {
                if (_selectedItem.OutputConfig.Type.ToString().ToLower() == _profile.OutputConfig.Type.ToString().ToLower()) {
                    //copy _selectedItem.OutputConfig TO _profile.OutputConfig
                    string  tmpxml = _selectedItem.OutputConfig.Config.GetXmlRepresentation();
                    Type  tmptype = _selectedItem.OutputConfig.Config.GetType();
                    _profile.OutputConfig.Config = (IConfig)Activator.CreateInstance(tmptype, new object[] { tmpxml });
                }
            }
            if (_historySelectorMode == HistorySelectorModeEnum.InputMode)
            {
                if (_selectedItem.InputConfig.Type.ToString().ToLower() == _profile.InputConfig.Type.ToString().ToLower())
                {
                    //copy _selectedItem.InputConfig TO _profile.InputConfig
                    string tmpxml = _selectedItem.InputConfig.Config.GetXmlRepresentation();
                    Type tmptype = _selectedItem.InputConfig.Config.GetType();
                    _profile.InputConfig.Config = (IConfig)Activator.CreateInstance(tmptype, new object[] { tmpxml });
                }
            }
        }
    }
}