using System.ComponentModel;

namespace SystemDb.Internal
{
	public interface IHistoryParameterValueFreeSelection : IDataObject, INotifyPropertyChanged
	{
		new int Id { get; }

		int UserId { get; }

		int IssueId { get; }

		int ParameterId { get; }

		int SelectionType { get; }

		string Value { get; }
	}
}
