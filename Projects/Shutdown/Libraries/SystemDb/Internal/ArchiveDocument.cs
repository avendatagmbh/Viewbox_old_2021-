using System;
using System.ComponentModel;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("tables", ForceInnoDb = true)]
	public class ArchiveDocument : TableObject, IArchiveDocument, ITableObject, IDataObject, INotifyPropertyChanged, ICloneable
	{
		public IArchiveSettingsCollection Settings { get; set; }

		public new int PreviousId { get; set; }

		public ArchiveDocument()
			: base(TableType.ArchiveDocument)
		{
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		public override object Clone()
		{
			TableObject clone = new ArchiveDocument();
			Clone(ref clone);
			return clone;
		}
	}
}
