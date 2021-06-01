using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	public class FileSysCollection : List<IFileSys>, IFileSysCollection, IEnumerable<IFileSys>, IEnumerable
	{
	}
}
