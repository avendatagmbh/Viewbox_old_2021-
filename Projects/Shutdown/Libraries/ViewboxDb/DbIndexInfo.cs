using System.Collections.Generic;
using System.Threading;
using SystemDb;
using DbAccess;

namespace ViewboxDb
{
	public class DbIndexInfo
	{
		public string Database { get; private set; }

		public List<string> ColNames { get; private set; }

		public string Name { get; private set; }

		public bool Exists { get; private set; }

		public string SQLCreateIndex { get; private set; }

		public string SQLDropIndex { get; private set; }

		public DbIndexInfo(ConnectionManager cmanager, CancellationToken token, ITableObject table, IEnumerable<int> columnIds)
		{
			using DatabaseBase connection = cmanager.GetConnection();
			ColNames = new List<string>();
			Database = (table.UserDefined ? connection.DbConfig.DbName : table.Database);
			foreach (int col in columnIds)
			{
				token.ThrowIfCancellationRequested();
				ColNames.Add(table.Columns[col].Name);
			}
			ColNames.Sort();
			Name = ("_" + table.TableName + "_" + string.Join("_", ColNames)).ToLower();
			try
			{
				foreach (string indexName in connection.GetIndexNames(Database, table.TableName))
				{
					if (indexName.ToLower() == Name)
					{
						Exists = true;
						break;
					}
				}
			}
			catch
			{
			}
			SQLCreateIndex = "CREATE INDEX " + Name + " ON " + connection.Enquote(Database, table.TableName) + "(`" + string.Join("`,`", ColNames) + "`)";
			SQLDropIndex = "DROP INDEX " + Name + " ON " + connection.Enquote(Database, table.TableName);
		}
	}
}
