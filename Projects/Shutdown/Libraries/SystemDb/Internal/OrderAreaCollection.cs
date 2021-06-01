using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SystemDb.Internal
{
	internal class OrderAreaCollection : Dictionary<Tuple<string, string, string, string>, IOrderArea>, IOrderAreaCollection, IEnumerable<IOrderArea>, IEnumerable, ICloneable
	{
		private readonly ITableObject _tableObject;

		public IOrderArea this[string language, string index, string split, string sort]
		{
			get
			{
				Tuple<string, string, string, string> tuple = Tuple.Create(language, index, split, sort);
				if (!ContainsKey(tuple))
				{
					return null;
				}
				return base[tuple];
			}
		}

		public OrderAreaCollection(ITableObject tobj)
		{
			_tableObject = tobj;
		}

		public new IEnumerator<IOrderArea> GetEnumerator()
		{
			if (base.Count > 0)
			{
				foreach (IOrderArea value in base.Values)
				{
					yield return value;
				}
			}
			else if (_tableObject != null && _tableObject.RowCount >= 0)
			{
				yield return new OrderArea
				{
					LanguageValue = null,
					IndexValue = null,
					SortValue = null,
					SplitValue = null,
					Start = 1L,
					End = _tableObject.RowCount,
					TableId = _tableObject.Id
				};
			}
		}

		public IEnumerable<Tuple<long, long>> GetRanges(string languageValue, string indexValue, string splitValue, string sortValue)
		{
			languageValue = LanguageKeyTransformer.Transformer(languageValue);
			return GetRanges(languageValue, indexValue, splitValue, sortValue, base.Values.ToList());
		}

		public List<IOrderArea> GetOrderAreas(string languageValue, string indexValue, string splitValue, string sortValue)
		{
			languageValue = LanguageKeyTransformer.Transformer(languageValue);
			return GetOrderAreas(languageValue, indexValue, splitValue, sortValue, base.Values.ToList());
		}

		public object Clone()
		{
			OrderAreaCollection orderAreas = new OrderAreaCollection(_tableObject);
			using IEnumerator<IOrderArea> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				IOrderArea o = enumerator.Current;
				if (o.Id != 0)
				{
					orderAreas.Add(o);
				}
			}
			return orderAreas;
		}

		public void Add(IOrderArea orderArea)
		{
			Tuple<string, string, string, string> key = Tuple.Create(orderArea.LanguageValue, orderArea.IndexValue, orderArea.SplitValue, orderArea.SortValue);
			if (!ContainsKey(key))
			{
				Add(key, orderArea);
			}
		}

		private static List<IOrderArea> GetOrderAreas(string languageValue, string indexValue, string splitValue, string sortValue, IEnumerable<IOrderArea> orderAreas)
		{
			int orderAreasCount = orderAreas.Count();
			int countNullValues = 0;
			countNullValues = orderAreas.Count((IOrderArea o) => o.SplitValue == null);
			bool ignoreSplitValues = countNullValues == orderAreasCount;
			countNullValues = orderAreas.Count((IOrderArea o) => o.SortValue == null);
			bool ignoreSortValues = countNullValues == orderAreasCount;
			countNullValues = orderAreas.Count((IOrderArea o) => o.IndexValue == null);
			bool ignoreIndexValues = countNullValues == orderAreasCount;
			languageValue = LanguageKeyTransformer.Transformer(languageValue);
			countNullValues = orderAreas.Count((IOrderArea o) => o.LanguageValue == null);
			bool ignoreLanguageValues = countNullValues == orderAreasCount;
			return (from o in orderAreas
				orderby o.Start
				where (indexValue == null || indexValue == o.IndexValue || ignoreIndexValues) && (splitValue == null || splitValue == o.SplitValue || ignoreSplitValues) && (sortValue == null || sortValue == o.SortValue || ignoreSortValues) && (languageValue == null || languageValue == o.LanguageValue || ignoreLanguageValues)
				select o).ToList();
		}

		public static List<Tuple<long, long>> GetRanges(string languageValue, string indexValue, string splitValue, string sortValue, IEnumerable<IOrderArea> orderAreas)
		{
			if (orderAreas.Count() != 0)
			{
				languageValue = LanguageKeyTransformer.Transformer(languageValue);
				List<Tuple<long, long>> list2 = new List<Tuple<long, long>>(from o in GetOrderAreas(languageValue, indexValue, splitValue, sortValue, orderAreas)
					select new Tuple<long, long>(o.Start, o.End));
				List<Tuple<long, long>> list = new List<Tuple<long, long>>();
				foreach (Tuple<long, long> a in list2)
				{
					list.Add(new Tuple<long, long>(a.Item1, a.Item2));
				}
				if (list.Count == 0)
				{
					list.Add(new Tuple<long, long>(0L, 0L));
				}
				return list;
			}
			return new List<Tuple<long, long>>();
		}
	}
}
