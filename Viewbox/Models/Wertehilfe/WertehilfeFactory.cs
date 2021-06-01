namespace Viewbox.Models.Wertehilfe
{
	public class WertehilfeFactory
	{
		public static IWertehilfe Create(int parameterId, string search, bool isExact, bool onlyCheck, string[] sortColumns = null, string[] directions = null, WertehilfeType type = WertehilfeType.Wertehilfe, string[] values = null)
		{
			IWertehilfe instance = null;
			switch (type)
			{
			case WertehilfeType.Wertehilfe:
				instance = new Wertehilfe(parameterId, search, isExact, sortColumns, directions, onlyCheck);
				break;
			case WertehilfeType.DynamicWertehilfe:
				instance = new DynamicWertehilfe(parameterId, search, isExact, sortColumns, directions, values, onlyCheck);
				break;
			}
			return instance;
		}
	}
}
