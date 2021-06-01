using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class CorePropertiesCollection : Dictionary<int, ICoreProperty>, ICorePropertiesCollection, IEnumerable<ICoreProperty>, IEnumerable
	{
		public new ICoreProperty this[int key]
		{
			get
			{
				if (!ContainsKey(key))
				{
					return null;
				}
				return base[key];
			}
		}

		public new IEnumerator<ICoreProperty> GetEnumerator()
		{
			return base.Values.GetEnumerator();
		}

		public void Add(CoreProperty property)
		{
			base[property.Id] = property;
		}
	}
}
