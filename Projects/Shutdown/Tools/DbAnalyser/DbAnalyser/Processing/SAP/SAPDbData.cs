namespace DbAnalyser
{
    /**
     * This class contains every information which is required for the analysation process for SAP databases 
     */
    public class SAPDbData
    {
        public string TabName { get; set; }
        public string FieldName { get; set; }
        public string dataType { get; set; }
        public int IntLength { get; set; }
        public int Length { get; set; }
        public int Decimals { get; set; }

        public SAPDbData(string tabname, string fieldname, string datatype, int intlen, int length, int decimals)
        {
            TabName = tabname;
            FieldName = fieldname;
            dataType = datatype;
            IntLength = intlen;
            Length = length;
            Decimals = decimals;
        }
    }
}
