using SystemDb;

namespace Viewbox.Models
{
	public interface ICategoryList
	{
		bool RightsMode { get; }

		ICategoryCollection Categories { get; }

		ICategory SelectedCategory { get; }

		TableType Type { get; }
	}
}
