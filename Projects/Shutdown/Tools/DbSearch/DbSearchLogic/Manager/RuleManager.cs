using AvdCommon.Rules;
using AvdCommon.Rules.ExecuteRules;
using DbSearchLogic.Config;

namespace DbSearchLogic.Manager {
    public class RuleManager {
        public static void FillProfileRules(RuleSet profileRules) {
            profileRules.AddRule(new RuleEliminateSpaces());
            profileRules.AddRule(new RuleConvertType(RuleConvertType.Types.Double));
            profileRules.AddRule(new RuleConvertType(RuleConvertType.Types.DateTime));
            profileRules.AddRule(new RuleConvertType(RuleConvertType.Types.Time));
            profileRules.AddRule(new RuleConvertType(RuleConvertType.Types.Date));
            profileRules.AddRule(new RuleChangeCase(RuleChangeCase.Types.ToLower));
            profileRules.AddRule(new RuleChangeCase(RuleChangeCase.Types.ToUpper));
            profileRules.AddRule(new RuleTrimSpaces());
            profileRules.AddRule(new RuleWordToUpper());
        }

        public static void SetProfileRules(Profile profile) {
            if (profile == null) return;
            FillProfileRules(profile.CustomRules);
        }
    }
}
