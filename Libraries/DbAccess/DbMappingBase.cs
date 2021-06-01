using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using AV.Log;
using DbAccess.Attributes;
using DbAccess.Enums;
using DbAccess.Exceptions;
using DbAccess.Structures;
using log4net;

namespace DbAccess
{
	public class DbMappingBase
	{
		private enum LoadingType
		{
			IsDbMapping,
			IsEnum,
			IsBoolean,
			IsGenericArg,
			IsTimeSpan,
			IsOther
		}

		private readonly ILog Log = LogHelper.GetLogger();

		private readonly Dictionary<Type, string> Property = new Dictionary<Type, string>();

		private static bool CheckForceInnoDbFlag { get; set; }

		protected DatabaseBase Database { get; private set; }

		public DbMappingBase(DatabaseBase database)
		{
			Database = database;
		}

		public bool IsDbMapping(Type type)
		{
			return Attribute.GetCustomAttributes(type).OfType<DbTableAttribute>().Any();
		}

		public void SetTableName<T>(string tableName)
		{
			Type type = typeof(T);
			Attribute[] customAttributes = Attribute.GetCustomAttributes(type);
			for (int i = 0; i < customAttributes.Length; i++)
			{
				if (customAttributes[i] is DbTableAttribute)
				{
					if (Property.ContainsKey(type))
					{
						Property[type] = tableName;
					}
					else
					{
						Property.Add(type, tableName);
					}
				}
			}
		}

		public string GetTableName(Type type)
		{
			if (Property.ContainsKey(type))
			{
				return Property[type];
			}
			using (IEnumerator<DbTableAttribute> enumerator = Attribute.GetCustomAttributes(type, inherit: false).OfType<DbTableAttribute>().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					DbTableAttribute current = enumerator.Current;
					_ = CheckForceInnoDbFlag;
					return current.Name;
				}
			}
			throw new MissingDbTableException(type.FullName);
		}

		public string GetEnquotedTableName(Type type)
		{
			return Database.Enquote(GetTableName(type));
		}

		public string GetEnquotedTableName(object entity)
		{
			return Database.Enquote(GetTableName(entity.GetType()));
		}

		protected DbTableAttribute GetTableAttribute(Type type)
		{
			if (Property.ContainsKey(type))
			{
				return null;
			}
			Attribute[] customAttributes = Attribute.GetCustomAttributes(type, inherit: false);
			foreach (Attribute attr in customAttributes)
			{
				if (attr is DbTableAttribute)
				{
					return (DbTableAttribute)attr;
				}
			}
			throw new MissingDbTableException(type.FullName);
		}

		protected string GetFullTableName(Type type)
		{
			return GetTableName(type);
		}

		public string GetTableName<T>()
		{
			return GetTableName(typeof(T));
		}

		public string GetTableDescription<T>()
		{
			Attribute[] customAttributes = Attribute.GetCustomAttributes(typeof(T));
			foreach (Attribute attr in customAttributes)
			{
				if (attr is DbTableAttribute)
				{
					return ((DbTableAttribute)attr).Description;
				}
			}
			throw new MissingDbTableException(typeof(T).FullName);
		}

		public string GetColumnName<T>(string propertyName)
		{
			return GetColumnName(typeof(T), propertyName);
		}

		private string GetColumnName(Type type, string propertyName)
		{
			PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property == null)
			{
				throw new Exception("the property '" + propertyName + "' does not exist in class '" + type.FullName);
			}
			Attribute[] customAttributes = Attribute.GetCustomAttributes(property);
			foreach (Attribute attr in customAttributes)
			{
				if (attr is DbColumnAttribute)
				{
					return ((DbColumnAttribute)attr).Name;
				}
			}
			throw new MissingDbColumnException(type.FullName, propertyName);
		}

		protected PropertyInfo GetPrimaryKeyProperty(Type type)
		{
			PropertyInfo result = null;
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (PropertyInfo pi in properties)
			{
				Attribute[] customAttributes = Attribute.GetCustomAttributes(pi);
				for (int j = 0; j < customAttributes.Length; j++)
				{
					if (customAttributes[j] is DbPrimaryKeyAttribute)
					{
						result = pi;
					}
					if (result != null)
					{
						break;
					}
				}
			}
			if (result == null)
			{
				throw new DbAccess.Exceptions.MissingPrimaryKeyException(type.FullName);
			}
			return result;
		}

		private string GetPrimaryKeyName(Type type)
		{
			bool isPK = false;
			string result = null;
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0; i < properties.Length; i++)
			{
				Attribute[] customAttributes = Attribute.GetCustomAttributes(properties[i]);
				foreach (Attribute attr in customAttributes)
				{
					if (attr is DbPrimaryKeyAttribute)
					{
						if (isPK)
						{
							throw new InvalidOperationException("This operation is permitted only for one-column primary keys");
						}
						isPK = true;
					}
					if (attr is DbColumnAttribute)
					{
						result = ((DbColumnAttribute)attr).Name;
					}
				}
				if (isPK)
				{
					break;
				}
			}
			if (!isPK)
			{
				throw new DbAccess.Exceptions.MissingPrimaryKeyException(type.FullName);
			}
			return result;
		}

		private void ExtractInformationFromTable(Type type, Dictionary<DbColumnAttribute, PropertyInfo> columns, List<string> primaryKeyColumns, Dictionary<string, List<string>> indizes, Dictionary<string, List<string>> uniqueKeys, Dictionary<string, List<KeyValuePair<string, DbForeignKeyAttribute>>> foreignKeys, Dictionary<DbRelationAttribute, PropertyInfo> relations)
		{
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (PropertyInfo pi in properties)
			{
				DbColumnAttribute dbColumn = null;
				DbPrimaryKeyAttribute dbPrimaryKey = null;
				DbUniqueKeyAttribute dbUniqueKey = null;
				DbIndexAttribute dbIndex = null;
				DbForeignKeyAttribute dbForeignKey = null;
				DbRelationAttribute dbRelation = null;
				Attribute[] customAttributes = Attribute.GetCustomAttributes(pi);
				foreach (Attribute attr in customAttributes)
				{
					if (attr is DbColumnAttribute)
					{
						dbColumn = attr as DbColumnAttribute;
					}
					else if (attr is DbPrimaryKeyAttribute)
					{
						dbPrimaryKey = attr as DbPrimaryKeyAttribute;
					}
					else if (attr is DbUniqueKeyAttribute)
					{
						dbUniqueKey = attr as DbUniqueKeyAttribute;
					}
					else if (attr is DbIndexAttribute)
					{
						dbIndex = attr as DbIndexAttribute;
					}
					else if (attr is DbForeignKeyAttribute)
					{
						dbForeignKey = attr as DbForeignKeyAttribute;
					}
					else if (attr is DbRelationAttribute)
					{
						dbRelation = attr as DbRelationAttribute;
					}
				}
				if (dbColumn == null)
				{
					continue;
				}
				if (dbPrimaryKey != null)
				{
					primaryKeyColumns.Add(dbColumn.Name);
				}
				if (dbUniqueKey != null)
				{
					if (string.IsNullOrEmpty(dbUniqueKey.Name))
					{
						dbUniqueKey.Name = "uk_" + Guid.NewGuid().ToString("N");
					}
					if (!uniqueKeys.ContainsKey(dbUniqueKey.Name))
					{
						uniqueKeys[dbUniqueKey.Name] = new List<string>();
					}
					uniqueKeys[dbUniqueKey.Name].Add(dbColumn.Name);
				}
				if (dbIndex != null)
				{
					if (string.IsNullOrEmpty(dbIndex.Name))
					{
						dbIndex.Name = "idx_" + Guid.NewGuid().ToString("N");
					}
					if (!indizes.ContainsKey(dbIndex.Name))
					{
						indizes[dbIndex.Name] = new List<string>();
					}
					indizes[dbIndex.Name].Add(dbColumn.Name);
				}
				if (dbForeignKey != null)
				{
					if (!foreignKeys.ContainsKey(dbColumn.Name))
					{
						foreignKeys[dbColumn.Name] = new List<KeyValuePair<string, DbForeignKeyAttribute>>();
					}
					foreignKeys[dbForeignKey.Name].Add(new KeyValuePair<string, DbForeignKeyAttribute>(dbColumn.Name, dbForeignKey));
				}
				if (dbRelation != null)
				{
					relations[dbRelation] = pi;
				}
				columns[dbColumn] = pi;
			}
		}

		public string GetCreateTableSqlStatement(Type T)
		{
			Dictionary<DbColumnAttribute, PropertyInfo> columns = new Dictionary<DbColumnAttribute, PropertyInfo>();
			List<string> primaryKeyColumns = new List<string>();
			Dictionary<string, List<string>> indizes = new Dictionary<string, List<string>>();
			Dictionary<string, List<string>> uniqueKeys = new Dictionary<string, List<string>>();
			Dictionary<string, List<KeyValuePair<string, DbForeignKeyAttribute>>> foreignKeys = new Dictionary<string, List<KeyValuePair<string, DbForeignKeyAttribute>>>();
			Dictionary<DbRelationAttribute, PropertyInfo> relations = new Dictionary<DbRelationAttribute, PropertyInfo>();
			ExtractInformationFromTable(T, columns, primaryKeyColumns, indizes, uniqueKeys, foreignKeys, relations);
			return GenerateCreateTableCommand(T, GetTableName(T), columns, primaryKeyColumns, indizes, uniqueKeys, foreignKeys);
		}

		public string GetIndicesValuePair(Type T)
		{
			Dictionary<DbColumnAttribute, PropertyInfo> columns = new Dictionary<DbColumnAttribute, PropertyInfo>();
			List<string> primaryKeyColumns = new List<string>();
			Dictionary<string, List<string>> indizes = new Dictionary<string, List<string>>();
			Dictionary<string, List<string>> uniqueKeys = new Dictionary<string, List<string>>();
			Dictionary<string, List<KeyValuePair<string, DbForeignKeyAttribute>>> foreignKeys = new Dictionary<string, List<KeyValuePair<string, DbForeignKeyAttribute>>>();
			Dictionary<DbRelationAttribute, PropertyInfo> relations = new Dictionary<DbRelationAttribute, PropertyInfo>();
			ExtractInformationFromTable(T, columns, primaryKeyColumns, indizes, uniqueKeys, foreignKeys, relations);
			GenerateCreateTableCommand(T, GetTableName(T), columns, primaryKeyColumns, indizes, uniqueKeys, foreignKeys);
			string result = "info = new IndexInformation();";
			result = result + "info.tableName = \"" + GetTableName(T) + "\";" + Environment.NewLine;
			result += "info.indices = new List<KeyValuePair<string, List<string>>>(); ";
			foreach (KeyValuePair<string, List<string>> index in indizes)
			{
				result = result + "info.indices.Add(new KeyValuePair<string, List<string>> (\"" + index.Key + "\",new List<string>{";
				for (int i = 0; i < index.Value.Count; i++)
				{
					result = result + "\"" + index.Value[i] + "\"";
					if (i != index.Value.Count - 1)
					{
						result += ",";
					}
				}
				result = result + "}));" + Environment.NewLine;
			}
			return result + "indexInfo.Add(info);" + Environment.NewLine;
		}

		public void CreateTableIfNotExists<T>()
		{
			Type type = typeof(T);
			string tableName = GetTableName<T>();
			if (Database.TableExists(tableName))
			{
				return;
			}
			Dictionary<DbColumnAttribute, PropertyInfo> columns = new Dictionary<DbColumnAttribute, PropertyInfo>();
			List<string> primaryKeyColumns = new List<string>();
			Dictionary<string, List<string>> indizes = new Dictionary<string, List<string>>();
			Dictionary<string, List<string>> uniqueKeys = new Dictionary<string, List<string>>();
			Dictionary<string, List<KeyValuePair<string, DbForeignKeyAttribute>>> foreignKeys = new Dictionary<string, List<KeyValuePair<string, DbForeignKeyAttribute>>>();
			Dictionary<DbRelationAttribute, PropertyInfo> relations = new Dictionary<DbRelationAttribute, PropertyInfo>();
			ExtractInformationFromTable(type, columns, primaryKeyColumns, indizes, uniqueKeys, foreignKeys, relations);
			if (tableName == "user_free_selection_parameter_history")
			{
				Dictionary<DbColumnAttribute, PropertyInfo> items = columns;
				for (int i = 0; i < items.Count; i++)
				{
					if (items.ElementAt(i).Key.Name == "user_defined" || items.ElementAt(i).Key.Name == "ordinal" || items.ElementAt(i).Key.Name == "name")
					{
						columns.Remove(items.ElementAt(i).Key);
						i--;
					}
				}
			}
			string sql = GenerateCreateTableCommand(type, tableName, columns, primaryKeyColumns, indizes, uniqueKeys, foreignKeys, ifNotExist: true);
			try
			{
				Database.ExecuteNonQuery(sql);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			foreach (KeyValuePair<DbRelationAttribute, PropertyInfo> kv in relations)
			{
				if (string.IsNullOrEmpty(kv.Key.RelationTableName))
				{
					kv.Key.RelationTableName = $"{tableName.ToLower()}_{GetTableName(kv.Value.GetType()).ToLower()}";
				}
				if (string.IsNullOrEmpty(kv.Key.ColumnName))
				{
					kv.Key.ColumnName = kv.Value.Name.FromCamelCaseToLowerUnderscore();
				}
				if (string.IsNullOrEmpty(kv.Key.ForeignName))
				{
					kv.Key.ForeignName = kv.Value.Name.FromCamelCaseToLowerUnderscore();
				}
			}
			foreach (KeyValuePair<string, List<string>> index in indizes)
			{
				try
				{
					CreateIndex(tableName, index);
				}
				catch (Exception)
				{
				}
			}
		}

		private void CreateIndex(string tableName, KeyValuePair<string, List<string>> index)
		{
			Database.CreateIndex(tableName, index.Key, index.Value);
		}

		protected string GenerateCreateTableCommand(Type type, string tableName, Dictionary<DbColumnAttribute, PropertyInfo> columns, List<string> primaryKeyColumns, Dictionary<string, List<string>> indizes, Dictionary<string, List<string>> uniqueKeys, Dictionary<string, List<KeyValuePair<string, DbForeignKeyAttribute>>> foreignKeys, bool ifNotExist = false)
		{
			StringBuilder sbSql = new StringBuilder();
			if (ifNotExist)
			{
				sbSql.Append("CREATE TABLE IF NOT EXISTS ");
			}
			else
			{
				sbSql.Append("CREATE TABLE ");
			}
			sbSql.Append(Database.Enquote(tableName) + "(");
			foreach (DbColumnAttribute dbColumn in columns.Keys)
			{
				PropertyInfo pi = columns[dbColumn];
				Type columnType = columns[dbColumn].PropertyType;
				if (columnType.Name.Equals("Nullable`1"))
				{
					if (columnType == typeof(DateTime?))
					{
						sbSql.Append(GetColumnDefinition("DateTime", dbColumn, pi) + ",");
					}
					else if (columnType == typeof(int?))
					{
						sbSql.Append(GetColumnDefinition("Int32", dbColumn, pi) + ",");
					}
					else if (columnType == typeof(long?))
					{
						sbSql.Append(GetColumnDefinition("Int64", dbColumn, pi) + ",");
					}
					else if (columnType == typeof(uint?))
					{
						sbSql.Append(GetColumnDefinition("UInt32", dbColumn, pi) + ",");
					}
					else if (columnType == typeof(ulong?))
					{
						sbSql.Append(GetColumnDefinition("UInt64", dbColumn, pi) + ",");
					}
					else if (columnType == typeof(bool?))
					{
						sbSql.Append(GetColumnDefinition("Boolean", dbColumn, pi) + ",");
					}
					else if (columnType == typeof(float?))
					{
						sbSql.Append(GetColumnDefinition("Float", dbColumn, pi) + ",");
					}
					else if (columnType == typeof(double?))
					{
						sbSql.Append(GetColumnDefinition("Double", dbColumn, pi) + ",");
					}
					else if (columnType == typeof(decimal?))
					{
						sbSql.Append(GetColumnDefinition("Decimal", dbColumn, pi) + ",");
					}
					else if (columnType == typeof(TimeSpan?))
					{
						sbSql.Append(GetColumnDefinition("TimeSpan", dbColumn, pi) + ",");
					}
				}
				else if (IsDbMapping(columnType))
				{
					sbSql.Append(GetColumnDefinition("DbMappingKey", dbColumn, pi) + ",");
					PropertyInfo primaryKeyPI = null;
					PropertyInfo[] properties = columnType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					Attribute[] customAttributes;
					foreach (PropertyInfo fpi in properties)
					{
						customAttributes = Attribute.GetCustomAttributes(fpi);
						for (int j = 0; j < customAttributes.Length; j++)
						{
							if (customAttributes[j] is DbPrimaryKeyAttribute)
							{
								primaryKeyPI = fpi;
								break;
							}
						}
						if (primaryKeyPI != null)
						{
							break;
						}
					}
					string primaryKeyColumn = null;
					customAttributes = Attribute.GetCustomAttributes(primaryKeyPI);
					foreach (Attribute attr in customAttributes)
					{
						if (attr is DbColumnAttribute)
						{
							primaryKeyColumn = ((DbColumnAttribute)attr).Name;
							break;
						}
					}
					bool foundFKeyDefinition = false;
					foreach (List<KeyValuePair<string, DbForeignKeyAttribute>> value in foreignKeys.Values)
					{
						foreach (KeyValuePair<string, DbForeignKeyAttribute> item in value)
						{
							if (item.Key == primaryKeyColumn)
							{
								foundFKeyDefinition = true;
								break;
							}
						}
						if (foundFKeyDefinition)
						{
							break;
						}
					}
					if (!foundFKeyDefinition)
					{
						indizes.Add("fki_" + tableName + "_" + dbColumn.Name, new List<string> { dbColumn.Name });
					}
				}
				else if (columns[dbColumn].PropertyType.BaseType == typeof(Enum))
				{
					sbSql.Append(GetColumnDefinition("Enum", dbColumn, pi) + ",");
				}
				else
				{
					sbSql.Append(GetColumnDefinition(pi.PropertyType.Name, dbColumn, pi) + ",");
				}
			}
			if (!sbSql.ToString().Contains("PRIMARY KEY"))
			{
				sbSql.Append(GetPrimaryKeyContraint(primaryKeyColumns) + ",");
			}
			foreach (string uniqueKeyName in uniqueKeys.Keys)
			{
				sbSql.Append(GetUniqueKeyConstraint(uniqueKeyName, uniqueKeys[uniqueKeyName]) + ",");
			}
			foreach (string foreignKey in foreignKeys.Keys)
			{
				sbSql.Append(GetForeignKeyConstraint(foreignKey, foreignKeys[foreignKey]) + ",");
			}
			sbSql.Remove(sbSql.Length - 1, 1).Append(")");
			return sbSql.ToString();
		}

		protected string GetPrimaryKeyContraint(IEnumerable<string> columns)
		{
			string result = "PRIMARY KEY(";
			foreach (string column in columns)
			{
				result = result + Database.Enquote(column) + ",";
			}
			result = result.Remove(result.Length - 1, 1);
			return result + ")";
		}

		protected string GetIndexConstraint(string indexName, IEnumerable<string> columns)
		{
			string result = "INDEX " + indexName + "(";
			foreach (string column in columns)
			{
				result = result + Database.Enquote(column) + ",";
			}
			result = result.Remove(result.Length - 1, 1);
			return result + ")";
		}

		protected string GetUniqueKeyConstraint(string uniqueKeyName, IEnumerable<string> columns)
		{
			string result = "UNIQUE KEY " + uniqueKeyName + "(";
			foreach (string column in columns)
			{
				result = result + Database.Enquote(column) + ",";
			}
			result = result.Remove(result.Length - 1, 1);
			return result + ")";
		}

		protected string GetForeignKeyConstraint(string fkeyName, List<KeyValuePair<string, DbForeignKeyAttribute>> columns)
		{
			string result = "FOREIGN KEY " + Database.Enquote(fkeyName) + "(";
			foreach (KeyValuePair<string, DbForeignKeyAttribute> column in columns)
			{
				result = result + Database.Enquote(column.Key) + ",";
			}
			result = result.Remove(result.Length - 1, 1);
			result = result + ") REFERENCES " + Database.Enquote(columns[0].Value.BaseTable) + "(";
			foreach (KeyValuePair<string, DbForeignKeyAttribute> column2 in columns)
			{
				_ = column2;
				result = result + Database.Enquote(columns[0].Value.BaseColumn) + ",";
			}
			result = result.Remove(result.Length - 1, 1);
			result += ")";
			if (columns[0].Value.CascadeOnDelete)
			{
				result += " ON DELETE CASCADE";
			}
			if (columns[0].Value.CascadeOnUpdate)
			{
				result += " ON UPDATE CASCADE";
			}
			return result;
		}

		public void DropTableIfExists<T>()
		{
			Database.DropTableIfExists(GetFullTableName(typeof(T)));
		}

		private bool HasDbMappingProperties(Type type)
		{
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (PropertyInfo pi in properties)
			{
				Attribute[] customAttributes;
				if (IsDbMapping(pi.PropertyType))
				{
					customAttributes = Attribute.GetCustomAttributes(pi);
					foreach (Attribute attr in customAttributes)
					{
						if (attr is DbColumnAttribute && !(attr as DbColumnAttribute).IsInverseMapping)
						{
							return true;
						}
					}
				}
				customAttributes = Attribute.GetCustomAttributes(pi);
				for (int j = 0; j < customAttributes.Length; j++)
				{
					if (customAttributes[j] is DbCollectionAttribute)
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool ExistsValue(Type type, string filter)
		{
			using IDataReader reader = Database.ExecuteReader("SELECT 1 FROM " + Database.Enquote(Database.DbMapping.GetTableName(type)) + " WHERE " + filter);
			return ((DbDataReader)reader).HasRows;
		}

		protected long GetMaxInsertId(string tableName, string idColumn)
		{
			object obj = Database.ExecuteScalar("SELECT MAX(" + Database.Enquote(idColumn) + ") FROM " + Database.Enquote(tableName));
			if (obj is DBNull)
			{
				return 0L;
			}
			return Convert.ToInt64(obj);
		}

		public string GetColumnDefinitionFromColumnInfo(DbColumnInfo columnInfo)
		{
			DbColumnAttribute attr = new DbColumnAttribute(columnInfo.Name)
			{
				AllowDbNull = columnInfo.AllowDBNull,
				AutoIncrement = columnInfo.AutoIncrement,
				AutoLoad = false,
				CascadeOnDelete = false,
				Description = string.Empty,
				IsInverseMapping = false,
				Length = columnInfo.MaxLength
			};
			string type;
			switch (columnInfo.Type)
			{
			case DbColumnTypes.DbNumeric:
				type = "Double";
				break;
			case DbColumnTypes.DbInt:
				type = "Int32";
				break;
			case DbColumnTypes.DbBigInt:
				type = "Int64";
				break;
			case DbColumnTypes.DbBool:
				type = "Boolean";
				break;
			case DbColumnTypes.DbText:
			case DbColumnTypes.DbLongText:
				type = "String";
				break;
			case DbColumnTypes.DbDate:
			case DbColumnTypes.DbTime:
			case DbColumnTypes.DbDateTime:
				type = "DateTime";
				break;
			default:
				throw new ArgumentOutOfRangeException("Type " + columnInfo.Type.ToString() + " is not implemented.");
			}
			return GetColumnDefinition(type, attr, null);
		}

		private IEnumerable<object> LoadOld(Type type, Dictionary<PropertyInfo, DbColumnAttribute> columns, string sql)
		{
			List<object> results = new List<object>();
			Dictionary<object, Dictionary<PropertyInfo, int>> dbMappingObjects = new Dictionary<object, Dictionary<PropertyInfo, int>>();
			using (IDataReader reader = Database.ExecuteReader(sql))
			{
				while (reader.Read())
				{
					object obj3 = type.GetConstructor(new Type[0]).Invoke(new object[0]);
					Dictionary<PropertyInfo, int> actualMappingObjects = new Dictionary<PropertyInfo, int>();
					int i = 0;
					foreach (PropertyInfo pi3 in columns.Keys)
					{
						if (reader.IsDBNull(i))
						{
							pi3.SetValue(obj3, null, null);
						}
						else if (IsDbMapping(pi3.PropertyType))
						{
							Attribute[] customAttributes = Attribute.GetCustomAttributes(pi3);
							foreach (Attribute attr2 in customAttributes)
							{
								if (attr2 is DbColumnAttribute)
								{
									DbColumnAttribute col = (DbColumnAttribute)attr2;
									if (col.AutoLoad && !col.IsInverseMapping)
									{
										actualMappingObjects.Add(pi3, Convert.ToInt32(reader[i]));
									}
								}
							}
						}
						else if (pi3.PropertyType.BaseType == typeof(Enum))
						{
							pi3.SetValue(obj3, Enum.Parse(pi3.PropertyType, reader[i].ToString()), null);
						}
						else if (pi3.PropertyType.Name == "Boolean")
						{
							pi3.SetValue(obj3, Convert.ToBoolean(reader[i]), null);
						}
						else if (pi3.PropertyType.GetGenericArguments() != null && pi3.PropertyType.GetGenericArguments().Length != 0 && pi3.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
						{
							Type genericType = pi3.PropertyType.GetGenericArguments()[0];
							pi3.SetValue(obj3, Convert.ChangeType(reader[i], genericType), null);
						}
						else
						{
							pi3.SetValue(obj3, Convert.ChangeType(reader[i], pi3.PropertyType), null);
						}
						i++;
					}
					dbMappingObjects.Add(obj3, actualMappingObjects);
					results.Add(obj3);
				}
			}
			foreach (object obj2 in dbMappingObjects.Keys)
			{
				foreach (PropertyInfo pi2 in dbMappingObjects[obj2].Keys)
				{
					LoadChild(obj2, pi2, dbMappingObjects[obj2][pi2]);
				}
			}
			foreach (object obj in results)
			{
				PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (PropertyInfo pi in properties)
				{
					Attribute[] customAttributes = Attribute.GetCustomAttributes(pi);
					foreach (Attribute attr in customAttributes)
					{
						if (attr is DbCollectionAttribute && !((DbCollectionAttribute)attr).LazyLoad)
						{
							LoadCollection((DbCollectionAttribute)attr, obj, pi);
						}
					}
				}
			}
			return results;
		}

		public List<T> FastLoad<T>() where T : new()
		{
			Dictionary<PropertyInfo, DbColumnAttribute> columns;
			string sql = GetLoadSql(typeof(T), out columns);
			return FastLoad<T>(columns, sql);
		}

		private List<T> FastLoad<T>(Dictionary<PropertyInfo, DbColumnAttribute> columns, string sql)
		{
			lock ("lock")
			{
				Stopwatch watch = null;
				if (LogHelper.PerformanceLogging)
				{
					watch = new Stopwatch();
					watch.Start();
				}
				Type type = typeof(T);
				List<T> results = new List<T>();
				List<Tuple<PropertyInfo, LoadingType>> columnLoadingType = new List<Tuple<PropertyInfo, LoadingType>>(columns.Count);
				foreach (PropertyInfo pi2 in columns.Keys)
				{
					if (IsDbMapping(pi2.PropertyType))
					{
						columnLoadingType.Add(new Tuple<PropertyInfo, LoadingType>(pi2, LoadingType.IsDbMapping));
					}
					else if (pi2.PropertyType.BaseType == typeof(Enum))
					{
						columnLoadingType.Add(new Tuple<PropertyInfo, LoadingType>(pi2, LoadingType.IsEnum));
					}
					else if (pi2.PropertyType.Name == "Boolean")
					{
						columnLoadingType.Add(new Tuple<PropertyInfo, LoadingType>(pi2, LoadingType.IsBoolean));
					}
					else if (pi2.PropertyType.Name == "TimeSpan")
					{
						columnLoadingType.Add(new Tuple<PropertyInfo, LoadingType>(pi2, LoadingType.IsTimeSpan));
					}
					else if (pi2.PropertyType.GetGenericArguments().Length != 0 && pi2.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
					{
						columnLoadingType.Add(new Tuple<PropertyInfo, LoadingType>(pi2, LoadingType.IsGenericArg));
					}
					else
					{
						columnLoadingType.Add(new Tuple<PropertyInfo, LoadingType>(pi2, LoadingType.IsOther));
					}
				}
				List<Tuple<object, List<Tuple<PropertyInfo, int>>>> dbMappingObjects = new List<Tuple<object, List<Tuple<PropertyInfo, int>>>>();
				IDataReader reader = Database.ExecuteReader(sql);
				try
				{
					while (reader.Read())
					{
						List<Tuple<PropertyInfo, int>> mappingObjects = new List<Tuple<PropertyInfo, int>>();
						T obj2 = (T)Activator.CreateInstance(type);
						int i = 0;
						foreach (Tuple<PropertyInfo, LoadingType> tuple in columnLoadingType)
						{
							if (reader.IsDBNull(i))
							{
								tuple.Item1.SetValue(obj2, null, null);
							}
							else
							{
								switch (tuple.Item2)
								{
								case LoadingType.IsDbMapping:
									mappingObjects.AddRange(from col in Attribute.GetCustomAttributes(tuple.Item1).OfType<DbColumnAttribute>()
										where col.AutoLoad && !col.IsInverseMapping
										select new Tuple<PropertyInfo, int>(tuple.Item1, Convert.ToInt32(reader[i])));
									break;
								case LoadingType.IsEnum:
									tuple.Item1.SetValue(obj2, Enum.Parse(tuple.Item1.PropertyType, reader[i].ToString()), null);
									break;
								case LoadingType.IsBoolean:
									tuple.Item1.SetValue(obj2, Convert.ToBoolean(reader[i]), null);
									break;
								case LoadingType.IsGenericArg:
								{
									Type genericType = tuple.Item1.PropertyType.GetGenericArguments()[0];
									tuple.Item1.SetValue(obj2, Convert.ChangeType(reader[i], genericType), null);
									break;
								}
								case LoadingType.IsTimeSpan:
									tuple.Item1.SetValue(obj2, new TimeSpan((long)reader[i]), null);
									break;
								case LoadingType.IsOther:
									tuple.Item1.SetValue(obj2, Convert.ChangeType(reader[i], tuple.Item1.PropertyType), null);
									break;
								default:
									throw new ArgumentOutOfRangeException();
								}
							}
							int num = i;
							i = num + 1;
						}
						if (mappingObjects.Count > 0)
						{
							dbMappingObjects.Add(new Tuple<object, List<Tuple<PropertyInfo, int>>>(obj2, mappingObjects));
						}
						results.Add(obj2);
					}
				}
				finally
				{
					if (reader != null)
					{
						reader.Dispose();
					}
				}
				foreach (Tuple<object, List<Tuple<PropertyInfo, int>>> objToProperties in dbMappingObjects)
				{
					foreach (Tuple<PropertyInfo, int> properties in objToProperties.Item2)
					{
						LoadChild(objToProperties.Item1, properties.Item1, properties.Item2);
					}
				}
				PropertyInfo[] properties2 = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (PropertyInfo pi in properties2)
				{
					Attribute[] customAttributes = Attribute.GetCustomAttributes(pi);
					foreach (Attribute attr in customAttributes)
					{
						foreach (T item in results)
						{
							object obj = item;
							if (attr is DbCollectionAttribute && !((DbCollectionAttribute)attr).LazyLoad)
							{
								LoadCollectionFast((DbCollectionAttribute)attr, obj, pi);
							}
						}
					}
				}
				if (LogHelper.PerformanceLogging && watch != null && watch.ElapsedMilliseconds > 150)
				{
					watch.Stop();
					Log.Log(LogLevelEnum.Debug, "long running " + watch.ElapsedMilliseconds + " " + sql);
				}
				return results;
			}
		}

		public List<T> FastLoad<T>(string filter, bool ignoreLazyLoad = false) where T : new()
		{
			Dictionary<PropertyInfo, DbColumnAttribute> columns;
			string sql = GetLoadSql(typeof(T), out columns, ignoreLazyLoad) + " WHERE " + filter;
			return FastLoad<T>(columns, sql);
		}

		private List<T> Load<T>(Dictionary<PropertyInfo, DbColumnAttribute> columns, string sql) where T : new()
		{
			return FastLoad<T>(columns, sql);
		}

		public List<T> Load<T>() where T : new()
		{
			Dictionary<PropertyInfo, DbColumnAttribute> columns;
			string sql = GetLoadSql(typeof(T), out columns);
			return Load<T>(columns, sql);
		}

		public List<T> LoadBySQL<T>(string sql) where T : new()
		{
			GetLoadSql(typeof(T), out var columns);
			return Load<T>(columns, sql);
		}

		public List<T> Load<T>(string filter, bool ignoreLazyLoad = false) where T : new()
		{
			Dictionary<PropertyInfo, DbColumnAttribute> columns;
			string sql = GetLoadSql(typeof(T), out columns, ignoreLazyLoad) + " WHERE " + filter;
			return Load<T>(columns, sql);
		}

		public T Load<T>(object id) where T : new()
		{
			Dictionary<PropertyInfo, DbColumnAttribute> columns;
			string sql = GetLoadSql(typeof(T), out columns) + " WHERE " + Database.Enquote(GetPrimaryKeyName(typeof(T))) + "=" + Database.GetSqlString(id.ToString());
			List<T> tmp = Load<T>(columns, sql);
			if (tmp.Count > 0)
			{
				return tmp[0];
			}
			return default(T);
		}

		public List<T> LoadSorted<T>(string orderColumns) where T : new()
		{
			Dictionary<PropertyInfo, DbColumnAttribute> columns;
			string sql = GetLoadSql(typeof(T), out columns) + " ORDER BY " + orderColumns;
			return Load<T>(columns, sql);
		}

		public List<T> LoadSorted<T>(string filter, string orderColumns) where T : new()
		{
			Dictionary<PropertyInfo, DbColumnAttribute> columns;
			string sql = GetLoadSql(typeof(T), out columns) + " WHERE " + filter + " ORDER BY " + orderColumns;
			return Load<T>(columns, sql);
		}

		private void LoadChild(object parent, PropertyInfo childProperty, int idValue)
		{
			Dictionary<PropertyInfo, DbColumnAttribute> columns;
			string sql = GetLoadSql(childProperty.PropertyType, out columns) + " WHERE " + Database.Enquote(GetPrimaryKeyName(childProperty.PropertyType)) + "=" + Database.GetSqlString(idValue.ToString(CultureInfo.InvariantCulture));
			IEnumerable tmp = LoadOld(childProperty.PropertyType, columns, sql);
			if (((IList)tmp).Count > 0)
			{
				childProperty.SetValue(parent, ((IList)tmp)[0], null);
			}
		}

		private void LoadCollection(DbCollectionAttribute dbCollectionAttr, object obj, PropertyInfo pi)
		{
			int key = Convert.ToInt32(GetPrimaryKeyProperty(obj.GetType()).GetValue(obj, null));
			if (pi.PropertyType.GetGenericArguments() == null || pi.PropertyType.GetGenericArguments().Length == 0)
			{
				return;
			}
			Type genericType = pi.PropertyType.GetGenericArguments()[0];
			string fkName = GetColumnName(genericType, dbCollectionAttr.RefProperty);
			Dictionary<PropertyInfo, DbColumnAttribute> columns;
			string sql = GetLoadSql(genericType, out columns) + " WHERE " + Database.Enquote(fkName) + "=" + key;
			IList targetList = (IList)pi.GetValue(obj, null);
			List<object> tmp = new List<object>(LoadOld(genericType, columns, sql));
			if (dbCollectionAttr.SortOnLoad)
			{
				tmp.Sort();
			}
			foreach (object targetObj in tmp)
			{
				genericType.GetProperty(dbCollectionAttr.RefProperty, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(targetObj, obj, null);
				targetList.Add(targetObj);
			}
		}

		private void LoadCollectionFast(DbCollectionAttribute dbCollectionAttr, object obj, PropertyInfo pi)
		{
			int key = Convert.ToInt32(GetPrimaryKeyProperty(obj.GetType()).GetValue(obj, null));
			if (pi.PropertyType.GetGenericArguments() == null || pi.PropertyType.GetGenericArguments().Length == 0)
			{
				return;
			}
			Type genericType = pi.PropertyType.GetGenericArguments()[0];
			string fkName = GetColumnName(genericType, dbCollectionAttr.RefProperty);
			Dictionary<PropertyInfo, DbColumnAttribute> columns;
			string sql = GetLoadSql(genericType, out columns) + " WHERE " + Database.Enquote(fkName) + "=" + key;
			IList targetList = (IList)pi.GetValue(obj, null);
			IEnumerable<object> tmp = typeof(DbMappingBase).GetMethod("FastLoad", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[2]
			{
				typeof(Dictionary<PropertyInfo, DbColumnAttribute>),
				typeof(string)
			}, null).MakeGenericMethod(genericType).Invoke(this, new object[2] { columns, sql }) as IEnumerable<object>;
			if (dbCollectionAttr.SortOnLoad)
			{
				tmp = tmp.OrderBy((object temp) => temp);
			}
			foreach (object targetObj in tmp)
			{
				genericType.GetProperty(dbCollectionAttr.RefProperty, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(targetObj, obj, null);
				targetList.Add(targetObj);
			}
		}

		private string GetLoadSql(Type type, out Dictionary<PropertyInfo, DbColumnAttribute> columns, bool ignoreLazyLoad = false)
		{
			string tableName = GetFullTableName(type);
			columns = new Dictionary<PropertyInfo, DbColumnAttribute>();
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (PropertyInfo pi in properties)
			{
				Attribute[] customAttributes = Attribute.GetCustomAttributes(pi);
				foreach (Attribute attr in customAttributes)
				{
					if (attr is DbColumnAttribute && (ignoreLazyLoad || ((DbColumnAttribute)attr).AutoLoad))
					{
						if (tableName == "user_free_selection_parameter_history" && pi.Name != "UserDefined" && pi.Name != "Ordinal" && pi.Name != "Name")
						{
							columns[pi] = (DbColumnAttribute)attr;
						}
						else if (tableName != "user_free_selection_parameter_history")
						{
							columns[pi] = (DbColumnAttribute)attr;
						}
						break;
					}
				}
			}
			string sqlColumns = string.Empty;
			foreach (PropertyInfo pi2 in columns.Keys)
			{
				if (sqlColumns.Length > 0)
				{
					sqlColumns += ",";
				}
				if (tableName == "user_free_selection_parameter_history" && pi2.Name != "UserDefined" && pi2.Name != "Ordinal" && pi2.Name != "Name")
				{
					sqlColumns += Database.Enquote(columns[pi2].Name);
				}
				else if (tableName != "user_free_selection_parameter_history")
				{
					sqlColumns = ((!columns[pi2].StoreAsVarBinary) ? (sqlColumns + Database.Enquote(columns[pi2].Name)) : (sqlColumns + $"CAST({Database.Enquote(columns[pi2].Name)} AS CHAR(255)) AS {columns[pi2].Name}"));
				}
			}
			return "SELECT " + sqlColumns + " FROM " + Database.Enquote(tableName);
		}

		public void Delete(string tableName, int userId, int issueId)
		{
			try
			{
				Database.DeleteHistory(tableName, userId, issueId);
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}
		}

		public void Save(object obj)
		{
			try
			{
				if (obj == null)
				{
					return;
				}
				Type type = obj.GetType();
				string tableName = GetFullTableName(type);
				long curId = 0L;
				bool autoIncrement = false;
				Dictionary<PropertyInfo, DbColumnAttribute> columns = new Dictionary<PropertyInfo, DbColumnAttribute>();
				List<PropertyInfo> dbCollections = new List<PropertyInfo>();
				string primaryKeyColumn = string.Empty;
				PropertyInfo primaryKeyPI = null;
				PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (PropertyInfo pi in properties)
				{
					if (tableName == "user_free_selection_parameter_history" && (pi.Name == "UserDefined" || pi.Name == "Ordinal" || pi.Name == "Name"))
					{
						continue;
					}
					bool foundPK = false;
					Attribute[] customAttributes = Attribute.GetCustomAttributes(pi);
					foreach (Attribute attr in customAttributes)
					{
						if (attr is DbColumnAttribute)
						{
							columns[pi] = attr as DbColumnAttribute;
						}
						if (attr is DbCollectionAttribute && (attr as DbCollectionAttribute).CascadeSave)
						{
							dbCollections.Add(pi);
						}
						if (attr is DbPrimaryKeyAttribute)
						{
							foundPK = true;
						}
					}
					if (!foundPK)
					{
						continue;
					}
					primaryKeyPI = pi;
					curId = Convert.ToInt64(primaryKeyPI.GetValue(obj, null));
					primaryKeyColumn = columns[pi].Name;
					autoIncrement = columns[pi].AutoIncrement;
					if (autoIncrement || curId != 0L)
					{
						continue;
					}
					long id = GetMaxInsertId(tableName, GetColumnName(type, primaryKeyPI.Name)) + 1;
					string name = primaryKeyPI.PropertyType.Name;
					if (!(name == "Int32"))
					{
						if (!(name == "Int64"))
						{
							throw new Exception("Type not supported for primary key column: " + primaryKeyPI.PropertyType.Name);
						}
						primaryKeyPI.SetValue(obj, id, null);
					}
					else
					{
						primaryKeyPI.SetValue(obj, (int)id, null);
					}
				}
				if (primaryKeyPI == null)
				{
					throw new DbAccess.Exceptions.MissingPrimaryKeyException(type.FullName);
				}
				DbColumnValues values = new DbColumnValues();
				foreach (PropertyInfo pi3 in columns.Keys)
				{
					if ((tableName == "user_free_selection_parameter_history" && (pi3.Name == "UserDefined" || pi3.Name == "Ordinal" || pi3.Name == "Name")) || (columns[pi3].Name == primaryKeyColumn && (autoIncrement || curId > 0)))
					{
						continue;
					}
					if (IsDbMapping(pi3.PropertyType))
					{
						object actValue2 = pi3.GetValue(obj, null);
						if (actValue2 == null)
						{
							values[columns[pi3].Name] = null;
							continue;
						}
						if (!columns[pi3].IsInverseMapping)
						{
							Save(actValue2);
						}
						values[columns[pi3].Name] = GetPrimaryKeyProperty(actValue2.GetType()).GetValue(actValue2, null);
					}
					else if (pi3.PropertyType.BaseType == typeof(Enum))
					{
						values[columns[pi3].Name] = (int)pi3.GetValue(obj, null);
					}
					else
					{
						values[columns[pi3].Name] = pi3.GetValue(obj, null);
					}
				}
				if (curId == 0L)
				{
					if (tableName.Contains("table_roles"))
					{
						Database.ReplaceInto(tableName, values);
					}
					else
					{
						Database.InsertInto(tableName, values);
					}
					if (autoIncrement)
					{
						primaryKeyPI.SetValue(obj, Convert.ToInt32(Database.GetLastInsertId()), null);
					}
				}
				else
				{
					Database.Update(tableName, values, Database.Enquote(primaryKeyColumn) + "=" + curId);
				}
				foreach (PropertyInfo pi2 in dbCollections)
				{
					object actValue = pi2.GetValue(obj, null);
					if (actValue == null)
					{
						continue;
					}
					Type genericType = pi2.PropertyType.GetGenericArguments()[0];
					if (!autoIncrement && HasDbMappingProperties(genericType))
					{
						foreach (object targetObject in (IEnumerable)actValue)
						{
							Save(targetObject);
						}
					}
					else
					{
						Save(genericType, (IEnumerable)actValue);
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}
		}

		public void Save(Type type, IEnumerable objects)
		{
			string tableName = GetFullTableName(type);
			long id = 0L;
			Dictionary<PropertyInfo, DbColumnAttribute> columns = new Dictionary<PropertyInfo, DbColumnAttribute>();
			List<PropertyInfo> dbCollections = new List<PropertyInfo>();
			string primaryKeyColumn = string.Empty;
			PropertyInfo primaryKeyPI = null;
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (PropertyInfo pi in properties)
			{
				bool foundPK = false;
				Attribute[] customAttributes = Attribute.GetCustomAttributes(pi);
				foreach (Attribute attr in customAttributes)
				{
					if (attr is DbColumnAttribute)
					{
						columns[pi] = attr as DbColumnAttribute;
					}
					if (attr is DbCollectionAttribute)
					{
						dbCollections.Add(pi);
					}
					if (attr is DbPrimaryKeyAttribute)
					{
						foundPK = true;
					}
				}
				if (foundPK)
				{
					primaryKeyColumn = columns[pi].Name;
					primaryKeyPI = pi;
					foundPK = false;
					id = GetMaxInsertId(tableName, GetColumnName(type, primaryKeyPI.Name));
				}
			}
			if (primaryKeyPI == null)
			{
				throw new DbAccess.Exceptions.MissingPrimaryKeyException(type.FullName);
			}
			List<DbColumnValues> valueCollection = new List<DbColumnValues>();
			int idOffset = 1;
			foreach (object obj in objects)
			{
				if (obj == null)
				{
					continue;
				}
				DbColumnValues values = new DbColumnValues();
				long curId = Convert.ToInt64(primaryKeyPI.GetValue(obj, null));
				if (curId == 0L)
				{
					string name = primaryKeyPI.PropertyType.Name;
					if (!(name == "Int32"))
					{
						if (!(name == "Int64"))
						{
							throw new Exception("Type not supported for primary key column: " + primaryKeyPI.PropertyType.Name);
						}
						primaryKeyPI.SetValue(obj, id + idOffset, null);
					}
					else
					{
						primaryKeyPI.SetValue(obj, (int)(id + idOffset), null);
					}
					idOffset++;
				}
				foreach (PropertyInfo pi2 in columns.Keys)
				{
					if (columns[pi2].Name == primaryKeyColumn && curId > 0)
					{
						continue;
					}
					if (IsDbMapping(pi2.PropertyType))
					{
						object actValue = null;
						actValue = pi2.GetValue(obj, null);
						if (actValue == null)
						{
							values[columns[pi2].Name] = null;
							continue;
						}
						if (!columns[pi2].IsInverseMapping)
						{
							Save(actValue);
						}
						values[columns[pi2].Name] = GetPrimaryKeyProperty(actValue.GetType()).GetValue(actValue, null);
					}
					else if (pi2.PropertyType.BaseType == typeof(Enum))
					{
						values[columns[pi2].Name] = (int)pi2.GetValue(obj, null);
					}
					else
					{
						values[columns[pi2].Name] = pi2.GetValue(obj, null);
					}
				}
				if (curId == 0L)
				{
					valueCollection.Add(values);
				}
				else
				{
					Database.Update(tableName, values, Database.Enquote(primaryKeyColumn) + "=" + curId);
				}
			}
			Database.InsertInto(tableName, valueCollection);
			SaveDbValues(objects, dbCollections);
		}

		private void SaveDbValues(IEnumerable objects, IEnumerable<PropertyInfo> dbCollections)
		{
			foreach (PropertyInfo pi in dbCollections)
			{
				Type genericType = pi.PropertyType.GetGenericArguments()[0];
				PropertyInfo primaryKeyProperty = GetPrimaryKeyProperty(genericType);
				bool autoIncrement = false;
				Attribute[] customAttributes = Attribute.GetCustomAttributes(primaryKeyProperty);
				foreach (Attribute attr in customAttributes)
				{
					if (attr is DbColumnAttribute)
					{
						autoIncrement = (attr as DbColumnAttribute).AutoIncrement;
					}
				}
				foreach (object obj in objects)
				{
					object actValue = pi.GetValue(obj, null);
					if (actValue == null)
					{
						continue;
					}
					if (autoIncrement)
					{
						foreach (object targetObject in (IEnumerable)actValue)
						{
							Save(targetObject);
						}
					}
					else
					{
						Save(genericType, (IEnumerable)actValue);
					}
				}
			}
		}

		public void Delete<T>(string filter)
		{
			Type type = typeof(T);
			string tableName = GetFullTableName(type);
			string sql = "DELETE FROM " + Database.Enquote(tableName) + " WHERE " + filter;
			Database.ExecuteNonQuery(sql);
		}

		public void Delete(object obj)
		{
			if (obj == null)
			{
				return;
			}
			Type type = obj.GetType();
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			Attribute[] customAttributes;
			foreach (PropertyInfo pi in properties)
			{
				if (IsDbMapping(pi.PropertyType))
				{
					customAttributes = Attribute.GetCustomAttributes(pi);
					foreach (Attribute attr in customAttributes)
					{
						if (attr is DbColumnAttribute)
						{
							DbColumnAttribute col = (DbColumnAttribute)attr;
							if (col.CascadeOnDelete && !col.IsInverseMapping)
							{
								object actValue2 = pi.GetValue(obj, null);
								Delete(actValue2);
							}
						}
					}
					continue;
				}
				customAttributes = Attribute.GetCustomAttributes(pi);
				for (int j = 0; j < customAttributes.Length; j++)
				{
					if (!(customAttributes[j] is DbCollectionAttribute))
					{
						continue;
					}
					object actValue = pi.GetValue(obj, null);
					if (actValue == null)
					{
						continue;
					}
					foreach (object targetObject in (IEnumerable)actValue)
					{
						Delete(targetObject);
					}
				}
			}
			string tableName = GetFullTableName(type);
			PropertyInfo primaryKeyPI = null;
			properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (PropertyInfo fpi in properties)
			{
				customAttributes = Attribute.GetCustomAttributes(fpi);
				for (int j = 0; j < customAttributes.Length; j++)
				{
					if (customAttributes[j] is DbPrimaryKeyAttribute)
					{
						primaryKeyPI = fpi;
						break;
					}
				}
				if (primaryKeyPI != null)
				{
					break;
				}
			}
			string primaryKeyColumn = null;
			customAttributes = Attribute.GetCustomAttributes(primaryKeyPI);
			foreach (Attribute attr2 in customAttributes)
			{
				if (attr2 is DbColumnAttribute)
				{
					primaryKeyColumn = ((DbColumnAttribute)attr2).Name;
					break;
				}
			}
			string sql = "DELETE FROM " + Database.Enquote(tableName) + " WHERE " + Database.Enquote(primaryKeyColumn) + "=" + primaryKeyPI.GetValue(obj, null);
			Database.ExecuteNonQuery(sql);
		}

		protected string GetColumnDefinition(string typeName, DbColumnAttribute dbColumn, PropertyInfo pi)
		{
			string result = Database.Enquote(dbColumn.Name) + " ";
			switch (typeName)
			{
			case "DbMappingKey":
			{
				PropertyInfo pkPi = GetPrimaryKeyProperty(pi.PropertyType);
				return GetColumnDefinition(pkPi.PropertyType.Name, dbColumn, pi);
			}
			case "Enum":
				result += "INT";
				break;
			case "String":
				if (dbColumn.Length == 0)
				{
					dbColumn.Length = 64;
				}
				result = ((!dbColumn.StoreAsVarBinary) ? ((dbColumn.Length > 256) ? ((dbColumn.Length > 65536) ? (result + "LONGTEXT") : (result + "TEXT")) : (result + "VARCHAR(" + dbColumn.Length + ")")) : (result + "VARBINARY(255)"));
				break;
			case "Int16":
				result += "SMALLINT";
				break;
			case "Int32":
				result += "INT";
				break;
			case "UInt32":
				result += "INT UNSIGNED";
				break;
			case "Int64":
				result += "BIGINT";
				break;
			case "UInt64":
				result += "BIGINT UNSIGNED";
				break;
			case "DateTime":
				result += "DATETIME";
				break;
			case "TimeSpan":
				result += "BIGINT";
				break;
			case "Boolean":
				result += "TINYINT(1)";
				break;
			case "Double":
				result += "double";
				break;
			case "Decimal":
				result += "decimal(20,2)";
				break;
			case "Byte[]":
				if (dbColumn.Length == 0)
				{
					dbColumn.Length = 255;
				}
				result = ((dbColumn.Length >= 256) ? ((dbColumn.Length >= 65536) ? (result + "LONGBLOB") : (result + "BLOB")) : (result + "TINYBLOB"));
				break;
			default:
				throw new NotImplementedException("The type '" + ((pi == null) ? typeName : pi.PropertyType.Name) + "' is not implemented for the chosen DatabaseBase implementation.");
			}
			if (!dbColumn.AllowDbNull)
			{
				result += " NOT NULL";
			}
			if (dbColumn.DefaultValue != null)
			{
				result = result + " DEFAULT " + dbColumn.DefaultValue;
			}
			if (dbColumn.AutoIncrement)
			{
				result += " AUTO_INCREMENT";
			}
			return result;
		}

		public void AddColumn(string table, string typeName, DbColumnAttribute dbColumn, PropertyInfo pi, string afterColumn = null, string defaultValue = null, string additionalFlags = null)
		{
			if (!FieldExist(Database, table, dbColumn.Name))
			{
				string sql = string.Format("ALTER TABLE {0} ADD COLUMN {1} {2} {3} {4}", Database.Enquote(table), GetColumnDefinition(typeName, dbColumn, pi), (additionalFlags == null) ? string.Empty : (" " + additionalFlags), (defaultValue == null) ? string.Empty : (" DEFAULT " + defaultValue), (afterColumn == null) ? string.Empty : ((afterColumn == "FIRST") ? "FIRST" : (" AFTER " + Database.Enquote(afterColumn))));
				Database.ExecuteNonQuery(sql);
			}
		}

		private bool FieldExist(DatabaseBase database, string tableName, string fieldName)
		{
			string command = $"SELECT count(*)\r\n                FROM \r\n                 information_schema.COLUMNS\r\n                WHERE\r\n                 TABLE_SCHEMA = '{database.DbConfig.DbName}'\r\n                AND\r\n                 TABLE_NAME = '{tableName}'\r\n                AND\r\n                 COLUMN_NAME = '{fieldName}'";
			return (long)database.ExecuteScalar(command) == 1;
		}
	}
}
