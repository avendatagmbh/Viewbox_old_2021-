using System.Xml;

namespace AvdCommon.Rules.ExecuteRules
{
    public class RuleComparisonTrueForValue : ExecuteRule
    {
        public RuleComparisonTrueForValue()
        {
            SetName();
            UniqueName = "TrueForValue";
            _isSpecialRule = true;
        }

        #region Properties

        public override bool HasParameter
        {
            get { return true; }
        }

        public override string SpecialValue
        {
            get { return TrueValue; }
        }

        #region TrueValue

        private string _trueValue;

        public string TrueValue
        {
            get { return _trueValue; }
            set
            {
                _trueValue = value;
                SetName();
            }
        }

        #endregion

        #endregion Properties

        private void SetName()
        {
            Name = "Wert ist immer korrekt (" + TrueValue + ")";
        }

        public override string Execute(string value)
        {
            return value;
        }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);
            writer.WriteAttributeString("TrueValue", TrueValue);
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            TrueValue = node.Attributes["TrueValue"].Value;
        }

        public override bool Equals(Rule rule)
        {
            if ((object) rule == null)
            {
                return false;
            }
            RuleComparisonTrueForValue ruleOther = rule as RuleComparisonTrueForValue;
            if (ruleOther == null) return false;
            return TrueValue == ruleOther.TrueValue && base.Equals(rule);
        }

        public override void CopyParametersTo(Rule rule)
        {
            base.CopyParametersTo(rule);
            RuleComparisonTrueForValue ruleOther = rule as RuleComparisonTrueForValue;
            if (ruleOther == null) return;
            ruleOther.TrueValue = TrueValue;
        }
    }
}