using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class CategoryCollection : Dictionary<int, ICategory>, ICategoryCollection, IEnumerable<ICategory>, IEnumerable
	{
		public new ICategory this[int id]
		{
			get
			{
				if (!ContainsKey(id))
				{
					return null;
				}
				return base[id];
			}
		}

		public void Add(ICategory category)
		{
			Add(category.Id, category);
		}

		public new IEnumerator<ICategory> GetEnumerator()
		{
			return base.Values.GetEnumerator();
		}

		public void AddRange(IEnumerable<ICategory> list)
		{
			foreach (ICategory c in list)
			{
				Add(c);
			}
		}
	}
}
