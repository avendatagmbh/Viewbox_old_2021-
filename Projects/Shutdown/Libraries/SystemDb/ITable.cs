using System;
using System.ComponentModel;

namespace SystemDb
{
	public interface ITable : ITableObject, IDataObject, INotifyPropertyChanged, ICloneable
	{
		string OriginalName { get; }
	}
}
