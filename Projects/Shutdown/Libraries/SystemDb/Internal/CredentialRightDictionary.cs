using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal abstract class CredentialRightDictionary<C, O> : Dictionary<Tuple<C, O>, Tuple<int, RightType>>, IEnumerable<Tuple<C, O, RightType>>, IEnumerable where C : ICredential
	{
		public RightType this[C credential, O obj]
		{
			get
			{
				Tuple<C, O> key = Tuple.Create(credential, obj);
				if (!ContainsKey(key))
				{
					return RightType.Inherit;
				}
				return base[key].Item2;
			}
			set
			{
				Tuple<C, O> key = Tuple.Create(credential, obj);
				int id = (ContainsKey(key) ? base[key].Item1 : 0);
				base[key] = Tuple.Create(id, value);
			}
		}

		public new IEnumerator<Tuple<C, O, RightType>> GetEnumerator()
		{
			foreach (Tuple<C, O> key in base.Keys)
			{
				yield return Tuple.Create(key.Item1, key.Item2, base[key].Item2);
			}
		}

		public RightDictionary<O> GetObjects(C credential)
		{
			RightDictionary<O> d = new RightDictionary<O>();
			//foreach (KeyValuePair<Tuple<C, O>, Tuple<int, RightType>> o in this)
			//{
			//	if (o.Key.Item1 != null && o.Key.Item1.Equals(credential) && o.Key.Item2 != null)
			//	{
			//		d.Add(o.Key.Item2, o.Value);
			//	}
			//}
			return d;
		}

		public void Add(C credential, O obj, int id, RightType right)
		{
			base[Tuple.Create(credential, obj)] = Tuple.Create(id, right);
		}

		protected int GetMappingId(C credential, O obj)
		{
			Tuple<C, O> key = Tuple.Create(credential, obj);
			if (!ContainsKey(key))
			{
				return 0;
			}
			return base[key].Item1;
		}
	}
}
