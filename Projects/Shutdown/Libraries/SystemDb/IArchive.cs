using System;
using System.ComponentModel;

namespace SystemDb
{
	public interface IArchive : ITableObject, IDataObject, INotifyPropertyChanged, ICloneable
	{
		IArchiveSettingsCollection Settings { get; }
	}
}
