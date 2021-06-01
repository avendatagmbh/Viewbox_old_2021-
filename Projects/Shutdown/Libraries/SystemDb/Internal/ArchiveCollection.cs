using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class ArchiveCollection : DataObjectCollection<Archive, IArchive>, IArchiveCollection, IDataObjectCollection<IArchive>, IEnumerable<IArchive>, IEnumerable
	{
	}
}
