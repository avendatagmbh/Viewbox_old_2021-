using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class PropertiesCollection : Dictionary<int, IProperty>, IPropertiesCollection, IEnumerable<IProperty>, IEnumerable
	{
		public new IProperty this[int key]
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

		public new IEnumerator<IProperty> GetEnumerator()
		{
			return base.Values.GetEnumerator();
		}

		public void Add(Property property)
		{
			base[property.Id] = property;
		}
	}
}
