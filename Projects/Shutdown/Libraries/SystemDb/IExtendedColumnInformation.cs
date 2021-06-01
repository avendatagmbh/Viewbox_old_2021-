namespace SystemDb
{
	internal interface IExtendedColumnInformation
	{
		int Id { get; set; }

		int ParentColumnId { get; set; }

		int ChildColumnId { get; set; }

		int InformationColumnId { get; set; }
	}
}
