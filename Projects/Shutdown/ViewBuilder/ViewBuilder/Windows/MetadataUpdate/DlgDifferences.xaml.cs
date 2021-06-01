using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ViewBuilder.Models.MetadataUpdate;
using ViewBuilderBusiness.MetadataUpdate;
using ViewBuilderBusiness.Structures.Config;
using Utils;
using System.ComponentModel;

namespace ViewBuilder.Windows.MetadataUpdate
{
	/// <summary>
	/// Interaction logic for DlgDifferences.xaml
	/// </summary>
	public partial class DlgDifferences : DlgWindowBase
	{
		#region Fields
		private readonly DlgDifferencesModel _model;
		private PopupProgressBar _popupTableProgressBar;
		private PopupProgressBar _popupColumnProgressBar;
		private ProgressCalculator _progressTableCalculator;
		private ProgressCalculator _progressColumnCalculator;
		#endregion

		#region Ctor
		public DlgDifferences(ProfileConfig profileConfig, IList<TableDifference> differences)
		{
			InitializeComponent();
			_model = new DlgDifferencesModel(profileConfig, differences);
			
			_model.ResolvingColumnsComplete += ModelOnResolvingColumnsComplete;
			_model.ResolvingColumnComplete += ModelOnResolvingColumnComplete;
			_model.ResolvingColumn += ModelOnResolvingColumn;
			_model.ResolvingTables += ModelOnResolvingTables;
			_model.ResolvingTable += ModelOnResolvingTable;
			_model.ResolvingTablesComplete += ModelOnResolvingTablesComplete;
			_model.ResolvingTableComplete += ModelOnResolvingTableComplete;
			
			_model.ErrorOccured += ModelOnErrorOccured;
		}			
		#endregion		

		#region Model event handlers
		void ModelOnResolvingColumn(object sender, ResolvingColumnEventArgs e)
		{
			if (_progressColumnCalculator != null)
				_progressColumnCalculator.Description = string.Format("Resolving column {0}", e.ColumnName);

			if (_progressTableCalculator != null)
				_progressTableCalculator.Description = string.Format("Resolving table {0} column {1}", e.TableName, e.ColumnName);
		}

		void ModelOnResolvingTable(object sender, ResolvingTableEventArgs e)
		{
			if (_progressTableCalculator != null)
				_progressTableCalculator.Description = string.Format("Resolving table {0}", e.TableName);
		}	

		void ModelOnResolvingTables(object sender, ResolvingTablesEventArgs e)
		{
			_progressTableCalculator.SetWorkSteps(e.NumberOfTables, false);
		}

		void ModelOnResolvingColumnsComplete(object sender, ResolvingColumnsEventArgs e)
		{
			//MessageBox.Show("Resolving columns complete!", "Complete", MessageBoxButton.OK, MessageBoxImage.Information);
			
			if (e.AllColumnsWereResolved)
			{
				ListViewTableDifferences.ItemsSource = _model.GetTables();
				ListViewDetails.ItemsSource = null;
			}
			else
			{
				ListViewDetails.ItemsSource = _model.GetDetails(e.TableName);
			}
			CheckBoxSelectAllDetails.IsChecked = false;
		}

		void ModelOnResolvingColumnComplete(object sender, ResolvingColumnEventArgs e)
		{
			if (_progressColumnCalculator != null) _progressColumnCalculator.StepDone();
			if (e.HasError) MessageBox.Show(string.Format("Resolving column failed! {0}", e.Error), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		void ModelOnResolvingTablesComplete(object sender, EventArgs e)
		{
			//MessageBox.Show("Resolving tables complete!", "Complete", MessageBoxButton.OK, MessageBoxImage.Information);

			ListViewTableDifferences.ItemsSource = _model.GetTables();
			ListViewDetails.ItemsSource = null;
			CheckBoxSelectAllTableDifferences.IsChecked = false;
		}

		void ModelOnResolvingTableComplete(object sender, ResolvingTableEventArgs e)
		{
			if (_progressTableCalculator != null) _progressTableCalculator.StepDone();
		}

		void ModelOnErrorOccured(object sender, ErrorEventArgs e)
		{
			MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}
		#endregion

		#region UI event handlers
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				ListViewTableDifferences.ItemsSource = _model.GetTables();
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void ListViewTableDifferences_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				CheckBoxSelectAllDetails.IsChecked = false;	
				var selectedItem = ((TableDifferenceModel) ((ListView) sender).SelectedItem);
				ListViewDetails.ItemsSource = selectedItem != null ? _model.GetDetails(selectedItem.TableName) : null;
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}


		private void CheckBoxSelectAllTableDifferences_OnChecked(object sender, RoutedEventArgs e)
		{
			foreach (var item in ListViewTableDifferences.Items)
			{
				((ISelectable)item).IsChecked = true;
			}
		}

		private void CheckBoxSelectAllTableDifferences_OnUnchecked(object sender, RoutedEventArgs e)
		{
			foreach (var item in ListViewTableDifferences.Items)
			{
				((ISelectable)item).IsChecked = false;
			}
		}

		private void CheckBoxSelectTableDifference_OnClick(object sender, RoutedEventArgs e)
		{
			CheckBoxSelectAllTableDifferences.IsChecked =
				AllElementsAreChecked(ListViewTableDifferences.Items.OfType<ISelectable>());
		}


		private void CheckBoxSelectAllDetails_OnChecked(object sender, RoutedEventArgs e)
		{
			foreach (var item in ListViewDetails.Items)
			{
				((ISelectable)item).IsChecked = true;
			}
		}

		private void CheckBoxSelectAllDetails_OnUnchecked(object sender, RoutedEventArgs e)
		{
			foreach (var item in ListViewDetails.Items)
			{
				((ISelectable)item).IsChecked = false;
			}
		}		

		private void CheckBoxSelectDetails_OnClick(object sender, RoutedEventArgs e)
		{
			CheckBoxSelectAllDetails.IsChecked =
				AllElementsAreChecked(ListViewDetails.Items.OfType<ISelectable>());
		}


		private bool? AllElementsAreChecked(IEnumerable<ISelectable> items)
		{
			var selectables = items as IList<ISelectable> ?? items.ToList();
			if (selectables.All(item => item.IsChecked))
			{
				return true;
			}
			if (selectables.All(item => !item.IsChecked))
			{
				return false;
			}
			return null;
		}


		private void ButtonResolveTableDiff_OnClick(object sender, RoutedEventArgs e)
		{
			_progressColumnCalculator = null;
			_popupColumnProgressBar = null;

			_progressTableCalculator = new ProgressCalculator();
			_progressTableCalculator.DoWork += ProgressTableCalculatorDoWork;
			_progressTableCalculator.RunWorkerCompleted += ProgressTableCalculatorRunWorkerCompleted;
			_popupTableProgressBar = new PopupProgressBar() { DataContext = _progressTableCalculator, Owner = this };

			_progressTableCalculator.RunWorkerAsync();
			_popupTableProgressBar.ShowDialog();			
		}

		private void ButtonResolveColumnDiff_OnClick(object sender, RoutedEventArgs e)
		{
			_progressTableCalculator = null;
			_popupTableProgressBar = null;

			_progressColumnCalculator = new ProgressCalculator();
			_progressColumnCalculator.DoWork += ProgressColumnCalculatorOnDoWork;
			_progressColumnCalculator.RunWorkerCompleted += ProgressColumnCalculatorOnRunWorkerCompleted;
			_popupColumnProgressBar = new PopupProgressBar() { DataContext = _progressColumnCalculator, Owner = this };

			_progressColumnCalculator.RunWorkerAsync();
			_popupColumnProgressBar.ShowDialog();
		}		
		#endregion

		#region ProgressCalculator event handlers
		void ProgressTableCalculatorDoWork(object sender, DoWorkEventArgs e)
		{
			var selectedTables = ListViewTableDifferences.Items.OfType<TableDifferenceModel>()
														 .Where(t => t.IsChecked).ToList();
			if (selectedTables.Count > 0)
			{								
				_model.ResolveTableDifferences(selectedTables);
			}
			else
			{
				_progressTableCalculator.Description = "There was no selection!";
				_progressTableCalculator.SetWorkSteps(1, false);
				_progressTableCalculator.StepDone();
			}
		}

		void ProgressTableCalculatorRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			_popupTableProgressBar.Close();			
		}

		void ProgressColumnCalculatorOnDoWork(object sender, DoWorkEventArgs e)
		{
			var selectedColumns = ListViewDetails.Items.OfType<ColumnDifferenceModel>()
														.Where(t => t.IsChecked).ToList();
			if (selectedColumns.Count > 0)
			{
				_progressColumnCalculator.SetWorkSteps(selectedColumns.Count, false);
				_model.ResolveColumnDifferences(selectedColumns);
			}
			else
			{
				_progressColumnCalculator.Description = "There was no selection!";
				_progressColumnCalculator.SetWorkSteps(1, false);
				_progressColumnCalculator.StepDone();
			}
		}

		void ProgressColumnCalculatorOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			_popupColumnProgressBar.Close();
		}
		#endregion
	}
}
