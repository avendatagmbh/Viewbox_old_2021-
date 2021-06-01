using SystemDb.Helper;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("user_parametergroup_texts", ForceInnoDb = true)]
	internal class UserParameterGroupTexts : LocalizedText
	{
	}
}
