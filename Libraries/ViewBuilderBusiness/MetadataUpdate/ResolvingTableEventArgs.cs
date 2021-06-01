namespace ViewBuilderBusiness.MetadataUpdate
{
    public class ResolvingTableEventArgs : System.EventArgs
    {
        public ResolvingTableEventArgs(string tableName)
        {
            TableName = tableName;
        }

        public string TableName { get; set; }
    }
}