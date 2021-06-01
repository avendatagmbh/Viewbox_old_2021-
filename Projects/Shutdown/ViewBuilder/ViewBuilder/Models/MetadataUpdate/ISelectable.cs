namespace ViewBuilder.Models.MetadataUpdate
{
	interface ISelectable
	{
		bool IsChecked { get; set; }
		bool IsEnabled { get; }
	}
}
