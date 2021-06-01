using System;
using System.ComponentModel;

namespace SystemDb
{
	public interface IColumn : IDataObject, INotifyPropertyChanged, ICloneable
	{
		ITableObject Table { get; set; }

		string OriginalName { get; set; }

		bool IsVisible { get; set; }

		bool IsTempedHidden { get; set; }

		bool IsEmpty { get; }

		SqlType DataType { get; set; }

		int MaxLength { get; set; }

		string ConstValue { get; }

		bool IsOptEmpty { get; set; }

		bool IsOneDistinct { get; set; }

		string HeaderClass { get; set; }

		string HeaderStyle { get; set; }

		int OriginalWidth { get; set; }

		int UserDefinedWidth { get; set; }

		string MinValue { get; set; }

		string MaxValue { get; set; }

		OptimizationType OptimizationType { get; set; }

		bool HasIndex { get; }

		bool ExactMatchUnchecked { get; set; }

		string FromColumn { get; set; }

		string FromColumnFormat { get; set; }

		short Flag { get; set; }

		int ParamOperator { get; set; }
	}
}
