using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvdCommon.Rules;
using AvdCommon.Rules.Gui.DragDrop;
using Utils;

namespace DbSearch.Models.Rules {
    public class RuleListBoxModel : NotifyPropertyChangedBase {
        public RuleListBoxModel(bool deleteOnFinished = false) {
            DragDropData = new RuleListDragDropData();
            _deleteOnFinished = deleteOnFinished;
        }

        #region Properties

        private Rule _selectedRule;
        public Rule SelectedRule {
            get { return _selectedRule; }
            set {
                if (_selectedRule != value) {
                    _selectedRule = value;
                    OnPropertyChanged("SelectedRule");
                }
            }
        }

        public RuleListDragDropData DragDropData { get; set; }
        private readonly bool _deleteOnFinished;

        #region Rules
        private RuleSet _rules;
        public RuleSet Rules {
            get { return _rules; }
            set {
                if (_rules != value) {
                    _rules = value;
                    OnPropertyChanged("Rules");
                }
            }
        }


        #endregion Rules


        #endregion Properties

        #region Methods
        public void ClearDragDropData() {
            DragDropData.Clear();
            DragDropData.SourceRuleSet = Rules;
            DragDropData.DeleteRuleOnFinished = _deleteOnFinished;
        }
        #endregion
    }
}
