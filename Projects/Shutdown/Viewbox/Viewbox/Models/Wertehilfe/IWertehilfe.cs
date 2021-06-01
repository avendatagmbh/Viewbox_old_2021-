using SystemDb;
using ViewboxDb;

namespace Viewbox.Models.Wertehilfe
{
	public interface IWertehilfe
	{
		int RowsPerPage { get; set; }

		int MaxRowCount { get; set; }

		int Start { get; set; }

		WertehilfeSorter Sorter { get; }

		string SearchText { get; set; }

		bool IsExactmatch { get; set; }

		IParameter Parameter { get; set; }

		IColumn Column { get; set; }

		IOptimization Optimization { get; set; }

		string Language { get; set; }

		bool HasDescription { get; }

		bool HasIndex { get; }

		int RowCount { get; }

		int FullWidth { get; }

		int MaxValueWidth { get; }

		string StyleFullWidth { get; }

		ValueListCollection ValueListCollection { get; }

		int Page { get; }

		int MaxPage { get; }

		int MaxRowCountToDisplay { get; }

		void BuildValuesCollection();
	}
}
