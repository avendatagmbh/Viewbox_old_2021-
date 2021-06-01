using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	public class ObjectTypeTextCollection : IObjectTypeTextCollection, IEnumerable<IObjectTypeText>, IEnumerable
	{
		private readonly Dictionary<Tuple<int, string>, ObjectTypeText> _dic = new Dictionary<Tuple<int, string>, ObjectTypeText>();

		public IObjectTypeText this[int refId, string countryCode]
		{
			get
			{
				Tuple<int, string> key = Tuple.Create(refId, countryCode);
				if (!_dic.ContainsKey(key))
				{
					return null;
				}
				return _dic[key];
			}
		}

		public IEnumerator<IObjectTypeText> GetEnumerator()
		{
			return _dic.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(ObjectTypeText settings)
		{
			_dic[Tuple.Create(settings.RefId, settings.CountryCode)] = settings;
		}
	}
}
