using System.Collections.Generic;
using System.Linq;
using SystemDb.Resources;
using Viewbox.Properties;
using ViewboxDb;

namespace Viewbox.Models
{
	public class ColumnValueListModels : ViewboxModel
	{
		private ValueListCollection _valueListCollection;

		public ValueListCollection ValueListCollection
		{
			get
			{
				if (_valueListCollection != null && _valueListCollection.Count() > 0 && AddShowAllItem)
				{
					ValueListCollection tempList = _valueListCollection.DeepClone();
					tempList.Insert(0, new ValueListElement
					{
						Id = -1,
						Value = Viewbox.Properties.Resources.ShowAll
					});
					return tempList;
				}
				return _valueListCollection;
			}
			set
			{
				_valueListCollection = value;
			}
		}

		public SortDirection Direction { get; internal set; }

		public string PopupTitle { get; set; }

		public bool AddShowAllItem { get; set; }

		public override string LabelCaption => "";

		public ColumnValueListModels(IEnumerable<string> columns)
		{
			ValueListCollection = new ValueListCollection();
			ValueListCollection.AddRange(columns.Select((string c) => new ValueListElement
			{
				Value = c
			}));
		}

		public ColumnValueListModels(IEnumerable<ValueListElement> columns, bool addDartValue = false)
		{
			ValueListCollection = new ValueListCollection();
			ValueListCollection.AddRange(columns);
			if (addDartValue)
			{
				ValueListCollection.Add(new ValueListElement
				{
					Value = SystemDb.Resources.Resources.ResourceManager.GetString("other", ViewboxSession.Language.CultureInfo),
					Id = 0
				});
			}
		}

		internal void Sort(SortDirection direction)
		{
			if (ValueListCollection == null || ValueListCollection.Count() <= 0)
			{
				return;
			}
			Direction = direction;
			if (direction == SortDirection.Ascending)
			{
				ValueListCollection.OrderBy((ValueListElement v) => v.Value);
			}
			else
			{
				ValueListCollection.OrderByDescending((ValueListElement v) => v.Value);
			}
		}
	}
}
