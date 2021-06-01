using System;
using System.Xml;

namespace AvdCommon.Rules.ExecuteRules
{
    public class RuleEliminateLeadingZeros : ExecuteRule
    {
        public RuleEliminateLeadingZeros()
        {
            Name = "Führende Nullen entfernen";
            UniqueName = "TrimLeadingZeros";
        }

        public bool DeleteLastZero { get; set; }

        public override bool HasParameter
        {
            get { return true; }
        }

        public override string Execute(string value)
        {
            if (value == "") return "";
            string trimmedValue = value.TrimStart(new[] {'0'});
            if (!DeleteLastZero && trimmedValue == "") return "0";
            return trimmedValue;
        }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);
            writer.WriteAttributeString("DeleteLastZero", DeleteLastZero.ToString());
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            if (node.Attributes["DeleteLastZero"] != null)
                DeleteLastZero = Convert.ToBoolean(node.Attributes["DeleteLastZero"].Value);
        }

        public override bool Equals(Rule rule)
        {
            if ((object) rule == null)
            {
                return false;
            }
            RuleEliminateLeadingZeros ruleOther = rule as RuleEliminateLeadingZeros;
            if (ruleOther == null) return false;
            return DeleteLastZero == ruleOther.DeleteLastZero && base.Equals(rule);
        }

        public override void CopyParametersTo(Rule rule)
        {
            base.CopyParametersTo(rule);
            RuleEliminateLeadingZeros ruleOther = rule as RuleEliminateLeadingZeros;
            if (ruleOther == null) return;
            ruleOther.DeleteLastZero = DeleteLastZero;
        }
    }
}