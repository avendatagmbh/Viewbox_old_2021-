using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DbAnalyser.Processing.NonSAP.Methods
{
    class DecimalAnalyser : TypeAnalyser
    {
        protected DecimalAnalyser()
        {
            expressions.Add(new Regex(@"^([+-]?)([0-9]+)(\\.|\\,)([0-9]+)([+-]?)$"));
            expressions.Add(new Regex(@"^([+-]?)([0-9]+)((\\.)([0-9]+))+(\\,)([0-9]+)([+-]?)$"));
        }

        private static DecimalAnalyser doubleFilter;

        public static DecimalAnalyser DoubleFilter
        {
            get
            {
                if (doubleFilter == null)
                {
                    doubleFilter = new DecimalAnalyser();
                }
                return doubleFilter;
            }
        }

        public override string AnalyzeInput(string input)
        {
            if (expressions.Any(exp => exp.IsMatch(input)))
            {
                return "DECIMAL";
            }
            return "unknown";
        }

        public static int getFractionPartSize(string input)
        {
            int firstPoint = -1;
            int firstToll = -1;

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == ',')
                {
                    firstToll = i;
                }
                else if (input[i] == '.')
                {
                    firstPoint = i;
                }

                if (firstPoint > -1 && firstToll > -1)
                {
                    break;
                }
            }
            if (firstToll > firstPoint)
            {
                if (input.Contains(","))
                {
                    string[] numberParts = input.Split(',');
                    if (numberParts.Count() == 2)
                    {
                        return numberParts[1].Length;
                    }
                }
            }
            else
            {
                if (input.Contains("."))
                {
                    string[] numberParts = input.Split('.');
                    if (numberParts.Count() == 2)
                    {
                        return numberParts[1].Length;
                    }
                }
                else
                {
                    string[] numberParts = input.Split(',');
                    if (numberParts.Count() == 2)
                    {
                        return numberParts[1].Length;
                    }
                }
            }            
            
            return 2;
        }

        public List<Regex> getRegex()
        {
            return expressions;
        }
    }
}
