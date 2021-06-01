using System;
using System.ComponentModel;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("tables", ForceInnoDb = true)]
	public class Table : TableObject, ITable, ITableObject, IDataObject, INotifyPropertyChanged, ICloneable
	{
		public string OriginalName { get; set; }

		public new int PreviousId { get; set; }

		public Table()
			: base(TableType.Table)
		{
		}

		public override object Clone()
		{
			TableObject clone = new Table();
			Clone(ref clone);
			(clone as Table).OriginalName = OriginalName;
			return clone;
		}
	}
}
