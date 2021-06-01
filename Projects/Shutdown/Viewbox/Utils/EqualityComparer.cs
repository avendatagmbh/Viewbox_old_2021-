using System;
using System.Collections.Generic;

namespace Utils
{
	public class EqualityComparer<T> : IEqualityComparer<T>
	{
		private readonly Func<T, int> _func;

		public EqualityComparer(Func<T, int> func)
		{
			_func = func;
		}

		public bool Equals(T x, T y)
		{
			return _func(x) == _func(y);
		}

		public int GetHashCode(T obj)
		{
			return _func(obj);
		}
	}
	public static class EqualityComparer
	{
		public static EqualityComparer<T> Create<T>(Func<T, int> hashCodeFunc)
		{
			return new EqualityComparer<T>(hashCodeFunc);
		}

		public static EqualityComparer<T> CreateEqualityComparerForElements<T>(this IEnumerable<T> enumerable, Func<T, int> func)
		{
			return new EqualityComparer<T>(func);
		}
	}
}
