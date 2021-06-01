namespace DbAnalyser
{
    public class ViewboxTablesInfo
    {
        public int category { get; set; }
        public string databaseName { get; set; }
        public string tableName { get; set; }
        public int type { get; set; }
        public long rowCount { get; set; }
        public int visible { get; set; }
        public int archived { get; set; }
        public int objectType { get; set; }
        public int defaultSheme { get; set; }
        public int transactionNumber { get; set; }
        public int tableId { get; set; }
        public int userDefined { get; set; }
        public int ordinal { get; set; }
    }
}
