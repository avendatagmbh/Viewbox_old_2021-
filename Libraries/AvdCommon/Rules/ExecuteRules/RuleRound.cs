using System;
using System.Xml;

namespace AvdCommon.Rules.ExecuteRules
{
    internal class RuleRound : ExecuteRule
    {
        public RuleRound()
        {
            SetName();
            UniqueName = "Round";
            RoundToDecimal = 2;
        }

        #region Properties

        public override bool HasParameter
        {
            get { return true; }
        }

        #region TrimCount

        private int _roundToDecimal;

        public int RoundToDecimal
        {
            get { return _roundToDecimal; }
            set
            {
                _roundToDecimal = value;
                SetName();
            }
        }

        #endregion

        #endregion Properties

        private void SetName()
        {
            Name = "Runden auf " + RoundToDecimal + ". Stelle";
        }

        public override string Execute(string value)
        {
            decimal number;
            if (!decimal.TryParse(value, out number))
            {
                return value;
            }
            return Math.Round(number, RoundToDecimal).ToString();
        }

        //public override object Clone() {
        //    return new RuleTrim() { IsSelected = this.IsSelected, FromLeft = FromLeft, TrimCount = TrimCount};
        //}
        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);
            writer.WriteAttributeString("RoundToDecimal", RoundToDecimal.ToString());
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            RoundToDecimal = Convert.ToInt32(node.Attributes["RoundToDecimal"].Value);
        }

        public override bool Equals(Rule rule)
        {
            if ((object) rule == null)
            {
                return false;
            }
            RuleRound ruleOther = rule as RuleRound;
            if (ruleOther == null) return false;
            return RoundToDecimal == ruleOther.RoundToDecimal && base.Equals(rule);
        }

        public override void CopyParametersTo(Rule rule)
        {
            base.CopyParametersTo(rule);
            RuleRound ruleOther = rule as RuleRound;
            if (ruleOther == null) return;
            ruleOther.RoundToDecimal = RoundToDecimal;
        }
    }
}