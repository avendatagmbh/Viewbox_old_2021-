namespace DbAnalyser.Models
{
    public class ResultBoxView
    {
        public int MyProperty { get; set; }
        public string tableName { get; set; }
        public string colName { get; set; }
        public string colType { get; set; }
        public int length { get; set; }
        public double complexity { get; set; }

        public ResultBoxView(string name, int len, float comp)
        {
            colName = name;
            length = len;
            complexity = System.Math.Round(comp / 1000, 2);
        }

        public ResultBoxView(string name, string type, int len)
        {
            colName = name;
            colType = type;
            length = len;
        }
    }
}
