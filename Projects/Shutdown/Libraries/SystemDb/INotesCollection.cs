using System.Collections.Generic;

namespace SystemDb
{
	public interface INotesCollection
	{
		int Count { get; }

		List<INote> this[IUser user] { get; }
	}
}
