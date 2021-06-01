using System.Collections.Generic;
using DbAccess;
using DbAccess.Attributes;

namespace SystemDb
{
	[DbTable("info", ForceInnoDb = true)]
	internal class Info
	{
		private readonly DatabaseBase _conn;

		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("key", AllowDbNull = false)]
		[DbUniqueKey]
		public string Key { get; set; }

		[DbColumn("value", AllowDbNull = true)]
		public string Value { get; set; }

		public string DbVersion
		{
			get
			{
				return GetValue("DbVersion");
			}
			set
			{
				SetValue("DbVersion", value);
			}
		}

		public Info()
		{
		}

		public Info(DatabaseBase conn)
		{
			_conn = conn;
		}

		private void SetValue(string key, string value)
		{
			SetValue(key, value, _conn);
		}

		internal static void SetValue(string key, string value, DatabaseBase conn)
		{
			List<Info> tmp = conn.DbMapping.Load<Info>(conn.Enquote("key") + "=" + conn.GetSqlString(key));
			if (tmp.Count > 0)
			{
				tmp[0].Value = value;
				conn.DbMapping.Save(tmp[0]);
				return;
			}
			Info info = new Info(conn)
			{
				Key = key,
				Value = value
			};
			conn.DbMapping.Save(info);
		}

		private string GetValue(string key)
		{
			return GetValue(key, _conn);
		}

		internal static string GetValue(string key, DatabaseBase conn)
		{
			string sql = "SELECT " + conn.Enquote("value") + " FROM " + conn.Enquote(conn.DbMapping.GetTableName<Info>()) + " WHERE " + conn.Enquote("key") + "=" + conn.GetSqlString(key);
			return conn.ExecuteScalar(sql)?.ToString();
		}

		internal void RemoveValue(string key)
		{
			RemoveValue(key, _conn);
		}

		internal static void RemoveValue(string key, DatabaseBase conn)
		{
			conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote(conn.DbMapping.GetTableName<Info>()) + " WHERE " + conn.Enquote("key") + "='" + key + "'");
		}

		public bool TableExists()
		{
			return _conn.TableExists("info");
		}
	}
}
