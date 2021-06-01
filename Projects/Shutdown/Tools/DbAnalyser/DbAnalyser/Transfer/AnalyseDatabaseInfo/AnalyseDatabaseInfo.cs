namespace DbAnalyser
{
    /**
     *  This class contains data which will be inserted to the analyse_database_info table
     */
    public class AnalyseDatabaseInfo
    {
        public int tableId { get; set; }
        public int tablesAll { get; set; }
        public int tablesEmpty { get; set; }
        public int tablesAnalysed { get; set; }
        public int columnsAll { get; set; }
        public int columnsNull { get; set; }
    }
}
