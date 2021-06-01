using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using SystemDb.Internal;
using DbAccess;
using Utils;

namespace SystemDb.Compatibility.Viewbuilder.OptimizationRelated
{
	public class Optimization : NotifyPropertyChangedBase
	{
		private bool _isExpanded;

		private bool _isSelected;

		private bool _isChecked;

		private string _value;

		private Layers Layers { get; set; }

		public ObservableCollectionAsync<Optimization> Children { get; private set; }

		private IOptimization CreatedFrom { get; set; }

		public OptimizationGroup Group { get; set; }

		private Optimization Parent { get; set; }

		public List<string> Descriptions { get; private set; }

		public bool IsExpanded
		{
			get
			{
				return _isExpanded;
			}
			set
			{
				if (_isExpanded != value)
				{
					_isExpanded = value;
					OnPropertyChanged("IsExpanded");
				}
			}
		}

		public bool IsSelected
		{
			get
			{
				return _isSelected;
			}
			set
			{
				if (_isSelected != value)
				{
					_isSelected = value;
					OnPropertyChanged("IsSelected");
				}
			}
		}

		public bool IsChecked
		{
			get
			{
				return _isChecked;
			}
			set
			{
				if (_isChecked != value)
				{
					_isChecked = value;
					OnPropertyChanged("IsChecked");
				}
			}
		}

		public string Value
		{
			private get
			{
				return _value;
			}
			set
			{
				if (_value != value)
				{
					_value = value;
					OnPropertyChanged("Value");
				}
			}
		}

		public Optimization(int languages, Layers layers, Optimization parent = null)
		{
			Layers = layers;
			Children = new ObservableCollectionAsync<Optimization>();
			Parent = parent;
			Descriptions = new List<string>(new string[languages]);
			Children.CollectionChanged += Children_CollectionChanged;
		}

		public Optimization()
		{
			Children = new ObservableCollectionAsync<Optimization>();
			Children.CollectionChanged += Children_CollectionChanged;
			Value = string.Empty;
		}

		private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems == null)
			{
				return;
			}
			foreach (Optimization item in e.NewItems)
			{
				if (item.Layers == null)
				{
					item.Layers = Layers;
				}
				if (item.Parent == null)
				{
					item.Parent = this;
				}
				if (item.Descriptions == null)
				{
					item.Descriptions = new List<string>(new string[Descriptions.Count]);
				}
				if (item.Group == null)
				{
					item.Group = Layers.GetNextLayer(item.Parent.Group.Type).Group;
				}
			}
		}

		~Optimization()
		{
			Children.CollectionChanged -= Children_CollectionChanged;
		}

		public static Optimization CreateFromViewboxOptimization(IOptimization opt, int languageCount, Layers layers)
		{
			if (opt == null)
			{
				return null;
			}
			Optimization optimization = new Optimization(languageCount, layers);
			Dictionary<OptimizationGroup, OptimizationGroup> groups = new Dictionary<OptimizationGroup, OptimizationGroup>();
			FillOptimizationRecursively(optimization, opt, groups);
			return optimization;
		}

		private static void FillOptimizationRecursively(Optimization newOpt, IOptimization baseOpt, Dictionary<OptimizationGroup, OptimizationGroup> groups)
		{
			newOpt.Value = baseOpt.Value;
			newOpt.CreatedFrom = baseOpt;
			OptimizationGroup group = new OptimizationGroup(baseOpt.Group.Type, baseOpt.Group);
			if (!groups.ContainsKey(group))
			{
				groups.Add(group, group);
			}
			else
			{
				group = groups[group];
			}
			newOpt.Group = group;
			int index = 0;
			foreach (KeyValuePair<string, string> langPair in baseOpt.Descriptions)
			{
				if (index >= newOpt.Descriptions.Count)
				{
					break;
				}
				newOpt.Descriptions[index++] = langPair.Value;
			}
			foreach (IOptimization child in baseOpt.Children)
			{
				Optimization opt = new Optimization(newOpt.Descriptions.Count, newOpt.Layers, newOpt);
				FillOptimizationRecursively(opt, child, groups);
				newOpt.Children.Add(opt);
			}
		}

		private static void AllChildren(IOptimization current, List<IOptimization> opts)
		{
			if (current == null)
			{
				return;
			}
			opts.Add(current);
			foreach (IOptimization child in current.Children)
			{
				AllChildren(child, opts);
			}
		}

		public static void Save(SystemDb viewboxDb, Optimization root, Layers layers, string system)
		{
			IOptimization current = viewboxDb.Optimizations.Where((IOptimization o) => o.Level == 1 && ((o.Value == null && system == null) || (o.Value != null && system != null && o.Value.ToLower() == system.ToLower()))).FirstOrDefault();
			List<IOptimization> oldOpts = new List<IOptimization>();
			AllChildren(current, oldOpts);
			foreach (IOptimization optToDelete in oldOpts)
			{
				((OptimizationCollection)viewboxDb.Optimizations).Remove(optToDelete.Id);
			}
			List<IOptimizationGroup> groupsToDelete = new List<IOptimizationGroup>();
			foreach (IOptimizationGroup group in viewboxDb.OptimizationGroups)
			{
				if (!viewboxDb.Optimizations.Any((IOptimization opt) => opt.Group == group))
				{
					groupsToDelete.Add(group);
				}
			}
			Dictionary<OptimizationType, IOptimizationGroup> typeToGroup = new Dictionary<OptimizationType, IOptimizationGroup>();
			foreach (KeyValuePair<OptimizationType, Layer> pair in layers.TypeToLayer)
			{
				typeToGroup[pair.Key] = new global::SystemDb.Internal.OptimizationGroup
				{
					Type = pair.Key
				};
			}
			string groupIdsString = string.Join(",", groupsToDelete.Select((IOptimizationGroup grp) => grp.Id));
			string optIdsString = string.Join(",", oldOpts.Select((IOptimization opt) => opt.Id));
			using DatabaseBase conn = viewboxDb.ConnectionManager.GetConnection();
			string sql = $"DELETE FROM optimization_groups WHERE id in ({groupIdsString})";
			if (groupsToDelete.Count != 0)
			{
				conn.ExecuteNonQuery(sql);
			}
			sql = $"DELETE FROM optimization_group_texts WHERE ref_id in ({groupIdsString})";
			if (groupsToDelete.Count != 0)
			{
				conn.ExecuteNonQuery(sql);
			}
			sql = $"DELETE FROM optimization_texts WHERE ref_id in ({optIdsString})";
			if (oldOpts.Count != 0)
			{
				conn.ExecuteNonQuery(sql);
			}
			List<OptimizationType> typesToSave = new List<OptimizationType>
			{
				OptimizationType.System,
				OptimizationType.IndexTable,
				OptimizationType.SortColumn,
				OptimizationType.SplitTable
			};
			List<IOptimizationGroup> groupsToSave = new List<IOptimizationGroup>();
			List<OptimizationGroupText> groupTexts = new List<OptimizationGroupText>();
			List<string> languageCodes = viewboxDb.Languages.Select((ILanguage language) => language.CountryCode).ToList();
			foreach (OptimizationType type2 in typesToSave)
			{
				if (layers.TypeToLayer[type2].UseLayer)
				{
					groupsToSave.Add(typeToGroup[type2]);
				}
			}
			conn.DbMapping.Save(typeof(global::SystemDb.Internal.OptimizationGroup), groupsToSave);
			foreach (OptimizationType type in typesToSave)
			{
				if (!layers.TypeToLayer[type].UseLayer)
				{
					continue;
				}
				for (int i = 0; i < layers.TypeToLayer[type].Descriptions.Count; i++)
				{
					string text = layers.TypeToLayer[type].Descriptions[i];
					if (text != null && !string.IsNullOrEmpty(text.Trim()))
					{
						groupTexts.Add(new OptimizationGroupText
						{
							CountryCode = languageCodes[i],
							Text = text,
							RefId = typeToGroup[type].Id
						});
					}
				}
			}
			conn.DbMapping.Save(typeof(OptimizationGroupText), groupTexts);
			List<OptimizationText> optTexts = new List<OptimizationText>();
			List<IOptimization> newOpts = new List<IOptimization>();
			SaveOptimizations(root, typeToGroup, optTexts, languageCodes, oldOpts, newOpts);
			sql = string.Format("DELETE FROM optimizations WHERE id in ({0})", string.Join(",", oldOpts.Select((IOptimization opt) => opt.Id)));
			if (oldOpts.Count != 0)
			{
				conn.ExecuteNonQuery(sql);
			}
			foreach (IOptimization opt2 in newOpts)
			{
				conn.DbMapping.Save(opt2);
			}
			conn.DbMapping.Save(typeof(OptimizationText), optTexts);
		}

		private static void SaveOptimizations(Optimization opt, Dictionary<OptimizationType, IOptimizationGroup> typeToGroup, List<OptimizationText> optTexts, List<string> languageCodes, List<IOptimization> oldOpts, List<IOptimization> newOpts)
		{
			oldOpts.Remove(opt.CreatedFrom);
			if (opt.Group != null)
			{
				int id = ((opt.CreatedFrom != null) ? opt.CreatedFrom.Id : 0);
				IOptimization parent = ((opt.Parent == null) ? null : opt.Parent.CreatedFrom);
				opt.CreatedFrom = new global::SystemDb.Internal.Optimization
				{
					Group = typeToGroup[opt.Group.Type],
					Value = opt.Value,
					Id = id,
					Parent = parent,
					ParentId = (parent?.Id ?? 0)
				};
				newOpts.Add(opt.CreatedFrom);
				for (int i = 0; i < opt.Descriptions.Count; i++)
				{
					if (opt.Descriptions[i] != null && !string.IsNullOrEmpty(opt.Descriptions[i].Trim()))
					{
						optTexts.Add(new OptimizationText
						{
							RefId = opt.CreatedFrom.Id,
							Text = opt.Descriptions[i],
							CountryCode = languageCodes[i]
						});
					}
				}
			}
			foreach (Optimization child in opt.Children)
			{
				SaveOptimizations(child, typeToGroup, optTexts, languageCodes, oldOpts, newOpts);
			}
		}
	}
}
