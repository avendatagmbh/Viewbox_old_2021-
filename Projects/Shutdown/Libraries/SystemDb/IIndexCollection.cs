using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IIndexCollection : IEnumerable<IIndex>, IEnumerable, ICloneable
	{
		int Count { get; }

		IIndex this[int id] { get; }

		IEnumerable<IIndex> GetByColumnName(string columnName);

		void Add(IIndex index);

		void Remove(IIndex index);

		void Clear();
	}
}
