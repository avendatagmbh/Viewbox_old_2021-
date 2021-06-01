namespace AvdCommon.Rules.ExecuteRules
{
    public class RuleDbNullToEmptyString : ExecuteRule
    {
        public RuleDbNullToEmptyString()
        {
            Name = "DbNull Eintr�ge in Leerstrings";
            UniqueName = "DbNullToEmpty";
        }

        public override bool HasParameter
        {
            get { return false; }
        }

        public override string Execute(string value)
        {
            return value == "DBNULL" ? "" : value;
        }
    }
}