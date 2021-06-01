namespace ViewBuilderBusiness.MetadataUpdate
{
    public class ResolvingColumnEventArgs : System.EventArgs
    {
        public ResolvingColumnEventArgs(string tableName, string columnName, string error = null)
        {
            TableName = tableName;
            ColumnName = columnName;
            HasError = !string.IsNullOrWhiteSpace(error);
            Error = HasError ? error : string.Empty;
        }

        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public bool HasError { get; set; }
        public string Error { get; set; }
    }
}