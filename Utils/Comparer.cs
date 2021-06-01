using System;
using System.Collections.Generic;

namespace Utils
{
	public class Comparer<T> : IComparer<T>
	{
		private readonly Func<T, T, int> _func;

		public Comparer(Func<T, T, int> func)
		{
			_func = func;
		}

		public int Compare(T x, T y)
		{
			return _func(x, y);
		}
	}
	public static class Comparer
	{
		public static Comparer<T> Create<T>(Func<T, T, int> func)
		{
			return new Comparer<T>(func);
		}

		public static Comparer<T> CreateComparerForElements<T>(this IEnumerable<T> enumerable, Func<T, T, int> func)
		{
			return new Comparer<T>(func);
		}
	}
}
