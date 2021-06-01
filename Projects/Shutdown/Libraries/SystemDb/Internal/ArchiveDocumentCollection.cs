using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class ArchiveDocumentCollection : DataObjectCollection<ArchiveDocument, IArchiveDocument>, IArchiveDocumentCollection, IDataObjectCollection<IArchiveDocument>, IEnumerable<IArchiveDocument>, IEnumerable
	{
	}
}
