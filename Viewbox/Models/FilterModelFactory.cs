using ViewboxDb.Filters;

namespace Viewbox.Models
{
	public static class FilterModelFactory
	{
		public static FilterModel GetFilterModel(IFilter filter, FilterModel parentModel)
		{
			return new FilterModel(filter, parentModel);
		}

		public static FilterModel GetFilterFromOp(Operator op)
		{
			return new FilterModel(op);
		}
	}
}
