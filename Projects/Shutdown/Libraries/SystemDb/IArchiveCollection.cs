using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IArchiveCollection : IDataObjectCollection<IArchive>, IEnumerable<IArchive>, IEnumerable
	{
	}
}
