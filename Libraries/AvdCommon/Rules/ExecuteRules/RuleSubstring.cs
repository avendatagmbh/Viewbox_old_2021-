using System;
using System.Xml;

namespace AvdCommon.Rules.ExecuteRules
{
    public class RuleSubstring : ExecuteRule
    {
        public RuleSubstring()
        {
            SetName();
            UniqueName = "Substring";
        }

        #region Properties

        public override bool HasParameter
        {
            get { return true; }
        }

        #region FromPos

        private int _fromPos;

        public int FromPos
        {
            get { return _fromPos; }
            set
            {
                _fromPos = value;
                SetName();
            }
        }

        #endregion

        #region ToPos

        private int _toPos;

        public int ToPos
        {
            get { return _toPos; }
            set
            {
                _toPos = value;
                SetName();
            }
        }

        #endregion

        #endregion Properties

        private void SetName()
        {
            Name = "Substring (" + FromPos + "," + ToPos + ")";
        }

        public override string Execute(string value)
        {
            if (FromPos < value.Length && ToPos < value.Length)
                return value.Substring(FromPos, ToPos - FromPos + 1);
            if (FromPos >= value.Length) return "";
            return value.Substring(FromPos, Math.Min(ToPos - FromPos + 1, value.Length - FromPos));
            //return value;
        }

        //public override object Clone() {
        //    return new RuleSubstring() { IsSelected = this.IsSelected, FromPos = FromPos, ToPos = ToPos};
        //}
        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);
            writer.WriteAttributeString("FromPos", FromPos.ToString());
            writer.WriteAttributeString("ToPos", ToPos.ToString());
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            FromPos = Convert.ToInt32(node.Attributes["FromPos"].Value);
            ToPos = Convert.ToInt32(node.Attributes["ToPos"].Value);
        }

        public override bool Equals(Rule rule)
        {
            if ((object) rule == null)
            {
                return false;
            }
            RuleSubstring ruleOther = rule as RuleSubstring;
            if (ruleOther == null) return false;
            return FromPos == ruleOther.FromPos && ToPos == ruleOther.ToPos && base.Equals(rule);
        }

        public override void CopyParametersTo(Rule rule)
        {
            base.CopyParametersTo(rule);
            RuleSubstring ruleOther = rule as RuleSubstring;
            if (ruleOther == null) return;
            ruleOther.FromPos = FromPos;
            ruleOther.ToPos = ToPos;
        }
    }
}