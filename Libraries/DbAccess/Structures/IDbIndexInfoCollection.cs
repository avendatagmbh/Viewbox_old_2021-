using System.Collections;
using System.Collections.Generic;

namespace DbAccess.Structures
{
	public interface IDbIndexInfoCollection : IList<DbIndexInfo>, ICollection<DbIndexInfo>, IEnumerable<DbIndexInfo>, IEnumerable
	{
		DbIndexInfo this[string columnName] { get; }

		IDbIndexInfoCollection Clone();

		void AddRange(IEnumerable<DbIndexInfo> collection);
	}
}
