using System.Collections.Generic;
using Utils;

namespace SystemDb.Compatibility.Viewbuilder.OptimizationRelated
{
	public class Layer : NotifyPropertyChangedBase
	{
		private static readonly List<OptimizationType> _parentHierarchie = new List<OptimizationType>
		{
			OptimizationType.NotSet,
			OptimizationType.None,
			OptimizationType.System,
			OptimizationType.IndexTable,
			OptimizationType.SplitTable,
			OptimizationType.SortColumn
		};

		private bool _useLayer;

		private OptimizationGroup _group;

		private ILanguageCollection Languages { get; set; }

		public List<string> Descriptions { get; private set; }

		public static List<OptimizationType> ParentHierarchie => _parentHierarchie;

		public bool UseLayer
		{
			get
			{
				return _useLayer;
			}
			set
			{
				if (_useLayer != value)
				{
					_useLayer = value;
					OnPropertyChanged("UseLayer");
				}
			}
		}

		public OptimizationGroup Group
		{
			get
			{
				return _group;
			}
			set
			{
				if (_group == value)
				{
					return;
				}
				_group = value;
				if (_group != null && _group.CreatedFrom != null)
				{
					int index = 0;
					foreach (ILanguage lang in Languages)
					{
						Descriptions[index++] = _group.CreatedFrom.Names[lang];
					}
				}
				OnPropertyChanged("Group");
			}
		}

		public Layer(OptimizationType optType, ILanguageCollection languages)
			: this(new OptimizationGroup(optType), languages)
		{
		}

		private Layer(OptimizationGroup group, ILanguageCollection languages)
		{
			Languages = languages;
			Descriptions = new List<string>(new string[languages.Count]);
			Group = group;
			int index = 0;
			if (group.CreatedFrom == null)
			{
				return;
			}
			foreach (ILanguage lang in languages)
			{
				Descriptions[index++] = group.CreatedFrom.Names[lang];
			}
		}

		public override string ToString()
		{
			return $"Layer {Group.Type}, Used: {UseLayer}";
		}

		public Layer Clone()
		{
			Layer layer = new Layer(Group, Languages)
			{
				UseLayer = UseLayer
			};
			for (int i = 0; i < Descriptions.Count; i++)
			{
				layer.Descriptions[i] = Descriptions[i];
			}
			return layer;
		}
	}
}
