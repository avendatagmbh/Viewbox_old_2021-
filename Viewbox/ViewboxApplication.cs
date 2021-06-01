using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SystemDb;
using SystemDb.Internal;
using SystemDb.Upgrader;
using AV.Log;
using DbAccess;
using DbAccess.Attributes;
using log4net;
using Viewbox.DcwBalance;
using Viewbox.Job;
using Viewbox.Properties;
using Viewbox.SapBalance;
using Viewbox.Structures;
using ViewboxDb;
using ViewboxDb.AggregationFunctionTranslator;
using ViewboxMdConverter;
using System.Net;

namespace Viewbox
{
	public static class ViewboxApplication
	{
		private class Language : ILanguage
		{
			public CultureInfo cultureInfo;

			public string CountryCode { get; set; }

			public string LanguageName { get; set; }

			public string LanguageMotto { get; set; }

			public CultureInfo CultureInfo
			{
				get
				{
					if (cultureInfo == null)
					{
						cultureInfo = new CultureInfo(CountryCode);
					}
					return cultureInfo;
				}
			}
		}

		internal static ILog _log = LogHelper.GetLogger();

		private static HttpApplicationStateBase _application;

		private static IPerformanceCounter _processorTime;

		private static IPerformanceCounter _hardDiskTransferSpeed;

		private static readonly Dictionary<string, Tuple<DateTime, IUser>> _accessKeys = new Dictionary<string, Tuple<DateTime, IUser>>();

		public static string SelectionTypeConcat { get; set; }

		public static HttpApplicationStateBase Application => _application;

		public static IPerformanceCounter ProcessorTime => _processorTime;

		public static IPerformanceCounter HardDiskTransferSpeed => _hardDiskTransferSpeed;

		public static string ArchiveSecurityKey => ConfigurationManager.AppSettings["ArchiveSecurityKey"];

		public static bool ArchiveAuthentication => ConfigurationManager.AppSettings["ArchiveAuthentication"] != null && Convert.ToBoolean(ConfigurationManager.AppSettings["ArchiveAuthentication"]);

		public static string ArchiveAuthenticationUsername => ConfigurationManager.AppSettings["ArchiveAuthenticationUsername"];

		public static string ArchiveAuthenticationPassword => ConfigurationManager.AppSettings["ArchiveAuthenticationPassword"];

		public static bool HasDefaultOrderField => ConfigurationManager.AppSettings["HasDefaultOrderField"] == null || Convert.ToBoolean(ConfigurationManager.AppSettings["HasDefaultOrderField"]);

		public static bool IsEnabledPreviewOnWhiteBooks => ConfigurationManager.AppSettings["IsEnabledPreviewOnWhiteBooks"] == null || Convert.ToBoolean(ConfigurationManager.AppSettings["IsEnabledPreviewOnWhiteBooks"]);

		public static int WertehilfeSize
		{
			get
			{
				if (ConfigurationManager.AppSettings["WertehilfeSize"] == null)
				{
					return 25;
				}
				return int.Parse(ConfigurationManager.AppSettings["WertehilfeSize"].ToString());
			}
		}

		public static int WertehilfeLimit
		{
			get
			{
				if (ConfigurationManager.AppSettings["WertehilfeLimit"] == null)
				{
					return 150;
				}
				return int.Parse(ConfigurationManager.AppSettings["WertehilfeLimit"].ToString());
			}
		}

		public static int PasswordSecurityLevel
		{
			get
			{
				if (ConfigurationManager.AppSettings["PasswordSecurityLevel"] == null)
				{
					return 0;
				}
				return int.Parse(ConfigurationManager.AppSettings["PasswordSecurityLevel"].ToString());
			}
		}

		public static int PasswordExpiredInDays
		{
			get
			{
				if (ConfigurationManager.AppSettings["PasswordExpiredInDays"] == null)
				{
					return -1;
				}
				return int.Parse(ConfigurationManager.AppSettings["PasswordExpiredInDays"].ToString());
			}
		}

		public static long SQLJoinRowsInThousands
		{
			get
			{
				if (ConfigurationManager.AppSettings["SQLJoinRowsInThousands"] == null)
				{
					return -1L;
				}
				return long.Parse(ConfigurationManager.AppSettings["SQLJoinRowsInThousands"].ToString()) * 1000;
			}
		}

		public static long SQLJoinIndexRowsInThousands
		{
			get
			{
				if (ConfigurationManager.AppSettings["SQLJoinIndexRowsInThousands"] == null)
				{
					return -1L;
				}
				return long.Parse(ConfigurationManager.AppSettings["SQLJoinIndexRowsInThousands"].ToString()) * 1000;
			}
		}

		public static bool HideWertehilfeSorting
		{
			get
			{
				if (ConfigurationManager.AppSettings["HideWertehilfeSorting"] == null)
				{
					return false;
				}
				return bool.Parse(ConfigurationManager.AppSettings["HideWertehilfeSorting"].ToString());
			}
		}

		public static int PasswordMinimumLength
		{
			get
			{
				if (ConfigurationManager.AppSettings["PasswordMinimumLength"] == null)
				{
					return 8;
				}
				return int.Parse(ConfigurationManager.AppSettings["PasswordMinimumLength"].ToString());
			}
		}

		public static bool DynamicWertehilfe
		{
			get
			{
				if (ConfigurationManager.AppSettings["DynamicWertehilfe"] != null)
				{
					return (ConfigurationManager.AppSettings["DynamicWertehilfe"] == "true") ? true : false;
				}
				return false;
			}
		}

		public static bool ShowDevelopmentProperties => ConfigurationManager.AppSettings["ShowDevelopmentProperties"] != null && Convert.ToBoolean(ConfigurationManager.AppSettings["ShowDevelopmentProperties"]);

		public static bool HasAnyDSVBug => ConfigurationManager.AppSettings["HasAnyDSVBug"] != null && Convert.ToBoolean(ConfigurationManager.AppSettings["HasAnyDSVBug"]);

		public static bool OnlyGermanLanguageEnabled => ConfigurationManager.AppSettings["OnlyGermanLanguageEnabled"] != null && Convert.ToBoolean(ConfigurationManager.AppSettings["OnlyGermanLanguageEnabled"]);

		public static bool UseNewIssueMethod => ConfigurationManager.AppSettings["UseNewIssueMethod"] != null && Convert.ToBoolean(ConfigurationManager.AppSettings["UseNewIssueMethod"]);

		public static bool UseDocumentsPreview => ConfigurationManager.AppSettings["UseDocumentsPreview"] != null && Convert.ToBoolean(ConfigurationManager.AppSettings["UseDocumentsPreview"]);

		public static bool IsOnTheFlyConvertion => ConfigurationManager.AppSettings["IsOnTheFlyConvertion"] != null && Convert.ToBoolean(ConfigurationManager.AppSettings["IsOnTheFlyConvertion"]);

		public static string LogPath => (ConfigurationManager.AppSettings["LogPath"] != string.Empty) ? ConfigurationManager.AppSettings["LogPath"].ToString() : "";

		public static bool ADImportSupported => ConfigurationManager.AppSettings["ADImportSupported"] != null && Convert.ToBoolean(ConfigurationManager.AppSettings["ADImportSupported"]);

		public static bool DropUserSchemas
		{
			get
			{
				if (ConfigurationManager.AppSettings["DropUserSchemas"] != null)
				{
					return (ConfigurationManager.AppSettings["DropUserSchemas"] == "true") ? true : false;
				}
				return false;
			}
		}

		public static bool BelegFileMerge
		{
			get
			{
				if (ConfigurationManager.AppSettings["BelegFileMerge"] != null)
				{
					return (ConfigurationManager.AppSettings["BelegFileMerge"] == "true") ? true : false;
				}
				return false;
			}
		}

		public static long SaveFilterIndexLimit
		{
			get
			{
				if (ConfigurationManager.AppSettings["SaveFilterIndexLimit"] != null && long.TryParse(ConfigurationManager.AppSettings["SaveFilterIndexLimit"], out var limit))
				{
					return limit;
				}
				return 50000L;
			}
		}

		public static bool MerckFileMerge
		{
			get
			{
				if (ConfigurationManager.AppSettings["MerckFileMerge"] != null)
				{
					return (ConfigurationManager.AppSettings["MerckFileMerge"] == "true") ? true : false;
				}
				return false;
			}
		}

		public static string ConvertedMerckFile => ConfigurationManager.AppSettings["ConvertedMerckFile"];

		public static bool AFPConverter
		{
			get
			{
				if (ConfigurationManager.AppSettings["AFPConverter"] != null)
				{
					return (ConfigurationManager.AppSettings["AFPConverter"] == "true") ? true : false;
				}
				return false;
			}
		}

		public static string OverlaysDirectory => ConfigurationManager.AppSettings["OverlaysDirectory"];

		public static string ConvertedAFPFilesDirectory => ConfigurationManager.AppSettings["ConvertedAFPFilesDirectory"];

		public static bool PayRollConverter
		{
			get
			{
				if (ConfigurationManager.AppSettings["PayRollConverter"] != null)
				{
					return ConfigurationManager.AppSettings["PayRollConverter"] == "true";
				}
				return false;
			}
		}

		public static bool BelegUseExtensionByTheExtesionColumnNotByFile
		{
			get
			{
				if (ConfigurationManager.AppSettings["BelegUseExtensionByTheExtesionColumnNotByFile"] != null)
				{
					return (ConfigurationManager.AppSettings["BelegUseExtensionByTheExtesionColumnNotByFile"] == "true") ? true : false;
				}
				return false;
			}
		}

		public static List<string> RequiredLanguages
		{
			get
			{
				if (ConfigurationManager.AppSettings["RequiredLanguages"] != null)
				{
					return ConfigurationManager.AppSettings["RequiredLanguages"].Split(',').ToList();
				}
				return null;
			}
		}

		public static string TemporaryDirectory => ConfigurationManager.AppSettings["ViewboxTemporaryDirectory"];

		public static string DocumentsConfigDirectory => ConfigurationManager.AppSettings["ViewboxDocumentsConfigDirectory"];

		public static string DocumentsConfigDirectory1 => ConfigurationManager.AppSettings["ViewboxDocumentsConfigDirectory1"];

		public static string DocumentsConfigDirectory2 => ConfigurationManager.AppSettings["ViewboxDocumentsConfigDirectory2"];

		public static string DocumentsDirectory => ConfigurationManager.AppSettings["ViewboxDocumentsDirectory"];

		public static string DocumentsDirectory1
		{
			get
			{
				if (ConfigurationManager.AppSettings.AllKeys.Contains("ViewboxDocumentsDirectory1"))
				{
					return ConfigurationManager.AppSettings["ViewboxDocumentsDirectory1"];
				}
				return null;
			}
		}

		public static string DocumentsDirectory2
		{
			get
			{
				if (ConfigurationManager.AppSettings.AllKeys.Contains("ViewboxDocumentsDirectory2"))
				{
					return ConfigurationManager.AppSettings["ViewboxDocumentsDirectory2"];
				}
				return null;
			}
		}

		public static string DocumentsDirectory3
		{
			get
			{
				if (ConfigurationManager.AppSettings.AllKeys.Contains("ViewboxDocumentsDirectory3"))
				{
					return ConfigurationManager.AppSettings["ViewboxDocumentsDirectory3"];
				}
				return null;
			}
		}

		public static string DocumentsPreviewDirectory => ConfigurationManager.AppSettings["ViewboxDocumentsPreviewDirectory"];

		public static string DocumentsPreviewDirectory1
		{
			get
			{
				if (ConfigurationManager.AppSettings.AllKeys.Contains("ViewboxDocumentsPreviewDirectory1"))
				{
					return ConfigurationManager.AppSettings["ViewboxDocumentsPreviewDirectory1"];
				}
				return null;
			}
		}

		public static string DocumentsPreviewDirectory2
		{
			get
			{
				if (ConfigurationManager.AppSettings.AllKeys.Contains("ViewboxDocumentsPreviewDirectory2"))
				{
					return ConfigurationManager.AppSettings["ViewboxDocumentsPreviewDirectory2"];
				}
				return null;
			}
		}

		public static string DocumentsPreviewDirectory3
		{
			get
			{
				if (ConfigurationManager.AppSettings.AllKeys.Contains("ViewboxDocumentsPreviewDirectory3"))
				{
					return ConfigurationManager.AppSettings["ViewboxDocumentsPreviewDirectory3"];
				}
				return null;
			}
		}

		public static string DocumentsAttachmentDirectory => ConfigurationManager.AppSettings["ViewboxDocumentsAttachmentDirectory"];

		public static string DocumentsAttachmentDirectory1
		{
			get
			{
				if (ConfigurationManager.AppSettings.AllKeys.Contains("ViewboxDocumentsAttachmentDirectory1"))
				{
					return ConfigurationManager.AppSettings["ViewboxDocumentsAttachmentDirectory1"];
				}
				return null;
			}
		}

		public static string DocumentsAttachmentDirectory2
		{
			get
			{
				if (ConfigurationManager.AppSettings.AllKeys.Contains("ViewboxDocumentsAttachmentDirectory2"))
				{
					return ConfigurationManager.AppSettings["ViewboxDocumentsAttachmentDirectory"];
				}
				return null;
			}
		}

		public static string DocumentsAttachmentDirectory3
		{
			get
			{
				if (ConfigurationManager.AppSettings.AllKeys.Contains("ViewboxDocumentsAttachmentDirectory3"))
				{
					return ConfigurationManager.AppSettings["ViewboxDocumentsAttachmentDirectory3"];
				}
				return null;
			}
		}

		public static string ThumbnailsDirectory => ConfigurationManager.AppSettings["ViewboxThumbnailsDirectory"];

		public static string ThumbnailsDirectory1
		{
			get
			{
				if (ConfigurationManager.AppSettings.AllKeys.Contains("ViewboxThumbnailsDirectory1"))
				{
					return ConfigurationManager.AppSettings["ViewboxThumbnailsDirectory1"];
				}
				return null;
			}
		}

		public static string ThumbnailsDirectory2
		{
			get
			{
				if (ConfigurationManager.AppSettings.AllKeys.Contains("ViewboxThumbnailsDirectory2"))
				{
					return ConfigurationManager.AppSettings["ViewboxThumbnailsDirectory2"];
				}
				return null;
			}
		}

		public static string ThumbnailsDirectory3
		{
			get
			{
				if (ConfigurationManager.AppSettings.AllKeys.Contains("ViewboxThumbnailsDirectory3"))
				{
					return ConfigurationManager.AppSettings["ViewboxThumbnailsDirectory3"];
				}
				return null;
			}
		}

		public static string ViewboxViewsciptFolder => ConfigurationManager.AppSettings["ViewboxViewsciptFolder"] ?? TemporaryDirectory;

		public static string ViewboxBasePath
		{
			get
			{
				return ConfigurationManager.AppSettings["ViewboxBasePath"];
			}
			internal set
			{
				ConfigurationManager.AppSettings["ViewboxBasePath"] = value;
			}
		}

		public static string ViewboxMailAddress => ConfigurationManager.AppSettings["ViewboxMailAddress"];

		public static string DataProvider
		{
			get
			{
				return ConfigurationManager.AppSettings["DataProvider"];
			}
			internal set
			{
				ConfigurationManager.AppSettings["DataProvider"] = value;
			}
		}

		public static string TempDatabaseName
		{
			get
			{
				return ConfigurationManager.AppSettings["TempDatabaseName"];
			}
			internal set
			{
				ConfigurationManager.AppSettings["TempDatabaseName"] = value;
			}
		}

		public static string IndexDatabasePostFix
		{
			get
			{
				return ConfigurationManager.AppSettings["IndexDatabasePostFix"] ?? "_index";
			}
			internal set
			{
				ConfigurationManager.AppSettings["IndexDatabasePostFix"] = value;
			}
		}

		public static string ImportDatabaseName
		{
			get
			{
				return ConfigurationManager.AppSettings["ImportDatabaseName"];
			}
			internal set
			{
				ConfigurationManager.AppSettings["ImportDatabaseName"] = value;
			}
		}

		public static bool CheckCharacterCodingInForLogging => ConfigurationManager.AppSettings["CheckCharacterCodingInLogging"] != null && Convert.ToBoolean(ConfigurationManager.AppSettings["CheckCharacterCodingInLogging"]);

		public static TimeSpan TemporaryObjectsLifetime => TimeSpan.FromHours(double.Parse(ConfigurationManager.AppSettings["ViewboxTemporaryObjectsLifetimeHours"]));

		public static TimeSpan TemporaryCheckInterval => TimeSpan.FromMinutes(double.Parse(ConfigurationManager.AppSettings["ViewboxTemporaryCheckIntervalMinutes"]));

		public static bool HideIssuesButton => ConfigurationManager.AppSettings["HideIssuesButton"] != null && Convert.ToBoolean(ConfigurationManager.AppSettings["HideIssuesButton"]);

		public static bool HideViewsButton => ConfigurationManager.AppSettings["HideViewsButton"] != null && Convert.ToBoolean(ConfigurationManager.AppSettings["HideViewsButton"]);

		public static string SMTPServer => ConfigurationManager.AppSettings["SMTPServer"] ?? "brockman.av.local";

		public static bool HideTablesButton => ConfigurationManager.AppSettings["HideTablesButton"] != null && Convert.ToBoolean(ConfigurationManager.AppSettings["HideTablesButton"]);

		public static bool HideDocumentsMenu => ConfigurationManager.AppSettings["HideDocumentsMenu"] != null && Convert.ToBoolean(ConfigurationManager.AppSettings["HideDocumentsMenu"]);

		public static bool MayrMelnhofBelegarchiv => ConfigurationManager.AppSettings["MayrMelnhofBelegarchiv"] != null && Convert.ToBoolean(ConfigurationManager.AppSettings["MayrMelnhofBelegarchiv"]);

		public static bool Initializing
		{
			get
			{
				return Application != null && Application["ViewboxApplication.Initializing"] != null && (bool)Application["ViewboxApplication.Initializing"];
			}
			set
			{
				Application["ViewboxApplication.Initializing"] = value;
			}
		}

		public static bool Initialized
		{
			get
			{
				return Application != null && Application["ViewboxApplication.Initialized"] != null && (bool)Application["ViewboxApplication.Initialized"];
			}
			private set
			{
				Application["ViewboxApplication.Initialized"] = value;
			}
		}

		public static DatabaseBaseOutOfDateInformation DatabaseOutOfDateInformation
		{
			get
			{
				return (DatabaseOutOfDateInformation)Application["ViewboxApplication.DatabaseOutOfDateInformation"];
			}
			private set
			{
				Application["ViewboxApplication.DatabaseOutOfDateInformation"] = value;
			}
		}

		public static TimeSpan InitTime => ServerInitializedTime - ServerStart;

		public static DateTime ServerStart
		{
			get
			{
				return (DateTime)Application["ViewboxApplication.ServerStart"];
			}
			private set
			{
				Application["ViewboxApplication.ServerStart"] = value;
			}
		}

		public static DateTime ServerInitializedTime
		{
			get
			{
				return (DateTime)Application["ViewboxApplication.ServerInitializedTime"];
			}
			private set
			{
				Application["ViewboxApplication.ServerInitializedTime"] = value;
			}
		}

		public static global::ViewboxDb.ViewboxDb Database
		{
			get
			{
				return Application["ViewboxApplication.Database"] as global::ViewboxDb.ViewboxDb;
			}
			set
			{
				Application["ViewboxApplication.Database"] = value;
			}
		}

		public static INotesCollection Notes
		{
			get
			{
				return Application["ViewboxApplication.Notes"] as INotesCollection;
			}
			set
			{
				Application["ViewboxApplication.Notes"] = value;
			}
		}

		public static ITableCrudCollection TableCruds
		{
			get
			{
				return Application["ViewboxApplication.TableCruds"] as ITableCrudCollection;
			}
			set
			{
				Application["ViewboxApplication.TableCruds"] = value;
			}
		}

		public static ViewboxDb.TableObjectCollection TempTableObjects
		{
			get
			{
				return Application["ViewboxApplication.TempTableObjects"] as ViewboxDb.TableObjectCollection;
			}
			set
			{
				Application["ViewboxApplication.TempTableObjects"] = value;
			}
		}

		public static string YearRangeStart
		{
			get
			{
				if (ConfigurationManager.AppSettings["YearRangeStart"] != null && ConfigurationManager.AppSettings["YearRangeEnd"] != null)
				{
					return ConfigurationManager.AppSettings["YearRangeStart"];
				}
				return "1970";
			}
		}

		public static string YearRangeEnd
		{
			get
			{
				if (ConfigurationManager.AppSettings["YearRangeStart"] != null && ConfigurationManager.AppSettings["YearRangeEnd"] != null)
				{
					return ConfigurationManager.AppSettings["YearRangeEnd"];
				}
				return (DateTime.Now.Year + 1).ToString();
			}
		}

		public static string HigherSecurityPasswordRegex => "^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[#?!@$%^&*-]).{10,}$";

		public static string HighSecurityPasswordRegex => "^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[#?!@$%^&*-]).{8,}$";

		public static string MediumSecurityPasswordRegex => "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).{8,}$";

		public static string LowSecurityPasswordRegex => "^(?=.*[A-Za-z])(?=.*\\d).{8,}$";

		public static string PasswordPolicyDescription
		{
			get
			{
				string desc = "";
				switch (ConfigurationManager.AppSettings["PasswordSecurityLevel"])
				{
				case "1":
					desc = Resources.LowSecurityPasswordPolicy;
					break;
				case "2":
					desc = Resources.MediumSecurityPasswordPolicy;
					break;
				case "3":
					desc = Resources.HighSecurityPasswordPolicy;
					break;
				case "4":
					desc = Resources.HigherSecurityPasswordPolicy;
					break;
				default:
					return "";
				}
				return Resources.PasswordPolicyViolation + " (" + desc + ")";
			}
		}

		public static string PasswordJustPolicyDescription => ConfigurationManager.AppSettings["PasswordSecurityLevel"] switch
		{
			"1" => Resources.LowSecurityPasswordPolicy, 
			"2" => Resources.MediumSecurityPasswordPolicy, 
			"3" => Resources.HighSecurityPasswordPolicy, 
			"4" => Resources.HigherSecurityPasswordPolicy, 
			_ => "", 
		};

		public static string PasswordRegex => ConfigurationManager.AppSettings["PasswordSecurityLevel"] switch
		{
			"1" => LowSecurityPasswordRegex, 
			"2" => MediumSecurityPasswordRegex, 
			"3" => HighSecurityPasswordRegex, 
			"4" => HigherSecurityPasswordRegex, 
			_ => ".*", 
		};

		public static int MinPasswordLength => ConfigurationManager.AppSettings["PasswordSecurityLevel"] switch
		{
			"1" => 6, 
			"2" => 6, 
			"3" => 8, 
			_ => 0, 
		};

		public static UrlHelper Url => new UrlHelper(new RequestContext(HttpContextFactory.Current, new RouteData()));

		public static IFileObjectCollection FileObjects => Database.SystemDb.FileObjects;

		public static IDirectoryObjectCollection DirectoryObjects => Database.SystemDb.DirectoryObjects;

		public static IFileSysCollection FileSystems => Database.SystemDb.FileSystems;

		public static bool IsFilesSystemInitialized { get; set; }

		public static IUserIdCollection Users => Database.SystemDb.Users;

		public static IRoleCollection Roles => Database.SystemDb.Roles;

		internal static SessionMarker UserSessions
		{
			get
			{
				return Application["ViewboxApplication.CurrentUserSessions"] as SessionMarker;
			}
			set
			{
				Application["ViewboxApplication.CurrentUserSessions"] = value;
			}
		}

		private static List<ILanguage> languages
		{
			get
			{
				return Application["ViewboxApplication.Languages"] as List<ILanguage>;
			}
			set
			{
				Application["ViewboxApplication.Languages"] = value;
			}
		}

		public static IEnumerable<ILanguage> Languages => languages;

		public static ILanguage BrowserLanguage => GetLanguageByCountryCode(RequiredLanguages.First());

		public static ILanguage DefaultLanguage => languages.First();

		public static bool ShowAdminUsersInUserManagement
		{
			get
			{
				if (ConfigurationManager.AppSettings["ShowAdminUsersInUserManagement"] != null)
				{
					string settingValue = ConfigurationManager.AppSettings["ShowAdminUsersInUserManagement"];
					return settingValue.ToLower() == "true";
				}
				return false;
			}
		}

		public static void Init(HttpApplicationStateBase application)
		{
			if (Initializing || Initialized)
			{
				throw new InvalidOperationException("already initialized");
			}
			_application = application;
			_processorTime = new EncapsulatedPerformanceCounter(new PerformanceCounter("Processor", "% Processor Time", "_Total", readOnly: true));
			_hardDiskTransferSpeed = new EncapsulatedPerformanceCounter(new PerformanceCounter("PhysicalDisk", "Disk Transfers/sec", "_Total", readOnly: true));
			_processorTime.NextValue();
			_hardDiskTransferSpeed.NextValue();
			Init();
		}

		public static void Init()
		{
			try
			{
				Janitor.Stop();
				ServerInitializedTime = (ServerStart = DateTime.Now);
				Initializing = true;
				Initialized = false;
				global::ViewboxDb.ViewboxDb.UseNewIssueMethod = UseNewIssueMethod;
				Database = new global::ViewboxDb.ViewboxDb(TempDatabaseName, IndexDatabasePostFix);
				OnTheFlyConverter.InitNReco();
				Database.ViewboxDbInitialized += ViewboxDatabaseInitialized;
				//ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
				Database.Connect(DataProvider, Settings.Default.ViewboxDatabase);
				Database.CreateServerLogEntry(ServerInitializedTime);
				UserSessions = new SessionMarker();
				try
				{
					foreach (string file in Directory.EnumerateFiles(TemporaryDirectory, "*.zip"))
					{
						File.Delete(file);
					}
				}
				catch
				{
				}
				Janitor.Start();
				TempTableObjects = new ViewboxDb.TableObjectCollection();
				Notes = Database.SystemDb.Notes;
				TableCruds = Database.SystemDb.TableCruds;
				languages = new List<ILanguage>
				{
					new Language
					{
						CountryCode = "de",
						LanguageName = SystemDb.Internal.Language.LanguageDescriptions["de"].Item1,
						LanguageMotto = SystemDb.Internal.Language.LanguageDescriptions["de"].Item2
					},
					new Language
					{
						CountryCode = "en",
						LanguageName = SystemDb.Internal.Language.LanguageDescriptions["en"].Item1,
						LanguageMotto = SystemDb.Internal.Language.LanguageDescriptions["en"].Item2
					}
				};
				DatabaseSpecificFactory.Init(DataProvider);
			}
			catch (Exception ex)
			{
				Initializing = false;
				Initialized = false;
				_log.ErrorFormat("The following error message is occured during starting application: {0}", ex.Message);
			}
		}

		private static void ViewboxDatabaseInitialized(object sender, EventArgs args)
		{
			using (NDC.Push(LogHelper.GetNamespaceContext()))
			{
				try
				{
					DatabaseOutOfDateInformation = Database.SystemDb.DatabaseOutOfDateInformation;
					if (Database.SystemDb.DatabaseOutOfDateInformation == null)
					{
						if (Database.SystemDb.Languages.Count > 0)
						{
							languages = new List<ILanguage>(Database.SystemDb.Languages);
						}
						ServerInitializedTime = DateTime.Now;
						Initialized = true;
						Initializing = false;
					}
					_log.DebugFormat("Viewbox database initialized successfully!");
				}
				catch (Exception ex)
				{
					_log.Error(ex.Message, ex);
					throw;
				}
			}
		}

		public static IPropertiesCollection GetProperties()
		{
			return Database.SystemDb.GetProperties(ViewboxSession.User);
		}

		private static IPropertiesCollection GetProperties(IUser user)
		{
			return Database.SystemDb.GetProperties(user);
		}

		public static IProperty FindProperty(string key)
		{
			IProperty property = null;
			if (ViewboxSession.User != null)
			{
				foreach (IProperty p in GetProperties())
				{
					if (p.Key == key)
					{
						property = p;
						break;
					}
				}
			}
			return property;
		}

		public static ICoreProperty FindCoreProperty(string key)
		{
			ICoreProperty property = null;
			try
			{
				foreach (ICoreProperty p in GetCoreProperties())
				{
					if (p.Key == key)
					{
						property = p;
						break;
					}
				}
			}
			catch
			{
			}
			return property;
		}

		public static IProperty FindProperty(IUser user, string key)
		{
			IProperty property = null;
			if (user != null)
			{
				foreach (IProperty p in GetProperties(user))
				{
					if (p.Key == key)
					{
						property = p;
						break;
					}
				}
			}
			return property;
		}

		private static ICorePropertiesCollection GetCoreProperties()
		{
			return Database.SystemDb.GetCoreProperties();
		}

		public static IEnumerable<GuvStructureRow> GetGuvBalanceStructure(ITableObject tobj, string dataBase)
		{
			using DatabaseBase db = ConnectionManager.CreateConnection(DataProvider, Settings.Default.ViewboxDatabase);
			db.Open();
			db.ChangeDatabase(dataBase);
			return db.DbMapping.Load<GuvStructureRow>();
		}

		public static IEnumerable<GuvStructureSignRow> GetGuvBalanceData(ITableObject tobj, string key, string dataBase)
		{
			DataTable datatable = ViewboxSession.LoadDataTableMin(tobj, dataBase);
			List<GuvStructureSignRow> res = new List<GuvStructureSignRow>();
			foreach (DataRow row in datatable.Rows)
			{
				GuvStructureSignRow account = new GuvStructureSignRow();
				account.Konto = GetStringValue(row["konto"]);
				account.Konto_Be = GetStringValue(row["konto_bezeichnung"]);
				account.Monat_S = GetStringValue(row["monat_saldo"]);
				account.Pos = GetStringValue(row["parent"]);
				account.Year_S = GetStringValue(row["year_saldo"]);
				res.Add(account);
			}
			return res;
		}

		public static IEnumerable<BilanzStructureRow> GetBilanzBalanceStructure(ITableObject tobj, string dataBase)
		{
			using DatabaseBase db = ConnectionManager.CreateConnection(DataProvider, Settings.Default.ViewboxDatabase);
			db.Open();
			db.ChangeDatabase(dataBase);
			return db.DbMapping.Load<BilanzStructureRow>();
		}

		public static List<BilanzStructureSignRow> GetBilanzBalanceData(ITableObject tobj, string key, string dataBase)
		{
			List<PropertyInfo> props = typeof(BilanzStructureSignRow).GetProperties().ToList();
			DataTable datatable = ViewboxSession.LoadDataTableMin(tobj, dataBase);
			List<BilanzStructureSignRow> res = new List<BilanzStructureSignRow>();
			foreach (DataRow row in datatable.Rows)
			{
				BilanzStructureSignRow account = new BilanzStructureSignRow();
				account.Konto = GetStringValue(row["konto"]);
				account.Konto_Be = GetStringValue(row["konto_bezeichnung"]);
				account.Monat_S = GetStringValue(row["monat_saldo"]);
				account.Pos = GetStringValue(row["parent"]);
				account.Year_S = GetStringValue(row["year_saldo"]);
				res.Add(account);
			}
			return res;
		}

		public static IEnumerable<IBilanzstrukturAnzeigen> GetBilanzstukturAnzeigen(ITableObject tobj, string key, string dataBase, bool newVersion)
		{
			DataTable datatable = ViewboxSession.LoadDataTableMin(tobj, dataBase);
			if (newVersion)
			{
				List<BilanzstrukturAnzeigenNew> res = new List<BilanzstrukturAnzeigenNew>();
				foreach (DataRow row2 in datatable.Rows)
				{
					BilanzstrukturAnzeigenNew account2 = new BilanzstrukturAnzeigenNew();
					account2.id = GetStringValue(row2["id"]);
					account2.parent = GetStringValue(row2["parent"]);
					account2.titel = GetStringValue(row2["titel"]);
					res.Add(account2);
				}
				return res;
			}
			List<BilanzstrukturAnzeigen> res2 = new List<BilanzstrukturAnzeigen>();
			foreach (DataRow row in datatable.Rows)
			{
				BilanzstrukturAnzeigen account = new BilanzstrukturAnzeigen();
				account.id = GetStringValue(row["id"]);
				account.parent = GetStringValue(row["parent"]);
				account.titel = GetStringValue(row["titel"]);
				res2.Add(account);
			}
			return res2;
		}

		public static IEnumerable<Workflowprotokoll> GetWorkflowprotokoll(ITableObject tobj, string key, string dataBase)
		{
			DataTable datatable = ViewboxSession.LoadDataTableMin(tobj, dataBase);
			List<Workflowprotokoll> res = new List<Workflowprotokoll>();
			foreach (DataRow row in datatable.Rows)
			{
				Workflowprotokoll account = new Workflowprotokoll();
				account.MANDT = GetStringValue(row["MANDT"]);
				account.WI_ID_ORIG = GetStringValue(row["WI_ID_ORIG"]);
				account.WI_PARENT = GetStringValue(row["WI_PARENT"]);
				account.WI_ID = GetStringValue(row["WI_ID"]);
				account.WI_TEXT = GetStringValue(row["WI_TEXT"]);
				account.WI_STAT_TXT = GetStringValue(row["WI_STAT_TXT"]);
				account.WI_CD = GetStringValue(row["WI_CD"]);
				account.WI_CT = GetStringValue(row["WI_CT"]);
				res.Add(account);
			}
			return res;
		}

		public static IEnumerable<KostenartenhierarhyStructure> GetKostenartenhierarhyStructure(ITableObject tobj, string key, string sub_type, string language = null)
		{
			using DatabaseBase db = ConnectionManager.CreateConnection(DataProvider, Settings.Default.ViewboxDatabase);
			db.Open();
			db.ChangeDatabase(tobj.Database);
			UpdateKostenartenhierarhyStructureTable(db, tobj);
			return db.DbMapping.Load<KostenartenhierarhyStructure>($"mandt = '{key}' AND sub_type = '{sub_type}'" + ((language != null) ? $" AND LANG_KEY = '{language}'" : ""));
		}

		private static void UpdateKostenartenhierarhyStructureTable(DatabaseBase db, ITableObject tobj)
		{
			string tableName = db.DbMapping.GetTableName(typeof(KostenartenhierarhyStructure));
			List<string> columns = db.GetColumnNames(db.DbConfig.DbName, tableName);
			string newColumn1Name = db.DbMapping.GetColumnName<KostenartenhierarhyStructure>("Mandt");
			string newColumn2Name = db.DbMapping.GetColumnName<KostenartenhierarhyStructure>("Pos");
			string newColumn3Name = db.DbMapping.GetColumnName<KostenartenhierarhyStructure>("Description");
			string newColumn4Name = db.DbMapping.GetColumnName<KostenartenhierarhyStructure>("Parent");
			if (columns.All((string c) => c.ToLower() != newColumn1Name.ToLower()))
			{
				db.DbMapping.AddColumn(tableName, "String", new DbColumnAttribute(newColumn1Name), null);
			}
			if (columns.All((string c) => c.ToLower() != newColumn2Name.ToLower()))
			{
				db.DbMapping.AddColumn(tableName, "String", new DbColumnAttribute(newColumn2Name), null);
			}
			if (columns.All((string c) => c.ToLower() != newColumn3Name.ToLower()))
			{
				db.DbMapping.AddColumn(tableName, "String", new DbColumnAttribute(newColumn3Name), null);
			}
			if (columns.All((string c) => c.ToLower() != newColumn4Name.ToLower()))
			{
				db.DbMapping.AddColumn(tableName, "String", new DbColumnAttribute(newColumn4Name), null);
			}
		}

		public static List<KostenartenhierarhyData> GetKostenartenhierarhyData(ITableObject tobj, string key, string dataBase)
		{
			List<PropertyInfo> props = typeof(KostenartenhierarhyData).GetProperties().ToList();
			DataTable datatable = ViewboxSession.LoadDataTableMin(tobj, dataBase);
			List<KostenartenhierarhyData> res = new List<KostenartenhierarhyData>();
			foreach (DataRow row in datatable.Rows)
			{
				KostenartenhierarhyData account = new KostenartenhierarhyData();
				account.Mandant = GetStringValue(row["mandt"]);
				account.Konto = GetStringValue(row["kstar"]);
				account.Konto_Be = GetStringValue(row["kstar_desc"]);
				account.Year_S = GetStringValue(row["ktopl"]);
				account.Pos = GetStringValue(row["Parent"]);
				res.Add(account);
			}
			return res;
		}

		public static IEnumerable<KostenstellengruppeAnzeigenStruct> GetKostenstellengruppeAnzeigenStruct(ITableObject tobj, string key, string sub_type, string language = null)
		{
			using DatabaseBase db = ConnectionManager.CreateConnection(DataProvider, Settings.Default.ViewboxDatabase);
			db.Open();
			db.ChangeDatabase(tobj.Database);
			UpdateKostenstellengruppeAnzeigenStructTable(db, tobj);
			return db.DbMapping.Load<KostenstellengruppeAnzeigenStruct>($"mandt = '{key}' AND sub_type = '{sub_type}'" + ((language != null) ? $" AND LANG_KEY = '{language}'" : ""));
		}

		private static void UpdateKostenstellengruppeAnzeigenStructTable(DatabaseBase db, ITableObject tobj)
		{
			string tableName = db.DbMapping.GetTableName(typeof(KostenstellengruppeAnzeigenStruct));
			List<string> columns = db.GetColumnNames(db.DbConfig.DbName, tableName);
			string newColumn1Name = db.DbMapping.GetColumnName<KostenstellengruppeAnzeigenStruct>("Mandt");
			string newColumn2Name = db.DbMapping.GetColumnName<KostenstellengruppeAnzeigenStruct>("Pos");
			string newColumn3Name = db.DbMapping.GetColumnName<KostenstellengruppeAnzeigenStruct>("Description");
			string newColumn4Name = db.DbMapping.GetColumnName<KostenstellengruppeAnzeigenStruct>("Parent");
			if (columns.All((string c) => c.ToLower() != newColumn1Name.ToLower()))
			{
				db.DbMapping.AddColumn(tableName, "String", new DbColumnAttribute(newColumn1Name), null);
			}
			if (columns.All((string c) => c.ToLower() != newColumn2Name.ToLower()))
			{
				db.DbMapping.AddColumn(tableName, "String", new DbColumnAttribute(newColumn2Name), null);
			}
			if (columns.All((string c) => c.ToLower() != newColumn3Name.ToLower()))
			{
				db.DbMapping.AddColumn(tableName, "String", new DbColumnAttribute(newColumn3Name), null);
			}
			if (columns.All((string c) => c.ToLower() != newColumn4Name.ToLower()))
			{
				db.DbMapping.AddColumn(tableName, "String", new DbColumnAttribute(newColumn4Name), null);
			}
		}

		public static List<KostenstellengruppeAnzeigenData> GetKostenstellengruppeAnzeigenData(ITableObject tobj, string key, string dataBase)
		{
			List<PropertyInfo> props = typeof(KostenstellengruppeAnzeigenData).GetProperties().ToList();
			DataTable datatable = ViewboxSession.LoadDataTableMin(tobj, dataBase);
			List<KostenstellengruppeAnzeigenData> res = new List<KostenstellengruppeAnzeigenData>();
			foreach (DataRow row in datatable.Rows)
			{
				KostenstellengruppeAnzeigenData account = new KostenstellengruppeAnzeigenData();
				account.Mandant = GetStringValue(row["mandt"]);
				account.Konto = GetStringValue(row["kostl"]);
				account.Konto_Be = GetStringValue(row["kostl_desc"]);
				account.Year_S = GetStringValue(row["kokrs"]);
				account.Pos = GetStringValue(row["parent"]);
				res.Add(account);
			}
			return res;
		}

		public static List<Viewbox.SapBalance.StructureRow> GetBalanceStructure(ITableObject tobj, string key, string languageKey)
		{
			using DatabaseBase db = ConnectionManager.CreateConnection(DataProvider, Settings.Default.ViewboxDatabase);
			db.Open();
			db.ChangeDatabase(tobj.Database);
			UpdateBalanceStructureTable(db);
			List<Viewbox.SapBalance.StructureRow> list = new List<Viewbox.SapBalance.StructureRow>();
			list = db.DbMapping.Load<Viewbox.SapBalance.StructureRow>(string.Format("BilStr = '{0}' " + ((tobj as IIssue).UseLanguageValue ? "AND (Lang_key = '{1}' OR lang_key = '')" : ""), key, languageKey));
			return new List<Viewbox.SapBalance.StructureRow>(from o in list
				orderby o.Id, o.Level
				select o);
		}

		public static IEnumerable<Summenbericht> GetSummenbericht(ITableObject tobj, string key, string dataBase)
		{
			DataTable datatable = ViewboxSession.LoadDataTableMin(tobj, dataBase);
			List<Summenbericht> res = new List<Summenbericht>();
			foreach (DataRow row in datatable.Rows)
			{
				Summenbericht account = new Summenbericht();
				account.MANDT = GetStringValue(row["MANDT"]);
				account.VBUKR = GetStringValue(row["VBUKR"]);
				account.ID = GetStringValue(row["ID"]);
				account.Parent = GetStringValue(row["Parent"]);
				account.Objekt = GetStringValue(row["Objekt"]);
				account.Descr = GetStringValue(row["Descr"]);
				account.GenSoll = GetStringValue(row["Gen.Soll"]);
				account.Istkosten = GetStringValue(row["Istkosten"]);
				account.Obligo = GetStringValue(row["Obligo"]);
				account.Anzahlung = GetStringValue(row["Anzahlung"]);
				account.lief = ((GetStringValue(row["lief"]).ToLower() == "false") ? "0" : "1");
				res.Add(account);
			}
			return res;
		}

		public static IEnumerable<SummenberichtFgnStructure> GetSummenberichtFgnStructure(ITableObject tobj, string key, string language = null)
		{
			using DatabaseBase db = ConnectionManager.CreateConnection(DataProvider, Settings.Default.ViewboxDatabase);
			db.Open();
			db.ChangeDatabase(tobj.Database);
			return db.DbMapping.Load<SummenberichtFgnStructure>();
		}

		private static void UpdateSummenberichtFgnStructureTable(DatabaseBase db, ITableObject tobj)
		{
			string tableName = db.DbMapping.GetTableName(typeof(SummenberichtFgnStructure));
			List<string> columns = db.GetColumnNames(db.DbConfig.DbName, tableName);
			string newColumn1Name = db.DbMapping.GetColumnName<SummenberichtFgnStructure>("idparent");
			string newColumn2Name = db.DbMapping.GetColumnName<SummenberichtFgnStructure>("idchild");
			if (columns.All((string c) => c.ToLower() != newColumn1Name.ToLower()))
			{
				db.DbMapping.AddColumn(tableName, "String", new DbColumnAttribute(newColumn1Name), null);
			}
			if (columns.All((string c) => c.ToLower() != newColumn2Name.ToLower()))
			{
				db.DbMapping.AddColumn(tableName, "String", new DbColumnAttribute(newColumn2Name), null);
			}
		}

		public static List<SummenberichtFgnData> GetSummenberichtFgnData(ITableObject tobj, string key, string dataBase)
		{
			List<PropertyInfo> props = typeof(SummenberichtFgnData).GetProperties().ToList();
			DataTable datatable = ViewboxSession.LoadDataTableMin(tobj, dataBase);
			List<SummenberichtFgnData> res = new List<SummenberichtFgnData>();
			foreach (DataRow row in datatable.Rows)
			{
				SummenberichtFgnData account = new SummenberichtFgnData();
				account.kstar = GetStringValue(row["kstar"]);
				account.ltext = GetStringValue(row["ltext"]);
				account.istkosten = GetStringValue(row["istkosten"]);
				account.id = GetStringValue(row["id"]);
				res.Add(account);
			}
			return res;
		}

		public static IEnumerable<Viewbox.DcwBalance.StructureRow> GetDcwBalanceStructure(ITableObject tobj, string key, int type, int version)
		{
			using DatabaseBase db = ConnectionManager.CreateConnection(DataProvider, Settings.Default.ViewboxDatabase);
			db.Open();
			db.ChangeDatabase(tobj.Database);
			List<Viewbox.DcwBalance.StructureRow> list = db.DbMapping.Load<Viewbox.DcwBalance.StructureRow>($"zdmnu = '{key}' AND type = {type} AND version = {version}");
			return new List<Viewbox.DcwBalance.StructureRow>(from o in list
				orderby o.Level, o.Key
				select o);
		}

		public static Dictionary<string, StructureSignRow> GetDcwBalanceStructureSign(ITableObject tobj, string key)
		{
			using DatabaseBase db = ConnectionManager.CreateConnection(DataProvider, Settings.Default.ViewboxDatabase);
			db.Open();
			db.ChangeDatabase(tobj.Database);
			if (!db.TableExists("_dcw_balance_structure_sign"))
			{
				return null;
			}
			List<StructureSignRow> list = db.DbMapping.Load<StructureSignRow>($"zdmnu = '{key}'");
			return list.ToDictionary((StructureSignRow signRow) => signRow.Key);
		}

		public static IEnumerable<StructureGraph> GetStructureGraph(ViewboxDb.TableObject obj, string userDatabase)
		{
			List<StructureGraph> res = new List<StructureGraph>();
			string tableName = (obj.OriginalTable as IIssue).Command.Replace("PROC", "GRAPH");
			DataTable datatable = ViewboxSession.LoadDataTableMin(tableName, userDatabase, obj.Table.Database.ToLower().Replace("_final", "_dynamic"));
			if (datatable != null && datatable.Rows.Count > 0)
			{
				ITableObject tobj = obj.Table;
				foreach (DataRow row in datatable.Rows)
				{
					StructureGraph account = new StructureGraph();
					account.ID = GetStringValue(row["ID"]);
					account.PARENT = GetStringValue(row["PARENT"]);
					int ordinal = 0;
					int.TryParse(GetStringValue(row["ORDINAL"]), out ordinal);
					account.ORDINAL = ordinal;
					int descCounter = 0;
					foreach (DataColumn dc in datatable.Columns)
					{
						if (dc.ColumnName.StartsWith("DESC_", StringComparison.OrdinalIgnoreCase))
						{
							account.DESC.Add(descCounter, GetStringValue(row[dc.ColumnName]));
							descCounter++;
						}
					}
					if (tobj.Columns.Any((IColumn c) => c.IsVisible && c.Name.Equals("MANDT", StringComparison.OrdinalIgnoreCase)))
					{
						account.MANDT = GetStringValue(row["MANDT"]);
					}
					if (tobj.Columns.Any((IColumn c) => c.IsVisible && c.Name.Equals("BUKRS", StringComparison.OrdinalIgnoreCase)) && (obj.OriginalTable as IIssue).NeedBukrs)
					{
						account.BUKRS = GetStringValue(row["BUKRS"]);
					}
					if (tobj.Columns.Any((IColumn c) => c.IsVisible && c.Name.Equals("GJAHR", StringComparison.OrdinalIgnoreCase)) && (obj.OriginalTable as IIssue).NeedGJahr)
					{
						account.GJAHR = GetStringValue(row["GJAHR"]);
					}
					if (tobj.Columns.Any((IColumn c) => c.IsVisible && c.Name.Equals("LANG_KEY", StringComparison.OrdinalIgnoreCase)) && (obj.OriginalTable as IIssue).UseLanguageValue)
					{
						account.LANG_KEY = GetStringValue(row["LANG_KEY"]);
					}
					res.Add(account);
				}
			}
			return res;
		}

		public static IEnumerable<StructureDyn> GetStructureDyn(ViewboxDb.TableObject obj, string userDatabase)
		{
			ITableObject tobj = obj.Table;
			DataTable datatable = ViewboxSession.LoadDataTableMin(tobj, userDatabase);
			List<StructureDyn> res = new List<StructureDyn>();
			foreach (DataRow row in datatable.Rows)
			{
				StructureDyn account = new StructureDyn();
				account.ID = GetStringValue(row["ID"]);
				account.PARENT = GetStringValue(row["PARENT"]);
				int ordinal = 0;
				int.TryParse(GetStringValue(row["ORDINAL"]), out ordinal);
				account.ORDINAL = ordinal;
				int descCounter = 0;
				foreach (IColumn dc in from x in obj.OriginalColumns
					orderby x.Ordinal
					where x.IsVisible
					select x)
				{
					if (dc.Name.StartsWith("DESC_", StringComparison.OrdinalIgnoreCase))
					{
						account.DESC.Add(descCounter, GetStringValue(row[dc.Name]));
						descCounter++;
					}
					else if (dc.Name.StartsWith("VALUE_", StringComparison.OrdinalIgnoreCase))
					{
						account.VALUE.Add(dc.Ordinal, GetDoubleValue(row[dc.Name]));
					}
					else if (dc.Name.StartsWith("FIELD", StringComparison.OrdinalIgnoreCase))
					{
						account.FIELD.Add(dc.Ordinal, GetStringValue(row[dc.Name]));
					}
				}
				if (tobj.Columns.Any((IColumn c) => c.IsVisible && c.Name.Equals("MANDT", StringComparison.OrdinalIgnoreCase)))
				{
					account.MANDT = GetStringValue(row["MANDT"]);
				}
				if (tobj.Columns.Any((IColumn c) => c.IsVisible && c.Name.Equals("BUKRS", StringComparison.OrdinalIgnoreCase)) && (obj.OriginalTable as IIssue).NeedBukrs)
				{
					account.BUKRS = GetStringValue(row["BUKRS"]);
				}
				if (tobj.Columns.Any((IColumn c) => c.IsVisible && c.Name.Equals("GJAHR", StringComparison.OrdinalIgnoreCase)) && (obj.OriginalTable as IIssue).NeedGJahr)
				{
					account.GJAHR = GetStringValue(row["GJAHR"]);
				}
				if (tobj.Columns.Any((IColumn c) => c.IsVisible && c.Name.Equals("LANG_KEY", StringComparison.OrdinalIgnoreCase)) && (obj.OriginalTable as IIssue).UseLanguageValue)
				{
					account.LANG_KEY = GetStringValue(row["LANG_KEY"]);
				}
				res.Add(account);
			}
			return res;
		}

		private static string GetStringValue(object obj)
		{
			return (obj == null) ? "" : obj.ToString();
		}

		private static double GetDoubleValue(object obj)
		{
			double.TryParse(GetStringValue(obj), out var val);
			return val;
		}

		public static List<Viewbox.DcwBalance.AccountRow> GetDcwAccounts(ITableObject tobj, string database, bool hasVJahr, bool hasPer, bool hasVon, bool hasBis, bool hasMandtSplit)
		{
			DataTable datatable = ViewboxSession.LoadDataTableMin(tobj, database);
			List<Viewbox.DcwBalance.AccountRow> res = new List<Viewbox.DcwBalance.AccountRow>();
			foreach (DataRow row in datatable.Rows)
			{
				Viewbox.DcwBalance.AccountRow account = new Viewbox.DcwBalance.AccountRow();
				account.Dzmnu = GetStringValue(row["Dzmnu"]);
				account.Bmnur = GetStringValue(row["dzmnur"]);
				account.Curr = GetStringValue(row["Curr"]);
				account.Bzuoh = GetStringValue(row["Bzuoh"]);
				account.Hko = GetStringValue(row["Hko"]);
				account.Uko = GetStringValue(row["Uko"]);
				account.HkoName = GetStringValue(row["HkoName"]);
				account.UkoName = GetStringValue(row["UkoName"]);
				account.Saldo_GJ = GetDoubleValue(row["Saldo_GJ"]);
				if (hasVJahr)
				{
					account.Saldo_VJ = GetDoubleValue(row["Saldo_VJ"]);
				}
				if (hasVon)
				{
					account.Saldo_Von = GetStringValue(row["Saldo_Von"]);
				}
				if (hasBis)
				{
					account.Saldo_Bis = GetStringValue(row["Saldo_Bis"]);
				}
				if (hasPer)
				{
					account.Saldo_Per = GetStringValue(row["Saldo_Per"]);
				}
				if (hasMandtSplit)
				{
					account.MandtSplit = GetStringValue(row["dzmnu"]);
					if (tobj.Columns.SingleOrDefault((IColumn c) => c.Name == "DZMNU_DESCRIPTION") != null)
					{
						account.MandtSplitName = GetStringValue(row["DZMNU_DESCRIPTION"]);
					}
				}
				res.Add(account);
			}
			return res;
		}

		private static void UpdateBalanceStructureTable(DatabaseBase db)
		{
			string tableName = db.DbMapping.GetTableName(typeof(Viewbox.SapBalance.StructureRow));
			List<string> columns = db.GetColumnNames(db.DbConfig.DbName, tableName);
			string newColumn1Name = db.DbMapping.GetColumnName<Viewbox.SapBalance.StructureRow>("ParentGroup");
			string newColumn2Name = db.DbMapping.GetColumnName<Viewbox.SapBalance.StructureRow>("AdditionalInformation");
			string newColumn3Name = db.DbMapping.GetColumnName<Viewbox.SapBalance.StructureRow>("AccountStructure");
			string newColumn4Name = db.DbMapping.GetColumnName<Viewbox.SapBalance.StructureRow>("SumAndAddToBalance");
			if (columns.All((string c) => c.ToLower() != newColumn1Name.ToLower()))
			{
				db.DbMapping.AddColumn(tableName, "Int32", new DbColumnAttribute(newColumn1Name), null, null, "0");
			}
			if (columns.All((string c) => c.ToLower() != newColumn2Name.ToLower()))
			{
				db.DbMapping.AddColumn(tableName, "String", new DbColumnAttribute(newColumn2Name), null);
			}
			if (columns.All((string c) => c.ToLower() != newColumn3Name.ToLower()))
			{
				db.DbMapping.AddColumn(tableName, "String", new DbColumnAttribute(newColumn3Name), null);
			}
			if (columns.All((string c) => c.ToLower() != newColumn4Name.ToLower()))
			{
				db.DbMapping.AddColumn(tableName, "Boolean", new DbColumnAttribute(newColumn4Name), null);
			}
		}

		public static List<Viewbox.SapBalance.AccountRow> GetBalanceData(ITableObject tobj, string database)
		{
			using DatabaseBase db = ConnectionManager.CreateConnection(DataProvider, Settings.Default.ViewboxDatabase);
			db.Open();
			db.ChangeDatabase(database);
			UpdateBalanceDataTable(db);
			return db.DbMapping.Load<Viewbox.SapBalance.AccountRow>();
		}

		public static List<AccountRowExtended> GetBalanceDataExtended(ITableObject tobj, string database)
		{
			using DatabaseBase db = ConnectionManager.CreateConnection(DataProvider, Settings.Default.ViewboxDatabase);
			db.Open();
			db.ChangeDatabase(database);
			UpdateBalanceDataTableExtended(db);
			return db.DbMapping.Load<AccountRowExtended>();
		}

		public static bool SplitByGesber(ITableObject tobj, string mandant, string bukares)
		{
			using (DatabaseBase db = ConnectionManager.CreateConnection(DataProvider, Settings.Default.ViewboxDatabase))
			{
				db.Open();
				db.ChangeDatabase(tobj.Database);
				List<T001> list = db.DbMapping.Load<T001>($"mandt = '{mandant}' AND bukrs = '{bukares}'");
				if (list.Count > 0 && list[0].XGSBE.ToLower() == "x")
				{
					return true;
				}
			}
			return false;
		}

		private static void UpdateBalanceDataTable(DatabaseBase db)
		{
			string tableName = db.DbMapping.GetTableName(typeof(Viewbox.SapBalance.AccountRow));
			List<string> columns = db.GetColumnNames(db.DbConfig.DbName, tableName);
			string newColumn1Name = db.DbMapping.GetColumnName<Viewbox.SapBalance.AccountRow>("Currency");
			string newColumn2Name = db.DbMapping.GetColumnName<Viewbox.SapBalance.AccountRow>("AccountStructure");
			if (columns.All((string c) => c.ToLower() != newColumn1Name.ToLower()))
			{
				db.DbMapping.AddColumn(tableName, "String", new DbColumnAttribute(newColumn1Name), null);
			}
			if (columns.All((string c) => c.ToLower() != newColumn2Name.ToLower()))
			{
				db.DbMapping.AddColumn(tableName, "String", new DbColumnAttribute(newColumn2Name), null);
			}
		}

		private static void UpdateBalanceDataTableExtended(DatabaseBase db)
		{
			string tableName = db.DbMapping.GetTableName(typeof(AccountRowExtended));
			List<string> columns = db.GetColumnNames(db.DbConfig.DbName, tableName);
			string newColumn1Name = db.DbMapping.GetColumnName<Viewbox.SapBalance.AccountRow>("Currency");
			string newColumn2Name = db.DbMapping.GetColumnName<Viewbox.SapBalance.AccountRow>("AccountStructure");
			if (columns.All((string c) => c.ToLower() != newColumn1Name.ToLower()))
			{
				db.DbMapping.AddColumn(tableName, "String", new DbColumnAttribute(newColumn1Name), null);
			}
			if (columns.All((string c) => c.ToLower() != newColumn2Name.ToLower()))
			{
				db.DbMapping.AddColumn(tableName, "String", new DbColumnAttribute(newColumn2Name), null);
			}
		}

		private static void CleanUpAccessKeys()
		{
			foreach (KeyValuePair<string, Tuple<DateTime, IUser>> i in _accessKeys.Where((KeyValuePair<string, Tuple<DateTime, IUser>> kv) => kv.Value.Item1 < DateTime.Now - TemporaryCheckInterval).ToList())
			{
				_accessKeys.Remove(i.Key);
			}
		}

		public static void SendInvitation(IUser user, string requestUrlString)
		{
			CleanUpAccessKeys();
			KeyValuePair<string, Tuple<DateTime, IUser>> existing = _accessKeys.FirstOrDefault((KeyValuePair<string, Tuple<DateTime, IUser>> kv) => kv.Value.Item2 == user);
			string key = (string.IsNullOrEmpty(existing.Key) ? BitConverter.ToString(Guid.NewGuid().ToByteArray()).Replace("-", string.Empty) : existing.Key);
			_accessKeys[key] = Tuple.Create(DateTime.Now, user);
			Uri url = HttpContext.Current.Request.Url;
			if (!string.IsNullOrWhiteSpace(requestUrlString))
			{
				try
				{
					url = new Uri(requestUrlString);
				}
				catch (UriFormatException)
				{
				}
			}
			string baseURL = url.AbsoluteUri.Remove(url.AbsoluteUri.Length - url.PathAndQuery.Length).TrimEnd('/');
			string link = baseURL + Url.Action("LogOn", "Account", new { key });
			string subject = "";
			string content = "";
			if (ViewboxSession.User != null)
			{
				subject = Resources.InvitationSubject;
				content = string.Format(Resources.InvitationBody, ViewboxSession.User.Email, link);
			}
			else
			{
				subject = Resources.ChangePasswordSubject;
				content = string.Format(Resources.ChangePasswordBody, link);
			}
			if (HttpContextFactory.Current.Session["UnitTesting"] == null || !(bool)HttpContextFactory.Current.Session["UnitTesting"])
			{
				SendMail(user, subject, content);
			}
		}

		public static IUser GetInvitedUser(string key)
		{
			CleanUpAccessKeys();
			return _accessKeys.ContainsKey(key) ? _accessKeys[key].Item2 : null;
		}

		public static void SendMail(IUser user, string subject, string body, bool html = false)
		{
			MailMessage msg = new MailMessage
			{
				Subject = subject,
				Body = body,
				IsBodyHtml = html
			};
			msg.To.Add(new MailAddress(user.Email, user.Name));
			new SmtpClient().Send(msg);
		}

		public static INote SaveNote(int id, string text, string title)
		{
			return (id > 0) ? Database.UpdateNote(id, ViewboxSession.User, title, text) : Database.CreateNote(ViewboxSession.User, title, text, DateTime.Now);
		}

		public static void UpgradeDatabase()
		{
			Database.UpgradeDatabase();
			DatabaseOutOfDateInformation = null;
		}

		public static DateTime BuildingTime()
		{
			string filePath = Assembly.GetCallingAssembly().Location;
			byte[] b = new byte[2048];
			Stream s = null;
			try
			{
				s = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				s.Read(b, 0, 2048);
			}
			finally
			{
				s?.Close();
			}
			int i = BitConverter.ToInt32(b, 60);
			int secondsSince1970 = BitConverter.ToInt32(b, i + 8);
			DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(secondsSince1970);
			return dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
		}

		public static IUser ByUserName(string userName)
		{
			return Database.SystemDb.UsersByUserName[userName];
		}

		public static IUser Login(string userName, string password)
		{
			IUser user = ByUserName(userName);
			if (user?.CheckPassword(password) ?? false)
			{
				return user;
			}
			return null;
		}

		public static ILanguage GetLanguageByCountryCode(string countryCode)
		{
			string cc = countryCode.ToLowerInvariant();
			if (Languages != null)
			{
				foreach (ILanguage i in Languages)
				{
					if (i.CountryCode == cc)
					{
						return i;
					}
				}
			}
			return null;
		}

		public static bool LoadInFileSystem(string database)
		{
			if (Database.SystemDb.LoadInFileSystem(database))
			{
				IsFilesSystemInitialized = true;
				return true;
			}
			return false;
		}

		public static IFileObjectCollection FilterForFiles(int id)
		{
			IEnumerable<IFileObject> temp = Database.SystemDb.FileObjects.Where((IFileObject f) => f.DirectoryId == id);
			FileObjectCollection filteredFiles = new FileObjectCollection();
			foreach (IFileObject file in temp)
			{
				filteredFiles.Add(file);
			}
			return filteredFiles;
		}

		public static IFileObjectCollection FilterForFiles(string name)
		{
			IEnumerable<IFileObject> temp = Database.SystemDb.FileObjects.Where((IFileObject f) => f.Name.Contains(name));
			FileObjectCollection filteredFiles = new FileObjectCollection();
			foreach (IFileObject file in temp)
			{
				filteredFiles.Add(file);
			}
			return filteredFiles;
		}

		public static IDirectoryObjectCollection FilterForDirectories(string name)
		{
			IEnumerable<IDirectoryObject> temp = Database.SystemDb.DirectoryObjects.Where((IDirectoryObject f) => f.Name.Contains(name));
			DirectoryObjectCollection filteredDirs = new DirectoryObjectCollection();
			foreach (IDirectoryObject file in temp)
			{
				if (!temp.Any((IDirectoryObject d) => d.Id == file.ParentId))
				{
					filteredDirs.Add(file);
				}
			}
			return filteredDirs;
		}
	}
}
