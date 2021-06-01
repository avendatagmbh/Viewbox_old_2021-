using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SystemDb;
using Viewbox.Properties;
using ViewboxDb;

namespace Viewbox.Models
{
	public class ViewList : ViewboxModel, ITableObjectList, ICategoryList
	{
		private ListOptionsModel _listOptions;

		private ListOptionsModel _visibleOptions;

		public IEnumerable<SystemDb.IView> Views { get; set; }

		public SystemDb.IView SelectedView => Views.FirstOrDefault();

		public override string LabelCaption => Resources.Views;

		public List<SelectListItem> SelectionList { get; set; }

		public ConcreteSelectionTypeFactory Selection { get; set; }

		public int VonBisOrdinal { get; set; }

		public bool IssuesCheck { get; set; }

		public bool RightsMode => base.Header.RightsMode;

		public TableType Type => TableType.View;

		public ICategoryCollection Categories => ViewboxSession.ViewCategories;

		public ICategory SelectedCategory { get; private set; }

		public string SearchPhrase { get; private set; }

		public IEnumerable<ITableObject> TableObjects => Views;

		public ITableObject SelectedTableObject => SelectedView;

		public string LabelSearch => Resources.ViewSearch;

		public string LabelEnterValue => Resources.LabelEnterValue;

		public string SelectedSystem => ViewboxSession.SelectedSystem;

		public bool ShowEmpty { get; internal set; }

		public bool ShowHidden { get; internal set; }

		public bool ShowArchived { get; internal set; }

		public bool ShowEmptyHidden { get; internal set; }

		public TableCounts TableCount { get; internal set; }

		public int Count { get; internal set; }

		public int CurrentPage { get; internal set; }

		public int PerPage { get; internal set; }

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

		public string LabelNumberOfRows => Resources.Views;

		public string LabelNoData => Resources.NoData;

		public int SortColumn { get; internal set; }

		public SortDirection Direction { get; internal set; }

		public ListOptionsModel ArchiveListOptions
		{
			get
			{
				return _listOptions ?? (_listOptions = new ListOptionsModel());
			}
			set
			{
				_listOptions = value;
			}
		}

		public ListOptionsModel VisibleListOptions
		{
			get
			{
				return _visibleOptions ?? (_visibleOptions = new ListOptionsModel());
			}
			set
			{
				_visibleOptions = value;
			}
		}

		public ViewList(int id = -1, string search = null, bool selectNoCategory = false)
		{
			if (!selectNoCategory)
			{
				SelectedCategory = ViewboxSession.GetViewCategoryById(id);
			}
			SearchPhrase = search?.ToLower();
		}

		public DialogModel GetWaitDialog(string action)
		{
			return new DialogModel
			{
				Title = Resources.PleaseWait,
				Content = string.Format(Resources.ExecutingAction, action),
				DialogType = DialogModel.Type.Info
			};
		}

		public List<SystemDb.IView> GetViewList(bool showEmpty = false, bool showHidden = false, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool isPdfExport = false, bool showEmptyHidden = false)
		{
			IViewCollection views = (isPdfExport ? ViewboxApplication.Database.SystemDb.Views : ViewboxSession.Views);
			List<SystemDb.IView> list = new List<SystemDb.IView>();
			if (showEmptyHidden)
			{
				list.AddRange(from v in views
					where v.Database == SelectedSystem && (SearchPhrase == null || v.GetDescription().ToLower().Contains(SearchPhrase) || v.TableName.ToLower().Contains(SearchPhrase) || v.TransactionNumber.Contains(SearchPhrase)) && v.GetRowCount() < 1 && !v.IsVisible
					orderby v.Ordinal
					select v);
			}
			else if (showEmpty)
			{
				list.AddRange(from v in views
					where v.Database == SelectedSystem && (SearchPhrase == null || v.GetDescription().ToLower().Contains(SearchPhrase) || v.TableName.ToLower().Contains(SearchPhrase) || v.TransactionNumber.Contains(SearchPhrase)) && v.GetRowCount() < 1 && v.IsVisible
					orderby v.Ordinal
					select v);
			}
			else if (showHidden)
			{
				list.AddRange(from v in views
					where v.Database == SelectedSystem && (SearchPhrase == null || v.GetDescription().ToLower().Contains(SearchPhrase) || v.TableName.ToLower().Contains(SearchPhrase) || v.TransactionNumber.Contains(SearchPhrase)) && v.GetRowCount() > 0 && !v.IsVisible
					orderby v.Ordinal
					select v);
			}
			else
			{
				list.AddRange(from v in views
					where v.Category.Id == SelectedCategory.Id && v.Database == SelectedSystem && (SearchPhrase == null || v.GetDescription().ToLower().Contains(SearchPhrase) || v.TableName.ToLower().Contains(SearchPhrase) || v.TransactionNumber.Contains(SearchPhrase)) && v.GetRowCount() > 0 && v.IsVisible
					orderby v.Ordinal
					select v);
			}
			if (sortColumn == 1)
			{
				if (direction == SortDirection.Ascending)
				{
					list = new List<SystemDb.IView>(from t in ViewboxSession.Views
						orderby t.GetObjectType(), t.TransactionNumber
						select t);
				}
				if (direction == SortDirection.Descending)
				{
					list = new List<SystemDb.IView>(from t in ViewboxSession.Views
						orderby t.GetObjectType() descending, t.TransactionNumber
						select t);
				}
			}
			if (sortColumn == 2)
			{
				if (direction == SortDirection.Ascending)
				{
					list = new List<SystemDb.IView>(ViewboxSession.Views.OrderBy((SystemDb.IView t) => t.GetObjectType()).ThenBy((SystemDb.IView l) => l.GetDescription()));
				}
				if (direction == SortDirection.Descending)
				{
					list = new List<SystemDb.IView>(ViewboxSession.Views.OrderByDescending((SystemDb.IView t) => t.GetObjectType()).ThenBy((SystemDb.IView l) => l.GetDescription()));
				}
			}
			if (sortColumn == 3)
			{
				if (direction == SortDirection.Ascending)
				{
					list = new List<SystemDb.IView>(from t in ViewboxSession.Views
						orderby t.GetObjectType(), t.GetRowCount()
						select t);
				}
				if (direction == SortDirection.Descending)
				{
					list = new List<SystemDb.IView>(from t in ViewboxSession.Views
						orderby t.GetObjectType() descending, t.GetRowCount()
						select t);
				}
			}
			return list;
		}

		public List<SystemDb.IView> GetListFromSessionViews(int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool showEmpty = false, bool filtered = false)
		{
			List<SystemDb.IView> list = new List<SystemDb.IView>();
			switch (sortColumn)
			{
			case 1:
				if (direction == SortDirection.Ascending)
				{
					list = new List<SystemDb.IView>(ViewboxSession.Views.OrderBy((SystemDb.IView t) => t.TransactionNumber));
				}
				if (direction == SortDirection.Descending)
				{
					list = new List<SystemDb.IView>(ViewboxSession.Views.OrderByDescending((SystemDb.IView t) => t.TransactionNumber));
				}
				break;
			case 2:
				if (direction == SortDirection.Ascending)
				{
					list = new List<SystemDb.IView>(ViewboxSession.Views.OrderBy((SystemDb.IView l) => l.GetDescription()));
				}
				if (direction == SortDirection.Descending)
				{
					list = new List<SystemDb.IView>(ViewboxSession.Views.OrderByDescending((SystemDb.IView l) => l.GetDescription()));
				}
				break;
			case 3:
				if (showEmpty)
				{
					list = new List<SystemDb.IView>(from t in ViewboxSession.Views
						orderby t.GetObjectType(), t.Ordinal
						select t);
					break;
				}
				if (direction == SortDirection.Ascending)
				{
					list = new List<SystemDb.IView>(ViewboxSession.Views.OrderBy((SystemDb.IView t) => t.GetRowCount()));
				}
				if (direction == SortDirection.Descending)
				{
					list = new List<SystemDb.IView>(ViewboxSession.Views.OrderByDescending((SystemDb.IView t) => t.GetRowCount()));
				}
				break;
			case 4:
				if (filtered)
				{
					list = new List<SystemDb.IView>(from t in ViewboxSession.Views
						orderby t.GetObjectType(), t.Ordinal
						select t);
					break;
				}
				if (direction == SortDirection.Ascending)
				{
					list = new List<SystemDb.IView>(from t in ViewboxSession.Views
						orderby t.GetObjectType(), t.GetDescription()
						select t);
				}
				if (direction == SortDirection.Descending)
				{
					list = new List<SystemDb.IView>(from t in ViewboxSession.Views
						orderby t.GetObjectType() descending, t.GetDescription()
						select t);
				}
				break;
			default:
			{
				bool ordered = false;
				IProperty property = ViewboxApplication.FindProperty("table_order");
				if (property != null && property.Value.ToLower() == "true")
				{
					Dictionary<int, int> tableOrder = ViewboxApplication.Database.SystemDb.GetTableObjectOrder(ViewboxSession.User, TableType.View);
					if (tableOrder != null)
					{
						ordered = true;
						list = new List<SystemDb.IView>(ViewboxSession.Views.OrderBy((SystemDb.IView t) => tableOrder.ContainsKey(t.Id) ? tableOrder[t.Id] : (t.Ordinal + tableOrder.Count)));
					}
				}
				if (!ordered)
				{
					list = new List<SystemDb.IView>(from t in ViewboxSession.Views
						orderby t.Ordinal, t.GetDescription()
						select t);
				}
				break;
			}
			}
			return list;
		}
	}
}
