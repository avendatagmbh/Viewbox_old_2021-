using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IArchiveSettingsCollection : IEnumerable<IArchiveSetting>, IEnumerable
	{
		int Count { get; }

		IArchiveSetting this[int id] { get; }
	}
}
