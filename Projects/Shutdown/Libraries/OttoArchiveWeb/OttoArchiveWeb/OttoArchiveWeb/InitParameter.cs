using OttoArchive.Properties;
using WebServiceInterfaces;

namespace OttoArchive.OttoArchiveWeb
{
	public class InitParameter : IParameter
	{
		public string ArchivServerName { get; private set; }

		public string SeratioServerName { get; private set; }

		public string ArchivPort { get; private set; }

		public string SeratioPort { get; private set; }

		public string SysDbName { get; private set; }

		public string DataDbName { get; private set; }

		public string User { get; private set; }

		public string Password { get; private set; }

		public string[] PeriNames { get; private set; }

		public string Errors { get; set; }

		public bool CloseServer { get; set; }

		public InitParameter()
		{
			ArchivServerName = Settings.Default.ArchivServerName;
			SeratioServerName = Settings.Default.SeratioServerName;
			ArchivPort = Settings.Default.ArchivPort;
			SeratioPort = Settings.Default.SeratioPort;
			SysDbName = Settings.Default.SysDBName;
			DataDbName = Settings.Default.DataDbName;
			User = Settings.Default.User;
			Password = Settings.Default.Password;
			PeriNames = new string[Settings.Default.PeriNames.Count];
			Settings.Default.PeriNames.CopyTo(PeriNames, 0);
			CloseServer = true;
		}
	}
}
