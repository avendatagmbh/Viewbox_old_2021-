using System;
using System.ComponentModel;
using ViewBuilderBusiness.MetadataUpdate;

namespace ViewBuilder.Models.MetadataUpdate
{
	internal class TableDifferenceModel : INotifyPropertyChanged, ISelectable, IComparable<TableDifferenceModel>
	{
		private readonly TableDifference _tableDifference;
		private bool _isChecked;

		public TableDifferenceModel(TableDifference tableDifference)
		{
			_tableDifference = tableDifference;
		}

		public string TableName
		{
			get { return _tableDifference.TableName; }
		}

		public string Problem
		{
			get
			{
				string message;
				switch (_tableDifference.DifferenceType)
				{
					case TableDifference.Type.MissingTable:
						message = "Missing table";
						break;
					case TableDifference.Type.ColumnDifferences:
						message = "Column differences";
						break;
					case TableDifference.Type.NoDifferences:
						message = "No difference";
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				return message;
			}
		}

		public bool IsChecked
		{
			get { return _isChecked; }
			set
			{
				if (_isChecked != value)
				{
					_isChecked = value;
					if (_isChecked && !IsEnabled)
					{
						_isChecked = false;
					}
					OnPropertyChanged("IsChecked");
				}				
			}
		}

		public bool IsEnabled
		{
			get { return _tableDifference.DifferenceType != TableDifference.Type.MissingTable; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public string ToolTip
		{
			get { return IsEnabled ? "Select table to resolve!" : "Missing table cannot be fixed!"; }
		}

		public int CompareTo(TableDifferenceModel other)
		{
			return string.Compare(TableName, other.TableName, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}