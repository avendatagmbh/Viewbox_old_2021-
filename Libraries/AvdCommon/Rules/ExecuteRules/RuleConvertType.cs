using System;

namespace AvdCommon.Rules.ExecuteRules
{
    public class RuleConvertType : ExecuteRule
    {
        #region Types enum

        public enum Types
        {
            Int,
            Double,
            DateTime,
            Time,
            Date
        }

        #endregion

        public RuleConvertType(Types type)
        {
            Type = type;
            Name = "Konvertieren in ";
            switch (Type)
            {
                case Types.Int:
                    Name += "Integer";
                    break;
                case Types.Double:
                    Name += "Double";
                    break;
                case Types.DateTime:
                    Name += "Datum+Zeit";
                    break;
                case Types.Time:
                    Name += "Zeit";
                    break;
                case Types.Date:
                    Name += "Datum";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            UniqueName = "ConvertTo" + Type.ToString();
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
                case Types.Int:
                    long intOutput;
                    if (Int64.TryParse(value, out intOutput))
                        return intOutput.ToString();
                    break;
                case Types.Double:
                    double doubleOutput;
                    if (Double.TryParse(value, out doubleOutput))
                        return doubleOutput.ToString();
                    break;
                case Types.DateTime:
                    DateTime dateTimeOutput;
                    if (DateTime.TryParse(value, out dateTimeOutput))
                        return dateTimeOutput.ToString();
                    break;
                case Types.Time:
                    DateTime timeOutput;
                    if (DateTime.TryParse(value, out timeOutput))
                        return timeOutput.ToString("HH:mm:ss");
                    break;
                case Types.Date:
                    DateTime dateOutput;
                    if (DateTime.TryParse(value, out dateOutput))
                        return dateOutput.ToString("dd.MM.yyyy");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return value;
        }

        public override bool Equals(Rule rule)
        {
            if ((object) rule == null)
            {
                return false;
            }
            RuleConvertType ruleOther = rule as RuleConvertType;
            if (ruleOther == null) return false;
            return Type == ruleOther.Type && base.Equals(rule);
        }

        public override void CopyParametersTo(Rule rule)
        {
            base.CopyParametersTo(rule);
            RuleConvertType ruleOther = rule as RuleConvertType;
            if (ruleOther == null) return;
            ruleOther.Type = Type;
        }
    }
}