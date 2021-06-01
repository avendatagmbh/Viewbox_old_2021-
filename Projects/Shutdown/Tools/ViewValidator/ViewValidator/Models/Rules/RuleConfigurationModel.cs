// -----------------------------------------------------------
// Created by Benjamin Held - 30.08.2011 10:27:29
// Copyright AvenDATA 2011
// -----------------------------------------------------------

using AvdCommon.Rules;

namespace ViewValidator.Models.Rules {
    public class RuleConfigurationModel {
        #region Properties
        private Rule Rule { get; set; }
        #endregion

        #region Constructor
        public RuleConfigurationModel(Rule rule) {
            Rule = rule;
        }
        #endregion
    }
}
