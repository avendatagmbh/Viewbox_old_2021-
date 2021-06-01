using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class ViewCollection : DataObjectCollection<View, IView>, IViewCollection, IDataObjectCollection<IView>, IEnumerable<IView>, IEnumerable
	{
	}
}
