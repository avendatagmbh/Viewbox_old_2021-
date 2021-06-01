using System;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace SystemDb.Compatibility.Viewbuilder.OptimizationRelated
{
	public class Layers : NotifyPropertyChangedBase
	{
		private List<Layer> Layer { get; set; }

		public Dictionary<OptimizationType, Layer> TypeToLayer { get; private set; }

		public bool this[OptimizationType type]
		{
			get
			{
				return TypeToLayer[type].UseLayer;
			}
			set
			{
				TypeToLayer[type].UseLayer = value;
				OnPropertyChanged("Item");
			}
		}

		private bool this[int index] => Layer[index].UseLayer;

		private Layers(ILanguageCollection languages)
		{
			Layer = new List<Layer>();
			TypeToLayer = new Dictionary<OptimizationType, Layer>();
			foreach (OptimizationType optType in Enum.GetValues(typeof(OptimizationType)))
			{
				Layer layer = new Layer(optType, languages);
				Layer.Add(layer);
				TypeToLayer[optType] = layer;
			}
		}

		public Layers(Layers layers, ILanguageCollection languages)
			: this(languages)
		{
			for (int i = 0; i < Layer.Count; i++)
			{
				Layer[i] = layers.Layer[i].Clone();
				TypeToLayer[layers.Layer[i].Group.Type] = Layer[i];
			}
		}

		public void DetectLayers(Optimization opt)
		{
			if (opt == null)
			{
				return;
			}
			if (opt.Group != null)
			{
				TypeToLayer[opt.Group.Type].UseLayer = true;
				TypeToLayer[opt.Group.Type].Group = opt.Group;
			}
			foreach (Optimization child in opt.Children)
			{
				DetectLayers(child);
			}
		}

		public static void ExtendOptimization(Layers oldLayers, Layers newLayers, Optimization root)
		{
			int[] array = new int[3] { 4, 3, 5 };
			foreach (int i in array)
			{
				if (oldLayers[i] && !newLayers[i])
				{
					DeleteLayer(root, newLayers.Layer[i]);
				}
			}
			array = new int[3] { 5, 3, 4 };
			foreach (int j in array)
			{
				if (!oldLayers[j] && newLayers[j])
				{
					AddLayer(root, newLayers.Layer[j], newLayers);
				}
			}
		}

		private static void DeleteLayer(Optimization opt, Layer layer)
		{
			if (opt.Children.Count > 0 && opt.Children.First().Group != null && opt.Children.First().Group.Type == layer.Group.Type)
			{
				List<Optimization> newChildren = new List<Optimization>();
				foreach (Optimization childOpt in opt.Children)
				{
					newChildren.AddRange(childOpt.Children);
				}
				opt.Children.Clear();
				foreach (Optimization newChildOpt in newChildren)
				{
					opt.Children.Add(newChildOpt);
				}
			}
			foreach (Optimization child in opt.Children)
			{
				DeleteLayer(child, layer);
			}
		}

		private static void AddLayer(Optimization opt, Layer layer, Layers layers)
		{
			OptimizationType parentType = OptimizationType.NotSet;
			for (int i = global::SystemDb.Compatibility.Viewbuilder.OptimizationRelated.Layer.ParentHierarchie.IndexOf(layer.Group.Type) - 1; i >= 0; i--)
			{
				parentType = global::SystemDb.Compatibility.Viewbuilder.OptimizationRelated.Layer.ParentHierarchie[i];
				if (CheckLayerExists(opt, parentType))
				{
					break;
				}
			}
			if (parentType == OptimizationType.NotSet)
			{
				throw new InvalidOperationException($"Can not add layer, because no suitable parent was found, type: {layer.Group.Type}");
			}
			AddLayer(opt, layer.Group, parentType, layers);
		}

		private static void AddLayer(Optimization opt, OptimizationGroup group, OptimizationType parentType, Layers layers)
		{
			if (opt.Group != null && opt.Group.Type == parentType)
			{
				Optimization newOpt = new Optimization(opt.Descriptions.Count, layers, opt)
				{
					Group = group,
					Value = group.ReadableTypeString
				};
				foreach (Optimization childOpt in opt.Children)
				{
					newOpt.Children.Add(childOpt);
				}
				opt.Children.Clear();
				opt.Children.Add(newOpt);
			}
			foreach (Optimization child in opt.Children)
			{
				AddLayer(child, group, parentType, layers);
			}
		}

		private static bool CheckLayerExists(Optimization opt, OptimizationType type)
		{
			if (opt.Group != null && opt.Group.Type == type)
			{
				return true;
			}
			bool result = false;
			foreach (Optimization childOpt in opt.Children)
			{
				result |= CheckLayerExists(childOpt, type);
			}
			return result;
		}

		public Layer GetNextLayer(OptimizationType type)
		{
			if (type == OptimizationType.SortColumn)
			{
				throw new InvalidOperationException("Unter der Geschäftsjahresebene können keine neuen Elemente hinzugefügt werden.");
			}
			List<int> order = new List<int> { 2, 5, 3, 4 };
			int orderIndex = order.IndexOf((int)type);
			if (orderIndex == -1)
			{
				return null;
			}
			for (int i = orderIndex + 1; i < order.Count; i++)
			{
				if (Layer[order[i]].UseLayer)
				{
					return Layer[order[i]];
				}
			}
			return null;
		}

		public Layer GetPreviousLayer(OptimizationType type)
		{
			if (type == OptimizationType.System)
			{
				throw new InvalidOperationException("Es gibt keine Ebene vor dem System.");
			}
			List<int> order = new List<int> { 2, 5, 3, 4 };
			int orderIndex = order.IndexOf((int)type);
			if (orderIndex == -1)
			{
				return null;
			}
			for (int i = orderIndex - 1; i >= 0; i--)
			{
				if (Layer[order[i]].UseLayer)
				{
					return Layer[order[i]];
				}
			}
			return null;
		}
	}
}
