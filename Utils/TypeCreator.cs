using System.Collections.Generic;

namespace Utils
{
	public static class TypeCreator
	{
		public static List<T> TypeGenerator<T>(this T[] t)
		{
			return new List<T>(t);
		}

		public static IDictionary<TKey, TValue> NewDictionary<TKey, TValue>(TKey key, TValue value)
		{
			return new Dictionary<TKey, TValue>();
		}
	}
}
