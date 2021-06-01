namespace DbAnalyser.Models
{
    public class NonSAPColumnItem
    {
        public System.Collections.Generic.List<AnalysedColumnInfo> colInfos { get; set; }
        public long rowCount { get; set; }
    }
}
