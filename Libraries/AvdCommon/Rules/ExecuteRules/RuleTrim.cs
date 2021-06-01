using System;
using System.Xml;

namespace AvdCommon.Rules.ExecuteRules
{
    public class RuleTrim : ExecuteRule
    {
        public RuleTrim()
        {
            SetName();
            UniqueName = "Trim";
            TrimCount = 2;
        }

        #region Properties

        public override bool HasParameter
        {
            get { return true; }
        }

        #region FromLeft

        private bool _fromLeft;

        public bool FromLeft
        {
            get { return _fromLeft; }
            set
            {
                _fromLeft = value;
                SetName();
            }
        }

        #endregion

        #region TrimCount

        private int _trimCount;

        public int TrimCount
        {
            get { return _trimCount; }
            set
            {
                _trimCount = value;
                SetName();
            }
        }

        #endregion

        #endregion Properties

        private void SetName()
        {
            Name = TrimCount + " Zeichen von " + (_fromLeft ? "links" : "rechts") + " abschneiden";
        }

        public override string Execute(string value)
        {
            if (TrimCount >= value.Length) return "";
            if (FromLeft)
            {
                return value.Substring(TrimCount);
            }
            return value.Substring(0, value.Length - TrimCount);
        }

        //public override object Clone() {
        //    return new RuleTrim() { IsSelected = this.IsSelected, FromLeft = FromLeft, TrimCount = TrimCount};
        //}
        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);
            writer.WriteAttributeString("FromLeft", FromLeft.ToString());
            writer.WriteAttributeString("TrimCount", TrimCount.ToString());
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            FromLeft = Convert.ToBoolean(node.Attributes["FromLeft"].Value);
            TrimCount = Convert.ToInt32(node.Attributes["TrimCount"].Value);
        }

        public override bool Equals(Rule rule)
        {
            if ((object) rule == null)
            {
                return false;
            }
            RuleTrim ruleOther = rule as RuleTrim;
            if (ruleOther == null) return false;
            return FromLeft == ruleOther.FromLeft && TrimCount == ruleOther.TrimCount && base.Equals(rule);
        }

        public override void CopyParametersTo(Rule rule)
        {
            base.CopyParametersTo(rule);
            RuleTrim ruleOther = rule as RuleTrim;
            if (ruleOther == null) return;
            ruleOther.FromLeft = FromLeft;
            ruleOther.TrimCount = TrimCount;
        }
    }
}