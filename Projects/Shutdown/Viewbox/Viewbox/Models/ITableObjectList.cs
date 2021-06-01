using System.Collections.Generic;
using System.Web.Mvc;
using SystemDb;
using ViewboxDb;

namespace Viewbox.Models
{
	public interface ITableObjectList : ICategoryList
	{
		IEnumerable<ITableObject> TableObjects { get; }

		ITableObject SelectedTableObject { get; }

		string SelectedSystem { get; }

		string SearchPhrase { get; }

		string LabelSearch { get; }

		string LabelEnterValue { get; }

		bool ShowEmpty { get; }

		bool ShowHidden { get; }

		bool ShowEmptyHidden { get; }

		bool ShowArchived { get; }

		int SortColumn { get; }

		SortDirection Direction { get; }

		TableCounts TableCount { get; }

		bool IssuesCheck { get; set; }

		int Count { get; }

		int CurrentPage { get; }

		int PerPage { get; }

		int LastPage { get; }

		int From { get; }

		int To { get; }

		string LabelNumberOfRows { get; }

		string LabelNoData { get; }

		int VonBisOrdinal { get; set; }

		List<SelectListItem> SelectionList { get; set; }

		ConcreteSelectionTypeFactory Selection { get; set; }

		ListOptionsModel ArchiveListOptions { get; }

		ListOptionsModel VisibleListOptions { get; }

		DialogModel GetWaitDialog(string action);
	}
}
