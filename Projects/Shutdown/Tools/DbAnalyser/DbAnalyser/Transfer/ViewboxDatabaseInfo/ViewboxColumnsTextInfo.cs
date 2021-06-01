namespace DbAnalyser.Transfer.ViewboxDatabaseInfo
{
    public class ViewboxColumnsTextInfo
    {
        public int id { get; set; }
        public int refCode { get; set; } // the real column id from the columns table
        public string countryCode { get; set; }
        public string text { get; set; }
    }
}
