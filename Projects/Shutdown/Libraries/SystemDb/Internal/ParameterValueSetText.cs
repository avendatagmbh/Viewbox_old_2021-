using SystemDb.Helper;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("collection_texts", ForceInnoDb = true)]
	internal class ParameterValueSetText : LocalizedText
	{
	}
}
