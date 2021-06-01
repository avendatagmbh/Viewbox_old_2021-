using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SystemDb;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class StructuedTableModel : ViewboxModel
	{
		public enum NodeType
		{
			Structure,
			Account
		}

		public class Node
		{
			private List<string> _values = new List<string>();

			private List<Node> _children = new List<Node>();

			public string Key { get; private set; }

			public string Description { get; set; }

			public NodeType Type { get; private set; }

			public Node Parent { get; internal set; }

			public List<Node> Children
			{
				get
				{
					return _children;
				}
				set
				{
					_children = value;
				}
			}

			public int Depth
			{
				get
				{
					int num;
					if (Parent != null)
					{
						_ = Parent.Depth;
						num = 0;
					}
					else
					{
						num = 1;
					}
					if (num != 0)
					{
						return 0;
					}
					return Parent.Depth + 1;
				}
			}

			public List<string> Values => _values;

			public bool HasValueChildren => Type == NodeType.Account || Children.Any((Node w) => w.HasValueChildren);

			internal Node(string key, string description, NodeType type, List<string> values, Node parent = null)
			{
				Key = key;
				Description = description;
				Parent = parent;
				Type = type;
				_values = values;
				parent?._children.Add(this);
			}

			public double Value(int id)
			{
				if (Type == NodeType.Account && (_values == null || _values.Count < id || _values.Count == 0))
				{
					return 0.0;
				}
				double _value = 0.0;
				if (Type == NodeType.Account)
				{
					double.TryParse(_values[id].Replace(".", ","), out _value);
				}
				else
				{
					_value = getChildrenValues(id);
				}
				return _value;
			}

			private double getChildrenValues(int id)
			{
				double returnValue = 0.0;
				foreach (Node child in Children)
				{
					returnValue += child.Value(id);
				}
				return returnValue;
			}
		}

		private Dictionary<NodeType, Dictionary<string, Node>> _nodes = new Dictionary<NodeType, Dictionary<string, Node>>();

		private ConcurrentDictionary<Tuple<string, Node>, string> _waitingForParent = new ConcurrentDictionary<Tuple<string, Node>, string>();

		private int _maxValueCount = 0;

		private readonly Node _treeRoot;

		private string _offset = "0";

		public int IssueFlag;

		private bool IsOrdered = false;

		public List<string> ColumnNameList = new List<string>();

		public int NodeCount => _nodes[NodeType.Structure].Count;

		public int MaximumValueCount => _maxValueCount;

		public List<Tuple<IParameter, string, string>> Parameters { get; private set; }

		public Node TreeRoot => _treeRoot;

		public override string LabelCaption => "Structured view";

		public StructuedTableModel()
		{
			_nodes.Add(NodeType.Structure, new Dictionary<string, Node>());
			_nodes.Add(NodeType.Account, new Dictionary<string, Node>());
			_treeRoot = new Node("0", "", NodeType.Structure, new List<string>());
			_nodes[_treeRoot.Type].Add(_treeRoot.Key, _treeRoot);
			Parameters = new List<Tuple<IParameter, string, string>>();
		}

		public StructuedTableModel(string offset)
		{
			_nodes.Add(NodeType.Structure, new Dictionary<string, Node>());
			_nodes.Add(NodeType.Account, new Dictionary<string, Node>());
			_treeRoot = new Node("ROOT", "", NodeType.Structure, new List<string>());
			_nodes[_treeRoot.Type].Add(_treeRoot.Key, _treeRoot);
			Parameters = new List<Tuple<IParameter, string, string>>();
			AddNode(offset, "ROOT", offset, new List<string>());
			_offset = offset;
		}

		public Node AddNode(string id, string parent, string description, List<string> values = null)
		{
			if (parent == null)
			{
				parent = "";
			}
			if (values != null)
			{
				_maxValueCount = Math.Max(_maxValueCount, values.Count);
			}
			if (id == _treeRoot.Key || (_offset != "0" && id == _offset))
			{
				if (id == _treeRoot.Key)
				{
					Node parentNode = _nodes[NodeType.Structure][_treeRoot.Key];
					parentNode.Description = description;
					return parentNode;
				}
				if (_offset != "0" && id == _offset)
				{
					Node parentNode2 = _nodes[NodeType.Structure][_offset];
					parentNode2.Description = description;
					return parentNode2;
				}
			}
			if (_nodes[NodeType.Structure].ContainsKey(parent))
			{
				Node parentNode3 = _nodes[NodeType.Structure][parent];
				if (description == "")
				{
					description = parentNode3.Description;
				}
				Node node2 = new Node(id, description, NodeType.Structure, values, parentNode3);
				_nodes[NodeType.Structure][id] = node2;
				return node2;
			}
			Node node = new Node(id, description, NodeType.Structure, values);
			Tuple<string, Node> tupi = new Tuple<string, Node>(parent, node);
			_waitingForParent.TryAdd(tupi, id);
			return null;
		}

		private Node AddNode(string parent, Node imputNode)
		{
			if (_nodes[NodeType.Structure].ContainsKey(parent))
			{
				Node parentNode = _nodes[NodeType.Structure][parent];
				if (imputNode.Description == "")
				{
					imputNode.Description = parentNode.Description;
				}
				Node node = new Node(imputNode.Key, imputNode.Description, NodeType.Structure, imputNode.Values, parentNode);
				_nodes[NodeType.Structure][imputNode.Key] = node;
				return node;
			}
			return null;
		}

		public Node AddLeaf(string id, string parent, string description, List<string> values)
		{
			if (!IsOrdered)
			{
				ReOrderNodes();
			}
			if (parent == null)
			{
				parent = "";
			}
			if (values != null && values.Count > 0)
			{
				_maxValueCount = Math.Max(_maxValueCount, values.Count);
			}
			if (_nodes[NodeType.Structure].ContainsKey(parent))
			{
				Node parentNode = _nodes[NodeType.Structure][parent];
				if (description == "")
				{
					description = parentNode.Description;
				}
				Node node = new Node(id, description, NodeType.Account, values, parentNode);
				_nodes[NodeType.Structure][id] = node;
				return node;
			}
			return null;
		}

		public Node AddLeaf(string id, string parent, string description, string value)
		{
			return AddLeaf(id, parent, description, new List<string> { value });
		}

		public Node AddLeafWithChildsupport(string id, string parent, string description, List<string> values)
		{
			if (!IsOrdered)
			{
				ReOrderNodes();
			}
			if (parent == null)
			{
				parent = "";
			}
			if (values != null && values.Count > 0)
			{
				_maxValueCount = Math.Max(_maxValueCount, values.Count);
			}
			if (_nodes[NodeType.Structure].ContainsKey(parent))
			{
				Node parentNode = _nodes[NodeType.Structure][parent];
				if (description == "")
				{
					description = parentNode.Description;
				}
				Node node = new Node(id, description, NodeType.Account, values, parentNode);
				_nodes[NodeType.Structure][id] = node;
				try
				{
					FatherTest(parentNode.Parent, node);
					foreach (Node bro in parentNode.Children)
					{
						if (bro.HasValueChildren)
						{
							ChildTest(bro, node);
						}
					}
				}
				catch
				{
				}
				return node;
			}
			return null;
		}

		private void FatherTest(Node Father, Node Noody)
		{
			if (Father != null)
			{
				foreach (Node child in Father.Children)
				{
					if (child.Key == Noody.Key)
					{
						Father.Children.Remove(child);
					}
				}
			}
			if (Father.Parent != null)
			{
				FatherTest(Father.Parent, Noody);
			}
		}

		private void ChildTest(Node Brah, Node Noody)
		{
			if (Brah != null)
			{
				foreach (Node child in Brah.Children)
				{
					if (child.Key == Noody.Key)
					{
						Noody.Parent.Children.Remove(Noody);
					}
				}
			}
			foreach (Node bro in Brah.Children)
			{
				if (bro.HasValueChildren)
				{
					ChildTest(bro, Noody);
				}
			}
		}

		public Node BuildStructure(string description, string parent, bool isLeaf)
		{
			if (parent == null)
			{
				parent = "";
			}
			if (_nodes[NodeType.Structure].ContainsKey(parent))
			{
				Node parentNode = _nodes[NodeType.Structure][parent];
				if (description == "")
				{
					description = parentNode.Description + "_";
				}
				Node node2 = new Node(description, description, isLeaf ? NodeType.Account : NodeType.Structure, null, parentNode);
				_nodes[NodeType.Structure][description] = node2;
				return node2;
			}
			Node node = new Node(description, description, NodeType.Structure, null);
			Tuple<string, Node> tupi = new Tuple<string, Node>(parent, node);
			_waitingForParent.TryAdd(tupi, description);
			return null;
		}

		public Node BuildStructure(string id, string description, string parent, bool isLeaf)
		{
			if (parent == null)
			{
				parent = "";
			}
			if (_nodes[NodeType.Structure].ContainsKey(parent))
			{
				Node parentNode = _nodes[NodeType.Structure][parent];
				if (description == "")
				{
					description = parentNode.Description + "_";
				}
				Node node2 = new Node(id, description, isLeaf ? NodeType.Account : NodeType.Structure, null, parentNode);
				_nodes[NodeType.Structure][id] = node2;
				return node2;
			}
			Node node = new Node(description, description, NodeType.Structure, null);
			Tuple<string, Node> tupi = new Tuple<string, Node>(parent, node);
			_waitingForParent.TryAdd(tupi, description);
			return null;
		}

		public void ReOrderNodes()
		{
			if (IsOrdered)
			{
				return;
			}
			IsOrdered = true;
			int listCount = -1;
			List<KeyValuePair<Tuple<string, Node>, string>> newList = _waitingForParent.OrderBy((KeyValuePair<Tuple<string, Node>, string> x) => x.Key.Item1).ToList();
			while (listCount != newList.Count)
			{
				listCount = newList.Count;
				string tmp = "";
				foreach (KeyValuePair<Tuple<string, Node>, string> kvp in newList)
				{
					if (AddNode(kvp.Key.Item1, kvp.Key.Item2) != null)
					{
						_waitingForParent.TryRemove(kvp.Key, out tmp);
					}
				}
				newList = _waitingForParent.OrderBy((KeyValuePair<Tuple<string, Node>, string> x) => x.Key.Item1).ToList();
			}
		}

		public void Stars(Node noody)
		{
			if (!noody.Description.StartsWith("*"))
			{
				for (int i = 0; i < 6 - noody.Depth; i++)
				{
					noody.Description = "*" + noody.Description;
				}
			}
			if (noody.Parent != null)
			{
				Stars(noody.Parent);
			}
		}

		public void RemoveEmptyNodes2(Node parent = null)
		{
			if (parent == null)
			{
				parent = TreeRoot;
			}
			for (int i = parent.Children.Count - 1; i >= 0; i--)
			{
				Node item = parent.Children[i];
				if (!item.HasValueChildren && item.Type == NodeType.Structure)
				{
					parent.Children.Remove(item);
				}
				else if (parent.Type == NodeType.Structure)
				{
					RemoveEmptyNodes2(item);
				}
			}
		}

		public List<Tuple<string, string>> GetParameters()
		{
			List<Tuple<string, string>> res = (from p in Parameters
				where !string.IsNullOrWhiteSpace(p.Item2)
				select p into param
				select new Tuple<string, string>(GetParameterDescription(param.Item1), param.Item2)).ToList();
			res.Add(new Tuple<string, string>(Resources.CreationDate, DateTime.Now.ToString(CultureInfo.InvariantCulture)));
			return res;
		}

		public double ReturnValue(string description, int id)
		{
			Node currentNode = _nodes[NodeType.Structure].Values.FirstOrDefault((Node n) => n.Description != null && n.Description.Contains(description));
			return ReturnValue(currentNode, id);
		}

		public double ReturnValue(Node node, int id)
		{
			return node?.Value(id) ?? 0.0;
		}

		public bool RemoveEmptyNodes(Node node)
		{
			try
			{
				if (node == null || node == TreeRoot)
				{
					for (int j = TreeRoot.Children.Count - 1; j >= 0; j--)
					{
						if (RemoveEmptyNodes(TreeRoot.Children[j]))
						{
							TreeRoot.Children.RemoveAt(j);
						}
					}
					return false;
				}
				if (AllValuesEmpty(node))
				{
					bool everChildRemoved = true;
					for (int k = node.Children.Count - 1; k >= 0; k--)
					{
						if (RemoveEmptyNodes(node.Children[k]))
						{
							node.Children.Remove(node.Children[k]);
						}
						else
						{
							everChildRemoved = false;
						}
					}
					if (everChildRemoved)
					{
						_nodes[NodeType.Structure].Remove(node.Key);
					}
					return everChildRemoved;
				}
				for (int i = node.Children.Count - 1; i >= 0; i--)
				{
					if (RemoveEmptyNodes(node.Children[i]))
					{
						node.Children.Remove(node.Children[i]);
					}
				}
				return false;
			}
			catch
			{
				return false;
			}
		}

		public bool AllValuesEmpty(Node node)
		{
			if (!node.HasValueChildren)
			{
				return true;
			}
			double sum = 0.0;
			for (int i = 0; i < MaximumValueCount; i++)
			{
				sum += Math.Abs(ReturnValue(node, i));
			}
			if (sum > 0.0)
			{
				return false;
			}
			return true;
		}

		public string PositionByName(string description)
		{
			Node currentNode = _nodes[NodeType.Structure].Values.FirstOrDefault((Node n) => n.Description != null && n.Description.Contains(description));
			return currentNode.Key;
		}

		public static string GetRelativeDiff(string item1, string item2)
		{
			double.TryParse(item1, out var BETRAG);
			double.TryParse(item2, out var BETRAG_VJ);
			if (BETRAG_VJ == 0.0)
			{
				return "n/m";
			}
			if (BETRAG == 0.0)
			{
				return "-100.00";
			}
			double REL_ABW = Math.Round(100.0 * (BETRAG - BETRAG_VJ) / BETRAG_VJ, 2);
			if (double.IsNaN(REL_ABW))
			{
				return "n/m";
			}
			return REL_ABW.ToString("#,0.00");
		}
	}
}
