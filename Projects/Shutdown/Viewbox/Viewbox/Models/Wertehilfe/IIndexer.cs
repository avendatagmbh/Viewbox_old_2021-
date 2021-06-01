using SystemDb;

namespace Viewbox.Models.Wertehilfe
{
	public interface IIndexer
	{
		IParameter Parameter { get; }

		IColumn Column { get; }

		string IndexDbName { get; }

		IndexingState State { get; }

		void DoIndexing();

		void CancelIndexing();

		bool CheckIfExists();
	}
}
