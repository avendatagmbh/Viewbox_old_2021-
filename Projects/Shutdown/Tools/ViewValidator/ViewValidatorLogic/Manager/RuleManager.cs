// -----------------------------------------------------------
// Created by Benjamin Held - 15.08.2011 14:35:10
// Copyright AvenDATA 2011
// -----------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AvdCommon.Rules;
using AvdCommon.Rules.ExecuteRules;
using AvdCommon.Rules.SortRules;
using ViewValidatorLogic.Config;

namespace ViewValidatorLogic.Manager {
    public class RuleManager {
        #region Constructor
        private RuleManager() {
        }

        #endregion

        #region Properties

        #region Instance
        private static readonly RuleManager _instance = new RuleManager();
        public static RuleManager Instance { get { return _instance; } }
        #endregion

        #endregion

        #region Methods
        private void FillProfileRules(RuleSet profileRules) {
            profileRules.AddRule(new RuleEliminateLeadingZeros());
            profileRules.AddRule(new RuleEliminateSpaces());
            profileRules.AddRule(new RuleDbNullToEmptyString());
            profileRules.AddRule(new RuleConvertType(RuleConvertType.Types.Int));
            profileRules.AddRule(new RuleConvertType(RuleConvertType.Types.Double));
            profileRules.AddRule(new RuleConvertType(RuleConvertType.Types.DateTime));
            profileRules.AddRule(new RuleConvertType(RuleConvertType.Types.Time));
            profileRules.AddRule(new RuleConvertType(RuleConvertType.Types.Date));
            profileRules.AddRule(new RuleChangeCase(RuleChangeCase.Types.ToLower));
            profileRules.AddRule(new RuleChangeCase(RuleChangeCase.Types.ToUpper));
            profileRules.AddRule(new RuleTrimSpaces());
            profileRules.AddRule(new RuleWordToUpper());

            profileRules.AddRule(new RuleSortString());
            profileRules.AddRule(new RuleSortNumeric());
            //profileRules.AddRule(new RuleSortDateTime());
        }
            
        
        public void SetProfileRules(ProfileConfig profile) {
            if (profile == null) return;
            FillProfileRules(profile.CustomRules);
        }
        #endregion
    }
}
