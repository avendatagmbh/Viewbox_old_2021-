using SystemDb;

namespace Viewbox.Models
{
	public interface IColumnFilterModel
	{
		bool ColumnFilterEnabled { get; set; }

		bool ExactMatch { get; set; }

		IColumn[] FilterColumns { get; set; }

		string[] FilterColumnDescriptions { get; set; }

		string[] ParameterValues { get; set; }
	}
}
