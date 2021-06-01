using System;
using DbAccess;

namespace AvdCommon.Rules.SortRules
{
    public class RuleSortNumeric : SortRule
    {
        public RuleSortNumeric()
        {
            Name = "Sortiere als Zahl";
            UniqueName = "SortNumeric";
        }

        public override bool HasParameter
        {
            get { return false; }
        }

        public override int Sort(string v1, string v2)
        {
            double value1, value2;
            if (Double.TryParse(v1, out value1) && Double.TryParse(v2, out value2))
            {
                if (value1 < value2) return -1;
                if (value1 > value2) return 1;
                return 0;
            }
            return base.Sort(v1, v2);
        }

        public override string GetSqlSortStatement(string columnName, IDatabase conn, int _which)
        {
            //return _which == 0 ? 
            //    "LEN(" + conn.Enquote(columnName) + ")," + conn.Enquote(columnName):
            //"LENGTH(" + conn.Enquote(columnName) + ")," + conn.Enquote(columnName);
            return _which == 0
                       ? //"CCur(" + conn.Enquote(columnName) + ")" : 
                   "Val(IIF(" + conn.Enquote(columnName) + " is null,0," + conn.Enquote(columnName) + "))"
                       : "CAST(" + conn.Enquote(columnName) + " AS DECIMAL(12,4))";
        }
    }
}