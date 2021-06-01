using System;
using System.Collections.Generic;
using DbAccess.Attributes;
using DbAccess.Structures;

namespace SystemDb.Internal
{
	[DbTable("indexes", ForceInnoDb = true)]
	internal class Index : IIndex, ICloneable
	{
		public delegate void IndexChangedHandler(IIndex index, string property, object old_value);

		public delegate void IndexIdChangedHandler(IIndex index, int id);

		public delegate void IndexRemovedHandler(IIndex index);

		private int _id;

		private string _indexName;

		[DbColumn("id", AutoIncrement = true)]
		[DbPrimaryKey]
		public int Id
		{
			get
			{
				return _id;
			}
			set
			{
				if (_id != value)
				{
					_id = value;
					if (this.IndexIdChanged != null)
					{
						this.IndexIdChanged(this, _id);
					}
				}
			}
		}

		[DbColumn("table_id")]
		public int TableId { get; set; }

		[DbColumn("index_name", Length = 128)]
		public string IndexName
		{
			get
			{
				return _indexName;
			}
			set
			{
				if (_indexName != value.ToLower())
				{
					string old = _indexName;
					_indexName = value.ToLower();
					if (this.IndexChanged != null)
					{
						this.IndexChanged(this, "IndexName", old);
					}
				}
			}
		}

		[DbColumn("type")]
		public DbIndexType IndexType { get; set; }

		public IList<IColumn> Columns { get; set; }

		public IList<IndexColumnMapping> IndexColumnMappings { get; set; }

		public event IndexRemovedHandler IndexRemoved;

		public event IndexChangedHandler IndexChanged;

		public event IndexIdChangedHandler IndexIdChanged;

		public Index()
		{
			IndexColumnMappings = new List<IndexColumnMapping>();
		}

		public object Clone()
		{
			return MemberwiseClone();
		}

		public void Remove()
		{
			if (this.IndexRemoved != null)
			{
				this.IndexRemoved(this);
			}
		}
	}
}
