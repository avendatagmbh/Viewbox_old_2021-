using System.Collections.Generic;
using System.Linq;
using ViewBuilderBusiness.MetadataUpdate;
using System.Windows;
using System;
using ViewBuilderBusiness.Structures.Config;
using ViewBuilder.Windows;
using Utils;
using System.ComponentModel;

namespace ViewBuilder.Models.MetadataUpdate
{
	class DlgDifferencesModel
	{
		#region Fields
		private readonly MetadataUpdater _metadataUpdater;
		private readonly IList<TableDifference> _tableDifferences;		
		#endregion

		#region Events and triggers
		public event EventHandler<ResolvingTableEventArgs> ResolvingTable;
		public event EventHandler<ResolvingTablesEventArgs> ResolvingTables;
		public event EventHandler<System.EventArgs> ResolvingTablesComplete;		
		public event EventHandler<ResolvingTableEventArgs> ResolvingTableComplete;
		public event EventHandler<ResolvingColumnEventArgs> ResolvingColumn;
		public event EventHandler<ResolvingColumnsEventArgs> ResolvingColumnsComplete;
		public event EventHandler<ResolvingColumnEventArgs> ResolvingColumnComplete;
		public event EventHandler<ErrorEventArgs> ErrorOccured;
		

		private void OnErrorOccured(string message)
		{
			if (ErrorOccured != null) ErrorOccured(this, new ErrorEventArgs(message)); 
		}

		private void OnResolvingColumn(string tableName, string columnName)
		{
			if (ResolvingColumn != null) ResolvingColumn(this, new ResolvingColumnEventArgs(tableName, columnName));
		}

		private void OnResolvingColumnsComplete(bool allColumnsWereResolved, string tableName)
		{
			if (ResolvingColumnsComplete != null) ResolvingColumnsComplete(this,
				new ResolvingColumnsEventArgs(allColumnsWereResolved, tableName));
		}

		private void OnResolvingColumnComplete(string tableName, string columnName, string error)
		{
			if (ResolvingColumnComplete != null) ResolvingColumnComplete(this, 
				new ResolvingColumnEventArgs(tableName, columnName, error));
		}

		private void OnResolvingTable(string tableName)
		{
			if (ResolvingTable != null) ResolvingTable(this, new ResolvingTableEventArgs(tableName));
		}

		private void OnResolvingTables(int numberOfTables)
		{
			if (ResolvingTables != null) ResolvingTables(this, new ResolvingTablesEventArgs(numberOfTables));
		}

		private void OnResolvingTablesComplete()
		{
			if (ResolvingTablesComplete != null) ResolvingTablesComplete(this, System.EventArgs.Empty);
		}

		private void OnResolvingTableComplete(string tableName)
		{ 
			if(ResolvingTableComplete != null) ResolvingTableComplete(this, 
				new ResolvingTableEventArgs(tableName));
		}
		#endregion

		#region Ctor
		public DlgDifferencesModel(ProfileConfig profileConfig, IList<TableDifference> tableDifferences)
		{			
			_tableDifferences = tableDifferences;
			_metadataUpdater = new MetadataUpdater(profileConfig);
			_metadataUpdater.ResolvingTableComplete += MetadataUpdaterOnResolvingTableComplete;
			_metadataUpdater.ResolvingColumnComplete += MetadataUpdaterOnResolvingColumnComplete;
			_metadataUpdater.ResolvingTable += MetadataUpdaterOnResolvingTable;
			_metadataUpdater.ResolvingColumn += MetadataUpdaterOnResolvingColumn;
		}				
		#endregion

		#region Model event handlers
		void MetadataUpdaterOnResolvingColumnComplete(object sender, ResolvingColumnEventArgs e)
		{
			OnResolvingColumnComplete(e.TableName, e.ColumnName, e.Error);
		}

		void MetadataUpdaterOnResolvingTableComplete(object sender, ResolvingTableEventArgs e)
		{
			OnResolvingTableComplete(e.TableName);
		}

		void MetadataUpdaterOnResolvingColumn(object sender, ResolvingColumnEventArgs e)
		{
			OnResolvingColumn(e.TableName, e.ColumnName);
		}

		void MetadataUpdaterOnResolvingTable(object sender, ResolvingTableEventArgs e)
		{
			OnResolvingTable(e.TableName);
		}
		#endregion

		internal void ResolveTableDifferences(List<TableDifferenceModel> selectedTables)
		{
			if (selectedTables == null || selectedTables.Count == 0)
			{
				OnErrorOccured("Select tables to resolve!");
				return;
			}

			var selectedTableNames = selectedTables.Select(t => t.TableName).ToList();

			var missingTables = _tableDifferences.Where(t =>
									t.DifferenceType == TableDifference.Type.MissingTable &&
									selectedTableNames.Contains(t.TableName)).ToList();

			var tablesToUpdate = _tableDifferences.Where(t =>
									t.DifferenceType == TableDifference.Type.ColumnDifferences &&
									selectedTableNames.Contains(t.TableName)).ToList();

			if (missingTables.Count > 0)
			{
				OnErrorOccured("Missing tables cannot be fixed!");
			}

			OnResolvingTables(tablesToUpdate.Count);
			var resolvedTables = _metadataUpdater.ResolveTableDifferences(tablesToUpdate);
			foreach (var resolvedTable in resolvedTables)
				_tableDifferences.Remove(resolvedTable);

			OnResolvingTablesComplete();
		}
		
		internal void ResolveColumnDifferences(IList<ColumnDifferenceModel> selectedColumns)
		{

			if (selectedColumns == null || selectedColumns.Count == 0)
			{
				OnErrorOccured("Select columns to resolve!");
				return;
			}

			var tableName = selectedColumns.Count > 0 ? selectedColumns[0].TableName : string.Empty;
			var tableDiff = _tableDifferences.First(t => t.TableName == tableName);
			var selectedColumnNames = selectedColumns.Select(c => c.ColumnName).ToList();
			var columnsToUpdate = tableDiff.ColumnDifferences.Where(c => selectedColumnNames.Contains(c.ColumnName)).ToList();

			var resolvedColumns = _metadataUpdater.ResolveColumnDifferences(columnsToUpdate);

			foreach (var resolvedColumn in resolvedColumns)
				tableDiff.ColumnDifferences.Remove(resolvedColumn);

			if (tableDiff.ColumnDifferences.Count == 0)
				_tableDifferences.Remove(tableDiff);

			OnResolvingColumnsComplete(!tableDiff.ColumnDifferences.Any(), tableName);			
		}

		internal IList<TableDifferenceModel> GetTables()
		{
			var tables = _tableDifferences.Select(d => new TableDifferenceModel(d)).ToList();
			tables.Sort();
			return tables;
		}

		internal IList<ISelectable> GetDetails(string tableName)
		{
			var items = new List<ISelectable>();
			var tableDiff = _tableDifferences.First(diff => diff.TableName == tableName);
			if (tableDiff.DifferenceType == TableDifference.Type.MissingTable)
			{
				items.Add(new TableDifferenceModel(tableDiff));
			}
			else
			{
				var columnDiffs = tableDiff.ColumnDifferences.Select(c => new ColumnDifferenceModel(c)).ToList();
				columnDiffs.Sort();
				items.AddRange(columnDiffs);
			}
			return items;
		}		
	}
}
