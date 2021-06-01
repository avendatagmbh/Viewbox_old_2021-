namespace AvdCommon.Rules.Interfaces
{
    public interface IColumn
    {
        RuleSet Rules { get; set; }
        string Name { get; set; }
    }
}