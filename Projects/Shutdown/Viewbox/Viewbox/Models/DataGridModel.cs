using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SystemDb;
using SystemDb.Internal;
using DbAccess;
using Viewbox.Properties;
using ViewboxDb;
using ViewboxDb.Filters;

namespace Viewbox.Models
{
	public class DataGridModel : ViewboxModel
	{
		private List<Tuple<IFilter, IColumn, string, object, string>> _filterParameters = new List<Tuple<IFilter, IColumn, string, object, string>>();

		public DataTable DataTable { get; internal set; }

		public ViewboxDb.TableObject TableObject { get; internal set; }

		public ITableObject TableInfo { get; internal set; }

		public IScheme SelectedScheme { get; internal set; }

		public SapBalanceModel SapBalance { get; internal set; }

		public SimpleTableModel SimpleTable { get; internal set; }

		public StructuedTableModel StructuedTable { get; internal set; }

		public UniversalTableModel UniversalTable { get; internal set; }

		public long RowsCount { get; internal set; }

		public int RowsPerPage { get; internal set; }

		public long FromRow { get; internal set; }

		public long ToRow => Math.Min(RowsCount, FromRow + RowsPerPage - 1);

		public long PageCount => RowsCount / RowsPerPage + ((RowsCount % RowsPerPage > 0) ? 1 : 0);

		public long Page => ToRow / RowsPerPage + 1;

		public string[] ColumnWidths { get; internal set; }

		public override string LabelCaption => TableInfo.GetDescription();

		public string LabelNumberOfRows => Resources.NumberOfRows;

		public string LabelNoData => Resources.NoData;

		public string Search { get; internal set; }

		public bool IsThereEmptyMessage { get; set; }

		public SortCollection Sort { get; set; }

		public IFilter Filter { get; set; }

		public List<IColumn> GroupColumn { get; set; }

		public List<IColumn> SubtotalColumns { get; set; }

		public bool SubtotalOnlyExpectedResult { get; set; }

		public List<int> subtotalRows { get; set; }

		public int fulltotalRow { get; set; }

		public bool Summa { get; set; }

		public IDictionary<int, List<Tuple<int, List<string>>>> RoleBasedFilters
		{
			get
			{
				if (TableObject == null)
				{
					return TableInfo.RoleBasedFilters;
				}
				return null;
			}
			set
			{
				TableInfo.RoleBasedFilters = value;
			}
		}

		public IDictionary<int, List<Tuple<string, string>>> RoleBasedFiltersNew
		{
			get
			{
				if (TableObject == null)
				{
					return TableInfo.RoleBasedFiltersNew;
				}
				return null;
			}
			set
			{
				TableInfo.RoleBasedFiltersNew = value;
			}
		}

		public SortCollection SortAlreadyAddedColumns { get; set; }

		public List<string> AlreadyAddedColumns { get; set; }

		public string TableDescription { get; set; }

		public ITableCrud Crud { get; set; }

		public List<Tuple<IFilter, IColumn, string, object, string>> FilterParameters
		{
			get
			{
				return _filterParameters;
			}
			set
			{
				_filterParameters = value;
			}
		}

		public DialogModel GetWaitDialog(string action)
		{
			return new DialogModel
			{
				Title = Resources.PleaseWait,
				Content = string.Format(Resources.ExecutingAction, action),
				DialogType = DialogModel.Type.Info
			};
		}

		public IList<IColumn> GetDecimalColumns()
		{
			return TableInfo.Columns.Where((IColumn c) => c.DataType == SqlType.Decimal).ToList();
		}

		public string DataGridNameDescription()
		{
			if (TableInfo.GetType().Name == "Issue" || (TableObject != null && (TableObject.Additional is SapBalanceModel || TableObject.Additional is DcwBalanceModel || TableObject.Additional is SimpleTableModel || TableObject.Additional is StructuedTableModel)))
			{
				return Resources.Issue;
			}
			if (TableInfo.GetType().Name == "View")
			{
				return Resources.View;
			}
			if (TableInfo.GetType().Name == "Table")
			{
				return Resources.Table;
			}
			return Resources.Name;
		}

		public decimal Sum(IColumn column)
		{
			if (column.DataType == SqlType.Decimal)
			{
				return DataTable.AsEnumerable().Sum((DataRow row) => (decimal)row[column.Name]);
			}
			throw new ArgumentException($"Type of column {column.Name} must be decimal instead of {column.DataType}");
		}

		public static string GetClickAndClickValue(int rowNo, int sourceColumnId)
		{
			string value = string.Empty;
			IColumn column = ViewboxApplication.Database.SystemDb.Columns.SingleOrDefault((IColumn c) => c.Id == sourceColumnId) ?? ViewboxSession.Columns[sourceColumnId];
			if (column != null)
			{
				using DatabaseBase conn = ViewboxApplication.Database.ConnectionManager.GetConnection();
				string query = "";
				query = ((!(column.Table is Issue) || (column.Table as Issue).IssueType != IssueType.StoredProcedure) ? $"SELECT {conn.Enquote(column.Name)} FROM {conn.Enquote(column.Table.Database, column.Table.TableName)} WHERE `_row_no_` = {rowNo} LIMIT 1;" : $"SELECT {conn.Enquote(column.Name)} FROM {conn.Enquote(ViewboxSession.User.UserTable(column.Table.Database), column.Table.TableName)} WHERE `_row_no_` = {rowNo} LIMIT 1;");
				using IDataReader reader = conn.ExecuteReader(query);
				while (reader.Read())
				{
					if (!reader.IsDBNull(0))
					{
						return reader.GetString(0);
					}
				}
			}
			return string.Empty;
		}
	}
}
