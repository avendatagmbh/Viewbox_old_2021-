using System.ComponentModel;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("user_issueparameter_history", ForceInnoDb = true)]
	public class HistoryParameterValue : DataObject, IHistoryParameterValue, IDataObject, INotifyPropertyChanged
	{
		[DbColumn("user_id")]
		public int UserId { get; set; }

		[DbColumn("parameter_id")]
		public int ParameterId { get; set; }

		[DbColumn("Value")]
		public string Value { get; set; }

		[DbColumn("selection_type")]
		public int SelectionType { get; set; }

		public HistoryParameterValue()
			: base(DataObjectType.ParHistoryValue)
		{
		}

		public object Clone()
		{
			HistoryParameterValue c = new HistoryParameterValue();
			base.Clone(ref c);
			c.UserId = UserId;
			c.ParameterId = ParameterId;
			c.Value = Value;
			c.SelectionType = SelectionType;
			return c;
		}
	}
}
