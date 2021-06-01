namespace ViewBuilderBusiness.MetadataUpdate
{
    public class InspectTablesEventArgs : System.EventArgs
    {
        public InspectTablesEventArgs(int count)
        {
            NumberOfTables = count;
        }

        public int NumberOfTables { get; private set; }
    }
}