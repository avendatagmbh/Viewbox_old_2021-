using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IParameterCollection : IDataObjectCollection<IParameter>, IEnumerable<IParameter>, IEnumerable
	{
	}
}
