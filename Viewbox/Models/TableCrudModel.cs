using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using SystemDb;
using AV.Log;
using DbAccess;
using log4net;

namespace Viewbox.Models
{
	public class TableCrudModel
	{
		private readonly ITableCrud _crud;

		private readonly ILog _logger = LogHelper.GetLogger();

		private int _rowno;

		public string Title { get; set; }

		public string SubmitButton { get; set; }

		public int Id { get; set; }

		public int Schema { get; set; }

		public int TableId { get; set; }

		public int Rowno
		{
			get
			{
				return _rowno;
			}
			set
			{
				_rowno = value;
			}
		}

		public List<TableCrudColumnModel> Columns { get; set; }

		public TableCrudModel(ITableCrud tableCrud, int rowno = -1)
		{
			_crud = tableCrud;
			Id = _crud.Id;
			TableId = _crud.Table.Id;
			Title = _crud.Title;
			Schema = _crud.DefaultScheme;
			_rowno = rowno;
			Columns = new List<TableCrudColumnModel>();
			DatabaseBase conn = ViewboxApplication.Database.IndexDatabase.ConnectionManager.GetConnection();
			try
			{
				using DatabaseBase conn2 = ViewboxApplication.Database.ConnectionManager.GetConnection();
				if (rowno > -1)
				{
					StringBuilder builder = new StringBuilder(" SELECT ");
					builder.Append(string.Join(",", _crud.Columns.Select((ITableCrudColumn w) => conn.Enquote(w.Column.Name))));
					builder.Append(" FROM ");
					builder.Append(conn.Enquote(_crud.Table.Database, _crud.Table.TableName));
					builder.Append(" WHERE _ROW_NO_ = ").Append(rowno);
					using IDataReader reader = conn2.ExecuteReader(builder.ToString());
					if (reader.Read())
					{
						int ord = 0;
						foreach (ITableCrudColumn col in _crud.Columns)
						{
							object value = (reader.IsDBNull(ord) ? null : reader.GetValue(ord));
							Columns.Add(new TableCrudColumnModel(col, value, conn));
							ord++;
						}
					}
				}
				else
				{
					Columns.AddRange(_crud.Columns.Select((ITableCrudColumn w) => new TableCrudColumnModel(w, null, conn)));
				}
			}
			finally
			{
				if (conn != null)
				{
					((IDisposable)conn).Dispose();
				}
			}
			CalculateFields();
		}

		public TableCrudModel()
		{
		}

		public void CalculateFields()
		{
			try
			{
				DatabaseBase conn = ViewboxApplication.Database.ConnectionManager.GetConnection();
				try
				{
					string where = string.Join(" AND ", from w in Columns
						where w.CalculateType == 2
						select conn.Enquote(w.Name) + " = " + conn.GetValueString(w.Value));
					foreach (TableCrudColumnModel col in Columns.Where((TableCrudColumnModel w) => w.CalculateType == 1))
					{
						string select = "SELECT SUM(" + conn.Enquote(col.FromColumnName) + ") FROM " + conn.Enquote(col.FromColumnTableDatabase, col.FromColumnTableTableName) + " WHERE " + where;
						object value1 = conn.ExecuteScalar(select);
						select = "SELECT SUM(" + conn.Enquote(col.FromColumnName) + ") FROM " + conn.Enquote(col.ColumnTableDatabase, col.ColumnTableTableName) + " WHERE " + where + ((_rowno > -1) ? (" AND _row_no_ <> " + _rowno) : "");
						object value2 = conn.ExecuteScalar(select);
						double doubleVal1 = 0.0;
						double doubleVal2 = 0.0;
						if (value1 != null)
						{
							double.TryParse(value1.ToString(), out doubleVal1);
						}
						if (value2 != null)
						{
							double.TryParse(value2.ToString(), out doubleVal2);
						}
						col.Value = doubleVal1 - doubleVal2;
					}
				}
				finally
				{
					if (conn != null)
					{
						((IDisposable)conn).Dispose();
					}
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex);
			}
		}
	}
}
