using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using SystemDb;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class UniversalTableModel : SimpleTableModel
	{
		public enum NodeType
		{
			Structure,
			Account
		}

		public class Node
		{
			public class DataStruct
			{
				public enum DataType
				{
					Field,
					Value
				}

				public int ordinal;

				public string field;

				public double value;

				public DataType dataType;

				public DataStruct(int o, string f, double v, DataType d)
				{
					ordinal = o;
					field = f;
					value = v;
					dataType = d;
				}

				public object GetData()
				{
					return dataType switch
					{
						DataType.Field => field, 
						DataType.Value => value, 
						_ => null, 
					};
				}
			}

			private List<DataStruct> _data = new List<DataStruct>();

			private List<Node> _children = new List<Node>();

			public Dictionary<int, string> Fields { get; set; }

			public Dictionary<int, double> Values { get; set; }

			public string Id { get; private set; }

			public string Description { get; set; }

			public NodeType Type { get; private set; }

			public int Ordinal { get; set; }

			public Node Parent { get; internal set; }

			public List<DataStruct> Data => _data;

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
					if (Parent == null)
					{
						return 0;
					}
					return Parent.Depth + 1;
				}
			}

			public bool HasValueChildren => Type == NodeType.Account || Children.Any((Node w) => w.HasValueChildren);

			internal Node(string id, string description, NodeType type, Dictionary<int, string> fields, Dictionary<int, double> values, int ordinal, Node parent = null)
			{
				Id = id;
				Description = description;
				Parent = parent;
				Type = type;
				Fields = fields;
				Values = values;
				int ordinalCounter = 0;
				if (values != null && fields != null)
				{
					for (int i = Math.Min((fields != null && fields.Count != 0) ? fields.Keys.Min() : 0, (values != null && values.Count != 0) ? values.Keys.Min() : 0); i <= Math.Max((fields != null && fields.Count != 0) ? fields.Keys.Max() : 0, (values != null && values.Count != 0) ? values.Keys.Max() : 0); i++)
					{
						if (fields.ContainsKey(i))
						{
							_data.Add(new DataStruct(ordinalCounter, fields[i], 0.0, DataStruct.DataType.Field));
							ordinalCounter++;
						}
						else if (values.ContainsKey(i))
						{
							_data.Add(new DataStruct(ordinalCounter, "", values[i], DataStruct.DataType.Value));
							ordinalCounter++;
						}
					}
				}
				Ordinal = ordinal;
				if (parent == null)
				{
					return;
				}
				for (int j = 0; j < parent._children.Count; j++)
				{
					if (parent._children[j].Ordinal > ordinal)
					{
						parent._children.Insert(j, this);
						return;
					}
				}
				parent._children.Add(this);
			}

			internal void AddValue(int id, double value)
			{
				if (Type == NodeType.Structure && Data.Count > id && Data[id].dataType == DataStruct.DataType.Value)
				{
					Data[id].value += value;
				}
				if (Parent != null)
				{
					Parent.AddValue(id, value);
				}
			}

			public double Value(int id)
			{
				if (Type == NodeType.Account && (_data == null || _data.Count < id || _data.Count == 0) && _data[id].dataType == DataStruct.DataType.Field)
				{
					return 0.0;
				}
				if (Type == NodeType.Account)
				{
					return (double)_data[id].GetData();
				}
				return GetChildrenValues(id);
			}

			private double GetChildrenValues(int id)
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

		private Dictionary<int, string> DummyFields = new Dictionary<int, string>();

		private Dictionary<int, double> DummyValues = new Dictionary<int, double>();

		private int _maxValueCount = 0;

		private readonly Node _treeRoot;

		public string KontoColumnName;

		public List<IColumn> Columns = new List<IColumn>();

		private bool IsOrdered = false;

		public bool IsDataGridNeeded = false;

		public bool HideEmptyNodes = true;

		public int MaxPrintedLevel = 0;

		private Bitmap objBitmap;

		private Graphics objGraphics;

		private List<double> columnWidhts;

		public int MaxDepth = 0;

		public int MaximumValueCount => _maxValueCount;

		public new List<Tuple<IParameter, string, string>> Parameters { get; private set; }

		public Node TreeRoot => _treeRoot;

		public ReadOnlyCollection<double> ColumnWidths => columnWidhts.AsReadOnly();

		public override string LabelCaption => "Tree view";

		public UniversalTableModel()
		{
			_nodes.Add(NodeType.Structure, new Dictionary<string, Node>());
			_nodes.Add(NodeType.Account, new Dictionary<string, Node>());
			_treeRoot = new Node("0", "", NodeType.Structure, null, null, 0);
			_nodes[_treeRoot.Type].Add(_treeRoot.Id, _treeRoot);
			Parameters = new List<Tuple<IParameter, string, string>>();
			KontoColumnName = Resources.Konto_structure;
		}

		public List<Tuple<string, string>> GetParameters(ILanguage language)
		{
			List<Tuple<string, string>> res = (from p in Parameters
				where !string.IsNullOrWhiteSpace(p.Item2)
				select p into param
				select new Tuple<string, string>(GetParameterDescription(param.Item1, language), param.Item2)).ToList();
			res.Add(new Tuple<string, string>(Resources.CreationDate, DateTime.Now.ToString(CultureInfo.InvariantCulture)));
			return res;
		}

		public List<Tuple<string, string>> GetParameters()
		{
			return GetParameters(ViewboxSession.Language);
		}

		public void SetDummyData(Dictionary<int, string> fields, Dictionary<int, double> values)
		{
			if (fields != null)
			{
				foreach (KeyValuePair<int, string> item2 in fields)
				{
					DummyFields.Add(item2.Key, "");
				}
			}
			if (values == null)
			{
				return;
			}
			foreach (KeyValuePair<int, double> item in values)
			{
				DummyValues.Add(item.Key, 0.0);
			}
		}

		public Node AddNode(string id, string parent, string description, int ordinal, Dictionary<int, string> fields = null, Dictionary<int, double> values = null)
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
					description = parentNode.Description;
				}
				Node node = new Node(id, description, NodeType.Structure, (fields == null) ? DummyFields : fields, (values == null) ? DummyValues : values, ordinal, parentNode);
				_nodes[NodeType.Structure][id] = node;
				return node;
			}
			if (!IsOrdered)
			{
				Node node2 = new Node(id, description, NodeType.Structure, (fields == null) ? DummyFields : fields, (values == null) ? DummyValues : values, ordinal);
				Tuple<string, Node> tupi = new Tuple<string, Node>(parent, node2);
				_waitingForParent.TryAdd(tupi, id);
			}
			return null;
		}

		public Node AddLeaf(string id, string parent, string description, Dictionary<int, string> fields, Dictionary<int, double> values, int ordinal)
		{
			if (!IsOrdered)
			{
				ReOrderNodes();
			}
			if (parent == null)
			{
				parent = "";
			}
			if (values != null && fields != null && values.Count + fields.Count > 0)
			{
				_maxValueCount = Math.Max(_maxValueCount, values.Count + fields.Count);
			}
			if (_nodes[NodeType.Structure].ContainsKey(parent))
			{
				Node parentNode = _nodes[NodeType.Structure][parent];
				Node node2 = new Node(id, description, NodeType.Account, fields, values, ordinal, parentNode);
				_nodes[NodeType.Structure][id] = node2;
				return node2;
			}
			if (_nodes[NodeType.Account].ContainsKey(parent))
			{
				Node parentNode2 = _nodes[NodeType.Account][parent];
				Node node3 = new Node(id, description, NodeType.Account, fields, values, ordinal, parentNode2);
				_nodes[NodeType.Structure][id] = node3;
				return node3;
			}
			Node node = new Node(id, description, NodeType.Account, fields, values, ordinal);
			Tuple<string, Node> tupi = new Tuple<string, Node>(parent, node);
			_waitingForParent.TryAdd(tupi, id);
			return null;
		}

		public void ReOrderNodes()
		{
			IsOrdered = true;
			int listCount = -1;
			List<KeyValuePair<Tuple<string, Node>, string>> newList = (from x in _waitingForParent.ToList()
				orderby x.Key.Item1, Number(x.Value)
				select x).ToList();
			while (listCount != newList.Count)
			{
				listCount = newList.Count;
				string tmp = "";
				foreach (KeyValuePair<Tuple<string, Node>, string> kvp in newList)
				{
					if (AddNode(kvp.Key.Item2.Id, kvp.Key.Item1, kvp.Key.Item2.Description, kvp.Key.Item2.Ordinal, kvp.Key.Item2.Fields, kvp.Key.Item2.Values) != null)
					{
						_waitingForParent.TryRemove(kvp.Key, out tmp);
					}
				}
				newList = (from x in _waitingForParent.ToList()
					orderby x.Key.Item1, Number(x.Value)
					select x).ToList();
			}
		}

		private int Number(string str)
		{
			if (int.TryParse(str, out var result))
			{
				return result;
			}
			return 0;
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

		private double GetWidthOfString(string str, bool header = false)
		{
			return Math.Ceiling(objGraphics.MeasureString(str, header ? new Font("Arial", 12f, FontStyle.Bold) : new Font("Arial", 12f)).Width);
		}

		public void GenerateColumnWidths()
		{
			columnWidhts = new List<double>();
			objBitmap = new Bitmap(800, 200);
			objGraphics = Graphics.FromImage(objBitmap);
			columnWidhts.Add(GetWidthOfString(KontoColumnName, header: true));
			foreach (IColumn col in Columns)
			{
				columnWidhts.Add(GetWidthOfString(col.GetDescription(), header: true));
			}
			Stack<Node> nodes = new Stack<Node>();
			for (int j = TreeRoot.Children.Count - 1; j >= 0; j--)
			{
				nodes.Push(TreeRoot.Children[j]);
			}
			while (nodes.Count > 0)
			{
				Node node = nodes.Pop();
				if (node.Depth > MaxDepth)
				{
					MaxDepth = node.Depth;
				}
				double width = GetWidthOfString(node.Description);
				if (width > ColumnWidths[0])
				{
					columnWidhts[0] = width;
				}
				int i = 1;
				foreach (Node.DataStruct data in node.Data)
				{
					width = ((data.dataType != 0) ? GetWidthOfString(data.value.ToString("N")) : GetWidthOfString(data.field));
					if (width > ColumnWidths[i])
					{
						columnWidhts[i] = width;
					}
					i++;
				}
				for (i = node.Children.Count - 1; i >= 0; i--)
				{
					nodes.Push(node.Children[i]);
				}
			}
			objGraphics.Dispose();
			objBitmap.Dispose();
		}
	}
}
