using System;
using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class OptimizationCollection : Dictionary<int, IOptimization>, IOptimizationCollection, IEnumerable<IOptimization>, IEnumerable
	{
		private int _highestLevel;

		public new IOptimization this[int id]
		{
			get
			{
				if (!ContainsKey(id))
				{
					return null;
				}
				return base[id];
			}
			private set
			{
				if (ContainsKey(id))
				{
					base[id] = value;
				}
			}
		}

		public int HighestLevel => _highestLevel;

		public new IEnumerator<IOptimization> GetEnumerator()
		{
			return base.Values.GetEnumerator();
		}

		public void Add(IOptimization optimization)
		{
			if (ContainsKey(optimization.Id))
			{
				this[optimization.Id] = optimization;
				if (HighestLevel == 0)
				{
					_highestLevel = optimization.Level;
				}
				else
				{
					_highestLevel = Math.Min(_highestLevel, optimization.Level);
				}
			}
			else
			{
				Add(optimization.Id, optimization);
				if (HighestLevel == 0)
				{
					_highestLevel = optimization.Level;
				}
				else
				{
					_highestLevel = Math.Min(_highestLevel, optimization.Level);
				}
			}
		}

		public void AddRange(IEnumerable<IOptimization> optimizations)
		{
			try
			{
				foreach (IOptimization o in optimizations)
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
