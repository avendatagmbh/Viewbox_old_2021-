using System;
using System.ComponentModel;
using SystemDb.Helper;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("optimizations", ForceInnoDb = true)]
	public class Optimization : IOptimization, ICloneable
	{
		private readonly OptimizationCollection _children = new OptimizationCollection();

		private Optimization _parent;

		private readonly LocalizedTextCollection _descriptions = new LocalizedTextCollection();

		private OptimizationGroup _optimizationGroup = new OptimizationGroup();

		public int Level
		{
			get
			{
				if (Parent != null)
				{
					return Parent.Level + 1;
				}
				return 0;
			}
		}

		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("parent_id", AllowDbNull = true)]
		[DbUniqueKey("_uk_parent_value")]
		public int ParentId { get; set; }

		public IOptimization Parent
		{
			get
			{
				return _parent;
			}
			set
			{
				if (_parent != value)
				{
					_parent = value as Optimization;
					NotifyPropertyChanged("Optimization");
				}
			}
		}

		public IOptimizationCollection Children => _children;

		public ILocalizedTextCollection Descriptions => _descriptions;

		[DbColumn("value", Length = 128)]
		[DbUniqueKey("_uk_parent_value")]
		public string Value { get; set; }

		[DbColumn("optimization_group_id")]
		public int OptimizationGroupId { get; set; }

		public IOptimizationGroup Group
		{
			get
			{
				return _optimizationGroup;
			}
			set
			{
				if (_optimizationGroup != value)
				{
					_optimizationGroup = value as OptimizationGroup;
					OptimizationGroupId = value.Id;
					NotifyPropertyChanged("OptimizationGroup");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public string FindValue(OptimizationType type)
		{
			if (Group != null && Group.Type == type)
			{
				return Value;
			}
			if (Id != 0 && Parent != null)
			{
				return Parent.FindValue(type);
			}
			return null;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}

		private void NotifyPropertyChanged(string property)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		public override string ToString()
		{
			return $"[{Group}] {(Descriptions as LocalizedTextCollection).First}";
		}

		public void SetDescription(string description, ILanguage language)
		{
			_descriptions.Add(language, description);
		}
	}
}
