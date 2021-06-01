using SystemDb.Helper;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("table_texts", ForceInnoDb = true)]
	public class TableObjectText : LocalizedText
	{
	}
}
