namespace AvdCommon.Rules.ExecuteRules
{
    public class RuleEliminateSpaces : ExecuteRule
    {
        public RuleEliminateSpaces()
        {
            Name = "Entferne Spaces";
            UniqueName = "EliminateSpaces";
        }

        public override bool HasParameter
        {
            get { return false; }
        }

        public override string Execute(string value)
        {
            return value.Replace(" ", "");
        }
    }
}