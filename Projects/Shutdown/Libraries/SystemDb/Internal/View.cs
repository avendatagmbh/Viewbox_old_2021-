using System;
using System.ComponentModel;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("tables", ForceInnoDb = true)]
	public class View : TableObject, IView, ITableObject, IDataObject, INotifyPropertyChanged, ICloneable
	{
		public new int PreviousId { get; set; }

		public View()
			: base(TableType.View)
		{
		}

		public override object Clone()
		{
			TableObject clone = new View();
			Clone(ref clone);
			return clone;
		}
	}
}
