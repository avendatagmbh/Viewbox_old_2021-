using System.Text.RegularExpressions;

namespace DbAnalyser.Processing.NonSAP.Methods
{
    class BooleanAnalyser : TypeAnalyser
    {
        protected BooleanAnalyser()
        {
            expressions.Add(new Regex("^(false|true)$"));
            expressions.Add(new Regex("^(False|True)$"));
            expressions.Add(new Regex("^(FALSE|TRUE)$"));
        }

        private static BooleanAnalyser boolFilter;

        public static BooleanAnalyser BoolFilter
        {
            get
            {
                if (boolFilter == null)
                {
                    boolFilter = new BooleanAnalyser();
                }
                return boolFilter;
            }
        }
           
        public override string AnalyzeInput(string input)
        {
            foreach (var exp in expressions)
            {
                if (exp.IsMatch(input))
                {
                    return "BOOL";
                }
            }
            return "unknown";
        }
    }
}
