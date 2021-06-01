using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DbAccess;

namespace ViewBuilderCommon
{
    public class Index
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="Index" /> class.
        /// </summary>
        public Index()
        {
            Columns = new List<string>();
            UnsortedColumns = new List<string>();
        }

        public Index(string table, List<string> columns)
        {
            Columns = new List<string>();
            UnsortedColumns = new List<string>();
            foreach (var c in columns) AddColumn(c.ToLower());
            //Columns.Sort();
            Table = table.ToLower();
            SetDefaultName();
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="Index" /> class.
        /// </summary>
        /// <param name="sql"> The SQL. </param>
        public Index(string sql)
        {
            Columns = new List<string>();
            UnsortedColumns = new List<string>();

            //Regex createIndexCmdRegex = new Regex(
            //    @"CREATE[ ] +INDEX[ ]+([^ ]+)[ |\t]+ON[ ]+([^ ]+)[ ]*\((.+)\)", RegexOptions.IgnoreCase);
            Regex createIndexCmdRegex = new Regex(
                @"CREATE +INDEX +([^ ]+)[ |\t]+ON[ ]+`(.+)`[ ]*\((.+)\)", RegexOptions.IgnoreCase);
            Regex createIndexCmdRegex2 = new Regex(
                @"CREATE +INDEX +([^ ]+)[ |\t]+ON[ ]+([^ ]+)[ ]*\((.+)\)", RegexOptions.IgnoreCase);
            //Regex createIndexCmdRegex = new Regex(
            //                    @"CREATE +INDEX +([^ ]+)[ |\t]+ON[ ]+([^ ]+)[ ]*\((.+)\)", RegexOptions.IgnoreCase);
            string columns;
            if (createIndexCmdRegex.IsMatch(sql))
            {
                Match match = createIndexCmdRegex.Match(sql);
                Table = match.Groups[2].Value.ToLower();
                Name = match.Groups[1].Value;
                columns = match.Groups[3].Value;
            }
            else if (createIndexCmdRegex2.IsMatch(sql))
            {
                Match match = createIndexCmdRegex2.Match(sql);
                Table = match.Groups[2].Value.ToLower();
                Name = match.Groups[1].Value;
                columns = match.Groups[3].Value;
            }
            else
            {
                createIndexCmdRegex = new Regex(
                    @"ALTER[ ]+TABLE[ ]+([^ ]+)[ ]+ADD[ ]+INDEX[ ]+([^ ]+)[ ]+\((.+)\)", RegexOptions.IgnoreCase);
                if (createIndexCmdRegex.IsMatch(sql))
                {
                    Match match = createIndexCmdRegex.Match(sql);
                    Table = match.Groups[1].Value.ToLower();
                    Name = match.Groups[2].Value;
                    columns = match.Groups[3].Value;
                }
                else
                {
                    throw new Exception("Der Index konnte nicht korrekt aus dem Befehl ausgelesen werden: " +
                                        Environment.NewLine + sql);
                }
            }
            foreach (string column in columns.Split(','))
            {
                if (column.Trim().Length > 0)
                    AddColumn(column.Trim().ToLower()); //Columns.Add(column.Trim().ToLower());
            }
            //Columns.Sort();
            SetDefaultName();
        }

        /// <summary>
        ///   Gets or sets the name.
        /// </summary>
        /// <value> The name. </value>
        public string Name { get; set; }

        /// <summary>
        ///   Gets or sets the table.
        /// </summary>
        /// <value> The table. </value>
        public string Table { get; set; }

        /// <summary>
        ///   Gets or sets the columns.
        /// </summary>
        /// <value> The columns. </value>
        public List<string> Columns { get; set; }

        public List<string> UnsortedColumns { get; set; }

        /// <summary>
        ///   Gets or sets the complexity.
        /// </summary>
        /// <value> The columns. </value>
        public long Complexity { get; set; }

        /// <summary>
        ///   Determines whether [is create index command] [the specified SQL].
        /// </summary>
        /// <param name="sql"> The SQL. </param>
        /// <returns> <c>true</c> if [is create index command] [the specified SQL]; otherwise, <c>false</c> . </returns>
        public static bool CheckSql(string sql)
        {
            Regex createIndexCmdRegex = new Regex(
                @"CREATE[ ]+INDEX[ ]+([^ ]+)[ ]+ON[ ]+([^ ]+)[ ]*\((.+)\)", RegexOptions.IgnoreCase);
            if (createIndexCmdRegex.IsMatch(sql))
            {
                return true;
            }
            createIndexCmdRegex = new Regex(
                @"ALTER[ ]+TABLE[ ]+([^ ]+)[ ]+ADD[ ]+INDEX[ ]+([^ ]+)[ ]+\((.+)\)", RegexOptions.IgnoreCase);
            return createIndexCmdRegex.IsMatch(sql);
        }

        //public string CreateCommand {
        //    get {
        //        string sql = "CREATE INDEX " + Name + " ON " + Table + "(";
        //        foreach (string column in UnsortedColumns) sql += column + ",";
        //        sql = sql.Remove(sql.Length - 1);
        //        sql += ")";
        //        return sql;
        //    }
        //}
        /// <summary>
        ///   Gets the create command.
        /// </summary>
        /// <value> The create command. </value>
        /// <summary>
        ///   Sets the default name.
        /// </summary>
        public void SetDefaultName()
        {
            if (Table != null)
                Name = "_" + Table.ToLower().Replace(" ", "_");
            //foreach (string column in UnsortedColumns) Name += "_" + column.ToLower();
            foreach (string column in UnsortedColumns) Name += "_" + column.ToLower().Replace(" ", "_");
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var i = obj as Index;
            if (i != null && i.GetHashCode() == GetHashCode()) return true;
            return false;
        }

        public void AddColumn(string name)
        {
            name = name.Replace("`", "");
            if (Columns.Contains(name))
                return;
            Columns.Add(name);
            Columns.Sort();
            UnsortedColumns.Add(name);
            SetDefaultName();
        }

        public bool Exists(IDatabase conn)
        {
            return conn.IsIndexExisting(Table, Columns);
        }
    }
}