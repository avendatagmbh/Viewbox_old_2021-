using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("parameter", ForceInnoDb = true)]
	[DebuggerDisplay("Parameter: {Name}")]
	public class Parameter : DataObject, IParameter, IDataObject, INotifyPropertyChanged
	{
		private Issue _issue;

		private OptimizationType _optimizationType;

		private OptimizationDirection _optimizationDirection;

		[DbColumn("issue_id")]
		public int IssueId { get; set; }

		public IIssue Issue
		{
			get
			{
				return _issue;
			}
			set
			{
				if (_issue != value)
				{
					_issue = value as Issue;
					IssueId = Issue.Id;
					NotifyPropertyChanged("Issue");
				}
			}
		}

		public int OriginalOrdinal { get; set; }

		[DbColumn("data_type")]
		public SqlType DataType { get; set; }

		[DbColumn("type_modifier")]
		public int TypeModifier { get; set; }

		[DbColumn("default")]
		public string Default { get; set; }

		[DbColumn("column_name", Length = 256)]
		public string ColumnName { get; set; }

		[DbColumn("table_name", Length = 256)]
		public string TableName { get; set; }

		[DbColumn("database_name")]
		public string DatabaseName { get; set; }

		[DbColumn("is_required")]
		public int IsRequired { get; set; }

		[DbColumn("optionally_required")]
		public int OptionallyRequired { get; set; }

		[DbColumn("group_id")]
		public int GroupId { get; set; }

		[DbColumn("freeselection")]
		public int FreeSelection { get; set; }

		[DbColumn("leading_zeros")]
		public int LeadingZeros { get; set; }

		[DbColumn("column_name_in_view")]
		public string ColumnNameInView { get; set; }

		[DbColumn("description_column_name")]
		public string DescriptionColumnName { get; set; }

		[DbColumn("use_absolute")]
		public int UseAbsolute { get; set; }

		public bool HasIndexData { get; set; }

		public bool HasIndexError { get; set; }

		public bool IndexDataGenerated { get; set; }

		public string IndexErrorStyleString { get; set; }

		public IEnumerable<IParameterValue> Values { get; set; }

		public IHistoryParameterValueCollection HistoryValues { get; set; }

		[DbColumn("optimization_type", AllowDbNull = true)]
		public OptimizationType OptimizationType
		{
			get
			{
				return _optimizationType;
			}
			set
			{
				if (_optimizationType != value)
				{
					_optimizationType = value;
					NotifyPropertyChanged("OptimizationGroupId");
				}
			}
		}

		[DbColumn("optimization_direction", AllowDbNull = true)]
		public OptimizationDirection OptimizationDirection
		{
			get
			{
				return _optimizationDirection;
			}
			set
			{
				if (_optimizationDirection != value)
				{
					_optimizationDirection = value;
				}
			}
		}

		public List<IHistoryParameterValueFreeSelection> HistoryFreeSelectionValues { get; set; }

		public Parameter()
			: base(DataObjectType.Parameter)
		{
			HasIndexData = true;
			IndexDataGenerated = false;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}
