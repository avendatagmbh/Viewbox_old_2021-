using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	public class FileObjectCollection : List<IFileObject>, IFileObjectCollection, IEnumerable<IFileObject>, IEnumerable
	{
		public new IFileObject this[int id]
		{
			get
			{
				if (!Contains(base[id]))
				{
					return null;
				}
				return base[id];
			}
			set
			{
				base[id] = value;
			}
		}

		void IFileObjectCollection.Add(IFileObject item)
		{
			Add(item);
		}
	}
}
