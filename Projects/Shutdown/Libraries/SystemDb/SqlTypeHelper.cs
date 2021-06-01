using System.Data;
using DbAccess.Enums;

namespace SystemDb
{
	public static class SqlTypeHelper
	{
		public static SqlType ColumnTypeToSqlType(DbColumnTypes columnType)
		{
			switch (columnType)
			{
			case DbColumnTypes.DbNumeric:
				return SqlType.Decimal;
			case DbColumnTypes.DbInt:
			case DbColumnTypes.DbBigInt:
				return SqlType.Integer;
			case DbColumnTypes.DbBool:
				return SqlType.Boolean;
			case DbColumnTypes.DbDate:
				return SqlType.Date;
			case DbColumnTypes.DbDateTime:
				return SqlType.DateTime;
			case DbColumnTypes.DbText:
			case DbColumnTypes.DbLongText:
			case DbColumnTypes.DbUnknown:
				return SqlType.String;
			case DbColumnTypes.DbBinary:
				return SqlType.Binary;
			case DbColumnTypes.DbTime:
				return SqlType.Time;
			default:
				return SqlType.Undefined;
			}
		}

		public static DbType SqlTypeToDbType(SqlType columnType)
		{
			return columnType switch
			{
				SqlType.Boolean => DbType.Boolean, 
				SqlType.Date => DbType.Date, 
				SqlType.DateTime => DbType.DateTime, 
				SqlType.Decimal => DbType.Decimal, 
				SqlType.Integer => DbType.Int32, 
				SqlType.Numeric => DbType.VarNumeric, 
				SqlType.String => DbType.String, 
				SqlType.Time => DbType.Time, 
				_ => DbType.String, 
			};
		}
	}
}
