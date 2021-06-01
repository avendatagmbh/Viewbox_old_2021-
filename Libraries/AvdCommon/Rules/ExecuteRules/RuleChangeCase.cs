using System;

namespace AvdCommon.Rules.ExecuteRules
{
    public class RuleChangeCase : ExecuteRule
    {
        #region Types enum

        public enum Types
        {
            ToUpper,
            ToLower,
        }

        #endregion

        public RuleChangeCase(Types type)
        {
            Type = type;
            switch (Type)
            {
                case Types.ToUpper:
                    Name = "Groﬂ schreiben";
                    break;
                case Types.ToLower:
                    Name = "Klein schreiben";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            UniqueName = "Case" + Type.ToString();
        }

        private Types Type { get; set; }

        public override bool HasParameter
        {
            get { return false; }
        }

        public override string Execute(string value)
        {
            switch (Type)
            {
                case Types.ToLower:
                    return value.ToLower();
                case Types.ToUpper:
                    return value.ToUpper();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override bool Equals(Rule rule)
        {
            if ((object) rule == null)
            {
                return false;
            }
            RuleChangeCase ruleOther = rule as RuleChangeCase;
            if (ruleOther == null) return false;
            return Type == ruleOther.Type && base.Equals(rule);
        }

        public override void CopyParametersTo(Rule rule)
        {
            base.CopyParametersTo(rule);
            RuleChangeCase ruleOther = rule as RuleChangeCase;
            if (ruleOther == null) return;
            ruleOther.Type = Type;
        }
    }
}