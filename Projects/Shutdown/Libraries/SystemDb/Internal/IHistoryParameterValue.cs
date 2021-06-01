using System.ComponentModel;

namespace SystemDb.Internal
{
	public interface IHistoryParameterValue : IDataObject, INotifyPropertyChanged
	{
		int UserId { get; }

		int ParameterId { get; }

		string Value { get; }

		int SelectionType { get; set; }
	}
}
