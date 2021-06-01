using SystemDb.Helper;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("collections", ForceInnoDb = true)]
	internal class ParameterValue : IParameterValue
	{
		private readonly LocalizedTextCollection _descriptions = new LocalizedTextCollection();

		[DbColumn("id", AutoIncrement = true)]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("collection_id")]
		public int CollectionId { get; set; }

		[DbColumn("value", Length = 128)]
		public string Value { get; set; }

		public ILocalizedTextCollection Descriptions => _descriptions;

		public void SetDescription(string description, ILanguage language)
		{
			if (language != null)
			{
				_descriptions.Add(language, description);
			}
		}
	}
}
