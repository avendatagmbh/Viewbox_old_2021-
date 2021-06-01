using System;
using System.ComponentModel;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("tables", ForceInnoDb = true)]
	public class Issue : TableObject, IIssue, ITableObject, IDataObject, INotifyPropertyChanged, ICloneable
	{
		private ParameterCollection _parameters = new ParameterCollection();

		public int ExecutedIssueId { get; set; }

		public int OriginalId { get; set; }

		public string Command { get; set; }

		public IssueType IssueType { get; set; }

		public bool UseLanguageValue { get; set; }

		public bool UseIndexValue { get; set; }

		public bool UseSplitValue { get; set; }

		public bool UseSortValue { get; set; }

		public ITableObject FilterTableObject { get; set; }

		public string RowNoFilter { get; set; }

		public bool NeedGJahr { get; set; }

		public bool NeedBukrs { get; set; }

		public ParameterSelectionLogic Logic { get; set; }

		public IParameterCollection Parameters => _parameters;

		public int Flag { get; set; }

		public Issue()
			: base(TableType.Issue)
		{
		}

		public override object Clone()
		{
			TableObject clone = new Issue();
			Clone(ref clone);
			Issue obj = clone as Issue;
			obj.Command = Command;
			obj._parameters = Parameters as ParameterCollection;
			obj.UseLanguageValue = UseLanguageValue;
			obj.UseSplitValue = UseSplitValue;
			obj.UseIndexValue = UseIndexValue;
			obj.UseSortValue = UseSortValue;
			obj.FilterTableObject = FilterTableObject;
			obj.IssueType = IssueType;
			obj.Flag = Flag;
			obj.RowNoFilter = RowNoFilter;
			obj.NeedGJahr = NeedGJahr;
			obj.NeedBukrs = NeedBukrs;
			obj.OriginalId = OriginalId;
			obj.Logic = Logic;
			return obj;
		}
	}
}
