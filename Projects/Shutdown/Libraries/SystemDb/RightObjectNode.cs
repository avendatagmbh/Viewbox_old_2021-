using System;
using System.Collections.Generic;

namespace SystemDb
{
	public class RightObjectNode
	{
		private readonly List<Tuple<ICredential, RightType>> _rightHierarchy = new List<Tuple<ICredential, RightType>>();

		private readonly List<RightObjectNode> _children = new List<RightObjectNode>();

		private RightObjectNode _parent;

		public UpdateRightType Type { get; set; }

		public int Id { get; set; }

		public RightType Right { get; set; }

		public List<Tuple<ICredential, RightType>> RightHierarchy => _rightHierarchy;

		public RightObjectNode Parent
		{
			get
			{
				return _parent;
			}
			set
			{
				if (_parent != value)
				{
					_parent = value;
				}
			}
		}

		public List<RightObjectNode> Children => _children;

		public RightType GetRight(bool isAllowed = false)
		{
			if (RightType.None < Right)
			{
				return Right;
			}
			if (Right == RightType.Inherit || (Right == RightType.None && isAllowed))
			{
				RightObjectNode node = this;
				foreach (RightObjectNode c in Children)
				{
					if (node.Right < c.Right)
					{
						node = c;
					}
				}
				return node.Right;
			}
			return RightType.None;
		}
	}
}
