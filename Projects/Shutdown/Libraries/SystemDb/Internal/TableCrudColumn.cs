using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("table_crud_columns")]
	public class TableCrudColumn : ITableCrudColumn
	{
		private IColumn _column;

		private IColumn _fromColumn;

		private ITableCrud _tableCrud;

		[DbColumn("table_crud_id")]
		public int TableCrudId { get; private set; }

		[DbColumn("column_id")]
		public int ColumnId { get; private set; }

		[DbColumn("from_column_id")]
		public int FromColumnId { get; private set; }

		[DbColumn("id", AutoIncrement = true)]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("calculatetype")]
		public int CalculateType { get; set; }

		public ITableCrud TableCrud
		{
			get
			{
				return _tableCrud;
			}
			set
			{
				if (_tableCrud != value)
				{
					_tableCrud = value;
					TableCrudId = _tableCrud.Id;
				}
			}
		}

		public IColumn Column
		{
			get
			{
				return _column;
			}
			set
			{
				if (_column != value)
				{
					_column = value;
					ColumnId = _column.Id;
				}
			}
		}

		public IColumn FromColumn
		{
			get
			{
				return _fromColumn;
			}
			set
			{
				if (_fromColumn != value)
				{
					_fromColumn = value;
					FromColumnId = _fromColumn.Id;
				}
			}
		}
	}
}
