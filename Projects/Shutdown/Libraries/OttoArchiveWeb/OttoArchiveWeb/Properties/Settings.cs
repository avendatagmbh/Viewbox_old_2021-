using System.CodeDom.Compiler;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace OttoArchive.Properties
{
	[CompilerGenerated]
	[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
	internal sealed class Settings : ApplicationSettingsBase
	{
		private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

		public static Settings Default => defaultInstance;

		[ApplicationScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("10.79.24.74")]
		public string ArchivServerName => (string)this["ArchivServerName"];

		[ApplicationScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("10.79.24.74")]
		public string SeratioServerName => (string)this["SeratioServerName"];

		[ApplicationScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("2000")]
		public string ArchivPort => (string)this["ArchivPort"];

		[ApplicationScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("2005")]
		public string SeratioPort => (string)this["SeratioPort"];

		[ApplicationScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("SYSTEM")]
		public string SysDBName => (string)this["SysDBName"];

		[ApplicationScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("SYSTEM1")]
		public string DataDbName => (string)this["DataDbName"];

		[ApplicationScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <string>ACTEBIS</string>\r\n</ArrayOfString>")]
		public StringCollection PeriNames => (StringCollection)this["PeriNames"];

		[ApplicationScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("fkt_viewbox")]
		public string User => (string)this["User"];

		[ApplicationScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("c4?+braS")]
		public string Password => (string)this["Password"];
	}
}
