using System.Linq;
using System.Text.RegularExpressions;

namespace DbAnalyser.Processing.NonSAP.Methods
{
    class NullAnalyser : TypeAnalyser
    {
        protected NullAnalyser()
        {
            expressions.Add(new Regex("^([n|N][u|U][l|L]{2})$"));
            expressions.Add(new Regex("^ *$"));
        }

        private static NullAnalyser nullFilter;

        public static NullAnalyser NullFilter
        {
            get { return nullFilter ?? (nullFilter = new NullAnalyser()); }
        }

        public override string AnalyzeInput(string input)
        {
            if (expressions.Any(exp => exp.IsMatch(input)))
            {
                return "null";
            }
            return "unknown";
        }
    }
}
