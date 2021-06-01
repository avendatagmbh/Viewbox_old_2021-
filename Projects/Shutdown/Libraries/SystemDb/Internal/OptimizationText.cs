using SystemDb.Helper;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("optimization_texts", ForceInnoDb = true)]
	internal class OptimizationText : LocalizedText
	{
	}
}
