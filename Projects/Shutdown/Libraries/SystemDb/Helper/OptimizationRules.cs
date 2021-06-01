using System.Collections.Generic;
using System.Linq;

namespace SystemDb.Helper
{
	internal class OptimizationRules
	{
		private readonly ISet<int> _nodes = new HashSet<int>();

		public IEnumerable<int> Ids => _nodes;

		public void AddOptimizationRule(IOptimization opt)
		{
			AddChildrenRule(opt);
			if (opt.Parent == null)
			{
				return;
			}
			while (opt.Parent.Id != 0)
			{
				opt = opt.Parent;
				if (!_nodes.Contains(opt.Id))
				{
					_nodes.Add(opt.Id);
				}
			}
		}

		public void AddOptimizationRules(IEnumerable<IOptimization> optCol)
		{
			foreach (IOptimization opt2 in optCol.Where((IOptimization opt) => !_nodes.Contains(opt.Id)))
			{
				_nodes.Add(opt2.Id);
			}
		}

		public void RemoveOptimizationRule(IOptimization opt)
		{
			if (_nodes.Contains(opt.Id))
			{
				_nodes.Remove(opt.Id);
			}
			foreach (IOptimization c in opt.Children)
			{
				RemoveOptimizationRule(c);
			}
		}

		public void AddChildrenRule(IOptimization opt)
		{
			if (!_nodes.Contains(opt.Id))
			{
				_nodes.Add(opt.Id);
			}
			foreach (IOptimization c in opt.Children)
			{
				AddChildrenRule(c);
			}
		}
	}
}
