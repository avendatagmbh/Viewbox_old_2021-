using System;
using SystemDb;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class TableObjectList : ViewboxModel, ICategoryList
	{
		public int Count { get; set; }

		public int CurrentPage { get; set; }

		public int PerPage { get; set; }

		public int LastPage
		{
			get
			{
				if (PerPage == 0)
				{
					return 0;
				}
				return (Count % PerPage == 0) ? (Count / PerPage - 1) : (Count / PerPage);
			}
		}

		public int From => CurrentPage * PerPage;

		public int To => Math.Min(Count, From + PerPage) - 1;

		public override string LabelCaption => Resources.Tables;

		public TableType Type { get; set; }

		public ICategory SelectedCategory { get; set; }

		public bool RightsMode
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public ICategoryCollection Categories { get; set; }
	}
}
