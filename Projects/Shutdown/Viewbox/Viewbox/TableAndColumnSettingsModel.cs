using System;
using System.Collections.Generic;
using System.Linq;
using SystemDb;
using DbAccess;
using ViewboxDb;

namespace Viewbox
{
	public class TableAndColumnSettingsModel
	{
		private static TableAndColumnSettingsModel instance;

		public static TableAndColumnSettingsModel Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new TableAndColumnSettingsModel();
				}
				return instance;
			}
		}

		private TableAndColumnSettingsModel()
		{
		}

		public IEnumerable<ITableObject> GetElemets(string searchText = "", int limit = 150)
		{
			if (searchText != string.Empty)
			{
				return ViewboxApplication.Database.SystemDb.Tables.Where((ITable t) => t.Name.Contains(searchText)).Take(limit);
			}
			return ViewboxApplication.Database.SystemDb.Tables;
		}

		public IEnumerable<IColumn> GetTableColumns(int tableId)
		{
			return ViewboxApplication.Database.SystemDb.Tables.SingleOrDefault((ITable t) => t.Id == tableId).Columns;
		}

		public bool UpdateColumn(int columnId, string newText)
		{
			global::ViewboxDb.ViewboxDb db = ViewboxApplication.Database;
			IColumn column = db.SystemDb.Columns.SingleOrDefault((IColumn c) => c.Id == columnId);
			if (column != null)
			{
				ILanguage language = ViewboxSession.Language;
				column.SetDescription(newText, language);
				using DatabaseBase conn = ViewboxApplication.Database.ConnectionManager.GetConnection();
				if (!conn.IsOpen)
				{
					conn.Open();
				}
				string query = string.Format("SELECT COUNT(*) FROM {0} WHERE `ref_id` = {1} AND country_code = '{2}';", conn.Enquote(ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, "column_texts"), columnId, language.CountryCode);
				int count = int.Parse(conn.ExecuteScalar(query).ToString());
				query = ((count <= 0) ? string.Format("INSERT INTO {0} VALUES (null, {1}, '{2}', '{3}');", conn.Enquote(ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, "column_texts"), columnId, language.CountryCode, newText) : string.Format("UPDATE {0} SET `text` = '{1}' WHERE `ref_id` = {2} AND country_code = '{3}';", conn.Enquote(ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, "column_texts"), newText, columnId, language.CountryCode));
				try
				{
					conn.ExecuteNonQuery(query);
					return true;
				}
				catch (Exception)
				{
				}
			}
			return false;
		}

		public bool UpdateTable(int id, string newText)
		{
			ITableObject tableObject = ViewboxApplication.Database.SystemDb.Objects.SingleOrDefault((ITableObject t) => t.Id == id);
			if (tableObject != null)
			{
				using DatabaseBase conn = ViewboxApplication.Database.ConnectionManager.GetConnection();
				if (!conn.IsOpen)
				{
					conn.Open();
				}
				string query = string.Format("SELECT count(*) FROM {0} WHERE `ref_id` = {1} AND country_code = '{2}';", conn.Enquote(ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, "table_texts"), id, ViewboxSession.Language.CountryCode);
				int count = int.Parse(conn.ExecuteScalar(query).ToString());
				query = string.Empty;
				query = ((count <= 0) ? string.Format("INSERT INTO {0} VALUES (null, {1}, '{2}', '{3}');", conn.Enquote(ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, "table_texts"), id, ViewboxSession.Language.CountryCode, newText) : string.Format("UPDATE {0} SET `text` = '{1}' WHERE `ref_id` = {2} AND country_code = '{3}'", conn.Enquote(ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, "table_texts"), newText, id, ViewboxSession.Language.CountryCode));
				try
				{
					if (conn.ExecuteNonQuery(query) > 0)
					{
						tableObject.SetDescription(newText, ViewboxSession.Language);
						ViewboxSession.TableObjects.SingleOrDefault((ITableObject t) => t.Id == id)?.SetDescription(newText, ViewboxSession.Language);
						return true;
					}
				}
				catch (Exception)
				{
				}
			}
			return false;
		}
	}
}
