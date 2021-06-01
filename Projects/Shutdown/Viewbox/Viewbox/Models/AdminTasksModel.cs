namespace Viewbox.Models
{
	public class AdminTasksModel : SettingsModel
	{
		public bool GeneratedTable { get; set; }

		public override string Partial => "_AdminTasksPartial";

		public bool AreIndexesPopulated { get; set; }

		public bool ExtendedColumnsInformationGenerated { get; set; }

		public AdminTasksModel()
			: base(SettingsType.AdminTasks)
		{
		}
	}
}
