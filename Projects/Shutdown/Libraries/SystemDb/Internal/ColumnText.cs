using SystemDb.Helper;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("column_texts", ForceInnoDb = true)]
	public class ColumnText : LocalizedText
	{
	}
}
