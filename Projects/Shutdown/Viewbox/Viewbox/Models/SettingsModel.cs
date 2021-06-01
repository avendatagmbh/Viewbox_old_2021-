using Viewbox.Properties;

namespace Viewbox.Models
{
	public abstract class SettingsModel : ViewboxModel
	{
		public enum SettingsType
		{
			User,
			Properties,
			Rights,
			AdminTasks,
			Information,
			RoleSettings,
			LogRead,
			ModifyStartScreen,
			RoleTree
		}

		public SettingsType Type { get; private set; }

		public abstract string Partial { get; }

		public override string LabelCaption => Resources.Settings;

		public SettingsModel(SettingsType type = SettingsType.Properties)
		{
			Type = type;
		}
	}
}
