using System;
using System.ComponentModel;

namespace SystemDb
{
	public interface IView : ITableObject, IDataObject, INotifyPropertyChanged, ICloneable
	{
	}
}
