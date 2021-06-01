using System;
using System.Collections.Generic;

namespace SystemDb
{
	public class RightObjectNodeCollection : Dictionary<Tuple<int, UpdateRightType>, RightObjectNode>
	{
		public RightObjectNode this[int id, UpdateRightType type]
		{
			get
			{
				if (!ContainsKey(new Tuple<int, UpdateRightType>(id, type)))
				{
					return null;
				}
				return base[new Tuple<int, UpdateRightType>(id, type)];
			}
			set
			{
				if (ContainsKey(new Tuple<int, UpdateRightType>(id, type)))
				{
					base[new Tuple<int, UpdateRightType>(id, type)] = value;
				}
			}
		}

		public void Add(RightObjectNode rightObjectNode)
		{
			if (ContainsKey(new Tuple<int, UpdateRightType>(rightObjectNode.Id, rightObjectNode.Type)))
			{
				this[rightObjectNode.Id, rightObjectNode.Type] = rightObjectNode;
			}
			else
			{
				Add(new Tuple<int, UpdateRightType>(rightObjectNode.Id, rightObjectNode.Type), rightObjectNode);
			}
		}

		public void AddRange(IEnumerable<RightObjectNode> rightObjectNodes)
		{
			foreach (RightObjectNode r in rightObjectNodes)
			{
				Add(r);
			}
		}

		public new IEnumerator<RightObjectNode> GetEnumerator()
		{
			return base.Values.GetEnumerator();
		}
	}
}
