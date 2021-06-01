using DbAnalyser.Processing;

namespace DbAnalyser
{
    public class ViewboxColumnsInfo
    {
        public int isVisible { get; set; }
        public int tableId { get; set; }
        public string tableName { get; set; }
        public ProcessDataTypes dataType { get; set; }
        public string dataTypeName { get; set; }
        public string originalName { get; set; }
        public int optimaliztaionType { get; set; }
        public int isEmpty { get; set; }
        public int maxLength { get; set; }
        public int decimals { get; set; }
        public int paramOperators { get; set; }
        public string constValue { get; set; }
        public string fromColumn { get; set; }
        public string fromColumnFormat { get; set; }
        public int flag { get; set; }
        public int colId { get; set; }
        public string colName { get; set; }
        public int userDefined { get; set; }
        public int ordinal { get; set; }
    }
}
