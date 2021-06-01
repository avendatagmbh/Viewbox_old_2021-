using System;
using System.Collections.Generic;
using System.Data;
using DbAccess;
using DbAccess.Attributes;

namespace SystemDb.Helper
{
	public class LocalizedText : ICloneable
	{
		[DbColumn("id", AutoIncrement = true)]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("ref_id")]
		public int RefId { get; set; }

		[DbColumn("country_code")]
		public string CountryCode { get; set; }

		[DbColumn("text", Length = 1024)]
		public string Text { get; set; }

		public object Clone()
		{
			return MemberwiseClone();
		}

		public static List<LocalizedText> FastLoad(DatabaseBase conn, Type concreteType)
		{
			List<LocalizedText> result = new List<LocalizedText>();
			using IDataReader reader = conn.ExecuteReader("SELECT id,ref_id, country_code,text FROM " + conn.DbMapping.GetTableName(concreteType));
			while (reader.Read())
			{
				LocalizedText text = new LocalizedText
				{
					Id = ((!reader.IsDBNull(0)) ? reader.GetInt32(0) : 0),
					RefId = ((!reader.IsDBNull(1)) ? reader.GetInt32(1) : 0),
					CountryCode = (reader.IsDBNull(2) ? null : reader.GetString(2)),
					Text = (reader.IsDBNull(3) ? null : reader.GetString(3))
				};
				result.Add(text);
			}
			return result;
		}
	}
}
