using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class RightDictionary<O> : Dictionary<O, Tuple<int, RightType>>, IEnumerable<Tuple<O, RightType>>, IEnumerable
	{
		public new RightType this[O obj]
		{
			get
			{
				if (!ContainsKey(obj))
				{
					return RightType.Inherit;
				}
				return base[obj].Item2;
			}
		}

		public new IEnumerator<Tuple<O, RightType>> GetEnumerator()
		{
			foreach (O key in base.Keys)
			{
				yield return Tuple.Create(key, base[key].Item2);
			}
		}
	}
}
