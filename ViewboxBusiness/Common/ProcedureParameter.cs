using System;
using System.Collections.Generic;
using System.Linq;
using SystemDb;

namespace ViewboxBusiness.Common
{
	public class ProcedureParameter
	{
		public Dictionary<string, string> LangToDescription { get; set; }

		public SqlType Type { get; set; }

		public int TypeModifier { get; set; }

		public string Name { get; set; }

		public string DatabaseName { get; set; }

		public string TableName { get; set; }

		public string ColumnName { get; set; }

		public string DefaultValue { get; set; }

		public ProcedureParameter(string name)
		{
			LangToDescription = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			Name = name;
		}

		public void TypeFromString(string typeString)
		{
			Type = TypeConverter.TypeFromString(typeString);
		}

		public string GetSystemDbType()
		{
			return TypeConverter.GetSystemDbType(Type);
		}

		public string GetDescription()
		{
			if (LangToDescription.ContainsKey("de"))
			{
				return LangToDescription["de"];
			}
			if (LangToDescription.Count == 0)
			{
				return Name;
			}
			return LangToDescription.Values.FirstOrDefault();
		}
	}
}
