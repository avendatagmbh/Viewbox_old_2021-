using System;
using System.ComponentModel;
using ViewBuilderBusiness.MetadataUpdate;

namespace ViewBuilder.Models.MetadataUpdate
{
	internal class ColumnDifferenceModel : INotifyPropertyChanged, ISelectable, IComparable<ColumnDifferenceModel>
	{
		private readonly ColumnDifference _columnDifference;
		private bool _isChecked;

		public ColumnDifferenceModel(ColumnDifference columnDifference)
		{
			_columnDifference = columnDifference;
		}

		public string TableName
		{
			get { return _columnDifference.TableName; }
		}

		public string ColumnName
		{
			get { return _columnDifference.ColumnName; }
		}

		public string Problem
		{
			get
			{
				string message;
				switch (_columnDifference.DifferenceType)
				{
					case ColumnDifference.Type.MissingColumn:
						message = string.Format("Missing column: there is no such column in the customer database!");
						break;
					case ColumnDifference.Type.MultipleColumns:
						message =
							string.Format("Multiple columns: there are more than one columns with this name in the customer database!");
						break;
					case ColumnDifference.Type.DataTypeCollisionText:
						message = GetCollisionMessage("Text collision");
						break;
					case ColumnDifference.Type.DataTypeCollisionBinary:
						message = GetCollisionMessage("Binary collision");
						break;
					case ColumnDifference.Type.DataTypeCollisionDouble:
						message = GetCollisionMessage("Double collision");
						break;
					case ColumnDifference.Type.DataTypeCollision:
						message = GetCollisionMessage("Other datatype collision");
						break;
					case ColumnDifference.Type.NoDifference:
						message = string.Format("No difference");
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				return message;
			}
		}

		private string GetCollisionMessage(string prefix)
		{
			try
			{
				return string.Format("{0}: metadata type {1} against {2}",
									 prefix, _columnDifference.MetadataColumn.DataType, _columnDifference.CustomerColumn.OriginalType);
			}
			catch
			{
				return prefix;
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
			get
			{
				return 
					_columnDifference.DifferenceType == ColumnDifference.Type.DataTypeCollisionBinary ||
					_columnDifference.DifferenceType == ColumnDifference.Type.DataTypeCollisionDouble;
			}
		}

		public string ToolTip
		{
			get { return IsEnabled ? "Select column to resolve!" : "Only binary and double collisions can be solved!"; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public int CompareTo(ColumnDifferenceModel other)
		{
			return string.Compare(ColumnName, other.ColumnName, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}