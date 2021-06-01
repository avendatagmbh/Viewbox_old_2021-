using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface ITableArchiveInformationCollection : IEnumerable<ITableArchiveInformation>, IEnumerable
	{
		ITableArchiveInformation this[int tableId] { get; }
	}
}
