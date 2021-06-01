using System.Collections.Generic;
using SystemDb.Helper;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("optimization_groups", ForceInnoDb = true)]
	public class OptimizationGroup : IOptimizationGroup
	{
		private readonly LocalizedTextCollection _names = new LocalizedTextCollection();

		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		public ILocalizedTextCollection Names => _names;

		[DbColumn("type")]
		public OptimizationType Type { get; set; }

		public void SetName(string name, ILanguage language)
		{
			if (language != null)
			{
				_names.Add(language, name);
			}
		}

		public override string ToString()
		{
			return $"[{Type}] {(Names as LocalizedTextCollection).First}";
		}

		public OptimizationGroup Clone()
		{
			OptimizationGroup clone = new OptimizationGroup
			{
				Id = Id,
				Type = Type
			};
			foreach (KeyValuePair<string, string> i in _names)
			{
				clone._names[i.Key] = i.Value;
			}
			return clone;
		}
	}
}
