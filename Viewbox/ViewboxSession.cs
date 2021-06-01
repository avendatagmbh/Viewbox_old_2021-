using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using SystemDb;
using SystemDb.Helper;
using SystemDb.Internal;
using AV.Log;
using DbAccess;
using DbAccess.Structures;
using log4net;
using Viewbox.Enums;
using Viewbox.Job;
using Viewbox.Models;
using Viewbox.Models.Wertehilfe;
using Viewbox.Properties;
using ViewboxDb;
using ViewboxDb.Exceptions;
using ViewboxDb.Filters;

namespace Viewbox
{
	public static class ViewboxSession
	{
		private static ILog _log = LogHelper.GetLogger();

		private static List<OptimizationType> _issueOptimizationFilter;

		private static ConcurrentDictionary<string, string> _currentlyRunningProcedures = new ConcurrentDictionary<string, string>();

		public static int? updMode = null;

		public static bool loadingIsVisible = false;

		private static string allowedIndex;

		private static string allowedSplit;

		private static string allowedSort;

		public static IOptimization LastOptimization;

		public static List<Tuple<OptimizationType, string>> AllowedOpts;

		public static string PreviousDynamicViewKey;

		public static bool isCurrentDynamic = false;

		public static Dictionary<int, List<int>> PreviousColumnOrder = new Dictionary<int, List<int>>();

		private static List<RoleTableObjectRightsCount> _issueViewTableCount;

		public static int isFreeSpaceChecked { get; set; }

		public static int mySqlConfigReady { get; set; }

		public static ViewOptionEnum ViewOptionEnum { get; set; }

		public static int LastIssueId { get; set; }

		public static bool IsAvAdmin
		{
			get
			{
				try
				{
					return User != null && !string.IsNullOrEmpty(User.UserName) && User.UserName.ToLower() == "avendata_admin";
				}
				catch
				{
					return false;
				}
			}
		}

		public static bool IsAdmin => User != null && User.IsSuper;

		public static bool HasOptChanged { get; set; }

		public static List<OptimizationType> IssueOptimizationFilter
		{
			get
			{
				if (_issueOptimizationFilter == null)
				{
					_issueOptimizationFilter = new List<OptimizationType>();
				}
				return _issueOptimizationFilter;
			}
			set
			{
				_issueOptimizationFilter = value;
			}
		}

		public static bool HideIssuesButton => !IsAvAdmin && ViewboxApplication.HideIssuesButton;

		public static bool HideViewsButton => !IsAvAdmin && ViewboxApplication.HideViewsButton;

		public static bool HideTablesButton => !IsAvAdmin && ViewboxApplication.HideTablesButton;

		public static ConcurrentDictionary<string, string> CurrentlyRunningProcedures => _currentlyRunningProcedures;

		public static string SearchTablePdfExport { get; set; }

		public static bool IsInitialized => User != null && HttpContextFactory.Current.Session != null && UserObjects != null && Language != null;

		public static string Key => HttpContextFactory.Current.Session.SessionID;

		public static bool IsMySQL => true;

		public static Dictionary<int, string> OpenIssueDate { get; set; }

		public static ILanguage Language
		{
			get
			{
				if (HttpContextFactory.Current != null && HttpContextFactory.Current.Session != null)
				{
					ILanguage lang = HttpContextFactory.Current.Session["Language"] as ILanguage;
					if (lang != null)
					{
						return lang;
					}
					try
					{
						if (ViewboxApplication.RequiredLanguages != null)
						{
							return ViewboxApplication.Languages.FirstOrDefault((ILanguage x) => x.CountryCode == ViewboxApplication.RequiredLanguages.First());
						}
						return ViewboxApplication.Languages.First() ?? ViewboxApplication.BrowserLanguage;
					}
					catch (Exception)
					{
						return null;
					}
				}
				return ViewboxApplication.BrowserLanguage;
			}
			set
			{
				if (value != null)
				{
					if (User != null)
					{
						User.CurrentLanguage = value.CountryCode;
					}
					HttpContextFactory.Current.Session["Language"] = value;
					MvcApplication.SetLanguage(Language);
				}
			}
		}

		public static bool IsAuthenticated => HttpContextFactory.Current != null && HttpContextFactory.Current.Request.IsAuthenticated;

		public static IUser User => IsAuthenticated ? ViewboxApplication.ByUserName(HttpContextFactory.Current.User.Identity.Name) : null;

		public static RightHelper UserRightHelper { get; set; }

		public static bool UserInitialized { get; set; }

		public static List<DocumentFileModel> SavedDocumentSettings
		{
			get
			{
				List<DocumentFileModel> temp = HttpContextFactory.Current.Session["SavedDocumentSettings"] as List<DocumentFileModel>;
				return temp ?? new List<DocumentFileModel>();
			}
		}

		public static string CurrentTransactionKey { get; set; }

		internal static SessionMarker.UserSessions SessionMarker => ViewboxApplication.UserSessions[User];

		private static IUserObjects UserObjects
		{
			get
			{
				return HttpContextFactory.Current.Session["UserObjects"] as IUserObjects;
			}
			set
			{
				HttpContextFactory.Current.Session["UserObjects"] = value;
			}
		}

		public static ViewboxDb.TableObjectCollection TempTableObjects
		{
			get
			{
				return HttpContextFactory.Current.Session["TempTableObjects"] as ViewboxDb.TableObjectCollection;
			}
			set
			{
				HttpContextFactory.Current.Session["TempTableObjects"] = value;
			}
		}

		public static ITableObjectCollection TableObjects => UserObjects.TableObjects;

		public static ICategoryCollection Categories => UserObjects.Categories;

		public static IIssueCollection Issues => UserObjects.Issues;

		public static int IssueCount => UserObjects.IssueCount;

		public static ICategoryCollection IssueCategories => UserObjects.IssueCategories;

		public static SortAndFilterSettings IssueSortAndFilterSettings
		{
			get
			{
				return HttpContextFactory.Current.Session["IssueSortAndFilterSetting"] as SortAndFilterSettings;
			}
			set
			{
				HttpContextFactory.Current.Session["IssueSortAndFilterSetting"] = value;
			}
		}

		public static IViewCollection Views => UserObjects.Views;

		public static int ViewCount => UserObjects.ViewCount;

		public static ICategoryCollection ViewCategories => UserObjects.ViewCategories;

		public static SortAndFilterSettings ViewSortAndFilterSettings
		{
			get
			{
				return HttpContextFactory.Current.Session["ViewSortAndFilterSetting"] as SortAndFilterSettings;
			}
			set
			{
				HttpContextFactory.Current.Session["ViewSortAndFilterSetting"] = value;
			}
		}

		public static ITableCollection Tables => UserObjects.Tables;

		public static int TableCount => UserObjects.TableCount;

		public static ICategoryCollection TableCategories => UserObjects.TableCategories;

		public static SortAndFilterSettings TableSortAndFilterSettings
		{
			get
			{
				return HttpContextFactory.Current.Session["TableSortAndFilterSetting"] as SortAndFilterSettings;
			}
			set
			{
				HttpContextFactory.Current.Session["TableSortAndFilterSetting"] = value;
			}
		}

		public static IArchiveCollection Archives => UserObjects.Archives;

		public static IArchiveDocumentCollection ArchiveDocuments => UserObjects.ArchiveDocuments;

		public static IFullColumnCollection Columns => UserObjects.Columns;

		public static IEnumerable<IOptimization> Optimizations
		{
			get
			{
				return HttpContextFactory.Current.Session["Optimizations"] as IEnumerable<IOptimization>;
			}
			private set
			{
				HttpContextFactory.Current.Session["Optimizations"] = value;
			}
		}

		public static bool? IsThereEmptyMessage
		{
			get
			{
				return (bool?)HttpContextFactory.Current.Session["IsThereEmptyMessage"];
			}
			set
			{
				HttpContextFactory.Current.Session["IsThereEmptyMessage"] = value;
			}
		}

		public static bool? IsThereOptChangeEmptyMessage
		{
			get
			{
				return (bool?)HttpContextFactory.Current.Session["IsThereOptChangeEmptyMessage"];
			}
			set
			{
				HttpContextFactory.Current.Session["IsThereOptChangeEmptyMessage"] = value;
			}
		}

		public static IOptimizationCollection AllowedOptimizations => (UserObjects != null) ? UserObjects.Optimizations : null;

		public static string AllowedIndexString
		{
			get
			{
				if (allowedIndex == null)
				{
					IOptimization[] allowedMandant = AllowedMandant;
					foreach (IOptimization item in allowedMandant)
					{
						if (item.Value != null)
						{
							allowedIndex = allowedIndex + item.Id + ", ";
						}
					}
					allowedIndex = allowedIndex.Remove(allowedIndex.Length - 2);
				}
				return allowedIndex;
			}
		}

		public static string AllowedSplitString
		{
			get
			{
				if (allowedSplit == null)
				{
					IOptimization[] allowedBukrs = AllowedBukrs;
					foreach (IOptimization item in allowedBukrs)
					{
						if (item.Value != null)
						{
							allowedSplit = allowedSplit + item.Id + ", ";
						}
					}
					allowedSplit = allowedSplit.Remove(allowedSplit.Length - 2);
				}
				return allowedSplit;
			}
		}

		public static string AllowedSortString
		{
			get
			{
				if (allowedSort == null)
				{
					IOptimization[] allowedGjahr = AllowedGjahr;
					foreach (IOptimization item in allowedGjahr)
					{
						if (item.Value != null)
						{
							allowedSort = allowedSort + item.Id + ", ";
						}
					}
					allowedSort = allowedSort.Remove(allowedSort.Length - 2);
				}
				return allowedSort;
			}
		}

		public static IOptimization[] AllowedSystem => AllowedOptimizations.Where((IOptimization x) => x.Group.Type == OptimizationType.System).ToArray();

		public static IOptimization[] AllowedMandant => AllowedOptimizations.Where((IOptimization x) => x.Group.Type == OptimizationType.IndexTable).ToArray();

		public static IOptimization[] AllowedBukrs => AllowedOptimizations.Where((IOptimization x) => x.Group.Type == OptimizationType.SplitTable).ToArray();

		public static IOptimization[] AllowedGjahr => AllowedOptimizations.Where((IOptimization x) => x.Group.Type == OptimizationType.SortColumn).ToArray();

		public static List<UserDefinedWidth> UserDefinedColumnWidths
		{
			get
			{
				return HttpContextFactory.Current.Session["UserDefinedColumnWidth"] as List<UserDefinedWidth>;
			}
			set
			{
				HttpContextFactory.Current.Session["UserDefinedColumnWidth"] = value;
			}
		}

		public static List<BackStack> PrevPageList
		{
			get
			{
				return HttpContextFactory.Current.Session["BackStackList"] as List<BackStack>;
			}
			set
			{
				HttpContextFactory.Current.Session["BackStackList"] = value;
			}
		}

		public static int BsI
		{
			get
			{
				if (HttpContextFactory.Current.Session["BackStackListIndex"] != null && int.TryParse(HttpContextFactory.Current.Session["BackStackListIndex"].ToString(), out var temp))
				{
					return (temp >= 0) ? temp : 0;
				}
				return 0;
			}
			set
			{
				HttpContextFactory.Current.Session["BackStackListIndex"] = value;
			}
		}

		public static string CurrentPage { get; set; }

		public static int LastOpt => Optimizations.LastOrDefault().Id;

		public static int LastDocOpt { get; set; }

		public static string SkippedPage { get; private set; }

		public static string PrevPage
		{
			get
			{
				BackStack temp = PrevPageList[BsI];
				BsI--;
				return temp.Link;
			}
			set
			{
				if (PrevPageList != null && PrevPageList.Count == 0)
				{
					PrevPageList.Add(new BackStack(value, 0));
				}
				else
				{
					AddToPrevPageList(new BackStack(value, LastOpt));
				}
			}
		}

		public static Credential RightsModeCredential
		{
			get
			{
				return HttpContextFactory.Current.Session["RightModeCredential"] as Credential;
			}
			private set
			{
				HttpContextFactory.Current.Session["RightModeCredential"] = value;
			}
		}

		public static IUserObjects RightsModeObjects
		{
			get
			{
				return HttpContextFactory.Current.Session["RightsModeUserObjects"] as IUserObjects;
			}
			private set
			{
				HttpContextFactory.Current.Session["RightsModeUserObjects"] = value;
			}
		}

		public static bool RightsMode => RightsModeCredential != null;

		public static string SelectedSystem => Optimizations.FirstOrDefault((IOptimization o) => o.Group.Type == OptimizationType.System)?.Value;

		public static Dictionary<int, bool> CheckedTableItems
		{
			get
			{
				if (HttpContextFactory.Current.Session["CheckedTableItems"] == null)
				{
					HttpContextFactory.Current.Session["CheckedTableItems"] = new Dictionary<int, bool>();
				}
				return (Dictionary<int, bool>)HttpContextFactory.Current.Session["CheckedTableItems"];
			}
		}

		public static bool RedirectionByCandCHappened { get; set; }

		public static Dictionary<string, string> LastPositionUserViewList { get; set; }

		public static Dictionary<string, string> LastPositionUserTableList { get; set; }

		public static void SaveStartLoadingTime()
		{
			DateTime start = ((DateTime?)HttpContextFactory.Current.Session["loadingtime"]) ?? DateTime.Now;
			HttpContextFactory.Current.Session["loadingtime"] = start;
		}

		public static int GetLoadingTime()
		{
			DateTime? start = (DateTime?)HttpContextFactory.Current.Session["loadingtime"];
			int result = (start.HasValue ? (DateTime.Now - start.Value).Milliseconds : 0);
			HttpContextFactory.Current.Session["loadingtime"] = null;
			return result;
		}

		public static IEnumerable<Base> GetFinishedJobs()
		{
			return new List<Base>(from j in Base.List
				where j.Listeners.Contains(User) && j.Status != Base.JobStatus.Running
				orderby j.RequestTime[User] descending
				select j);
		}

		public static void Init()
		{
			PrevPageList = new List<BackStack>();
			UserDefinedColumnWidths = new List<UserDefinedWidth>();
			ILanguage language = ((HttpContextFactory.Current.Session != null) ? (HttpContextFactory.Current.Session["Language"] as ILanguage) : null) ?? ViewboxApplication.BrowserLanguage;
			HttpContextFactory.Current.Session.Clear();
			Language = language;
			SetupObjects();
		}

		public static string GetColumnWidths(int id)
		{
			return ViewboxApplication.Database.SystemDb.UserTableColumnWidthsSettings[User, TableObjects[id]]?.ColumnWidths;
		}

		public static void AddTableObject(int id)
		{
			ViewboxApplication.Database.SystemDb.AddTableToTableObjects(User, TableObjects, id);
		}

		public static void AddIssue(int id)
		{
			ViewboxApplication.Database.SystemDb.AddTableToIssue(User, TableObjects, Issues, id);
		}

		public static void SetupTableColumns(int id)
		{
			IProperty prop = ViewboxApplication.FindProperty("empty_distinct_columns");
			bool useDistinctInfo = prop != null && prop.Value.ToLower() == "true";
			ViewboxApplication.Database.SystemDb.AddTableColumns(User, Columns, TableObjects, Tables, Views, Optimizations.LastOrDefault(), useDistinctInfo, id);
		}

		public static TableCounts GetTableObjectCount(TableType type, string search, int? objectTypeFilter = null, int? extendedObjectTypeFilter = null)
		{
			return ViewboxApplication.Database.SystemDb.GetObjectCounts(type, User, SelectedSystem, search, Language, Optimizations.LastOrDefault(), objectTypeFilter, extendedObjectTypeFilter);
		}

		public static void SetupTableObjectsForExport(TableType type, int page, int size, string search, int sortColumn, bool direction, string database, out int count)
		{
			ViewboxApplication.Database.SystemDb.AddTableObjectsForExport(type, User, Tables, Views, TableObjects, database, search, page, size, sortColumn, direction, Language, Optimizations.LastOrDefault(), out count);
		}

		public static void TableObjectSetup(TableType type, int page, int size, string search, bool showEmpty, bool showHidden, bool showArchived, int sortColumn, bool direction, out int fullTableListCount, bool showEmptyHidden = false, IEnumerable<int> FavoriteIds = null, int? objectTypeFilter = null, int? extendedObjectTypeFilter = null)
		{
			ViewboxApplication.Database.SystemDb.AddTableObjects(type, User, Tables, Views, Issues, TableObjects, SelectedSystem, search, page, size, showEmpty, showHidden, showArchived, sortColumn, direction, Language, Optimizations.LastOrDefault(), out fullTableListCount, FavoriteIds, objectTypeFilter, extendedObjectTypeFilter, showEmptyHidden);
		}

		public static void LoadFavoriteIssues(IEnumerable<int> favIds)
		{
			ViewboxApplication.Database.SystemDb.AddIssues(favIds, User, Issues, TableObjects, SelectedSystem);
		}

		public static void SetupObjects()
		{
			IFullColumnCollection tempCols = null;
			ITableObjectCollection tempTableObjects = null;
			if (UserObjects != null)
			{
				if (UserObjects.Issues != null)
				{
					UserObjects.Issues.RemoveHandler();
				}
				if (UserObjects.Tables != null)
				{
					UserObjects.Tables.RemoveHandler();
				}
				if (UserObjects.Views != null)
				{
					UserObjects.Views.RemoveHandler();
				}
				ViewboxApplication.Database.SystemDb.CopyColumns(UserObjects.Columns, out tempCols);
				ViewboxApplication.Database.SystemDb.CopyTableObjects(UserObjects.TableObjects, out tempTableObjects);
			}
			allowedIndex = null;
			allowedSplit = null;
			allowedSort = null;
			_issueViewTableCount = null;
			UserObjects = null;
			GC.Collect();
			UserObjects = ViewboxApplication.Database.SystemDb.GetUserObjects(User);
			if (tempCols != null)
			{
				ViewboxApplication.Database.SystemDb.CopyColumnsToUserObjects(tempCols, UserObjects);
			}
			if (tempTableObjects != null)
			{
				ViewboxApplication.Database.SystemDb.CopyTableObjectsToUserObjects(tempTableObjects, UserObjects);
			}
			TempTableObjects = new ViewboxDb.TableObjectCollection();
			List<ViewboxDb.TableObject> list = new List<ViewboxDb.TableObject>(from j in Transformation.List
				where j.Listeners.Contains(User) && j.TransformationObject != null
				select j.TransformationObject);
			foreach (ViewboxDb.TableObject to in list)
			{
				if (to.Table != null && to.Table.Id > 0)
				{
					ViewboxApplication.Database.SystemDb.RemoveColumns(UserObjects.Columns, to.Table.Columns);
				}
			}
			foreach (ViewboxDb.TableObject to2 in list)
			{
				if (to2.Table != null && to2.Table.Id < 0)
				{
					TempTableObjects.Add(to2);
				}
				if (to2.Table != null && to2.Table.Id > 0 && !TableObjects.Contains(to2.Table.Id))
				{
					TableObjects.Add(to2.Table.Clone() as ITableObject);
				}
				if (ViewboxApplication.Database.SystemDb.AddColumnToCollection(Columns, (to2.Table.Clone() as ITableObject).Columns))
				{
					List<Tuple<int, string>> columnOrder = ViewboxApplication.Database.SystemDb.ResetColumnOrder(to2.Table.Clone() as ITableObject, User, null);
				}
			}
			if (User != null)
			{
				IOptimization optimization = ViewboxApplication.Database.SystemDb.GetUserOptimization(User);
				if (optimization != null)
				{
					SetOptimization(optimization.Id);
				}
				else
				{
					SetOptimization();
				}
			}
			else
			{
				SetOptimization();
			}
		}

		public static Dictionary<int, string> OpenIssueDateDefine()
		{
			return new Dictionary<int, string>();
		}

		public static void Logoff()
		{
			try
			{
				PrevPageList = new List<BackStack>();
				UserDefinedColumnWidths = new List<UserDefinedWidth>();
				ViewboxApplication.Database.SystemDb.UpdateUserOptimization(User, Optimizations.Last(), saveToDb: true);
			}
			catch (Exception)
			{
			}
			if (UserObjects != null)
			{
				if (UserObjects.Issues != null)
				{
					UserObjects.Issues.RemoveHandler();
				}
				if (UserObjects.Tables != null)
				{
					UserObjects.Tables.RemoveHandler();
				}
				if (UserObjects.Views != null)
				{
					UserObjects.Views.RemoveHandler();
				}
			}
			UserObjects = null;
			GC.Collect();
			ILanguage lang = HttpContextFactory.Current.Session["Language"] as ILanguage;
			HttpContextFactory.Current.Session.Clear();
			if (lang != null)
			{
				Language = lang;
			}
		}

		public static void UpdateSavedDocumentSettings(string path, string reverse)
		{
			List<DocumentFileModel> original = (HttpContextFactory.Current.Session["SavedDocumentSettings"] as List<DocumentFileModel>) ?? new List<DocumentFileModel>();
			original.Add(new DocumentFileModel
			{
				Url = path,
				Reverse = reverse
			});
			HttpContextFactory.Current.Session["SavedDocumentSettings"] = original;
		}

		public static void ClearSavedDocumentSettings(string usercontroller)
		{
			if (!string.IsNullOrWhiteSpace(usercontroller) && usercontroller.ToLower() != "datagrid")
			{
				ViewboxApplication.Database.SystemDb.UpdateUserController(User, usercontroller);
			}
			HttpContextFactory.Current.Session["SavedDocumentSettings"] = null;
		}

		public static ICategory GetIssueCategoryById(int id = -1)
		{
			return (id < 0) ? IssueCategories.FirstOrDefault() : UserObjects.IssueCategories[id];
		}

		public static ICategory GetViewCategoryById(int id = -1)
		{
			return (id < 0) ? ViewCategories.FirstOrDefault() : UserObjects.ViewCategories[id];
		}

		public static ICategory GetTableCategoryById(int id = -1)
		{
			return (id < 0) ? TableCategories.FirstOrDefault() : UserObjects.TableCategories[id];
		}

		public static List<string> AllowedGjahrByBukrs(string Bukrs)
		{
			return (from x in AllowedOptimizations
				where x != null && x.Value != null && x.Value.ToString() != "" && x.Group != null && x.Group.Type == OptimizationType.SortColumn && x.Parent != null && x.Parent.Group != null && x.Parent.Group.Type == OptimizationType.SplitTable && x.Parent.Value == Bukrs
				select x.Value).ToList();
		}

		public static string GetRoleBasedOptimizationFilter(ITableObject tinfo, IUser user = null)
		{
			ICoreProperty icp = ViewboxApplication.FindCoreProperty("roleBasedOptFilter");
			if (icp == null || icp.Value == "false")
			{
				return "";
			}
			if ((User != null && User.IsSuper) || (user != null && user.IsSuper) || HttpContextFactory.Current == null)
			{
				return "";
			}
			IOptimization currentSystem = Optimizations.LastOrDefault().GetOptimization(OptimizationType.System);
			IOptimization currentMandt = Optimizations.LastOrDefault().GetOptimization(OptimizationType.IndexTable);
			bool allBukrsAllowed = AllowedBukrs.Any((IOptimization x) => x != null && x.Parent == currentMandt && x.Value == null);
			IOptimizationCollection AllowedOptimizationsTemp = null;
			StringBuilder sqlFilters = new StringBuilder();
			if (user != null && (HttpContextFactory.Current == null || UserObjects == null || AllowedOptimizations == null))
			{
				AllowedOptimizationsTemp = ViewboxApplication.Database.SystemDb.GetUserObjects(user).Optimizations;
			}
			if (AllowedOptimizationsTemp == null)
			{
				if (tinfo.Columns.Any((IColumn c) => c.OptimizationType == OptimizationType.SplitTable) && !allBukrsAllowed)
				{
					sqlFilters.Append("(");
					foreach (IColumn bukrsColumn2 in tinfo.Columns.Where((IColumn c) => c.OptimizationType == OptimizationType.SplitTable))
					{
						foreach (string bukrs2 in (from x in AllowedBukrs
							where x.Parent == currentMandt
							select x.Value).ToList())
						{
							sqlFilters.Append("(");
							sqlFilters.Append(bukrsColumn2.Name + " = " + bukrs2);
							foreach (IColumn gjahrColumn4 in tinfo.Columns.Where((IColumn c) => c.OptimizationType == OptimizationType.SortColumn))
							{
								if (!AllowedOptimizations.Any((IOptimization x) => x != null && x.Value == null && x.Group != null && x.Group.Type == OptimizationType.SortColumn && x.Parent != null && x.Parent.Group != null && x.Parent.Group.Type == OptimizationType.SplitTable && x.Parent.Value == bukrs2))
								{
									sqlFilters.Append(" AND " + gjahrColumn4.Name + " IN(" + string.Join(",", AllowedGjahrByBukrs(bukrs2)) + ") ");
								}
							}
							sqlFilters.Append(") OR ");
						}
					}
					if (sqlFilters.ToString().EndsWith("OR "))
					{
						sqlFilters.Length -= 3;
					}
					sqlFilters.Append(")");
				}
				else
				{
					foreach (IColumn gjahrColumn3 in tinfo.Columns.Where((IColumn c) => c.OptimizationType == OptimizationType.SortColumn))
					{
						if (!AllowedOptimizations.Any((IOptimization x) => x != null && x.Value == null && x.Group != null && x.Group.Type == OptimizationType.SortColumn && x.Parent != null && x.Parent.Group != null && x.Parent.Group.Type == OptimizationType.SplitTable))
						{
							sqlFilters.Append(gjahrColumn3.Name + " IN(" + string.Join(",", AllowedGjahr.Select((IOptimization x) => x.Value).ToList()) + ") ");
						}
					}
				}
			}
			else if (tinfo.Columns.Any((IColumn c) => c.OptimizationType == OptimizationType.SplitTable))
			{
				sqlFilters.Append("(");
				foreach (IColumn bukrsColumn in tinfo.Columns.Where((IColumn c) => c.OptimizationType == OptimizationType.SplitTable))
				{
					foreach (string bukrs in (from x in AllowedOptimizationsTemp
						where x.Group.Type == OptimizationType.SplitTable
						select x.Value).ToList())
					{
						sqlFilters.Append("(");
						sqlFilters.Append(bukrsColumn.Name + " = " + bukrs);
						foreach (IColumn gjahrColumn2 in tinfo.Columns.Where((IColumn c) => c.OptimizationType == OptimizationType.SortColumn))
						{
							sqlFilters.Append(" AND " + gjahrColumn2.Name + " IN(" + string.Join(",", (from x in AllowedOptimizationsTemp
								where x != null && x.Value != null && x.Value.ToString() != "" && x.Group != null && x.Group.Type == OptimizationType.SortColumn && x.Parent != null && x.Parent.Group != null && x.Parent.Group.Type == OptimizationType.SplitTable && x.Parent.Value == bukrs
								select x.Value).ToList()) + ") ");
						}
						sqlFilters.Append(") OR ");
					}
				}
				if (sqlFilters.ToString().EndsWith("OR "))
				{
					sqlFilters.Length -= 3;
				}
				sqlFilters.Append(")");
			}
			else
			{
				foreach (IColumn gjahrColumn in tinfo.Columns.Where((IColumn c) => c.OptimizationType == OptimizationType.SortColumn))
				{
					sqlFilters.Append(gjahrColumn.Name + " IN(" + string.Join(",", (from x in AllowedOptimizationsTemp
						where x.Group.Type == OptimizationType.SortColumn
						select x.Value).ToList()) + ") ");
				}
			}
			return sqlFilters.ToString();
		}

		public static void ClearPreviousPageList()
		{
			BsI = 0;
			PrevPageList = new List<BackStack>();
		}

		private static void AddToPrevPageList(BackStack currentState)
		{
			PrevPageList.Add(currentState);
			BsI = PrevPageList.Count - 1;
		}

		public static BackStack GetBackStackItem(int index)
		{
			if (PrevPageList != null && PrevPageList.Count > index)
			{
				return PrevPageList[index];
			}
			return null;
		}

		public static void InsertBackStackItem(int index, BackStack newItem)
		{
			PrevPageList.Insert(index, newItem);
			if (index <= BsI)
			{
				BsI++;
			}
		}

		public static void RemoveBackStackItem(int index)
		{
			PrevPageList.RemoveAt(index);
			if (index <= BsI)
			{
				BsI--;
			}
		}

		public static void SetOptimization(int optid = 0)
		{
			List<IOptimization> list = new List<IOptimization>();
			if (AllowedOptimizations != null && AllowedOptimizations.Count > 0)
			{
				IOptimization opt = AllowedOptimizations[optid];
				if (opt != null)
				{
					list.Add(opt);
				}
				else
				{
					list.Add(AllowedOptimizations.FirstOrDefault());
				}
				while (list[0].Id != 0)
				{
					list.Insert(0, list[0].Parent);
				}
				opt = list.Last();
				while (opt.Children.Count > 0)
				{
					IOptimization oldOpt = null;
					List<IOptimization> current = Optimizations as List<IOptimization>;
					if (current != null && current.Count >= list.Count)
					{
						IOptimization last = current[list.Count - 1];
						IOptimization child = opt.Children.OrderBy((IOptimization o) => o.Value).FirstOrDefault((IOptimization o) => o.Value == last.Value && AllowedOptimizations.Contains(o));
						oldOpt = child ?? opt.Children.OrderBy((IOptimization o) => o.Value).FirstOrDefault((IOptimization o) => AllowedOptimizations.Contains(o));
					}
					else
					{
						oldOpt = opt.Children.OrderBy((IOptimization o) => o.Value).FirstOrDefault((IOptimization o) => AllowedOptimizations.Contains(o));
					}
					opt = ((oldOpt == null) ? opt.Children.OrderBy((IOptimization o) => o.Value).FirstOrDefault() : oldOpt);
					if (AllowedOptimizations.Contains(opt))
					{
						list.Add(opt);
					}
				}
				list.RemoveAt(0);
			}
			else
			{
				UserObjects = ViewboxApplication.Database.SystemDb.GetUserObjects(User);
				if (AllowedOptimizations.Count > 0)
				{
					SetOptimization(optid);
					return;
				}
			}
			Optimizations = list;
			if (optid != 0 && User != null && Optimizations.LastOrDefault() != null)
			{
				ViewboxApplication.Database.SystemDb.UpdateUserOptimization(User, Optimizations.Last());
			}
		}

		public static void ExecuteIssue(global::ViewboxDb.ViewboxDb.ProcedureTransformation func, int id, ViewboxDb.TableObjectCollection tempTableObjects, ITableObjectCollection tableObjects, IOptimization opt, IUser user, object[] parameters)
		{
			ITableObject obj = ((id < 0) ? tempTableObjects[id].Table : tableObjects[id]);
			if (AllowedOpts == null)
			{
				IOptimizationCollection tempCollection = null;
				if (HttpContextFactory.Current == null)
				{
					IUserObjects iUserObjects = ViewboxApplication.Database.SystemDb.GetUserObjects(user);
					tempCollection = iUserObjects.Optimizations;
				}
				else
				{
					tempCollection = AllowedOptimizations;
				}
				string allowedIndex = "";
				string allowedSplit = "";
				string allowedSort = "";
				foreach (IOptimization item in tempCollection)
				{
					if (item.Value != null)
					{
						if (item.Group.Type == OptimizationType.IndexTable && !allowedIndex.Contains(item.Value))
						{
							allowedIndex = allowedIndex + item.Value + ",";
						}
						else if (item.Group.Type == OptimizationType.SplitTable && !allowedSplit.Contains(item.Value))
						{
							allowedSplit = allowedSplit + item.Value + ",";
						}
						else if (item.Group.Type == OptimizationType.SortColumn && item.Parent != null && item.Parent.Group.Type == OptimizationType.SplitTable && !allowedSort.Contains(item.Parent.Value + ":" + item.Value))
						{
							allowedSort = allowedSort + item.Parent.Value + ":" + item.Value + ",";
						}
					}
				}
				AllowedOpts = new List<Tuple<OptimizationType, string>>();
				if (allowedIndex != "")
				{
					AllowedOpts.Add(Tuple.Create(OptimizationType.IndexTable, allowedIndex));
				}
				if (allowedSplit != "")
				{
					AllowedOpts.Add(Tuple.Create(OptimizationType.SplitTable, allowedSplit));
				}
				if (allowedSort != "")
				{
					AllowedOpts.Add(Tuple.Create(OptimizationType.SortColumn, allowedSort));
				}
			}
			if (ViewboxApplication.UseNewIssueMethod)
			{
				ViewboxApplication.Database.ExecuteIssue(func, obj as IIssue, opt, user, parameters, AllowedOpts);
			}
			else
			{
				ViewboxApplication.Database.ExecuteIssueOld(func, obj as IIssue, opt, user, parameters);
			}
		}

		public static ViewboxDb.TableObject CreateTableObject(TempDb.ReaderTransformation func, int id, ViewboxDb.TableObjectCollection tempTableObjects, ITableObjectCollection tableObjects, IFullColumnCollection columns, IOptimization opt, IUser user, CancellationToken token, SortCollection sortList = null, IFilter filter = null, object[] paramValues = null, object[] displayValues = null, List<int> itemId = null, List<int> selectionTypes = null, string filterIssue = null, string joinKey = null, bool joinSort = false, bool joinFilter = false, bool joinIssue = false, bool onlyCount = false, string preloadText = null, long tempTableSize = 0L, List<int> summedColumns = null, bool originalColumnIds = true, SubTotalParameters groupSubTotal = null, bool multiOptimization = false, IDictionary<int, Tuple<int, int>> optimizationSelected = null)
		{
			ITableObject obj = ((id < 0) ? tempTableObjects[id].Table : tableObjects[id]);
			ITableObject original = ((id < 0) ? tempTableObjects[id].OriginalTable : obj);
			bool isNewGroupByOrJoinTable = id < 0 && tempTableObjects[id].IsNewGroupByOrJoinTable;
			List<string> summedColumnNames = null;
			if (summedColumns != null)
			{
				summedColumnNames = (from x in columns
					where summedColumns.Contains(x.Id)
					select x into y
					select y.Name).ToList();
			}
			ViewboxDb.TableObject tobj = ViewboxApplication.Database.TempDatabase.CreateSortedAndFilteredTable(func, obj, opt, token, sortList, filter, paramValues, displayValues, itemId, selectionTypes, filterIssue, joinKey, joinSort, joinFilter, joinIssue, original, user, isNewGroupByOrJoinTable, onlyCount, preloadText, tempTableSize, summedColumnNames, originalColumnIds, groupSubTotal, multiOptimization, optimizationSelected);
			tobj.OptimizationCollection = ViewboxApplication.Database.SystemDb.Optimizations.ToList();
			tempTableObjects.Add(tobj);
			ViewboxApplication.Database.SystemDb.AddColumnToCollection(columns, tobj.Table.Columns);
			return tobj;
		}

		public static ViewboxDb.TableObject CreateTableObject(TempDb.ReaderTransformation func, int id, ViewboxDb.TableObjectCollection tempTableObjects, ITableObjectCollection tableObjects, IFullColumnCollection columns, IOptimization opt, CancellationToken token, List<int> colIds, AggregationCollection aggs, IFilter filter, bool save, string tableName, IUser user, Dictionary<Tuple<ILanguage, string>, string> aggDescriptions, string filterIssue)
		{
			ITableObject obj = ((id < 0) ? tempTableObjects[id].Table : tableObjects[id]);
			Dictionary<string, string> descriptions = new Dictionary<string, string>();
			foreach (ILanguage i in ViewboxApplication.Languages)
			{
				if (save)
				{
					descriptions[i.CountryCode] = tableName;
					continue;
				}
				CultureInfo culture = new CultureInfo(i.CountryCode);
				descriptions[i.CountryCode] = string.Format("{0} {1}", Resources.ResourceManager.GetString("GroupedTable", culture), obj.GetDescription(i));
			}
			ViewboxDb.TableObject tobj = ViewboxApplication.Database.LoadDataTable(func, obj, opt, colIds, aggs, filter, descriptions, ViewboxApplication.Languages, token, save, user, aggDescriptions, filterIssue);
			tempTableObjects.Add(tobj);
			ViewboxApplication.Database.SystemDb.AddColumnToCollection(columns, tobj.Table.Columns);
			return tobj;
		}

		public static ViewboxDb.TableObject CreateTableObject(TempDb.ReaderTransformation func, int id, ViewboxDb.TableObjectCollection tempTableObjects, ITableObjectCollection tableObjects, IFullColumnCollection columns, IOptimization opt, CancellationToken token, List<int> colIds, List<AggregationFunction> aggfuncs, IFilter filter, bool save, string tableName, IUser user, Dictionary<Tuple<ILanguage, string>, string> aggDescriptions, string filterIssue)
		{
			ITableObject tobj = ((id < 0) ? tempTableObjects[id].Table : tableObjects[id]);
			AggregationCollection aggs = new AggregationCollection();
			if (aggfuncs != null)
			{
				foreach (AggregationFunction aggItem in aggfuncs)
				{
					foreach (IColumn col in tobj.Columns)
					{
						if ((aggItem != AggregationFunction.GroupBy && aggItem != 0 && (aggItem == AggregationFunction.Min || aggItem == AggregationFunction.Max || aggItem == AggregationFunction.Count || ((col.DataType == SqlType.Decimal) | (col.DataType == SqlType.Integer)))) || (aggItem == AggregationFunction.Sum && col.DataType == SqlType.Decimal && !col.IsEmpty && (colIds == null || (colIds != null && !colIds.Contains(col.Id)))))
						{
							aggs.Add(new Aggregation
							{
								cid = col.Id,
								agg = aggItem
							});
						}
					}
				}
			}
			return CreateTableObject(func, id, tempTableObjects, tableObjects, columns, opt, token, colIds, aggs, filter, save, tableName, user, aggDescriptions, filterIssue);
		}

		public static ViewboxDb.TableObject CreateTableObject(TempDb.ReaderTransformation func, ViewboxDb.TableObjectCollection tempTableObjects, ITableObjectCollection tableObjects, IFullColumnCollection columns, IOptimization opt, CancellationToken token, int table1Id, int table2Id, JoinColumnsCollection joinColumns, IFilter filter1, IFilter filter2, JoinType type, IUser user, bool saveJoin = false, string tableName = "", string userName = null)
		{
			string startedByUser = $"(started by: {user.UserName})";
			try
			{
				ITableObject obj = ((table1Id < 0) ? tempTableObjects[table1Id].Table : tableObjects[table1Id]);
				Join join = new Join(obj, tableObjects[table2Id], joinColumns, filter1, filter2, type);
				Dictionary<string, string> descriptions = new Dictionary<string, string>();
				foreach (ILanguage i in ViewboxApplication.Languages)
				{
					if (saveJoin)
					{
						descriptions[i.CountryCode] = tableName;
						continue;
					}
					CultureInfo culture = new CultureInfo(i.CountryCode);
					descriptions[i.CountryCode] = string.Format("{0} {1} {2}", join.Table1.GetDescription(i), Resources.ResourceManager.GetString("JoinedWith", culture), join.Table2.GetDescription(i));
				}
				ViewboxDb.TableObject tobj = ViewboxApplication.Database.TempDatabase.CreateJoinTable(func, join, opt, descriptions, ViewboxApplication.Languages, token, user, saveJoin, ViewboxApplication.Database);
				tempTableObjects.Add(tobj);
				ViewboxApplication.Database.SystemDb.AddColumnToCollection(columns, tobj.Table.Columns);
				if (saveJoin)
				{
					ViewboxApplication.Database.SystemDb.AddColumnToCollection(ViewboxApplication.Database.SystemDb.Columns, tobj.Table.Columns);
				}
				return tobj;
			}
			catch (Exception e)
			{
				LogHelper.GetLogger().Error($"[FUNCTIONS] - There was a problem on creating temptable object for Join(joining table {table1Id} and {table2Id}){startedByUser}", e);
			}
			return null;
		}

		public static DataTable LoadDataTable(ViewboxDb.TableObject tinfo, IOptimization opt, long start = 0L, int size = 0, PageSystem pageSystem = null)
		{
			try
			{
				if (pageSystem != null)
				{
					pageSystem.Start = start + 1;
				}
				return ViewboxApplication.Database.TempDatabase.GetData(tinfo, opt, start, size, User, pageSystem, GetRoleBasedOptimizationFilter(tinfo.Table));
			}
			catch
			{
				return null;
			}
		}

		public static DataTable LoadDataTable(ITableObject table, long start = 0L, int size = 0)
		{
			try
			{
				return ViewboxApplication.Database.TempDatabase.LoadDataTable(table, Optimizations.LastOrDefault(), start, size, User, GetRoleBasedOptimizationFilter(table));
			}
			catch
			{
				return null;
			}
		}

		public static DataTable LoadDataTableMin(ITableObject table, string database)
		{
			return ViewboxApplication.Database.TempDatabase.LoadDataTableMin(table, database);
		}

		public static DataTable LoadDataTableMin(string tableName, string userDatabase, string defaultDatabase)
		{
			DataTable table = null;
			try
			{
				table = ViewboxApplication.Database.TempDatabase.LoadDataTableMin(tableName, userDatabase);
			}
			catch (TableNotExistsException)
			{
				try
				{
					table = ViewboxApplication.Database.TempDatabase.LoadDataTableMin(tableName, defaultDatabase);
				}
				catch (TableNotExistsException)
				{
				}
			}
			return table;
		}

		public static Dictionary<string, decimal> SummarizeColumns(string tableName, IList<string> columnNames)
		{
			return ViewboxApplication.Database.TempDatabase.SummarizeColumns(tableName, columnNames);
		}

		public static long GetDataCount(ITableObject table, int originalId = -1337, bool MultiOptimizations = false, IDictionary<int, Tuple<int, int>> OptimizationSelected = null, bool writeToDatabase = true)
		{
			if (originalId == -1337)
			{
				if (table.Id > 0)
				{
					originalId = table.Id;
				}
				else
				{
					ITableObject TblObj = ViewboxApplication.Database.SystemDb.Objects.FirstOrDefault((ITableObject Obj) => Obj.Database == table.Database && Obj.TableName == table.TableName);
					if (TblObj != null)
					{
						originalId = TblObj.Id;
					}
				}
			}
			if (TableObjects[originalId] == null)
			{
				AddTableObject(originalId);
			}
			ITableObject baseObject = TableObjects[originalId].GetBaseObject();
			if (baseObject.Columns.Count == 0 && baseObject.RoleBasedFilters != null)
			{
				IEnumerable<KeyValuePair<int, List<Tuple<int, List<string>>>>> roleBasedFilters = baseObject.RoleBasedFilters.Where((KeyValuePair<int, List<Tuple<int, List<string>>>> rbf) => User.Roles.Any((IRole r) => r.Id == rbf.Key) && rbf.Value.Count > 0);
				if (roleBasedFilters.Count() != 0)
				{
					SetupTableColumns(baseObject.Id);
				}
			}
			if (table.RoleBasedOptimization == null || table.RoleBasedOptimization == "")
			{
				table.RoleBasedOptimization = GetRoleBasedOptimizationFilter(table);
			}
			return ViewboxApplication.Database.SystemDb.GetDataCount(table, Optimizations.LastOrDefault(), baseObject, User, MultiOptimizations, OptimizationSelected, writeToDatabase);
		}

		public static long GetSubtotalDataCount(string sql)
		{
			string tempSQL = $"SELECT COUNT(*) FROM ({sql}) AS tableCount";
			return ViewboxApplication.Database.SystemDb.GetSubtotalDataCount(tempSQL);
		}

		public static IColumnCollection GetColumnList(ITableObject table, int tableId)
		{
			return ViewboxApplication.Database.SystemDb.GetColumnList(table, Optimizations.LastOrDefault(), tableId, User);
		}

		public static IFilter GetFilter(int tableId, string toParse, ViewboxDb.TableObjectCollection tempTableObjects, ITableObjectCollection tableObjects, bool decode = false, bool forcedForCol = false, IFullColumnCollection cols = null)
		{
			ITableObject tobj = ((tableId < 0) ? tempTableObjects[tableId].Table : tableObjects[tableId]);
			ColumnCollection filterCols = new ColumnCollection();
			if (forcedForCol && cols != null)
			{
				StringBuilder sb = new StringBuilder(toParse);
				Filter.CutFilterAndOuterBrackets(sb, toParse.IndexOf("("));
				string[] innerFilters = sb.ToString().Split(new string[1] { ")," }, StringSplitOptions.RemoveEmptyEntries);
				string[] array = innerFilters;
				foreach (string iFilter in array)
				{
					List<string> parameters = Filter.ParseParameters(new StringBuilder(iFilter));
					parameters.RemoveAll((string x) => string.IsNullOrWhiteSpace(x));
					if (parameters.Count <= 0)
					{
						continue;
					}
					int colParamId = (parameters[0].StartsWith("Between") ? 1 : 0);
					if (parameters != null && parameters.Count > 1)
					{
						int indexOfPercentSign = parameters[colParamId].IndexOf("%");
						if (indexOfPercentSign > -1)
						{
							parameters[colParamId] = parameters[colParamId].Substring(indexOfPercentSign);
						}
						if (parameters[colParamId].StartsWith("%") && parameters[colParamId].Length > 1 && int.TryParse(parameters[colParamId].Substring(1), out var colId))
						{
							filterCols.Add(cols[colId]);
						}
					}
				}
			}
			return ViewboxApplication.Database.TempDatabase.GetFilter(tobj, toParse, decode, filterCols);
		}

		public static void LoadExportData(global::ViewboxDb.ViewboxDb.ReaderExport func, ExportType type, ITableObjectCollection objects, CancellationToken token, IOptimization opt, ILanguage language, IUser user, string exportDescription, int page, long size, string fileName, bool getAllFields, string filter = "", IEnumerable<IOptimization> optimization = null)
		{
			List<string> roleBasedOptFilterList = new List<string>();
			foreach (ITableObject item in objects)
			{
				if (item != null)
				{
					roleBasedOptFilterList.Add(GetRoleBasedOptimizationFilter(item, user));
				}
			}
			ViewboxApplication.Database.LoadExportData(func, type, objects, opt, token, language, page, size, user, exportDescription, fileName, getAllFields, filter, roleBasedOptFilterList, optimization);
		}

		public static void LoadExportData(global::ViewboxDb.ViewboxDb.ReaderExport func, ExportType type, ViewboxDb.TableObjectCollection tempTableObjects, CancellationToken token, IOptimization opt, ILanguage language, IUser user, string exportDescription, int page, long size, string fileName, bool getAllFields, IEnumerable<IOptimization> optimization = null)
		{
			List<string> roleBasedOptFilterList = new List<string>();
			foreach (ViewboxDb.TableObject item in tempTableObjects)
			{
				if (item.Table != null)
				{
					roleBasedOptFilterList.Add(GetRoleBasedOptimizationFilter(item.Table, user));
				}
			}
			ViewboxApplication.Database.LoadExportData(func, type, tempTableObjects, opt, token, language, page, size, user, exportDescription, fileName, getAllFields, roleBasedOptFilterList, optimization);
		}

		public static void CreateUser(string userName, string name, SpecialRights flags, ExportRights exportAllowed, string password, string email, int id, bool isAdUser = false, string domain = null, bool firstLogin = false)
		{
			ViewboxApplication.Database.CreateUser(userName, name, flags, exportAllowed, password, email, id, isAdUser, domain, firstLogin);
		}

		public static void DeleteUser(int id)
		{
			ViewboxApplication.Database.SystemDb.RemoveUser(id);
		}

		public static void CreateRole(string name, SpecialRights flags, ExportRights exportAllowed, RoleType type, int id)
		{
			ViewboxApplication.Database.CreateRole(name, flags, exportAllowed, type, id);
		}

		public static void DeleteRole(int id)
		{
			ViewboxApplication.Database.SystemDb.RemoveRole(id);
		}

		public static void AddUserRoleMapping(int user, int role)
		{
			ViewboxApplication.Database.SystemDb.AddUserRoleMapping(user, role);
		}

		public static void RemoveUserRoleMapping(int user, int role)
		{
			ViewboxApplication.Database.SystemDb.RemoveUserRoleMapping(user, role);
		}

		public static void UpdateColumn(int id, bool visible, IUser user = null, bool save = false)
		{
			if (id >= 0)
			{
				if (user == null)
				{
					user = User;
				}
				ViewboxApplication.Database.SystemDb.UpdateColumn(user, id, visible, save);
			}
		}

		public static void EnableRightsMode(int id, CredentialType type)
		{
			RightsModeCredential = new Credential
			{
				Id = id,
				Type = type
			};
			RightsModeObjects = ((type == CredentialType.User) ? ViewboxApplication.Database.SystemDb.GetUserObjects(ViewboxApplication.Users[id]) : ViewboxApplication.Database.SystemDb.GetRoleObjects(ViewboxApplication.Roles[id]));
			if (type == CredentialType.User)
			{
				RightsModeObjects.RightObjectTree.ClearRightTree();
				RightsModeObjects.RightObjectTree.FillRightTree(ViewboxApplication.Users[id], ViewboxApplication.Database.SystemDb);
			}
			if (UserObjects.RightObjectTree.Nodes.Count == 0)
			{
				UserObjects.RightObjectTree.FillRightTree(User, ViewboxApplication.Database.SystemDb);
			}
		}

		public static void DisableRightsMode()
		{
			RightsModeCredential = null;
			RightsModeObjects = null;
			UserObjects.RightObjectTree.ClearRightTree();
		}

		public static bool IsInRightsModeObjects(int id, UpdateRightType type)
		{
			return type switch
			{
				UpdateRightType.Optimization => RightsModeObjects.Optimizations[id] != null, 
				UpdateRightType.Category => RightsModeObjects.Categories[id] != null, 
				UpdateRightType.TableObject => RightsModeObjects.TableObjects[id] != null, 
				UpdateRightType.Column => RightsModeObjects.Columns[id] != null, 
				_ => throw new ArgumentException("type"), 
			};
		}

		public static string GetRightInfo(int id, UpdateRightType type, bool isAllowed = false)
		{
			string output = "";
			RightObjectNode rightNode = RightsModeObjects.RightObjectTree.Nodes[id, type];
			if (RightsModeCredential.Type == CredentialType.User)
			{
				foreach (Tuple<ICredential, RightType> h2 in rightNode.RightHierarchy)
				{
					output += string.Format(Resources.RightHierarchy + "\n", (h2.Item1.Type == CredentialType.User) ? Resources.UserRight : Resources.RoleRight, (h2.Item1.Type == CredentialType.User) ? Resources.User : Resources.Role, h2.Item1.Name, h2.Item2.ToString());
				}
			}
			else
			{
				foreach (Tuple<ICredential, RightType> h in rightNode.RightHierarchy)
				{
					output += string.Format(Resources.RightHierarchy + "\n", Resources.RoleRight, Resources.Role, h.Item1.Name, h.Item2.ToString());
				}
			}
			RightType right = (isAllowed ? RightType.Read : RightType.None);
			output += "=> ";
			switch (type)
			{
			case UpdateRightType.Optimization:
				output += Resources.OptimizationRight;
				break;
			case UpdateRightType.Category:
				output += Resources.CategoryRight;
				break;
			case UpdateRightType.TableObject:
				output += Resources.TableObjectRight;
				break;
			case UpdateRightType.Column:
				output += Resources.ColumnRight;
				break;
			}
			output = output + ": " + right;
			if (right != rightNode.Right)
			{
				output = ((rightNode.Parent == null || TableObjects[rightNode.Parent.Id] == null) ? (output + " " + Resources.NoSuperiorObject) : (output + string.Format(" " + Resources.InheritedFrom, rightNode.Parent.GetNodeText())));
			}
			return output + "\n";
		}

		public static void UpdateGroupRight(int id = 0, UpdateRightType type = UpdateRightType.Optimization, RightType right = RightType.Read, bool enableRightsMode = true)
		{
			if (type != UpdateRightType.Optimization)
			{
				return;
			}
			foreach (IOptimization child in UserObjects.Optimizations[id].Children)
			{
				UpdateRight(child.Id, type, right, enableRightsMode);
			}
		}

		public static void UpdateRight(int id = 0, UpdateRightType type = UpdateRightType.Optimization, RightType right = RightType.Read, bool enableRightsMode = true)
		{
			ICredential credential = null;
			if (RightsModeCredential.Type == CredentialType.User)
			{
				IUser user = ViewboxApplication.Database.SystemDb.Users[RightsModeCredential.Id];
				if (User.CanGrant(user))
				{
					credential = ViewboxApplication.Database.SystemDb.Users[RightsModeCredential.Id];
				}
				ViewboxApplication.UserSessions[user].MarkAll();
			}
			else
			{
				IRole role = ViewboxApplication.Database.SystemDb.Roles[RightsModeCredential.Id];
				if (User.CanGrant(role))
				{
					credential = ViewboxApplication.Database.SystemDb.Roles[RightsModeCredential.Id];
				}
				foreach (IUser user2 in role.Users)
				{
					ViewboxApplication.UserSessions[user2].MarkAll();
				}
			}
			if (credential == null)
			{
				return;
			}
			switch (type)
			{
			case UpdateRightType.Optimization:
			{
				Action<IOptimization> cleanupForChildren = null;
				cleanupForChildren = delegate(IOptimization o)
				{
					foreach (IOptimization current in o.Children.Where((IOptimization c) => c.IsAllowedInRightsMode()))
					{
						ViewboxApplication.Database.SystemDb.UpdateRight(credential, type, current.Id, RightType.Inherit);
						cleanupForChildren(current);
					}
				};
				IOptimization opt;
				if (right > RightType.None)
				{
					opt = UserObjects.Optimizations[id];
				}
				else
				{
					opt = RightsModeObjects.Optimizations[id];
					if (opt == null || !opt.IsAllowedInRightsMode())
					{
						return;
					}
				}
				RightType formerCredentialRight = RightsModeObjects.RightObjectTree.Nodes[id, type].GetRight(RightsModeObjects.Optimizations[id] != null);
				ViewboxApplication.Database.SystemDb.UpdateRight(credential, type, id, right);
				cleanupForChildren(opt);
				if (right != RightType.Inherit && formerCredentialRight != right)
				{
					UpdateRightSubTree(credential, RightsModeObjects.RightObjectTree.Nodes[id, type], type, formerCredentialRight, right);
				}
				if (credential.Type == CredentialType.User)
				{
					ViewboxApplication.Database.SystemDb.GetOptimizations(credential as IUser);
				}
				else
				{
					ViewboxApplication.Database.SystemDb.GetOptimizations(credential as IRole);
				}
				break;
			}
			case UpdateRightType.Category:
			case UpdateRightType.TableObject:
			case UpdateRightType.Column:
			{
				if (right <= RightType.None && ((type == UpdateRightType.Category && RightsModeObjects.Categories[id] == null) || (type == UpdateRightType.TableObject && !ViewboxApplication.Database.SystemDb.GetUserRightsToTable(id, User.Id)) || (type == UpdateRightType.Column && !ViewboxApplication.Database.SystemDb.GetUserRightsToColumn(User.Id, id))))
				{
					return;
				}
				RightType formerCredentialRight2 = RightsModeObjects.RightObjectTree.Nodes[id, type].GetRight(RightsModeObjects.Optimizations[id] != null);
				ViewboxApplication.Database.SystemDb.UpdateRight(credential, type, id, right);
				if (right != RightType.Inherit && formerCredentialRight2 != right)
				{
					UpdateRightSubTree(credential, RightsModeObjects.RightObjectTree.Nodes[id, type], type, formerCredentialRight2, right);
				}
				break;
			}
			}
			if (enableRightsMode)
			{
				EnableRightsMode(RightsModeCredential.Id, RightsModeCredential.Type);
			}
		}

		public static bool IsTableTypeAllowedInRightsMode(TableType type)
		{
			if (RightsModeCredential.Type == CredentialType.User)
			{
				return ViewboxApplication.Database.SystemDb.GetUserRightToTableType(type, SelectedSystem, RightsModeCredential.Id);
			}
			if (RightsModeCredential.Type == CredentialType.Role)
			{
				return ViewboxApplication.Database.SystemDb.GetRoleRightToTableType(type, SelectedSystem, RightsModeCredential.Id);
			}
			throw new ArgumentException("Unknown credential type: " + RightsModeCredential.Type);
		}

		public static bool IsRoleAllowedToSeeTheTableType(TableType type)
		{
			if (!User.IsSuper)
			{
				bool isSuper = false;
				List<bool> isAllowed = new List<bool>();
				foreach (IRole r in User.Roles)
				{
					if (!r.IsSuper)
					{
						isAllowed.Add(ViewboxApplication.Database.SystemDb.GetRoleRightToTableType(type, SelectedSystem, r.Id) ? true : false);
						continue;
					}
					isSuper = true;
					break;
				}
				if (isSuper)
				{
					return true;
				}
				return isAllowed.Any((bool b) => b);
			}
			return true;
		}

		public static string IsRoleSettingAllowedInRightsMode(RoleSettingsType type)
		{
			if (RightsModeCredential.Type == CredentialType.Role)
			{
				return ViewboxApplication.Database.SystemDb.GetRoleSettingOnSpecificType(type, RightsModeCredential.Id).Value;
			}
			throw new ArgumentException("Unknown credential type: " + RightsModeCredential.Type);
		}

		public static bool IsRoleAllowedByRoleSetting(RoleSettingsType type)
		{
			if (!User.IsSuper)
			{
				bool isSuper = false;
				List<bool> isAllowed = new List<bool>();
				foreach (IRole r in User.Roles)
				{
					if (!r.IsSuper)
					{
						IRoleSetting setting = ViewboxApplication.Database.SystemDb.GetRoleSettingOnSpecificType(type, r.Id);
						isAllowed.Add(setting != null && Convert.ToInt32(setting.Value) > 0);
						continue;
					}
					isSuper = true;
					break;
				}
				if (isSuper)
				{
					return true;
				}
				return isAllowed.Any((bool b) => b);
			}
			return true;
		}

		public static bool IsRoleSettingFunctionTabAllowedInRightsMode()
		{
			if (RightsModeCredential.Type == CredentialType.Role)
			{
				IRoleSetting setting = ViewboxApplication.Database.SystemDb.GetRoleSettingOnSpecificType(RoleSettingsType.FunctionTab, RightsModeCredential.Id);
				if (setting == null)
				{
					return false;
				}
				return Convert.ToInt32(setting.Value) > 0;
			}
			throw new ArgumentException("Unknown credential type: " + RightsModeCredential.Type);
		}

		public static void UpdateTableObjRightJob(int id, UpdateRightType type, RightType right, CredentialType rightsModeCredentialType, int rightsModeCredentialId, IUser grantUser)
		{
			ICredential credential = null;
			if (rightsModeCredentialType == CredentialType.User)
			{
				IUser user2 = ViewboxApplication.Database.SystemDb.Users[rightsModeCredentialId];
				if (grantUser.CanGrant(user2))
				{
					credential = ViewboxApplication.Database.SystemDb.Users[rightsModeCredentialId];
				}
				ViewboxApplication.UserSessions[user2].MarkAll();
			}
			else
			{
				IRole role = ViewboxApplication.Database.SystemDb.Roles[rightsModeCredentialId];
				if (grantUser.CanGrant(role))
				{
					credential = ViewboxApplication.Database.SystemDb.Roles[rightsModeCredentialId];
				}
				foreach (IUser user in role.Users)
				{
					ViewboxApplication.UserSessions[user].MarkAll();
				}
			}
			ViewboxApplication.Database.SystemDb.UpdateRight(credential, type, id, right);
		}

		private static void UpdateRightSubTree(ICredential credential, RightObjectNode node, UpdateRightType type, RightType formerRight, RightType setRight)
		{
			if (node.Right != RightType.Inherit)
			{
				return;
			}
			if ((type == UpdateRightType.Optimization && UserObjects.Optimizations[node.Id] != null) || (type == UpdateRightType.Category && UserObjects.Categories[node.Id] != null) || (type == UpdateRightType.TableObject && ViewboxApplication.Database.SystemDb.GetUserRightsToTable(node.Id, User.Id)) || (type == UpdateRightType.Column && ViewboxApplication.Database.SystemDb.GetUserRightsToColumn(User.Id, node)))
			{
				foreach (RightObjectNode childNode in node.Children)
				{
					UpdateRightSubTree(credential, childNode, childNode.Type, formerRight, setRight);
				}
			}
			else
			{
				ViewboxApplication.Database.SystemDb.UpdateRight(credential, type, node.Id, formerRight);
			}
		}

		public static bool HasOptimizationChanged(this ViewboxDb.TableObject tobj)
		{
			bool sort = tobj.Optimization.FindValue(OptimizationType.SortColumn) != Optimizations.Last().FindValue(OptimizationType.SortColumn);
			bool split = tobj.Optimization.FindValue(OptimizationType.SplitTable) != Optimizations.Last().FindValue(OptimizationType.SplitTable);
			bool index = tobj.Optimization.FindValue(OptimizationType.IndexTable) != Optimizations.Last().FindValue(OptimizationType.IndexTable);
			ITableObject tbl = tobj.OriginalTable;
			if (tbl is IIssue)
			{
				IIssue issue = tbl as IIssue;
				if (issue.IssueType == IssueType.StoredProcedure)
				{
					return (issue.UseIndexValue && index) || (issue.UseSplitValue && split) || (issue.UseSortValue && sort);
				}
				if (issue.IssueType == IssueType.Filter)
				{
					tbl = issue.FilterTableObject;
				}
			}
			return (tbl.IndexTableColumn != null && index) || (tbl.SplitTableColumn != null && split) || (tbl.SortColumn != null && sort);
		}

		public static bool IsContainingSystemUsed(this ITableObject tobj)
		{
			return tobj.Database == Optimizations.Last().FindValue(OptimizationType.System);
		}

		public static bool IsStoredProcedure(int id, ViewboxDb.TableObjectCollection tempTableObjects, ITableObjectCollection tableObjects)
		{
			IIssue issue = ((id < 0) ? (tempTableObjects[id].OriginalTable as IIssue) : (tableObjects[id] as IIssue));
			return issue.IssueType == IssueType.StoredProcedure;
		}

		public static bool IsStoredProcedure(int id)
		{
			IIssue issue = ((id < 0) ? (TempTableObjects[id].OriginalTable as IIssue) : (TableObjects[id] as IIssue));
			return issue != null && issue.IssueType == IssueType.StoredProcedure;
		}

		public static void SaveUser(IUser user)
		{
			ViewboxApplication.Database.SaveUser(user);
		}

		public static void SaveIssue(int id, IFilter filter, DescriptionCollection descriptions, List<bool> freeSeleciton = null)
		{
			ITableObject tobj = ((id < 0) ? TempTableObjects[id].OriginalTable : TableObjects[id]);
			if (tobj.Type == TableType.Issue && ((IIssue)tobj).IssueType == IssueType.Filter)
			{
				tobj = ((IIssue)tobj).FilterTableObject;
			}
			bool isLoggedAsGerman = ((Language.CountryCode != null && Language.CountryCode.Contains("de")) ? true : false);
			int issueId = ViewboxApplication.Database.SaveIssue(tobj, filter, "", ViewboxApplication.Languages, User, descriptions, isLoggedAsGerman, freeSeleciton);
			Issue newIssue = ViewboxApplication.Database.SystemDb.Objects.FirstOrDefault((ITableObject i) => i.Id == issueId) as Issue;
			foreach (IParameter param in newIssue.Parameters)
			{
				IIndexer indexer = new Indexer(param);
				if (!indexer.CheckIfExists() && tobj.RowCount <= ViewboxApplication.SaveFilterIndexLimit)
				{
					indexer.DoIndexing();
				}
			}
			AddIssue(issueId);
			SetupObjects();
			foreach (IUser user in ViewboxApplication.Users.Where((IUser u) => u.IsSuper && u.Id != User.Id))
			{
				ViewboxApplication.UserSessions[user].MarkAll();
			}
		}

		public static void DeleteIssue(int id)
		{
			Issue issue = TableObjects[id] as Issue;
			ViewboxApplication.Database.DeleteIssue(issue, User);
			IUserTableObjectOrderSettings tobjOrder = ViewboxApplication.Database.SystemDb.UserTableObjectOrderSettings[User, TableType.Issue];
			if (tobjOrder != null)
			{
				ViewboxApplication.Database.SystemDb.UpdateTableObjectOrder(User, TableType.Issue, issue, remove: true);
			}
			(TableObjects as SystemDb.Internal.TableObjectCollection)?.RemoveById(id);
			SetupObjects();
			foreach (IUser user in ViewboxApplication.Users.Where((IUser u) => u.IsSuper && u.Id != User.Id))
			{
				IUserTableObjectOrderSettings tableObjectOrder = ViewboxApplication.Database.SystemDb.UserTableObjectOrderSettings[user, TableType.Issue];
				if (tableObjectOrder != null)
				{
					ViewboxApplication.Database.SystemDb.UpdateTableObjectOrder(user, TableType.Issue, issue, remove: true);
				}
				ViewboxApplication.UserSessions[user].MarkAll();
			}
		}

		public static void DeleteView(int id)
		{
			IView view = TableObjects[id] as IView;
			Janitor.RemoveTempTable(view.TableName);
			SetupTableColumns(id);
			ViewboxApplication.Database.DeleteView(view, User);
			IUserTableObjectOrderSettings tobjOrder = ViewboxApplication.Database.SystemDb.UserTableObjectOrderSettings[User, TableType.View];
			if (tobjOrder != null)
			{
				ViewboxApplication.Database.SystemDb.UpdateTableObjectOrder(User, TableType.View, view, remove: true);
			}
			SetupObjects();
			foreach (IUser user in ViewboxApplication.Users.Where((IUser u) => u.IsSuper && u.Id != User.Id))
			{
				IUserTableObjectOrderSettings tableObjectOrder = ViewboxApplication.Database.SystemDb.UserTableObjectOrderSettings[user, TableType.View];
				if (tableObjectOrder != null)
				{
					ViewboxApplication.Database.SystemDb.UpdateTableObjectOrder(user, TableType.View, view, remove: true);
				}
				ViewboxApplication.UserSessions[user].MarkAll();
			}
		}

		public static Transformation CreateTransformation(int id, List<int> colIds, AggregationCollection aggs, IFilter filter, bool saveGroup = false, string tableName = "")
		{
			return Transformation.Create(id, TableObjects, Columns, Optimizations.LastOrDefault(), colIds, aggs, filter, saveGroup, tableName);
		}

		public static Transformation CreateTransformation(int id, List<int> colIds, List<AggregationFunction> aggfunc, IFilter filter, bool saveGroup = false, string tableName = "")
		{
			return Transformation.Create(id, TableObjects, Columns, Optimizations.LastOrDefault(), colIds, aggfunc, filter, saveGroup, tableName);
		}

		public static List<string> GetDataOfMassExport(string exports)
		{
			List<string> result = new List<string>();
			int j = 0;
			for (int i = 0; i < exports.Length; i++)
			{
				if (exports[i] != ']')
				{
					continue;
				}
				string sub = exports.Substring(j, i - j + 1);
				string[] subSplitted = sub.Split(new char[2] { '[', ']' }, sub.Length, StringSplitOptions.RemoveEmptyEntries);
				switch (subSplitted.Length)
				{
				case 1:
				{
					if (int.TryParse(subSplitted[0], out var toInt) && ViewboxApplication.Database.SystemDb.Objects.Contains(toInt))
					{
						result.Add(ViewboxApplication.Database.SystemDb.Objects[toInt].GetDescription());
					}
					break;
				}
				}
				j = i + 2;
			}
			return result;
		}

		public static ITableObjectCollection GetMassExportObjects(string exports)
		{
			string system = Optimizations.FirstOrDefault((IOptimization o) => o.Group.Type == OptimizationType.System)?.Value;
			ITableObjectCollection objects = ViewboxApplication.Database.SystemDb.CreateTableObjectCollection();
			int j = 0;
			for (int i = 0; i < exports.Length; i++)
			{
				if (exports[i] == ']')
				{
					string sub2 = exports.Substring(j, i - j + 1);
					string[] subSplitted2 = sub2.Split(new char[2] { '[', ']' }, sub2.Length, StringSplitOptions.RemoveEmptyEntries);
					switch (subSplitted2.Length)
					{
					case 1:
					{
						if (int.TryParse(subSplitted2[0], out var toInt3) && TableObjects.Contains(toInt3))
						{
							SetupTableColumns(toInt3);
							ITableObject objToAdd2 = TableObjects[toInt3];
							if (system == objToAdd2.Database)
							{
								objects.Add(TableObjects[toInt3]);
							}
						}
						break;
					}
					case 2:
					{
						string[] columns = subSplitted2[1].Split(',');
						if (!int.TryParse(subSplitted2[0], out var toInt2) || !TableObjects.Contains(toInt2))
						{
							break;
						}
						ITableObject tobj = TableObjects[toInt2].Clone() as ITableObject;
						string[] array = columns;
						foreach (string cId in array)
						{
							if (int.TryParse(cId, out var columnId) && TableObjects[toInt2].Columns.Contains(columnId))
							{
								if (TableObjects[toInt2].Columns[columnId].IsVisible)
								{
									tobj.Columns.Add(TableObjects[toInt2].Columns[columnId]);
									continue;
								}
								IColumn col = TableObjects[toInt2].Columns[columnId].Clone() as IColumn;
								col.IsVisible = true;
								tobj.Columns.Add(col);
							}
						}
						ITableObject objToAdd = TableObjects[toInt2];
						if (system == objToAdd.Database)
						{
							objects.Add(tobj);
						}
						break;
					}
					}
					j = i + 2;
				}
				if (exports[i] != '}')
				{
					continue;
				}
				string sub = exports.Substring(j, i - j + 1);
				string[] subSplitted = sub.Split(new char[2] { '{', '}' }, sub.Length, StringSplitOptions.RemoveEmptyEntries);
				int length = subSplitted.Length;
				string[] tableTypes = subSplitted[1].Split(',');
				if (int.TryParse(subSplitted[0], out var toInt))
				{
					string[] array2 = tableTypes;
					foreach (string t in array2)
					{
						if (Enum.TryParse<TableType>(t, out var tableType))
						{
							objects.AddRange(new List<ITableObject>(TableObjects.Where((ITableObject to) => to.Type == tableType && to.Category.Id == toInt && (system == null || to.Database == system))));
						}
					}
				}
				j = i + 2;
			}
			return objects;
		}

		private static void UpdateAction(ViewboxDb.Action a)
		{
		}

		private static void UpdateNewLogAction(NewLogAction a)
		{
		}

		private static void SaveNewLogAction(NewLogAction a)
		{
		}

		private static void PageSizeSearch(ViewboxDb.Action a)
		{
		}

		private static void NewLogPageSizeSearch(NewLogAction a)
		{
		}

		public static void SetAction(string actionName, string controllerName, DateTime timestamp, IDictionary<string, object> actionParameters, bool rightsMode)
		{
		}

		public static void NewLogSetAction(string actionName, string controllerName, DateTime timestamp, IDictionary<string, object> actionParameters, bool rightsMode)
		{
		}

		public static string FormatFilter(string filter)
		{
			string help = string.Empty;
			string result = string.Empty;
			bool isColumnId = false;
			for (int i = 0; i < filter.Length; i++)
			{
				if (filter[i] == '%')
				{
					isColumnId = true;
					continue;
				}
				if (filter[i] == ',')
				{
					if (!isColumnId)
					{
						result += filter[i];
					}
					isColumnId = false;
					continue;
				}
				if (!isColumnId && int.TryParse(help, out var columnId))
				{
					help = "";
					result = result + GetColumnDescription(columnId) + ",";
				}
				if (isColumnId && int.TryParse(filter[i].ToString(), out columnId))
				{
					help += filter[i];
				}
				else
				{
					result += filter[i];
				}
			}
			return result;
		}

		public static string GetColumnDescription(int columnId)
		{
			if (columnId < 0)
			{
				return Resources.TemporaryColumn;
			}
			IColumn col = ViewboxApplication.Database.SystemDb.Columns[columnId];
			if (col == null)
			{
				return "";
			}
			string description = col.Descriptions[Language];
			if (!string.IsNullOrEmpty(description))
			{
				return description;
			}
			return col.Name;
		}

		public static string GetTableDescription(int tableId)
		{
			if (tableId < 0)
			{
				return Resources.TemporaryTable;
			}
			ITableObject table = ViewboxApplication.Database.SystemDb.Objects[tableId];
			if (table == null)
			{
				return "";
			}
			string tableDescription = table.Descriptions[Language];
			if (!string.IsNullOrEmpty(tableDescription))
			{
				return tableDescription;
			}
			return table.TableName;
		}

		public static void RemoveJob(string key)
		{
			Base job = Base.Find(key);
			if (job != null && !job.JobShown.Contains(User))
			{
				job.JobShown.Add(User);
				if (RightsModeCredential != null)
				{
					EnableRightsMode(RightsModeCredential.Id, RightsModeCredential.Type);
				}
			}
		}

		public static string GetJobInfo(string key, bool tableCheck = false)
		{
			StringBuilder output = new StringBuilder();
			try
			{
				Base job = Base.Find(key);
				Transformation transform = job as Transformation;
				Export export = job as Export;
				if (transform != null && transform.TransformationObject != null)
				{
					output.Append(GetTableObjectInfo(transform.TransformationObject));
				}
				if (export != null)
				{
					output.Append(export.Type);
					output.Append("-");
					output.Append(Resources.ExportOf);
					output.Append(":\n");
					if (export.TransformationObjects != null && export.TransformationObjects.Count > 0)
					{
						foreach (ViewboxDb.TableObject t2 in export.TransformationObjects)
						{
							output.Append(GetTableObjectInfo(t2));
						}
					}
					else if (export.ExportObjects != null && Optimizations.LastOrDefault() != null && tableCheck)
					{
						output.Append(GetTableObjectInfo(export.ExportObjects.First(), Optimizations.LastOrDefault()));
					}
					else if (export.TransformationObjects == null || export.TransformationObjects.Count == 0)
					{
						foreach (ITableObject t in export.ExportObjects)
						{
							output.Append(t.GetDescription(Language));
							output.Append("\n");
							int i = 0;
							output.Append(Resources.ChosenOptimizations);
							output.Append(":\n");
							List<Tuple<string, string>> optimizationTexts = Optimizations.LastOrDefault().GetDescriptions(Language);
							optimizationTexts.Reverse();
							foreach (Tuple<string, string> opt in optimizationTexts)
							{
								if (i > 1)
								{
									break;
								}
								output.Append("   ");
								output.Append(opt.Item1.Trim());
								output.Append(": ");
								output.Append(opt.Item2.Trim());
								output.Append("\n");
								i++;
							}
						}
					}
				}
			}
			catch (Exception)
			{
				output.Clear();
				output.Append(Resources.ErrorGettingInfos);
			}
			return output.ToString();
		}

		private static string GetOptimizationInfo(List<Tuple<string, string>> opt, int optHidden = 0, IOrderAreaCollection orderAreas = null)
		{
			StringBuilder output = new StringBuilder();
			output.Append(Resources.ChosenOptimizations);
			output.Append(":\n");
			int i = 0;
			foreach (Tuple<string, string> item in opt)
			{
				bool checkSplit = false;
				IOrderArea orderSplit = orderAreas.FirstOrDefault((IOrderArea x) => !string.IsNullOrWhiteSpace(x.SplitValue));
				if (orderSplit != null && !string.IsNullOrWhiteSpace(orderSplit.SplitValue))
				{
					checkSplit = true;
				}
				bool checkSort = false;
				IOrderArea orderSort = orderAreas.FirstOrDefault((IOrderArea x) => !string.IsNullOrWhiteSpace(x.SortValue));
				if (orderSort != null && !string.IsNullOrWhiteSpace(orderSort.SortValue))
				{
					checkSort = true;
				}
				if ((optHidden == 1 && i >= 2) || (!checkSplit && i == 2) || (!checkSort && i == 3))
				{
					i++;
					continue;
				}
				output.Append("   ");
				output.Append(item.Item1.Trim());
				output.Append(": ");
				output.Append(item.Item2.Trim());
				output.Append("\n");
				i++;
			}
			return output.ToString();
		}

		private static string GetTableObjectInfo(ITableObject tobj, IOptimization opt)
		{
			StringBuilder output = new StringBuilder();
			if (tobj != null)
			{
				output.Append("- ");
				if (tobj.Type == TableType.View)
				{
					output.Append(Resources.View);
				}
				if (tobj.Type == TableType.Table)
				{
					output.Append(Resources.Table);
				}
				if (tobj.Type == TableType.Issue)
				{
					output.Append(Resources.Issue);
				}
				output.Append(": ");
				output.Append(tobj.GetDescription(Language));
				output.Append("\n");
			}
			List<Tuple<string, string>> optimizationTexts = opt.GetDescriptions(Language);
			optimizationTexts.Reverse();
			if (opt != null && tobj.Type != TableType.Table)
			{
				output.Append(GetOptimizationInfo(optimizationTexts, tobj.OptimizationHidden, tobj.OrderAreas));
			}
			return output.ToString();
		}

		private static void BuildSimpleTable(IIssue issue, SimpleTableModel simpleTableModel, object[] paramValues, List<int> itemId, List<int> selectionTypes)
		{
			if (itemId == null)
			{
				itemId = new List<int>();
			}
			if (issue == null)
			{
				return;
			}
			int i = 0;
			bool hasRequired = issue.Parameters.Count((IParameter p) => p.IsRequired == 1) > 0;
			foreach (int id in itemId)
			{
				IParameter param = issue.Parameters.FirstOrDefault((IParameter k) => k.Id == id);
				int ordinal = 0;
				ordinal = ((!hasRequired) ? param.Ordinal : param.OriginalOrdinal);
				if (ordinal > paramValues.Count())
				{
					ordinal = i;
				}
				string value = ((paramValues == null || paramValues[i] == null) ? "" : paramValues[i].ToString());
				DateTime? datValue = ((paramValues == null) ? null : (paramValues[i] as DateTime?));
				if (datValue.HasValue)
				{
					value = datValue.Value.ToString("dd/MM/yyyy");
				}
				simpleTableModel.Parameters.Add(new Tuple<IParameter, string, string>(param, value, ""));
				i++;
			}
			DisplayParameterLoad(simpleTableModel, selectionTypes);
		}

		public static string replaceLanguageItem(string value, ILanguage language)
		{
			if (language.CountryCode.ToLower() == "en")
			{
				return value.Replace(" from", "");
			}
			if (language.CountryCode.ToLower() == "de")
			{
				return value.Replace(" von", "");
			}
			return value;
		}

		private static void DisplayParameterLoad(SimpleTableModel tableModel, List<int> selectionTypes)
		{
			tableModel.DisplayParameters.Clear();
			Tuple<IParameter, string, string> lastParameter = null;
			List<string> incommingParameters = new List<string>();
			string textBufferDescription = string.Empty;
			string textBufferValue = string.Empty;
			string issueExtension = string.Empty;
			int i = 0;
			int j = 0;
			foreach (Tuple<IParameter, string, string> p in tableModel.Parameters)
			{
				int selection = 0;
				try
				{
					selection = ((selectionTypes != null && selectionTypes.Count != 0) ? selectionTypes[j] : 0);
				}
				catch (Exception)
				{
					selection = 0;
				}
				if (p.Item1.GroupId == 0 || selection > 1)
				{
					if (p.Item2 == null || !(p.Item2 != "") || !(p.Item2 != ";"))
					{
						continue;
					}
					string val = p.Item2;
					StringBuilder validValue = new StringBuilder();
					switch (selection)
					{
					case 0:
						tableModel.DisplayParameters.Add(new Tuple<string, string>($"{p.Item1.GetDescription(Language)} {Resources.DisplayEqualsFilter} ", $"{ViewboxApplication.Database.GetFilterIssueValue(p.Item2, p.Item1.DataType)}"));
						break;
					case 1:
						tableModel.DisplayParameters.Add(new Tuple<string, string>($"{p.Item1.GetDescription(Language)} {Resources.DisplayNotEqualsFilter} ", $"{ViewboxApplication.Database.GetFilterIssueValue(p.Item2, p.Item1.DataType)}"));
						break;
					case 2:
					{
						if (val[val.Length - 1] == ';')
						{
							val = val.Remove(val.Length - 1);
						}
						string[] inputs = val.Replace("\\;", "SEMICOLON").Split(';');
						for (int k = 0; k < inputs.Count(); k++)
						{
							inputs[k] = inputs[k].Replace("SEMICOLON", ";");
						}
						string[] array2 = inputs;
						foreach (string inputValue in array2)
						{
							validValue.Append($"{ViewboxApplication.Database.GetFilterIssueValue(inputValue, p.Item1.DataType)},");
						}
						validValue.Length--;
						tableModel.DisplayParameters.Add(new Tuple<string, string>($"{p.Item1.GetDescription(Language)} {Resources.DisplayContainsFilter} ", $"{p.Item2}"));
						if (p.Item1.GroupId != 0)
						{
							j--;
						}
						break;
					}
					case 3:
					{
						if (val[val.Length - 1] == ';')
						{
							val = val.Remove(val.Length - 1);
						}
						string[] inputs = val.Replace("\\;", "SEMICOLON").Split(';');
						for (int l = 0; l < inputs.Count(); l++)
						{
							inputs[l] = inputs[l].Replace("SEMICOLON", ";");
						}
						string[] array = inputs;
						foreach (string inputValue2 in array)
						{
							validValue.Append($"{ViewboxApplication.Database.GetFilterIssueValue(inputValue2, p.Item1.DataType)},");
						}
						validValue.Length--;
						tableModel.DisplayParameters.Add(new Tuple<string, string>($"{p.Item1.GetDescription(Language)} {Resources.DisplayNotContainsFilter} ", $"{p.Item2}"));
						if (p.Item1.GroupId != 0)
						{
							j--;
						}
						break;
					}
					}
					continue;
				}
				if (lastParameter != null && lastParameter.Item1.GroupId == p.Item1.GroupId && !p.Item1.GetDescription(Language).ToLower().Contains("von") && !p.Item1.GetDescription(Language).ToLower().Contains("from"))
				{
					if (p.Item2 != "")
					{
						if (p.Item1.DataType == SqlType.Date && DateTime.TryParse(p.Item2, out var dateOutput2))
						{
							textBufferValue += string.Format("'{0}'", dateOutput2.ToString("dd.MM.yyyy"));
						}
						else
						{
							textBufferValue += ViewboxApplication.Database.GetFilterIssueValue(p.Item2, p.Item1.DataType);
						}
						if (!tableModel.DisplayParameters.Any((Tuple<string, string> dp) => dp.Item1 == textBufferDescription && dp.Item2 == textBufferValue))
						{
							tableModel.DisplayParameters.Add(new Tuple<string, string>(textBufferDescription, textBufferValue));
						}
					}
					else if (lastParameter.Item2 != "")
					{
						switch (selection)
						{
						case 0:
							tableModel.DisplayParameters.Add(new Tuple<string, string>($"{replaceLanguageItem(lastParameter.Item1.GetDescription(Language), Language)} {Resources.DisplayEqualsFilter} ", $"{ViewboxApplication.Database.GetFilterIssueValue(lastParameter.Item2, lastParameter.Item1.DataType)}"));
							break;
						case 1:
							tableModel.DisplayParameters.Add(new Tuple<string, string>($"{replaceLanguageItem(lastParameter.Item1.GetDescription(Language), Language)} {Resources.DisplayNotEqualsFilter} ", $"{ViewboxApplication.Database.GetFilterIssueValue(lastParameter.Item2, lastParameter.Item1.DataType)}"));
							break;
						}
					}
				}
				else
				{
					DateTime dateOutput;
					string tempValue = ((p.Item1.DataType != SqlType.Date || !DateTime.TryParse(p.Item2, out dateOutput)) ? ViewboxApplication.Database.GetFilterIssueValue(p.Item2, p.Item1.DataType) : string.Format("'{0}'", dateOutput.ToString("dd.MM.yyyy")));
					switch (selection)
					{
					case 0:
						textBufferDescription = $"{replaceLanguageItem(p.Item1.GetDescription(Language), Language)} {Resources.DisplayBetweenFilter} ";
						textBufferValue = $"{tempValue} {Resources.And} ";
						break;
					case 1:
						textBufferDescription = $"{replaceLanguageItem(p.Item1.GetDescription(Language), Language)} {Resources.DisplayNotBetweenFilter} ";
						textBufferValue = $"{tempValue} {Resources.And} ";
						break;
					}
					i--;
					j--;
				}
				lastParameter = p;
			}
		}

		public static string GetTableObjectInfo(ViewboxDb.TableObject tobj)
		{
			StringBuilder output = new StringBuilder();
			if (tobj.OriginalTable != null)
			{
				output.Append("  ");
				if (tobj.OriginalTable.Type == TableType.View)
				{
					output.Append(Resources.View);
				}
				if (tobj.OriginalTable.Type == TableType.Table)
				{
					output.Append(Resources.Table);
				}
				if (tobj.OriginalTable.Type == TableType.Issue)
				{
					output.Append(Resources.Issue);
				}
				output.Append(": ");
				output.Append(tobj.OriginalTable.GetDescription(Language).Trim());
				output.Append("\n");
			}
			SimpleTableModel simpleModel = new SimpleTableModel();
			IIssue issue = tobj.OriginalTable as IIssue;
			BuildSimpleTable(issue, simpleModel, tobj.ParamValues, tobj.ItemId, tobj.SelectionTypes);
			if (simpleModel.DisplayParameters != null)
			{
				output.Append(Resources.DisplayParameter);
				output.Append("\n");
				foreach (Tuple<string, string> item in simpleModel.GetParameters(tobj.OriginalTable.Id))
				{
					if (!(item.Item2.TrimStart('\'').TrimEnd('\'').Trim()
						.Replace("'", string.Empty) == string.Empty))
					{
						output.Append(string.Format("   {0}: {1}\n", item.Item1.Trim().Replace("'", string.Empty), item.Item2.TrimStart('\'').TrimEnd('\'').Trim()
							.Replace("'", string.Empty)));
					}
				}
			}
			List<Tuple<string, string>> optimizationTexts = tobj.Optimization.GetDescriptions(Language);
			optimizationTexts.Reverse();
			if (tobj.Optimization != null && tobj.OriginalTable.Type != TableType.Table)
			{
				output.Append(GetOptimizationInfo(optimizationTexts, tobj.Table.OptimizationHidden, tobj.Table.OrderAreas));
			}
			if (tobj.Sort != null && tobj.Sort.Count > 0)
			{
				output.Append("- ");
				output.Append(Resources.SortedBy);
				output.Append(":\n");
				foreach (Sort s in tobj.Sort)
				{
					output.Append("\t\t");
					output.Append(tobj.Table.Columns[s.cid].GetDescription(Language));
					output.Append(" ");
					output.Append((s.dir == SortDirection.Ascending) ? Resources.Ascending : Resources.Descending);
					output.Append("\n");
				}
			}
			if (tobj.Filter != null)
			{
				output.Append("- ");
				output.Append(Resources.FilteredBy);
				output.Append(":\n");
				using (ConnectionManager.CreateConnection(new DbConfig()))
				{
					string originalFilterString = tobj.Filter.ToOriginalString();
					originalFilterString = originalFilterString.Remove(originalFilterString.Length - 1);
					string[] filterParts = originalFilterString.Split(new string[1] { "\")," }, StringSplitOptions.None);
					filterParts[0] = filterParts[0].Substring(filterParts[0].IndexOf('(') + 1);
					StringBuilder filter = new StringBuilder();
					for (int i = 0; i < filterParts.Count(); i++)
					{
						string filterModification = filterParts[i];
						if (!filterModification.Contains("\")"))
						{
							filterModification += "\")";
						}
						IFilter filterObj = Filter.GetFilter(tobj.Table, filterModification, decode: false);
						try
						{
							if (filterObj != null)
							{
								filter.Append(filterObj.GetToolTipText());
							}
						}
						catch (Exception)
						{
							if (filterObj != null)
							{
								filter.Append(filterObj.ToString());
							}
						}
						if (i != filterParts.Count() - 1)
						{
							filter.Append(" AND \n");
						}
						output.Append(filter);
					}
				}
				output.Append("\n");
			}
			if (tobj.Join != null)
			{
				output.Append("- ");
				output.Append(Resources.JoinedWith);
				output.Append(" ");
				output.Append(tobj.Join.Table2.GetDescription(Language));
				output.Append("\n");
			}
			return output.ToString();
		}

		public static string FilterValueToString(IColumn column, object value)
		{
			string output = string.Empty;
			if (column != null && (column.DataType == SqlType.Time || column.DataType == SqlType.Date || column.DataType == SqlType.DateTime))
			{
				if (DateTime.TryParseExact(value.ToString(), "yyyyMMddHHmmss", new CultureInfo(Language.CountryCode), DateTimeStyles.None, out var date))
				{
					if (column.DataType == SqlType.Time)
					{
						output = date.ToString("HH:mm:ss", new CultureInfo(Language.CountryCode));
					}
					if (column.DataType == SqlType.Date)
					{
						output = date.ToShortDateString();
					}
					if (column.DataType == SqlType.DateTime)
					{
						output = date.ToString(new CultureInfo(Language.CountryCode));
					}
				}
				else
				{
					output = value.ToString();
				}
			}
			else
			{
				output = value.ToString();
			}
			return output;
		}

		public static string GetValueFromInfo(string key)
		{
			using DatabaseBase connection = ViewboxApplication.Database.ConnectionManager.GetConnection();
			return Info.GetValue("UpdateMode", connection);
		}

		public static string GetThumbnailPathByPath(ITableObject table, string path)
		{
			try
			{
				return ViewboxApplication.Database.TempDatabase.GetThumbnailPathByPath(table, path);
			}
			catch
			{
				return string.Empty;
			}
		}

		public static string GetReserve(IArchive archive, string filename)
		{
			try
			{
				return ViewboxApplication.Database.TempDatabase.GetReserve(archive, filename);
			}
			catch
			{
				return string.Empty;
			}
		}

		public static string GetPath(IArchive archive, string filter)
		{
			try
			{
				return ViewboxApplication.Database.TempDatabase.GetPath(archive, filter);
			}
			catch
			{
				return string.Empty;
			}
		}

		public static int GetMaxValueOfColumn(IArchive archive, string column, string filter)
		{
			try
			{
				return ViewboxApplication.Database.TempDatabase.GetMaxValueOfColumn(archive, column, filter);
			}
			catch
			{
				return -1;
			}
		}

		public static List<string> GetBelegParts(string dbName, string dokId)
		{
			return ViewboxApplication.Database.TempDatabase.GetBelegPathes(dbName, dokId);
		}

		public static int GetIssueViewTableCount(TableType tableType)
		{
			if (_issueViewTableCount == null)
			{
				_issueViewTableCount = new List<RoleTableObjectRightsCount>();
				if (User.IsSuper)
				{
					IOptimization[] allowedSystem = AllowedSystem;
					foreach (IOptimization system3 in allowedSystem)
					{
						_issueViewTableCount.Add(new RoleTableObjectRightsCount
						{
							SystemOptimization = system3,
							TableObjecType = TableType.Table,
							Count = TableCount
						});
						_issueViewTableCount.Add(new RoleTableObjectRightsCount
						{
							SystemOptimization = system3,
							TableObjecType = TableType.Issue,
							Count = IssueCount
						});
						_issueViewTableCount.Add(new RoleTableObjectRightsCount
						{
							SystemOptimization = system3,
							TableObjecType = TableType.View,
							Count = ViewCount
						});
						_issueViewTableCount.Add(new RoleTableObjectRightsCount
						{
							SystemOptimization = system3,
							TableObjecType = TableType.Archive,
							Count = Archives.Count
						});
					}
				}
				else
				{
					int[,] t = new int[AllowedSystem.Count(), 4];
					List<Tuple<IOptimization, int>> systemTempId = new List<Tuple<IOptimization, int>>();
					int nr = 0;
					IOptimization[] allowedSystem2 = AllowedSystem;
					foreach (IOptimization system2 in allowedSystem2)
					{
						systemTempId.Add(new Tuple<IOptimization, int>(system2, nr));
						nr++;
					}
					for (int i = 0; i < AllowedSystem.Count(); i++)
					{
						for (int j = 0; j < 4; j++)
						{
							t[i, j] = 0;
						}
					}
					List<Tuple<IRole, ITableObject, RightType>> roleTableObjectRights = ViewboxApplication.Database.SystemDb.RoleTableObjectRights.Where((Tuple<IRole, ITableObject, RightType> x) => x.Item3 != 0 && User.Roles.Any((IRole y) => y.Id == x.Item1.Id)).ToList();
					foreach (Tuple<IRole, ITableObject, RightType> item in roleTableObjectRights)
					{
						Tuple<IOptimization, int> isEnabled = systemTempId.FirstOrDefault((Tuple<IOptimization, int> x) => x.Item1.FindValue(OptimizationType.System).ToLower() == item.Item2.Database.ToLower());
						if (isEnabled == null)
						{
							continue;
						}
						switch (item.Item2.Type)
						{
						case TableType.Table:
							t[systemTempId.First((Tuple<IOptimization, int> x) => x.Item1.FindValue(OptimizationType.System).ToLower() == item.Item2.Database.ToLower()).Item2, 0]++;
							break;
						case TableType.Issue:
							t[systemTempId.First((Tuple<IOptimization, int> x) => x.Item1.FindValue(OptimizationType.System).ToLower() == item.Item2.Database.ToLower()).Item2, 1]++;
							break;
						case TableType.View:
							t[systemTempId.First((Tuple<IOptimization, int> x) => x.Item1.FindValue(OptimizationType.System).ToLower() == item.Item2.Database.ToLower()).Item2, 2]++;
							break;
						case TableType.Archive:
							t[systemTempId.First((Tuple<IOptimization, int> x) => x.Item1.FindValue(OptimizationType.System).ToLower() == item.Item2.Database.ToLower()).Item2, 3]++;
							break;
						}
					}
					IOptimization[] allowedSystem3 = AllowedSystem;
					foreach (IOptimization system in allowedSystem3)
					{
						_issueViewTableCount.Add(new RoleTableObjectRightsCount
						{
							SystemOptimization = system,
							TableObjecType = TableType.Table,
							Count = t[systemTempId.First((Tuple<IOptimization, int> x) => x.Item1.FindValue(OptimizationType.System).ToLower() == system.FindValue(OptimizationType.System).ToLower()).Item2, 0]
						});
						_issueViewTableCount.Add(new RoleTableObjectRightsCount
						{
							SystemOptimization = system,
							TableObjecType = TableType.Issue,
							Count = t[systemTempId.First((Tuple<IOptimization, int> x) => x.Item1.FindValue(OptimizationType.System).ToLower() == system.FindValue(OptimizationType.System).ToLower()).Item2, 1]
						});
						_issueViewTableCount.Add(new RoleTableObjectRightsCount
						{
							SystemOptimization = system,
							TableObjecType = TableType.View,
							Count = t[systemTempId.First((Tuple<IOptimization, int> x) => x.Item1.FindValue(OptimizationType.System).ToLower() == system.FindValue(OptimizationType.System).ToLower()).Item2, 2]
						});
						_issueViewTableCount.Add(new RoleTableObjectRightsCount
						{
							SystemOptimization = system,
							TableObjecType = TableType.Archive,
							Count = t[systemTempId.First((Tuple<IOptimization, int> x) => x.Item1.FindValue(OptimizationType.System).ToLower() == system.FindValue(OptimizationType.System).ToLower()).Item2, 3]
						});
					}
				}
			}
			if (Optimizations.Count() > 0)
			{
				try
				{
					IOptimization lastSystemOpt = Optimizations.Last().GetOptimization(OptimizationType.System);
					return _issueViewTableCount.Where((RoleTableObjectRightsCount x) => x.SystemOptimization == lastSystemOpt && x.TableObjecType == tableType).FirstOrDefault()?.Count ?? 0;
				}
				catch
				{
					return 0;
				}
			}
			return 0;
		}
	}
}
