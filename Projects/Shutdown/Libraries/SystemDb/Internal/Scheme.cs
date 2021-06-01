using System;
using SystemDb.Helper;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("schemes", ForceInnoDb = true)]
	public class Scheme : IScheme, ICloneable
	{
		private static readonly Scheme _datagrid = new Scheme
		{
			Id = 0,
			Partial = "_DefaultPartial"
		};

		private readonly LocalizedTextCollection _descriptions = new LocalizedTextCollection();

		public static Scheme Default => _datagrid;

		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("partial", Length = 128)]
		public string Partial { get; set; }

		public ILocalizedTextCollection Descriptions => _descriptions;

		public void SetDescription(string description, ILanguage language)
		{
			_descriptions.Add(language, description);
		}

		object ICloneable.Clone()
		{
			return MemberwiseClone();
		}

		public Scheme Clone()
		{
			return (Scheme)((ICloneable)this).Clone();
		}
	}
}
