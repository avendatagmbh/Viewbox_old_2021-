using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Web.Mvc;
using SystemDb.Upgrader;
using DbAccess;
using DbAccess.Enums;
using Viewbox.Job;
using Viewbox.Properties;
using ViewboxDb;

namespace Viewbox.Models
{
	public class ControlCenterModel : ViewboxModel
	{
		private static string mySqlVersion = string.Empty;

		private readonly Storage storage;

		public IEnumerable<Transformation> AllJobs => new List<Transformation>(Transformation.List.Where((Transformation job) => job.Listeners.Count > 0 && !job.Listeners.Contains(ViewboxSession.User)));

		public IEnumerable<Transformation> Jobs => new List<Transformation>(Transformation.List.Where((Transformation job) => job.Listeners.Contains(ViewboxSession.User)));

		public float CpuLoad { get; set; }

		public string CpuLoadDisplayString => Math.Min(100f, CpuLoad).ToString("0") + " %";

		public string CpuLoadStyleWidth => Math.Min(100f, CpuLoad).ToString("0") + "%";

		public float DiscSpeed { get; set; }

		public string DiscSpeedDisplayString
		{
			get
			{
				if (DiscSpeed < 10f)
				{
					return Resources.Low;
				}
				if (DiscSpeed < 60f)
				{
					return Resources.Mid;
				}
				return Resources.High;
			}
		}

		public static string MySqlVersion
		{
			get
			{
				if (mySqlVersion == string.Empty)
				{
					using DatabaseBase connection = ViewboxApplication.Database.ConnectionManager.GetConnection();
					using IDataReader reader = connection.ExecuteReader("SELECT Version();");
					while (reader.Read())
					{
						mySqlVersion = reader.GetString(0);
					}
				}
				return mySqlVersion;
			}
		}

		public string MySqlConnectionState
		{
			get
			{
				ViewboxApplication.Database.ConnectionManager.UpdateConnectionState();
				return (ViewboxApplication.Database.ConnectionManager.ConnectionState == ConnectionStates.Online) ? "Online" : "Offline";
			}
		}

		public string DiscSpeedStyleWidth => Math.Min(100f, DiscSpeed).ToString("0") + "%";

		public ulong AvailableMem { get; set; }

		public ulong FreeMem { get; set; }

		public string FreeMemDisplayString => ConvertBytes(FreeMem);

		public string FreeMemStyleWidth => Math.Min(100.0, 100.0 - 100.0 * ((double)FreeMem / (double)AvailableMem)).ToString("0") + "%";

		public List<ulong> AvailableDiscSpace { get; set; }

		public List<ulong> FreeDiscSpace { get; set; }

		public Storage Discs => storage;

		public ActionCollection Actions { get; set; }

		public NewLogActionCollection NewLogActions { get; set; }

		public bool ShowHidden { get; set; }

		public int UserId { get; set; }

		public string LogDate { get; set; }

		public override string LabelCaption => Resources.ControlCenter;

		public string SearchPhrase { get; private set; }

		public string LabelSearch => Resources.ActionSearch;

		public string DatabaseVersion => VersionInfo.CurrentDbVersion;

		public string ViewboxVersion
		{
			get
			{
				if (Assembly.GetAssembly(typeof(ViewboxSession)) == null)
				{
					return "?.?.?.?";
				}
				Version version = Assembly.GetAssembly(typeof(ViewboxSession)).GetName().Version;
				return version.Major + "." + version.Minor + "." + version.Build + "." + version.MinorRevision;
			}
		}

		public List<SelectListItem> UserList { get; set; }

		public ControlCenterModel(string search = "")
		{
			SearchPhrase = search;
			storage = new Storage();
			AvailableDiscSpace = new List<ulong>();
			FreeDiscSpace = new List<ulong>();
		}

		public string DiscSpaceStyleWidth(int i)
		{
			return Math.Min(100.0, 100.0 - 100.0 * ((double)FreeDiscSpace[i] / (double)AvailableDiscSpace[i])).ToString("0") + "%";
		}

		public string LabelRuntime(Transformation job)
		{
			if (job.Runtime < TimeSpan.FromMinutes(1.0))
			{
				return $"{job.Runtime.Seconds}{Resources.Seconds}.";
			}
			if (job.Runtime < TimeSpan.FromHours(1.0))
			{
				return string.Format("{1}{3}. {0}{2}.", job.Runtime.Seconds, job.Runtime.Minutes, Resources.Seconds, Resources.Minutes);
			}
			if (job.Runtime < TimeSpan.FromDays(1.0))
			{
				return string.Format("{1}{3}. {0}{2}.", job.Runtime.Minutes, job.Runtime.Hours, Resources.Minutes, Resources.Hours);
			}
			return string.Format("> {0} {2}", job.Runtime.Days, Resources.Days);
		}

		public static string GetDirectoryPermissions()
		{
			string folderPath = ViewboxApplication.TemporaryDirectory;
			if (!Directory.Exists(folderPath))
			{
				return Resources.BooleanNo;
			}
			string userName = WindowsIdentity.GetCurrent().Name;
			string identityReference = userName.ToLower();
			DirectorySecurity dirSecurity = Directory.GetAccessControl(folderPath, AccessControlSections.Access);
			foreach (FileSystemAccessRule fsRule in dirSecurity.GetAccessRules(includeExplicit: true, includeInherited: true, typeof(NTAccount)))
			{
				if (fsRule.IdentityReference.Value.ToLower() == identityReference)
				{
					return Resources.BooleanYes;
				}
			}
			return Resources.BooleanNo;
		}

		public static string ConvertBytes(ulong bytes)
		{
			if (bytes < 1000)
			{
				return $"{bytes:0} Bytes";
			}
			if (bytes < 1000000)
			{
				return $"{(double)bytes / 1024.0:0.00} KB";
			}
			if (bytes < 1000000000)
			{
				return $"{(double)bytes / 1048576.0:0.00} MB";
			}
			if (bytes < 1000000000000L)
			{
				return $"{(double)bytes / 1073741824.0:0.00} GB";
			}
			return $"{(double)bytes / 1099511627776.0:##,0.00} TB";
		}

		public static string GetDatabaseAddress()
		{
			string serverInfo = ViewboxApplication.Database.ConnectionString;
			try
			{
				string[] connectionData = serverInfo.Split(';');
				string[] host = connectionData[0].Split('=');
				string[] port = connectionData[1].Split('=');
				return $"{host[1]} : {port[1]}";
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		public static long RunBenchmark(long repeat = 1000000000L)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			string query = $"SELECT BENCHMARK({repeat}, 1+1);";
			using (DatabaseBase connection = ViewboxApplication.Database.ConnectionManager.GetConnection())
			{
				using IDataReader reader = connection.ExecuteReader(query);
				while (reader.Read())
				{
					reader.GetInt32(0);
				}
			}
			sw.Stop();
			return sw.ElapsedMilliseconds;
		}
	}
}
