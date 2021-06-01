using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IArchiveDocumentCollection : IDataObjectCollection<IArchiveDocument>, IEnumerable<IArchiveDocument>, IEnumerable
	{
	}
}
