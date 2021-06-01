using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class OptimizationGroupCollection : Dictionary<int, IOptimizationGroup>, IOptimizationGroupCollection, IEnumerable<IOptimizationGroup>, IEnumerable
	{
		public new IOptimizationGroup this[int id]
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

		public new IEnumerator<IOptimizationGroup> GetEnumerator()
		{
			return base.Values.GetEnumerator();
		}

		public void Add(IOptimizationGroup opt_group)
		{
			Add(opt_group.Id, opt_group);
		}

		public void AddRange(IEnumerable<IOptimizationGroup> opt_groups)
		{
			try
			{
				foreach (IOptimizationGroup o in opt_groups)
				{
					Add(o);
				}
			}
			catch (Exception)
			{
			}
		}
	}
}
