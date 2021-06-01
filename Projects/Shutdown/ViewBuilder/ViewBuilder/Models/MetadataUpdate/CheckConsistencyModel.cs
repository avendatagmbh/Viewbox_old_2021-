using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Utils;
using ViewBuilder.Windows;
using ViewBuilder.Windows.MetadataUpdate;
using ViewBuilderBusiness.MetadataUpdate;
using ViewBuilderBusiness.Structures.Config;

namespace ViewBuilder.Models.MetadataUpdate
{
	class CheckConsistencyModel
	{
		private readonly PopupProgressBar _popupProgressBar;
		private readonly ProgressCalculator _progressCalculator;
		private readonly Window _ownerWindow;
		private readonly MetadataConsistencyInspector _metadataConsistencyInspector;
		private readonly ProfileConfig _profileConfig;
		private IList<TableDifference> _differences;

		public CheckConsistencyModel(ProfileConfig profileConfig, Window ownerWindow)
		{
			_ownerWindow = ownerWindow;
			_profileConfig = profileConfig;

			_progressCalculator = new ProgressCalculator();
			_progressCalculator.DoWork += ProgressOnDoWork;
			_progressCalculator.RunWorkerCompleted += ProgressOnRunWorkerCompleted;

			_popupProgressBar = new PopupProgressBar()
				{
					DataContext = _progressCalculator, 
					Owner = _ownerWindow					
				};

			_metadataConsistencyInspector = new MetadataConsistencyInspector(profileConfig);
			_metadataConsistencyInspector.Initializing += MetadataConsistencyInspectorOnInitializing;
			_metadataConsistencyInspector.InitializationComplete += MetadataConsistencyInspectorOnInitializationComplete;
			_metadataConsistencyInspector.CollectMissingTables += MetadataConsistencyInspectorOnCollectMissingTables;
			_metadataConsistencyInspector.CollectMissingTablesComplete += MetadataConsistencyInspectorOnCollectMissingTablesComplete;
			_metadataConsistencyInspector.InspectingTables += MetadataConsistencyInspectorOnInspectingTables;
			_metadataConsistencyInspector.InspectingTable += MetadataConsistencyInspectorOnInspectingTable;
			_metadataConsistencyInspector.InspectingTablesComplete += MetadataConsistencyInspectorOnInspectingTablesComplete;
			_metadataConsistencyInspector.CompareComplete += MetadataConsistencyInspectorOnCompareComplete;
		}		

		public void CheckConsistency()
		{
			_progressCalculator.RunWorkerAsync();
			_popupProgressBar.ShowDialog();
		}

		private void ProgressOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
		{
			_popupProgressBar.Close();
			if (runWorkerCompletedEventArgs.Error != null)
			{
				MessageBox.Show(_popupProgressBar,
				                runWorkerCompletedEventArgs.Error.Message, 
								"Error", 
								MessageBoxButton.OK, MessageBoxImage.Error);
			}
			else
			{
				var dlgDifferences = new DlgDifferences(_profileConfig, _differences ?? new List<TableDifference>());
				dlgDifferences.ShowDialog();				
			}
		}

		private void ProgressOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
		{			
			_progressCalculator.Title = "Checking database";

			//_differences = new List<TableDifference>
			//    {
			//        new TableDifference("001", TableDifference.Type.MissingTable),
			//        new TableDifference("002", TableDifference.Type.MissingTable)
			//    };

			//var diff1 = new TableDifference("003", TableDifference.Type.ColumnDifferences);
			//diff1.ColumnDifferences.Add(new ColumnDifference("003", "Column 1", ColumnDifference.Type.DataTypeCollisionBinary));
			//diff1.ColumnDifferences.Add(new ColumnDifference("003", "Column 2", ColumnDifference.Type.MissingColumn));
			//diff1.ColumnDifferences.Add(new ColumnDifference("003", "Column 3", ColumnDifference.Type.DataTypeCollisionText));

			//_differences.Add(diff1);



			_differences = _metadataConsistencyInspector.CompareTableInformation();			
		}		
			
		private void MetadataConsistencyInspectorOnInitializing(object sender, EventArgs eventArgs)
		{
			_progressCalculator.Description = "Initializing...";
		}

		private void MetadataConsistencyInspectorOnInitializationComplete(object sender, EventArgs eventArgs)
		{
			_progressCalculator.Description = "Initialization complete!";
		}

		private void MetadataConsistencyInspectorOnCollectMissingTables(object sender, EventArgs eventArgs)
		{
			_progressCalculator.Description = "Collecting missing tables...";
		}		

		private void MetadataConsistencyInspectorOnCollectMissingTablesComplete(object sender, EventArgs eventArgs)
		{
			_progressCalculator.Description = "Collecting missing tables complete!";
		}

		private void MetadataConsistencyInspectorOnInspectingTables(object sender, InspectTablesEventArgs inspectTablesEventArgs)
		{
			_progressCalculator.Description = "Inspecting tables...";
			_progressCalculator.SetWorkSteps(inspectTablesEventArgs.NumberOfTables + 1, false);
		}

		private void MetadataConsistencyInspectorOnInspectingTable(object sender, InspectTableEventArgs inspectTableEventArgs)
		{
			_progressCalculator.Description = string.Format("Inspecting table: {0}", inspectTableEventArgs.TableName);
			_progressCalculator.StepDone();
		}

		private void MetadataConsistencyInspectorOnInspectingTablesComplete(object sender, EventArgs eventArgs)
		{
			_progressCalculator.Description = "Inspecting tables complete!";
		}

		private void MetadataConsistencyInspectorOnCompareComplete(object sender, EventArgs eventArgs)
		{
			_progressCalculator.Description = "Complete!";
			_progressCalculator.StepDone();
		}		
	}
}
