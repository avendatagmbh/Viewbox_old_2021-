using System;
using SystemDb;
using ViewboxBusiness.Resources;

namespace ViewboxBusiness.Common
{
	public class TypeConverter
	{
		public static SqlType TypeFromString(string typeString)
		{
			switch (typeString.ToLower())
			{
			case "string":
			case "char":
				return SqlType.String;
			case "natural number":
				return SqlType.Integer;
			case "numeric":
				return SqlType.Decimal;
			case "date":
				return SqlType.Date;
			case "time":
				return SqlType.Time;
			case "datetime":
				return SqlType.DateTime;
			case "integer":
				return SqlType.Integer;
			case "float":
				return SqlType.Numeric;
			case "int":
				return SqlType.Integer;
			case "number":
				return SqlType.Integer;
			case "bool":
			case "boolean":
				return SqlType.Boolean;
			default:
				throw new Exception(Resource.UnknownParameterType + " " + typeString);
			}
		}

		public static string GetSystemDbType(SqlType type)
		{
			return type switch
			{
				SqlType.String => "string", 
				SqlType.Integer => "integer", 
				SqlType.Decimal => "numeric", 
				SqlType.Numeric => "float", 
				SqlType.Date => "date", 
				SqlType.Time => "time", 
				SqlType.DateTime => "datetime", 
				SqlType.Boolean => "boolean", 
				_ => throw new ArgumentOutOfRangeException(Resource.UnknownType + " " + type), 
			};
		}
	}
}
