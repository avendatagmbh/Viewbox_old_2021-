using System;
using System.Collections.Generic;
using System.Linq;
using SystemDb;
using AV.Log;
using log4net;
using ViewboxDb;

namespace Viewbox.Models
{
	public class ValueListModels : ViewboxModel
	{
		private const int MaxCachedTempTableValues = 10;

		private static readonly Dictionary<int, TableObject> TempTables = new Dictionary<int, TableObject>();

		private static readonly Dictionary<int, ValueListCollectionCache> CachedTempTableValues = new Dictionary<int, ValueListCollectionCache>();

		private readonly ILog _log = LogHelper.GetLogger();

		private readonly IOptimization _optimization;

		private readonly int _parameterId;

		private int _columnId;

		private int _hash;

		private TableObject _tableObject;

		private int RealRowCount;

		public ValueListCollectionCache ValueListCollectionCache { get; set; }

		public ValueListCollection ValueListCollection { get; set; }

		public int RowsPerPage { get; internal set; }

		public int Page { get; internal set; }

		public IIssue Issue { get; set; }

		public int Uid { get; set; }

		public bool HasDescription { get; set; }

		public int Limit { get; set; }

		public int RowsCount => RealRowCount;

		public int FromRow => RowsPerPage * Page;

		public int ToRow => Math.Min(RowsCount, FromRow + RowsPerPage);

		public int PageCount => (RealRowCount - 1) / RowsPerPage;

		public bool ShowEmpty { get; internal set; }

		public SortDirection Direction { get; internal set; }

		public string Search { get; internal set; }

		public bool ExactMatch { get; internal set; }

		public IParameter Parameter { get; internal set; }

		public override string LabelCaption => "";

		public ValueListModels(int parameterId, int columnId, IOptimization optimization, int uid, int id = -1, string search = null, bool exactMatch = false, bool showEmpty = false, int sortColumn = 0, SortDirection direction = SortDirection.Ascending, bool json = false, int page = 0, int size = 25, bool isPreview = false, string[] selectedParamValues = null, int limit = 150)
		{
			_parameterId = parameterId;
			_columnId = columnId;
			_optimization = optimization;
			RowsPerPage = size;
			Page = page;
			ShowEmpty = showEmpty;
			Direction = direction;
			Search = search;
			ExactMatch = exactMatch;
			Uid = uid;
			foreach (IIssue issue2 in ViewboxApplication.Database.SystemDb.Issues)
			{
				if (issue2.Parameters.Contains(_parameterId))
				{
					Issue = issue2;
					Parameter = issue2.Parameters[parameterId];
					break;
				}
			}
			if (Issue != null)
			{
				GetCachedPage(selectedParamValues, limit);
				return;
			}
			foreach (IIssue issue in ViewboxSession.Issues)
			{
				if (issue.Parameters.Contains(_parameterId))
				{
					Issue = issue;
					Parameter = issue.Parameters[parameterId];
					ViewboxApplication.Database.SystemDb.Issues.AddToIssueCollection(Issue);
					break;
				}
			}
			GetCachedPage(selectedParamValues, limit);
		}

		private void GetCachedPage(string[] selectedParamValues = null, int limit = 150)
		{
			ValueListCollection = null;
			bool hasDescription = false;
			try
			{
				IParameter parameter = ViewboxApplication.Database.SystemDb.Parameters[_parameterId];
				string cname = parameter.ColumnName;
				IColumn column = ViewboxSession.Columns.FirstOrDefault((IColumn x) => x.Name == parameter.ColumnName && x.Table != null && x.Table.Database == parameter.DatabaseName && x.Table.Name == parameter.TableName);
				IColumn column_temp = parameter.Issue.Columns.FirstOrDefault((IColumn c) => c.Name == parameter.ColumnName);
				int columnId = column?.Id ?? (-1);
				int tableId = column?.Table.Id ?? (-1);
				SortDirection direction2 = Direction;
				string userID = base.User.Id.ToString();
				string roleID = "";
				foreach (IRole item in base.User.Roles)
				{
					roleID = roleID + item.Id + ", ";
				}
				string allowedIndex = "";
				string allowedSplit = "";
				string allowedSort = "";
				if (!base.User.IsSuper)
				{
					allowedIndex = ViewboxSession.AllowedIndexString;
					allowedSplit = ViewboxSession.AllowedSplitString;
					allowedSort = ViewboxSession.AllowedSortString;
				}
				ValueListCollection = ValueListCollectionCache.GetRealPage(Issue, _parameterId, out RealRowCount, out hasDescription, _optimization, ViewboxApplication.Database.TempDatabase, ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, Page * RowsPerPage, RowsPerPage, Uid, userID, roleID, allowedIndex, allowedSplit, allowedSort, ViewboxSession.Language.CountryCode, Search, ExactMatch, columnId, tableId, direction2, selectedParamValues, oldSmallBooks: true, ViewboxApplication.WertehilfeLimit);
				HasDescription = hasDescription;
			}
			catch (Exception)
			{
				SortDirection direction = Direction;
				ValueListCollection = ValueListCollectionCache.GetRealPage(Issue, _parameterId, out RealRowCount, out hasDescription, _optimization, ViewboxApplication.Database.TempDatabase, ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, Page * RowsPerPage, RowsPerPage, Uid, "", "", "", "", "", ViewboxSession.Language.CountryCode, Search, ExactMatch, -1, -1, direction, selectedParamValues, oldSmallBooks: true, ViewboxApplication.WertehilfeLimit);
				HasDescription = hasDescription;
			}
		}
	}
}
