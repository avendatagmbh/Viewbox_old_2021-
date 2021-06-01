using DbAnalyser.Processing;

namespace DbAnalyser
{
    /**
    *  This class contains data which will be inserted to the analyse_column_info table
    */
    public class AnalysedColumnInfo
    {
        public int tableId { get; set; }
        public string tableName { get; set; }
        public int colId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int length { get; set; }
        public int decimals { get; set; }
        public string decimalFormat { get; set; }
        public string dateFormat { get; set; }
        public string type { get; set; }
        public ProcessDataTypes typeId { get; set; }
    }
}
