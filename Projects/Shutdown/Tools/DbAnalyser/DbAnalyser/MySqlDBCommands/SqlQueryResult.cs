using System.Collections.Generic;

namespace DbAnalyser.MySqlDBCommands
{
    public enum SqlQueryResult
    {
        Exist,          // Table exist
        NotExist,       // Table not exist
        Successful,     // Query was succesful
        Unsuccessful,   // Query was not succesful 
        Exception       // Exception throwed
    };

    public class QueryResult
    {
        public List<string> tableresult = new List<string>();
        public List<int> columnnumber = new List<int>();
        public Dictionary<string, int> rownumber = new Dictionary<string, int>();

        public string result
        {
            get;
            set;
        }

        public SqlQueryResult technicalresult
        {
            get;
            set;
        }
    }
}
