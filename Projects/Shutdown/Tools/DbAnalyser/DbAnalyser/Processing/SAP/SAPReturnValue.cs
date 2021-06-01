namespace DbAnalyser.Processing.SAP
{
    public class SAPReturnValue
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int Length { get; set; }
        public int Decimals { get; set; }

        public SAPReturnValue(string name, string type, int length, int decimals)
        {
            Name = name;
            Type = type;
            Length = length;
            Decimals = decimals;
        }
    }
}
