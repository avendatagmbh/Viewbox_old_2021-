using SystemDb.Helper;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("parameter_selection_logic_texts", ForceInnoDb = true)]
	public class ParameterSelectionLogicText : LocalizedText
	{
	}
}
