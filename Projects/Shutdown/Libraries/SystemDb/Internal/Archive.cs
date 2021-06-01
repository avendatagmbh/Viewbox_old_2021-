using System;
using System.ComponentModel;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("tables", ForceInnoDb = true)]
	internal class Archive : TableObject, IArchive, ITableObject, IDataObject, INotifyPropertyChanged, ICloneable
	{
		public new int PreviousId { get; set; }

		public IArchiveSettingsCollection Settings { get; private set; }

		public Archive()
			: base(TableType.Archive)
		{
			Settings = new ArchiveSettingsCollection();
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		public override object Clone()
		{
			TableObject clone = new Archive();
			Clone(ref clone);
			(clone as Archive).Settings = Settings;
			return clone;
		}
	}
}
