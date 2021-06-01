using SystemDb.Helper;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("object_type_texts", ForceInnoDb = true)]
	public class ObjectTypeText : LocalizedText, IObjectTypeText
	{
	}
}
