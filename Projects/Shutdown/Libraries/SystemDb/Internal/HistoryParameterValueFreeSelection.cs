using System.ComponentModel;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("user_free_selection_parameter_history", ForceInnoDb = true)]
	public class HistoryParameterValueFreeSelection : DataObject, IHistoryParameterValueFreeSelection, IDataObject, INotifyPropertyChanged
	{
		[DbColumn("id", AutoIncrement = true)]
		[DbPrimaryKey]
		public new int Id { get; set; }

		[DbColumn("userId")]
		public int UserId { get; set; }

		[DbColumn("issueId")]
		public int IssueId { get; set; }

		[DbColumn("parameterId")]
		public int ParameterId { get; set; }

		[DbColumn("selectionType")]
		public int SelectionType { get; set; }

		[DbColumn("value")]
		public string Value { get; set; }

		public HistoryParameterValueFreeSelection()
			: base(DataObjectType.ParHistoryValue)
		{
		}
	}
}
