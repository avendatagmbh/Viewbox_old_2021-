using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;
using DbAccess;

namespace Viewbox.Models
{
	public static class RoleManagement
	{
		public static void UpdateTableObjectVisibilities(IRole role, Dictionary<int, bool> settings)
		{
			using DatabaseBase db = ViewboxApplication.Database.ConnectionManager.GetConnection();
			try
			{
				db.BeginTransaction();
				int deleteStmtNr = db.Prepare(string.Format("DELETE FROM {0} WHERE `role_id` = @id AND `table_id` IN (SELECT `id` FROM {1} WHERE `type` IN ({2}, {3}))", db.Enquote(ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, "table_roles"), db.Enquote(ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, "tables"), 2, 3));
				db.SetParameterForPreparedStmt(deleteStmtNr, "@id", role.Id);
				db.ExecutePreparedNonQuery(deleteStmtNr);
				db.ClosePreparedStmt(deleteStmtNr);
				StringBuilder sb = new StringBuilder();
				sb.Append(string.Format("REPLACE INTO {0} (`table_id`, `role_id`, `right`) VALUES ", db.Enquote(ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, "table_roles")));
				lock (ViewboxApplication.Database.SystemDb.RoleTableObjectRights)
				{
					IEnumerable<ITableObject> tableObjects = ViewboxApplication.Database.SystemDb.Objects.Where((ITableObject t) => settings.ContainsKey(t.Id));
					foreach (ITableObject tobj in tableObjects)
					{
						RightType right = (settings[tobj.Id] ? RightType.Read : RightType.None);
						int sqlright = ((right == RightType.Read) ? 1 : 0);
						ViewboxApplication.Database.SystemDb.RoleTableObjectRights[role, tobj] = right;
						sb.Append($"({tobj.Id}, {role.Id}, {sqlright}),");
					}
				}
				sb.Length--;
				string query = sb.ToString();
				db.ExecuteNonQuery(query);
				db.CommitTransaction();
			}
			catch
			{
				db.RollbackTransaction();
			}
		}

		public static void UpdateTablesVisibility(IRole role, bool visible)
		{
			using DatabaseBase db = ViewboxApplication.Database.ConnectionManager.GetConnection();
			try
			{
				db.BeginTransaction();
				int deleteStmtNr = db.Prepare(string.Format("DELETE FROM {0} WHERE `role_id` = @id AND `table_id` IN (SELECT `id` FROM {1} WHERE `type` IN ({2}))", db.Enquote(ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, "table_roles"), db.Enquote(ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, "tables"), 1));
				db.SetParameterForPreparedStmt(deleteStmtNr, "@id", role.Id);
				db.ExecutePreparedNonQuery(deleteStmtNr);
				db.ClosePreparedStmt(deleteStmtNr);
				StringBuilder sb = new StringBuilder();
				sb.Append(string.Format("REPLACE INTO {0} (`table_id`, `role_id`, `right`) VALUES ", db.Enquote(ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, "table_roles")));
				lock (ViewboxApplication.Database.SystemDb.RoleTableObjectRights)
				{
					IEnumerable<ITableObject> tableObjects = ViewboxApplication.Database.SystemDb.Objects.Where((ITableObject t) => t.Type == TableType.Table);
					RightType right = (visible ? RightType.Read : RightType.None);
					int sqlright = ((right == RightType.Read) ? 1 : 0);
					foreach (ITableObject tobj in tableObjects)
					{
						ViewboxApplication.Database.SystemDb.RoleTableObjectRights[role, tobj] = right;
						sb.Append($"({tobj.Id}, {role.Id}, {sqlright}),");
					}
				}
				sb.Length--;
				db.ExecuteNonQuery(sb.ToString());
				db.CommitTransaction();
			}
			catch
			{
				db.RollbackTransaction();
			}
		}

		public static void UpdateOneTableVisibility(int tableId, IRole role, bool visible)
		{
			using DatabaseBase db = ViewboxApplication.Database.ConnectionManager.GetConnection();
			try
			{
				db.BeginTransaction();
				RightType right = (visible ? RightType.Read : RightType.None);
				int sqlright = ((right == RightType.Read) ? 1 : 0);
				int updateStmtNr = db.Prepare(string.Format("UPDATE {0} SET `right` = @right WHERE `role_id` = @roleId AND `table_id` = @tableId", db.Enquote(ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, "table_roles")));
				db.SetParameterForPreparedStmt(updateStmtNr, "@right", sqlright);
				db.SetParameterForPreparedStmt(updateStmtNr, "@roleId", role.Id);
				db.SetParameterForPreparedStmt(updateStmtNr, "@tableId", tableId);
				int updatedRows = db.ExecutePreparedNonQuery(updateStmtNr);
				db.ClosePreparedStmt(updateStmtNr);
				ITable tobj = ViewboxApplication.Database.SystemDb.Tables.First((ITable t) => t.Id == tableId);
				ViewboxApplication.Database.SystemDb.RoleTableObjectRights[role, tobj] = right;
				if (updatedRows == 0)
				{
					int insertStmtNr = db.Prepare(string.Format("INSERT INTO {0} (`table_id`, `role_id`, `right`) VALUES (@tableId, @roleId, @right)", db.Enquote(ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, "table_roles")));
					db.SetParameterForPreparedStmt(insertStmtNr, "@right", sqlright);
					db.SetParameterForPreparedStmt(insertStmtNr, "@roleId", role.Id);
					db.SetParameterForPreparedStmt(insertStmtNr, "@tableId", tableId);
					db.ExecutePreparedNonQuery(insertStmtNr);
					db.ClosePreparedStmt(insertStmtNr);
				}
				else if (updatedRows > 1)
				{
					int deleteStmtNr = db.Prepare(string.Format("DELETE FROM {0} WHERE `id` IN (\r\n                                    SELECT `id` FROM {0} WHERE `table_id` = @tableId AND `role_id` = @roleId ORDER BY `id` ASC LIMIT {1}", db.Enquote(ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, "table_roles"), updatedRows - 1));
					db.SetParameterForPreparedStmt(deleteStmtNr, "@roleId", role.Id);
					db.SetParameterForPreparedStmt(deleteStmtNr, "@tableId", tableId);
					db.ExecutePreparedNonQuery(deleteStmtNr);
					db.ClosePreparedStmt(deleteStmtNr);
				}
				db.CommitTransaction();
			}
			catch
			{
				db.RollbackTransaction();
			}
		}

		private static IDictionary<int, bool> BFS(IEnumerable<IOptimization> roots, IDictionary<int, bool> settings)
		{
			IDictionary<int, bool> completeSettings = new Dictionary<int, bool>();
			Queue<IOptimization> q = new Queue<IOptimization>();
			foreach (IOptimization c2 in roots)
			{
				q.Enqueue(c2);
			}
			while (q.Count > 0)
			{
				IOptimization opt = q.Dequeue();
				if (settings.Keys.Contains(opt.Id))
				{
					bool visibility = settings[opt.Id];
					completeSettings.Add(opt.Id, visibility);
				}
				else if (opt.Parent != null && completeSettings.ContainsKey(opt.Parent.Id))
				{
					bool p2 = completeSettings[opt.Parent.Id];
					completeSettings.Add(opt.Id, p2);
				}
				else if (opt.Parent != null && settings.ContainsKey(opt.Parent.Id))
				{
					bool p = settings[opt.Parent.Id];
					completeSettings.Add(opt.Id, p);
				}
				else
				{
					completeSettings.Add(opt.Id, value: false);
				}
				foreach (IOptimization c in opt.Children)
				{
					q.Enqueue(c);
				}
			}
			return completeSettings;
		}

		private static IDictionary<int, bool> BuildUpCompleteTree(IDictionary<int, bool> settings)
		{
			IOptimizationCollection optimizations = ViewboxApplication.Database.SystemDb.Optimizations;
			List<IOptimization> roots = ViewboxApplication.Database.SystemDb.Optimizations.Where((IOptimization o) => o.Level == 1).ToList();
			return BFS(roots, settings);
		}

		public static void UpdateOptimizations(IRole role, Dictionary<int, bool> settings)
		{
			IDictionary<int, bool> completeSettings = BuildUpCompleteTree(settings);
			using DatabaseBase db = ViewboxApplication.Database.ConnectionManager.GetConnection();
			try
			{
				db.BeginTransaction();
				int deleteStmtNr = db.Prepare(string.Format("DELETE FROM {0} WHERE `role_id` = @id", db.Enquote(ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, "optimization_roles")));
				db.SetParameterForPreparedStmt(deleteStmtNr, "@id", role.Id);
				db.ExecutePreparedNonQuery(deleteStmtNr);
				db.ClosePreparedStmt(deleteStmtNr);
				StringBuilder sb = new StringBuilder();
				sb.Append(string.Format("REPLACE INTO {0} (`optimization_id`, `role_id`, `visible`) VALUES ", db.Enquote(ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, "optimization_roles")));
				foreach (KeyValuePair<int, bool> s in completeSettings)
				{
					int optId = s.Key;
					IOptimization opt = ViewboxApplication.Database.SystemDb.Optimizations[optId];
					if (opt != null)
					{
						RightType right = (s.Value ? RightType.Read : RightType.None);
						ViewboxApplication.Database.SystemDb.RoleOptimizationRights[role, opt] = right;
						sb.Append($"({optId}, {role.Id}, {(s.Value ? 1 : 0)}),");
					}
				}
				sb.Length--;
				string query = sb.ToString();
				db.ExecuteNonQuery(query);
				ViewboxApplication.Database.SystemDb.GetOptimizations(role);
				db.CommitTransaction();
			}
			catch
			{
				db.RollbackTransaction();
			}
		}
	}
}
