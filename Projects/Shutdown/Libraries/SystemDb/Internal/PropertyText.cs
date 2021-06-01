using SystemDb.Helper;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("property_texts", ForceInnoDb = true)]
	internal class PropertyText : LocalizedText
	{
		[DbColumn("name", Length = 1024)]
		public string Name { get; set; }
	}
}
