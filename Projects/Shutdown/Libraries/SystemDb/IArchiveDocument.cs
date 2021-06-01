using System;
using System.ComponentModel;

namespace SystemDb
{
	public interface IArchiveDocument : ITableObject, IDataObject, INotifyPropertyChanged, ICloneable
	{
	}
}
