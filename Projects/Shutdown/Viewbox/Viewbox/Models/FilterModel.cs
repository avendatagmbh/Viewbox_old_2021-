using System.Collections.Generic;
using ViewboxDb.Filters;

namespace Viewbox.Models
{
	public class FilterModel : BaseModel
	{
		private Operator _opToDisplay;

		public IFilter Filter { get; private set; }

		public FilterModel ParentFilterModel { get; private set; }

		public bool IsOperator { get; set; }

		public Operator OpToDisplay
		{
			get
			{
				return _opToDisplay;
			}
			set
			{
				_opToDisplay = value;
				UseOpToDisplay = true;
			}
		}

		public bool UseOpToDisplay { get; set; }

		public List<int> Level { get; set; }

		public string Number
		{
			get
			{
				if (ParentFilterModel == null)
				{
					return "1";
				}
				BinaryFilter binaryFilter = ParentFilterModel.Filter as BinaryFilter;
				if (binaryFilter != null)
				{
					int position = 1;
					foreach (IFilter condition in binaryFilter.Conditions)
					{
						if (condition == Filter)
						{
							return ParentFilterModel.Number + "." + position;
						}
						position++;
					}
				}
				return ParentFilterModel.Number + ".1";
			}
		}

		public FilterModel(IFilter filter, FilterModel parent)
		{
			Filter = filter;
			ParentFilterModel = parent;
			Level = new List<int>();
		}

		public FilterModel(Operator op)
		{
			OpToDisplay = op;
			IsOperator = true;
			Level = new List<int>();
			Level.Add(1);
		}
	}
}
