using DbAccess;

namespace AvdCommon.Rules.SortRules
{
    public class RuleSortString : SortRule
    {
        public RuleSortString()
        {
            Name = "Sortiere als String";
            UniqueName = "SortString";
        }

        public override bool HasParameter
        {
            get { return false; }
        }

        public override int Sort(string v1, string v2)
        {
            return v1.CompareTo(v2);
        }

        public override string GetSqlSortStatement(string columnName, IDatabase conn, int _which)
        {
            return _which == 0
                       ? "LEN(" + conn.Enquote(columnName) + ")," + conn.Enquote(columnName)
                       : "LENGTH(" + conn.Enquote(columnName) + ")," + conn.Enquote(columnName);
        }
    }
}