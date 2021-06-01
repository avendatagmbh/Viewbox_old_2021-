using SystemDb.Helper;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("scheme_texts", ForceInnoDb = true)]
	public class SchemeText : LocalizedText
	{
	}
}
