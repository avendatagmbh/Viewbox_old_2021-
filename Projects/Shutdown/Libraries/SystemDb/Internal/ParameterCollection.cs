using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class ParameterCollection : DataObjectCollection<Parameter, IParameter>, IParameterCollection, IDataObjectCollection<IParameter>, IEnumerable<IParameter>, IEnumerable
	{
	}
}
