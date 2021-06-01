// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-10-26
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvdCommon.Rules;
using AvdCommon.Rules.Gui.DragDrop;
using Utils;

namespace DbSearch.Models.Rules {
    public class RuleListModel : NotifyPropertyChangedBase{
        #region Constructor
        public RuleListModel(MainWindowModel mainWindowModel) {
            _mainWindowModel = mainWindowModel;
            _mainWindowModel.PropertyChanged += _mainWindowModel_PropertyChanged;
            RuleListBoxModel = new RuleListBoxModel();

        }
        #endregion Constructor

        #region Properties
        private readonly MainWindowModel _mainWindowModel;
        public RuleListBoxModel RuleListBoxModel { get; private set; }
        #endregion Properties


        #region EventHandler
        void _mainWindowModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (_mainWindowModel.SelectedProfile == null) RuleListBoxModel.Rules = null;
            else RuleListBoxModel.Rules = _mainWindowModel.SelectedProfile.CustomRules;
        }
        #endregion EventHandler
    }
}
