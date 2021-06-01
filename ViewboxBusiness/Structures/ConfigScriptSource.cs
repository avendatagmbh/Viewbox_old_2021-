using DbAccess.Structures;

namespace ViewboxBusiness.Structures
{
	public class ConfigScriptSource
	{
		public ScriptSourceMode ScriptSourceMode { get; set; }

		public string Directory { get; set; }

		public string BilanzDirectory { get; set; }

		public string ExtendedColumnInformationDirectory { get; set; }

		public bool IncludeSubdirectories { get; set; }

		public DbConfig DbConfig { get; set; }

		public ConfigScriptSource()
		{
			ScriptSourceMode = ScriptSourceMode.Directory;
			Directory = "Q:\\Großprojekte";
			IncludeSubdirectories = false;
			DbConfig = new DbConfig();
		}
	}
}
