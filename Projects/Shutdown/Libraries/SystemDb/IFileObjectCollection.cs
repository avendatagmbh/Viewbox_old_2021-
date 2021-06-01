using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IFileObjectCollection : IEnumerable<IFileObject>, IEnumerable
	{
		IFileObject this[int id] { get; set; }

		void Add(IFileObject item);
	}
}
