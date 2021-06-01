namespace ViewBuilderBusiness.MetadataUpdate
{
    public class InspectTableEventArgs : System.EventArgs
    {
        public InspectTableEventArgs(string table)
        {
            TableName = table;
        }

        public string TableName { get; private set; }
    }
}