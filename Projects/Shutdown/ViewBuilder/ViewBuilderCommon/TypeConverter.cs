using System;
using SystemDb;
using ViewBuilderCommon.Resources;

namespace ViewBuilderCommon
{
    public class TypeConverter
    {
        public static SqlType TypeFromString(string typeString)
        {
            SqlType type;
            switch (typeString.ToLower())
            {
                case "string":
                case "char":
                    type = SqlType.String;
                    break;
                case "natural number":
                    type = SqlType.Integer;
                    break;
                case "numeric":
                    type = SqlType.Decimal;
                    break;
                case "date":
                    type = SqlType.Date;
                    break;
                case "time":
                    type = SqlType.Time;
                    break;
                case "datetime":
                    type = SqlType.DateTime;
                    break;
                case "integer":
                    type = SqlType.Integer;
                    break;
                    //Numeric is not used
                case "float":
                    type = SqlType.Numeric;
                    break;
                case "int":
                    type = SqlType.Integer;
                    break;
                case "number":
                    type = SqlType.Integer;
                    break;
                case "bool":
                case "boolean":
                    type = SqlType.Boolean;
                    break;
                default:
                    throw new Exception(Resource.UnknownParameterType + " " + typeString);
            }
            return type;
        }

        public static string GetSystemDbType(SqlType Type)
        {
            switch (Type)
            {
                case SqlType.String:
                    return "string";
                case SqlType.Integer:
                    return "integer";
                case SqlType.Decimal:
                    return "numeric";
                case SqlType.Numeric:
                    return "float";
                case SqlType.Date:
                    return "date";
                case SqlType.Time:
                    return "time";
                case SqlType.DateTime:
                    return "datetime";
                case SqlType.Boolean:
                    return "boolean";
                default:
                    throw new ArgumentOutOfRangeException(Resource.UnknownType + " " + Type.ToString());
            }
        }
    }
}