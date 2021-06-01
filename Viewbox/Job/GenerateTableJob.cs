using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using SystemDb;
using DbAccess;
using gudusoft.gsqlparser;
using Utils;
using Viewbox.Properties;
using ViewboxBusiness.Common;
using ViewboxBusiness.ProfileDb;
using ViewboxBusiness.ProfileDb.Tables;
using ViewboxBusiness.Structures.Config;

namespace Viewbox.Job
{
	public class GenerateTableJob : Base
	{
		private static readonly Dictionary<string, GenerateTableJob> Jobs = new Dictionary<string, GenerateTableJob>();

		private static readonly TSourceTokenList LcTableTokens = new TSourceTokenList(IsOwnsObjects: false);

		private static readonly TSourceTokenList LcFieldTokens = new TSourceTokenList(IsOwnsObjects: false);

		private readonly Dictionary<string, string> _tempTableNames = new Dictionary<string, string>();

		private readonly ScriptTableCollection _tableCollection = new ScriptTableCollection();

		private GenerateTableJob()
		{
			Jobs.Add(base.Key, this);
			Base.AddJob(base.Key, this);
			base.JobFinished += delegate(Base j)
			{
				Janitor.RegisterTempObject(j.Key);
			};
			base.JobCrashed += delegate(Base j, Exception e)
			{
				Janitor.RegisterError(j.Key, e);
			};
			StartJob(delegate(object obj, CancellationToken token)
			{
				DoReadScripts((string)((object[])obj)[0]);
			}, new object[1] { ViewboxSession.SelectedSystem });
			foreach (ILanguage language in ViewboxApplication.Languages)
			{
				CultureInfo culture = new CultureInfo(language.CountryCode);
				base.Descriptions.Add(language.CountryCode, Resources.ResourceManager.GetString("DeleteRelations", culture));
			}
		}

		public static GenerateTableJob Create()
		{
			return new GenerateTableJob();
		}

		private void DoReadScripts(string dataBase)
		{
			using DatabaseBase connection = ViewboxApplication.Database.SystemDb.ConnectionManager.GetConnection();
			ProfileConfig profileConfig = new ProfileConfig
			{
				ProjectDb = new ProjectDb()
			};
			profileConfig.ProjectDb.Init(connection.DbConfig);
			Match match = null;
			DirectoryInfo di = new DirectoryInfo(ViewboxApplication.ViewboxViewsciptFolder);
			FileInfo[] files = di.GetFiles();
			foreach (FileInfo fileInfo in files)
			{
				_tableCollection.Clear();
				List<Viewscript> viewScripts = ViewscriptParser.Parse(fileInfo, profileConfig, forViewComparison: true);
				foreach (Viewscript viewScript in viewScripts)
				{
					BuildTableInformation(viewScript.ViewInfo.CompleteStatement);
					string temp = viewScript.Name;
					if ((match = Regex.Match(viewScript.Name, "\\d{1,2}[\\w\\S]*[A-z]+[\\w\\S]*", RegexOptions.None)) != null && match.Success)
					{
						temp = "$" + match.Value.Trim();
					}
					FillDatabaseWithInformation(viewScript.Name, temp, dataBase);
				}
			}
			FillDatabaseWithConcreteInformation(dataBase);
		}

		private string RemoveTruncate(string sqlScript)
		{
			StringBuilder sb = new StringBuilder(sqlScript);
			Match i;
			while ((i = Regex.Match(sb.ToString(), "\\s*TRUNCATE\\s+TABLE.+?;", RegexOptions.Singleline)) != null && i.Success)
			{
				sb = sb.Replace(i.Value, "", i.Index, i.Length);
			}
			return sb.ToString();
		}

		private string RemoveDelimiters(string sqlScript)
		{
			StringBuilder sb = null;
			Match i = Regex.Match(sqlScript, "^\\s*DELIMITER\\s*(?<sign>\\S+)\\s*", RegexOptions.Multiline);
			string delimiterSign = "";
			if (i.Success)
			{
				sb = new StringBuilder(sqlScript);
				sb = sb.Replace(i.Value, "", i.Index, i.Value.Length);
				delimiterSign = i.Groups["sign"].Value;
			}
			if (sb != null)
			{
				while ((i = Regex.Match(sb.ToString(), string.Join("", delimiterSign.Select((char c) => "\\" + c)))) != null && i.Success)
				{
					sb = sb.Replace(delimiterSign, ";", i.Index, delimiterSign.Length);
				}
				return sb.ToString();
			}
			return sqlScript;
		}

		private string GetScriptWithTempTableNames(string sqlScript)
		{
			StringBuilder sb = new StringBuilder(sqlScript);
			Match match = null;
			while ((match = Regex.Match(sb.ToString(), "\\s\\d{1,2}[\\w\\S]*[A-z]+[\\w\\S]*", RegexOptions.None)) != null && match.Success)
			{
				string tableName = match.Value.Trim();
				string tempTableName = "$" + tableName;
				if (!_tempTableNames.ContainsKey(tempTableName))
				{
					_tempTableNames.Add(tempTableName, tableName);
				}
				sb = sb.Replace(tableName, tempTableName, match.Index, tempTableName.Length);
			}
			return sb.ToString();
		}

		public void GetDbObjectsFromNode(TLz_Node pNode)
		{
			LcTableTokens.Clear();
			LcFieldTokens.Clear();
			TLzGetDbObjectsVisitor av = new TLzGetDbObjectsVisitor
			{
				ParseTree = pNode,
				TableTokens = LcTableTokens,
				FieldTokens = LcFieldTokens
			};
			av.Doit();
		}

		private KeyValuePair<string, ScriptTable> LoadTableForScriptColumn(ScriptTable tab, ScriptDbColumn column)
		{
			try
			{
				string val = column.SourceTableAlias;
				KeyValuePair<string, ScriptTable> loctable;
				if (val != null)
				{
					loctable = tab.ScriptTables.First((KeyValuePair<string, ScriptTable> t) => string.Compare(t.Value.Alias, val, StringComparison.InvariantCultureIgnoreCase) == 0);
				}
				else
				{
					loctable = tab.ScriptTables.FirstOrDefault((KeyValuePair<string, ScriptTable> t) => string.IsNullOrEmpty(t.Value.Alias));
					if (loctable.Key != null && !loctable.Value.ScriptColumns.ContainsKey(column.Name) && tab.ScriptColumns.ContainsKey("*"))
					{
						return new KeyValuePair<string, ScriptTable>(tab.ScriptColumns["*"].RealTableName, tab.ScriptTables[tab.ScriptColumns["*"].RealTableName]);
					}
				}
				if (loctable.Key != null && _tableCollection.ContainsKey(loctable.Value.Name))
				{
					if (loctable.Value.ScriptColumns.ContainsKey(column.SourceTableColumn))
					{
						string tableName = loctable.Value.ScriptColumns[column.SourceTableColumn].RealTableName;
						return new KeyValuePair<string, ScriptTable>(tableName, loctable.Value.ScriptTables[tableName]);
					}
					ScriptDbColumn colClone = column.Clone() as ScriptDbColumn;
					loctable = LoadTableForScriptColumn(_tableCollection[loctable.Value.Name], colClone);
				}
				return loctable;
			}
			catch
			{
				return default(KeyValuePair<string, ScriptTable>);
			}
		}

		private void BuildTableInformation(string completeScriptStatement)
		{
			TGSqlParser mysqlParser = new TGSqlParser(TDbVendor.DbVMysql)
			{
				SqlText = 
				{
					Text = GetScriptWithTempTableNames(RemoveTruncate(RemoveDelimiters(completeScriptStatement)))
				}
			};
			mysqlParser.Parse();
			_log.Info("----- Start script parsing -----");
			for (int i = 0; i < mysqlParser.SqlStatements.Count(); i++)
			{
				TCreateTableSqlStatement tempData = mysqlParser.SqlStatements[i] as TCreateTableSqlStatement;
				if (tempData == null || string.IsNullOrEmpty(tempData.Table.DisplayName))
				{
					continue;
				}
				_log.Info("Create table found: " + tempData.Table.DisplayName);
				ScriptTable table = new ScriptTable
				{
					Name = tempData.Table.DisplayName
				};
				_tableCollection.Add(table);
				if (tempData.SelectStmt == null)
				{
					continue;
				}
				for (int k = 0; k < tempData.SelectStmt.Fields.Count(); k++)
				{
					string columnName = tempData.SelectStmt.Fields[k].FieldAlias;
					if (string.IsNullOrEmpty(columnName))
					{
						columnName = tempData.SelectStmt.Fields[k].FieldName;
					}
					ScriptDbColumn column = new ScriptDbColumn
					{
						Name = columnName
					};
					table.ScriptColumns.Add(column);
					string sourceTableAlias = tempData.SelectStmt.Fields[k].FieldPrefix;
					if (string.IsNullOrEmpty(sourceTableAlias))
					{
						if (tempData.SelectStmt.Fields[k].FieldExpr != null && tempData.SelectStmt.Fields[k].FieldExpr.oper == TLzOpType.Expr_FuncCall)
						{
							TLz_FuncCall func = (TLz_FuncCall)tempData.SelectStmt.Fields[k].FieldExpr.lexpr;
							if (func == null || func.args == null)
							{
								continue;
							}
							for (int l = 0; l < func.args.Count(); l++)
							{
								if (func.args[l] is TSourceToken)
								{
									continue;
								}
								if (func.args[l] is TLz_CastArg)
								{
									TLz_CastArg castArg = (TLz_CastArg)func.args[l];
									string[] dataArray = castArg._ndexpr.AsText.Split('.');
									if (dataArray.Length == 2)
									{
										column.SourceTableAlias = dataArray[0];
										column.SourceTableColumn = dataArray[1];
									}
								}
								else
								{
									LcTableTokens.Clear();
									LcFieldTokens.Clear();
									GetDbObjectsFromNode((TLz_Node)func.args[l]);
									if (LcFieldTokens.Count() == 1 && LcTableTokens.Count() == 1)
									{
										column.SourceTableAlias = LcTableTokens[0].AsText;
										column.SourceTableColumn = LcFieldTokens[0].AsText;
									}
								}
							}
						}
					}
					else
					{
						column.SourceTableAlias = sourceTableAlias;
						column.SourceTableColumn = tempData.SelectStmt.Fields[k].FieldName;
					}
					_log.Info(" select column : " + columnName + ", source table alias is : " + sourceTableAlias + ", source table field name : " + tempData.SelectStmt.Fields[k].FieldName + ", Description : " + tempData.SelectStmt.Fields[k].FieldDesc);
				}
				for (int j = 0; j < tempData.SelectStmt.Tables.Count(); j++)
				{
					if (_tableCollection.ContainsKey(tempData.SelectStmt.Tables[j].Name))
					{
						ScriptTable stable = _tableCollection[tempData.SelectStmt.Tables[j].Name].Clone() as ScriptTable;
						if (stable != null)
						{
							stable.Alias = tempData.SelectStmt.Tables[j].TableAlias;
							table.ScriptTables.Add(stable);
						}
					}
					else
					{
						ScriptTable fromTable = new ScriptTable
						{
							Name = tempData.SelectStmt.Tables[j].Name,
							Alias = tempData.SelectStmt.Tables[j].TableAlias
						};
						table.ScriptTables.Add(fromTable);
					}
				}
			}
			foreach (KeyValuePair<string, ScriptTable> t in _tableCollection)
			{
				foreach (KeyValuePair<string, ScriptDbColumn> scriptColumn in t.Value.ScriptColumns)
				{
					KeyValuePair<string, ScriptTable> table2 = LoadTableForScriptColumn(t.Value, scriptColumn.Value);
					_log.Info(table2);
					if (table2.Value != null)
					{
						scriptColumn.Value.RealTableName = table2.Value.Name;
						scriptColumn.Value.RealColumnName = scriptColumn.Value.SourceTableColumn;
					}
				}
			}
			_log.Info("----- Finish script parsing -----");
			_log.Info(_tableCollection.ToString());
		}

		private void FillDatabaseWithInformation(string tableName, string tempTableName, string dataBase)
		{
			string localtempTableName = tempTableName.ToLower();
			if (!_tableCollection.ContainsKey(localtempTableName))
			{
				localtempTableName = tempTableName.ToUpper();
				if (!_tableCollection.ContainsKey(localtempTableName))
				{
					return;
				}
			}
			CsvReader csvReader = new CsvReader(ViewboxApplication.ViewboxViewsciptFolder + "ExtendedInformations.csv")
			{
				Separator = ';'
			};
			DataTable dataTable = csvReader.GetCsvData(0, Encoding.UTF8);
			foreach (KeyValuePair<string, ScriptDbColumn> column in _tableCollection[localtempTableName].ScriptColumns)
			{
				if (string.IsNullOrEmpty(column.Value.RealTableName) || string.IsNullOrEmpty(column.Value.RealColumnName))
				{
					continue;
				}
				string localTableName = column.Value.RealTableName.ToUpper().TrimStart('_');
				foreach (DataRow dataRow in dataTable.Rows)
				{
					if (string.CompareOrdinal(dataRow["Source table"].ToString().ToUpper(), localTableName) == 0 && string.CompareOrdinal(dataRow["Source column"].ToString().ToUpper(), column.Value.RealColumnName.ToUpper()) == 0)
					{
						ViewboxApplication.Database.SystemDb.SaveExtendedColumnInformation(dataBase, tableName, column.Value.Name.ToUpper(), dataRow["Target table"].ToString().ToUpper(), dataRow["Target column"].ToString().ToUpper(), dataRow["Information"].ToString().ToUpper());
					}
				}
			}
		}

		private void FillDatabaseWithConcreteInformation(string dataBase)
		{
			_log.Info("Save concrete information.");
			CsvReader csvReader = new CsvReader(ViewboxApplication.ViewboxViewsciptFolder + "ConcreteExtendedInformations.csv")
			{
				Separator = ';'
			};
			DataTable dataTable = csvReader.GetCsvData(0, Encoding.UTF8);
			foreach (DataRow dataRow in dataTable.Rows)
			{
				ViewboxApplication.Database.SystemDb.SaveExtendedColumnInformation(dataBase, dataRow["Source table"].ToString().ToUpper(), dataRow["Source column"].ToString(), dataRow["Target table"].ToString().ToUpper(), dataRow["Target column"].ToString().ToUpper(), dataRow["Information"].ToString().ToUpper());
			}
		}
	}
}
