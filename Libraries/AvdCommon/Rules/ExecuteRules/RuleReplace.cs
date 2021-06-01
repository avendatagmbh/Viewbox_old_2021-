using System;
using System.Text.RegularExpressions;
using System.Xml;

namespace AvdCommon.Rules.ExecuteRules
{
    public class RuleReplace : ExecuteRule
    {
        #region Properties

        public override bool HasParameter
        {
            get { return true; }
        }

        public bool UseRegularExpression { get; set; }
        public bool CaseSensitive { get; set; }

        #region NewString

        private string _oldString;

        public string OldString
        {
            get { return _oldString; }
            set
            {
                if (_oldString != value)
                {
                    _oldString = value;
                    SetName();
                }
            }
        }

        #endregion

        #region NewString

        private string _newString;

        public string NewString
        {
            get { return _newString; }
            set
            {
                if (_newString != value)
                {
                    _newString = value;
                    SetName();
                }
            }
        }

        #endregion

        #endregion Properties

        public RuleReplace()
        {
            SetName();
            UniqueName = "RuleReplace";
            OldString = string.Empty;
            NewString = string.Empty;
        }

        private void SetName()
        {
            Name = "Ersetze (" + OldString + "," + NewString + ")";
        }

        public override string Execute(string value)
        {
            //if (string.IsNullOrEmpty(OldString)) return value;
            if (UseRegularExpression)
            {
                Regex rgx = new Regex(OldString, CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
                return rgx.Replace(value, NewString);
            }
            if (CaseSensitive) return value.Replace(OldString, NewString);
            return ReplaceCaseInsensitive(value, OldString, NewString);
        }

        //public override object Clone() {
        //    return new RuleReplace() { IsSelected = this.IsSelected, OldString = OldString, NewString = NewString};
        //}
        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);
            writer.WriteAttributeString("Regex", UseRegularExpression.ToString());
            writer.WriteAttributeString("CaseSensitive", CaseSensitive.ToString());
            writer.WriteAttributeString("OldStr", OldString);
            writer.WriteAttributeString("NewStr", NewString);
        }

        public override void ReadXml(XmlNode node)
        {
            base.ReadXml(node);
            UseRegularExpression = Convert.ToBoolean(node.Attributes["Regex"].Value);
            CaseSensitive = Convert.ToBoolean(node.Attributes["CaseSensitive"].Value);
            OldString = node.Attributes["OldStr"].Value;
            NewString = node.Attributes["NewStr"].Value;
        }

        public override bool Equals(Rule rule)
        {
            if ((object) rule == null)
            {
                return false;
            }
            RuleReplace ruleOther = rule as RuleReplace;
            if (ruleOther == null) return false;
            return UseRegularExpression == ruleOther.UseRegularExpression && OldString == ruleOther.OldString &&
                   NewString == ruleOther.NewString && CaseSensitive == ruleOther.CaseSensitive && base.Equals(rule);
        }

        public override void CopyParametersTo(Rule rule)
        {
            base.CopyParametersTo(rule);
            RuleReplace ruleOther = rule as RuleReplace;
            if (ruleOther == null) return;
            ruleOther.UseRegularExpression = UseRegularExpression;
            ruleOther.OldString = OldString;
            ruleOther.NewString = NewString;
        }

        #region ReplaceEx

        //Taken from http://www.codeproject.com/KB/string/fastestcscaseinsstringrep.aspx
        private static string ReplaceCaseInsensitive(string original,
                                                     string pattern, string replacement)
        {
            if (replacement == null) replacement = string.Empty;
            if (pattern == null) pattern = string.Empty;
            int count, position0, position1;
            count = position0 = position1 = 0;
            string upperString = original.ToUpper();
            string upperPattern = pattern.ToUpper();
            int inc = (original.Length/pattern.Length)*
                      (replacement.Length - pattern.Length);
            char[] chars = new char[original.Length + Math.Max(0, inc)];
            while ((position1 = upperString.IndexOf(upperPattern,
                                                    position0)) != -1)
            {
                for (int i = position0; i < position1; ++i)
                    chars[count++] = original[i];
                for (int i = 0; i < replacement.Length; ++i)
                    chars[count++] = replacement[i];
                position0 = position1 + pattern.Length;
            }
            if (position0 == 0) return original;
            for (int i = position0; i < original.Length; ++i)
                chars[count++] = original[i];
            return new string(chars, 0, count);
        }

        #endregion
    }
}