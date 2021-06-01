namespace DbAnalyser.Profile
{
    public class ProfileInfo
    {
        public string sourceHost { get; set; }
        public int sourcePort { get; set; }
        public string sourceUid { get; set; }
        public string sourcePassword { get; set; }
        public string sourceDatabase { get; set; }

        public string destHost { get; set; }
        public int destPort { get; set; }
        public string destUid { get; set; }
        public string destPassword { get; set; }
        public string destDatabase { get; set; }

        public int treshold { get; set; }
        public string finalDatabaseName { get; set; }

        public long FromRowCount { get; set; }
        public long ToRowCount { get; set; }
    }
}
