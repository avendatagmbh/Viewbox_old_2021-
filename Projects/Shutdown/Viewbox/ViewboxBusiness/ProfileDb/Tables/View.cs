using System.Collections.Generic;
using DbAccess;
using DbAccess.Attributes;

namespace ViewboxBusiness.ProfileDb.Tables
{
	[DbTable("views")]
	public class View : ViewBase
	{
		private const string TABLENAME = "views";

		private const string RowNumberSelect = "SELECT\r\n     name\r\n    ,fileName\r\n    ,description\r\n    ,agent\r\n    ,creationTimeStamp\r\n    ,duration\r\n    ,script\r\n    ,state\r\n    ,error\r\n    ,`warnings`\r\n    ,id\r\nFROM\r\n(\r\n  SELECT  @row_num := IF(@prev_value=o.name,@row_num+1,1) AS RowNumber\r\n       ,o.name\r\n       ,o.fileName\r\n       ,o.description\r\n       ,o.agent\r\n       ,o.creationTimeStamp\r\n       ,o.duration\r\n       ,o.script\r\n       ,o.state\r\n       ,o.error\r\n       ,o.`warnings`\r\n       ,o.id\r\n       ,@prev_value := o.name\r\n  FROM Views o,\r\n      (SELECT @row_num := 1) x,\r\n      (SELECT @prev_value := '') y\r\n  ORDER BY o.name, o.id DESC\r\n) z\r\nWHERE RowNumber = 1";

		public View()
		{
			base.Id = 0;
		}

		public View(Viewscript script, string agent)
		{
			base.Name = script.Name;
			base.Description = script.Description;
			base.Duration = script.Duration;
			base.CreationTimestamp = script.CreationTimestamp;
			base.Script = script.Script;
			base.ProjectDb = script.ProjectDb;
			base.Agent = agent;
			base.State = ViewStates.Ok;
			base.FileName = script.FileName;
			base.Error = script.LastError;
			base.Warnings = script.Warnings;
			base.ViewScriptState = script.State;
		}

		internal static List<View> Load(DatabaseBase conn)
		{
			return conn.DbMapping.Load<View>();
		}

		internal static List<View> LoadLatestRecords(DatabaseBase conn)
		{
			return conn.DbMapping.LoadBySQL<View>("SELECT\r\n     name\r\n    ,fileName\r\n    ,description\r\n    ,agent\r\n    ,creationTimeStamp\r\n    ,duration\r\n    ,script\r\n    ,state\r\n    ,error\r\n    ,`warnings`\r\n    ,id\r\nFROM\r\n(\r\n  SELECT  @row_num := IF(@prev_value=o.name,@row_num+1,1) AS RowNumber\r\n       ,o.name\r\n       ,o.fileName\r\n       ,o.description\r\n       ,o.agent\r\n       ,o.creationTimeStamp\r\n       ,o.duration\r\n       ,o.script\r\n       ,o.state\r\n       ,o.error\r\n       ,o.`warnings`\r\n       ,o.id\r\n       ,@prev_value := o.name\r\n  FROM Views o,\r\n      (SELECT @row_num := 1) x,\r\n      (SELECT @prev_value := '') y\r\n  ORDER BY o.name, o.id DESC\r\n) z\r\nWHERE RowNumber = 1");
		}

		public void DeleteAllWithName()
		{
			using DatabaseBase conn = base.ProjectDb.ConnectionManager.GetConnection();
			conn.ExecuteNonQuery("DELETE FROM views WHERE name=" + conn.GetSqlString(base.Name));
		}
	}
}
