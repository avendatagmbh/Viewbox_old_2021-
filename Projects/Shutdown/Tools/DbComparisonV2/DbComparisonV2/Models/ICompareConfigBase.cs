using System;
namespace DbComparisonV2.Models
{
    public interface ICompareConfigBase<TCompareConfig1,TCompareConfig2>
    {
        string ConfigName { get; set; }
        TCompareConfig1 Object1 { get; set; }
        TCompareConfig2 Object2 { get; set; }
        string OutputDir { get; set; }
    }
}
