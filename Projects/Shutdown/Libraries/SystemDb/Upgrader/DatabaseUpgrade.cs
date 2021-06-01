using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using SystemDb.Internal;
using DbAccess;
using DbAccess.Attributes;

namespace SystemDb.Upgrader
{
	internal class DatabaseUpgrade
	{
		private readonly Info _info;

		public DatabaseUpgrade(DatabaseBase conn)
		{
			_info = new Info(conn);
		}

		internal DatabaseOutOfDateInformation HasUpgrade()
		{
			if (!_info.TableExists() || _info.DbVersion == null)
			{
				return new DatabaseOutOfDateInformation("1.0");
			}
			if (!VersionInfo.NewerDbVersionExists(_info.DbVersion))
			{
				return null;
			}
			return new DatabaseOutOfDateInformation(_info.DbVersion);
		}

		internal void UpgradeDatabase(DatabaseBase conn)
		{
			if (!_info.TableExists() || Info.GetValue("Upgrade_1_1", conn) != null)
			{
				if (!conn.TableExists("users"))
				{
					conn.DbMapping.CreateTableIfNotExists<Info>();
					_info.DbVersion = VersionInfo.CurrentDbVersion;
					return;
				}
				if (conn.TableExists("order_area") && !conn.GetColumnNames("order_area").Contains("index_value"))
				{
					throw new Exception("The database version is too old to be updated by this version of the viewbox admin. Please use the viewbox admin tag.");
				}
				UpgradeTo_1_1(conn);
			}
			while (VersionInfo.NewerDbVersionExists(_info.DbVersion))
			{
				switch (_info.DbVersion)
				{
				case "1.1":
					UpgradeTo_1_2(conn);
					break;
				case "1.2":
					UpgradeTo_1_3(conn);
					break;
				case "1.3":
					UpgradeTo_1_4(conn);
					break;
				case "1.4":
					UpgradeTo_1_5(conn);
					break;
				case "1.5":
					UpgradeTo_1_6(conn);
					break;
				case "1.6":
					UpgradeTo_1_7(conn);
					break;
				case "1.7":
					UpgradeTo_1_8(conn);
					break;
				case "1.8":
					UpgradeTo_1_9(conn);
					break;
				case "1.9":
					UpgradeTo_1_9_1(conn);
					break;
				case "1.9.1":
					UpgradeTo_1_9_2(conn);
					break;
				case "1.9.2":
					UpgardeTo_1_9_3(conn);
					break;
				case "1.9.3":
					UpgardeTo_1_9_4(conn);
					break;
				case "1.9.4":
					UpgardeTo_1_9_5(conn);
					break;
				case "1.9.5":
					UpgardeTo_1_9_6(conn);
					break;
				case "1.9.6":
					UpgardeTo_1_9_7(conn);
					break;
				case "1.9.7":
					UpgardeTo_1_9_8(conn);
					break;
				case "1.9.8":
					UpgardeTo_1_9_9(conn);
					break;
				case "1.9.9":
					UpgardeTo_2_0_0(conn);
					break;
				case "2.0.0":
					UpgardeTo_2_0_1(conn);
					break;
				case "2.0.1":
					UpgardeTo_2_0_2(conn);
					break;
				case "2.0.2":
					UpgardeTo_2_0_3(conn);
					break;
				case "2.0.3":
					UpgardeTo_2_0_4(conn);
					break;
				case "2.0.4":
					UpgardeTo_2_0_5(conn);
					break;
				case "2.0.5":
					UpgardeTo_2_0_6(conn);
					break;
				case "2.0.6":
					UpgardeTo_2_0_7(conn);
					break;
				}
			}
		}

		private void UpgradeTo_1_1(DatabaseBase conn)
		{
			conn.DbMapping.CreateTableIfNotExists<Info>();
			string progressStr = Info.GetValue("Upgrade_1_1", conn);
			int curProgress = ((progressStr != null) ? int.Parse(progressStr) : 0);
			if (progressStr == null)
			{
				Info.SetValue("Upgrade_1_1", "0", conn);
			}
			List<string> userColumns = conn.GetColumnNames("users");
			if (userColumns.Contains("user_id") && !userColumns.Contains("id"))
			{
				conn.RenameColumn("users", "user_id", "id");
				conn.ExecuteNonQuery("ALTER TABLE users MODIFY COLUMN id INT");
			}
			if (!userColumns.Contains("user_name") && userColumns.Contains("email"))
			{
				conn.RenameColumn("users", "email", "user_name");
				conn.DbMapping.AddColumn("users", "String", new DbColumnAttribute("email")
				{
					Length = 128
				}, null, "name");
				conn.ExecuteNonQuery("UPDATE `users` as u SET u.email = `user_name`");
			}
			List<string> columnNames = conn.GetColumnNames("tables");
			if (!columnNames.Contains("visible"))
			{
				conn.DbMapping.AddColumn("tables", "Boolean", new DbColumnAttribute("visible"), null, "ordinal", conn.GetSqlString("1"));
			}
			if (!columnNames.Contains("transaction_nr"))
			{
				conn.DbMapping.AddColumn("tables", "Int32", new DbColumnAttribute("transaction_nr"), null, "default_scheme", conn.GetSqlString("1"));
				conn.ExecuteNonQuery("UPDATE tables SET transaction_nr = ordinal + 1001 WHERE type = 3;");
				conn.ExecuteNonQuery("UPDATE tables SET transaction_nr = ordinal + 2001 WHERE type = 2;");
				conn.ExecuteNonQuery("UPDATE tables SET transaction_nr = ordinal + 10001 WHERE type = 1;");
			}
			List<string> tables = conn.GetTableList();
			foreach (KeyValuePair<string, string> pair in new Dictionary<string, string>
			{
				{ "category_role", "category_roles" },
				{ "category_user", "category_users" },
				{ "column_role", "column_roles" },
				{ "column_user", "column_users" },
				{ "optimization", "optimizations" },
				{ "optimization_role", "optimization_roles" },
				{ "optimization_user", "optimization_users" },
				{ "order_area", "order_areas" },
				{ "table_role", "table_roles" },
				{ "table_user", "table_users" }
			})
			{
				if (tables.Contains(pair.Key) && !tables.Contains(pair.Value))
				{
					conn.RenameTable(pair.Key, pair.Value);
				}
			}
			List<string> columnsColumnNames = conn.GetColumnNames("columns");
			if (!columnsColumnNames.Contains("optimization_type") && columnsColumnNames.Contains("optimization_id"))
			{
				conn.RenameColumn("columns", "optimization_id", "optimization_type");
			}
			if (!columnsColumnNames.Contains("const_value"))
			{
				conn.DbMapping.AddColumn("columns", "String", new DbColumnAttribute("const_value")
				{
					Length = 50
				}, null, "user_defined");
				curProgress = 1;
			}
			if (curProgress == 1)
			{
				try
				{
					List<string> sqlStatements = new List<string>();
					using (IDataReader reader = conn.ExecuteReader("SELECT value FROM optimizations WHERE parent_id=0"))
					{
						while (reader.Read())
						{
							string database = reader.GetString(0) + "_system";
							string sql2 = "UPDATE columns as cols" + Environment.NewLine + "JOIN tables as tab ON tab.id = cols.table_id" + Environment.NewLine + $"JOIN {database}.table as t ON t.name COLLATE latin1_general_ci = tab.name COLLATE latin1_general_ci" + Environment.NewLine + $"JOIN {database}.col as c ON c.table_id = t.table_id AND c.name = cols.name" + Environment.NewLine + "SET cols.const_value = c.const_value " + Environment.NewLine + "WHERE c.is_empty = 1 AND c.const_value IS NOT NULL";
							sqlStatements.Add(sql2);
						}
					}
					foreach (string sql in sqlStatements)
					{
						conn.ExecuteNonQuery(sql);
					}
				}
				catch (Exception)
				{
					Info.SetValue("Upgrade_1_1", curProgress.ToString(), conn);
					throw;
				}
				Info.SetValue("Upgrade_1_1", 2.ToString(), conn);
			}
			if (!conn.GetColumnNames("languages").Contains("id"))
			{
				conn.ExecuteNonQuery("ALTER TABLE languages DROP PRIMARY KEY");
				conn.DbMapping.AddColumn("languages", "Int32", new DbColumnAttribute("id"), null, "FIRST", null, "PRIMARY KEY AUTO_INCREMENT");
				conn.ExecuteNonQuery("ALTER TABLE languages MODIFY COLUMN id INT");
			}
			_info.DbVersion = "1.1";
			Info.RemoveValue("Upgrade_1_1", conn);
		}

		private void UpgradeTo_1_2(DatabaseBase conn)
		{
			string issueExtensionTableName = conn.DbMapping.GetTableName<IssueExtension>();
			if (conn.TableExists(issueExtensionTableName) && !conn.GetColumnNames(issueExtensionTableName).Contains("checked"))
			{
				conn.DbMapping.AddColumn(issueExtensionTableName, "Boolean", new DbColumnAttribute("checked"), null, "flag");
			}
			_info.DbVersion = "1.2";
		}

		private void UpgradeTo_1_3(DatabaseBase conn)
		{
			string userTableName = conn.DbMapping.GetTableName<User>();
			if (conn.TableExists(userTableName))
			{
				if (!conn.GetColumnNames(userTableName).Contains("is_ad_user"))
				{
					conn.DbMapping.AddColumn(userTableName, "Boolean", new DbColumnAttribute("is_ad_user"), null, "flags", "0");
				}
				if (!conn.GetColumnNames(userTableName).Contains("domain"))
				{
					conn.DbMapping.AddColumn(userTableName, "String", new DbColumnAttribute("domain"), null, "is_ad_user");
				}
			}
			_info.DbVersion = "1.3";
		}

		private void UpgradeTo_1_4(DatabaseBase conn)
		{
			conn.DbMapping.AddColumn("tables", "Boolean", new DbColumnAttribute("archived"), null, "ordinal", "0");
			conn.DbMapping.CreateTableIfNotExists<TableArchiveInformation>();
			_info.DbVersion = "1.4";
		}

		private void UpgradeTo_1_5(DatabaseBase conn)
		{
			conn.DbMapping.AddColumn("users", "Int32", new DbColumnAttribute("display_row_count"), null, "is_ad_user");
			_info.DbVersion = "1.5";
		}

		private void UpgradeTo_1_6(DatabaseBase conn)
		{
			conn.DbMapping.CreateTableIfNotExists<HistoryParameterValue>();
			conn.DbMapping.CreateTableIfNotExists<EmptyDistinctColumn>();
			Property property = new Property
			{
				Key = "empty_distinct_columns",
				Value = "false",
				Type = PropertyType.Bool
			};
			conn.DbMapping.Save(property);
			PropertyText text = null;
			text = new PropertyText
			{
				CountryCode = "en",
				Name = "Empty distinct columns",
				RefId = property.Id,
				Text = "Do you want to hide the empty or distinct columns?"
			};
			conn.DbMapping.Save(text);
			text = new PropertyText
			{
				CountryCode = "de",
				Name = "Leere verschiedenen Spalten",
				RefId = property.Id,
				Text = "Wollen Sie die leeren oder verschiedene Spalten auszublenden?"
			};
			conn.DbMapping.Save(text);
			_info.DbVersion = "1.6";
		}

		private void UpgradeTo_1_7(DatabaseBase conn)
		{
			conn.DbMapping.AddColumn("parameter", "String", new DbColumnAttribute("column_name"), null, "ordinal");
			_info.DbVersion = "1.7";
		}

		private void UpgradeTo_1_8(DatabaseBase conn)
		{
			conn.DbMapping.CreateTableIfNotExists<Index>();
			conn.DbMapping.CreateTableIfNotExists<IndexColumnMapping>();
			_info.DbVersion = "1.8";
		}

		private void UpgradeTo_1_9(DatabaseBase conn)
		{
			conn.DbMapping.AddColumn("parameter", "String", new DbColumnAttribute("table_name"), null, "ordinal");
			conn.DbMapping.AddColumn("parameter", "String", new DbColumnAttribute("database_name"), null, "ordinal");
			List<Language> languages = conn.DbMapping.Load<Language>();
			foreach (KeyValuePair<string, Tuple<string, string>> languageDescription in Language.LanguageDescriptions)
			{
				if (!languages.Any((Language l) => string.Compare(l.CountryCode, languageDescription.Key, StringComparison.InvariantCultureIgnoreCase) == 0))
				{
					languages.Add(new Language
					{
						CountryCode = languageDescription.Key,
						LanguageName = languageDescription.Value.Item1,
						LanguageMotto = languageDescription.Value.Item2
					});
				}
			}
			foreach (Language language in languages)
			{
				conn.DbMapping.Save(language);
			}
			_info.DbVersion = "1.9";
		}

		private void UpgradeTo_1_9_1(DatabaseBase conn)
		{
			conn.DbMapping.AddColumn("tables", "Int32", new DbColumnAttribute("object_type"), null, "archived");
			_info.DbVersion = "1.9.1";
		}

		private void UpgradeTo_1_9_2(DatabaseBase conn)
		{
			conn.ExecuteNonQuery("ALTER TABLE `tables` MODIFY `name` VARCHAR(128);");
			_info.DbVersion = "1.9.2";
		}

		private void UpgardeTo_1_9_3(DatabaseBase conn)
		{
			conn.DbMapping.AddColumn("users", "Int32", new DbColumnAttribute("can_export"), null, "is_ad_user", "1");
			conn.DbMapping.AddColumn("roles", "Int32", new DbColumnAttribute("can_export"), null, "flags", "1");
			_info.DbVersion = "1.9.3";
		}

		private void UpgardeTo_1_9_4(DatabaseBase conn)
		{
			conn.DbMapping.AddColumn("users", typeof(DateTime).Name, new DbColumnAttribute("password_creation_date"), null, "domain");
			conn.DbMapping.AddColumn("users", "Int32", new DbColumnAttribute("password_trials"), null, "domain", "0");
			_info.DbVersion = "1.9.4";
		}

		private void UpgardeTo_1_9_5(DatabaseBase conn)
		{
			conn.DbMapping.AddColumn("issue_extensions", "String", new DbColumnAttribute("row_no_filter")
			{
				Length = 65537
			}, null);
			_info.DbVersion = "1.9.5";
		}

		private void UpgardeTo_1_9_6(DatabaseBase conn)
		{
			conn.DbMapping.AddColumn("relations", "Int32", new DbColumnAttribute("operator"), null, "child");
			_info.DbVersion = "1.9.6";
		}

		private void UpgardeTo_1_9_7(DatabaseBase conn)
		{
			if (conn.TableExists("extended_column_information"))
			{
				conn.DbMapping.AddColumn("extended_column_information", "Int32", new DbColumnAttribute("information2"), null, "information");
				conn.DbMapping.AddColumn("extended_column_information", "String", new DbColumnAttribute("reltype"), null, "information2");
			}
			_info.DbVersion = "1.9.7";
		}

		private void UpgardeTo_1_9_8(DatabaseBase conn)
		{
			conn.DbMapping.AddColumn("relations", "Int16", new DbColumnAttribute("full_line"), null, "operator");
			_info.DbVersion = "1.9.8";
		}

		private void UpgardeTo_1_9_9(DatabaseBase conn)
		{
			conn.DbMapping.AddColumn("relations", "Int32", new DbColumnAttribute("type"), null, "full_line");
			conn.DbMapping.AddColumn("relations", "String", new DbColumnAttribute("ext_info"), null, "type");
			_info.DbVersion = "1.9.9";
		}

		private void UpgardeTo_2_0_0(DatabaseBase conn)
		{
			conn.DbMapping.AddColumn("relations", "String", new DbColumnAttribute("column_ext_info"), null, "type");
			_info.DbVersion = "2.0.0";
		}

		private void UpgardeTo_2_0_1(DatabaseBase conn)
		{
			if (conn.TableExists("archive_documents"))
			{
				conn.DbMapping.AddColumn("archive_documents", "String", new DbColumnAttribute("belegart"), null, "content_type");
			}
			_info.DbVersion = "2.0.1";
		}

		private void UpgardeTo_2_0_2(DatabaseBase conn)
		{
			conn.DbMapping.AddColumn("parameter", "Int32", new DbColumnAttribute("type_modifier"), null, "data_type");
			_info.DbVersion = "2.0.2";
		}

		private void UpgardeTo_2_0_3(DatabaseBase conn)
		{
			conn.DbMapping.AddColumn("columns", "String", new DbColumnAttribute("from_column")
			{
				Length = 255
			}, null, "ordinal");
			conn.DbMapping.AddColumn("columns", "String", new DbColumnAttribute("from_column_format")
			{
				Length = 255
			}, null, "from_column");
			conn.DbMapping.AddColumn("columns", "Int16", new DbColumnAttribute("flag"), null, "from_column_format");
			_info.DbVersion = "2.0.3";
		}

		private void UpgardeTo_2_0_4(DatabaseBase conn)
		{
			try
			{
				conn.DbMapping.CreateTableIfNotExists<TableCrud>();
				conn.DbMapping.CreateTableIfNotExists<TableCrudColumn>();
				conn.DbMapping.AddColumn("table_crud_columns", "Int32", new DbColumnAttribute("from_column_id"), null, "column_id");
				conn.DbMapping.AddColumn("table_cruds", "Int32", new DbColumnAttribute("default_scheme"), null, "title");
			}
			catch
			{
			}
			_info.DbVersion = "2.0.4";
		}

		private void UpgardeTo_2_0_5(DatabaseBase conn)
		{
			conn.DbMapping.AddColumn("columns", "Int32", new DbColumnAttribute("param_operator"), null, "from_column_format");
			_info.DbVersion = "2.0.5";
		}

		private void UpgardeTo_2_0_6(DatabaseBase conn)
		{
			try
			{
				conn.DbMapping.AddColumn("table_crud_columns", "Int32", new DbColumnAttribute("calculatetype"), null, "from_column_id");
			}
			catch (Exception)
			{
			}
			_info.DbVersion = "2.0.6";
		}

		private void UpgardeTo_2_0_7(DatabaseBase conn)
		{
			try
			{
				conn.DbMapping.AddColumn("issue_extensions", "Boolean", new DbColumnAttribute("need_gjahr"), null, "row_no_filter");
				conn.DbMapping.AddColumn("relations", "Boolean", new DbColumnAttribute("user_defined")
				{
					AllowDbNull = false
				}, null, null, conn.GetSqlString("0"));
			}
			catch (Exception)
			{
			}
			_info.DbVersion = "2.0.7";
		}

		internal static string TableSqlCreationCode(DatabaseBase conn)
		{
			Type[] types = Assembly.GetExecutingAssembly().GetTypes();
			string result = "List<string> tableCreation = new List<string> {";
			Type[] array = types;
			foreach (Type type in array)
			{
				Attribute[] customAttributes = Attribute.GetCustomAttributes(type, inherit: true);
				for (int j = 0; j < customAttributes.Length; j++)
				{
					if (customAttributes[j] is DbTableAttribute)
					{
						result = result + "\"" + conn.DbMapping.GetCreateTableSqlStatement(type) + "\"," + Environment.NewLine;
					}
				}
			}
			result.Remove(result.Length - 1);
			return result + "};";
		}

		internal static string SqlCreateIndices(DatabaseBase conn)
		{
			Type[] types = Assembly.GetExecutingAssembly().GetTypes();
			string result = "#region indices" + Environment.NewLine + "IndexInformation ";
			Type[] array = types;
			foreach (Type type in array)
			{
				Attribute[] customAttributes = Attribute.GetCustomAttributes(type, inherit: true);
				for (int j = 0; j < customAttributes.Length; j++)
				{
					if (customAttributes[j] is DbTableAttribute)
					{
						string index = conn.DbMapping.GetIndicesValuePair(type);
						if (index != "")
						{
							result = result + index + Environment.NewLine;
						}
					}
				}
			}
			return result + "#endregion" + Environment.NewLine;
		}
	}
}
