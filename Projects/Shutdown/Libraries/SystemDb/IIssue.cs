using System;
using System.ComponentModel;
using SystemDb.Internal;

namespace SystemDb
{
	public interface IIssue : ITableObject, IDataObject, INotifyPropertyChanged, ICloneable
	{
		string Command { get; set; }

		bool UseLanguageValue { get; set; }

		bool UseIndexValue { get; set; }

		bool UseSplitValue { get; set; }

		bool UseSortValue { get; set; }

		IParameterCollection Parameters { get; }

		IssueType IssueType { get; }

		ITableObject FilterTableObject { get; }

		int Flag { get; }

		string RowNoFilter { get; }

		bool NeedGJahr { get; set; }

		bool NeedBukrs { get; set; }

		ParameterSelectionLogic Logic { get; set; }
	}
}
