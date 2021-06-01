namespace AvdCommon.Rules.ExecuteRules
{
    public class RuleTrimSpaces : ExecuteRule
    {
        public RuleTrimSpaces()
        {
            Name = "Entferne Spaces am Anfang/Ende";
            UniqueName = "TrimSpaces";
        }

        public override bool HasParameter
        {
            get { return false; }
        }

        public override string Execute(string value)
        {
            return value.Trim();
        }
    }
}