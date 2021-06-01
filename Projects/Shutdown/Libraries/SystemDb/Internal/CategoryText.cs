using SystemDb.Helper;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("category_texts", ForceInnoDb = true)]
	internal class CategoryText : LocalizedText
	{
	}
}
