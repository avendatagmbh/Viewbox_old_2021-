using System.Collections.Generic;
using System.ComponentModel;

namespace SystemDb
{
	public interface IMapBusinessObjectList : INotifyPropertyChanged
	{
		string ProcessedElements { get; set; }

		void MapCollection<T>(List<T> itemsToMap);
	}
}
