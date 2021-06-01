using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DbAccess.Structures
{
	public class IndexInfoList : List<DbIndexInfo>, IDbIndexInfoCollection, IList<DbIndexInfo>, ICollection<DbIndexInfo>, IEnumerable<DbIndexInfo>, IEnumerable
	{
		public DbIndexInfo this[string columnName] => (from i in this
			from c in i.Columns
			where c == columnName
			select i).FirstOrDefault();

		public IDbIndexInfoCollection Clone()
		{
			return MemberwiseClone() as IDbIndexInfoCollection;
		}

		public new void AddRange(IEnumerable<DbIndexInfo> collection)
		{
			if (collection == null)
			{
				return;
			}
			foreach (DbIndexInfo indexInfo in collection)
			{
				if (!Exists((DbIndexInfo i) => i.Name == indexInfo.Name))
				{
					Add(indexInfo);
				}
			}
		}
	}
}
