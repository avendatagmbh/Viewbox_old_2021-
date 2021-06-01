using System;
using SystemDb.Helper;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("properties", ForceInnoDb = true)]
	internal class Property : IProperty, ICloneable
	{
		private LocalizedTextCollection _descriptions = new LocalizedTextCollection();

		private LocalizedTextCollection _names = new LocalizedTextCollection();

		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("key", Length = 128)]
		public string Key { get; set; }

		[DbColumn("value")]
		public string Value { get; set; }

		public ILocalizedTextCollection Names => _names;

		public ILocalizedTextCollection Descriptions => _descriptions;

		[DbColumn("type")]
		public PropertyType Type { get; set; }

		public object Clone()
		{
			return new Property
			{
				Key = Key,
				Id = Id,
				Value = Value,
				Type = Type,
				_descriptions = _descriptions,
				_names = _names
			};
		}

		public void SetName(string name, ILanguage language)
		{
			_names.Add(language, name);
		}

		public void SetDescription(string description, ILanguage language)
		{
			_descriptions.Add(language, description);
		}
	}
}
