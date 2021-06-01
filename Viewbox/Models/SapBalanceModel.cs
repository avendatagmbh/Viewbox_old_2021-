using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using SystemDb;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class SapBalanceModel : SimpleTableModel
	{
		public enum AccountType
		{
			Inherit,
			Creditor,
			Debitor,
			Fixed
		}

		public enum StructureType
		{
			MainNode,
			RangeNode,
			AccountNode,
			AccountLeaf
		}

		public class Node
		{
			private readonly List<Node> _children = new List<Node>();

			private string _description = string.Empty;

			private decimal _value = default(decimal);

			private decimal _value_vj = default(decimal);

			private decimal _value_abs = default(decimal);

			private decimal _value_rel = default(decimal);

			private bool _hasValueChildren = false;

			public Node ParentGroup { get; set; }

			public string AdditionalInformation { get; set; }

			public bool SumAndAddToBalance { get; set; }

			public Node Parent { get; private set; }

			public IEnumerable<Node> Children => _children;

			public string Description
			{
				get
				{
					return _description;
				}
				internal set
				{
					_description = value;
				}
			}

			public string GesBer { get; set; }

			public decimal Value
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

			public decimal Value_VJ
			{
				get
				{
					return _value_vj;
				}
				internal set
				{
					if (value != _value_vj && Parent != null)
					{
						Parent.Value_VJ += value - _value_vj;
						_value_vj = value;
					}
				}
			}

			public decimal Value_ABS
			{
				get
				{
					return _value_abs;
				}
				internal set
				{
					if (value != _value_abs && Parent != null)
					{
						Parent.Value_ABS += value - _value_abs;
						_value_abs = value;
					}
				}
			}

			public decimal Value_REL
			{
				get
				{
					if (_value_rel != 0m)
					{
						return _value_rel;
					}
					if (_value_vj == 0m)
					{
						return 0m;
					}
					_value_rel = (decimal)((!(Value_ABS < 0m)) ? 1 : (-1)) * Math.Abs(Math.Round(100m * (_value - _value_vj) / _value_vj, 1));
					return _value_rel;
				}
				internal set
				{
					_value_rel = value;
				}
			}

			public bool HasValueChildren
			{
				get
				{
					return _hasValueChildren;
				}
				internal set
				{
					if (value != _hasValueChildren && Parent != null)
					{
						Parent.HasValueChildren |= value;
						_hasValueChildren = value;
						if (!Parent.Children.Any((Node x) => x.HasValueChildren) && !_hasValueChildren)
						{
							Parent.HasValueChildren = false;
						}
					}
				}
			}

			public AccountType AccountType { get; set; }

			public StructureType Type { get; internal set; }

			internal Node(Node parent = null, Node parentGroup = null, StructureType type = StructureType.MainNode, string additionalInformation = null, bool sumAndAddToBalance = false)
			{
				Parent = parent;
				if (Parent != null)
				{
					Parent._children.Add(this);
				}
				Type = type;
				ParentGroup = parentGroup;
				AdditionalInformation = additionalInformation;
				SumAndAddToBalance = sumAndAddToBalance;
			}

			public void RemoveChildren(Node children)
			{
				Value -= children.Value;
				_children.Remove(children);
				if (_children.Count == 0)
				{
					HasValueChildren = false;
				}
			}
		}

		private class NonOverlappingInterval : IComparable
		{
			private class Key : IComparable
			{
				public string Prefix { get; private set; }

				public ulong Number { get; private set; }

				public string Suffix { get; private set; }

				public Key(string raw)
				{
					if (string.IsNullOrEmpty(raw) || ".".Equals(raw))
					{
						Prefix = raw;
						Suffix = "";
						return;
					}
					Prefix = new Regex("^([a-zA-Z.,-/(/)]*)").Match(raw).Value;
					Regex regex = new Regex("(\\d+)");
					Number = 0uL;
					if (regex.IsMatch(raw))
					{
						Number = ulong.Parse(regex.Match(raw).Value);
					}
					string splitted = raw;
					if (Number != 0L && raw.IndexOf(Number.ToString()) > -1)
					{
						splitted = raw.Substring(raw.IndexOf(Number.ToString()) + Number.ToString().Length);
					}
					Suffix = new Regex("[/\\-]([0-9a-zA-Z.,-/(/)]+)$").Match(splitted).Value;
					if (string.IsNullOrEmpty(Suffix))
					{
						Suffix = ((regex.IsMatch(splitted) && regex.Match(splitted).Groups.Count == 1) ? new Regex("([a-zA-Z.,-/(/)]+)$").Match(splitted).Value : new Regex("([0-9a-zA-Z.,-/(/)]+)$").Match(splitted).Value);
					}
					if (string.IsNullOrEmpty(Prefix) && raw.IndexOf(Number.ToString()) != 0)
					{
						string prefix = raw.Substring(0, raw.IndexOf(Number.ToString()));
						string compare = prefix.Aggregate(string.Empty, (string current, char t) => current + "0");
						if (prefix != compare)
						{
							Prefix = prefix;
						}
					}
				}

				public int CompareTo(object obj)
				{
					Key key = obj as Key;
					if (key == null)
					{
						throw new ArgumentException();
					}
					int cmp = Prefix.CompareTo(key.Prefix);
					if (cmp != 0)
					{
						return cmp;
					}
					cmp = Number.CompareTo(key.Number);
					if (cmp != 0)
					{
						return cmp;
					}
					return Suffix.CompareTo(key.Suffix);
				}
			}

			private Key Min { get; set; }

			private Key Max { get; set; }

			public NonOverlappingInterval(string min, string max)
			{
				Key k_min = new Key(min);
				Key k_max = new Key(max);
				int cmp = k_min.CompareTo(k_max);
				Min = ((cmp <= 0) ? k_min : k_max);
				Max = ((cmp <= 0) ? k_max : k_min);
			}

			public override bool Equals(object obj)
			{
				return CompareTo(obj) == 0;
			}

			public override int GetHashCode()
			{
				return string.Concat(Min, "_", Max).GetHashCode();
			}

			public int CompareTo(object obj)
			{
				NonOverlappingInterval other = obj as NonOverlappingInterval;
				if (other == null)
				{
					throw new ArgumentException();
				}
				if (other.Min.CompareTo(Max) > 0)
				{
					return -1;
				}
				if (other.Max.CompareTo(Min) < 0)
				{
					return 1;
				}
				if (other.Min.CompareTo(Min) == 0 && other.Max.CompareTo(Max) == 0)
				{
					return 0;
				}
				if (other.Min.CompareTo(Min) >= 0 && other.Max.CompareTo(Max) <= 0)
				{
					return -1;
				}
				if (Min.CompareTo(other.Min) >= 0 && Max.CompareTo(other.Max) <= 0)
				{
					return 1;
				}
				throw new ArgumentException("OverlappingInterval");
			}

			public int CompareTo(string value)
			{
				Key key = new Key(value);
				if (key.CompareTo(Max) > 0)
				{
					return -1;
				}
				if (key.CompareTo(Min) < 0)
				{
					return 1;
				}
				return 0;
			}
		}

		private Dictionary<int, Node> _nodes = new Dictionary<int, Node> { 
		{
			0,
			new Node()
		} };

		private Dictionary<AccountType, SortedDictionary<NonOverlappingInterval, Node>> _ranges = new Dictionary<AccountType, SortedDictionary<NonOverlappingInterval, Node>>
		{
			{
				AccountType.Creditor,
				new SortedDictionary<NonOverlappingInterval, Node>()
			},
			{
				AccountType.Debitor,
				new SortedDictionary<NonOverlappingInterval, Node>()
			},
			{
				AccountType.Fixed,
				new SortedDictionary<NonOverlappingInterval, Node>()
			}
		};

		public bool SplitByGesber = false;

		private bool enableEveryNodeToMove = true;

		private Dictionary<Node, List<Tuple<string, string, decimal>>> _nodesToAssign = new Dictionary<Node, List<Tuple<string, string, decimal>>>();

		public List<Node> AllNodes
		{
			get
			{
				if (_nodes != null)
				{
					return _nodes.Values.ToList();
				}
				return new List<Node>();
			}
		}

		public Node UnassignedAccounts { get; set; }

		public string Currency { get; set; }

		public new List<Tuple<IParameter, string>> Parameters { get; private set; }

		public string AccountStructure { get; set; }

		public Node TreeRoot => _nodes[0];

		public override string LabelCaption => "SAP Bilanz";

		public int Flag { get; set; }

		public SapBalanceModel()
		{
			Parameters = new List<Tuple<IParameter, string>>();
		}

		public Node AddNode(int id, int parent, int parentGroup, string description, AccountType type, StructureType stype, string additionalInformation, bool sumAndAddToBalance, string gesBer = "")
		{
			if (_nodes.ContainsKey(parent))
			{
				Node node = new Node(_nodes[parent], null, stype, additionalInformation, sumAndAddToBalance)
				{
					Description = description,
					AccountType = ((type == AccountType.Inherit) ? _nodes[parent].AccountType : type)
				};
				_nodes[id] = node;
				node.ParentGroup = _nodes[parentGroup];
				return node;
			}
			return null;
		}

		private SortedDictionary<NonOverlappingInterval, Node> ChooseRange(AccountType type)
		{
			if (type == AccountType.Inherit)
			{
				type = AccountType.Fixed;
			}
			return _ranges[type];
		}

		public void AddRange(Node node, string from, string to = "0")
		{
			SortedDictionary<NonOverlappingInterval, Node> range = ChooseRange(node.AccountType);
			NonOverlappingInterval key = ((to != "0") ? new NonOverlappingInterval(from, to) : new NonOverlappingInterval(from, from));
			if (range.ContainsKey(key))
			{
				range.Remove(key);
			}
			range.Add(key, node);
		}

		private Node Find2(string account, decimal value)
		{
			SortedDictionary<NonOverlappingInterval, Node> ranges = ChooseRange(AccountType.Fixed);
			NonOverlappingInterval range = ranges.Keys.FirstOrDefault((NonOverlappingInterval i) => i.CompareTo(account) == 0);
			if (range != null)
			{
				return ranges[range];
			}
			SortedDictionary<NonOverlappingInterval, Node> creditRanges = ChooseRange(AccountType.Creditor);
			NonOverlappingInterval creditRange = creditRanges.Keys.FirstOrDefault((NonOverlappingInterval i) => i.CompareTo(account) == 0);
			SortedDictionary<NonOverlappingInterval, Node> debitRanges = ChooseRange(AccountType.Debitor);
			NonOverlappingInterval debitRange = debitRanges.Keys.FirstOrDefault((NonOverlappingInterval i) => i.CompareTo(account) == 0);
			if (creditRange != null && value <= 0m)
			{
				return creditRanges[creditRange];
			}
			if (debitRange != null && value >= 0m)
			{
				return debitRanges[debitRange];
			}
			return UnassignedAccounts;
		}

		private Node Find(AccountType type, string account)
		{
			SortedDictionary<NonOverlappingInterval, Node> ranges = ChooseRange(type);
			NonOverlappingInterval range = ranges.Keys.FirstOrDefault((NonOverlappingInterval i) => i.CompareTo(account) == 0);
			if (range == null)
			{
				return UnassignedAccounts;
			}
			return ranges[range] ?? UnassignedAccounts;
		}

		public Node AddAccount(string account, string description, decimal value, string gesBer, decimal value_vj = 0m, decimal value_abs = 0m, decimal value_rel = 0m)
		{
			Node parent = Find2(account, value);
			return (parent == null) ? null : new Node(parent, null, StructureType.AccountLeaf)
			{
				Description = description,
				Value = value,
				HasValueChildren = (value != 0m || value_vj != 0m),
				GesBer = gesBer,
				Value_VJ = value_vj,
				Value_ABS = value_abs,
				Value_REL = value_rel
			};
		}

		public Node AddAccount(AccountType type, string account, string description, decimal value, string gesBer, decimal value_vj = 0m, decimal value_abs = 0m, decimal value_rel = 0m)
		{
			return new Node(Find(type, account), null, StructureType.AccountLeaf)
			{
				Description = description,
				Value = value,
				HasValueChildren = (value != 0m || value_vj != 0m),
				GesBer = gesBer,
				Value_VJ = value_vj,
				Value_ABS = value_abs,
				Value_REL = value_rel
			};
		}

		public Node AddAccount(Node parent, string description, decimal value, string gesBer, decimal value_vj = 0m, decimal value_abs = 0m, decimal value_rel = 0m)
		{
			return new Node(parent, null, StructureType.AccountLeaf)
			{
				Description = description,
				Value = value,
				HasValueChildren = (value != 0m || value_vj != 0m),
				GesBer = gesBer,
				Value_VJ = value_vj,
				Value_ABS = value_abs,
				Value_REL = value_rel
			};
		}

		public Node AddAccount(Node parent, Node noody)
		{
			return AddAccount(parent, noody.Description, noody.Value, noody.GesBer);
		}

		public Node AddSubTree(Node parent, Node subTree)
		{
			Node returnNode = new Node(parent, null, subTree.Type)
			{
				Description = subTree.Description,
				Value = ((subTree.Type == StructureType.AccountLeaf) ? subTree.Value : 0m),
				HasValueChildren = (subTree.Value != 0m)
			};
			List<Node> children = (subTree.Children as List<Node>).Where((Node x) => x.Value != 0m).ToList();
			for (int i = 0; i < children.Count(); i++)
			{
				AddSubTree(returnNode, children[i]);
			}
			return returnNode;
		}

		public void CompleteAssignment()
		{
			foreach (KeyValuePair<Node, List<Tuple<string, string, decimal>>> node in _nodesToAssign)
			{
				decimal sum = node.Value.Sum((Tuple<string, string, decimal> n) => n.Item3);
				if ((!(sum < 0m) || node.Key.AccountType != AccountType.Creditor) && (!(sum >= 0m) || node.Key.AccountType != AccountType.Debitor))
				{
					continue;
				}
				foreach (Tuple<string, string, decimal> newNode in node.Value)
				{
					AddAccount(node.Key.AccountType, newNode.Item1, newNode.Item2, newNode.Item3, node.Key.GesBer);
				}
			}
		}

		public void InsertAdditionalInformation()
		{
			IEnumerable<KeyValuePair<int, Node>> gewinnNodeKeyValuePairs = _nodes.Where((KeyValuePair<int, Node> n) => n.Value.AdditionalInformation != null && n.Value.AdditionalInformation.ToLower() == "gew" && Math.Abs(n.Value.Value) > 0m);
			foreach (KeyValuePair<int, Node> gewinnNodeKeyValuePair in gewinnNodeKeyValuePairs)
			{
				if (!gewinnNodeKeyValuePair.Equals(default(KeyValuePair<int, Node>)))
				{
					KeyValuePair<int, Node> positiveNodeKeyValuePair = _nodes.FirstOrDefault((KeyValuePair<int, Node> n) => n.Value.AdditionalInformation != null && n.Value.AdditionalInformation.ToLower() == "gep" && n.Value.Description.Contains(gewinnNodeKeyValuePair.Value.Description));
					KeyValuePair<int, Node> negativeNodeKeyValuePair = _nodes.FirstOrDefault((KeyValuePair<int, Node> n) => n.Value.AdditionalInformation != null && n.Value.AdditionalInformation.ToLower() == "gem" && n.Value.Description.Contains(gewinnNodeKeyValuePair.Value.Description));
					if (gewinnNodeKeyValuePair.Value.Value > 0m && gewinnNodeKeyValuePair.Value.Parent == negativeNodeKeyValuePair.Value)
					{
						AddAccount(positiveNodeKeyValuePair.Value, gewinnNodeKeyValuePair.Value.Description, gewinnNodeKeyValuePair.Value.Value, gewinnNodeKeyValuePair.Value.GesBer);
						negativeNodeKeyValuePair.Value.RemoveChildren(gewinnNodeKeyValuePair.Value);
					}
					else if (gewinnNodeKeyValuePair.Value.Value < 0m && gewinnNodeKeyValuePair.Value.Parent == positiveNodeKeyValuePair.Value)
					{
						AddAccount(negativeNodeKeyValuePair.Value, gewinnNodeKeyValuePair.Value.Description, gewinnNodeKeyValuePair.Value.Value, gewinnNodeKeyValuePair.Value.GesBer);
						positiveNodeKeyValuePair.Value.RemoveChildren(gewinnNodeKeyValuePair.Value);
					}
				}
			}
			KeyValuePair<int, Node> guvNodeKeyValuePair = _nodes.FirstOrDefault((KeyValuePair<int, Node> n) => n.Value.AdditionalInformation != null && n.Value.AdditionalInformation.ToLower() == "guv");
			if (guvNodeKeyValuePair.Equals(default(KeyValuePair<int, Node>)))
			{
				return;
			}
			decimal sum = guvNodeKeyValuePair.Value.Value;
			KeyValuePair<int, Node> notAssignedKeyValuePair = _nodes.FirstOrDefault((KeyValuePair<int, Node> n) => !string.IsNullOrEmpty(n.Value.Description) && (n.Value.Description.ToLower().Contains("zugeordnet") || n.Value.Description.ToLower().Contains("not assigned")));
			if (!notAssignedKeyValuePair.Equals(default(KeyValuePair<int, Node>)))
			{
				sum += notAssignedKeyValuePair.Value.Value;
			}
			decimal finanzValue = default(decimal);
			KeyValuePair<int, Node> finanzergebnisKeyValuePair = _nodes.FirstOrDefault((KeyValuePair<int, Node> n) => n.Value.AdditionalInformation != null && n.Value.AdditionalInformation.ToLower() == "f");
			if (!finanzergebnisKeyValuePair.Equals(default(KeyValuePair<int, Node>)))
			{
				finanzValue = finanzergebnisKeyValuePair.Value.Value;
			}
			sum += finanzValue;
			if (sum > 0m)
			{
				KeyValuePair<int, Node> profitKeyValuePair = _nodes.FirstOrDefault((KeyValuePair<int, Node> n) => n.Value.AdditionalInformation != null && n.Value.AdditionalInformation.ToLower() == "s");
				if (profitKeyValuePair.Equals(default(KeyValuePair<int, Node>)))
				{
					profitKeyValuePair = _nodes.FirstOrDefault((KeyValuePair<int, Node> n) => n.Value.AdditionalInformation != null && n.Value.AdditionalInformation.ToLower() == "x");
				}
				if (!profitKeyValuePair.Equals(default(KeyValuePair<int, Node>)))
				{
					AddAccount(profitKeyValuePair.Value, profitKeyValuePair.Value.Description, sum, profitKeyValuePair.Value.GesBer);
				}
			}
			else if (sum < 0m)
			{
				KeyValuePair<int, Node> lossKeyValuePair = _nodes.FirstOrDefault((KeyValuePair<int, Node> n) => n.Value.AdditionalInformation != null && n.Value.AdditionalInformation.ToLower() == "h");
				if (lossKeyValuePair.Equals(default(KeyValuePair<int, Node>)))
				{
					lossKeyValuePair = _nodes.FirstOrDefault((KeyValuePair<int, Node> n) => n.Value.AdditionalInformation != null && n.Value.AdditionalInformation.ToLower() == "x");
				}
				if (!lossKeyValuePair.Equals(default(KeyValuePair<int, Node>)))
				{
					AddAccount(lossKeyValuePair.Value, lossKeyValuePair.Value.Description, sum, lossKeyValuePair.Value.GesBer);
				}
			}
			KeyValuePair<int, Node> gNodeKeyValuePair = _nodes.FirstOrDefault((KeyValuePair<int, Node> n) => n.Value.AdditionalInformation != null && n.Value.AdditionalInformation.ToLower() == "g");
			if (!gNodeKeyValuePair.Equals(default(KeyValuePair<int, Node>)))
			{
				KeyValuePair<int, Node> eNodeKeyValuePair = _nodes.FirstOrDefault((KeyValuePair<int, Node> n) => n.Value.AdditionalInformation != null && n.Value.AdditionalInformation.ToLower() == "e");
				if (!guvNodeKeyValuePair.Equals(default(KeyValuePair<int, Node>)) && !eNodeKeyValuePair.Equals(default(KeyValuePair<int, Node>)))
				{
					Node guvNode = guvNodeKeyValuePair.Value;
					Node eNode = eNodeKeyValuePair.Value;
					AddAccount(gNodeKeyValuePair.Value, gNodeKeyValuePair.Value.Description, -guvNode.Value - eNode.Value - finanzValue, gNodeKeyValuePair.Value.GesBer);
				}
			}
		}

		public void SumGesber()
		{
			ICoreProperty icp = ViewboxApplication.FindCoreProperty("enableEveryNodeToMove");
			if (icp != null && icp.Value.ToString().ToLower() == "false")
			{
				enableEveryNodeToMove = false;
			}
			foreach (Node item in _nodes.Values.Where((Node w) => w.Parent == null))
			{
				SumGesberItem(item);
			}
			icp = ViewboxApplication.FindCoreProperty("Bilanz_Notassigned");
			if (icp != null && UnassignedAccounts != null && UnassignedAccounts.Parent != null && UnassignedAccounts.Value == 0m)
			{
				UnassignedAccounts.Parent.RemoveChildren(UnassignedAccounts);
			}
		}

		public void SumGesberItem(Node parent)
		{
			if ((parent.AccountType == AccountType.Creditor || parent.AccountType == AccountType.Debitor) && (enableEveryNodeToMove || parent.Type != StructureType.RangeNode))
			{
				ICoreProperty icp = ViewboxApplication.FindCoreProperty("useNewBilanzBuilding");
				if (icp != null && icp.Value.ToString().ToLower() == "true")
				{
					UniteBrother2(parent);
				}
				else
				{
					UniteBrother(parent);
				}
			}
			if (parent != null && parent.Description != null && (parent.Description.Contains("X_") || parent.Description.Contains("_X")))
			{
				int startIndex = parent.Description.IndexOf("X_");
				if (startIndex < 0)
				{
					startIndex = parent.Description.IndexOf("_X");
				}
				if (startIndex > 0)
				{
					parent.Description = parent.Description.Remove(startIndex).Trim();
				}
			}
			IOrderedEnumerable<Node> list = from w in parent.Children
				where w.Type == StructureType.AccountLeaf
				orderby w.Description
				select w;
			Node oldItem = null;
			foreach (Node item in list)
			{
				if (oldItem == null || oldItem.Description != item.Description)
				{
					oldItem = item;
					continue;
				}
				if (oldItem.GesBer != item.GesBer && SplitByGesber)
				{
					oldItem = item;
					continue;
				}
				oldItem.Value += item.Value;
				oldItem.HasValueChildren = true;
				item.Parent.RemoveChildren(item);
			}
			foreach (Node child in parent.Children.Where((Node w) => w.Type != StructureType.AccountLeaf))
			{
				SumGesberItem(child);
			}
		}

		private void UniteBrother(Node noody)
		{
			Node brother = FindBrother(noody);
			if (brother == null || brother.Value == 0m || brother.Value == noody.Value)
			{
				return;
			}
			if (Math.Abs(noody.Value) >= Math.Abs(brother.Value))
			{
				List<Node> nephews = (SplitByGesber ? (brother.Children as List<Node>).Where((Node x) => x.GesBer == noody.GesBer).ToList() : (brother.Children as List<Node>));
				for (int i = nephews.Count() - 1; i >= 0; i--)
				{
					if (Math.Abs(nephews[i].Value) > 0m)
					{
						Node exactTarget = null;
						List<Node> brothers2 = (SplitByGesber ? (noody.Children as List<Node>).Where((Node x) => x.GesBer == noody.GesBer).ToList() : (noody.Children as List<Node>));
						for (int m = 0; m < noody.Children.Count(); m++)
						{
							if (GetIdentifier(nephews[i].Description) == GetIdentifier(brothers2[m].Description))
							{
								exactTarget = brothers2[m];
								break;
							}
						}
						if (exactTarget != null)
						{
							brothers2 = (SplitByGesber ? (nephews[i].Children as List<Node>).Where((Node x) => x.GesBer == noody.GesBer).ToList() : (nephews[i].Children as List<Node>));
							List<int> brothersToRemove2 = new List<int>();
							for (int n = 0; n < brothers2.Count; n++)
							{
								if (brothers2[n].Value != 0m)
								{
									AddAccount(exactTarget, brothers2[n].Description, brothers2[n].Value, brothers2[n].GesBer);
									brothers2[n].HasValueChildren = false;
									brothersToRemove2.Add(n);
								}
							}
							while (brothersToRemove2.Count > 0)
							{
								nephews[i].RemoveChildren(brothers2[brothersToRemove2.Last()]);
								brothersToRemove2.RemoveAt(brothersToRemove2.Count - 1);
							}
							if (brothers2.Count == 0 && nephews[i].Type == StructureType.AccountLeaf)
							{
								AddAccount(exactTarget.Parent, nephews[i].Description, nephews[i].Value, nephews[i].GesBer);
							}
						}
						else
						{
							AddSubTree(noody, nephews[i]);
						}
						brother.RemoveChildren(nephews[i]);
					}
				}
				if (brother.Children.Count() == 0 || !brother.Children.Any((Node x) => x.HasValueChildren))
				{
					brother.HasValueChildren = false;
				}
				Node grandpa = brother.Parent;
				while (grandpa != null && grandpa != TreeRoot && grandpa.Value == 0m && grandpa.HasValueChildren)
				{
					grandpa.HasValueChildren = false;
					grandpa = grandpa.Parent;
				}
				return;
			}
			List<Node> nephews2 = (SplitByGesber ? (noody.Children as List<Node>).Where((Node x) => x.GesBer == noody.GesBer).ToList() : (noody.Children as List<Node>));
			for (int j = nephews2.Count() - 1; j >= 0; j--)
			{
				if (Math.Abs(nephews2[j].Value) > 0m)
				{
					Node exactTarget2 = null;
					List<Node> brothers = (SplitByGesber ? (brother.Children as List<Node>).Where((Node x) => x.GesBer == noody.GesBer).ToList() : (brother.Children as List<Node>));
					for (int l = 0; l < brother.Children.Count(); l++)
					{
						if (GetIdentifier(nephews2[j].Description) == GetIdentifier(brothers[l].Description))
						{
							exactTarget2 = brothers[l];
							break;
						}
					}
					if (exactTarget2 != null)
					{
						brothers = (SplitByGesber ? (nephews2[j].Children as List<Node>).Where((Node x) => x.GesBer == noody.GesBer).ToList() : (nephews2[j].Children as List<Node>));
						List<int> brothersToRemove = new List<int>();
						for (int k = 0; k < brothers.Count; k++)
						{
							if (brothers[k].Value != 0m)
							{
								AddAccount(exactTarget2, brothers[k].Description, brothers[k].Value, brothers[k].GesBer);
								brothers[k].HasValueChildren = false;
								brothersToRemove.Add(k);
							}
						}
						while (brothersToRemove.Count > 0)
						{
							nephews2[j].RemoveChildren(brothers[brothersToRemove.Last()]);
							brothersToRemove.RemoveAt(brothersToRemove.Count - 1);
						}
						if (brothers.Count == 0 && nephews2[j].Type == StructureType.AccountLeaf)
						{
							AddAccount(exactTarget2.Parent, nephews2[j].Description, nephews2[j].Value, nephews2[j].GesBer);
						}
					}
					else
					{
						AddSubTree(noody, nephews2[j]);
					}
					noody.RemoveChildren(nephews2[j]);
				}
			}
			if (noody.Children.Count() == 0 || !noody.Children.Any((Node x) => x.HasValueChildren))
			{
				noody.HasValueChildren = false;
			}
			Node grandpa2 = noody.Parent;
			while (grandpa2 != null && grandpa2 != TreeRoot && grandpa2.Value == 0m && grandpa2.HasValueChildren)
			{
				grandpa2.HasValueChildren = false;
				grandpa2 = grandpa2.Parent;
			}
		}

		private void UniteBrother2(Node noody)
		{
			Node brother = FindBrother(noody);
			if (brother == null || !(brother.Value != noody.Value))
			{
				return;
			}
			if (noody.Type == StructureType.RangeNode && !noody.Children.Any((Node x) => x.Type == StructureType.RangeNode) && (noody.Parent == null || noody.Parent.AccountType == AccountType.Fixed))
			{
				foreach (string desc in from x in noody.Children
					group x by GetIdentifier(x.Description) into x
					select x.FirstOrDefault().Description)
				{
					List<Node> fromList;
					Node target;
					if (Math.Abs(noody.Children.Where((Node x) => GetIdentifier(x.Description) == GetIdentifier(desc)).Sum((Node x) => x.Value)) < Math.Abs(brother.Children.Where((Node x) => GetIdentifier(x.Description) == GetIdentifier(desc)).Sum((Node x) => x.Value)))
					{
						fromList = noody.Children.Where((Node x) => GetIdentifier(x.Description) == GetIdentifier(desc)).ToList();
						List<Node> targetList = brother.Children.Where((Node x) => GetIdentifier(x.Description) == GetIdentifier(desc)).ToList();
						target = brother;
					}
					else
					{
						fromList = brother.Children.Where((Node x) => GetIdentifier(x.Description) == GetIdentifier(desc)).ToList();
						List<Node> targetList = noody.Children.Where((Node x) => GetIdentifier(x.Description) == GetIdentifier(desc)).ToList();
						target = noody;
					}
					foreach (Node fromNode in fromList)
					{
						AddAccount(target, fromNode);
						fromNode.Parent.RemoveChildren(fromNode);
					}
				}
				return;
			}
			if (Math.Abs(noody.Value) > Math.Abs(brother.Value))
			{
				Node tmp = noody;
				noody = brother;
				brother = tmp;
			}
			List<Node> nephews = noody.Children as List<Node>;
			int i;
			for (i = nephews.Count - 1; i >= 0; i--)
			{
				if (!(Math.Abs(nephews[i].Value) > 0m))
				{
					continue;
				}
				Node exactTarget = null;
				List<Node> brothers = brother.Children as List<Node>;
				List<Node> exactTargets = (from x in brother.Children
					where GetIdentifier(x.Description) == GetIdentifier(nephews[i].Description)
					orderby x.Type descending
					select x).ToList();
				if (exactTargets.Count > 0)
				{
					foreach (Node item in exactTargets)
					{
						if (exactTarget == null)
						{
							exactTarget = item;
							if (exactTarget.Type != StructureType.RangeNode)
							{
								exactTarget.Type = StructureType.AccountLeaf;
							}
						}
						else if (item.Type != StructureType.AccountLeaf)
						{
							foreach (Node itemCh in item.Children)
							{
								AddAccount(exactTarget, itemCh);
							}
						}
						else
						{
							exactTarget.Value += item.Value;
							brother.RemoveChildren(item);
						}
					}
				}
				if (exactTarget != null)
				{
					brothers = nephews[i].Children as List<Node>;
					List<int> brothersToRemove = new List<int>();
					for (int j = 0; j < brothers.Count; j++)
					{
						if (brothers[j].Value != 0m)
						{
							AddAccount(exactTarget, brothers[j]);
							brothers[j].HasValueChildren = false;
							brothersToRemove.Add(j);
						}
					}
					while (brothersToRemove.Count > 0)
					{
						nephews[i].RemoveChildren(brothers[brothersToRemove.Last()]);
						brothersToRemove.RemoveAt(brothersToRemove.Count - 1);
					}
					if (brothers.Count == 0 && nephews[i].Type == StructureType.AccountLeaf)
					{
						if (nephews[i].Type == StructureType.AccountLeaf && exactTarget.Type == StructureType.AccountLeaf && nephews[i].Description == exactTarget.Description)
						{
							exactTarget.Value += nephews[i].Value;
							exactTarget.HasValueChildren = true;
						}
						else
						{
							AddAccount(exactTarget, nephews[i]);
						}
					}
				}
				else
				{
					AddSubTree(noody, nephews[i]);
				}
				noody.RemoveChildren(nephews[i]);
			}
			if (noody.Children.Count() == 0 || !noody.Children.Any((Node x) => x.HasValueChildren))
			{
				noody.HasValueChildren = false;
			}
			Node grandpa = noody.Parent;
			while (grandpa != null && grandpa != TreeRoot && grandpa.Value == 0m && grandpa.HasValueChildren)
			{
				grandpa.HasValueChildren = false;
				grandpa = grandpa.Parent;
			}
		}

		private string GetIdentifier(string name)
		{
			int tmp = 0;
			string[] vector = name.Split(' ');
			string retstr = "";
			for (int i = 0; i < vector.Count(); i++)
			{
				if (int.TryParse(vector[i].Replace(":", ""), out tmp))
				{
					return vector[i];
				}
			}
			return "";
		}

		private Node FindBrother(Node noody)
		{
			Node brother = null;
			Node tempNode = noody;
			bool isSpecial = !(tempNode.Description == string.Empty) && tempNode.Description != null && tempNode.Description.Contains('>');
			int generationCounter = 0;
			if (noody.Value != 0m)
			{
				while (tempNode.Children.Count() > 0 && tempNode.Children.FirstOrDefault((Node n) => n.Value != 0m).Type != StructureType.AccountLeaf)
				{
					tempNode = tempNode.Children.FirstOrDefault((Node n) => n.Value != 0m);
					generationCounter++;
				}
			}
			if (tempNode.Description.Length < 2)
			{
				return null;
			}
			int startIndex = tempNode.Description.IndexOf("X_");
			if (startIndex < 0)
			{
				startIndex = tempNode.Description.IndexOf("_X");
			}
			brother = _nodes.Values.FirstOrDefault((Node n) => n.Description != null && n.Description.StartsWith(tempNode.Description.Remove(startIndex)) && n != tempNode);
			for (int i = 0; i < generationCounter; i++)
			{
				if (brother != null && brother.Parent != null)
				{
					brother = brother.Parent;
				}
			}
			if (brother == null && isSpecial)
			{
				tempNode = noody;
				string identifier = (noody.Description.Contains(":") ? noody.Description.Split(':')[0] : "");
				if (identifier != "")
				{
					brother = _nodes.Values.FirstOrDefault((Node n) => n.Description != null && n.Description.Contains(identifier) && n != tempNode);
				}
				else if (noody.Description.Contains("X_") || noody.Description.Contains("_X"))
				{
					identifier = (noody.Description.Contains("X_") ? noody.Description.Split(new string[1] { "X_" }, StringSplitOptions.None)[0] : noody.Description.Split(new string[1] { "_X" }, StringSplitOptions.None)[0]).Trim();
					brother = _nodes.Values.FirstOrDefault((Node n) => n.Description != null && n.Description.StartsWith(identifier) && n != tempNode && n.Type == tempNode.Type);
				}
			}
			if (brother == noody)
			{
				return null;
			}
			return brother;
		}

		public decimal ReturnValue(string description)
		{
			return _nodes.Values.FirstOrDefault((Node n) => n.Description != null && n.Description.Contains(description))?.Value ?? 0m;
		}

		public Node ReturnParent(string name, string konto)
		{
			try
			{
				if (konto != "")
				{
					return _nodes.First((KeyValuePair<int, Node> x) => x.Value != null && x.Value.Description != null && x.Value.Description.Contains(konto)).Value;
				}
				if (name != "")
				{
					return _nodes.First((KeyValuePair<int, Node> x) => x.Value != null && x.Value.Description != null && x.Value.Description.Contains(name)).Value;
				}
				return null;
			}
			catch
			{
				return null;
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
