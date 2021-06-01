namespace Viewbox.Models
{
	public abstract class SelectionTypeFactory
	{
		public abstract SelectionType GetSelectionObject(SelectionTypeEnum type, int selected);
	}
}
