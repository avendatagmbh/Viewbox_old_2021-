using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IRelation : IEnumerable<IColumnConnection>, IEnumerable
	{
		int RelationId { get; set; }
	}
}
