using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DbAccess.Structures;

namespace SystemDb.Internal
{
	internal class IndexCollection : Dictionary<int, IIndex>, IIndexCollection, IEnumerable<IIndex>, IEnumerable, ICloneable
	{
		public new IIndex this[int id]
		{
			get
			{
				if (!ContainsKey(id))
				{
					return null;
				}
				return base[id];
			}
		}

		public void Add(IIndex index)
		{
			Add(index.Id, index);
			(index as Index).IndexRemoved += delegate(IIndex i)
			{
				Remove(i.Id);
			};
		}

		public void Remove(IIndex index)
		{
			Remove(index.Id);
		}

		public new void Clear()
		{
			base.Clear();
		}

		public new IEnumerator<IIndex> GetEnumerator()
		{
			return base.Values.GetEnumerator();
		}

		public IEnumerable<IIndex> GetByColumnName(string columnName)
		{
			foreach (IIndex index in base.Values)
			{
				if (index.Columns.ToList().Exists((IColumn c) => c.Name == columnName))
				{
					yield return index;
				}
			}
		}

		public object Clone()
		{
			IndexCollection clone = new IndexCollection();
			using IEnumerator<IIndex> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				IIndex i = enumerator.Current;
				clone.Add(i.Clone() as IIndex);
			}
			return clone;
		}

		internal void Add(IIndex index, IEnumerable<IColumn> columns)
		{
			index.Columns = columns.ToList();
			Add(index);
		}

		internal static IEnumerable<IIndex> CreateFromDbIndexObject(IDbIndexInfoCollection dbIndexes, ITableObject tableObject)
		{
			if (dbIndexes == null || dbIndexes.Count == 0)
			{
				yield break;
			}
			foreach (DbIndexInfo dbIndex in dbIndexes)
			{
				Index index = new Index
				{
					Columns = tableObject.Columns.ToList().FindAll((IColumn c) => dbIndex.Columns.Contains(c.Name)),
					IndexName = dbIndex.Name,
					IndexType = dbIndex.Type,
					TableId = tableObject.Id
				};
				index.IndexIdChanged += delegate(IIndex indx, int indexId)
				{
					index.IndexColumnMappings.ToList().ForEach(delegate(IndexColumnMapping cm)
					{
						cm.IndexId = indexId;
					});
				};
				index.IndexColumnMappings = index.Columns.Select((IColumn c) => new IndexColumnMapping
				{
					ColumnId = c.Id
				}).ToList();
				yield return index;
			}
		}
	}
}
