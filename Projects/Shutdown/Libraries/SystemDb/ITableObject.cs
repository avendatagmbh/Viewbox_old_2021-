using System;
using System.Collections.Generic;
using System.ComponentModel;
using SystemDb.Internal;
using DbAccess.Enums;

namespace SystemDb
{
	public interface ITableObject : IDataObject, INotifyPropertyChanged, ICloneable
	{
		bool IsVisible { get; set; }

		bool IsArchived { get; set; }

		bool IsUnderArchiving { get; set; }

		bool IsFavorite { get; }

		long RowCount { get; }

		PageSystem PageSystem { get; }

		int ColumnCount { get; }

		string Database { get; }

		string TableName { get; }

		List<RoleArea> RoleAreas { get; }

		IColumn SortColumn { get; }

		IColumn IndexTableColumn { get; }

		IColumn SplitTableColumn { get; }

		string OptimizationFilter { get; set; }

		ICategory Category { get; }

		new TableType Type { get; }

		string TransactionNumber { get; set; }

		bool IsNewGroupByOrJoinTable { get; }

		IColumnCollection Columns { get; }

		IRelationCollection Relations { get; }

		IOrderAreaCollection OrderAreas { get; }

		IScheme DefaultScheme { get; }

		ISchemeCollection Schemes { get; }

		IIndexCollection Indexes { get; }

		IExtendedColumnInformationCollection ExtendedColumnInformations { get; }

		int PreviousId { get; set; }

		IDictionary<int, List<Tuple<int, List<string>>>> RoleBasedFilters { get; set; }

		IDictionary<int, List<Tuple<string, string>>> RoleBasedFiltersNew { get; set; }

		string RoleBasedOptimization { get; set; }

		IEnumerable<RowVisibilityCountCache> RoleBasedFilterRowCount { get; set; }

		int OptimizationHidden { get; set; }

		int ObjectTypeCode { get; set; }

		EngineTypes EngineType { get; set; }

		ITableObject GetBaseObject();

		string GetRoleBasedFilterSQL(IUser user);
	}
}
