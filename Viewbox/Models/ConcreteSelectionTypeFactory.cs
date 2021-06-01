namespace Viewbox.Models
{
	public class ConcreteSelectionTypeFactory : SelectionTypeFactory
	{
		public override SelectionType GetSelectionObject(SelectionTypeEnum type, int selected)
		{
			return type switch
			{
				SelectionTypeEnum.Simple => new SimpleSelection(selected), 
				SelectionTypeEnum.VonBis => new VonBisSelection(selected), 
				SelectionTypeEnum.Required => new RequiredSelection(selected), 
				SelectionTypeEnum.RequiredFree => new RequiredFreeSelection(selected), 
				_ => null, 
			};
		}
	}
}
