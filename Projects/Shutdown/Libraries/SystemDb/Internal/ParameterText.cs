using SystemDb.Helper;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("parameter_texts", ForceInnoDb = true)]
	public class ParameterText : LocalizedText
	{
	}
}
