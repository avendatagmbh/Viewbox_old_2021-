using System.Text.RegularExpressions;

namespace DbAnalyser.Processing.NonSAP.Methods
{
    class IntAnalyser : TypeAnalyser
    {
        protected IntAnalyser()
        {
            expressions.Add(new Regex("^([+-]?[1-9][0-9]*|0)$"));
        }

        private static IntAnalyser intFilter;

        public static IntAnalyser IntFilter
        {
            get { return intFilter ?? (intFilter = new IntAnalyser()); }
        }

        public override string AnalyzeInput(string input)
        {
            foreach (var exp in expressions)
            {
                if (exp.IsMatch(input))
                {
                    return "INT";
                }
            }
            return "unknown";
        }

        public Regex getRegex()
        {
            return expressions[0];
        }
    }
}
