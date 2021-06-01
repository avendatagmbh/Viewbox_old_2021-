using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemDb;
using AV.Log;
using DbAccess;
using log4net;

namespace Viewbox.Models.Wertehilfe
{
	public class Indexer : IIndexer
	{
		private IParameter _parameter;

		private IColumn _column;

		private string _dbName = ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName;

		private string _indexDbName = ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName + ViewboxApplication.IndexDatabasePostFix;

		private IndexingState _state = IndexingState.Init;

		private DatabaseBase _connection = ViewboxApplication.Database.SystemDb.ConnectionManager.GetConnection();

		private ILog _logger = LogHelper.GetLogger();

		public IParameter Parameter => _parameter;

		public IColumn Column => _column;

		public string IndexDbName => _indexDbName;

		public IndexingState State => _state;

		public Indexer(IParameter parameter)
		{
			_parameter = parameter;
			int columnId = GetColumnId();
			_column = ViewboxApplication.Database.SystemDb.Columns.FirstOrDefault((IColumn c) => c.Id == columnId);
		}

		public void DoIndexing()
		{
			Task.Factory.StartNew(delegate
			{
				try
				{
					string sql = $"CALL `{_indexDbName}`.GenerateColumnInfo({_column.Id});";
					_connection.ExecuteNonQuery(sql);
				}
				catch (Exception ex)
				{
					_logger.Error($"Error while indexing: {ex.Message}");
				}
				finally
				{
					_connection.Close();
				}
			});
		}

		public void CancelIndexing()
		{
			try
			{
				if (_connection.IsOpen)
				{
					_connection.Close();
				}
			}
			catch (Exception ex)
			{
				_logger.Error($"Error while canceling indexing: {ex.Message}");
			}
		}

		protected int GetColumnId()
		{
			StringBuilder selectSql = new StringBuilder();
			selectSql.Append($"SELECT DISTINCT c.id FROM {_dbName}.parameter p ");
			selectSql.Append($"JOIN {_dbName}.issue_extensions i ON i.ref_id = p.issue_id ");
			selectSql.Append($"JOIN {_dbName}.columns c ON c.table_id = i.obj_id AND c.name = p.column_name ");
			selectSql.Append($"WHERE p.id ={_parameter.Id}");
			return int.Parse(_connection.ExecuteScalar(selectSql.ToString()).ToString());
		}

		public bool CheckIfExists()
		{
			string query = $"SELECT * FROM {_indexDbName}.VALUE_{_column.Id} LIMIT 1;";
			try
			{
				_connection.ExecuteNonQuery(query);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
