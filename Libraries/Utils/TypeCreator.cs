using System;
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

    public class Comparer<T> : IComparer<T>
    {
        private readonly Func<T, T, int> _func;

        public Comparer(Func<T, T, int> func)
        {
            _func = func;
        }

        #region IComparer<T> Members

        public int Compare(T x, T y)
        {
            return _func(x, y);
        }

        #endregion
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

    public class EqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, int> _func;

        public EqualityComparer(Func<T, int> func)
        {
            _func = func;
        }

        #region Implementation of IEqualityComparer<in T>

        public bool Equals(T x, T y)
        {
            return _func(x) == _func(y);
        }

        public int GetHashCode(T obj)
        {
            return _func(obj);
        }

        #endregion
    }

    public static class EqualityComparer
    {
        public static EqualityComparer<T> Create<T>(Func<T, int> hashCodeFunc)
        {
            return new EqualityComparer<T>(hashCodeFunc);
        }

        public static EqualityComparer<T> CreateEqualityComparerForElements<T>(this IEnumerable<T> enumerable,
                                                                               Func<T, int> func)
        {
            return new EqualityComparer<T>(func);
        }
    }
}