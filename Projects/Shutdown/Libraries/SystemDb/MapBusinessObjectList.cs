using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using DbAccess;

namespace SystemDb
{
	public class MapBusinessObjectList : IMapBusinessObjectList, INotifyPropertyChanged
	{
		private string _processedElements;

		private DatabaseBase Database { get; set; }

		public string ProcessedElements
		{
			get
			{
				return _processedElements;
			}
			set
			{
				if (_processedElements != value)
				{
					_processedElements = value;
					OnPropertyChanged("ProcessedElements");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public MapBusinessObjectList(DatabaseBase database)
		{
			Database = database;
		}

		public void MapCollection<T>(List<T> itemsToMap)
		{
			using (Database)
			{
				Database.Open();
				int maxItems = itemsToMap.Count;
				int currentItem = 0;
				Stopwatch sw = Stopwatch.StartNew();
				foreach (T item in itemsToMap)
				{
					Database.DbMapping.Save(item);
					currentItem++;
					if (currentItem % 100 == 0 || currentItem == maxItems)
					{
						ProcessedElements = string.Format("{0}/{1}  Avg creation time:{2,12:#,#.0000} sec", currentItem, maxItems, (currentItem == 0) ? 0.0 : ((double)sw.ElapsedMilliseconds / (double)currentItem / 1000.0));
					}
				}
			}
		}

		private void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
