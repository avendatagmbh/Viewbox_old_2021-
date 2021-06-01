using System;
using System.Collections.Generic;
using System.Linq;
using SystemDb;

namespace ViewboxBusiness.Common
{
	public class ReportParameter
	{
		public string DatabaseName { get; internal set; }

		public string TableName { get; internal set; }

		public string ParameterName { get; internal set; }

		public string ColumnName { get; internal set; }

		public string DefaultValue { get; internal set; }

		public SqlType Type { get; internal set; }

		public int TypeModifier { get; internal set; }

		public Dictionary<string, string> ParameterDescription { get; internal set; }

		public ReportParameter(string databaseName, string tableName, string columnName, string parameterName, SqlType type, string defaultValue, int typeModifier)
		{
			ParameterDescription = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			DatabaseName = databaseName;
			TableName = tableName;
			ColumnName = columnName;
			ParameterName = parameterName;
			Type = type;
			TypeModifier = typeModifier;
			DefaultValue = defaultValue;
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
			if (ParameterDescription.Count == 0)
			{
				return ParameterName;
			}
			return ParameterDescription.Values.FirstOrDefault();
		}
	}
}
