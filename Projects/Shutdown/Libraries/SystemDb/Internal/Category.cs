using System;
using System.Collections.Generic;
using SystemDb.Helper;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("categories", ForceInnoDb = true)]
	public class Category : ICategory, ICloneable
	{
		private readonly LocalizedTextCollection _names = new LocalizedTextCollection();

		private readonly TableObjectCollection _tableObjects = new TableObjectCollection();

		private int _ordinal;

		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		public ILocalizedTextCollection Names => _names;

		public ITableObjectCollection TableObjects => _tableObjects;

		public int Ordinal => _ordinal;

		public void SetName(string name, ILanguage language)
		{
			_names.Add(language, name);
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		private ICategory Clone()
		{
			Category clone = new Category
			{
				Id = Id,
				_ordinal = Ordinal
			};
			foreach (KeyValuePair<string, string> i in _names)
			{
				clone._names.Add(i.Key, i.Value);
			}
			return clone;
		}
	}
}
