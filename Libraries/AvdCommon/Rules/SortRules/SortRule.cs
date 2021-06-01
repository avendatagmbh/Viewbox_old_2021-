using DbAccess;

namespace AvdCommon.Rules.SortRules
{
    public abstract class SortRule : Rule
    {
        public override int Sort(string v1, string v2)
        {
            return v1.CompareTo(v2);
        }

        public virtual string GetSqlSortStatement(string columnName, IDatabase conn, int _which)
        {
            return conn.Enquote(columnName);
        }
    }
}