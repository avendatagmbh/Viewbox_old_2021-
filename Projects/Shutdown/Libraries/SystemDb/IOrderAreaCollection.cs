using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IOrderAreaCollection : IEnumerable<IOrderArea>, IEnumerable, ICloneable
	{
		int Count { get; }

		IOrderArea this[string language, string index, string split, string sort] { get; }

		IEnumerable<Tuple<long, long>> GetRanges(string languageValue, string indexValue, string splitValue, string sortValue);

		List<IOrderArea> GetOrderAreas(string languageValue, string indexValue, string splitValue, string sortValue);

		void Clear();
	}
}
