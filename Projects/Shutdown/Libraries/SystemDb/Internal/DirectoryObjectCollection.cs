using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	public class DirectoryObjectCollection : List<IDirectoryObject>, IDirectoryObjectCollection, IEnumerable<IDirectoryObject>, IEnumerable
	{
		public new IDirectoryObject this[int id]
		{
			get
			{
				if (!Exists((IDirectoryObject d) => d.Id == id))
				{
					return null;
				}
				return Find((IDirectoryObject d) => d.Id == id);
			}
			set
			{
				base[id] = value;
			}
		}
	}
}
