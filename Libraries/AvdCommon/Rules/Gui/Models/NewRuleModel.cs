using System.Collections.ObjectModel;
using AvdCommon.Rules.Gui.Factories;

namespace AvdCommon.Rules.Gui.Models
{
    public class NewRuleModel
    {
        #region Constructor

        public NewRuleModel()
        {
            EditRuleModel = new EditRuleModel();
            if (PossibleNewRules.Count != 0)
            {
                SelectedRule = PossibleNewRules[0];
            }
        }

        #endregion Constructor

        #region Properties

        public ObservableCollection<Rule> PossibleNewRules
        {
            get { return RuleControlFactory.PossibleNewRules; }
        }

        public EditRuleModel EditRuleModel { get; set; }

        #region SelectedRule

        private Rule _selectedRule;

        public Rule SelectedRule
        {
            get { return _selectedRule; }
            set
            {
                if (_selectedRule != value)
                {
                    _selectedRule = value;
                    EditRuleModel.Rule = _selectedRule;
                }
            }
        }

        #endregion SelectedRule

        #endregion Properties
    }
}