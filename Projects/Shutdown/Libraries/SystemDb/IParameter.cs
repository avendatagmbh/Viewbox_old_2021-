using System.Collections.Generic;
using System.ComponentModel;
using SystemDb.Internal;

namespace SystemDb
{
	public interface IParameter : IDataObject, INotifyPropertyChanged
	{
		SqlType DataType { get; set; }

		int TypeModifier { get; set; }

		IIssue Issue { get; set; }

		int OriginalOrdinal { get; set; }

		string Default { get; }

		string ColumnName { get; }

		string TableName { get; }

		string DatabaseName { get; }

		int IsRequired { get; }

		int OptionallyRequired { get; }

		int GroupId { get; set; }

		int LeadingZeros { get; }

		int FreeSelection { get; }

		bool HasIndexData { get; set; }

		string ColumnNameInView { get; set; }

		int UseAbsolute { get; set; }

		string IndexErrorStyleString { get; set; }

		bool IndexDataGenerated { get; set; }

		IEnumerable<IParameterValue> Values { get; set; }

		IHistoryParameterValueCollection HistoryValues { get; set; }

		List<IHistoryParameterValueFreeSelection> HistoryFreeSelectionValues { get; set; }

		OptimizationType OptimizationType { get; set; }

		OptimizationDirection OptimizationDirection { get; set; }
	}
}
