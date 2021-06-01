using System.Collections.ObjectModel;
using System.Windows.Controls;
using AvdCommon.Rules.ExecuteRules;
using AvdCommon.Rules.Gui.Controls.RuleSpecific;

namespace AvdCommon.Rules.Gui.Factories
{
    public static class RuleControlFactory
    {
        static RuleControlFactory()
        {
            PossibleNewRules = new ObservableCollection<Rule>
                                   {
                                       new RuleSubstring(),
                                       new RuleReplace(),
                                       new RuleTrim(),
                                       new RuleEliminateLeadingZeros(),
                                       new RuleComparisonTrueForValue(),
                                       new RuleRound()
                                   };
        }

        public static ObservableCollection<Rule> PossibleNewRules { get; private set; }

        public static UserControl UserControlFromRule(Rule rule)
        {
            switch (rule.GetType().Name.ToLower())
            {
                case "rulesubstring":
                    return new CtlRuleConfigSubstring();
                case "rulereplace":
                    return new CtlRuleConfigReplace();
                case "ruletrim":
                    return new CtlRuleConfigTrim();
                case "ruleeliminateleadingzeros":
                    return new CtlRuleConfigEliminateLeadingZeros();
                case "rulecomparisontrueforvalue":
                    return new CtlRuleComparisonTrueForValue();
                case "ruleround":
                    return new CtlRuleConfigRound();
                default:
                    return new CtlRuleNoParameters();
                    //throw new ArgumentException("No configuration dialog for this rule.");
            }
        }
    }
}