using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IDirectoryObjectCollection : IEnumerable<IDirectoryObject>, IEnumerable
	{
		IDirectoryObject this[int id] { get; set; }
	}
}
