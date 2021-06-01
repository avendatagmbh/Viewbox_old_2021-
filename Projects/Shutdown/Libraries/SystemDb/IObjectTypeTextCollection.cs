using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IObjectTypeTextCollection : IEnumerable<IObjectTypeText>, IEnumerable
	{
		IObjectTypeText this[int refId, string countryCode] { get; }
	}
}
