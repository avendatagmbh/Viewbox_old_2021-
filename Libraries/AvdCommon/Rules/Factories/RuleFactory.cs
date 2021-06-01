using System;
using System.Xml;
using AvdCommon.Rules.ExecuteRules;
using AvdCommon.Rules.SortRules;

namespace AvdCommon.Rules.Factories
{
    public static class RuleFactory
    {
        public static Rule RuleFromUniqueName(string name)
        {
            Rule result;
            switch (name)
            {
                case "ConvertToInt":
                    result = new RuleConvertType(RuleConvertType.Types.Int);
                    break;
                case "ConvertToDouble":
                    result = new RuleConvertType(RuleConvertType.Types.Double);
                    break;
                case "ConvertToDateTime":
                    result = new RuleConvertType(RuleConvertType.Types.DateTime);
                    break;
                case "ConvertToTime":
                    result = new RuleConvertType(RuleConvertType.Types.Time);
                    break;
                case "ConvertToDate":
                    result = new RuleConvertType(RuleConvertType.Types.Date);
                    break;
                case "DbNullToEmpty":
                    result = new RuleDbNullToEmptyString();
                    break;
                case "TrimLeadingZeros":
                    result = new RuleEliminateLeadingZeros();
                    break;
                case "EliminateSpaces":
                    result = new RuleEliminateSpaces();
                    break;
                case "Substring":
                    result = new RuleSubstring();
                    break;
                case "RuleReplace":
                    result = new RuleReplace();
                    break;
                case "Trim":
                    result = new RuleTrim();
                    break;
                case "TrimSpaces":
                    result = new RuleTrimSpaces();
                    break;
                case "CaseToUpper":
                    result = new RuleChangeCase(RuleChangeCase.Types.ToUpper);
                    break;
                case "CaseToLower":
                    result = new RuleChangeCase(RuleChangeCase.Types.ToLower);
                    break;
                case "WordToUpper":
                    result = new RuleWordToUpper();
                    break;
                case "Round":
                    result = new RuleRound();
                    break;
                    //Sort Rules
                case "SortString":
                    result = new RuleSortString();
                    break;
                case "SortNumeric":
                    result = new RuleSortNumeric();
                    break;
                case "SortDateTime":
                    result = new RuleSortDateTime();
                    break;
                case "TrueForValue":
                    result = new RuleComparisonTrueForValue();
                    break;
                default:
                    throw new ArgumentException("Regeltyp " + name + " ist unbekannt.");
            }
            return result;
        }

        public static Rule RuleFromNode(XmlNode node)
        {
            Rule result = RuleFromUniqueName(node.Attributes["Name"].Value);
            result.ReadXml(node);
            return result;
        }
    }
}