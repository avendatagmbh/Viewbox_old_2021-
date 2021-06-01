using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SystemDb;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class DcwBalanceModel : ViewboxModel
	{
		public enum DcwNodeType
		{
			Structure,
			Account
		}

		public enum DcwNodeSubType
		{
			SachKonto,
			HauptKonten,
			UnterKonten,
			MandtSplit
		}

		public enum DcwNodeOrdering
		{
			None,
			Alphabetical,
			Roman,
			Numeric
		}

		public class DcwNode
		{
			private List<DcwNode> _children = new List<DcwNode>();

			private double _value = 0.0;

			private double _value2 = 0.0;

			public string Key { get; private set; }

			public DcwNodeType Type { get; private set; }

			public DcwNodeSubType SubType { get; private set; }

			public DcwNode Parent { get; internal set; }

			public string Description { get; set; }

			public DcwNodeOrdering OrderingType { get; set; }

			public string OrderingText { get; set; }

			public string SumFrom { get; private set; }

			public string SumTo { get; private set; }

			public int Sign { get; private set; }

			public List<DcwNode> Children
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

			public double Value
			{
				get
				{
					return _value;
				}
				internal set
				{
					if (value != _value && Parent != null)
					{
						Parent.Value += value - _value;
						_value = value;
					}
				}
			}

			public double Value2
			{
				get
				{
					return _value2;
				}
				internal set
				{
					if (value != _value2 && Parent != null)
					{
						Parent.Value2 += value - _value2;
						_value2 = value;
					}
				}
			}

			public bool HasValueChildren => Type == DcwNodeType.Account || Children.Any((DcwNode w) => w.HasValueChildren);

			internal DcwNode(string key, DcwNodeType type, DcwNodeSubType subtype, string sumFrom, string sumTo, int sign, DcwNode parent = null)
			{
				Key = key;
				Parent = parent;
				Type = type;
				SubType = subtype;
				SumFrom = sumFrom;
				SumTo = sumTo;
				Sign = sign;
				parent?._children.Add(this);
			}

			public void ResetValues(double val1, double val2)
			{
				_value = val1;
				_value2 = val2;
			}
		}

		private Dictionary<DcwNodeType, Dictionary<string, DcwNode>> _nodes = new Dictionary<DcwNodeType, Dictionary<string, DcwNode>>();

		private readonly DcwNode _treeRoot;

		private DcwNode _notAssigned;

		public bool HasVJahr { get; set; }

		public bool HasPer { get; set; }

		public bool HasBis { get; set; }

		public bool HasVon { get; set; }

		public bool HasMandtSplit { get; set; }

		public string RefMandt { get; set; }

		public string Mandt { get; set; }

		public string MandtKreis { get; set; }

		public string SaldoVon { get; set; }

		public string SaldoBis { get; set; }

		public string SaldoPer { get; set; }

		public string Currency { get; set; }

		public List<Tuple<IParameter, string>> Parameters { get; private set; }

		public int Version { get; set; }

		public string ShowVJahr { get; set; }

		public string ShowLevel { get; set; }

		public string DetHK { get; set; }

		public DcwNode TreeRoot => _treeRoot;

		public DcwNode NotAssigned
		{
			get
			{
				return _notAssigned;
			}
			set
			{
				_notAssigned = value;
			}
		}

		public override string LabelCaption => "Dcw Bilanz";

		public DcwBalanceModel()
		{
			_nodes.Add(DcwNodeType.Structure, new Dictionary<string, DcwNode>());
			_nodes.Add(DcwNodeType.Account, new Dictionary<string, DcwNode>());
			_treeRoot = new DcwNode("", DcwNodeType.Structure, DcwNodeSubType.SachKonto, "", "", 1);
			_nodes[_treeRoot.Type].Add(_treeRoot.Key, _treeRoot);
			Parameters = new List<Tuple<IParameter, string>>();
		}

		public DcwNode AddNode(string id, string parent, string description, DcwNodeOrdering orderingType, string sumFrom, string sumTo, int sign)
		{
			if (parent == null)
			{
				parent = "";
			}
			if (_nodes[DcwNodeType.Structure].ContainsKey(parent))
			{
				DcwNode parentNode = _nodes[DcwNodeType.Structure][parent];
				DcwNode node = new DcwNode(id, DcwNodeType.Structure, DcwNodeSubType.SachKonto, sumFrom, sumTo, sign, parentNode)
				{
					Description = description,
					OrderingType = orderingType
				};
				_nodes[DcwNodeType.Structure][id] = node;
				return node;
			}
			return null;
		}

		public void AddNode(string id, DcwNode node)
		{
			_nodes[DcwNodeType.Structure][id] = node;
		}

		public DcwNode FindNode(string id, DcwNodeType type)
		{
			return _nodes[type].ContainsKey(id) ? _nodes[type][id] : null;
		}

		public DcwNode AddAccount(DcwNode parent, DcwNodeSubType subtype, string key, string description, double value, double value2, int sign = 1)
		{
			DcwNode node = new DcwNode(key, DcwNodeType.Account, subtype, "", "", sign, parent)
			{
				Description = description
			};
			node.Value = value;
			node.Value2 = value2;
			_nodes[DcwNodeType.Account][key] = node;
			return node;
		}

		public void ClearEmpty(DcwNode node)
		{
			foreach (DcwNode child in node.Children.Where((DcwNode w) => w.Type == DcwNodeType.Structure).ToList())
			{
				ClearEmpty(child);
			}
			if (!node.HasValueChildren && string.IsNullOrEmpty(node.SumFrom) && string.IsNullOrEmpty(node.SumTo))
			{
				_nodes[DcwNodeType.Structure].Remove(node.Key);
				if (node.Parent != null)
				{
					node.Parent.Children.Remove(node);
				}
			}
		}

		public void CreateSums()
		{
			foreach (KeyValuePair<string, DcwNode> node in _nodes[DcwNodeType.Structure].Where((KeyValuePair<string, DcwNode> w) => !string.IsNullOrEmpty(w.Value.SumFrom) && !string.IsNullOrEmpty(w.Value.SumTo)))
			{
				List<KeyValuePair<string, DcwNode>> nodes = _nodes[DcwNodeType.Structure].Where((KeyValuePair<string, DcwNode> w) => string.IsNullOrEmpty(w.Value.SumFrom) && string.IsNullOrEmpty(w.Value.SumTo) && string.Compare(w.Value.Key, node.Value.SumFrom, ignoreCase: true) >= 0 && string.Compare(w.Value.Key, node.Value.SumTo, ignoreCase: true) <= 0 && w.Value.Parent == node.Value.Parent).ToList();
				node.Value.Value = nodes.Sum((KeyValuePair<string, DcwNode> w) => w.Value.Value);
				node.Value.Value2 = nodes.Sum((KeyValuePair<string, DcwNode> w) => w.Value.Value2);
			}
		}

		public void ClearLevels(DcwNode node, string showLevel, string showMandt)
		{
			if (!string.IsNullOrEmpty(node.SumFrom) || !string.IsNullOrEmpty(node.SumTo))
			{
				return;
			}
			if ((showLevel == "s" && node.Type == DcwNodeType.Account) || (showLevel == "h" && node.SubType == DcwNodeSubType.UnterKonten) || (showMandt == "n" && node.SubType == DcwNodeSubType.MandtSplit))
			{
				if (showMandt == "j" && showLevel == "h")
				{
					List<DcwNode> children = node.Children;
					DcwNode parent = node.Parent;
					foreach (DcwNode child2 in children)
					{
						child2.Parent = parent;
						parent.Children.Add(child2);
					}
					node.Children = null;
					var duplicateKeys = from c in parent.Children
						group c by c.Key into g
						select new
						{
							Key = g.Key,
							Count = g.Count()
						} into c
						where c.Count > 1
						select c;
					foreach (string key in duplicateKeys.Select(k => k.Key))
					{
						IEnumerable<DcwNode> nodesToMerge = parent.Children.Where((DcwNode c) => c.Key == key);
						DcwNode sampleNode = nodesToMerge.FirstOrDefault();
						DcwNode mergedNode = new DcwNode(sampleNode.Key, sampleNode.Type, sampleNode.SubType, sampleNode.SumFrom, sampleNode.SumTo, sampleNode.Sign);
						mergedNode.Description = sampleNode.Description;
						double val1 = nodesToMerge.Sum((DcwNode c) => c.Value);
						double val2 = nodesToMerge.Sum((DcwNode c) => c.Value2);
						mergedNode.ResetValues(val1, val2);
						parent.Children.RemoveAll((DcwNode n) => n.Key == key);
						parent.Children.Add(mergedNode);
						mergedNode.Parent = parent;
					}
				}
				_nodes[DcwNodeType.Structure].Remove(node.Key);
				if (node.Parent != null)
				{
					node.Parent.Children.Remove(node);
				}
				return;
			}
			foreach (DcwNode child in node.Children.ToList())
			{
				ClearLevels(child, showLevel, showMandt);
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
	}
}
