using System.Collections.Generic;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("table_cruds")]
	public class TableCrud : ITableCrud
	{
		private ITableObject _onTable;

		private ITableObject _table;

		[DbColumn("table_id")]
		public int TableId { get; private set; }

		[DbColumn("on_table_id")]
		public int OnTableId { get; private set; }

		[DbColumn("id", AutoIncrement = true)]
		[DbPrimaryKey]
		public int Id { get; set; }

		public ITableObject Table
		{
			get
			{
				return _table;
			}
			set
			{
				if (_table != value)
				{
					_table = value;
					TableId = _table.Id;
				}
			}
		}

		public ITableObject OnTable
		{
			get
			{
				return _onTable;
			}
			set
			{
				if (_onTable != value)
				{
					_onTable = value;
					OnTableId = _onTable.Id;
				}
			}
		}

		[DbColumn("title", Length = 1500)]
		public string Title { get; set; }

		[DbColumn("default_scheme")]
		public int DefaultScheme { get; set; }

		public List<ITableCrudColumn> Columns { get; set; }

		public TableCrud()
		{
			Columns = new List<ITableCrudColumn>();
		}
	}
}
