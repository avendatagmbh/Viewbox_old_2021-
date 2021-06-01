using System.Collections.Generic;
using DbAccess;
using ProjectDb.Enums;
using ProjectDb.Tables.Base;

namespace ProjectDb.Tables
{
    /// <summary>
    /// </summary>
    [DbTable(TABLENAME)]
    public class View : ViewBase
    {
        private const string TABLENAME = "views";

        private static string rowNumberSelect =
            @"SELECT
     name
    ,fileName
    ,description
    ,agent
    ,creationTimeStamp
    ,duration
    ,script
    ,state
    ,error
    ,`warnings`
    ,id
FROM
(
  SELECT  @row_num := IF(@prev_value=o.name,@row_num+1,1) AS RowNumber
       ,o.name
       ,o.fileName
       ,o.description
       ,o.agent
       ,o.creationTimeStamp
       ,o.duration
       ,o.script
       ,o.state
       ,o.error
       ,o.`warnings`
       ,o.id
       ,@prev_value := o.name
  FROM Views o,
      (SELECT @row_num := 1) x,
      (SELECT @prev_value := '') y
  ORDER BY o.name, o.id DESC
) z
WHERE RowNumber = 1";

        /// <summary>
        ///   Initializes a new instance of the <see cref="View" /> class.
        /// </summary>
        public View()
        {
            Id = 0;
        }

        public View(Viewscript script, string agent)
        {
            Name = script.Name;
            Description = script.Description;
            Duration = script.Duration;
            CreationTimestamp = script.CreationTimestamp;
            Script = script.Script;
            ProjectDb = script.ProjectDb;
            Agent = agent;
            State = ViewStates.OK;
            FileName = script.FileName;
            Error = script.LastError;
            Warnings = script.Warnings;
            ViewScriptState = script.State;
        }

        /// <summary>
        ///   Loads the view table.
        /// </summary>
        /// <param name="connMgr"> The connection manager. </param>
        /// <returns> </returns>
        internal static List<View> Load(IDatabase conn)
        {
            return conn.DbMapping.Load<View>();
        }

        /// <summary>
        ///   Loads only the latest records of the view table.
        /// </summary>
        /// <param name="conn"> </param>
        /// <returns> </returns>
        internal static List<View> LoadLatestRecords(IDatabase conn)
        {
            return conn.DbMapping.LoadBySQL<View>(rowNumberSelect);
        }

        public void DeleteAllWithName()
        {
            using (IDatabase conn = ProjectDb.ConnectionManager.GetConnection())
            {
                conn.ExecuteNonQuery("DELETE FROM " + TABLENAME + " WHERE name=" + conn.GetSqlString(Name));
            }
        }
    }
}