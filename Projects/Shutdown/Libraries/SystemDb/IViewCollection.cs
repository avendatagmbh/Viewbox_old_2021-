using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IViewCollection : IDataObjectCollection<IView>, IEnumerable<IView>, IEnumerable
	{
	}
}
