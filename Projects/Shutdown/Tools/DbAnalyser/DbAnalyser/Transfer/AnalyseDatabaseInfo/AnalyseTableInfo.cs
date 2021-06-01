namespace DbAnalyser
{
    /**
    *  This class contains data which will be inserted to the analyse_table_info table
    */
    public class AnalyseTableInfo
    {
        public int tableId { get; set; }
        public string name { get; set; }
        public System.DateTime timeStamp { get; set; }
        public double duration { get; set; }
        public string comment { get; set; }
        public string description { get; set; }
        public Transfer.AnalyseDatabaseInfo.AnalysationState analysationState { get; set; }
        public int type { get; set; }
        public long count { get; set; }
    }
}
