using SystemDb.Helper;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("optimization_group_texts", ForceInnoDb = true)]
	internal class OptimizationGroupText : LocalizedText
	{
	}
}
