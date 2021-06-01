using System;
using System.Collections.Generic;
using SystemDb.Internal;
using DbAccess.Structures;

namespace SystemDb
{
	public interface IIndex : ICloneable
	{
		int Id { get; set; }

		int TableId { get; set; }

		string IndexName { get; set; }

		DbIndexType IndexType { get; set; }

		IList<IColumn> Columns { get; set; }

		IList<IndexColumnMapping> IndexColumnMappings { get; set; }
	}
}
