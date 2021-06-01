using System.Collections.Generic;
using DbAccess;
using DbAccess.Attributes;

namespace ViewboxBusiness.ProfileDb.Tables
{
	[DbTable("info")]
	internal class Info
	{
		internal const string TABLENAME = "info";

		private const string ColnameKey = "key";

		private const string ColnameValue = "value";

		internal static string KeyVersion = "version";

		internal static string ValueVersionActual = "1.0.1";

		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("key", Length = 64)]
		public string Key { get; set; }

		[DbColumn("value", Length = 1024)]
		public string Value { get; set; }

		internal static void CreateTable(DatabaseBase conn)
		{
			conn.DbMapping.CreateTableIfNotExists<Info>();
			if (GetValue(conn, KeyVersion) == null)
			{
				SetValue(conn, KeyVersion, ValueVersionActual);
			}
		}

		internal static string GetValue(DatabaseBase conn, string key)
		{
			return conn.ExecuteScalar("SELECT " + conn.Enquote("value") + " FROM " + conn.Enquote("info") + " WHERE " + conn.Enquote("key") + "=" + conn.GetSqlString(key))?.ToString();
		}

		internal static void SetValue(DatabaseBase conn, string key, string value)
		{
			List<Info> infos = conn.DbMapping.Load<Info>(string.Format("{0}={1}", conn.Enquote("key"), conn.GetSqlString(value)));
			Info info = new Info
			{
				Key = key,
				Value = value
			};
			if (infos.Count >= 1)
			{
				info = infos[0];
			}
			conn.DbMapping.Save(info);
		}

		internal static void DeleteKey(DatabaseBase conn, string key)
		{
			conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("info") + " WHERE " + conn.Enquote("key") + "=" + conn.GetSqlString(key));
		}
	}
}
