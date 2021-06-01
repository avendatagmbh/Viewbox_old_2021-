using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SystemDb.Compatibility;
using SystemDb.Helper;
using SystemDb.Internal;
using SystemDb.Resources;
using SystemDb.Upgrader;
using AV.Log;
using DbAccess;
using DbAccess.Attributes;
using DbAccess.Enums;
using DbAccess.Structures;
using log4net;
using MySql.Data.MySqlClient;
using Utils;

namespace SystemDb
{
	public class SystemDb : IDisposable, ISystemDb, INotifyPropertyChanged, IDataRepository
	{
		public enum CheckPart
		{
			Index,
			ParameterCollectionMapping,
			ParameterValue
		}

		public enum Part
		{
			Languages,
			Users,
			Optimizations,
			Categories,
			Properties,
			Schemes,
			TableObjects,
			IssueExtensions,
			TableSchemes,
			OrderAreas,
			Columns,
			Indexes,
			Parameters,
			ParameterHistory,
			Relations,
			ExtendedColumnInformation,
			About,
			Notes,
			EmptyDistinctTable,
			TableCruds
		}

		public delegate void PartLoadingCompletedHandler(SystemDb sender, Part part);

		private class ReplaceOccurence
		{
			public Group Group { get; set; }

			public ITableObject TableObject { get; set; }
		}

		private bool _useNewIssueMethod;

		private DatabaseBase specDbX;

		private readonly Archive _archive;

		private readonly ArchiveDocument _archiveDocument;

		private readonly ArchiveDocumentCollection _archiveDocuments = new ArchiveDocumentCollection();

		private readonly CategoryCollection _categories = new CategoryCollection();

		private readonly IndexColumnMapping _indexColumnMapping = new IndexColumnMapping();

		private readonly IndexCollection _indexes = new IndexCollection();

		private readonly LanguageCollection _languages = new LanguageCollection();

		private readonly NotesCollection _notes = new NotesCollection();

		private readonly ObjectTypeTextCollection _objectTypeTextCollection = new ObjectTypeTextCollection();

		private readonly ObjectTypeRelationsCollection _objectTypeRelationsCollection = new ObjectTypeRelationsCollection();

		private readonly ObjectTypesCollection _objectTypesCollection = new ObjectTypesCollection();

		private readonly TableObjectCollection _objects = new TableObjectCollection();

		private readonly OptimizationGroupCollection _optimizationGroups = new OptimizationGroupCollection();

		private readonly PropertiesCollection _properties = new PropertiesCollection();

		private readonly CorePropertiesCollection _coreProperties = new CorePropertiesCollection();

		private readonly RoleCategoryRights _roleCategoryRights = new RoleCategoryRights();

		private readonly RoleOptimizationRights _roleOptimizationRights = new RoleOptimizationRights();

		private readonly RoleCollection _roles = new RoleCollection();

		private readonly List<SchemeText> _schemeTexts = new List<SchemeText>();

		private readonly SchemeCollection _schemes = new SchemeCollection();

		private readonly TableCrudCollection _tableCruds = new TableCrudCollection();

		private readonly TableCollection _tables = new TableCollection();

		private readonly UserCategoryRights _userCategoryRights = new UserCategoryRights();

		private readonly UserControllerSettingsCollection _userControllerSettings = new UserControllerSettingsCollection();

		private readonly UserFavoriteIssueSettingsCollection _userFavoriteIssueCollection = new UserFavoriteIssueSettingsCollection();

		private readonly FullHistoryParameterValueCollection _userHistoryIssueParameterCollection = new FullHistoryParameterValueCollection();

		private readonly FullHistoryParameterValueFreeSelectionCollection _userHistoryFreeSelectionIssueParameterCollection = new FullHistoryParameterValueFreeSelectionCollection();

		private readonly UserLastIssueSettingsCollection _userLastIssueCollection = new UserLastIssueSettingsCollection();

		private readonly UserLogRights _userLogRights = new UserLogRights();

		private readonly UserOptimizationRights _userOptimizationRights = new UserOptimizationRights();

		private readonly UserPropertySettingCollection _userPropertySettings = new UserPropertySettingCollection();

		private readonly UserRoleCollection _userRoles = new UserRoleCollection();

		private readonly UserTableColumnWidthsSettingsCollection _userTableColumnWidthSettings = new UserTableColumnWidthsSettingsCollection();

		private readonly UserTableTransactionIdSettingsCollection _userTableTransactionIdSettings = new UserTableTransactionIdSettingsCollection();

		private readonly UserUserLogSettingsCollection _userUserLogSettings = new UserUserLogSettingsCollection();

		private readonly UserIdCollection _users = new UserIdCollection();

		private readonly UserNameCollection _usersByUserName = new UserNameCollection();

		private readonly ViewCollection _views = new ViewCollection();

		private Queue Queue = new Queue();

		private About _about;

		private TableArchiveInformationCollection _archiveInformation = new TableArchiveInformationCollection();

		private ArchiveCollection _archives = new ArchiveCollection();

		private ConnectionManager _cmanager;

		private FullColumnCollection _columns = new FullColumnCollection();

		private Dictionary<int, IParameter> _parameters;

		private List<ParameterSelectionLogic> _parameterSelectionLogics;

		private string _connectionString;

		private IssueCollection _issues = new IssueCollection();

		private readonly RoleAreaCollection _roleAreas = new RoleAreaCollection();

		internal ILog _log = LogHelper.GetLogger();

		private List<OptimizationText> _optimizationTexts = new List<OptimizationText>();

		private OptimizationCollection _optimizations = new OptimizationCollection();

		private PasswordCollection _passwords = new PasswordCollection();

		private RoleColumnRights _roleColumnRights = new RoleColumnRights();

		private RoleTableObjectRights _roleTableObjectRights = new RoleTableObjectRights();

		private UserColumnOrderSettingsCollection _userColumnOrderSettings = new UserColumnOrderSettingsCollection();

		private UserColumnRights _userColumnRights = new UserColumnRights();

		private UserColumnSettingCollection _userColumnSettings = new UserColumnSettingCollection();

		private UserOptimizationSettingsCollection _userOptimizationSettings = new UserOptimizationSettingsCollection();

		private UserSettingsCollection _userSettings = new UserSettingsCollection();

		private RoleSettingsCollection _roleSettings = new RoleSettingsCollection();

		private List<TableTransactions> _tableTransactions = new List<TableTransactions>();

		private DirectoryObjectCollection _directoriesObjects = new DirectoryObjectCollection();

		private FileObjectCollection _filesObjects = new FileObjectCollection();

		private FileSysCollection _fileSystems = new FileSysCollection();

		private List<StartScreen> _startscreens = new List<StartScreen>();

		private UserTableObjectOrderSettingsCollection _userTableObjectOrderSettings = new UserTableObjectOrderSettingsCollection();

		private UserTableObjectRights _userTableObjectRights = new UserTableObjectRights();

		private UserTableObjectSettingsCollection _userTableObjectSettings = new UserTableObjectSettingsCollection();

		private int columnId = -1;

		private List<RowVisibilityCountCache> _rowVisibilityCountCache = new List<RowVisibilityCountCache>();

		public bool IsCalculated;

		public bool IsReport;

		private int tableId = -1;

		private Optimization _rootOptimization;

		private static Regex _regexForSkriptParsing;

		public List<StartScreen> StartScreens
		{
			get
			{
				return _startscreens;
			}
			set
			{
				_startscreens = value;
			}
		}

		public List<RowVisibilityCountCache> RowVisibilityCountCacheCollection
		{
			get
			{
				return _rowVisibilityCountCache;
			}
			set
			{
				_rowVisibilityCountCache = value;
			}
		}

		public IIndexCollection Indexes => _indexes;

		public IndexColumnMapping IndexColumnMapping => _indexColumnMapping;

		public IArchiveDocument ArchiveDocument => _archiveDocument;

		public IArchiveDocumentCollection ArchiveDocuments => _archiveDocuments;

		public IUserTableColumnWidthsSettingsCollection UserTableColumnWidthsSettings => _userTableColumnWidthSettings;

		public IUserTableTransactionIdSettingsCollection UserTableTransactionIdSettings => _userTableTransactionIdSettings;

		public IUserUserLogSettingsCollection UserUserLogSettings => _userUserLogSettings;

		public RoleAreaCollection RoleAreaCollection => _roleAreas;

		public ITableCrudCollection TableCruds => _tableCruds;

		public IUserLogRights UserLogRights => _userLogRights;

		public IUserFavoriteIssueSettingsCollection UserFavoriteIssueSettingsCollection => _userFavoriteIssueCollection;

		public IUserLastIssueSettingsCollection UserLastIssueSettingsCollection => _userLastIssueCollection;

		public IFullHistoryParameterValueCollection UserHistoryIssueParameterCollection => _userHistoryIssueParameterCollection;

		public IObjectTypesCollection ObjectTypesCollection => _objectTypesCollection;

		public IObjectTypeTextCollection ObjectTypeTextCollection => _objectTypeTextCollection;

		public IObjectTypeRelationsCollection ObjectTypeRelationsCollection => _objectTypeRelationsCollection;

		public List<TableTransactions> TableTransactions => _tableTransactions;

		public int TransactionNumber { get; set; }

		public ILanguageCollection Languages => _languages;

		public ILanguage DefaultLanguage => Languages.First();

		public About About => _about;

		public IOptimizationGroupCollection OptimizationGroups => _optimizationGroups;

		public IOptimizationCollection Optimizations => _optimizations;

		public IRoleOptimizationRights RoleOptimizationRights => _roleOptimizationRights;

		public IUserOptimizationRights UserOptimizationRights => _userOptimizationRights;

		public IUserNameCollection UsersByUserName => _usersByUserName;

		public IUserIdCollection Users => _users;

		public IRoleCollection Roles => _roles;

		public IPasswordCollection Passwords => _passwords;

		public IUserSettingsCollection UserSettings => _userSettings;

		public IUserRoleCollection UserRole => _userRoles;

		public IPropertiesCollection Properties => _properties;

		public ICorePropertiesCollection CoreProperties => _coreProperties;

		public ICategoryCollection Categories => _categories;

		public IRoleCategoryRights RoleCategoryRights => _roleCategoryRights;

		public IUserCategoryRights UserCategoryRights => _userCategoryRights;

		public ITableObjectCollection Objects => _objects;

		public IRoleTableObjectRights RoleTableObjectRights => _roleTableObjectRights;

		public IUserTableObjectRights UserTableObjectRights => _userTableObjectRights;

		public IIssueCollection Issues => _issues;

		public IViewCollection Views => _views;

		public ITableCollection Tables => _tables;

		public IArchive Archive => _archive;

		public IArchiveCollection Archives => _archives;

		public IFullColumnCollection Columns => _columns;

		public Dictionary<int, IParameter> Parameters => _parameters;

		public List<ParameterSelectionLogic> ParameterSelectionLogics
		{
			get
			{
				if (_parameterSelectionLogics == null)
				{
					_parameterSelectionLogics = new List<ParameterSelectionLogic>();
				}
				return _parameterSelectionLogics;
			}
		}

		public IRoleColumnRights RoleColumnRights => _roleColumnRights;

		public IUserColumnRights UserColumnRights => _userColumnRights;

		public IUserColumnSettingCollection UserColumnSettings => _userColumnSettings;

		public IUserTableObjectSettingsCollection UserTableObjectSettings => _userTableObjectSettings;

		public IUserColumnOrderSettingsCollection UserColumnOrderSettings => _userColumnOrderSettings;

		public IUserPropertySettingCollection UserPropertySettings => _userPropertySettings;

		public IUserControllerSettingsCollection UserControllerSettings => _userControllerSettings;

		public IUserTableObjectOrderSettingsCollection UserTableObjectOrderSettings => _userTableObjectOrderSettings;

		public IUserOptimizationSettingsCollection UserOptimizationSettings => _userOptimizationSettings;

		public ISchemeCollection Schemes => _schemes;

		public IList<SchemeText> SchemeTexts => _schemeTexts;

		public INotesCollection Notes => _notes;

		public ITableArchiveInformationCollection ArchiveInformation => _archiveInformation;

		public IFileSysCollection FileSystems => _fileSystems;

		public IDirectoryObjectCollection DirectoryObjects => _directoriesObjects;

		public IFileObjectCollection FileObjects => _filesObjects;

		public IDataRepository BusinessObjectRepository
		{
			set
			{
				LoadMockRepository(value);
			}
		}

		public string ConnectionString => _connectionString;

		public DatabaseBaseOutOfDateInformation DatabaseOutOfDateInformation { get; private set; }

		public ConnectionManager ConnectionManager
		{
			get
			{
				return _cmanager;
			}
			set
			{
				if (_cmanager != value)
				{
					_cmanager = value;
					OnPropertyChanged("ConnectionManager");
				}
			}
		}

		public DatabaseBase DB { get; set; }

		public event PartLoadingCompletedHandler PartLoadingCompleted;

		public event PropertyChangedEventHandler PropertyChanged;

		public event EventHandler ConnectionEstablished;

		public event EventHandler SystemDbInitialized;

		public event Action DataBaseUpGradeChecked;

		public event Action LoadingFinished;

		public event ErrorEventHandler Error;

		public void CheckTablesImpl(DatabaseBase db)
		{
			Stopwatch watch = new Stopwatch();
			watch.Start();
			using (NDC.Push(LogHelper.GetNamespaceContext()))
			{
				List<CheckPart> queue = Enum.GetValues(typeof(CheckPart)).Cast<CheckPart>().ToList();
				int lastCount = -1;
				while (lastCount != queue.Count)
				{
					lastCount = queue.Count;
					foreach (CheckPart part in queue.ToList())
					{
						string enumName = Enum.GetName(typeof(CheckPart), part);
						_log.InfoFormat("Check part {0}", enumName);
						try
						{
							CheckTableAlpha(db, enumName);
							queue.Remove(part);
							_log.InfoFormat("Check part {0} successful", enumName);
						}
						catch (Exception ex)
						{
							string message = $"Error. Check part {enumName} unsuccessful. {ex.Message}";
							_log.Error(message, ex);
						}
					}
				}
				watch.Stop();
				if (queue.Count > 0)
				{
					string message2 = string.Format("Error while checking objects. Failed parts:{0}", string.Join(",", queue.Select((CheckPart itm) => Enum.GetName(typeof(CheckPart), itm))));
					_log.Error(message2);
					throw new Exception(message2);
				}
			}
		}

		private void CheckTableAlpha(DatabaseBase db, string className)
		{
			string classFullName = "SystemDb.Internal." + className;
			Type t = Type.GetType(classFullName);
			if (t == null)
			{
				throw new NullReferenceException("Can not initialize type for " + classFullName + " class. Please edit CheckPart enum in SystemDb.CheckImpl.cs");
			}
			string tableName = null;
			object[] customAttributes = t.GetCustomAttributes(inherit: true);
			foreach (object attr2 in customAttributes)
			{
				if (attr2 is DbTableAttribute)
				{
					tableName = (attr2 as DbTableAttribute).Name;
					break;
				}
			}
			if (string.IsNullOrEmpty(tableName))
			{
				throw new NullReferenceException("Can not found table attribute in " + classFullName + " class. Please edit CheckPart enum in SystemDb.CheckImpl.cs");
			}
			PropertyInfo pkPi = null;
			PropertyInfo[] properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			Attribute[] customAttributes2;
			foreach (PropertyInfo pi in properties)
			{
				customAttributes2 = Attribute.GetCustomAttributes(pi);
				for (int j = 0; j < customAttributes2.Length; j++)
				{
					if (customAttributes2[j] is DbPrimaryKeyAttribute)
					{
						pkPi = pi;
						break;
					}
				}
			}
			if (pkPi == null)
			{
				throw new NullReferenceException("Can not found primary key attribute in " + classFullName + " class. Please edit CheckPart enum in SystemDb.CheckImpl.cs");
			}
			DbColumnAttribute pkCol = null;
			customAttributes2 = Attribute.GetCustomAttributes(pkPi);
			foreach (Attribute attr in customAttributes2)
			{
				if (attr is DbColumnAttribute)
				{
					pkCol = attr as DbColumnAttribute;
					break;
				}
			}
			if (pkCol == null)
			{
				throw new NullReferenceException("Can not found column attribute in " + pkPi.Name + " column in " + classFullName + " class. Please edit CheckPart enum in SystemDb.CheckImpl.cs");
			}
			if (pkCol.AutoIncrement && !db.GetCreateTableStructure(tableName).ToUpper().Contains("AUTO_INCREMENT"))
			{
				db.AlterAutoIncrementPrimaryColumn(tableName, pkPi.Name);
			}
		}

		private void NotifyPartLoadingCompleted(Part part)
		{
			if (this.PartLoadingCompleted != null)
			{
				this.PartLoadingCompleted(this, part);
			}
		}

		private void Logger(string methodName)
		{
			_log.Log(LogLevelEnum.Info, "Loading of '" + methodName + "' was successful!");
		}

		public ITableObject GetTableObjectNeeded(int id)
		{
			if (_views[id] != null)
			{
				return _views[id];
			}
			if (_tables[id] != null)
			{
				return _tables[id];
			}
			if (_issues[id] != null)
			{
				return _issues[id];
			}
			return null;
		}

		public List<ITableObject> GetTableObjectByColumName(string columnName)
		{
			List<ITableObject> returnList = new List<ITableObject>();
			if (_views.Any((IView x) => x.Columns.Any((IColumn y) => y.Name.ToLower() == columnName.ToLower())))
			{
				returnList.AddRange(_views.Where((IView x) => x.Columns.Any((IColumn y) => y.Name.ToLower() == columnName.ToLower())).ToList());
			}
			if (_tables.Any((ITable x) => x.Columns.Any((IColumn y) => y.Name.ToLower() == columnName.ToLower())))
			{
				returnList.AddRange(_tables.Where((ITable x) => x.Columns.Any((IColumn y) => y.Name.ToLower() == columnName.ToLower())).ToList());
			}
			if (_issues.Any((IIssue x) => x.Columns.Any((IColumn y) => y.Name.ToLower() == columnName.ToLower())))
			{
				returnList.AddRange(_issues.Where((IIssue x) => x.Columns.Any((IColumn y) => y.Name.ToLower() == columnName.ToLower())).ToList());
			}
			return returnList;
		}

		public void LoadTablesImpl(DatabaseBase db, bool useNewIssueMethod = false)
		{
			Stopwatch watch = new Stopwatch();
			specDbX = _cmanager.GetConnection();
			Task theMainTask = Task.Factory.StartNew(delegate
			{
				List<Task> list = new List<Task>();
				watch.Start();
				_useNewIssueMethod = useNewIssueMethod;
				NDC.Push(LogHelper.GetNamespaceContext());
				Task.Factory.StartNew(delegate
				{
					using DatabaseBase db8 = _cmanager.GetConnection();
					LoadFileSystem(db8);
					Logger("FileSystem");
					LoadParameterHistory(db8);
					Logger("ParamterHistory");
					LoadFreeSelection(db8);
					Logger("FreeSelection");
					LoadEmptyDistinctTable(db8);
					Logger("EmptyDistinctTable");
					LoadAbout(db8);
					Logger("About");
					LoadStartScreen(db8);
					Logger("StartScreens");
				}, TaskCreationOptions.AttachedToParent);
				list.Add(Task.Factory.StartNew(delegate
				{
					using DatabaseBase db7 = _cmanager.GetConnection();
					LoadLanguages(db7);
					Logger("Languages");
					LoadUsers(db7);
					Logger("Users");
				}));
				Task.WaitAll(list.ToArray());
				Task.Factory.StartNew(delegate
				{
					using DatabaseBase db6 = _cmanager.GetConnection();
					LoadOptimizations(db6);
					Logger("Optimizations");
					LoadProperties(db6);
					Logger("Properties");
					LoadCoreProperties(db6);
					Logger("Core Properties");
					LoadNotes(db6);
					Logger("Notes");
				}, TaskCreationOptions.AttachedToParent);
				list.Add(Task.Factory.StartNew(delegate
				{
					using DatabaseBase db5 = _cmanager.GetConnection();
					LoadCategories(db5);
					Logger("Categories");
					LoadSchemes(db5);
					Logger("Schemes");
				}));
				Task.WaitAll(list.ToArray());
				LoadTableObjects(db);
				Logger("TableObjects");
				list.Add(Task.Factory.StartNew(delegate
				{
					using DatabaseBase db4 = _cmanager.GetConnection();
					LoadColumns(db4);
					Logger("Columns");
				}));
				Task.Factory.StartNew(delegate
				{
					using DatabaseBase db3 = _cmanager.GetConnection();
					LoadOrderAreas(db3);
					Logger("OrderAreas");
					LoadParameters(db3);
					Logger("Parameters");
					LoadIssueExtensions(db3);
					Logger("issueExtensions");
					LoadTableSchemes(db3);
					Logger("TableSchemes");
					LoadIndexes(db3);
					Logger("Indexes");
				}, TaskCreationOptions.AttachedToParent);
				Task.WaitAll(list.ToArray());
				Task.Factory.StartNew(delegate
				{
					using DatabaseBase db2 = _cmanager.GetConnection();
					LoadRelations(db2);
					Logger("Relations");
					LoadTableCruds(db2);
					Logger("TableCruds");
					LoadExtendedColumnInformation(db2);
					Logger("ExtendedColumnInformation");
					LoadRowVisibility(db2);
					Logger("RowVisibility");
					LoadRowVisibilityNew(db2);
					Logger("RowVisibilityNew");
				}, TaskCreationOptions.AttachedToParent);
				FillDefaultsIfEmpty(db);
				watch.Stop();
				OnLoadingFinished();
			});
			Task.WaitAll(theMainTask);
			string timeLog = "Full Loading Time: " + (double)watch.ElapsedMilliseconds / 1000.0 + " secs";
			_log.Log(LogLevelEnum.Info, timeLog);
		}

		public void LoadLanguages(DatabaseBase db)
		{
			_languages.Clear();
			db.DbMapping.CreateTableIfNotExists<Language>();
			foreach (Language language in db.DbMapping.Load<Language>())
			{
				if (!_languages.ContainsKey(language.CountryCode))
				{
					_languages.Add(language);
				}
			}
			if (_languages.Count == 0)
			{
				_languages.Add(new Language
				{
					CountryCode = "de",
					LanguageName = Language.LanguageDescriptions["de"].Item1,
					LanguageMotto = Language.LanguageDescriptions["de"].Item2
				});
				db.DbMapping.Save(DefaultLanguage);
			}
		}

		public void LoadUsers(DatabaseBase db)
		{
			_usersByUserName.Clear();
			_users.Clear();
			db.DbMapping.CreateTableIfNotExists<global::SystemDb.Internal.User>();
			db.DbMapping.AddColumn("users", "Boolean", new DbColumnAttribute("first_login"), null, null, "0");
			foreach (global::SystemDb.Internal.User user4 in db.DbMapping.Load<global::SystemDb.Internal.User>())
			{
				if (user4.DisplayRowCount == 0)
				{
					user4.DisplayRowCount = 35;
					db.DbMapping.Save(user4);
				}
				_usersByUserName.Add(user4);
				_users.Add(user4);
			}
			_roles.Clear();
			db.DbMapping.CreateTableIfNotExists<global::SystemDb.Internal.Role>();
			foreach (global::SystemDb.Internal.Role role3 in db.DbMapping.Load<global::SystemDb.Internal.Role>())
			{
				_roles.Add(role3.Id, role3);
			}
			_userRoles.Clear();
			db.DbMapping.CreateTableIfNotExists<UserRoleMapping>();
			foreach (UserRoleMapping i in db.DbMapping.Load<UserRoleMapping>())
			{
				IRole role2 = _roles[i.RoleId];
				IUser user3 = _users[i.UserId];
				if (role2 != null && user3 != null)
				{
					role2.Users.Add(user3);
					user3.Roles.Add(role2);
					_userRoles.Add(user3, role2, i);
				}
			}
			db.DbMapping.CreateTableIfNotExists<Password>();
			List<Password> list3 = db.DbMapping.Load<Password>();
			_passwords = new PasswordCollection(list3);
			foreach (IUser user5 in _users)
			{
				user5.PasswordHistory = new PasswordCollection(from pwd in list3
					where pwd.UserId == user5.Id
					orderby pwd.CreationDate descending
					select pwd);
			}
			db.DbMapping.CreateTableIfNotExists<UserSetting>();
			List<UserSetting> list2 = db.DbMapping.Load<UserSetting>();
			_userSettings = new UserSettingsCollection(list2);
			foreach (IUser user2 in _users)
			{
				user2.Settings = new UserSettingsCollection(user2, _userSettings[user2]);
				user2.Settings.SettingModifiedEvent += delegate(IUser user_, string userSettingName, string value)
				{
					AddUserSetting(user_, userSettingName, value);
				};
				user2.Settings.SettingRemovedEvent += (IUser user_, string userSettingName) => RemoveUserSetting(user_, userSettingName);
			}
			db.DbMapping.CreateTableIfNotExists<RoleSetting>();
			List<RoleSetting> list = db.DbMapping.Load<RoleSetting>();
			_roleSettings = new RoleSettingsCollection(list);
			foreach (IRole role in _roles)
			{
				role.Settings = new RoleSettingsCollection(role, _roleSettings[role]);
				role.Settings.SettingModifiedEvent += delegate(IRole role_, RoleSettingsType roleSettingType, string value)
				{
					AddRoleSetting(role_, roleSettingType, value);
				};
				role.Settings.SettingRemovedEvent += (IRole role_, RoleSettingsType roleSettingType) => RemoveRoleSetting(role_, roleSettingType);
			}
			db.DbMapping.CreateTableIfNotExists<UserLogUserMapping>();
			_userLogRights.Clear();
			foreach (UserLogUserMapping o in db.DbMapping.Load<UserLogUserMapping>())
			{
				IUser user = _users[o.UserLogId];
				if (user != null)
				{
					_userLogRights.Add(user, _users[o.UserId], o.Id, o.Right);
				}
				else
				{
					db.DbMapping.Delete<UserLogUserMapping>("userlog_id = " + o.UserLogId);
				}
			}
			db.DbMapping.CreateTableIfNotExists<UserControllerSettings>();
			foreach (UserControllerSettings s2 in db.DbMapping.Load<UserControllerSettings>())
			{
				s2.User = Users[s2.UserId];
				_userControllerSettings.Add(s2);
			}
			db.DbMapping.CreateTableIfNotExists<UserTableObjectOrderSettings>();
			foreach (UserTableObjectOrderSettings s in db.DbMapping.Load<UserTableObjectOrderSettings>())
			{
				s.User = Users[s.UserId];
				_userTableObjectOrderSettings.Add(s);
			}
			db.DbMapping.CreateTableIfNotExists<UserUserLogSettings>();
			foreach (UserUserLogSettings logSettings in db.DbMapping.Load<UserUserLogSettings>())
			{
				logSettings.User = Users[logSettings.UserId];
				_userUserLogSettings.Add(logSettings);
			}
			db.DbMapping.CreateTableIfNotExists<UserFavoriteIssueSettings>();
			foreach (UserFavoriteIssueSettings favoriteSetting in db.DbMapping.Load<UserFavoriteIssueSettings>())
			{
				favoriteSetting.User = Users[favoriteSetting.UserId];
				_userFavoriteIssueCollection.Add(favoriteSetting);
			}
			db.DbMapping.CreateTableIfNotExists<UserLastIssueSettings>();
			foreach (UserLastIssueSettings settings in db.DbMapping.Load<UserLastIssueSettings>())
			{
				settings.User = Users[settings.UserId];
				if (settings.User != null)
				{
					_userLastIssueCollection.Add(settings);
				}
			}
		}

		public void LoadOptimizations(DatabaseBase db)
		{
			_optimizationGroups.Clear();
			db.DbMapping.CreateTableIfNotExists<OptimizationGroup>();
			_optimizationGroups.AddRange(db.DbMapping.Load<OptimizationGroup>());
			db.DbMapping.CreateTableIfNotExists<OptimizationGroupText>();
			foreach (OptimizationGroupText t in db.DbMapping.Load<OptimizationGroupText>())
			{
				try
				{
					(_optimizationGroups[t.RefId] as OptimizationGroup).SetName(t.Text, _languages[t.CountryCode]);
				}
				catch
				{
					_log.ContextLog(LogLevelEnum.Info, $"OptimizationGroup is not exist [OptimizationGroupText: {t.Id}]");
				}
			}
			_optimizations.Clear();
			_rootOptimization = new Optimization
			{
				Id = 0,
				ParentId = 0
			};
			_optimizations.Add(_rootOptimization);
			db.DbMapping.CreateTableIfNotExists<Optimization>();
			db.DbMapping.CreateTableIfNotExists<OptimizationText>();
			_optimizationTexts = db.DbMapping.Load<OptimizationText>();
			List<Optimization> optimizations = db.DbMapping.Load<Optimization>();
			FillOptimizationTree(optimizations, _optimizationTexts);
			_roleOptimizationRights.Clear();
			db.DbMapping.CreateTableIfNotExists<OptimizationRoleMapping>();
			using (IDataReader dr = db.ExecuteReader("SELECT COLUMN_NAME, COLUMN_TYPE, EXTRA FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'optimization_roles' AND TABLE_SCHEMA = '" + db.DbConfig.DbName + "' AND COLUMN_NAME = 'id' ORDER BY COLUMN_NAME"))
			{
				dr.Read();
				if (dr.GetString(1).ToLower() != "bigint" && dr.GetString(2).ToLower() != "auto_increment")
				{
					specDbX.AlterColumnType("optimization_roles", "id", "BIGINT auto_increment");
				}
			}
			foreach (OptimizationRoleMapping j in db.DbMapping.Load<OptimizationRoleMapping>())
			{
				_roleOptimizationRights.Add(_roles[j.RoleId], _optimizations[j.OptimizationId], j.Id, j.Visible ? RightType.Read : RightType.None);
			}
			_userOptimizationRights.Clear();
			db.DbMapping.CreateTableIfNotExists<OptimizationUserMapping>();
			foreach (OptimizationUserMapping i in db.DbMapping.Load<OptimizationUserMapping>())
			{
				_userOptimizationRights.Add(_users[i.UserId], _optimizations[i.OptimizationId], i.Id, i.Visible ? RightType.Read : RightType.None);
			}
			db.DbMapping.CreateTableIfNotExists<UserOptimizationSettings>();
			foreach (UserOptimizationSettings s in db.DbMapping.Load<UserOptimizationSettings>())
			{
				s.User = Users[s.UserId];
				s.Optimization = Optimizations[s.OptimizationId];
				try
				{
					_userOptimizationSettings.Add(s);
				}
				catch
				{
				}
			}
		}

		public void LoadCategories(DatabaseBase db)
		{
			Category generalCategory = new Category
			{
				Id = 0
			};
			_categories.Clear();
			_categories.Add(generalCategory);
			(generalCategory.Names as LocalizedTextCollection).Clear();
			_roleCategoryRights.Clear();
			db.DbMapping.CreateTableIfNotExists<Category>();
			db.DbMapping.Delete<Category>("id = 0");
			foreach (Category c in db.DbMapping.Load<Category>())
			{
				_categories.Add(c);
			}
			_userCategoryRights.Clear();
			db.DbMapping.CreateTableIfNotExists<CategoryText>();
			foreach (CategoryText t in db.DbMapping.Load<CategoryText>())
			{
				try
				{
					_categories[t.RefId].SetName(t.Text, _languages[t.CountryCode]);
				}
				catch
				{
					_log.ContextLog(LogLevelEnum.Info, $"Category is not exist [CategoryText: {t.Id}]");
				}
			}
			db.DbMapping.CreateTableIfNotExists<CategoryRoleMapping>();
			foreach (CategoryRoleMapping j in db.DbMapping.Load<CategoryRoleMapping>())
			{
				_roleCategoryRights.Add(_roles[j.RoleId], _categories[j.CategoryId], j.Id, j.Right);
			}
			db.DbMapping.CreateTableIfNotExists<CategoryUserMapping>();
			foreach (CategoryUserMapping i in db.DbMapping.Load<CategoryUserMapping>())
			{
				_userCategoryRights.Add(_users[i.UserId], _categories[i.CategoryId], i.Id, i.Right);
			}
		}

		public void LoadProperties(DatabaseBase db)
		{
			_properties.Clear();
			db.DbMapping.CreateTableIfNotExists<Property>();
			foreach (Property p in db.DbMapping.Load<Property>())
			{
				_properties.Add(p);
			}
			db.DbMapping.CreateTableIfNotExists<PropertyText>();
			using (IDataReader dr = db.ExecuteReader("SELECT COLUMN_NAME, COLUMN_TYPE, EXTRA FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'property_texts' AND TABLE_SCHEMA = '" + db.DbConfig.DbName + "' AND COLUMN_NAME = 'id' ORDER BY COLUMN_NAME"))
			{
				dr.Read();
				if (dr.GetString(1).ToLower() != "bigint" && dr.GetString(2).ToLower() != "auto_increment")
				{
					specDbX.AlterColumnType("property_texts", "id", "BIGINT AUTO_INCREMENT");
				}
			}
			List<PropertyText> list = db.DbMapping.Load<PropertyText>();
			if (list.FindAll((PropertyText item) => item.CountryCode == "en" && item.Text.Equals("Do you want to the null value to be displayed?")).Count > 0)
			{
				db.ExecuteNonQuery("UPDATE `" + DB.DbConfig.DbName + "`.`property_texts` SET `text` = 'Do you want the null values to be displayed?' WHERE `name` like 'Show Null Values';");
				list = db.DbMapping.Load<PropertyText>();
			}
			foreach (PropertyText t in list)
			{
				try
				{
					Property obj = _properties[t.RefId] as Property;
					obj.SetName(t.Name, _languages[t.CountryCode]);
					obj.SetDescription(t.Text, _languages[t.CountryCode]);
				}
				catch
				{
					_log.ContextLog(LogLevelEnum.Info, $"Property is not exist [PropertyText: {t.Id}]");
				}
			}
			db.DbMapping.CreateTableIfNotExists<UserPropertySettings>();
			foreach (UserPropertySettings s in db.DbMapping.Load<UserPropertySettings>())
			{
				s.User = Users[s.UserId];
				s.Property = Properties[s.PropertyId];
				_userPropertySettings.Add(s);
			}
		}

		public void LoadCoreProperties(DatabaseBase db)
		{
			_coreProperties.Clear();
			db.DbMapping.CreateTableIfNotExists<CoreProperty>();
			foreach (CoreProperty p in db.DbMapping.Load<CoreProperty>())
			{
				_coreProperties.Add(p);
			}
		}

		public void LoadSchemes(DatabaseBase db)
		{
			_schemes.Clear();
			_schemes.Add(Scheme.Default);
			db.DbMapping.CreateTableIfNotExists<Scheme>();
			foreach (Scheme s in db.DbMapping.Load<Scheme>())
			{
				_schemes.Add(s);
			}
			db.DbMapping.CreateTableIfNotExists<SchemeText>();
			foreach (SchemeText t in db.DbMapping.Load<SchemeText>())
			{
				try
				{
					_schemes[t.RefId].SetDescription(t.Text, _languages[t.CountryCode]);
				}
				catch
				{
					_log.ContextLog(LogLevelEnum.Info, $"Scheme is not exist [SchemeText: {t.Id}]");
				}
			}
		}

		public void LoadTableObjects(DatabaseBase db)
		{
			List<Task> tasks = new List<Task>();
			_objects.Clear();
			db.DbMapping.CreateTableIfNotExists<TableObject>();
			if (db.TableExists("tables"))
			{
				using IDataReader dataReader = db.ExecuteReader("SELECT COLUMN_NAME, COLUMN_TYPE, EXTRA FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tables' AND TABLE_SCHEMA = '" + db.DbConfig.DbName + "' AND COLUMN_NAME IN ('name','id','transaction_nr') ORDER BY COLUMN_NAME");
				dataReader.Read();
				if (dataReader.GetString(1).ToLower() != "int(11)" && dataReader.GetString(2).ToLower() != "auto_increment")
				{
					specDbX.AlterColumnType("tables", "id", "INT(11) auto_increment");
				}
				dataReader.Read();
				if (dataReader.GetString(1).ToLower() != "varchar(255)")
				{
					specDbX.AlterColumnType("tables", "name", "varchar(255) DEFAULT NULL");
				}
				dataReader.Read();
				if (dataReader.GetString(1).ToLower() != "varchar(64)")
				{
					specDbX.AlterColumnType("tables", "transaction_nr", "varchar(64) DEFAULT NULL");
				}
			}
			db.DbMapping.AddColumn("tables", "Int32", new DbColumnAttribute("optimization_hidden"), null, null, "0");
			LoadTableTransaction(db);
			Logger("TableTransactions");
			db.DbMapping.CreateTableIfNotExists<TableArchiveInformation>();
			_archiveInformation.Clear();
			foreach (TableArchiveInformation o in db.DbMapping.Load<TableArchiveInformation>())
			{
				_archiveInformation.Add(o);
			}
			tasks.Add(Task.Factory.StartNew(delegate
			{
				using DatabaseBase databaseBase4 = _cmanager.GetConnection();
				databaseBase4.DbMapping.CreateTableIfNotExists<ArchiveSetting>();
				_archives = new ArchiveCollection();
				foreach (Archive a2 in databaseBase4.DbMapping.Load<Archive>("type = " + 4))
				{
					if (_archives.Where((IArchive archive) => archive.Database.ToLower() == a2.Database.ToLower()).Any())
					{
						throw new Exception("no multiple archive tables per system allowed");
					}
					a2.Category = _categories[a2.CategoryId];
					a2.DefaultScheme = _schemes[a2.DefaultSchemeId];
					(a2.Category.TableObjects as TableObjectCollection).Add(a2);
					foreach (ArchiveSetting current2 in databaseBase4.DbMapping.Load<ArchiveSetting>("table_id = " + a2.Id))
					{
						(a2.Settings as ArchiveSettingsCollection).Add(current2);
					}
					_objects.Add(a2);
					_archives.Add(a2);
				}
				foreach (ArchiveDocument a in databaseBase4.DbMapping.Load<ArchiveDocument>("type = " + 8))
				{
					if (_archiveDocuments.Where((IArchiveDocument archive) => archive.Database.ToLower() == a.Database.ToLower()).Any())
					{
						throw new Exception("no multiple archive tables per system allowed");
					}
					a.Category = _categories[a.CategoryId];
					a.DefaultScheme = _schemes[a.DefaultSchemeId];
					(a.Category.TableObjects as TableObjectCollection).Add(a);
					foreach (ArchiveSetting current3 in databaseBase4.DbMapping.Load<ArchiveSetting>("table_id = " + a.Id))
					{
						(a.Settings as ArchiveSettingsCollection).Add(current3);
					}
					_objects.Add(a);
					_archiveDocuments.Add(a);
				}
			}));
			tasks.Add(Task.Factory.StartNew(delegate
			{
				using DatabaseBase databaseBase3 = _cmanager.GetConnection();
				_issues.Clear();
				foreach (Issue k in databaseBase3.DbMapping.Load<Issue>("type = " + 3))
				{
					k.Category = _categories[k.CategoryId];
					k.DefaultScheme = _schemes[k.DefaultSchemeId];
					(k.Schemes as SchemeCollection).Add(k.DefaultScheme as Scheme);
					(k.Category.TableObjects as TableObjectCollection).Add(k);
					TableTransactions tableTransactions3 = _tableTransactions.SingleOrDefault((TableTransactions t) => t.TableId == k.Id);
					if (tableTransactions3 != null)
					{
						k.TransactionNumber = tableTransactions3.TransactionNumber;
					}
					_objects.Add(k);
					_issues.Add(k);
				}
			}));
			tasks.Add(Task.Factory.StartNew(delegate
			{
				using DatabaseBase databaseBase2 = _cmanager.GetConnection();
				_views.Clear();
				foreach (View v in databaseBase2.DbMapping.Load<View>("type = " + 2))
				{
					v.Category = _categories[v.CategoryId];
					v.DefaultScheme = _schemes[v.DefaultSchemeId];
					(v.Schemes as SchemeCollection).Add(v.DefaultScheme as Scheme);
					(v.Category.TableObjects as TableObjectCollection).Add(v);
					_objects.Add(v);
					TableTransactions tableTransactions2 = _tableTransactions.SingleOrDefault((TableTransactions t) => t.TableId == v.Id);
					if (tableTransactions2 != null)
					{
						v.TransactionNumber = tableTransactions2.TransactionNumber;
					}
					_views.Add(v);
				}
			}));
			tasks.Add(Task.Factory.StartNew(delegate
			{
				using DatabaseBase databaseBase = _cmanager.GetConnection();
				_tables.Clear();
				foreach (global::SystemDb.Internal.Table t3 in databaseBase.DbMapping.Load<global::SystemDb.Internal.Table>("type = " + 1))
				{
					t3.Category = _categories[t3.CategoryId];
					t3.DefaultScheme = _schemes[t3.DefaultSchemeId];
					(t3.Schemes as SchemeCollection).Add(t3.DefaultScheme as Scheme);
					(t3.Category.TableObjects as TableObjectCollection).Add(t3);
					TableTransactions tableTransactions = _tableTransactions.SingleOrDefault((TableTransactions tt) => tt.TableId == t3.Id);
					if (tableTransactions != null)
					{
						t3.TransactionNumber = tableTransactions.TransactionNumber;
					}
					_objects.Add(t3);
					_tables.Add(t3);
				}
				databaseBase.DbMapping.CreateTableIfNotExists<TableOriginalName>();
				foreach (TableOriginalName current in databaseBase.DbMapping.Load<TableOriginalName>())
				{
					(_tables[current.RefId] as global::SystemDb.Internal.Table).OriginalName = current.OriginalName;
				}
			}));
			Task.WaitAll(tasks.ToArray());
			db.DbMapping.CreateTableIfNotExists<TableObjectText>();
			foreach (TableObjectText t2 in db.DbMapping.Load<TableObjectText>())
			{
				TableObject tobj = _objects[t2.RefId] as TableObject;
				if (tobj != null)
				{
					tobj.SetDescription(t2.Text, _languages[t2.CountryCode]);
				}
				else
				{
					db.DbMapping.Delete(t2);
				}
			}
			db.DbMapping.CreateTableIfNotExists<ObjectTypes>();
			ObjectTypesCollection.Clear();
			foreach (ObjectTypes objectType in db.DbMapping.Load<ObjectTypes>())
			{
				ObjectTypesCollection.Add(objectType.Id, objectType.Value);
			}
			db.DbMapping.CreateTableIfNotExists<ObjectTypeText>();
			foreach (ObjectTypeText objectTypeText in db.DbMapping.Load<ObjectTypeText>())
			{
				_objectTypeTextCollection.Add(objectTypeText);
			}
			db.DbMapping.CreateTableIfNotExists<ObjectTypeRelations>();
			foreach (ObjectTypeRelations objectTypeRelations in db.DbMapping.Load<ObjectTypeRelations>())
			{
				_objectTypeRelationsCollection.Add(objectTypeRelations);
			}
			foreach (ITableObject tableObject in _objects)
			{
				SetObjectType(tableObject as TableObject, db);
				if (!ObjectTypeRelationsCollection.IsEmpty)
				{
					(tableObject as TableObject).ExtendedObjectType = _objectTypeRelationsCollection.ValueByObjectId((tableObject as TableObject).ObjectTypeCode);
				}
			}
			_roleTableObjectRights.Clear();
			db.DbMapping.CreateTableIfNotExists<TableRoleMapping>();
			using (IDataReader dr = db.ExecuteReader("SELECT COLUMN_NAME, COLUMN_TYPE, EXTRA FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'table_roles' AND TABLE_SCHEMA = '" + db.DbConfig.DbName + "' AND COLUMN_NAME = 'id' ORDER BY COLUMN_NAME"))
			{
				dr.Read();
				if (dr.GetString(1).ToLower() != "bigint" && dr.GetString(2).ToLower() != "auto_increment")
				{
					specDbX.AlterColumnType("table_roles", "id", "BIGINT auto_increment");
				}
			}
			foreach (TableRoleMapping j in db.DbMapping.Load<TableRoleMapping>())
			{
				ITableObject obj = _objects[j.TableId];
				if (obj != null)
				{
					_roleTableObjectRights.Add(_roles[j.RoleId], obj, j.Id, j.Right);
				}
				else
				{
					db.DbMapping.Delete<TableRoleMapping>("table_id = " + j.TableId);
				}
			}
			_userTableObjectRights.Clear();
			db.DbMapping.CreateTableIfNotExists<TableUserMapping>();
			foreach (TableUserMapping i in db.DbMapping.Load<TableUserMapping>())
			{
				if (_objects[i.TableId] != null)
				{
					_userTableObjectRights.Add(_users[i.UserId], _objects[i.TableId], i.Id, i.Right);
				}
				else
				{
					db.DbMapping.Delete<TableUserMapping>("table_id = " + i.TableId);
				}
			}
			try
			{
				db.DbMapping.CreateTableIfNotExists<UserTableColumnWidthsSettings>();
				foreach (UserTableColumnWidthsSettings s4 in db.DbMapping.Load<UserTableColumnWidthsSettings>())
				{
					s4.User = Users[s4.UserId];
					s4.TableObject = Objects[s4.TableId];
					if (s4.User != null && s4.TableObject != null)
					{
						_userTableColumnWidthSettings.Add(s4);
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			db.DbMapping.CreateTableIfNotExists<UserTableTransactionIdSettings>();
			db.AlterColumnType("user_table_transactionid_settings", "transaction_id", "varchar(64)");
			foreach (UserTableTransactionIdSettings s3 in db.DbMapping.Load<UserTableTransactionIdSettings>())
			{
				s3.User = Users[s3.UserId];
				s3.Table = Objects[s3.TableId];
				_userTableTransactionIdSettings.Add(s3);
			}
			db.DbMapping.CreateTableIfNotExists<UserColumnOrderSettings>();
			foreach (UserColumnOrderSettings s2 in db.DbMapping.Load<UserColumnOrderSettings>())
			{
				s2.User = Users[s2.UserId];
				s2.TableObject = Objects[s2.TableId];
				_userColumnOrderSettings.Add(s2);
			}
			db.DbMapping.CreateTableIfNotExists<UserTableObjectSettings>();
			foreach (UserTableObjectSettings s in db.DbMapping.Load<UserTableObjectSettings>())
			{
				s.User = Users[s.UserId];
				s.TableObject = Objects[s.TableId];
				if (s.User != null && s.TableObject != null)
				{
					_userTableObjectSettings.Add(s);
				}
			}
		}

		public void LoadIssueExtensions(DatabaseBase db)
		{
			db.DbMapping.CreateTableIfNotExists<IssueExtension>();
			db.DbMapping.AddColumn("issue_extensions", "Boolean", new DbColumnAttribute("language_value"), null, null, "0");
			db.DbMapping.AddColumn("issue_extensions", "Boolean", new DbColumnAttribute("need_bukrs"), null, null, "0");
			List<IssueExtension> list = db.DbMapping.Load<IssueExtension>();
			bool checkedProcedure = false;
			TableObjectCollection tableObjectsForModifying = null;
			foreach (IssueExtension i in list)
			{
				Issue issue = _issues[i.RefId] as Issue;
				if (issue != null)
				{
					issue.IssueType = ((i.TableObjectId <= 0) ? IssueType.StoredProcedure : IssueType.Filter);
					issue.Command = i.Command;
					issue.OriginalId = i.TableObjectId;
					issue.FilterTableObject = _objects[i.TableObjectId];
					issue.UseLanguageValue = i.UseLanguageValue;
					issue.UseIndexValue = i.UseIndexValue;
					issue.UseSortValue = i.UseSortValue;
					issue.UseSplitValue = i.UseSplitValue;
					issue.RowNoFilter = i.RowNoFilter;
					issue.NeedGJahr = i.NeedGJahr;
					issue.NeedBukrs = i.NeedBukrs;
					issue.Flag = i.Flag;
					if (_useNewIssueMethod && !i.Checked && issue.IssueType == IssueType.StoredProcedure)
					{
						checkedProcedure = true;
						tableObjectsForModifying = (TableObjectCollection)ModifyIssue(db, issue, tableObjectsForModifying);
					}
					if (issue.IssueType == IssueType.StoredProcedure)
					{
						issue.OrderAreas = new OrderAreaCollection(issue);
					}
					else if (issue.OrderAreas.Count == 0)
					{
						issue.OrderAreas = new OrderAreaCollection(issue.FilterTableObject);
					}
				}
			}
			if (checkedProcedure)
			{
				db.ExecuteNonQuery("UPDATE " + db.DbMapping.GetTableName<IssueExtension>() + " SET checked=1");
			}
		}

		public void LoadTableSchemes(DatabaseBase db)
		{
			db.DbMapping.CreateTableIfNotExists<TableObjectSchemeMapping>();
			foreach (TableObjectSchemeMapping i in db.DbMapping.Load<TableObjectSchemeMapping>())
			{
				(_objects[i.TableObjectId].Schemes as SchemeCollection)?.Add(_schemes[i.SchemeId] as Scheme);
			}
		}

		public void LoadOrderAreas(DatabaseBase db)
		{
			db.DbMapping.CreateTableIfNotExists<global::SystemDb.Internal.OrderArea>();
			db.AlterColumnType("order_areas", "id", "int(11) not null auto_increment");
			db.DbMapping.AddColumn("order_areas", "String", new DbColumnAttribute("language_value")
			{
				StoreAsVarBinary = true,
				AllowDbNull = true
			}, null);
			foreach (global::SystemDb.Internal.OrderArea o in db.DbMapping.Load<global::SystemDb.Internal.OrderArea>())
			{
				TableObject tobj = _objects[o.TableId] as TableObject;
				if (tobj != null)
				{
					if (tobj.OrderAreas == null)
					{
						tobj.OrderAreas = new OrderAreaCollection(tobj);
					}
					(tobj.OrderAreas as OrderAreaCollection).Add(o);
				}
			}
		}

		public void LoadColumns(DatabaseBase db)
		{
			_columns.Clear();
			if (db.TableExists("columns"))
			{
				using IDataReader dr = db.ExecuteReader("SELECT COLUMN_NAME, COLUMN_TYPE, EXTRA FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'columns' AND TABLE_SCHEMA = '" + db.DbConfig.DbName + "' AND COLUMN_NAME = 'name' ORDER BY COLUMN_NAME");
				dr.Read();
				if (dr.GetString(1).ToLower() != "varchar(256)")
				{
					specDbX.AlterColumnType("columns", "name", "varchar(256) DEFAULT NULL");
				}
			}
			db.DbMapping.CreateTableIfNotExists<global::SystemDb.Internal.Column>();
			foreach (global::SystemDb.Internal.Column c in db.DbMapping.Load<global::SystemDb.Internal.Column>())
			{
				c.Table = _objects[c.TableId];
				switch (c.OptimizationType)
				{
				case OptimizationType.IndexTable:
					(c.Table as TableObject).IndexTableColumn = c;
					break;
				case OptimizationType.SplitTable:
					(c.Table as TableObject).SplitTableColumn = c;
					break;
				case OptimizationType.SortColumn:
					(c.Table as TableObject).SortColumn = c;
					break;
				}
				if (!_objects.Contains(c.TableId))
				{
					continue;
				}
				ITableObject table = _objects[c.TableId];
				if (table.Type != TableType.Table || table.RowCount != 0L)
				{
					ColumnCollection cols = table.Columns as ColumnCollection;
					if (cols != null && !cols.Contains(c.Name))
					{
						cols.Add(c);
						_columns.Add(c);
					}
				}
			}
			db.DbMapping.CreateTableIfNotExists<ColumnText>();
			foreach (ColumnText t in db.DbMapping.Load<ColumnText>())
			{
				try
				{
					if (_columns[t.RefId] != null)
					{
						_columns[t.RefId].SetDescription(t.Text, _languages[t.CountryCode]);
					}
				}
				catch
				{
					_log.ContextLog(LogLevelEnum.Info, $"Column does not exist [ColumnText: {t.Id}]");
				}
			}
			Task.Factory.StartNew(delegate
			{
				foreach (IIssue issue in _objects.Where((ITableObject o) => o.Type == TableType.Issue && (o as IIssue).IssueType == IssueType.Filter).ToList())
				{
					try
					{
						if (issue.FilterTableObject != null)
						{
							foreach (IColumn current in issue.FilterTableObject.Columns.ToList())
							{
								issue.Columns.Add(current);
							}
						}
					}
					catch (Exception message)
					{
						_log.Error(message);
					}
				}
			}, TaskCreationOptions.AttachedToParent);
			_roleColumnRights.Clear();
			db.DbMapping.CreateTableIfNotExists<ColumnRoleMapping>();
			foreach (ColumnRoleMapping j in db.DbMapping.Load<ColumnRoleMapping>())
			{
				_roleColumnRights.Add(_roles[j.RoleId], _columns[j.ColumnId], j.Id, j.Right);
			}
			_userColumnRights.Clear();
			db.DbMapping.CreateTableIfNotExists<ColumnUserMapping>();
			foreach (ColumnUserMapping i in db.DbMapping.Load<ColumnUserMapping>())
			{
				_userColumnRights.Add(_users[i.UserId], _columns[i.ColumnId], i.Id, i.Right);
			}
			db.DbMapping.CreateTableIfNotExists<UserColumnSettings>();
			foreach (UserColumnSettings s in db.DbMapping.Load<UserColumnSettings>())
			{
				s.User = Users[s.UserId];
				s.Column = Columns[s.ColumnId];
				if (s.User != null && s.Column != null)
				{
					_userColumnSettings.Add(s);
				}
			}
			db.DbMapping.CreateTableIfNotExists<ColumnFilters>();
			if (db.DbMapping.Load<ColumnFilters>().Count != 0)
			{
				return;
			}
			IArchive archive = Archives.FirstOrDefault();
			if (archive == null)
			{
				return;
			}
			foreach (IColumn relationColumn in Columns.Where((IColumn w) => w.Table == archive && (w.Name.ToLower() == "beleg_nr" || w.Name.ToLower() == "ar_date" || w.Name.ToLower() == "del_date" || w.Name.ToLower() == "arc_doc_id" || w.Name.ToLower() == "name" || w.Name.ToLower() == "name_2")))
			{
				ColumnFilters columnFilter = new ColumnFilters
				{
					ColumnId = relationColumn.Id,
					TableId = archive.Id
				};
				db.DbMapping.Save(columnFilter);
			}
		}

		public void LoadIndexes(DatabaseBase db)
		{
			LoadIndexesObjects(db);
		}

		public void LoadParameters(DatabaseBase db)
		{
			_parameters = new Dictionary<int, IParameter>();
			if (db.TableExists("parameter"))
			{
				using IDataReader dr = db.ExecuteReader("SELECT COLUMN_NAME, COLUMN_TYPE, EXTRA FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'parameter' AND TABLE_SCHEMA = '" + db.DbConfig.DbName + "' AND COLUMN_NAME IN ('name','column_name','table_name') ORDER BY COLUMN_NAME");
				dr.Read();
				if (dr.GetString(1).ToLower() != "varchar(256)")
				{
					specDbX.AlterColumnType("parameter", "name", "varchar(256) DEFAULT NULL");
				}
				dr.Read();
				if (dr.GetString(1).ToLower() != "varchar(256)")
				{
					specDbX.AlterColumnType("parameter", "column_name", "varchar(256) DEFAULT NULL");
				}
				dr.Read();
				if (dr.GetString(1).ToLower() != "varchar(256)")
				{
					specDbX.AlterColumnType("parameter", "table_name", "varchar(256) DEFAULT NULL");
				}
			}
			db.DbMapping.CreateTableIfNotExists<Parameter>();
			db.DbMapping.AddColumn("parameter", "Boolean", new DbColumnAttribute("is_required"), null, null, "0");
			db.DbMapping.AddColumn("parameter", "Int32", new DbColumnAttribute("group_id"), null, null, "0");
			db.DbMapping.AddColumn("parameter", "Int32", new DbColumnAttribute("optimization_type"), null, null, "0");
			db.DbMapping.AddColumn("parameter", "Int32", new DbColumnAttribute("optimization_direction"), null, null, "0");
			db.DbMapping.AddColumn("parameter", "Int32", new DbColumnAttribute("optionally_required"), null, null, "0");
			db.DbMapping.AddColumn("parameter", "Int32", new DbColumnAttribute("freeselection"), null, null, "0");
			db.DbMapping.AddColumn("parameter", "Int32", new DbColumnAttribute("leading_zeros"), null, null, "0");
			db.DbMapping.AddColumn("parameter", "String", new DbColumnAttribute("column_name_in_view"), null, null, "''");
			db.DbMapping.AddColumn("parameter", "Int32", new DbColumnAttribute("use_absolute"), null, null, "0");
			db.DbMapping.AddColumn("parameter", "String", new DbColumnAttribute("description_column_name"), null, null, "0");
			foreach (Parameter p2 in db.DbMapping.Load<Parameter>())
			{
				IIssue issue = _issues[p2.IssueId];
				if (issue != null)
				{
					(issue.Parameters as ParameterCollection).Add(p2);
					_parameters.Add(p2.Id, p2);
					p2.Issue = issue;
				}
			}
			db.DbMapping.CreateTableIfNotExists<ParameterText>();
			foreach (ParameterText t2 in db.DbMapping.Load<ParameterText>())
			{
				if (t2.CountryCode == null || !_languages.ContainsKey(t2.CountryCode) || !_parameters.ContainsKey(t2.RefId))
				{
					continue;
				}
				try
				{
					if (_parameters.ContainsKey(t2.RefId) && _languages.ContainsKey(t2.CountryCode))
					{
						_parameters[t2.RefId].SetDescription(t2.Text, _languages[t2.CountryCode]);
					}
				}
				catch
				{
					_log.ContextLog(LogLevelEnum.Info, $"Parameter is not exist [ParameterText: {t2.Id}]");
				}
			}
			Dictionary<int, ParameterValue> parameter_values = new Dictionary<int, ParameterValue>();
			Dictionary<int, List<ParameterValue>> parameter_collection = new Dictionary<int, List<ParameterValue>>();
			db.DbMapping.CreateTableIfNotExists<ParameterValue>();
			foreach (ParameterValue v in db.DbMapping.Load<ParameterValue>())
			{
				parameter_values.Add(v.Id, v);
				if (!parameter_collection.ContainsKey(v.CollectionId))
				{
					parameter_collection.Add(v.CollectionId, new List<ParameterValue>());
				}
				parameter_collection[v.CollectionId]?.Add(v);
			}
			db.DbMapping.CreateTableIfNotExists<ParameterValueSetText>();
			foreach (ParameterValueSetText t in db.DbMapping.Load<ParameterValueSetText>())
			{
				if (parameter_values.ContainsKey(t.RefId))
				{
					try
					{
						parameter_values[t.RefId].SetDescription(t.Text, _languages[t.CountryCode]);
					}
					catch
					{
						_log.ContextLog(LogLevelEnum.Info, $"ParameterValueSet is not exist [ParameterValueSetText: {t.Id}]");
					}
				}
			}
			db.DbMapping.CreateTableIfNotExists<ParameterCollectionMapping>();
			foreach (ParameterCollectionMapping j in db.DbMapping.Load<ParameterCollectionMapping>())
			{
				try
				{
					if (_parameters.ContainsKey(j.ParameterId) && parameter_collection.ContainsKey(j.CollectionId))
					{
						_parameters[j.ParameterId].Values = parameter_collection[j.CollectionId];
					}
				}
				catch
				{
					_log.ContextLog(LogLevelEnum.Warn, $"Parameter missing from collection. Collection id is {j.ParameterId}.");
				}
			}
			db.DbMapping.CreateTableIfNotExists<ParameterValueOrder>();
			db.DbMapping.CreateTableIfNotExists<ParameterSelectionLogic>();
			foreach (ParameterSelectionLogic pl in db.DbMapping.Load<ParameterSelectionLogic>())
			{
				ParameterSelectionLogics.Add(pl);
			}
			db.DbMapping.CreateTableIfNotExists<ParameterSelectionLogicText>();
			foreach (ParameterSelectionLogicText plt in db.DbMapping.Load<ParameterSelectionLogicText>())
			{
				ParameterSelectionLogics.FirstOrDefault((ParameterSelectionLogic p) => p.Id == plt.RefId)?.SetText(plt);
			}
			foreach (IIssue i in _issues)
			{
				ParameterSelectionLogic logic = ParameterSelectionLogics.FirstOrDefault((ParameterSelectionLogic l) => l.IssueId == i.Id);
				if (logic != null)
				{
					i.Logic = logic;
				}
			}
		}

		public void LoadParameterHistory(DatabaseBase db)
		{
			if (db.TableExists("user_issueparameter_history"))
			{
				using (IDataReader dr = db.ExecuteReader("SELECT COLUMN_NAME, COLUMN_TYPE, EXTRA FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'user_issueparameter_history' AND TABLE_SCHEMA = '" + db.DbConfig.DbName + "' AND COLUMN_NAME IN ('name','value') ORDER BY COLUMN_NAME"))
				{
					dr.Read();
					if (dr.GetString(1).ToLower() != "text")
					{
						specDbX.AlterColumnType("user_issueparameter_history", "name", "text DEFAULT NULL");
					}
					dr.Read();
					if (dr.GetString(1).ToLower() != "text")
					{
						specDbX.AlterColumnType("user_issueparameter_history", "value", "text DEFAULT NULL");
					}
				}
				db.DbMapping.AddColumn("user_issueparameter_history", "Int32", new DbColumnAttribute("selection_type"), null, null, "0");
			}
			db.DbMapping.CreateTableIfNotExists<HistoryParameterValue>();
			foreach (HistoryParameterValue historyParameterValue in db.DbMapping.Load<HistoryParameterValue>())
			{
				_userHistoryIssueParameterCollection.Add(historyParameterValue);
			}
		}

		public void LoadFreeSelection(DatabaseBase db)
		{
			_userHistoryFreeSelectionIssueParameterCollection.Clear();
			db.DbMapping.CreateTableIfNotExists<HistoryParameterValueFreeSelection>();
			foreach (HistoryParameterValueFreeSelection historyParameterFreeselectionValue in db.DbMapping.Load<HistoryParameterValueFreeSelection>())
			{
				_userHistoryFreeSelectionIssueParameterCollection.Add(historyParameterFreeselectionValue);
			}
			if (!db.TableExists("user_free_selection_parameter_history"))
			{
				return;
			}
			using IDataReader dr = db.ExecuteReader("SELECT COLUMN_NAME, COLUMN_TYPE, EXTRA FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'user_free_selection_parameter_history' AND TABLE_SCHEMA = '" + db.DbConfig.DbName + "' AND COLUMN_NAME IN ('id','value') ORDER BY COLUMN_NAME");
			dr.Read();
			if (dr.GetString(1).ToLower() != "bigint(20)" && dr.GetString(2).ToLower() != "auto_increment")
			{
				specDbX.AlterColumnType("user_free_selection_parameter_history", "id", "bigint(20) auto_increment");
			}
			dr.Read();
			if (dr.GetString(1).ToLower() != "text")
			{
				specDbX.AlterColumnType("user_free_selection_parameter_history", "value", "TEXT DEFAULT NULL");
			}
		}

		public void LoadRelations(DatabaseBase db)
		{
			db.DbMapping.CreateTableIfNotExists<Relation>();
			try
			{
				db.DbMapping.AddColumn("relations", "Boolean", new DbColumnAttribute("user_defined")
				{
					AllowDbNull = false
				}, null, null, db.GetSqlString("0"));
			}
			catch (Exception)
			{
			}
			Dictionary<int, TableObject.Relation> relations = new Dictionary<int, TableObject.Relation>();
			foreach (Relation r in db.DbMapping.Load<Relation>())
			{
				if (!relations.ContainsKey(r.RelationId))
				{
					relations.Add(r.RelationId, new TableObject.Relation());
				}
				TableObject.Relation relation2 = relations[r.RelationId];
				if (relation2 == null)
				{
					continue;
				}
				IColumn columnParent = _columns[r.ParentId];
				IDataObject columnChild;
				switch (r.Type)
				{
				case 0:
				case 3:
					columnChild = _columns[r.ChildId];
					if (relation2.Count > 0 && (columnParent == null || columnChild == null || relation2[0].Source.Table.Id != columnParent.Table.Id || (relation2[0].Target as IColumn).Table.Id != (columnChild as IColumn).Table.Id))
					{
						_log.ContextLog(LogLevelEnum.Warn, $"Incorrect relation: Child or parent column is incorrect. Relation id: {r.RelationId}; Parent id: {r.ParentId}; Child id: {r.ChildId}.");
						continue;
					}
					break;
				case 2:
					columnChild = _parameters[r.ChildId];
					if (relation2.Count > 0 && (columnParent == null || columnChild == null || relation2[0].Source.Table.Id != columnParent.Table.Id || (relation2[0].Target as IParameter).Issue.Id != (columnChild as IParameter).Issue.Id))
					{
						_log.ContextLog(LogLevelEnum.Warn, $"Incorrect relation: Child or parent column is incorrect. Relation id: {r.RelationId}; Parent id: {r.ParentId}; Child id: {r.ChildId}.");
						continue;
					}
					break;
				default:
					columnChild = null;
					break;
				}
				relation2.RelationId = r.RelationId;
				relation2.Add(new TableObject.ColumnConnection
				{
					Source = columnParent,
					Target = columnChild,
					Operator = r.Operator,
					FullLine = r.FullLine,
					RelationType = (RelationType)r.Type,
					ExtInfo = r.ExtInfo,
					ColumnExtInfo = r.ColumnExtInfo,
					RelationId = r.RelationId,
					UserDefined = r.UserDefined
				});
			}
			foreach (KeyValuePair<int, TableObject.Relation> relation in relations)
			{
				if (relation.Value[0].Source != null && relation.Value[0].Target != null)
				{
					(relation.Value[0].Source.Table as TableObject).AddRelation(relation.Value);
				}
			}
		}

		public void LoadExtendedColumnInformation(DatabaseBase db)
		{
			db.DbMapping.CreateTableIfNotExists<ExtendedColumnInformation>();
			foreach (ExtendedColumnInformation inf in db.DbMapping.Load<ExtendedColumnInformation>())
			{
				if (inf.ChildColumnId != 0 && inf.ParentColumnId != 0 && inf.InformationColumnId != 0)
				{
					if (!_columns.ContainsKey(inf.ParentColumnId) && !_columns.ContainsKey(inf.ChildColumnId) && !_columns.ContainsKey(inf.InformationColumnId) && _columns[inf.ParentColumnId].Table == null && !_objects.Contains(_columns[inf.ParentColumnId].Table.Id) && !_objects[_columns[inf.ParentColumnId].Table.Id].ExtendedColumnInformations.ContainsKey(_columns[inf.ParentColumnId].Name))
					{
						_objects[_columns[inf.ParentColumnId].Table.Id].ExtendedColumnInformations.Add(_columns[inf.ParentColumnId].Name, new List<IColumnConnection>());
					}
					_objects[_columns[inf.ParentColumnId].Table.Id].ExtendedColumnInformations[_columns[inf.ParentColumnId].Name].Add(new TableObject.ColumnConnection
					{
						Source = _columns[inf.ChildColumnId],
						Target = _columns[inf.InformationColumnId]
					});
				}
			}
		}

		public void LoadAbout(DatabaseBase db)
		{
			db.DbMapping.CreateTableIfNotExists<About>();
			_about = db.DbMapping.Load<About>().FirstOrDefault();
		}

		public void LoadNotes(DatabaseBase db)
		{
			_notes.Clear();
			db.DbMapping.CreateTableIfNotExists<Note>();
			foreach (Note i in db.DbMapping.Load<Note>())
			{
				if (_users.ContainsKey(i.UserId))
				{
					i.User = _users[i.UserId];
					_notes.Add(i);
				}
			}
		}

		public void LoadEmptyDistinctTable(DatabaseBase db)
		{
			_notes.Clear();
			db.DbMapping.CreateTableIfNotExists<EmptyDistinctColumn>();
		}

		public void LoadTableCruds(DatabaseBase db)
		{
			_tableCruds.Clear();
			db.DbMapping.CreateTableIfNotExists<TableCrud>();
			db.DbMapping.CreateTableIfNotExists<TableCrudColumn>();
			List<TableCrud> list3 = db.DbMapping.Load<TableCrud>();
			List<TableCrudColumn> list2 = db.DbMapping.Load<TableCrudColumn>();
			foreach (TableCrud i in list3)
			{
				if (!_objects.Contains(i.TableId))
				{
					continue;
				}
				i.Table = _objects[i.TableId];
				if (!_objects.Contains(i.OnTableId))
				{
					continue;
				}
				i.OnTable = _objects[i.OnTableId];
				foreach (TableCrudColumn col in list2.Where((TableCrudColumn w) => w.TableCrudId == i.Id))
				{
					if (_columns.ContainsKey(col.ColumnId))
					{
						col.Column = _columns[col.ColumnId];
						if (_columns.ContainsKey(col.FromColumnId))
						{
							col.FromColumn = _columns[col.FromColumnId];
						}
						i.Columns.Add(col);
					}
				}
				if (i.Columns.Count > 0)
				{
					_tableCruds.Add(i);
				}
			}
		}

		public void LoadRowVisibility(DatabaseBase db)
		{
			db.DbMapping.CreateTableIfNotExists<RowVisibilityCountCache>();
			foreach (RowVisibilityCountCache item2 in db.DbMapping.Load<RowVisibilityCountCache>())
			{
				_rowVisibilityCountCache.Add(item2);
			}
			db.DbMapping.CreateTableIfNotExists<RowVisibility>();
			List<RowVisibility> list = db.DbMapping.Load<RowVisibility>();
			for (int i = 0; i < (from e in list
				group e by e.TableId).Count(); i++)
			{
				IGrouping<int, RowVisibility> roleLimit = (from e in list
					group e by e.TableId).ToList()[i];
				List<Tuple<int, List<string>>> struc = new List<Tuple<int, List<string>>>();
				List<string> values = new List<string>();
				ITableObject neededObject = GetTableObjectNeeded(roleLimit.Key);
				if (neededObject == null)
				{
					continue;
				}
				List<IGrouping<int, RowVisibility>> list2 = (from v in list.Where((RowVisibility f) => f.TableId == neededObject.Id).ToList()
					group v by v.RoleId).ToList();
				neededObject.RoleBasedFilters = new Dictionary<int, List<Tuple<int, List<string>>>>();
				foreach (IGrouping<int, RowVisibility> filterList in list2)
				{
					foreach (RowVisibility item in filterList)
					{
						filterList.Where((RowVisibility f) => f.ColumnId == item.ColumnId && !struc.Any((Tuple<int, List<string>> a) => a.Item1 == item.ColumnId)).ToList().ForEach(delegate(RowVisibility v)
						{
							values.Add(v.FilterValue);
						});
						if (values != null && values.Count > 0)
						{
							struc.Add(new Tuple<int, List<string>>(item.ColumnId, values.ToList()));
						}
						values.Clear();
					}
					neededObject.RoleBasedFilters.Add(filterList.Key, struc.ToList());
					struc.Clear();
				}
				neededObject.RoleBasedFilterRowCount = _rowVisibilityCountCache.Where((RowVisibilityCountCache r) => r.TableId == neededObject.Id).ToList();
			}
		}

		public void LoadRowVisibilityNew(DatabaseBase db)
		{
			List<ColumnDictionary> cd = new List<ColumnDictionary>();
			db.DbMapping.CreateTableIfNotExists<ColumnDictionary>();
			cd = db.DbMapping.Load<ColumnDictionary>();
			foreach (ColumnDictionary item2 in cd)
			{
				_log.Log(LogLevelEnum.Info, item2.ToString() + " - " + cd.IndexOf(item2));
			}
			db.DbMapping.CreateTableIfNotExists<RowVisibilityNew>();
			db.DbMapping.AddColumn("row_visibility_new", "String", new DbColumnAttribute("column_name"), null);
			List<RowVisibilityNew> list = db.DbMapping.Load<RowVisibilityNew>();
			for (int i = 0; i < (from e in list
				group e by e.ColumnName).Count(); i++)
			{
				IGrouping<string, RowVisibilityNew> roleLimit = (from e in list
					group e by e.ColumnName).ToList()[i];
				new List<Tuple<int, string>>();
				List<ITableObject> neededObject = GetTableObjectByColumName(roleLimit.Key);
				new List<ITableObject>();
				if (neededObject != null && neededObject.Count > 0)
				{
					foreach (ITableObject ito2 in neededObject)
					{
						foreach (RowVisibilityNew element2 in roleLimit)
						{
							if (ito2 is View && ito2.RoleBasedFiltersNew == null)
							{
								(ito2 as View).RoleBasedFiltersNew = new Dictionary<int, List<Tuple<string, string>>>();
							}
							else if (ito2 is Issue && ito2.RoleBasedFiltersNew == null)
							{
								(ito2 as Issue).RoleBasedFiltersNew = new Dictionary<int, List<Tuple<string, string>>>();
							}
							else if (ito2 is global::SystemDb.Internal.Table && ito2.RoleBasedFiltersNew == null)
							{
								(ito2 as global::SystemDb.Internal.Table).RoleBasedFiltersNew = new Dictionary<int, List<Tuple<string, string>>>();
							}
							if (ito2.RoleBasedFiltersNew.ContainsKey(element2.RoleId))
							{
								ito2.RoleBasedFiltersNew[element2.RoleId].Add(Tuple.Create(element2.ColumnName, element2.FilterValue));
								continue;
							}
							List<Tuple<string, string>> tempList2 = new List<Tuple<string, string>>();
							tempList2.Add(Tuple.Create(element2.ColumnName, element2.FilterValue));
							ito2.RoleBasedFiltersNew.Add(element2.RoleId, tempList2);
						}
					}
				}
				if (cd.Count((ColumnDictionary x) => x.FilterType.ToLower() == roleLimit.Key.ToLower()) <= 0)
				{
					continue;
				}
				_log.Log(LogLevelEnum.Info, "cd.Count(x =>x.FilterType.ToLower()== roleLimit.Key.ToLower()) > 0" + cd.Count((ColumnDictionary x) => x.FilterType.ToLower() == roleLimit.Key.ToLower()));
				ITableObject ito = null;
				foreach (ColumnDictionary item in cd.Where((ColumnDictionary x) => x.FilterType.ToLower() == roleLimit.Key.ToLower()))
				{
					switch (item.Type)
					{
					case "1":
						ito = Tables.FirstOrDefault((ITable x) => x.TableName.ToLower() == item.Table.ToLower());
						break;
					case "2":
						ito = Views.FirstOrDefault((IView x) => x.TableName.ToLower() == item.Table.ToLower());
						break;
					case "3":
						ito = Issues.FirstOrDefault((IIssue x) => x.TableName.ToLower() == item.Table.ToLower());
						break;
					}
					if (ito == null)
					{
						Logger("No table found for the following filter: " + item.Table + "." + item.Column + " - " + item.FilterType + " (ID: " + item.Id + ")");
						continue;
					}
					foreach (RowVisibilityNew element in roleLimit)
					{
						if (ito != null && ito is View && ito.RoleBasedFiltersNew == null)
						{
							(ito as View).RoleBasedFiltersNew = new Dictionary<int, List<Tuple<string, string>>>();
						}
						else if (ito != null && ito is Issue && ito.RoleBasedFiltersNew == null)
						{
							(ito as Issue).RoleBasedFiltersNew = new Dictionary<int, List<Tuple<string, string>>>();
						}
						else if (ito != null && ito is global::SystemDb.Internal.Table && ito.RoleBasedFiltersNew == null)
						{
							(ito as global::SystemDb.Internal.Table).RoleBasedFiltersNew = new Dictionary<int, List<Tuple<string, string>>>();
						}
						if (ito.RoleBasedFiltersNew.ContainsKey(element.RoleId))
						{
							ito.RoleBasedFiltersNew[element.RoleId].Add(Tuple.Create(item.Column, element.FilterValue));
							continue;
						}
						List<Tuple<string, string>> tempList = new List<Tuple<string, string>>();
						tempList.Add(Tuple.Create(item.Column, element.FilterValue));
						ito.RoleBasedFiltersNew.Add(element.RoleId, tempList);
					}
				}
			}
		}

		public void LoadFileSystem(DatabaseBase db)
		{
			if (!db.TableExists("file_systems"))
			{
				return;
			}
			foreach (FileSys item in db.DbMapping.Load<FileSys>())
			{
				_fileSystems.Add(item);
			}
		}

		public void LoadStartScreen(DatabaseBase db)
		{
			db.DbMapping.CreateTableIfNotExists<StartScreen>();
			using (IDataReader dr = db.ExecuteReader("SELECT COLUMN_NAME, COLUMN_TYPE, EXTRA FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'startscreens' AND TABLE_SCHEMA = '" + db.DbConfig.DbName + "' AND COLUMN_NAME IN ('name','value') ORDER BY COLUMN_NAME"))
			{
				dr.Read();
				if (dr.GetString(1).ToLower() != "varchar(45)")
				{
					specDbX.AlterColumnType("startscreens", "name", "varchar(45) NOT NULL");
				}
				dr.Read();
				if (dr.GetString(1).ToLower() != "mediumtext")
				{
					specDbX.AlterColumnType("startscreens", "value", "mediumtext NOT NULL");
				}
			}
			foreach (StartScreen item in db.DbMapping.Load<StartScreen>())
			{
				_startscreens.Add(item);
			}
		}

		public void LoadTableTransaction(DatabaseBase db)
		{
			db.DbMapping.CreateTableIfNotExists<TableTransactions>();
			db.AlterColumnType("table_transactions", "transaction_number", "varchar(64)");
			_tableTransactions.Clear();
			foreach (TableTransactions item in db.DbMapping.Load<TableTransactions>())
			{
				_tableTransactions.Add(new TableTransactions
				{
					Id = item.Id,
					TableId = item.TableId,
					TransactionNumber = item.TransactionNumber
				});
			}
		}

		public IHistoryParameterValueFreeSelection GetFreeSelectionHistoryValue(int userId, int parameterId, int cloneId = 0)
		{
			IHistoryParameterValueFreeSelection[] histories = _userHistoryFreeSelectionIssueParameterCollection.Values.Where((IHistoryParameterValueFreeSelection h) => h.UserId == userId && h.ParameterId == parameterId).ToArray();
			if (histories.Count() > 0)
			{
				try
				{
					return histories[cloneId];
				}
				catch (Exception)
				{
				}
			}
			return null;
		}

		public IEnumerable<IHistoryParameterValueFreeSelection> GetFreeSelectionHistoryValuesByIssueId(int userId, int issueId)
		{
			return _userHistoryFreeSelectionIssueParameterCollection.Values.Where((IHistoryParameterValueFreeSelection h) => h.UserId == userId && h.IssueId == issueId);
		}

		public List<IHistoryParameterValueFreeSelection> GetFreeSelectionHistoryValuesByParameterId(int userId, int parameterId)
		{
			return _userHistoryFreeSelectionIssueParameterCollection.Values.Where((IHistoryParameterValueFreeSelection h) => h.UserId == userId && h.ParameterId == parameterId).ToList();
		}

		public void Dispose()
		{
			if (_cmanager != null)
			{
				_cmanager.Dispose();
			}
		}

		public void FreeSelectionLoadParameter(int userId, int issueId)
		{
			using DatabaseBase db = _cmanager.GetConnection();
			db.DbMapping.CreateTableIfNotExists<HistoryParameterValueFreeSelection>();
			List<HistoryParameterValueFreeSelection> list = db.DbMapping.Load<HistoryParameterValueFreeSelection>();
			IIssue issue = _issues[issueId];
			if (issue != null && list.Count != 0)
			{
				(issue.Parameters as ParameterCollection).Clear();
			}
			foreach (HistoryParameterValueFreeSelection historyParameterFreeselectionValue in list)
			{
				if (historyParameterFreeselectionValue.UserId == userId && historyParameterFreeselectionValue.IssueId == issueId && issue != null)
				{
					(issue.Parameters as ParameterCollection).Add(historyParameterFreeselectionValue as IParameter);
					_parameters.Add(historyParameterFreeselectionValue.Id, historyParameterFreeselectionValue as IParameter);
				}
			}
		}

		public void Connect(string connectionString, int maxConnections)
		{
			DbConfig cfg = new DbConfig
			{
				ConnectionString = connectionString
			};
			using (DatabaseBase db = ConnectionManager.CreateConnection(new DbConfig
			{
				ConnectionString = connectionString,
				DbName = null
			}))
			{
				db.Open();
				db.CreateDatabaseIfNotExists(cfg.DbName);
			}
			_connectionString = connectionString;
			_cmanager = new ConnectionManager(connectionString, maxConnections);
			_cmanager.PropertyChanged += cmanager_PropertyChanged;
			if (!_cmanager.IsInitialized)
			{
				_cmanager.Init();
			}
			DB = _cmanager.GetConnection();
		}

		public IOptimizationCollection GetOptimizations(IRole role, bool grantOnly = false)
		{
			if (role.IsSuper)
			{
				return Optimizations;
			}
			OptimizationCollection result = new OptimizationCollection();
			OptimizationRules rules = new OptimizationRules();
			if (grantOnly && !role.CanGrant)
			{
				return result;
			}
			foreach (Tuple<IOptimization, RightType> opt in _roleOptimizationRights.GetObjects(role))
			{
				if (opt.Item2 > RightType.None)
				{
					rules.AddOptimizationRule(opt.Item1);
				}
				if (opt.Item2 == RightType.None)
				{
					rules.RemoveOptimizationRule(opt.Item1);
				}
			}
			foreach (int id in rules.Ids)
			{
				result.Add(_optimizations[id]);
			}
			return result;
		}

		public IOptimizationCollection GetOptimizations(IUser user, bool grantOnly = false)
		{
			if (user != null && user.IsSuper)
			{
				return Optimizations;
			}
			OptimizationCollection result = new OptimizationCollection();
			OptimizationRules rules = new OptimizationRules();
			foreach (IRole role in user.Roles)
			{
				rules.AddOptimizationRules(GetOptimizations(role, grantOnly));
			}
			if (!grantOnly || user.CanGrant)
			{
				foreach (Tuple<IOptimization, RightType> opt in _userOptimizationRights.GetObjects(user))
				{
					if (opt.Item2 > RightType.None)
					{
						rules.AddOptimizationRule(opt.Item1);
					}
					else if (opt.Item2 == RightType.None)
					{
						rules.RemoveOptimizationRule(opt.Item1);
					}
				}
			}
			foreach (int id in rules.Ids)
			{
				result.Add(_optimizations[id]);
			}
			return result;
		}

		public IOptimizationCollection GetOptimizationSubTrees(IRole role, bool grantOnly = false)
		{
			if (role.IsSuper)
			{
				return Optimizations;
			}
			OptimizationCollection result = new OptimizationCollection();
			if (grantOnly && !role.CanGrant)
			{
				return result;
			}
			OptimizationRules rules = new OptimizationRules();
			foreach (Tuple<IOptimization, RightType> opt2 in _roleOptimizationRights.GetObjects(role))
			{
				rules.AddChildrenRule(opt2.Item1);
			}
			foreach (int id in rules.Ids)
			{
				IOptimization opt = _optimizations[id];
				if (opt != null)
				{
					result.Add(opt);
				}
			}
			return result;
		}

		public IOptimizationCollection GetOptimizationSubTrees(IUser user, bool grantOnly = false)
		{
			if (user.IsSuper)
			{
				return Optimizations;
			}
			OptimizationCollection result = new OptimizationCollection();
			OptimizationRules rules = new OptimizationRules();
			foreach (IRole role in user.Roles.Reverse())
			{
				if (grantOnly && !role.CanGrant && !user.CanGrant)
				{
					continue;
				}
				foreach (Tuple<IOptimization, RightType> opt3 in _roleOptimizationRights.GetObjects(role))
				{
					rules.AddChildrenRule(opt3.Item1);
				}
			}
			if (!grantOnly || user.CanGrant)
			{
				foreach (Tuple<IOptimization, RightType> opt2 in _userOptimizationRights.GetObjects(user))
				{
					if (RightType.None < opt2.Item2)
					{
						rules.AddChildrenRule(opt2.Item1);
					}
					if (opt2.Item2 == RightType.None)
					{
						rules.RemoveOptimizationRule(opt2.Item1);
					}
				}
			}
			foreach (int id in rules.Ids)
			{
				IOptimization opt = _optimizations[id];
				if (opt != null)
				{
					result.Add(opt);
				}
			}
			return result;
		}

		public IColumn CreateTemporaryColumn(IColumn col, string name, int ordinal, bool originalColumnIds = false)
		{
			global::SystemDb.Internal.Column obj = CreateTemporaryColumn(col, originalColumnIds) as global::SystemDb.Internal.Column;
			obj.IsVisible = true;
			obj.Name = name;
			obj.Ordinal = ordinal;
			return obj;
		}

		public IColumn CreateTemporaryColumn(IColumn column, bool originalColumnIds = false)
		{
			global::SystemDb.Internal.Column col = column.Clone() as global::SystemDb.Internal.Column;
			if (!originalColumnIds)
			{
				col.Id = columnId;
				columnId--;
			}
			return col;
		}

		public IColumnCollection CreateTemporaryColumnCollection()
		{
			return new ColumnCollection();
		}

		public void SetRowCount(ITableObject tobj, long rowCount)
		{
			(tobj as TableObject).RowCount = rowCount;
		}

		public ITableObjectCollection CreateTableObjectCollection()
		{
			return new TableObjectCollection();
		}

		public void UpdateProperty(IUser user, IProperty property)
		{
			UserPropertySettings settings = _userPropertySettings[user, property] as UserPropertySettings;
			if (settings != null)
			{
				settings.Property = property;
				settings.Value = property.Value;
				Properties[property.Id].Value = property.Value;
				using (DatabaseBase databaseBase = _cmanager.GetConnection())
				{
					databaseBase.DbMapping.Save(settings);
				}
				return;
			}
			if (property.Key == "defaultPDFfontSize")
			{
				int.TryParse(property.Value, out var tmp);
				if (tmp == 0)
				{
					property.Value = "12";
				}
				else if (tmp < 5)
				{
					property.Value = "5";
				}
				else if (tmp > 20)
				{
					property.Value = "20";
				}
			}
			settings = new UserPropertySettings
			{
				UserId = user.Id,
				User = user,
				PropertyId = property.Id,
				Property = property,
				Value = property.Value
			};
			using (DatabaseBase db = _cmanager.GetConnection())
			{
				db.DbMapping.Save(settings);
			}
			_userPropertySettings.Add(settings);
		}

		public IPropertiesCollection GetProperties(IUser user)
		{
			PropertiesCollection properties = new PropertiesCollection();
			if (Properties != null)
			{
				foreach (IProperty property in Properties)
				{
					Property prop = property.Clone() as Property;
					if (_userPropertySettings[user, prop] != null)
					{
						prop.Value = _userPropertySettings[user, prop].Value;
					}
					properties.Add(prop);
				}
				return properties;
			}
			return properties;
		}

		public ICorePropertiesCollection GetCoreProperties()
		{
			CorePropertiesCollection properties = new CorePropertiesCollection();
			if (CoreProperties != null)
			{
				foreach (ICoreProperty coreProperty in CoreProperties)
				{
					CoreProperty prop = coreProperty.Clone() as CoreProperty;
					properties.Add(prop);
				}
				return properties;
			}
			return properties;
		}

		public bool AddColumnToCollection(IFullColumnCollection columns, IColumnCollection cols)
		{
			lock (columns)
			{
				bool columnsChange = false;
				foreach (IColumn col in cols)
				{
					if (!(columns as FullColumnCollection).ContainsKey(col.Id))
					{
						(columns as FullColumnCollection).Add(col as global::SystemDb.Internal.Column);
						columnsChange = true;
					}
				}
				return columnsChange;
			}
		}

		public ICategoryCollection GetCategoryCollection()
		{
			return new CategoryCollection();
		}

		public IUser CreateUser(DatabaseBase connection, string userName, string name, SpecialRights flags, ExportRights exportAllowed, string password, string email, int id, bool isAdUser = false, string domain = null, bool firstLogin = false)
		{
			IUser user = new global::SystemDb.Internal.User
			{
				UserName = userName,
				Name = name,
				Password = password,
				Flags = flags,
				CanExport = exportAllowed,
				Id = id,
				Email = email,
				Domain = domain,
				IsADUser = isAdUser,
				DisplayRowCount = 35,
				FirstLogin = firstLogin,
				PasswordCreationDate = DateTime.Now
			};
			connection.DbMapping.Save(user);
			UsersByUserName.Add(user);
			Users.Add(user);
			LoadUsers(connection);
			return user;
		}

		public IUserSetting AddUserSetting(IUser user, string settingName, string value)
		{
			using (ConnectionManager.GetConnection())
			{
				try
				{
					IUserSetting setting = user.Settings.Get(user.Id, settingName);
					bool num = setting == null;
					if (num)
					{
						setting = new UserSetting
						{
							Name = settingName,
							UserId = user.Id,
							Value = value
						};
					}
					else
					{
						setting.Value = value;
					}
					if (!num)
					{
						user.Settings.Remove(setting);
					}
					user.Settings.Add(setting);
					return setting;
				}
				catch (Exception ex)
				{
					_log.Error("Error during AddUserSetting", ex);
				}
				return null;
			}
		}

		public bool RemoveUserSetting(IUser user, string settingName)
		{
			using DatabaseBase connection = ConnectionManager.GetConnection();
			try
			{
				IUserSetting setting = user.Settings.Get(user.Id, settingName);
				if (setting != null)
				{
					connection.DbMapping.Delete(setting);
					return true;
				}
			}
			catch (Exception ex)
			{
				_log.Error("Error during AddUserSetting", ex);
			}
			return false;
		}

		public IRole CreateRole(DatabaseBase connection, string name, SpecialRights flags, ExportRights export, RoleType type, int id)
		{
			IRole role = new global::SystemDb.Internal.Role
			{
				Name = name,
				Flags = flags,
				CanExport = export,
				Id = id
			};
			connection.DbMapping.Save(role);
			Roles.Add(role);
			LoadUsers(connection);
			return role;
		}

		public IRole ModifyRole(DatabaseBase connection, string name, SpecialRights flags, ExportRights export, RoleType type, int id)
		{
			IRole role = Roles.FirstOrDefault((IRole x) => x.Id == id);
			if (role != null)
			{
				IRole irole = Roles[id];
				Roles.Remove(irole);
				role = new global::SystemDb.Internal.Role
				{
					Name = name,
					Flags = flags,
					CanExport = export,
					Id = id
				};
				connection.DbMapping.Save(role);
				Roles.Add(role);
			}
			return role;
		}

		public IRoleSetting AddRoleSetting(IRole role, RoleSettingsType roleSettingType, string value)
		{
			using DatabaseBase connection = ConnectionManager.GetConnection();
			try
			{
				IRoleSetting setting = role.Settings.Get(roleSettingType, role.Id);
				bool num = setting == null;
				if (num)
				{
					setting = new RoleSetting
					{
						Name = roleSettingType.ToString(),
						RoleId = role.Id,
						Value = value
					};
				}
				else
				{
					setting.Value = value;
				}
				connection.DbMapping.Save(setting);
				if (!num)
				{
					role.Settings.Remove(setting);
				}
				role.Settings.Add(setting);
				return setting;
			}
			catch (Exception ex)
			{
				_log.Error("Error during AddroleSetting", ex);
			}
			return null;
		}

		public bool RemoveRoleSetting(IRole role, RoleSettingsType roleSettingType)
		{
			using DatabaseBase connection = ConnectionManager.GetConnection();
			try
			{
				IRoleSetting setting = role.Settings.Get(roleSettingType, role.Id);
				if (setting != null)
				{
					connection.DbMapping.Delete(setting);
					return true;
				}
			}
			catch (Exception ex)
			{
				_log.Error("Error during RemoveRoleSetting", ex);
			}
			return false;
		}

		public void EditIssue(DatabaseBase connection, IIssue issue, IEnumerable<ILanguage> languages, Dictionary<string, List<string>> descriptions)
		{
			connection.Delete("table_texts", "ref_id=" + issue.Id);
			foreach (ILanguage j in languages)
			{
				issue.SetDescription(descriptions[j.CountryCode][0], j);
				Objects[issue.Id].SetDescription(descriptions[j.CountryCode][0], j);
				connection.DbMapping.Save(new TableObjectText
				{
					CountryCode = j.CountryCode,
					RefId = issue.Id,
					Text = descriptions[j.CountryCode][0]
				});
			}
			int ord = 0;
			foreach (IParameter p in issue.Parameters)
			{
				connection.Delete("parameter_texts", "ref_id=" + p.Id);
				foreach (ILanguage i in languages)
				{
					p.SetDescription(descriptions[i.CountryCode][ord + 1], i);
					connection.DbMapping.Save(new ParameterText
					{
						CountryCode = i.CountryCode,
						RefId = p.Id,
						Text = descriptions[i.CountryCode][ord + 1]
					});
				}
				ord++;
			}
		}

		public void SaveIssueExtensionsAndParameter(DatabaseBase connection, IEnumerable<ILanguage> languages, Dictionary<string, List<string>> descriptions, IIssue issue, List<IParameter> parameters, ITableObject tableObject = null)
		{
			SaveIssueExtension(connection, issue, parameters, tableObject, issue.RowNoFilter);
			SaveIssueParameters(connection, languages, descriptions, issue, parameters, tableObject);
		}

		public void SaveIssueParameters(DatabaseBase connection, IEnumerable<ILanguage> languages, Dictionary<string, List<string>> descriptions, IIssue issue, List<IParameter> parameters, ITableObject tableObject = null)
		{
			int ord = 0;
			foreach (Parameter p in parameters)
			{
				p.Ordinal = ord;
				SaveParameterTexts(connection, languages, descriptions, ord, p);
				if ((issue.Parameters as ParameterCollection)[p.Id] == null)
				{
					(issue.Parameters as ParameterCollection).Add(p);
				}
				ord++;
			}
		}

		public void SaveIssueExtension(DatabaseBase connection, IIssue issue, List<IParameter> realParameters, ITableObject tableObject, string rownoFilter)
		{
			IssueExtension ext = connection.DbMapping.Load<IssueExtension>("ref_id = " + issue.Id + " and obj_id = " + (tableObject?.Id ?? 0)).FirstOrDefault() ?? new IssueExtension();
			ext.Command = issue.Command;
			ext.Flag = issue.Flag;
			ext.RefId = issue.Id;
			ext.TableObjectId = tableObject?.Id ?? 0;
			ext.UseLanguageValue = issue.UseLanguageValue;
			ext.UseIndexValue = issue.UseIndexValue;
			ext.UseSortValue = issue.UseSortValue;
			ext.UseSplitValue = issue.UseSplitValue;
			ext.RowNoFilter = rownoFilter;
			ext.NeedGJahr = issue.NeedGJahr;
			ext.NeedBukrs = issue.NeedBukrs;
			connection.DbMapping.Save(ext);
		}

		public void AddUserRoleMapping(int userId, int roleId)
		{
			using DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			try
			{
				IUser iuser = Users[userId];
				IRole irole = Roles[roleId];
				if (iuser != null && irole != null && _userRoles[iuser, irole] == null)
				{
					UserRoleMapping i = new UserRoleMapping
					{
						UserId = userId,
						RoleId = roleId,
						Ordinal = iuser.Roles.Count
					};
					db.Open();
					db.DbMapping.Save(i);
					iuser.Roles.Add(irole);
					irole.Users.Add(iuser);
					_userRoles.Add(iuser, irole, i);
				}
			}
			catch
			{
			}
		}

		public void RemoveUserRoleMapping(int userId, int roleId)
		{
			using DatabaseBase db = ConnectionManager.GetConnection();
			try
			{
				IUser iuser = Users[userId];
				if (iuser == null)
				{
					return;
				}
				IUserRoleMapping toDelete = null;
				do
				{
					IRole irole = Roles[roleId];
					if (irole != null)
					{
						Tuple<IUser, IRole> key = Tuple.Create(iuser, irole);
						toDelete = _userRoles[key];
						if (toDelete != null)
						{
							Queue.AddDelete(toDelete);
							Queue.PerformChanges(db);
							iuser.Roles.Remove(irole);
							irole.Users.Remove(iuser);
							_userRoles.Remove(key);
						}
					}
					toDelete = null;
				}
				while (toDelete != null);
				int i = 0;
				foreach (IRole role in iuser.Roles)
				{
					IUserRoleMapping roleMap = _userRoles[iuser, role];
					if (roleMap.Ordinal != i)
					{
						roleMap.Ordinal = i;
						Queue.AddSave(roleMap);
					}
					i++;
				}
				Queue.PerformChanges(db);
			}
			catch
			{
			}
		}

		public void RemoveUser(int user)
		{
			global::SystemDb.Internal.User iuser = Users[user] as global::SystemDb.Internal.User;
			foreach (IRole irole in _userRoles[iuser])
			{
				irole.Users.Remove(iuser);
				IUserRoleMapping m = _userRoles[iuser, irole];
				_userRoles.Remove(Tuple.Create((IUser)iuser, irole));
				Queue.AddDelete(m.Clone());
			}
			foreach (Tuple<IUser, IOptimization> r5 in new List<Tuple<IUser, IOptimization>>(from Tuple<IUser, IOptimization, RightType> r in _userOptimizationRights
				where r.Item1 == iuser
				select Tuple.Create(r.Item1, r.Item2)))
			{
				OptimizationUserMapping l = _userOptimizationRights.Mapping(r5.Item1, r5.Item2);
				_userOptimizationRights.Remove(Tuple.Create(r5.Item1, r5.Item2));
				Queue.AddDelete(l.Clone());
			}
			foreach (Tuple<IUser, ICategory> r4 in new List<Tuple<IUser, ICategory>>(from Tuple<IUser, ICategory, RightType> r in _userCategoryRights
				where r.Item1 == iuser
				select Tuple.Create(r.Item1, r.Item2)))
			{
				CategoryUserMapping k = _userCategoryRights.Mapping(r4.Item1, r4.Item2);
				_userCategoryRights.Remove(Tuple.Create(r4.Item1, r4.Item2));
				Queue.AddDelete(k.Clone());
			}
			foreach (Tuple<IUser, ITableObject> r3 in new List<Tuple<IUser, ITableObject>>(from Tuple<IUser, ITableObject, RightType> r in _userTableObjectRights
				where r.Item1 == iuser
				select Tuple.Create(r.Item1, r.Item2)))
			{
				TableUserMapping j = _userTableObjectRights.Mapping(r3.Item1, r3.Item2);
				_userTableObjectRights.Remove(Tuple.Create(r3.Item1, r3.Item2));
				Queue.AddDelete(j.Clone());
			}
			foreach (Tuple<IUser, IColumn> r2 in new List<Tuple<IUser, IColumn>>(from Tuple<IUser, IColumn, RightType> r in _userColumnRights
				where r.Item1 == iuser
				select Tuple.Create(r.Item1, r.Item2)))
			{
				ColumnUserMapping i = _userColumnRights.Mapping(r2.Item1, r2.Item2);
				_userColumnRights.Remove(Tuple.Create(r2.Item1, r2.Item2));
				Queue.AddDelete(i.Clone());
			}
			foreach (Tuple<IUser, IUser> usr in new List<Tuple<IUser, IUser>>(from Tuple<IUser, IUser, RightType> r in _userLogRights
				where r.Item1 == iuser || r.Item2 == iuser
				select Tuple.Create(r.Item1, r.Item2)))
			{
				_userLogRights.Remove(Tuple.Create(usr.Item1, usr.Item2));
			}
			using (DatabaseBase databaseBase = _cmanager.GetConnection())
			{
				foreach (UserLogUserMapping uulog in databaseBase.DbMapping.Load<UserLogUserMapping>().FindAll((UserLogUserMapping p) => p.UserId == iuser.Id || p.UserLogId == iuser.Id))
				{
					Queue.AddDelete(uulog);
				}
			}
			foreach (UserColumnSettings s5 in _userColumnSettings[iuser])
			{
				_userColumnSettings.Remove(s5);
				Queue.AddDelete(s5.Clone());
			}
			foreach (UserPropertySettings s4 in _userPropertySettings[iuser])
			{
				_userPropertySettings.Remove(s4);
				Queue.AddDelete(s4.Clone());
			}
			foreach (UserColumnOrderSettings s3 in _userColumnOrderSettings[iuser])
			{
				_userColumnOrderSettings.Remove(s3);
				Queue.AddDelete(s3.Clone());
			}
			IUserControllerSettings userControllerSettings = _userControllerSettings[iuser];
			if (userControllerSettings != null)
			{
				_userControllerSettings.Remove(userControllerSettings as UserControllerSettings);
				Queue.AddDelete(userControllerSettings);
			}
			foreach (UserTableObjectSettings s2 in _userTableObjectSettings[iuser])
			{
				_userTableObjectSettings.Remove(s2);
				Queue.AddDelete(s2.Clone());
			}
			foreach (UserTableObjectOrderSettings s in _userTableObjectOrderSettings[iuser])
			{
				_userTableObjectOrderSettings.Remove(s);
				Queue.AddDelete(s.Clone());
			}
			IUserOptimizationSettings userOptimizationSettings = _userOptimizationSettings[iuser];
			if (userOptimizationSettings != null)
			{
				_userOptimizationSettings.Remove(userOptimizationSettings as UserOptimizationSettings);
				Queue.AddDelete(userOptimizationSettings);
			}
			foreach (UserTableColumnWidthsSettings userTableColumnWidthsSettings in _userTableColumnWidthSettings[iuser])
			{
				_userTableColumnWidthSettings.Remove(userTableColumnWidthsSettings);
				Queue.AddDelete(userTableColumnWidthsSettings.Clone());
			}
			foreach (UserTableTransactionIdSettings userTableTransactionIdSettings in _userTableTransactionIdSettings[iuser])
			{
				_userTableTransactionIdSettings.Remove(userTableTransactionIdSettings);
				Queue.AddDelete(userTableTransactionIdSettings.Clone());
			}
			foreach (UserUserLogSettings userUserLogSettingse in _userUserLogSettings[iuser])
			{
				_userUserLogSettings.Remove(userUserLogSettingse);
				Queue.AddDelete(userUserLogSettingse.Clone());
			}
			IUserFavoriteIssueSettings userfav = _userFavoriteIssueCollection[iuser];
			if (userfav != null)
			{
				_userFavoriteIssueCollection.Remove(userfav);
				Queue.AddDelete(userfav);
			}
			IUserLastIssueSettings lastIssue = _userLastIssueCollection[iuser];
			if (lastIssue != null)
			{
				_userLastIssueCollection.Remove(lastIssue);
				Queue.AddDelete(lastIssue);
			}
			using (DatabaseBase databaseBase2 = _cmanager.GetConnection())
			{
				foreach (HistoryParameterValue historyParameterValue in databaseBase2.DbMapping.Load<HistoryParameterValue>().FindAll((HistoryParameterValue p) => p.UserId == iuser.Id))
				{
					Queue.AddDelete(historyParameterValue);
				}
			}
			using (DatabaseBase databaseBase3 = _cmanager.GetConnection())
			{
				foreach (ParameterValueOrder parOder in databaseBase3.DbMapping.Load<ParameterValueOrder>().FindAll((ParameterValueOrder p) => p.UserId == iuser.Id))
				{
					Queue.AddDelete(parOder);
				}
			}
			using (DatabaseBase db = _cmanager.GetConnection())
			{
				foreach (string dbName in from o in Optimizations
					where o.Group.Type == OptimizationType.System
					select o.Value)
				{
					db.DropDatabaseIfExists(iuser.UserTable(dbName));
				}
			}
			iuser.Remove();
			Queue.AddDelete(iuser);
			PerformChanges();
		}

		public void RemoveRole(int role)
		{
			IRole irole = Roles[role];
			Roles.Remove(irole);
			foreach (IUser iuser in _userRoles[irole])
			{
				iuser.Roles.Remove(irole);
				IUserRoleMapping m2 = _userRoles[iuser, irole];
				IList<IRole> j = _userRoles[iuser];
				for (int i = m2.Ordinal + 1; i < j.Count; i++)
				{
					IUserRoleMapping m3 = _userRoles[iuser, j[i]];
					m3.Ordinal--;
					Queue.AddSave(m3.Clone());
				}
				_userRoles.Remove(Tuple.Create(iuser, irole));
				Queue.AddDelete(m2.Clone());
			}
			foreach (Tuple<IRole, IOptimization> r5 in new List<Tuple<IRole, IOptimization>>(from Tuple<IRole, IOptimization, RightType> r in _roleOptimizationRights
				where r.Item1 == irole && r.Item2 != null
				select Tuple.Create(r.Item1, r.Item2)))
			{
				OptimizationRoleMapping n = _roleOptimizationRights.Mapping(r5.Item1, r5.Item2);
				_roleOptimizationRights.Remove(Tuple.Create(r5.Item1, r5.Item2));
				Queue.AddDelete(n.Clone());
			}
			foreach (Tuple<IRole, ICategory> r4 in new List<Tuple<IRole, ICategory>>(from Tuple<IRole, ICategory, RightType> r in _roleCategoryRights
				where r.Item1 == irole
				select Tuple.Create(r.Item1, r.Item2)))
			{
				CategoryRoleMapping m = _roleCategoryRights.Mapping(r4.Item1, r4.Item2);
				_roleCategoryRights.Remove(Tuple.Create(r4.Item1, r4.Item2));
				Queue.AddDelete(m.Clone());
			}
			foreach (Tuple<IRole, ITableObject> r3 in new List<Tuple<IRole, ITableObject>>(from Tuple<IRole, ITableObject, RightType> r in _roleTableObjectRights
				where r.Item1 == irole
				select Tuple.Create(r.Item1, r.Item2)))
			{
				TableRoleMapping l = _roleTableObjectRights.Mapping(r3.Item1, r3.Item2);
				_roleTableObjectRights.Remove(Tuple.Create(r3.Item1, r3.Item2));
				Queue.AddDelete(l.Clone());
				if (r3.Item2.RoleBasedFilters != null && r3.Item2.RoleBasedFilters.Count > 0)
				{
					r3.Item2.RoleBasedFilters.Remove(irole.Id);
				}
			}
			foreach (Tuple<IRole, IColumn> r2 in new List<Tuple<IRole, IColumn>>(from Tuple<IRole, IColumn, RightType> r in _roleColumnRights
				where r.Item1 == irole
				select Tuple.Create(r.Item1, r.Item2)))
			{
				ColumnRoleMapping k = _roleColumnRights.Mapping(r2.Item1, r2.Item2);
				_roleColumnRights.Remove(Tuple.Create(r2.Item1, r2.Item2));
				Queue.AddDelete(k.Clone());
			}
			using (DatabaseBase db = ConnectionManager.GetConnection())
			{
				db.ExecuteNonQuery($"DELETE FROM {db.DbConfig.DbName}.`row_visibility` WHERE `role_id` = {irole.Id};");
			}
			Queue.AddDelete(irole);
			PerformChanges();
		}

		public void UpdateTableObjectText(ITableObject tableobject, ILanguage lang)
		{
			using DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			db.Open();
			db.DbMapping.Delete<TableObjectText>("ref_id=" + tableobject.Id + " AND country_code='" + lang.CountryCode + "'");
			db.DbMapping.Save(new TableObjectText
			{
				CountryCode = lang.CountryCode,
				RefId = tableobject.Id,
				Text = tableobject.Descriptions[lang]
			});
		}

		public void UpdateCategoriesText(ICategory cat, ILanguage lang)
		{
			Category category = cat as Category;
			using DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			db.Open();
			db.DbMapping.Delete<CategoryText>("ref_id=" + category.Id + " AND country_code='" + lang.CountryCode + "'");
			db.DbMapping.Save(new CategoryText
			{
				CountryCode = lang.CountryCode,
				RefId = category.Id,
				Text = category.Names[lang]
			});
		}

		public void UpdateParameterValueText(IParameterValue paramvalue, ILanguage language)
		{
			ParameterValue parvalue = paramvalue as ParameterValue;
			using DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			db.Open();
			db.DbMapping.Delete<ParameterValueSetText>("ref_id=" + parvalue.Id + " AND country_code='" + language.CountryCode + "'");
			db.DbMapping.Save(new ParameterValueSetText
			{
				CountryCode = language.CountryCode,
				RefId = parvalue.Id,
				Text = parvalue.Descriptions[language]
			});
		}

		public void UpdateColumnText(IColumn column, ILanguage language)
		{
			global::SystemDb.Internal.Column col = column as global::SystemDb.Internal.Column;
			using DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			db.Open();
			db.DbMapping.Delete<ColumnText>("ref_id=" + col.Id + " AND country_code='" + language.CountryCode + "'");
			db.DbMapping.Save(new ColumnText
			{
				CountryCode = language.CountryCode,
				RefId = col.Id,
				Text = col.Descriptions[language]
			});
		}

		public void UpdateOptimizationGroupText(IOptimizationGroup optgroup, ILanguage language)
		{
			OptimizationGroup optg = optgroup as OptimizationGroup;
			using DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			db.Open();
			db.DbMapping.Delete<OptimizationGroupText>("ref_id=" + optg.Id + " AND country_code='" + language.CountryCode + "'");
			db.DbMapping.Save(new OptimizationGroupText
			{
				CountryCode = language.CountryCode,
				RefId = optg.Id,
				Text = optg.Names[language]
			});
		}

		public void UpdateOptimizationText(IOptimization optim, ILanguage language)
		{
			Optimization opt = optim as Optimization;
			using DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			db.Open();
			db.DbMapping.Delete<OptimizationText>("ref_id=" + opt.Id + " AND country_code='" + language.CountryCode + "'");
			db.DbMapping.Save(new OptimizationText
			{
				CountryCode = language.CountryCode,
				RefId = opt.Id,
				Text = opt.Descriptions[language]
			});
		}

		public void UpdateParameterText(IParameter param, ILanguage language)
		{
			Parameter parameter = param as Parameter;
			using DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			db.Open();
			db.DbMapping.Delete<ParameterText>("ref_id=" + parameter.Id + " AND country_code='" + language.CountryCode + "'");
			db.DbMapping.Save(new ParameterText
			{
				CountryCode = language.CountryCode,
				RefId = parameter.Id,
				Text = parameter.Descriptions[language]
			});
		}

		public void UpdatePropertyText(IProperty property, ILanguage language)
		{
			Property pro = property as Property;
			using DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			db.Open();
			db.DbMapping.Delete<PropertyText>("ref_id=" + pro.Id + " AND country_code='" + language.CountryCode + "'");
			db.DbMapping.Save(new PropertyText
			{
				Name = pro.Names[language],
				CountryCode = language.CountryCode,
				RefId = pro.Id,
				Text = pro.Descriptions[language]
			});
		}

		public void UpdateSchemeText(IScheme scheme, ILanguage language)
		{
			Scheme pro = scheme as Scheme;
			using DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			db.Open();
			db.DbMapping.Delete<SchemeText>("ref_id=" + pro.Id + " AND country_code='" + language.CountryCode + "'");
			db.DbMapping.Save(new SchemeText
			{
				CountryCode = language.CountryCode,
				RefId = pro.Id,
				Text = pro.Descriptions[language]
			});
		}

		public void UpdateParameterValue(IParameterValue iParameterValue, string p)
		{
			ParameterValue parametervalue = iParameterValue as ParameterValue;
			parametervalue.Value = p;
			using DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			db.Open();
			db.DbMapping.Save(parametervalue);
		}

		public void CreateParameterValue(IParameter parentparam, string p, Dictionary<ILanguage, string> texts)
		{
			using DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			db.Open();
			List<ParameterCollectionMapping> list = db.DbMapping.Load<ParameterCollectionMapping>();
			int? collection_id = null;
			int maxid = 0;
			foreach (ParameterCollectionMapping parameterCollectionMapping in list)
			{
				if (parameterCollectionMapping.ParameterId == parentparam.Id)
				{
					collection_id = parameterCollectionMapping.CollectionId;
					break;
				}
				if (parameterCollectionMapping.CollectionId > maxid)
				{
					maxid = parameterCollectionMapping.CollectionId;
				}
			}
			if (!collection_id.HasValue)
			{
				ParameterCollectionMapping pcm = new ParameterCollectionMapping();
				maxid = (pcm.CollectionId = maxid + 1);
				pcm.ParameterId = parentparam.Id;
				db.DbMapping.Save(pcm);
			}
			if (!collection_id.HasValue)
			{
				collection_id = maxid++;
			}
			ParameterValue pv = new ParameterValue();
			pv.CollectionId = collection_id.Value;
			pv.Value = p;
			db.DbMapping.Save(pv);
			foreach (KeyValuePair<ILanguage, string> a in texts)
			{
				ParameterValueSetText i = new ParameterValueSetText();
				i.RefId = pv.Id;
				i.CountryCode = a.Key.CountryCode;
				i.Text = a.Value;
				db.DbMapping.Save(i);
			}
		}

		public void DeleteColumns(List<int> columnlisttoremove)
		{
			foreach (int id in columnlisttoremove)
			{
				_columns.Remove(id);
			}
			using DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			db.Open();
			foreach (string infilter in CreateInFilter(columnlisttoremove))
			{
				db.DbMapping.Delete<global::SystemDb.Internal.Column>("id " + infilter);
			}
		}

		public void DeleteColumnTexts(List<int> columntextlisttoremove)
		{
			using DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			db.Open();
			foreach (string infilter in CreateInFilter(columntextlisttoremove))
			{
				db.DbMapping.Delete<ColumnText>("ref_id " + infilter);
			}
		}

		public void DeleteOrderArea(List<int> tablecollection)
		{
			using DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			db.Open();
			foreach (string infilter in CreateInFilter(tablecollection))
			{
				db.DbMapping.Delete<global::SystemDb.Internal.OrderArea>("table_id " + infilter);
			}
		}

		public void DeleteTableObjects(List<ITableObject> tableobjectlisttoremove)
		{
			List<int> tableidcoll = new List<int>();
			foreach (ITableObject table in tableobjectlisttoremove)
			{
				Objects.Remove(table as TableObject);
				tableidcoll.Add(table.Id);
			}
			using DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			db.Open();
			foreach (string infilter in CreateInFilter(tableidcoll))
			{
				db.DbMapping.Delete<TableObject>("id " + infilter);
			}
		}

		public void DeleteTableTexts(List<int> tabletextremove)
		{
			using DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			db.Open();
			foreach (string infilter in CreateInFilter(tabletextremove))
			{
				db.DbMapping.Delete<TableObjectText>("ref_id " + infilter);
			}
		}

		public void UpdateColumn(IUser user, int columnId, bool visible, bool save = true)
		{
			IColumn column = _columns[columnId];
			UserColumnSettings settings = _userColumnSettings[user, column] as UserColumnSettings;
			if (settings != null)
			{
				if (save)
				{
					settings.IsVisible = visible;
				}
			}
			else
			{
				settings = new UserColumnSettings
				{
					UserId = user.Id,
					User = user,
					ColumnId = column.Id,
					Column = column,
					IsVisible = visible
				};
				_userColumnSettings.Add(settings);
			}
			if (save)
			{
				using DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig);
				db.Open();
				db.DbMapping.Save(settings);
			}
		}

		public void UpdateTableObject(IUser user, ITableObject tobj)
		{
			UserTableObjectSettings settings = _userTableObjectSettings[user, tobj] as UserTableObjectSettings;
			if (tobj.IsVisible == Objects[tobj.Id].IsVisible && settings != null)
			{
				_userTableObjectSettings.Remove(settings);
				Queue.AddDelete(settings.Clone());
				PerformChanges();
			}
			else if (tobj.IsVisible != Objects[tobj.Id].IsVisible && settings == null)
			{
				settings = new UserTableObjectSettings
				{
					UserId = user.Id,
					User = user,
					TableId = tobj.Id,
					TableObject = tobj,
					IsVisible = tobj.IsVisible
				};
				using (DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig))
				{
					db.Open();
					db.DbMapping.Save(settings);
				}
				_userTableObjectSettings.Add(settings);
			}
		}

		public void UpdateColumnOrder(IUser user, ITableObject tobj, string columnOrder)
		{
			UserColumnOrderSettings settings = _userColumnOrderSettings[user, tobj] as UserColumnOrderSettings;
			if (settings == null)
			{
				settings = new UserColumnOrderSettings
				{
					UserId = user.Id,
					User = user,
					TableId = tobj.Id,
					TableObject = tobj,
					ColumnOrder = columnOrder
				};
			}
			else
			{
				settings.ColumnOrder = columnOrder;
			}
			using (DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig))
			{
				db.Open();
				db.DbMapping.Save(settings);
			}
			_userColumnOrderSettings.Add(settings);
		}

		public void UpdateTableObjectOrder(IUser user, TableType type, ITableObject tobj, bool remove = false)
		{
			string tableIdStr = tobj.Id.ToString();
			UserTableObjectOrderSettings settings = _userTableObjectOrderSettings[user, type] as UserTableObjectOrderSettings;
			if (settings == null)
			{
				if (!remove)
				{
					settings = new UserTableObjectOrderSettings
					{
						UserId = user.Id,
						User = user,
						Type = type,
						TableObjectOrder = tableIdStr
					};
					_userTableObjectOrderSettings.Add(settings);
				}
			}
			else
			{
				List<string> actualOrder = settings.TableObjectOrder.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
				actualOrder.Remove(tableIdStr);
				if (!remove)
				{
					actualOrder.Insert(0, tableIdStr);
				}
				settings.TableObjectOrder = string.Join(",", actualOrder);
			}
			if (settings != null)
			{
				using DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig);
				db.Open();
				db.DbMapping.Save(settings);
			}
		}

		public void UpdateTableObjectArchived(ITableObject tableObject, bool archived)
		{
			ITableObject settings = Objects[tableObject.Id];
			settings.IsArchived = archived;
			using (DatabaseBase db = _cmanager.GetConnection())
			{
				db.DbMapping.Save(settings);
			}
			tableObject.IsArchived = archived;
		}

		public void UpdateTableObjectCreateStructure(ITableObject tableObject, string createStructure)
		{
			TableArchiveInformation settings = _archiveInformation[tableObject.Id] as TableArchiveInformation;
			if (settings == null)
			{
				settings = new TableArchiveInformation
				{
					TableId = tableObject.Id,
					CreateStatement = createStructure
				};
			}
			else
			{
				settings.CreateStatement = createStructure;
			}
			using (DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig))
			{
				db.Open();
				db.DbMapping.Save(settings);
			}
			_archiveInformation.Add(settings);
		}

		public string GetCreationString(ITableObject tableObject)
		{
			string createStatement = _archiveInformation[tableObject.Id].CreateStatement;
			if (string.IsNullOrWhiteSpace(createStatement))
			{
				throw new NullReferenceException($"There is no information on creating this tableobject : {tableObject.Name}");
			}
			return createStatement;
		}

		public string GetUserController(IUser user)
		{
			if (_userControllerSettings[user] == null)
			{
				return string.Empty;
			}
			return _userControllerSettings[user].Controller;
		}

		public void UpdateUserController(IUser user, string controller)
		{
			UserControllerSettings settings = _userControllerSettings[user] as UserControllerSettings;
			if (settings == null)
			{
				settings = new UserControllerSettings
				{
					UserId = user.Id,
					User = user,
					Controller = controller
				};
			}
			else
			{
				settings.Controller = controller;
			}
			_userControllerSettings.Add(settings);
		}

		public IOptimization GetUserOptimization(IUser user)
		{
			IOptimization ipot = null;
			if (_userOptimizationSettings[user] == null)
			{
				for (int j = 0; j < _userOptimizationSettings.Count(); j++)
				{
					UserOptimizationSettings iUser2 = _userOptimizationSettings.GetElementAtSettings(j);
					if (iUser2.Optimization != null && iUser2.Optimization.Value != null && !iUser2.Optimization.Value.ToLower().EndsWith("_dynamic"))
					{
						return iUser2.Optimization;
					}
				}
			}
			ipot = ((_userOptimizationSettings[user] != null) ? _userOptimizationSettings[user].Optimization : null);
			if (ipot != null && ipot.Value != null && ipot.Value.ToLower().EndsWith("_dynamic"))
			{
				for (int i = 0; i < _userOptimizationSettings.Count(); i++)
				{
					UserOptimizationSettings iUser = _userOptimizationSettings.GetElementAtSettings(i);
					if (!iUser.Optimization.Value.ToLower().EndsWith("_dynamic"))
					{
						return iUser.Optimization;
					}
				}
			}
			return ipot;
		}

		public void DeleteTableText(int tableId)
		{
			using DatabaseBase DB = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			if (!DB.IsOpen)
			{
				DB.Open();
			}
			DbColumnValues c3 = new DbColumnValues();
			c3["ref_id"] = tableId;
			DB.Delete("table_texts", c3);
		}

		public void DeleteTableOriginalName(int tableId)
		{
			using DatabaseBase DB = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			if (!DB.IsOpen)
			{
				DB.Open();
			}
			DbColumnValues column2Remove = new DbColumnValues();
			column2Remove["id"] = tableId;
			DB.Delete("table_original_names", column2Remove);
		}

		public void DeleteTableSchemes(int tableId)
		{
			using DatabaseBase DB = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			if (!DB.IsOpen)
			{
				DB.Open();
			}
			DbColumnValues column2Remove = new DbColumnValues();
			column2Remove["table_id"] = tableId;
			DB.Delete("table_schemes", column2Remove);
		}

		public void DeleteUserTableSettings(int tableId)
		{
			List<IUserTableObjectSettings> ucs = new List<IUserTableObjectSettings>();
			foreach (IUserTableObjectSettings userTableSetting in _userTableObjectSettings)
			{
				if (userTableSetting.TableObject.Id == tableId)
				{
					ucs.Add(userTableSetting);
				}
			}
			foreach (IUserTableObjectSettings userTableSettingse in ucs)
			{
				_userTableObjectSettings.Remove(userTableSettingse);
			}
			using DatabaseBase DB = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			if (!DB.IsOpen)
			{
				DB.Open();
			}
			foreach (IUserTableObjectSettings userTableObjectSettings in ucs)
			{
				DB.DbMapping.Delete(userTableObjectSettings);
			}
		}

		public void DeleteTableArchiveInfo(int tableId)
		{
			List<ITableArchiveInformation> archivelist = new List<ITableArchiveInformation>();
			foreach (ITableArchiveInformation tableArchiveInformation2 in _archiveInformation)
			{
				if (tableArchiveInformation2.TableId == tableId)
				{
					archivelist.Add(tableArchiveInformation2);
				}
			}
			foreach (ITableArchiveInformation tableArchiveInformation in archivelist)
			{
				_archiveInformation.Remove(tableArchiveInformation);
			}
			using DatabaseBase DB = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			if (!DB.IsOpen)
			{
				DB.Open();
			}
			foreach (ITableArchiveInformation item in archivelist)
			{
				TableArchiveInformation v = item as TableArchiveInformation;
				DB.DbMapping.Delete(v);
			}
		}

		public void DeleteTableUsers(int tableId)
		{
			List<Tuple<IUser, ITableObject>> tableuserlist = new List<Tuple<IUser, ITableObject>>();
			List<TableUserMapping> tumappinglist = new List<TableUserMapping>();
			foreach (Tuple<IUser, ITableObject, RightType> d in _userTableObjectRights)
			{
				if (d.Item2.Id == tableId)
				{
					tableuserlist.Add(Tuple.Create(d.Item1, d.Item2));
					tumappinglist.Add(_userTableObjectRights.Mapping(d.Item1, d.Item2));
				}
			}
			foreach (Tuple<IUser, ITableObject> t in tableuserlist)
			{
				_userTableObjectRights.Remove(t);
			}
			using DatabaseBase DB = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			if (!DB.IsOpen)
			{
				DB.Open();
			}
			foreach (TableUserMapping v in tumappinglist)
			{
				DB.DbMapping.Delete(v);
			}
		}

		public void DeleteTableRoles(int tableId)
		{
			List<Tuple<IRole, ITableObject>> tablerolelist = new List<Tuple<IRole, ITableObject>>();
			List<TableRoleMapping> trmappinglist = new List<TableRoleMapping>();
			foreach (Tuple<IRole, ITableObject, RightType> d in _roleTableObjectRights)
			{
				if (d.Item2.Id == tableId)
				{
					tablerolelist.Add(Tuple.Create(d.Item1, d.Item2));
					trmappinglist.Add(_roleTableObjectRights.Mapping(d.Item1, d.Item2));
				}
			}
			foreach (Tuple<IRole, ITableObject> t in tablerolelist)
			{
				_roleTableObjectRights.Remove(t);
			}
			using DatabaseBase DB = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			if (!DB.IsOpen)
			{
				DB.Open();
			}
			foreach (TableRoleMapping v in trmappinglist)
			{
				DB.DbMapping.Delete(v);
			}
		}

		public void DeleteIssues(int tableId)
		{
			if (!_issues.Contains(tableId))
			{
				return;
			}
			Issue i = _issues[tableId] as Issue;
			_issues.Remove(i);
			using DatabaseBase DB = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			if (!DB.IsOpen)
			{
				DB.Open();
			}
			DbColumnValues dv = new DbColumnValues();
			dv["ref_id"] = tableId;
			DB.Delete("issue_extensions", dv);
		}

		public void DeleteOrderArea(int tableId)
		{
			using DatabaseBase DB = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			if (!DB.IsOpen)
			{
				DB.Open();
			}
			DbColumnValues dv = new DbColumnValues();
			dv["table_id"] = tableId;
			DB.Delete("order_areas", dv);
		}

		public void DeleteColumn(int columnId2Remove)
		{
			global::SystemDb.Internal.Column columnId = _columns[columnId2Remove] as global::SystemDb.Internal.Column;
			DeleteMapping(columnId);
			_columns.Remove(columnId2Remove);
		}

		public void DeleteColumnOrder(int tableID)
		{
			List<IUserColumnOrderSettings> columnorderlist = new List<IUserColumnOrderSettings>();
			foreach (IUserColumnOrderSettings columnorder2 in _userColumnOrderSettings)
			{
				if (columnorder2.TableObject.Id == tableID)
				{
					columnorderlist.Add(columnorder2);
				}
			}
			foreach (IUserColumnOrderSettings columnorder in columnorderlist)
			{
				_userColumnOrderSettings.Remove(columnorder);
			}
			using DatabaseBase DB = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			if (!DB.IsOpen)
			{
				DB.Open();
			}
			foreach (IUserColumnOrderSettings item in columnorderlist)
			{
				UserColumnOrderSettings v = item as UserColumnOrderSettings;
				DB.DbMapping.Delete(v);
			}
		}

		public void DeleteTableOrder(int tableID)
		{
			List<IUserTableObjectOrderSettings> tableorderlist = new List<IUserTableObjectOrderSettings>();
			foreach (IUserTableObjectOrderSettings tableorder2 in _userTableObjectOrderSettings)
			{
				if (tableorder2.TableObjectOrder.Contains(tableID.ToString()))
				{
					tableorderlist.Add(tableorder2);
				}
			}
			foreach (IUserTableObjectOrderSettings tableorder in tableorderlist)
			{
				_userTableObjectOrderSettings.Remove(tableorder);
			}
			using DatabaseBase DB = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			if (!DB.IsOpen)
			{
				DB.Open();
			}
			foreach (IUserTableObjectOrderSettings item in tableorderlist)
			{
				UserTableObjectOrderSettings v = item as UserTableObjectOrderSettings;
				DB.DbMapping.Delete(v);
			}
		}

		public void DeleteUsersColumn(int columnId)
		{
			List<Tuple<IUser, IColumn>> rcl = new List<Tuple<IUser, IColumn>>();
			List<ColumnUserMapping> rcr = new List<ColumnUserMapping>();
			foreach (Tuple<IUser, IColumn, RightType> usercolumnrights in _userColumnRights)
			{
				if (usercolumnrights.Item2.Id == columnId)
				{
					rcl.Add(Tuple.Create(usercolumnrights.Item1, usercolumnrights.Item2));
					rcr.Add(_userColumnRights.Mapping(usercolumnrights.Item1, usercolumnrights.Item2));
				}
			}
			foreach (Tuple<IUser, IColumn> tuple in rcl)
			{
				_userColumnRights.Remove(tuple);
			}
			using DatabaseBase DB = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			if (!DB.IsOpen)
			{
				DB.Open();
			}
			foreach (ColumnUserMapping v in rcr)
			{
				DB.DbMapping.Delete(v);
			}
		}

		public void DeleteColumnTexts(int columnId)
		{
			using DatabaseBase DB = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			if (!DB.IsOpen)
			{
				DB.Open();
			}
			DbColumnValues column = new DbColumnValues();
			column["ref_id"] = columnId;
			DB.Delete("column_texts", column);
		}

		public void DeleteParameters(int parameterid)
		{
			using DatabaseBase DB = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			if (!DB.IsOpen)
			{
				DB.Open();
			}
			DbColumnValues column = new DbColumnValues();
			column["id"] = parameterid;
			DB.Delete("parameter", column);
		}

		public void DeleteParameterCollection(int parameterid)
		{
			using DatabaseBase DB = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			if (!DB.IsOpen)
			{
				DB.Open();
			}
			DbColumnValues column = new DbColumnValues();
			column["parameter_id"] = parameterid;
			DB.Delete("parameter_collections", column);
		}

		public void DeleteParameterValue(IParameterValue paramvalue)
		{
			ParameterValue paramv = paramvalue as ParameterValue;
			using (DatabaseBase DB = ConnectionManager.CreateConnection(_cmanager.DbConfig))
			{
				if (!DB.IsOpen)
				{
					DB.Open();
				}
				DbColumnValues column = new DbColumnValues();
				column["id"] = paramv.Id;
				DB.Delete("collections", column);
				DbColumnValues column2 = new DbColumnValues();
				column2["ref_id"] = paramv.Id;
				DB.Delete("collection_texts", column2);
			}
			paramv = null;
		}

		public void DeleteParameterText(int parameterid)
		{
			using DatabaseBase DB = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			if (!DB.IsOpen)
			{
				DB.Open();
			}
			DbColumnValues column = new DbColumnValues();
			column["ref_id"] = parameterid;
			DB.Delete("parameter_texts", column);
		}

		public void DeleteRoleColumn(int columnId)
		{
			List<Tuple<IRole, IColumn>> rcl = new List<Tuple<IRole, IColumn>>();
			List<ColumnRoleMapping> rcr = new List<ColumnRoleMapping>();
			foreach (Tuple<IRole, IColumn, RightType> roleColumnRights in _roleColumnRights)
			{
				if (roleColumnRights.Item2.Id == columnId)
				{
					rcl.Add(Tuple.Create(roleColumnRights.Item1, roleColumnRights.Item2));
					rcr.Add(_roleColumnRights.Mapping(roleColumnRights.Item1, roleColumnRights.Item2));
				}
			}
			foreach (Tuple<IRole, IColumn> tuple in rcl)
			{
				_roleColumnRights.Remove(tuple);
			}
			using DatabaseBase DB = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			if (!DB.IsOpen)
			{
				DB.Open();
			}
			foreach (ColumnRoleMapping v in rcr)
			{
				DB.DbMapping.Delete(v);
			}
		}

		public void DeleteUserColumnSettings(int columnId)
		{
			List<IUserColumnSettings> ucs = new List<IUserColumnSettings>();
			foreach (IUserColumnSettings userColumnSetting in _userColumnSettings)
			{
				if (userColumnSetting.Column.Id == columnId)
				{
					ucs.Add(userColumnSetting);
				}
			}
			foreach (IUserColumnSettings userColumnSettingse2 in ucs)
			{
				_userColumnSettings.Remove(userColumnSettingse2);
			}
			using DatabaseBase DB = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			if (!DB.IsOpen)
			{
				DB.Open();
			}
			foreach (IUserColumnSettings userColumnSettingse in ucs)
			{
				DB.DbMapping.Delete(userColumnSettingse);
			}
		}

		public void DeleteOptimizationUser(IOptimization opt, IUser user)
		{
			UserOptimizationSettings optimizationsetting = _userOptimizationSettings[user] as UserOptimizationSettings;
			if (optimizationsetting != null && opt.Id == optimizationsetting.OptimizationId)
			{
				DeleteMapping(optimizationsetting);
				_userOptimizationSettings.Remove(optimizationsetting);
			}
		}

		public void DeleteOptimization(IOptimization optimization)
		{
			Optimization opt = _optimizations[optimization.Id] as Optimization;
			if (opt != null)
			{
				DeleteMapping(opt);
				_optimizations.Remove(optimization.Id);
			}
		}

		public void DeleteOptimizationRole(IOptimization optimization, IRole role)
		{
			OptimizationRoleMapping v = _roleOptimizationRights.Mapping(role, optimization);
			using (DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig))
			{
				db.Open();
				db.DbMapping.Delete(v);
			}
			_roleOptimizationRights.Remove(Tuple.Create(role, optimization));
		}

		public void DeleteTableObjectRights(ITableObject tableObject, IRole role)
		{
			TableRoleMapping v = _roleTableObjectRights.Mapping(role, tableObject);
			using (DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig))
			{
				db.Open();
				db.DbMapping.Delete(v);
			}
			_roleTableObjectRights.Remove(Tuple.Create(role, tableObject));
		}

		public void DeleteRowVisibilityNewSettings(IRole role)
		{
			using DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			db.Open();
			foreach (RowVisibilityNew v4 in db.DbMapping.Load<RowVisibilityNew>("role_id = '" + role.Id + "'"))
			{
				db.DbMapping.Delete(v4);
			}
			try
			{
				foreach (IView v3 in Views)
				{
					if (v3 != null && v3.RoleBasedFiltersNew != null && v3.RoleBasedFiltersNew.ContainsKey(role.Id))
					{
						v3.RoleBasedFiltersNew.Remove(role.Id);
					}
				}
				foreach (ITable v2 in Tables)
				{
					if (v2 != null && v2.RoleBasedFiltersNew != null && v2.RoleBasedFiltersNew.ContainsKey(role.Id))
					{
						v2.RoleBasedFiltersNew.Remove(role.Id);
					}
				}
				foreach (IIssue v in Issues)
				{
					if (v != null && v.RoleBasedFiltersNew != null && v.RoleBasedFiltersNew.ContainsKey(role.Id))
					{
						v.RoleBasedFiltersNew.Remove(role.Id);
					}
				}
			}
			catch
			{
			}
		}

		public void DeleteUserOptimizationMapping(IOptimization opt, IUser user)
		{
			OptimizationUserMapping optusermapping = _userOptimizationRights.Mapping(user, opt);
			using (DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig))
			{
				db.Open();
				db.DbMapping.Delete(optusermapping);
			}
			_userOptimizationRights.Remove(Tuple.Create(user, opt));
		}

		public void DeleteTable(int tableId)
		{
			global::SystemDb.Internal.Table table = _tables[tableId] as global::SystemDb.Internal.Table;
			using (DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig))
			{
				db.Open();
				db.DbMapping.Delete(table);
			}
			_tables.Remove(table);
		}

		public void DeleteTableObject(int tableobjectId)
		{
			TableObject tableobject = Objects[tableobjectId] as TableObject;
			using (DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig))
			{
				db.Open();
				db.DbMapping.Delete(tableobject);
			}
			Objects.Remove(tableobject);
		}

		public void DeleteOptimizationText(IOptimization opt)
		{
			using (DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig))
			{
				db.Open();
				DbColumnValues val = new DbColumnValues();
				val["ref_id"] = opt.Id;
				db.Delete("optimization_texts", val);
			}
			_optimizationTexts.RemoveAll((OptimizationText opttext) => opttext.RefId == opt.Id);
		}

		public void RemoveOptimizationFromAllTables(IOptimization opt)
		{
			List<IOptimization> optcoll = new List<IOptimization>();
			CollectAllChildren(opt, ref optcoll);
			foreach (IOptimization optimization in optcoll)
			{
				DeleteOptimization(optimization);
				DeleteOptimizationText(optimization);
				foreach (IUser user in Users)
				{
					DeleteOptimizationUser(optimization, user);
					DeleteUserOptimizationMapping(optimization, user);
				}
				foreach (IRole role in Roles)
				{
					DeleteOptimizationRole(optimization, role);
				}
			}
		}

		public void RemoveOptimizationFromAllTables(int optid)
		{
			RemoveOptimizationFromAllTables(Optimizations[optid]);
		}

		public void UpdateUserOptimization(IUser user, IOptimization optimization, bool saveToDb = false)
		{
			UserOptimizationSettings settings = _userOptimizationSettings[user] as UserOptimizationSettings;
			if (settings == null)
			{
				settings = new UserOptimizationSettings
				{
					UserId = user.Id,
					User = user,
					Optimization = optimization,
					OptimizationId = optimization.Id
				};
			}
			else
			{
				settings.Optimization = optimization;
				settings.OptimizationId = optimization.Id;
			}
			if (saveToDb)
			{
				using DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig);
				db.Open();
				db.DbMapping.Save(settings);
			}
			_userOptimizationSettings.Add(settings);
		}

		public void PerformChanges()
		{
			using DatabaseBase db = _cmanager.GetConnection();
			Queue.PerformChanges(db);
		}

		public INote CreateNote(DatabaseBase db, IUser user, string title, string text, DateTime date)
		{
			INote note = new Note
			{
				Id = 0,
				User = user,
				Title = title,
				Text = text,
				Date = date
			};
			db.DbMapping.Save(note);
			_notes.Add(note);
			return note;
		}

		public INote UpdateNote(DatabaseBase db, int id, IUser user, string title, string text)
		{
			Note note = _notes[user].FirstOrDefault((INote n) => n.Id == id) as Note;
			note.Text = text;
			note.Title = title;
			db.DbMapping.Save(note);
			return note;
		}

		public void DeleteNote(DatabaseBase db, int id, IUser user)
		{
			Note note = _notes[user].FirstOrDefault((INote n) => n.Id == id) as Note;
			_notes[user].Remove(note);
			db.DbMapping.Delete(note);
		}

		public void CreateServerLogEntry(DatabaseBase db, DateTime dt)
		{
			db.DbMapping.CreateTableIfNotExists<ServerLog>();
			db.DbMapping.Save(new ServerLog
			{
				DateTime = dt
			});
		}

		public bool IsOriginalColumnOrder(ITableObject tobj, int originalId = 0)
		{
			int objId = ((originalId == 0) ? tobj.Id : originalId);
			ITableObject table = Objects[objId];
			IOrderedEnumerable<IColumn> newList = from c in tobj.Columns.ToList()
				orderby c.Ordinal
				select c;
			if (table == null)
			{
				table = tobj;
			}
			if (table is Issue)
			{
				Issue tableIssue = table as Issue;
				if (objId > 0 && tableIssue.OriginalId > 0)
				{
					objId = tableIssue.OriginalId;
				}
			}
			List<string> resultList = new List<string>();
			using (DatabaseBase conn = _cmanager.GetConnection())
			{
				string sqlQuerry = "SELECT vc.name,vc.ordinal FROM " + conn.DbConfig.DbName + ".COLUMNS vc WHERE vc.table_id='" + objId + "' order by vc.ordinal;";
				using IDataReader reader = conn.ExecuteReader(sqlQuerry);
				while (reader.Read())
				{
					resultList.Add(reader.GetString(0));
				}
			}
			for (int i = 0; i < resultList.Count(); i++)
			{
				string originalColumnName = resultList.ElementAt(i);
				string newListColumnName = string.Empty;
				if (newList.Count() > i)
				{
					newListColumnName = newList.ElementAt(i).Name;
					if (originalColumnName != newListColumnName)
					{
						return false;
					}
				}
			}
			return true;
		}

		public void ReserColumnOrderOriginalTable(ITableObject tobj, IUser user)
		{
			IUserColumnOrderSettings settings = _userColumnOrderSettings[user, tobj];
			if (settings != null)
			{
				_userColumnOrderSettings.Remove(settings);
				Queue.AddDelete(settings);
			}
			while (settings != null)
			{
				settings = _userColumnOrderSettings[user, tobj];
				_userColumnOrderSettings.Remove(settings);
				Queue.AddDelete(settings);
			}
		}

		public List<Tuple<int, string>> ResetColumnOrder(ITableObject tobj, IUser user, ITableObject originalTable, int originalId = 0)
		{
			int objId = ((originalId == 0) ? tobj.Id : originalId);
			ITableObject table = Objects[objId];
			if (table is Issue)
			{
				Issue tableIssue = table as Issue;
				if (tableIssue.OriginalId > 0)
				{
					objId = tableIssue.OriginalId;
				}
			}
			List<Tuple<int, string>> resultList = new List<Tuple<int, string>>();
			using (DatabaseBase databaseBase = _cmanager.GetConnection())
			{
				string sqlQuerry2 = "SELECT vc.name,vc.ordinal FROM " + databaseBase.DbConfig.DbName + ".COLUMNS vc WHERE vc.table_id=" + objId + " order by vc.ordinal, vc.id;";
				using IDataReader reader = databaseBase.ExecuteReader(sqlQuerry2);
				while (reader.Read())
				{
					resultList.Add(new Tuple<int, string>(reader.GetInt32(1), reader.GetString(0)));
				}
			}
			if (resultList.Count((Tuple<int, string> r) => r.Item1 == 0) > 1)
			{
				for (int j = 0; j < resultList.Count; j++)
				{
					string tempColumnName = resultList[j].Item2;
					resultList[j] = new Tuple<int, string>(j, tempColumnName);
				}
			}
			List<IColumn> tobjColumns = tobj.Columns.ToList();
			int index = 0;
			for (int i = 0; i < tobjColumns.Count; i++)
			{
				index = 0;
				while (index < resultList.Count)
				{
					if (tobjColumns.ElementAt(i).Name == resultList[index].Item2)
					{
						tobjColumns.ElementAt(i).Ordinal = resultList[index].Item1;
					}
					if (!(tobjColumns.ElementAt(i).Name != resultList[index++].Item2))
					{
						break;
					}
				}
			}
			tobjColumns = (from x in tobjColumns
				orderby x.Ordinal, x.Id
				select x).ToList();
			using (DatabaseBase conn = _cmanager.GetConnection())
			{
				string sqlQuerry = "DELETE FROM " + conn.DbConfig.DbName + ".user_column_order_settings  WHERE table_id=" + objId + " AND user_id = " + user.Id + ";";
				conn.ExecuteNonQuery(sqlQuerry);
			}
			IUserColumnOrderSettings settings = _userColumnOrderSettings[user, tobj];
			if (settings != null)
			{
				_userColumnOrderSettings.Remove(settings);
				Queue.AddDelete(settings);
			}
			while (settings != null)
			{
				settings = _userColumnOrderSettings[user, tobj];
				_userColumnOrderSettings.Remove(settings);
				Queue.AddDelete(settings);
			}
			if (table != null)
			{
				settings = _userColumnOrderSettings[user, table];
				if (settings != null)
				{
					_userColumnOrderSettings.Remove(settings);
					Queue.AddDelete(settings);
				}
				while (settings != null)
				{
					settings = _userColumnOrderSettings[user, table];
					_userColumnOrderSettings.Remove(settings);
					Queue.AddDelete(settings);
				}
			}
			if (originalTable != null)
			{
				settings = _userColumnOrderSettings[user, originalTable];
				if (settings != null)
				{
					_userColumnOrderSettings.Remove(settings);
					Queue.AddDelete(settings);
				}
				while (settings != null)
				{
					settings = _userColumnOrderSettings[user, table];
					_userColumnOrderSettings.Remove(settings);
					Queue.AddDelete(settings);
				}
			}
			return resultList;
		}

		public Dictionary<string, string> GetTableNamesAndDescriptionsFromSystemDb(TableType type = TableType.All)
		{
			using DatabaseBase conn = _cmanager.GetConnection();
			Dictionary<string, string> tableNames = new Dictionary<string, string>();
			string sql = "SELECT name, comment FROM " + conn.Enquote("tableobject");
			if (type == TableType.Table)
			{
				sql = sql + " WHERE system_id = " + 1;
			}
			if (type == TableType.View || type == TableType.Issue)
			{
				sql = sql + " WHERE system_id = " + 2;
			}
			sql += " ORDER BY name";
			try
			{
				using IDataReader reader = conn.ExecuteReader(sql);
				while (reader.Read())
				{
					tableNames.Add(reader[0].ToString(), reader[1].ToString());
				}
			}
			catch
			{
			}
			return tableNames;
		}

		public void AddTableTexts(Dictionary<string, string> tableDescriptions, ITableObject tobj, DatabaseBase db)
		{
			foreach (KeyValuePair<string, string> langToDescr in tableDescriptions)
			{
				AddAndSaveLanguageIfNotExists(langToDescr.Key);
				db.DbMapping.Delete<TableObjectText>("ref_id = " + tobj.Id + " AND lower(country_code) = '" + langToDescr.Key.ToLower() + "'");
				db.DbMapping.Save(new TableObjectText
				{
					RefId = tobj.Id,
					CountryCode = langToDescr.Key,
					Text = langToDescr.Value
				});
			}
		}

		public void AddColumnTexts(Dictionary<string, Dictionary<string, string>> columnDictionary, DatabaseBase db, string columnName, int columnId)
		{
			if (!columnDictionary.ContainsKey(columnName))
			{
				return;
			}
			foreach (KeyValuePair<string, string> pair in columnDictionary[columnName])
			{
				AddAndSaveLanguageIfNotExists(pair.Key);
				db.DbMapping.Delete<ColumnText>("ref_id = " + columnId + " AND lower(country_code) = '" + pair.Key.ToLower() + "'");
				db.DbMapping.Save(new ColumnText
				{
					RefId = columnId,
					CountryCode = pair.Key,
					Text = pair.Value
				});
			}
		}

		public void UpdateOptimizationText(IOptimization optimization, Dictionary<string, string> langTodescription)
		{
			using DatabaseBase db = _cmanager.GetConnection();
			foreach (ILanguage i in _languages)
			{
				OptimizationText textObject = _optimizationTexts.FirstOrDefault((OptimizationText o) => optimization.Id == o.RefId && o.CountryCode == i.CountryCode) ?? new OptimizationText
				{
					CountryCode = i.CountryCode,
					RefId = optimization.Id
				};
				textObject.Text = langTodescription[i.CountryCode];
				db.DbMapping.Save(textObject);
			}
		}

		public List<string> GetFakeTables(string dbName)
		{
			using DatabaseBase conn = _cmanager.GetConnection();
			return (from fakeTable in conn.DbMapping.Load<FakeTable>(string.Format("{0}={1} AND {2}={3}", conn.Enquote("database"), conn.GetSqlString(dbName), conn.Enquote("type"), 6))
				select fakeTable.TableName).ToList();
		}

		public List<ITableObject> GetFakeTableObjects(string dbName)
		{
			using DatabaseBase conn = _cmanager.GetConnection();
			return ((IEnumerable<FakeTable>)conn.DbMapping.Load<FakeTable>(string.Format("{0}={1} AND {2}={3}", conn.Enquote("database"), conn.GetSqlString(dbName), conn.Enquote("type"), 6))).Select((Func<FakeTable, ITableObject>)((FakeTable fakeTable) => fakeTable)).ToList();
		}

		public void AddFakeTable(string dbName, string table)
		{
			using DatabaseBase conn = _cmanager.GetConnection();
			FakeTable fakeTable = new FakeTable
			{
				Database = dbName,
				TableName = table
			};
			conn.DbMapping.Save(fakeTable);
		}

		public void DeleteFakeTable(string dbName, string table)
		{
			using DatabaseBase conn = _cmanager.GetConnection();
			conn.ExecuteNonQuery(string.Format("DELETE FROM {0} WHERE LOWER({1})={2} AND LOWER({3})={4}", conn.DbMapping.GetTableName<FakeTable>(), conn.Enquote("database"), conn.GetSqlString(dbName), conn.Enquote("name"), conn.GetSqlString(table)));
		}

		public void DeleteRelations(string systemName)
		{
			using DatabaseBase conn = _cmanager.GetConnection();
			List<Relation> list = conn.DbMapping.Load<Relation>();
			List<int> ids = new List<int>();
			foreach (Relation relation in list)
			{
				IColumn source = Columns[relation.ParentId];
				IColumn target = Columns[relation.ChildId];
				if (source != null && target != null && !(source.Table.Database.ToLower() != systemName.ToLower()))
				{
					ids.Add(relation.Id);
					((TableObject.RelationCollection)source.Table.Relations).Clear();
				}
			}
			if (ids.Count > 0)
			{
				string deleteSql = string.Format("DELETE FROM relations WHERE {0} in ({1})", conn.Enquote("id"), string.Join(",", ids));
				conn.ExecuteNonQuery(deleteSql);
			}
		}

		public void AddRelations(List<IRelationDatabaseObject> relations)
		{
			using DatabaseBase conn = _cmanager.GetConnection();
			object idObj = conn.ExecuteScalar(string.Format("SELECT MAX({0}) FROM relations", conn.Enquote("relation_id")));
			int currentRelationId = ((idObj is DBNull) ? 1 : (Convert.ToInt32(idObj) + 1));
			Dictionary<int, int> relationIdMapping = new Dictionary<int, int>();
			foreach (IRelationDatabaseObject relation in relations)
			{
				if (relationIdMapping.ContainsKey(relation.RelationId))
				{
					relation.RelationId = relationIdMapping[relation.RelationId];
					continue;
				}
				relationIdMapping[relation.RelationId] = currentRelationId;
				relation.RelationId = currentRelationId;
				currentRelationId++;
			}
			conn.DbMapping.Save(typeof(Relation), relations);
		}

		public ITableObject CreateAndSaveTableObject(TableType type, string database, string name, long rowCount, List<DbColumnInfo> columnInfos, Dictionary<string, string> optimizeCriterias, Dictionary<string, string> tableDescriptions, Dictionary<string, Dictionary<string, string>> columnDictionary, string procedureCommand = "", string rowNoCommand = "", string objectTypeName = null)
		{
			return CreateAndSaveTableObject(type, database, name, rowCount, columnInfos, optimizeCriterias, tableDescriptions, columnDictionary, procedureCommand, rowNoCommand, null, objectTypeName);
		}

		public ITableObject CreateAndSaveTableObject(TableType type, string database, string name, long rowCount, List<DbColumnInfo> columnInfos, Dictionary<string, string> optimizeCriterias, Dictionary<string, string> tableDescriptions, Dictionary<string, Dictionary<string, string>> columnDictionary, string procedureCommand = "", string rowNoCommand = "", ITableObject parentObject = null, string objectTypeName = null, bool needGjahr = false, bool needBukrs = false)
		{
			object objType = null;
			if (objectTypeName != null)
			{
				objType = ObjectTypesCollection.FirstOrDefault((KeyValuePair<int, string> o) => o.Value.ToLower() == objectTypeName.ToLower());
			}
			int objectTypeId = ((objType != null) ? ((KeyValuePair<int, string>)objType).Key : 0);
			TableObject tobj = null;
			using (DatabaseBase db = ConnectionManager.GetConnection())
			{
				db.SetHighTimeout();
				switch (type)
				{
				case TableType.Issue:
					tobj = (Issues.FirstOrDefault((IIssue w) => w.Database.ToLower() == database.ToLower() && w.TableName.ToLower() == name.ToLower()) as Issue) ?? new Issue();
					(tobj as Issue).Command = procedureCommand;
					(tobj as Issue).RowNoFilter = rowNoCommand;
					(tobj as Issue).UseLanguageValue = optimizeCriterias.ContainsKey("Language");
					(tobj as Issue).UseIndexValue = optimizeCriterias.ContainsKey("Index");
					(tobj as Issue).UseSortValue = optimizeCriterias.ContainsKey("Sort");
					(tobj as Issue).UseSplitValue = optimizeCriterias.ContainsKey("Split");
					(tobj as Issue).NeedGJahr = needGjahr;
					(tobj as Issue).NeedBukrs = needBukrs;
					(tobj as Issue).IssueType = ((parentObject == null) ? IssueType.StoredProcedure : IssueType.Filter);
					break;
				case TableType.View:
					tobj = (Views.FirstOrDefault((IView w) => w.Database.ToLower() == database.ToLower() && w.TableName.ToLower() == name.ToLower()) as View) ?? new View();
					break;
				case TableType.Table:
					tobj = (Tables.FirstOrDefault((ITable w) => w.Database.ToLower() == database.ToLower() && w.TableName.ToLower() == name.ToLower()) as global::SystemDb.Internal.Table) ?? new global::SystemDb.Internal.Table();
					break;
				}
				tobj.ObjectTypeCode = objectTypeId;
				tobj.Database = database;
				tobj.TableName = name;
				tobj.RowCount = rowCount;
				tobj.IsVisible = true;
				tobj.UserDefined = false;
				tobj.Category = new Category
				{
					Id = 0
				};
				db.DbMapping.Save(tobj);
				int ord = 0;
				foreach (DbColumnInfo c in columnInfos)
				{
					if (!(c.Name.ToLower() == "_row_no_"))
					{
						global::SystemDb.Internal.Column column = (tobj.Columns.FirstOrDefault((IColumn w) => w.Name.ToLower() == c.Name.ToLower()) as global::SystemDb.Internal.Column) ?? new global::SystemDb.Internal.Column();
						column.MaxLength = c.MaxLength;
						column.Name = c.Name;
						column.Ordinal = ord;
						column.Table = tobj;
						column.IsVisible = true;
						column.IsEmpty = false;
						column.UserDefined = false;
						column.OptimizationType = OptimizationType.NotSet;
						switch (c.Type)
						{
						case DbColumnTypes.DbNumeric:
							column.DataType = SqlType.Decimal;
							break;
						case DbColumnTypes.DbInt:
						case DbColumnTypes.DbBigInt:
							column.DataType = SqlType.Integer;
							break;
						case DbColumnTypes.DbBool:
							column.DataType = SqlType.Boolean;
							break;
						case DbColumnTypes.DbDate:
							column.DataType = SqlType.Date;
							break;
						case DbColumnTypes.DbDateTime:
							column.DataType = SqlType.DateTime;
							break;
						case DbColumnTypes.DbText:
						case DbColumnTypes.DbLongText:
						case DbColumnTypes.DbBinary:
						case DbColumnTypes.DbUnknown:
							column.DataType = SqlType.String;
							break;
						case DbColumnTypes.DbTime:
							column.DataType = SqlType.Time;
							break;
						}
						db.DbMapping.Save(column);
						AddColumnTexts(columnDictionary, db, column.Name, column.Id);
						if (!tobj.Columns.Any((IColumn w) => w.Id == column.Id))
						{
							tobj.Columns.Add(column);
						}
						if (_columns[column.Id] == null)
						{
							_columns.Add(column);
						}
						ord++;
					}
				}
				if (type != TableType.Issue)
				{
					if (optimizeCriterias.ContainsKey("Index"))
					{
						(tobj.Columns[optimizeCriterias["Index"].ToLower()] as global::SystemDb.Internal.Column).OptimizationType = OptimizationType.IndexTable;
						tobj.IndexTableColumn = tobj.Columns[optimizeCriterias["Index"].ToLower()];
						db.DbMapping.Save(tobj.IndexTableColumn);
					}
					if (optimizeCriterias.ContainsKey("Split"))
					{
						(tobj.Columns[optimizeCriterias["Split"].ToLower()] as global::SystemDb.Internal.Column).OptimizationType = OptimizationType.SplitTable;
						tobj.SplitTableColumn = tobj.Columns[optimizeCriterias["Split"].ToLower()];
						db.DbMapping.Save(tobj.SplitTableColumn);
					}
					if (optimizeCriterias.ContainsKey("Sort"))
					{
						(tobj.Columns[optimizeCriterias["Sort"].ToLower()] as global::SystemDb.Internal.Column).OptimizationType = OptimizationType.SortColumn;
						tobj.SortColumn = tobj.Columns[optimizeCriterias["Sort"].ToLower()];
						db.DbMapping.Save(tobj.SortColumn);
					}
				}
				db.DbMapping.Save(tobj);
				CheckDatatypes(db, tobj, parentObject);
				AddTableTexts(tableDescriptions, tobj, db);
				List<ITableObject> objects = null;
				int startNumber = 0;
				switch (type)
				{
				case TableType.Issue:
					tobj.Ordinal = CheckOrdinals(TableType.Issue, db);
					startNumber = 1001;
					objects = Objects.Where((ITableObject obj) => obj.Type == TableType.Issue).ToList();
					if (!(Issues as IssueCollection).Any((IIssue w) => w.Id == (tobj as IIssue).Id))
					{
						(Issues as IssueCollection).Add(tobj as IIssue);
					}
					break;
				case TableType.View:
					tobj.Ordinal = CheckOrdinals(TableType.View, db);
					startNumber = 2001;
					objects = Objects.Where((ITableObject obj) => obj.Type == TableType.View).ToList();
					if (!(Views as ViewCollection).Any((IView w) => w.Id == (tobj as IView).Id))
					{
						(Views as ViewCollection).Add(tobj as IView);
					}
					break;
				case TableType.Table:
					tobj.Ordinal = CheckOrdinals(TableType.Table, db);
					startNumber = 10001;
					objects = Objects.Where((ITableObject obj) => obj.Type == TableType.Table).ToList();
					if (!(Tables as TableCollection).Any((ITable w) => w.Id == (tobj as ITable).Id))
					{
						(Tables as TableCollection).Add(tobj as ITable);
					}
					break;
				default:
					throw new InvalidOperationException("Unknown type");
				}
				tobj.TransactionNumber = startNumber.ToString();
				if (objects.Any((ITableObject i) => i.TransactionNumber == tobj.TransactionNumber))
				{
					string transactionsnr = startNumber.ToString();
					tobj.TransactionNumber = transactionsnr;
				}
				db.DbMapping.Save(tobj);
				SetObjectType(tobj, db);
			}
			if (!_objects.Any((ITableObject w) => w.Id == tobj.Id))
			{
				Objects.Add(tobj);
			}
			return tobj;
		}

		public void SaveObjectType(DatabaseBase conn, string objectTypeName, Dictionary<string, string> langDescr, bool overwriteDescription = false)
		{
			List<ObjectTypes> list = conn.DbMapping.Load<ObjectTypes>("value = '" + objectTypeName + "'");
			if (!list.Any())
			{
				conn.DbMapping.Save(new ObjectTypes
				{
					Value = objectTypeName
				});
				list = conn.DbMapping.Load<ObjectTypes>("value = '" + objectTypeName + "'");
			}
			foreach (ObjectTypes objectTypes in list)
			{
				List<ObjectTypeText> objTypeTexts = conn.DbMapping.Load<ObjectTypeText>(conn.Enquote("ref_id") + " = " + objectTypes.Id);
				foreach (Language language in _languages)
				{
					ObjectTypeText temp = objTypeTexts.FirstOrDefault((ObjectTypeText t) => t.CountryCode == language.CountryCode && t.RefId == objectTypes.Id);
					if (!(temp == null || overwriteDescription))
					{
						continue;
					}
					Tuple<string, string> text = (from l in langDescr
						where l.Key == language.CountryCode
						select new Tuple<string, string>(l.Key, l.Value)).FirstOrDefault();
					if (text != null && !string.IsNullOrWhiteSpace(text.Item2))
					{
						AddAndSaveLanguageIfNotExists(text.Item1);
						if (temp == null)
						{
							conn.DbMapping.Save(new ObjectTypeText
							{
								RefId = objectTypes.Id,
								CountryCode = text.Item1,
								Text = text.Item2
							});
						}
						else
						{
							conn.DbMapping.Save(new ObjectTypeText
							{
								Id = temp.Id,
								RefId = objectTypes.Id,
								CountryCode = text.Item1,
								Text = text.Item2
							});
						}
					}
				}
			}
		}

		public void DeleteIssue(DatabaseBase connection, IIssue issue, IUser user)
		{
			bool deleteIssueIsAllowed = RightObjectTree.DeleteTableObjectIsAllowed(issue, this);
			if (user.IsSuper || deleteIssueIsAllowed)
			{
				foreach (IRole r in Roles)
				{
					UpdateRight(r, UpdateRightType.TableObject, issue.Id, RightType.Inherit);
				}
				foreach (IUser u in Users)
				{
					UpdateRight(u, UpdateRightType.TableObject, issue.Id, RightType.Inherit);
					UserTableObjectSettings settings = _userTableObjectSettings[user, issue] as UserTableObjectSettings;
					if (settings != null)
					{
						_userTableObjectSettings.Remove(settings);
						connection.DbMapping.Delete(settings.Clone());
					}
				}
				foreach (IParameter param in issue.Parameters)
				{
					foreach (ParameterText text2 in connection.DbMapping.Load<ParameterText>().FindAll((ParameterText p) => p.RefId == param.Id))
					{
						connection.DbMapping.Delete(text2);
					}
					foreach (HistoryParameterValue historyParameterValue in connection.DbMapping.Load<HistoryParameterValue>().FindAll((HistoryParameterValue p) => p.UserId == user.Id && p.ParameterId == param.Id))
					{
						connection.DbMapping.Delete(historyParameterValue);
					}
					param.HistoryValues = new HistoryParameterValueCollection();
					connection.DbMapping.Delete(param);
				}
				IssueExtension issueExtension = connection.DbMapping.Load<IssueExtension>().FirstOrDefault((IssueExtension i) => i.RefId == issue.Id);
				if (issueExtension != null)
				{
					connection.DbMapping.Delete(issueExtension);
				}
				Objects.RemoveById(issue.Id);
				if (_issues.Contains(issue.Id))
				{
					_issues.RemoveById(issue.Id);
				}
				if (_userLastIssueCollection[user] != null)
				{
					_userLastIssueCollection.Remove(_userLastIssueCollection[user]);
					connection.DbMapping.Delete(_userLastIssueCollection[user]);
				}
				ICategory cat = _categories[0];
				if (cat != null && cat.TableObjects.Contains(issue.Id))
				{
					(cat.TableObjects as TableObjectCollection).Remove(cat.TableObjects[issue.Id] as TableObject);
				}
				foreach (TableObjectText text in connection.DbMapping.Load<TableObjectText>().FindAll((TableObjectText p) => p.RefId == issue.Id))
				{
					connection.DbMapping.Delete(text);
				}
				foreach (ITableObject j in from o in Objects
					where o.Type == TableType.Issue && o.Ordinal > issue.Ordinal
					orderby o.Ordinal
					select o)
				{
					j.Ordinal--;
					connection.DbMapping.Save(j);
				}
				connection.DbMapping.Delete(issue);
			}
			else
			{
				UpdateRight(user, UpdateRightType.TableObject, issue.Id, RightType.None);
			}
		}

		public ITableObject SaveView(DatabaseBase connection, ITableObject tobj, IEnumerable<ILanguage> languages, IUser user, string database, bool joinSave = false)
		{
			View view = new View
			{
				Category = tobj.Category,
				Database = database,
				DefaultScheme = tobj.DefaultScheme,
				IndexTableColumn = tobj.IndexTableColumn,
				Relations = tobj.Relations,
				SortColumn = tobj.SortColumn,
				SplitTableColumn = tobj.SplitTableColumn,
				Type = TableType.View,
				UserDefined = true,
				TableName = tobj.TableName,
				IsVisible = true
			};
			List<ITableObject> views = Objects.Where((ITableObject o) => o.Type == TableType.View).ToList();
			view.Ordinal = ((views.Count != 0) ? (views.Max((ITableObject v) => v.Ordinal) + 1) : 0);
			view.RowCount = (int)connection.CountTable(tobj.Database, tobj.TableName);
			foreach (ILanguage k in languages)
			{
				view.SetDescription(tobj.Descriptions[k], k);
			}
			foreach (IScheme s in tobj.Schemes)
			{
				(view.Schemes as SchemeCollection).Add(s as Scheme);
			}
			view.Users.Add(user as global::SystemDb.Internal.User);
			view.ObjectTypeCode = 0;
			RefreshTableObjectType(view);
			connection.DbMapping.Save(view);
			int transactionNumberMax = _tableTransactions.Max((TableTransactions x) => x.TransactionNumber.Contains("CUST_") ? int.Parse(x.TransactionNumber.Replace("CUST_", string.Empty)) : 0);
			TableTransactions tableTransaction = new TableTransactions();
			tableTransaction.TableId = view.Id;
			tableTransaction.TransactionNumber = "CUST_" + ++transactionNumberMax;
			connection.DbMapping.Save(tableTransaction);
			_tableTransactions.Add(tableTransaction);
			view.TransactionNumber = tableTransaction.TransactionNumber;
			foreach (IColumn c in tobj.Columns)
			{
				(c as global::SystemDb.Internal.Column).Id = 0;
				c.Table = view;
				connection.DbMapping.Save(c as global::SystemDb.Internal.Column);
				foreach (ILanguage j in languages)
				{
					connection.DbMapping.Save(new ColumnText
					{
						CountryCode = j.CountryCode,
						RefId = c.Id,
						Text = c.Descriptions[j]
					});
				}
				view.Columns.Add(c);
				_columns.Add(c);
			}
			foreach (ILanguage i in languages)
			{
				connection.DbMapping.Save(new TableObjectText
				{
					CountryCode = i.CountryCode,
					RefId = view.Id,
					Text = tobj.Descriptions[i]
				});
			}
			if (joinSave)
			{
				foreach (global::SystemDb.Internal.OrderArea oa in view.OrderAreas)
				{
					oa.Id = 0;
					oa.TableId = view.Id;
					connection.DbMapping.Save(oa);
					((OrderAreaCollection)view.OrderAreas).Add(oa);
				}
			}
			TableUserMapping t = new TableUserMapping
			{
				Right = RightType.Read,
				TableId = view.Id,
				UserId = user.Id
			};
			connection.DbMapping.Save(t);
			Objects.Add(view);
			_categories[0]?.TableObjects.Add(view);
			return view;
		}

		public void DeleteView(DatabaseBase connection, IView view, IUser user)
		{
			bool deleteViewIsAllowed = RightObjectTree.DeleteTableObjectIsAllowed(view, this);
			if (user.IsSuper || deleteViewIsAllowed)
			{
				TableTransactions transactionNumber = TableTransactions.FirstOrDefault((TableTransactions t) => t.TableId == view.Id);
				if (transactionNumber != null)
				{
					connection.DbMapping.Delete(transactionNumber);
					TableTransactions.Remove(transactionNumber);
				}
				foreach (IRole r3 in Roles)
				{
					foreach (IColumn c3 in view.Columns)
					{
						UpdateRight(r3, UpdateRightType.Column, c3.Id, RightType.Inherit);
					}
					UpdateRight(r3, UpdateRightType.TableObject, view.Id, RightType.Inherit);
				}
				foreach (IUser u in Users)
				{
					UserTableObjectSettings settings = _userTableObjectSettings[user, view] as UserTableObjectSettings;
					if (settings != null)
					{
						_userTableObjectSettings.Remove(settings);
						connection.DbMapping.Delete(settings.Clone());
					}
					UserTableTransactionIdSettings transactionSettings = _userTableTransactionIdSettings[view.Id, user.Id] as UserTableTransactionIdSettings;
					if (transactionSettings != null)
					{
						_userTableTransactionIdSettings.Remove(transactionSettings);
						connection.DbMapping.Delete(transactionSettings.Clone());
					}
					UserColumnOrderSettings columnOrderSettings = _userColumnOrderSettings[user, view] as UserColumnOrderSettings;
					if (columnOrderSettings != null)
					{
						_userColumnOrderSettings.Remove(columnOrderSettings);
						connection.DbMapping.Delete(columnOrderSettings.Clone());
					}
					UserTableColumnWidthsSettings tableColumnWidthSettings = _userTableColumnWidthSettings[user, view] as UserTableColumnWidthsSettings;
					if (tableColumnWidthSettings != null)
					{
						_userTableColumnWidthSettings.Remove(tableColumnWidthSettings);
						connection.DbMapping.Delete(tableColumnWidthSettings.Clone());
					}
					foreach (IColumn c2 in view.Columns)
					{
						UpdateRight(u, UpdateRightType.Column, c2.Id, RightType.Inherit);
						UserColumnSettings columnSettings = _userColumnSettings[user, c2] as UserColumnSettings;
						if (columnSettings != null)
						{
							_userColumnSettings.Remove(columnSettings);
							connection.DbMapping.Delete(columnSettings.Clone());
						}
					}
					UpdateRight(u, UpdateRightType.TableObject, view.Id, RightType.Inherit);
				}
				foreach (Tuple<IUser, ITableObject> r2 in new List<Tuple<IUser, ITableObject>>(from Tuple<IUser, ITableObject, RightType> r in _userTableObjectRights
					where r.Item2 != null && r.Item2.Id == view.Id
					select Tuple.Create(r.Item1, r.Item2)))
				{
					TableUserMapping j = _userTableObjectRights.Mapping(r2.Item1, r2.Item2);
					_userTableObjectRights.Remove(Tuple.Create(r2.Item1, r2.Item2));
					connection.DbMapping.Delete(j);
				}
				foreach (IColumn c in view.Columns)
				{
					_columns.Remove(c.Id);
					connection.DbMapping.Delete<ColumnText>("ref_id = " + c.Id);
					connection.DbMapping.Delete(c);
				}
				Objects.RemoveById(view.Id);
				ICategory cat = _categories[0];
				if (cat != null && cat.TableObjects.Contains(view.Id))
				{
					(cat.TableObjects as TableObjectCollection).Remove(cat.TableObjects[view.Id] as TableObject);
				}
				connection.DbMapping.Delete<TableObjectText>("ref_id = " + view.Id);
				foreach (ITableObject i in from o in Objects
					where o.Type == TableType.View && o.Ordinal > view.Ordinal
					orderby o.Ordinal
					select o)
				{
					i.Ordinal--;
					connection.DbMapping.Save(i);
				}
				if (view.UserDefined)
				{
					connection.ExecuteNonQuery($"DROP TABLE `temp`.`{view.TableName}`;");
				}
				connection.DbMapping.Delete(view);
			}
			else
			{
				UpdateRight(user, UpdateRightType.TableObject, view.Id, RightType.None);
			}
		}

		public int SaveIssue(DatabaseBase connection, ITableObject tobj, string command, string rownocommand, List<IParameter> realParameters, IEnumerable<ILanguage> languages, IUser user, Dictionary<string, List<string>> descriptions, out List<IParameter> issueParameters, int decimalSeparator, List<bool> freeselection = null)
		{
			Issue issue = new Issue
			{
				Category = tobj.Category,
				Command = command,
				Database = tobj.Database,
				DefaultScheme = tobj.DefaultScheme,
				FilterTableObject = tobj,
				Flag = 0,
				IndexTableColumn = tobj.IndexTableColumn,
				IssueType = IssueType.Filter,
				OrderAreas = tobj.OrderAreas,
				RoleAreas = tobj.RoleAreas,
				Relations = tobj.Relations,
				RowCount = -1L,
				SortColumn = tobj.SortColumn,
				SplitTableColumn = tobj.SplitTableColumn,
				Type = TableType.Issue,
				UseLanguageValue = (languages.Count() > 1),
				UseIndexValue = (tobj.IndexTableColumn != null),
				UserDefined = true,
				UseSortValue = (tobj.SortColumn != null),
				UseSplitValue = (tobj.SplitTableColumn != null),
				IsVisible = true,
				RowNoFilter = rownocommand,
				NeedGJahr = false,
				NeedBukrs = false,
				TableName = tobj.TableName + "_filter_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss"),
				ObjectTypeCode = tobj.ObjectTypeCode
			};
			foreach (IColumn c in tobj.Columns)
			{
				issue.Columns.Add(c);
			}
			foreach (ILanguage k in languages)
			{
				issue.SetDescription(descriptions[k.CountryCode][0], k);
			}
			foreach (IScheme s in tobj.Schemes)
			{
				(issue.Schemes as SchemeCollection).Add(s as Scheme);
			}
			foreach (KeyValuePair<string, string> item in tobj.ObjectTypes)
			{
				if (languages.Any((ILanguage l) => l.CountryCode == item.Key))
				{
					(issue.ObjectTypes as LocalizedTextCollection).Add(languages.First((ILanguage l) => l.CountryCode == item.Key), item.Value);
				}
			}
			issue.Users.Add(user as global::SystemDb.Internal.User);
			foreach (IUser u in Users)
			{
				if (u.Id != user.Id && u.IsSuper)
				{
					issue.Users.Add(u as global::SystemDb.Internal.User);
				}
			}
			IEnumerable<ITableObject> issues = Objects.Where((ITableObject o) => o.Type == TableType.Issue);
			issue.Ordinal = ((issues.Count() > 0) ? (issues.Max((ITableObject i) => i.Ordinal) + 1) : 0);
			issue.TransactionNumber = "2001";
			connection.DbMapping.Save(issue);
			foreach (ILanguage j in languages)
			{
				connection.DbMapping.Save(new TableObjectText
				{
					CountryCode = j.CountryCode,
					RefId = issue.Id,
					Text = descriptions[j.CountryCode][0]
				});
			}
			foreach (IParameter realParameter in realParameters)
			{
				realParameter.Issue = issue;
				realParameter.HistoryValues = new HistoryParameterValueCollection();
				realParameter.TypeModifier = decimalSeparator;
			}
			SaveIssueExtensionsAndParameter(connection, languages, descriptions, issue, realParameters, issue.FilterTableObject);
			issueParameters = realParameters;
			if (!user.IsSuper)
			{
				_userTableObjectRights[user, issue] = RightType.Read;
				TableUserMapping mapping = _userTableObjectRights.Mapping(user, issue);
				connection.DbMapping.Save(mapping);
				_userTableObjectRights.Add(user, issue, mapping.Id, RightType.Read);
			}
			Objects.Add(issue);
			_categories[0]?.TableObjects.Add(issue);
			return issue.Id;
		}

		public void UpdateRight(ICredential credential, UpdateRightType type, int id, RightType right)
		{
			if ((credential.Type == CredentialType.User) ? (credential as IUser).IsSuper : (credential as IRole).IsSuper)
			{
				return;
			}
			switch (type)
			{
			case UpdateRightType.Optimization:
			{
				if (credential.Type == CredentialType.User)
				{
					RightType current2 = _userOptimizationRights[credential as IUser, Optimizations[id]];
					_userOptimizationRights[credential as IUser, Optimizations[id]] = right;
					OptimizationUserMapping mapping2 = _userOptimizationRights.Mapping(credential as IUser, Optimizations[id]);
					if (right == current2)
					{
						break;
					}
					if (right == RightType.Inherit)
					{
						Queue.AddDelete(mapping2.Clone());
						_userOptimizationRights[credential as IUser, Optimizations[id]] = right;
						break;
					}
					using (DatabaseBase databaseBase5 = ConnectionManager.CreateConnection(_cmanager.DbConfig))
					{
						databaseBase5.Open();
						databaseBase5.DbMapping.Save(mapping2);
					}
					_userOptimizationRights.Add(credential as IUser, Optimizations[id], mapping2.Id, right);
					break;
				}
				RightType current = _roleOptimizationRights[credential as IRole, Optimizations[id]];
				_roleOptimizationRights[credential as IRole, Optimizations[id]] = right;
				OptimizationRoleMapping mapping = _roleOptimizationRights.Mapping(credential as IRole, Optimizations[id]);
				if (right == current)
				{
					break;
				}
				if (right == RightType.Inherit)
				{
					Queue.AddDelete(mapping.Clone());
					_roleOptimizationRights[credential as IRole, Optimizations[id]] = right;
					break;
				}
				using (DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig))
				{
					db.Open();
					db.DbMapping.Save(mapping);
				}
				_roleOptimizationRights.Add(credential as IRole, Optimizations[id], mapping.Id, right);
				break;
			}
			case UpdateRightType.Category:
			{
				foreach (ITableObject t in Categories[id].TableObjects)
				{
					UpdateRight(credential, UpdateRightType.TableObject, t.Id, RightType.Inherit);
				}
				if (credential.Type == CredentialType.User)
				{
					RightType current4 = _userCategoryRights[credential as IUser, Categories[id]];
					_userCategoryRights[credential as IUser, Categories[id]] = right;
					CategoryUserMapping mapping4 = _userCategoryRights.Mapping(credential as IUser, Categories[id]);
					if (right == current4)
					{
						break;
					}
					if (right == RightType.Inherit)
					{
						Queue.AddDelete(mapping4.Clone());
						_userCategoryRights[credential as IUser, Categories[id]] = right;
						break;
					}
					using (DatabaseBase databaseBase6 = ConnectionManager.CreateConnection(_cmanager.DbConfig))
					{
						databaseBase6.Open();
						databaseBase6.DbMapping.Save(mapping4);
					}
					_userCategoryRights.Add(credential as IUser, Categories[id], mapping4.Id, right);
					break;
				}
				RightType current3 = _roleCategoryRights[credential as IRole, Categories[id]];
				_roleCategoryRights[credential as IRole, Categories[id]] = right;
				CategoryRoleMapping mapping3 = _roleCategoryRights.Mapping(credential as IRole, Categories[id]);
				if (right == current3)
				{
					break;
				}
				if (right == RightType.Inherit)
				{
					Queue.AddDelete(mapping3.Clone());
					_roleCategoryRights[credential as IRole, Categories[id]] = right;
					break;
				}
				using (DatabaseBase databaseBase7 = ConnectionManager.CreateConnection(_cmanager.DbConfig))
				{
					databaseBase7.Open();
					databaseBase7.DbMapping.Save(mapping3);
				}
				_roleCategoryRights.Add(credential as IRole, Categories[id], mapping3.Id, right);
				break;
			}
			case UpdateRightType.TableObject:
			{
				if (Objects[id].Type != TableType.Issue || (Objects[id] as IIssue).IssueType != IssueType.Filter)
				{
					foreach (IColumn c in Objects[id].Columns)
					{
						UpdateRight(credential, UpdateRightType.Column, c.Id, RightType.Inherit);
					}
				}
				if (credential.Type == CredentialType.User)
				{
					RightType current6 = _userTableObjectRights[credential as IUser, Objects[id]];
					_userTableObjectRights[credential as IUser, Objects[id]] = right;
					TableUserMapping mapping6 = _userTableObjectRights.Mapping(credential as IUser, Objects[id]);
					if (right == current6)
					{
						break;
					}
					if (right == RightType.Inherit)
					{
						Queue.AddDelete(mapping6.Clone());
						_userTableObjectRights[credential as IUser, Objects[id]] = right;
						break;
					}
					using (DatabaseBase databaseBase3 = ConnectionManager.CreateConnection(_cmanager.DbConfig))
					{
						databaseBase3.Open();
						databaseBase3.DbMapping.Save(mapping6);
					}
					_userTableObjectRights.Add(credential as IUser, Objects[id], mapping6.Id, right);
					break;
				}
				RightType current5 = _roleTableObjectRights[credential as IRole, Objects[id]];
				_roleTableObjectRights[credential as IRole, Objects[id]] = right;
				TableRoleMapping mapping5 = _roleTableObjectRights.Mapping(credential as IRole, Objects[id]);
				if (right == current5)
				{
					break;
				}
				if (right == RightType.Inherit)
				{
					Queue.AddDelete(mapping5.Clone());
					_roleTableObjectRights[credential as IRole, Objects[id]] = right;
					break;
				}
				using (DatabaseBase databaseBase4 = ConnectionManager.CreateConnection(_cmanager.DbConfig))
				{
					databaseBase4.Open();
					databaseBase4.DbMapping.Save(mapping5);
				}
				_roleTableObjectRights.Add(credential as IRole, Objects[id], mapping5.Id, right);
				break;
			}
			case UpdateRightType.Column:
			{
				if (credential.Type == CredentialType.User)
				{
					RightType current8 = _userColumnRights[credential as IUser, Columns[id]];
					_userColumnRights[credential as IUser, Columns[id]] = right;
					ColumnUserMapping mapping8 = _userColumnRights.Mapping(credential as IUser, Columns[id]);
					if (right == current8)
					{
						break;
					}
					if (right == RightType.Inherit)
					{
						Queue.AddDelete(mapping8.Clone());
						_userColumnRights[credential as IUser, Columns[id]] = right;
						break;
					}
					using (DatabaseBase databaseBase = ConnectionManager.CreateConnection(_cmanager.DbConfig))
					{
						databaseBase.Open();
						databaseBase.DbMapping.Save(mapping8);
					}
					_userColumnRights.Add(credential as IUser, Columns[id], mapping8.Id, right);
					break;
				}
				RightType current7 = _roleColumnRights[credential as IRole, Columns[id]];
				_roleColumnRights[credential as IRole, Columns[id]] = right;
				ColumnRoleMapping mapping7 = _roleColumnRights.Mapping(credential as IRole, Columns[id]);
				if (right == current7)
				{
					break;
				}
				if (right == RightType.Inherit)
				{
					Queue.AddDelete(mapping7.Clone());
					_roleColumnRights[credential as IRole, Columns[id]] = right;
					break;
				}
				using (DatabaseBase databaseBase2 = ConnectionManager.CreateConnection(_cmanager.DbConfig))
				{
					databaseBase2.Open();
					databaseBase2.DbMapping.Save(mapping7);
				}
				_roleColumnRights.Add(credential as IRole, Columns[id], mapping7.Id, right);
				break;
			}
			default:
				throw new ArgumentException("type");
			}
		}

		public void UpdateRoleSetting(RightType right, RoleSettingsType type, int roleId, string value = null)
		{
			IRole role = Roles[roleId];
			GetRoleSettingOnSpecificType(type, roleId);
			IRoleSettingsCollection settings = role.Settings;
			string value2;
			if (value != null)
			{
				value2 = value;
			}
			else
			{
				int num = (int)right;
				value2 = num.ToString();
			}
			settings[type, roleId] = value2;
		}

		private void OnPropertyChanged(string property)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		private void LoadMockRepository(IDataRepository rep)
		{
			_archiveInformation = (TableArchiveInformationCollection)rep.ArchiveInformation;
			_userTableObjectRights = (UserTableObjectRights)rep.UserTableObjectRights;
			_roleTableObjectRights = (RoleTableObjectRights)rep.RoleTableObjectRights;
			_issues = (IssueCollection)rep.Issues;
			_columns = (FullColumnCollection)rep.Columns;
			_userColumnOrderSettings = (UserColumnOrderSettingsCollection)rep.UserColumnOrderSettings;
			_userTableObjectOrderSettings = (UserTableObjectOrderSettingsCollection)rep.UserTableObjectOrderSettings;
			_userColumnRights = (UserColumnRights)rep.UserColumnRights;
			_roleColumnRights = (RoleColumnRights)rep.RoleColumnRights;
			_userColumnSettings = (UserColumnSettingCollection)rep.UserColumnSettings;
			_userTableObjectSettings = (UserTableObjectSettingsCollection)rep.UserTableObjectSettings;
			_userOptimizationSettings = (UserOptimizationSettingsCollection)rep.UserOptimizationSettings;
			_optimizations = (OptimizationCollection)rep.Optimizations;
		}

		private void OnDataBaseUpGradeCheck()
		{
			if (this.DataBaseUpGradeChecked != null)
			{
				this.DataBaseUpGradeChecked();
			}
		}

		private void cmanager_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "ConnectionState" && _cmanager.ConnectionState == ConnectionStates.Online)
			{
				_log.Log(LogLevelEnum.Info, "Connection established");
				try
				{
					int connectionTimeOut = 180;
					_cmanager.SetupTimeOuts(connectionTimeOut);
					_log.Log(LogLevelEnum.Info, $"Sleeping connection time out set to {connectionTimeOut}");
				}
				catch (Exception ex2)
				{
					_log.Log(LogLevelEnum.Warn, ex2.Message);
				}
				if (this.ConnectionEstablished != null)
				{
					this.ConnectionEstablished(this, EventArgs.Empty);
				}
				try
				{
					using DatabaseBase db = _cmanager.GetConnection();
					DatabaseOutOfDateInformation = HasDatabaseUpgrade(db);
					OnDataBaseUpGradeCheck();
					if (DatabaseOutOfDateInformation == null)
					{
						if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["SkipCheckTables"]))
						{
							CheckTables(db);
						}
						LoadTables(db, useNewIssueMethod: true);
						if (ConfigurationManager.AppSettings["DropUserSchemas"] != null && ConfigurationManager.AppSettings["DropUserSchemas"] == "true")
						{
							foreach (IUser user in Users)
							{
								foreach (IOptimization o in Optimizations.Where((IOptimization opt) => opt.Group.Type == OptimizationType.System))
								{
									if (db.DatabaseExists(user.UserTable(o.Value)))
									{
										db.DropDatabase(user.UserTable(o.Value));
									}
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					_log.Error(ex.Message, ex);
					throw;
				}
				if (this.SystemDbInitialized != null)
				{
					this.SystemDbInitialized(this, EventArgs.Empty);
				}
			}
			if (e.PropertyName == "ConnectionState")
			{
				OnPropertyChanged("ConnectionManager");
			}
		}

		private Dictionary<int, int> GetColumnOrder(IUser user, ITableObject tobj)
		{
			IUserColumnOrderSettings settings = _userColumnOrderSettings[user, tobj];
			Dictionary<int, int> columnOrderDict = new Dictionary<int, int>();
			int ordinal = 0;
			if (settings == null)
			{
				return null;
			}
			string[] array = settings.ColumnOrder.Split(',');
			foreach (string columnId in array)
			{
				if (!columnOrderDict.ContainsKey(int.Parse(columnId)))
				{
					columnOrderDict.Add(int.Parse(columnId), ordinal);
					ordinal++;
				}
			}
			return columnOrderDict;
		}

		public Dictionary<int, int> GetTableObjectOrder(IUser user, TableType type)
		{
			IUserTableObjectOrderSettings settings = _userTableObjectOrderSettings[user.Id, type];
			if (settings == null)
			{
				return null;
			}
			Dictionary<int, int> tableObjectOrderDict = new Dictionary<int, int>();
			if (!string.IsNullOrEmpty(settings.TableObjectOrder))
			{
				string[] array = settings.TableObjectOrder.Split(',');
				int ordinal = 0;
				string[] array2 = array;
				foreach (string tableId in array2)
				{
					tableObjectOrderDict.Add(int.Parse(tableId), ordinal);
					ordinal++;
				}
			}
			return tableObjectOrderDict;
		}

		public string GetTableColumnWidths(int userId, int tableId)
		{
			return _userTableColumnWidthSettings[userId, tableId]?.ColumnWidths;
		}

		private void UpdateRelations(ITableObject tobj, IColumn col)
		{
			if (tobj == null || tobj.Relations == null || col == null)
			{
				return;
			}
			foreach (IRelation relation in tobj.Relations)
			{
				foreach (IColumnConnection cc in relation as TableObject.Relation)
				{
					if (cc != null && cc.Source != null)
					{
						if (cc.Source.Id == col.Id)
						{
							(cc as TableObject.ColumnConnection).Source = col;
						}
						if (cc.Target != null && cc.Target.Id == col.Id)
						{
							(cc as TableObject.ColumnConnection).Target = col;
						}
					}
				}
			}
		}

		private void CleanUpRelations(ICategory c)
		{
			foreach (ITableObject tobj in c.TableObjects)
			{
				List<IRelation> invalidRelations = (from r in tobj.Relations
					from cc in r
					where (!(cc.Target is IColumn)) ? (c.TableObjects[(cc.Target as IParameter).Issue.Id] == null) : (c.TableObjects[(cc.Target as IColumn).Table.Id] == null)
					select r).ToList();
				lock (tobj.Relations)
				{
					foreach (IRelation i in invalidRelations)
					{
						(tobj.Relations as TableObject.RelationCollection).Remove(i);
					}
				}
			}
		}

		private void CleanUpTableObjectsRelations(IUser user, ITableObjectCollection tableObjects)
		{
			GetOptimizations(user);
			List<ITableObject> allTables = Objects.Where((ITableObject t) => !user.IsSuper && GetUserRightToTableObject(t, user) > RightType.None).ToList();
			foreach (ITableObject oneTableFromAll in allTables)
			{
				foreach (IColumn column in oneTableFromAll.Columns)
				{
					UpdateRelations(oneTableFromAll, column);
				}
			}
			foreach (ITableObject tableObject in tableObjects)
			{
				TableObject.RelationCollection invalidRelations = new TableObject.RelationCollection();
				foreach (IRelation r in tableObject.Relations)
				{
					foreach (IColumnConnection cc in r)
					{
						if (cc.Target is IColumn)
						{
							if (!allTables.Contains((cc.Target as IColumn).Table) && (cc.Target as IColumn).Table.Id != tableObject.Id)
							{
								invalidRelations.Add(r);
							}
						}
						else if (!allTables.Contains((cc.Target as IParameter).Issue) && (cc.Target as IParameter).Issue.Id != tableObject.Id)
						{
							invalidRelations.Add(r);
						}
					}
				}
				foreach (IRelation inv in invalidRelations)
				{
					(tableObject.Relations as TableObject.RelationCollection).Remove(inv);
				}
			}
		}

		public void SetPageSystem(ITableObject tobj, PageSystem pageSystem)
		{
			(tobj as TableObject).PageSystem = pageSystem;
		}

		public IPassword AddPasswordToHistory(DatabaseBase connection, string passwordHash, DateTime creationDate, int userId)
		{
			IPassword pwd = new Password
			{
				CreationDate = creationDate,
				PasswordHash = passwordHash,
				UserId = userId
			};
			connection.DbMapping.Save(pwd);
			return pwd;
		}

		public void DeletePasswordFromHistory(DatabaseBase connection, IPassword pwd)
		{
			connection.DbMapping.Delete(pwd);
		}

		public static void SaveParameterTexts(DatabaseBase connection, IEnumerable<ILanguage> languages, Dictionary<string, List<string>> descriptions, int ord, IParameter param)
		{
			foreach (ILanguage j in languages)
			{
				param.SetDescription(descriptions[j.CountryCode][ord + 1], j);
			}
			connection.DbMapping.Save(param);
			foreach (ILanguage i in languages)
			{
				connection.DbMapping.Delete<ParameterText>("ref_id = " + param.Id + " AND country_code = '" + i.CountryCode + "'");
				connection.DbMapping.Save(new ParameterText
				{
					CountryCode = i.CountryCode,
					RefId = param.Id,
					Text = descriptions[i.CountryCode][ord + 1]
				});
			}
		}

		private IEnumerable<string> CreateInFilter(IEnumerable<int> collection)
		{
			StringBuilder sb = new StringBuilder();
			int count = 0;
			foreach (int item in collection)
			{
				sb.Append(item.ToString()).Append(',');
				if (count++ > 1000)
				{
					count = 0;
					sb.Insert(0, " IN (");
					string filter2 = sb.ToString();
					filter2 = filter2.Trim(',');
					yield return filter2 + " )";
					sb.Clear();
				}
			}
			if (count > 0)
			{
				sb.Insert(0, " IN (");
				string filter = sb.ToString();
				filter = filter.Trim(',');
				yield return filter + " )";
				sb.Clear();
			}
		}

		public void UpdateUserLogRights(int credentialId, int userId, RightType right)
		{
			if (_users[userId] == null || _users[credentialId] == null)
			{
				return;
			}
			_userLogRights[_users[userId], _users[credentialId]] = right;
			UserLogUserMapping mapping = _userLogRights.Mapping(_users[userId], _users[credentialId]);
			if (right == RightType.None)
			{
				using (DatabaseBase databaseBase = ConnectionManager.CreateConnection(_cmanager.DbConfig))
				{
					databaseBase.Open();
					databaseBase.DbMapping.Delete(mapping.Clone());
				}
				_userLogRights.Remove(Tuple.Create(_users[userId], _users[credentialId]));
			}
			else
			{
				using DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig);
				db.Open();
				db.DbMapping.Save(mapping);
			}
		}

		public void ShowHideLog(int id, int userId, bool showHidden)
		{
			IUserUserLogSettings settings = _userUserLogSettings[userId, id];
			if (settings == null)
			{
				settings = new UserUserLogSettings
				{
					UserId = userId,
					User = Users[userId],
					IsVisible = showHidden,
					UserLogId = id
				};
				using (DatabaseBase databaseBase = ConnectionManager.CreateConnection(_cmanager.DbConfig))
				{
					databaseBase.Open();
					databaseBase.DbMapping.Save(settings);
				}
				_userUserLogSettings.Add(settings as UserUserLogSettings);
			}
			else
			{
				(settings as UserUserLogSettings).IsVisible = showHidden;
				using DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig);
				db.Open();
				db.DbMapping.Save(settings);
			}
		}

		public List<IUser> GetLoggedUsers(IUser user)
		{
			List<IUser> userList = new List<IUser>();
			if (user.IsSuper)
			{
				foreach (IUser userData2 in Users)
				{
					userList.Add(userData2);
				}
				return userList;
			}
			foreach (IUser userData in Users)
			{
				if (_userLogRights[userData, user] > RightType.None)
				{
					userList.Add(userData);
				}
			}
			return userList;
		}

		private void DeleteMapping<T>(T itemTodelete)
		{
			using DatabaseBase DB = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			if (!DB.IsOpen)
			{
				DB.Open();
			}
			DB.DbMapping.Delete(itemTodelete);
		}

		private void CollectAllChildren(IOptimization opt, ref List<IOptimization> descendantlist)
		{
			descendantlist.Add(opt);
			foreach (IOptimization copt in opt.Children)
			{
				CollectAllChildren(copt, ref descendantlist);
			}
		}

		public void SetRoleAreas(ITableObject tobj, List<RoleArea> roleAreas)
		{
			(tobj as TableObject).RoleAreas = roleAreas;
		}

		public void ResetColumnWidthsOnResetColumnOrder(ITableObject tobj, IUser user, int originalId = 0)
		{
			int id = ((originalId == 0) ? tobj.Id : originalId);
			ITableObject table = Objects[id];
			if (table is Issue)
			{
				id = (table as Issue).OriginalId;
			}
			List<string> resultList = new List<string>();
			using (DatabaseBase conn = _cmanager.GetConnection())
			{
				string sqlQuerry = "SELECT vc.name,vc.ordinal FROM " + conn.DbConfig.DbName + ".COLUMNS vc WHERE vc.table_id='" + id + "' order by vc.ordinal;";
				using IDataReader reader = conn.ExecuteReader(sqlQuerry);
				while (reader.Read())
				{
					resultList.Add(reader.GetString(0));
				}
			}
			IOrderedEnumerable<IColumn> newList = from c in tobj.Columns.ToList()
				orderby c.Ordinal
				select c;
			string data = GetTableColumnWidths(user.Id, id);
			Dictionary<string, string> columnWidthsByName = new Dictionary<string, string>();
			if (data != null)
			{
				string[] widthData = data.Split(',');
				for (int j = 0; j < newList.Count(); j++)
				{
					columnWidthsByName.Add(newList.ElementAt(j).Name, widthData[j]);
				}
				List<string> test = new List<string>();
				for (int i = 0; i < resultList.Count(); i++)
				{
					test.Add(columnWidthsByName[resultList.ElementAt(i)]);
				}
				if (id > 0)
				{
					SaveColumnWidthSizes(id, string.Join(",", test.ToArray()), user);
				}
			}
		}

		private void AddAndSaveLanguageIfNotExists(string countryCode)
		{
			if (Languages[countryCode] == null)
			{
				Language language = new Language
				{
					CountryCode = countryCode.ToLower()
				};
				if (Language.LanguageDescriptions.ContainsKey(language.CountryCode))
				{
					language.LanguageName = Language.LanguageDescriptions[language.CountryCode].Item1;
					language.LanguageMotto = Language.LanguageDescriptions[language.CountryCode].Item2;
				}
				_languages.Add(countryCode, language);
				using DatabaseBase conn = ConnectionManager.GetConnection();
				conn.DbMapping.Save(language);
			}
		}

		private int CheckOrdinals(TableType type, DatabaseBase db)
		{
			IEnumerable<ITableObject> tobjs = null;
			switch (type)
			{
			case TableType.Issue:
				tobjs = Issues.OrderBy((IIssue i) => i.Ordinal);
				break;
			case TableType.View:
				tobjs = Views.OrderBy((IView v) => v.Ordinal);
				break;
			case TableType.Table:
				tobjs = Tables.OrderBy((ITable t) => t.Ordinal);
				break;
			}
			int ord = 0;
			bool check = true;
			foreach (ITableObject t3 in tobjs)
			{
				if (t3.Ordinal != ord)
				{
					t3.Ordinal = ord;
					check = false;
				}
				ord++;
			}
			if (!check)
			{
				foreach (ITableObject t2 in tobjs)
				{
					db.DbMapping.Save(t2);
				}
				return ord;
			}
			return ord;
		}

		internal List<FakeTable> GetFakeTables(DatabaseBase viewboxDb)
		{
			List<FakeTable> list = viewboxDb.DbMapping.Load<FakeTable>("type = " + 6);
			foreach (FakeTable fakeTable in list)
			{
				foreach (global::SystemDb.Internal.OrderArea o in viewboxDb.DbMapping.Load<global::SystemDb.Internal.OrderArea>("table_id = " + fakeTable.Id))
				{
					if (fakeTable.OrderAreas == null)
					{
						fakeTable.OrderAreas = new OrderAreaCollection(fakeTable);
					}
					(fakeTable.OrderAreas as OrderAreaCollection).Add(o);
				}
			}
			return list;
		}

		public void AddRelationsToDataBase(int id, int joinTableId, ITableObjectCollection tableObjectCollection, List<IRelationDatabaseObject> relations, ITableObject tableObject)
		{
			using DatabaseBase conn = _cmanager.GetConnection();
			int relationId = conn.GetMaxValueOfColumn("relations", "relation_id") + 1;
			TableObject.Relation relation = new TableObject.Relation();
			foreach (IRelationDatabaseObject relationDatabase in relations)
			{
				relationDatabase.RelationId = relationId;
				relation.RelationId = relationId;
				relation.Add(new TableObject.ColumnConnection
				{
					Source = _columns[relationDatabase.ParentId],
					Target = _columns[relationDatabase.ChildId],
					Operator = relationDatabase.Operator,
					FullLine = relationDatabase.FullLine,
					RelationType = (RelationType)relationDatabase.Type,
					ExtInfo = relationDatabase.ExtInfo,
					ColumnExtInfo = relationDatabase.ColumnExtInfo,
					RelationId = relationDatabase.RelationId,
					UserDefined = relationDatabase.UserDefined
				});
			}
			bool relationAlreadyExists = false;
			ITableObject actualTable = tableObjectCollection[id];
			if (actualTable == null && id < 0)
			{
				actualTable = tableObject;
			}
			if (actualTable == null)
			{
				return;
			}
			foreach (IRelation checkRelation in actualTable.Relations)
			{
				if (checkRelation.Count() != relation.Count)
				{
					continue;
				}
				int matchCount = 0;
				foreach (IColumnConnection checkRelationConnection in checkRelation)
				{
					foreach (IColumnConnection relConnection in relation)
					{
						if (relConnection.Source.Id == checkRelationConnection.Source.Id && relConnection.Target.Id == checkRelationConnection.Target.Id)
						{
							matchCount++;
						}
					}
				}
				if (matchCount == relation.Count)
				{
					relationAlreadyExists = true;
					break;
				}
			}
			if (relationAlreadyExists)
			{
				return;
			}
			(actualTable as TableObject).AddRelation(relation);
			if (id < 0)
			{
				ITableObject table2 = tableObjectCollection.FirstOrDefault((ITableObject x) => x.Database == actualTable.Database && x.Name == actualTable.Name);
				if (table2 != null)
				{
					(table2 as TableObject).AddRelation(relation);
				}
				foreach (ITableObject item in Objects.Where((ITableObject x) => x.Database == actualTable.Database && x.Name == actualTable.Name))
				{
					(item as TableObject).AddRelation(relation);
				}
			}
			conn.DbMapping.Save(typeof(Relation), relations);
		}

		public void DeleteRelationsFromDatabase(int id, string[] deleteRelations, ITableObjectCollection tableObjectCollection, ITableObject tableObject)
		{
			if (deleteRelations == null || tableObjectCollection == null)
			{
				return;
			}
			ITableObject actualTable = tableObjectCollection[id];
			if (actualTable == null && id < 0)
			{
				actualTable = tableObject;
			}
			IRelationCollection actualTableColumnConnections = actualTable.Relations;
			using DatabaseBase conn = _cmanager.GetConnection();
			foreach (string s in deleteRelations)
			{
				foreach (IRelation relation in actualTableColumnConnections)
				{
					if (!(((relation.RelationId == 0) ? relation.First().RelationId : relation.RelationId).ToString() == s))
					{
						continue;
					}
					(actualTable as TableObject).RemoveRelation(relation);
					if (id >= 0)
					{
						break;
					}
					ITableObject table2 = tableObjectCollection.FirstOrDefault((ITableObject x) => x.Database == actualTable.Database && x.Name == actualTable.Name);
					if (table2 != null)
					{
						(table2 as TableObject).RemoveRelation(relation);
					}
					foreach (ITableObject item in Objects.Where((ITableObject x) => x.Database == actualTable.Database && x.Name == actualTable.Name))
					{
						(item as TableObject).RemoveRelation(relation);
					}
					break;
				}
				conn.Delete("relations", "relation_id=" + s);
			}
		}

		private void GenerateTable(DatabaseBase conn, IColumn column, ITableObject table, string distinctQuery, string minMaxQuery, global::SystemDb.Internal.OrderArea orderArea = null)
		{
			object minValueObj = null;
			object maxValueObj = null;
			string minValue = string.Empty;
			string maxValue = string.Empty;
			try
			{
				if (column.IsEmpty)
				{
					return;
				}
				if (column.DataType == SqlType.Numeric || column.DataType == SqlType.Integer || column.DataType == SqlType.Decimal || column.DataType == SqlType.Date || column.DataType == SqlType.Time || column.DataType == SqlType.DateTime)
				{
					string sqlQuery = $"SELECT {((column.DataType == SqlType.Decimal) ? $"CAST(MIN({conn.Enquote(column.Name)}) as CHAR)" : $"MIN({conn.Enquote(column.Name)})")}, {((column.DataType == SqlType.Decimal) ? $"CAST(MAX({conn.Enquote(column.Name)}) as CHAR)" : $"MAX({conn.Enquote(column.Name)})")} {minMaxQuery}";
					using (IDataReader reader = conn.ExecuteReader(sqlQuery))
					{
						if (reader.Read())
						{
							if (column.DataType == SqlType.Date || column.DataType == SqlType.DateTime)
							{
								try
								{
									minValueObj = reader.GetDateTime(0);
								}
								catch (Exception)
								{
									CultureInfo culture2 = CultureInfo.CreateSpecificCulture("de");
									if (DateTime.TryParse(reader[0].ToString(), culture2, DateTimeStyles.None, out var dateResult2))
									{
										minValueObj = dateResult2;
									}
									else
									{
										using (NDC.Push(LogHelper.GetNamespaceContext()))
										{
											_log.Warn($"Can't parse min value {reader[0]}, column_id = {column.Id}");
										}
									}
								}
							}
							else
							{
								minValueObj = reader.GetValue(0);
							}
							minValue = ((minValueObj == null) ? null : ((!(minValueObj is DateTime)) ? minValueObj.ToString() : ((column.DataType != SqlType.Date) ? ((DateTime)minValueObj).ToString("u") : ((DateTime)minValueObj).ToString("d"))));
							if (column.DataType == SqlType.Date || column.DataType == SqlType.DateTime)
							{
								try
								{
									maxValueObj = reader.GetDateTime(1);
								}
								catch
								{
									CultureInfo culture = CultureInfo.CreateSpecificCulture("de");
									if (DateTime.TryParse(reader[1].ToString(), culture, DateTimeStyles.None, out var dateResult))
									{
										maxValueObj = dateResult;
									}
									else
									{
										using (NDC.Push(LogHelper.GetNamespaceContext()))
										{
											_log.Warn($"Can't parse max value {reader[1]}, column_id = {column.Id}");
										}
									}
								}
							}
							else
							{
								maxValueObj = reader.GetValue(1);
							}
							maxValue = ((maxValueObj == null) ? null : ((!(maxValueObj is DateTime)) ? maxValueObj.ToString() : ((DateTime)maxValueObj).ToString((column.DataType == SqlType.Date) ? "d" : "u")));
						}
					}
					EmptyDistinctColumn data = new EmptyDistinctColumn
					{
						ColumnId = column.Id,
						IndexValue = orderArea?.IndexValue,
						SplitValue = orderArea?.SplitValue,
						SortValue = orderArea?.SortValue,
						TableId = table.Id,
						OneDistinct = (minValue != null && maxValue != null && minValue == maxValue),
						MinValue = minValue,
						MaxValue = maxValue,
						IsEmpty = (string.IsNullOrEmpty(minValue) && string.IsNullOrEmpty(maxValue))
					};
					conn.DbMapping.Save(data);
					return;
				}
				int num = 0;
				string rowData = string.Empty;
				using (IDataReader dataReader = conn.ExecuteReader(distinctQuery))
				{
					while (dataReader.Read())
					{
						num++;
						rowData = (dataReader.IsDBNull(0) ? "" : dataReader.GetString(0));
						if (num == 2)
						{
							break;
						}
					}
				}
				EmptyDistinctColumn data2 = new EmptyDistinctColumn
				{
					ColumnId = column.Id,
					IndexValue = orderArea?.IndexValue,
					SplitValue = orderArea?.SplitValue,
					SortValue = orderArea?.SortValue,
					TableId = table.Id,
					OneDistinct = (num < 2),
					MinValue = minValue,
					MaxValue = maxValue,
					IsEmpty = (num < 2 && (string.IsNullOrEmpty(rowData) || string.IsNullOrWhiteSpace(rowData)))
				};
				conn.DbMapping.Save(data2);
			}
			catch (Exception ex)
			{
				StringBuilder builder = new StringBuilder();
				builder.Append("Exception occurred while generating table. Column id = ").Append(column.Id).Append(", Column name = ")
					.Append(column.Name)
					.Append(", Min value = ")
					.Append(minValueObj)
					.Append(", Max value = ")
					.Append(maxValueObj);
				using (NDC.Push(LogHelper.GetNamespaceContext()))
				{
					_log.ErrorWithCheck(builder.ToString(), ex);
				}
			}
		}

		public void CreateEmptyDistinctTable(ProgressCalculator progress = null, string databaseName = null, string tableName = null)
		{
			if (progress != null)
			{
				progress.Description = "Start generating Empty/Distinct table";
				progress.SetStep(0);
			}
			using (NDC.Push(LogHelper.GetNamespaceContext()))
			{
				_log.Log(LogLevelEnum.Info, "Start generating Empty/Distinct table", collectEntry: true);
				try
				{
					using DatabaseBase conn = _cmanager.GetConnection();
					conn.DbMapping.CreateTableIfNotExists<EmptyDistinctColumn>();
					List<TableObject> tables = new List<TableObject>();
					tables.AddRange(conn.DbMapping.Load<global::SystemDb.Internal.Table>("type = " + 1));
					tables.AddRange(conn.DbMapping.Load<View>("type = " + 2));
					List<global::SystemDb.Internal.Column> allColumns = conn.DbMapping.Load<global::SystemDb.Internal.Column>().ToList();
					conn.DbMapping.Load<global::SystemDb.Internal.OrderArea>().ToList();
					new StringBuilder();
					if (!string.IsNullOrWhiteSpace(databaseName) && !string.IsNullOrWhiteSpace(tableName))
					{
						tables = tables.Where((TableObject t) => t.Database.ToLower() == databaseName.ToLower() && t.TableName.ToLower() == tableName.ToLower()).ToList();
					}
					progress?.SetWorkSteps(tables.Count, hasChildren: false);
					foreach (TableObject table in tables)
					{
						progress?.StepDone();
						allColumns.Where((global::SystemDb.Internal.Column w) => w.TableId == table.Id).ToList();
					}
					_log.Log(LogLevelEnum.Info, $"Finished", collectEntry: true);
				}
				catch (Exception ex)
				{
					_log.ErrorWithCheck("Exception occurred while generating table.", ex);
					throw;
				}
			}
		}

		internal List<Tuple<long, long>> GetRanges(IEnumerable<global::SystemDb.Internal.OrderArea> orderAreas, string languageValue, string indexValue, string splitValue, string sortValue)
		{
			return OrderAreaCollection.GetRanges(languageValue, indexValue, splitValue, sortValue, orderAreas);
		}

		public bool IsEmptyDistinctGenerated()
		{
			using DatabaseBase conn = _cmanager.GetConnection();
			conn.DbMapping.CreateTableIfNotExists<EmptyDistinctColumn>();
			return conn.CountTable("empty_distinct_columns") > 0;
		}

		public bool IsExtendedGenerated()
		{
			using DatabaseBase conn = _cmanager.GetConnection();
			conn.DbMapping.CreateTableIfNotExists<ExtendedColumnInformation>();
			return conn.CountTable("extended_column_information") > 0;
		}

		public static ITableObject GetArchiveById(DatabaseBase connection, int id)
		{
			if (id <= 0)
			{
				return null;
			}
			Archive tobj = connection.DbMapping.Load<Archive>(id);
			foreach (global::SystemDb.Internal.Column c in connection.DbMapping.Load<global::SystemDb.Internal.Column>("table_id = " + id))
			{
				c.Table = tobj;
				switch (c.OptimizationType)
				{
				case OptimizationType.IndexTable:
					(c.Table as TableObject).IndexTableColumn = c;
					break;
				case OptimizationType.SplitTable:
					(c.Table as TableObject).SplitTableColumn = c;
					break;
				case OptimizationType.SortColumn:
					(c.Table as TableObject).SortColumn = c;
					break;
				}
				(tobj.Columns as ColumnCollection).Add(c);
			}
			foreach (global::SystemDb.Internal.OrderArea o in connection.DbMapping.Load<global::SystemDb.Internal.OrderArea>("table_id = " + id))
			{
				if (tobj.OrderAreas == null)
				{
					tobj.OrderAreas = new OrderAreaCollection(tobj);
				}
				(tobj.OrderAreas as OrderAreaCollection).Add(o);
			}
			return tobj;
		}

		private List<ParameterValue> RearangeValues(List<ParameterValueOrder> order, IParameter parameter)
		{
			List<ParameterValue> tempParValues = new List<ParameterValue>();
			foreach (ParameterValueOrder item in order)
			{
				string[] array = item.CollectionsIds.Split(',');
				foreach (string valueId in array)
				{
					int id = 0;
					foreach (IParameterValue parameterValue in parameter.Values)
					{
						int.TryParse(valueId, out id);
						if ((parameterValue as ParameterValue).Id != id)
						{
							continue;
						}
						ParameterValue parVal = new ParameterValue
						{
							CollectionId = (parameterValue as ParameterValue).CollectionId,
							Value = (parameterValue as ParameterValue).Value,
							Id = id
						};
						foreach (KeyValuePair<string, string> desc in (parameterValue as ParameterValue).Descriptions)
						{
							parVal.SetDescription(desc.Value, _languages[desc.Key]);
						}
						tempParValues.Add(parVal);
					}
				}
			}
			return tempParValues;
		}

		public void ChangeSequenceOfIssueParameterValues(IIssueCollection issues, IUser user)
		{
			using DatabaseBase conn = _cmanager.GetConnection();
			try
			{
				foreach (IIssue issue in issues)
				{
					foreach (IParameter parameter in issue.Parameters)
					{
						if (parameter.Values != null && parameter.Values.Count() != 0)
						{
							List<ParameterValueOrder> settings = conn.DbMapping.Load<ParameterValueOrder>($"user_id = {user.Id} AND parameter_id = {parameter.Id} AND collection_id = {(parameter.Values.ElementAt(0) as ParameterValue).CollectionId}");
							if (settings.Count != 0)
							{
								(parameter as Parameter).Values = RearangeValues(settings, parameter);
							}
						}
					}
				}
			}
			catch (Exception)
			{
			}
		}

		public void SaveParameterValueOrder(IParameter parameter, IUser user, object paramValue)
		{
			LinkedList<int> paramOrder = new LinkedList<int>();
			foreach (IParameterValue parameterValue in parameter.Values)
			{
				if (paramValue.ToString() == parameterValue.Value)
				{
					paramOrder.AddFirst((parameterValue as ParameterValue).Id);
				}
				else
				{
					paramOrder.AddLast((parameterValue as ParameterValue).Id);
				}
			}
			using DatabaseBase conn = _cmanager.GetConnection();
			List<ParameterValueOrder> settings = conn.DbMapping.Load<ParameterValueOrder>($"user_id = {user.Id} AND parameter_id = {parameter.Id} AND collection_id = {(parameter.Values.ElementAt(0) as ParameterValue).CollectionId}");
			ParameterValueOrder setting = new ParameterValueOrder();
			if (settings.Count == 0)
			{
				setting.CollectionId = (parameter.Values.ElementAt(0) as ParameterValue).CollectionId;
				setting.UserId = user.Id;
				setting.ParameterId = parameter.Id;
				setting.CollectionsIds = string.Join(",", paramOrder.ToArray());
			}
			else
			{
				setting = settings.ElementAt(0);
				setting.CollectionsIds = string.Join(",", paramOrder.ToArray());
			}
			conn.DbMapping.Save(setting);
		}

		private void OnLoadingFinished()
		{
			if (this.LoadingFinished != null)
			{
				this.LoadingFinished();
			}
		}

		public IEnumerable<int> GetUserFavoriteIssues(IUser user)
		{
			IUserFavoriteIssueSettings settings = _userFavoriteIssueCollection[user];
			if (settings == null)
			{
				return new List<int>();
			}
			return from f in settings.FavoriteList.Split(',')
				select int.Parse(f);
		}

		public void SaveFavoriteIssue(IUser user, int id)
		{
			UserFavoriteIssueSettings settings = _userFavoriteIssueCollection[user] as UserFavoriteIssueSettings;
			if (settings == null)
			{
				settings = new UserFavoriteIssueSettings
				{
					FavoriteList = id.ToString(),
					User = user,
					UserId = user.Id
				};
			}
			else
			{
				UserFavoriteIssueSettings userFavoriteIssueSettings = settings;
				userFavoriteIssueSettings.FavoriteList = userFavoriteIssueSettings.FavoriteList + "," + id;
			}
			_userFavoriteIssueCollection.Add(settings);
			using DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			db.Open();
			Queue.AddSave(settings);
		}

		public void DeleteFavoriteIssue(IUser user, int id)
		{
			UserFavoriteIssueSettings settings = _userFavoriteIssueCollection[user] as UserFavoriteIssueSettings;
			if (settings == null)
			{
				return;
			}
			IEnumerable<string> a = from f in settings.FavoriteList.Split(',')
				where !f.Contains(id.ToString())
				select f;
			settings.FavoriteList = string.Join(",", a);
			_userFavoriteIssueCollection.Remove(settings);
			if (settings.FavoriteList != "")
			{
				_userFavoriteIssueCollection.Add(settings);
			}
			using DatabaseBase db = ConnectionManager.CreateConnection(_cmanager.DbConfig);
			db.Open();
			if (settings.FavoriteList == "")
			{
				Queue.AddDelete(settings);
			}
			else
			{
				Queue.AddSave(settings);
			}
		}

		public void SaveLastExecutedIssue(IUser user, int id)
		{
			UserLastIssueSettings settings = _userLastIssueCollection[user] as UserLastIssueSettings;
			if (settings == null)
			{
				settings = new UserLastIssueSettings
				{
					LastUsedIssue = id,
					User = user,
					UserId = user.Id
				};
			}
			else
			{
				settings.LastUsedIssue = id;
			}
			_userLastIssueCollection.Add(settings);
		}

		public List<string> ReadDistinctData(int id)
		{
			List<string> result = new List<string>();
			using DatabaseBase conn = _cmanager.GetConnection();
			using IDataReader reader = conn.ExecuteReader($"SELECT DISTINCT({_columns[id]}) FROM {_columns[id].Table.Name}");
			while (reader.Read())
			{
				result.Add(reader.GetString(0));
			}
			return result;
		}

		public void CalculateExtendedColumnInformation(string viewTableName, string databaseTableName, string columnName)
		{
			using (NDC.Push(LogHelper.GetNamespaceContext()))
			{
				try
				{
					using DatabaseBase conn = _cmanager.GetConnection();
					string dataBaseName = Objects.ElementAt(0).Database;
					string databaseTable = (Objects.Contains(dataBaseName + "." + databaseTableName) ? databaseTableName : databaseTableName.TrimStart('_'));
					if (!Objects.Contains(dataBaseName + "." + databaseTable))
					{
						return;
					}
					ITableObject to = Objects[dataBaseName + "." + databaseTable];
					ITableObject viewObj = Objects[dataBaseName + "." + viewTableName];
					StringBuilder sb = new StringBuilder();
					sb.Append("SELECT CHECKTABLE,DOMNAME FROM ").Append(conn.Enquote(to.Database, "DD03L")).Append(" WHERE TABNAME = \"")
						.Append(databaseTable)
						.Append("\" AND FIELDNAME = \"")
						.Append(columnName)
						.Append("\"");
					int parentColumnId = 0;
					if (viewObj != null && viewObj.Columns != null && viewObj.Columns.Contains(columnName))
					{
						parentColumnId = viewObj.Columns[columnName].Id;
					}
					string targetTable = string.Empty;
					string targetColumn = string.Empty;
					using (IDataReader reader = conn.ExecuteReader(sb.ToString()))
					{
						if (reader.Read())
						{
							targetTable = reader.GetString(0);
							targetColumn = reader.GetString(1);
						}
					}
					targetTable += "t";
					int childColumnId = 0;
					if (Objects.Contains(dataBaseName + "." + targetTable))
					{
						int tableId = Objects[dataBaseName + "." + targetTable].Id;
						if (Objects[tableId].Columns.Contains(columnName))
						{
							childColumnId = Objects[tableId].Columns[columnName].Id;
						}
						else if (Objects[tableId].Columns.Contains(targetColumn))
						{
							childColumnId = Objects[tableId].Columns[targetColumn].Id;
						}
						_log.Info("TableName : " + Objects[tableId].TableName + ", Table id : " + tableId);
						ExtendedColumnInformation extendedInformation = new ExtendedColumnInformation
						{
							ParentColumnId = parentColumnId,
							ChildColumnId = childColumnId,
							InformationColumnId = Objects[tableId].Columns.ElementAt(Objects[tableId].Columns.Count - 1).Id
						};
						Queue.AddSave(extendedInformation);
					}
				}
				catch (Exception ex)
				{
					_log.Error(ex.Message, ex);
					throw;
				}
			}
		}

		public void SaveExtendedColumnInformation(string dataBase, string viewName, string columnName, string targetTable, string targetColumn, string information)
		{
			using (NDC.Push(LogHelper.GetNamespaceContext()))
			{
				try
				{
					ITableObject viewObject = Objects[dataBase + "." + viewName];
					int parentColumnId = 0;
					if (viewObject != null && viewObject.Columns != null && viewObject.Columns.Contains(columnName))
					{
						parentColumnId = viewObject.Columns[columnName].Id;
					}
					ITableObject targetObject = Objects[dataBase + "." + targetTable];
					int childColumnId = 0;
					int informationColumn = 0;
					if (targetObject != null && targetObject.Columns != null)
					{
						if (targetObject.Columns.Contains(targetColumn))
						{
							childColumnId = targetObject.Columns[targetColumn].Id;
						}
						if (targetObject.Columns.Contains(information))
						{
							informationColumn = targetObject.Columns[information].Id;
						}
					}
					using DatabaseBase conn = _cmanager.GetConnection();
					List<ExtendedColumnInformation> settings = conn.DbMapping.Load<ExtendedColumnInformation>($"parent = {parentColumnId}");
					if (settings != null)
					{
						foreach (ExtendedColumnInformation extendedColumnInformation in settings)
						{
							_log.Info($"Row delete : {extendedColumnInformation.Id}");
							conn.DbMapping.Delete(extendedColumnInformation);
						}
					}
					ExtendedColumnInformation extendedInformation = new ExtendedColumnInformation
					{
						ParentColumnId = parentColumnId,
						ChildColumnId = childColumnId,
						InformationColumnId = informationColumn
					};
					_log.Info($"Row save parent : {extendedInformation.ParentColumnId}");
					conn.DbMapping.Save(extendedInformation);
				}
				catch (Exception ex)
				{
					_log.Error(ex.Message, ex);
					throw;
				}
			}
		}

		public int FileInArchiveData(string system, string fileName, string column)
		{
			using (NDC.Push(LogHelper.GetNamespaceContext()))
			{
				try
				{
					ITableObject archiveFile = Objects.First((ITableObject s) => s.Type == TableType.Archive && string.CompareOrdinal(s.Database.ToLower(), system.ToLower()) == 0);
					if (archiveFile == null)
					{
						throw new NullReferenceException("Archive does not exist.");
					}
					using DatabaseBase conn = _cmanager.GetConnection();
					StringBuilder sb = new StringBuilder();
					sb.Append("SELECT COUNT(1) FROM ").Append(conn.Enquote(archiveFile.Database, archiveFile.TableName)).Append(" WHERE `")
						.Append(column)
						.Append("` LIKE \"%")
						.Append(fileName)
						.Append("%\"");
					return Convert.ToInt32(conn.ExecuteScalar(sb.ToString()));
				}
				catch (Exception ex)
				{
					_log.Error(ex.Message, ex);
					throw;
				}
			}
		}

		public void CheckFilesInArchiveData(string system, string fileFolder, string thumbnailFolder)
		{
			using (NDC.Push(LogHelper.GetNamespaceContext()))
			{
				try
				{
					ITableObject archiveFile = Objects.First((ITableObject s) => s.Type == TableType.Archive && string.CompareOrdinal(s.Database.ToLower(), system.ToLower()) == 0);
					if (archiveFile == null)
					{
						throw new NullReferenceException("Archive does not exist.");
					}
					using DatabaseBase conn = _cmanager.GetConnection();
					string sqlQuerry = "SELECT path,thumbnail_path FROM " + conn.Enquote(archiveFile.Database, archiveFile.TableName);
					using IDataReader reader = conn.ExecuteReader(sqlQuerry);
					while (reader.Read())
					{
						string fileName = reader.GetString(0);
						string thumnailPath = reader.GetString(1);
						if (!File.Exists(fileFolder + fileName))
						{
							_log.Info($"Missing archive file: {fileName}");
						}
						if (!File.Exists(thumbnailFolder + thumnailPath))
						{
							_log.Info($"Missing thumbnail file: {thumnailPath}");
						}
					}
				}
				catch (Exception ex)
				{
					_log.Error(ex.Message, ex);
					throw;
				}
			}
		}

		public void RenameThumbnailFiles(string system, string fileFolder, string thumbnailFolder)
		{
			using (NDC.Push(LogHelper.GetNamespaceContext()))
			{
				try
				{
					ITableObject archiveFile = Objects.First((ITableObject s) => s.Type == TableType.Archive && string.CompareOrdinal(s.Database.ToLower(), system.ToLower()) == 0);
					if (archiveFile == null)
					{
						throw new NullReferenceException("Archive does not exist.");
					}
					using DatabaseBase conn = _cmanager.GetConnection();
					string sqlQuerry = "SELECT path,thumbnail_path FROM " + conn.Enquote(archiveFile.Database, archiveFile.TableName);
					using IDataReader reader = conn.ExecuteReader(sqlQuerry);
					while (reader.Read())
					{
						string fileName = reader.GetString(0);
						string thumbnailPath = reader.GetString(1);
						if (File.Exists(thumbnailFolder + fileName.Replace(".FAX", ".JPG")))
						{
							File.Move(thumbnailFolder + fileName.Replace(".FAX", ".JPG"), thumbnailFolder + thumbnailPath);
							_log.Info($"File renamed : {thumbnailPath}");
						}
					}
				}
				catch (Exception ex)
				{
					_log.Error(ex.Message, ex);
					throw;
				}
			}
		}

		public int GetMaxTableObjectOrdinal(TableType type)
		{
			List<ITable> tablesInViewbox = Tables.Where((ITable w) => w.Type == type).ToList();
			if (!tablesInViewbox.Any())
			{
				return 0;
			}
			return tablesInViewbox.Max((ITable w) => w.Ordinal);
		}

		public bool AreIndexesPopulated()
		{
			using DatabaseBase conn = _cmanager.GetConnection();
			conn.DbMapping.CreateTableIfNotExists<Index>();
			return conn.CountTable("indexes") > 0;
		}

		public void PopulateWithIndexes()
		{
			PopulateWithIndexes(DB, null);
		}

		public void PopulateWithIndexes(DatabaseBase viewboxConn, Action addProgressBarStep)
		{
			_indexes.Clear();
			viewboxConn.DbMapping.CreateTableIfNotExists<Index>();
			viewboxConn.DbMapping.CreateTableIfNotExists<IndexColumnMapping>();
			foreach (ITableObject tableObject in Objects)
			{
				addProgressBarStep?.Invoke();
				CreateIndexesForTable(DB, tableObject);
			}
		}

		public void CreateIndexesForTable(DatabaseBase conn, string name)
		{
			ITableObject tableObject = Objects.FirstOrDefault((ITableObject o) => o.TableName.Equals(name, StringComparison.InvariantCultureIgnoreCase));
			CreateIndexesForTable(conn, tableObject);
		}

		private void CreateIndexesForTable(DatabaseBase conn, ITableObject tableObject)
		{
			if (tableObject == null)
			{
				return;
			}
			IDbIndexInfoCollection dbIndexes = conn.GetIndexesFromTableName(tableObject.Database, tableObject.TableName);
			if (dbIndexes == null)
			{
				_log.Warn($"Indexes cannot be pulled from missing table. ({tableObject.Database}.{tableObject.TableName})");
				return;
			}
			foreach (IIndex index in IndexCollection.CreateFromDbIndexObject(dbIndexes, Objects.Single((ITableObject o) => o.Database.ToLower() == tableObject.Database.ToLower() && o.TableName.ToLower() == tableObject.TableName.ToLower())))
			{
				conn.DbMapping.Save(index);
				conn.DbMapping.Save(typeof(IndexColumnMapping), index.IndexColumnMappings);
			}
		}

		public void LoadIndexesObjects()
		{
			LoadIndexesObjects(DB);
		}

		public void LoadIndexesObjects(DatabaseBase db)
		{
			_indexes.Clear();
			db.DbMapping.CreateTableIfNotExists<Index>();
			db.DbMapping.CreateTableIfNotExists<IndexColumnMapping>();
			List<Index> list = db.DbMapping.Load<Index>();
			List<IndexColumnMapping> indexColumnMapping = db.DbMapping.Load<IndexColumnMapping>();
			foreach (Index index in list)
			{
				IEnumerable<IndexColumnMapping> tmpMapping = indexColumnMapping.Where((IndexColumnMapping im) => im.IndexId == index.Id);
				if (Objects.Contains(index.TableId))
				{
					IEnumerable<IColumn> columns = Objects[index.TableId].Columns.Where((IColumn c) => tmpMapping.Any((IndexColumnMapping m) => m.ColumnId == c.Id));
					_indexes.Add(index, columns);
					Objects[index.TableId].Indexes.Add(index);
				}
				else
				{
					_log.Warn("No object (table/view/archive) found in _objects for id " + index.TableId);
				}
			}
		}

		public void DropIndexes(DatabaseBase db)
		{
			_indexes.Clear();
			foreach (ITableObject @object in _objects)
			{
				@object.Indexes.Clear();
			}
			db.DbMapping.DropTableIfExists<Index>();
			db.DbMapping.DropTableIfExists<IndexColumnMapping>();
		}

		private IColumn GetOptimizationColumnForJoinTempTable(IColumnCollection cols, string optColName)
		{
			string optColUnderscoreOne = optColName + "_1";
			IColumn col = cols.FirstOrDefault((IColumn c) => c.Name == optColUnderscoreOne);
			if (col == null)
			{
				string optColUnderscoreTwo = optColName + "_2";
				col = cols.FirstOrDefault((IColumn c) => c.Name == optColUnderscoreTwo);
			}
			return col;
		}

		public ITableObject CreateTemporaryJoinTableObject(ITableObject table, string database, IColumnCollection columns1, IColumnCollection columns2, string tableName, Dictionary<string, string> descriptions, IEnumerable<ILanguage> languages, string saveName = null, int? forcedTableId = null)
		{
			TableObject tobj = table.Clone() as TableObject;
			lock (this)
			{
				tobj.Database = database;
				tobj.Id = (forcedTableId.HasValue ? forcedTableId.Value : tableId);
				tobj.TableName = (string.IsNullOrWhiteSpace(saveName) ? tableName : saveName);
				tobj.DefaultScheme = Scheme.Default;
				(tobj.Descriptions as LocalizedTextCollection).Clear();
				tobj.Relations = new TableObject.RelationCollection();
				tobj.PageSystem = null;
				foreach (KeyValuePair<string, string> d in descriptions)
				{
					ILanguage language = languages.First((ILanguage l) => l.CountryCode == d.Key);
					if (language != null)
					{
						tobj.SetDescription(d.Value, language);
					}
				}
				foreach (IColumn c2 in columns1)
				{
					IColumn col2 = CreateTemporaryColumn(c2, c2.Name + "_1", tobj.Columns.Count);
					col2.IsVisible = c2.IsVisible;
					col2.Table = tobj;
					tobj.Columns.Add(col2);
				}
				foreach (IColumn c in columns2)
				{
					IColumn col = CreateTemporaryColumn(c, c.Name + "_2", tobj.Columns.Count);
					col.IsVisible = c.IsVisible;
					col.Table = tobj;
					tobj.Columns.Add(col);
				}
				OrderAreaCollection newOrderAreas = new OrderAreaCollection(tobj);
				using (DatabaseBase conn = _cmanager.GetConnection())
				{
					foreach (global::SystemDb.Internal.OrderArea oa in (OrderAreaCollection)tobj.OrderAreas.Clone())
					{
						StringBuilder whereValue = new StringBuilder();
						bool HaswhereValue = false;
						if (!string.IsNullOrEmpty(oa.IndexValue))
						{
							string columnName3 = string.Empty;
							try
							{
								columnName3 = tobj.Columns.First((IColumn item) => item.OptimizationType == OptimizationType.IndexTable && item.Name.EndsWith("_1")).Name;
							}
							catch
							{
							}
							if (!string.IsNullOrEmpty(columnName3))
							{
								whereValue.Append(conn.Enquote(columnName3) + " = " + oa.IndexValue);
								HaswhereValue = true;
							}
						}
						if (!string.IsNullOrEmpty(oa.SplitValue))
						{
							string columnName2 = string.Empty;
							try
							{
								columnName2 = tobj.Columns.First((IColumn item) => item.OptimizationType == OptimizationType.SplitTable && item.Name.EndsWith("_1")).Name;
							}
							catch
							{
							}
							if (HaswhereValue)
							{
								whereValue.Append(" AND ");
							}
							if (!string.IsNullOrEmpty(columnName2))
							{
								whereValue.Append(conn.Enquote(columnName2) + " = " + oa.SplitValue);
								HaswhereValue = true;
							}
						}
						if (!string.IsNullOrEmpty(oa.SortValue))
						{
							string columnName = string.Empty;
							try
							{
								columnName = tobj.Columns.First((IColumn item) => item.OptimizationType == OptimizationType.SortColumn && item.Name.EndsWith("_1")).Name;
							}
							catch
							{
							}
							if (HaswhereValue)
							{
								whereValue.Append(" AND ");
							}
							if (!string.IsNullOrEmpty(columnName))
							{
								whereValue.Append(conn.Enquote(columnName) + " = " + oa.SortValue);
								HaswhereValue = true;
							}
						}
						string db = conn.Enquote(database) + "." + conn.Enquote(tableName);
						string whereSql = string.Empty;
						if (whereValue.Length > 0)
						{
							whereSql = $"WHERE {whereValue.ToString()}";
						}
						string sql = $"SELECT min(_row_no_), max(_row_no_) FROM {db} {whereSql};";
						using IDataReader r = conn.ExecuteReader(sql);
						while (r.Read())
						{
							if (int.TryParse(r.GetValue(0).ToString(), out var start) && int.TryParse(r.GetValue(1).ToString(), out var end))
							{
								global::SystemDb.Internal.OrderArea noa = new global::SystemDb.Internal.OrderArea();
								noa.Id = 0;
								noa.IndexValue = oa.IndexValue;
								noa.LanguageValue = oa.LanguageValue;
								noa.SortValue = oa.SortValue;
								noa.SplitValue = oa.SplitValue;
								noa.Start = start;
								noa.TableId = tobj.Id;
								noa.End = end;
								newOrderAreas.Add(noa);
							}
						}
					}
				}
				tobj.OrderAreas = newOrderAreas;
				if (tobj.SplitTableColumn != null)
				{
					tobj.SplitTableColumn = GetOptimizationColumnForJoinTempTable(tobj.Columns, tobj.SplitTableColumn.Name);
				}
				if (tobj.IndexTableColumn != null)
				{
					tobj.IndexTableColumn = GetOptimizationColumnForJoinTempTable(tobj.Columns, tobj.IndexTableColumn.Name);
				}
				if (tobj.SortColumn != null)
				{
					tobj.SortColumn = GetOptimizationColumnForJoinTempTable(tobj.Columns, tobj.SortColumn.Name);
				}
				if (!forcedTableId.HasValue)
				{
					tableId--;
					return tobj;
				}
				return tobj;
			}
		}

		public ITableObject CreateTemporaryGroupTableObject(ITableObject table, string database, List<Tuple<IColumn, string, string>> columns, string tableName, Dictionary<string, string> descriptions, IEnumerable<ILanguage> languages, Dictionary<Tuple<ILanguage, string>, string> aggDescriptions, string saveName = null)
		{
			TableObject tobj = table.Clone() as TableObject;
			lock (this)
			{
				tobj.Database = database;
				tobj.Id = tableId;
				tobj.TableName = (string.IsNullOrWhiteSpace(saveName) ? tableName : saveName);
				tobj.DefaultScheme = Scheme.Default;
				(tobj.Descriptions as LocalizedTextCollection).Clear();
				(tobj.OrderAreas as OrderAreaCollection).Clear();
				tobj.SortColumn = null;
				tobj.SplitTableColumn = null;
				tobj.IndexTableColumn = null;
				tobj.UserDefined = true;
				tobj.Relations = new TableObject.RelationCollection();
				tobj.PageSystem = null;
				foreach (ILanguage j in languages)
				{
					tobj.SetDescription(descriptions[j.CountryCode], j);
				}
				foreach (Tuple<IColumn, string, string> c in columns)
				{
					IColumn col = CreateTemporaryColumn(c.Item1, c.Item2, tobj.Columns.Count);
					if (!string.IsNullOrEmpty(c.Item3))
					{
						foreach (ILanguage i in languages)
						{
							if (!string.IsNullOrEmpty(col.Descriptions[i]))
							{
								col.SetDescription(col.Descriptions[i] + " (" + aggDescriptions[new Tuple<ILanguage, string>(i, c.Item3.ToLower())] + ")", i);
							}
						}
					}
					col.Table = tobj;
					tobj.Columns.Add(col);
				}
				tableId--;
				return tobj;
			}
		}

		public ITableObject CreateTemporaryTableObject(ITableObject table, IColumnCollection columns, bool originalColumnIds = false)
		{
			TableObject tobj = table.Clone() as TableObject;
			lock (this)
			{
				Dictionary<int, int> _columns = new Dictionary<int, int>();
				foreach (IColumn c2 in columns)
				{
					IColumn col = CreateTemporaryColumn(c2, originalColumnIds);
					col.Table = tobj;
					tobj.Columns.Add(col);
					_columns.Add(c2.Id, col.Id);
				}
				TableObject.RelationCollection relations = new TableObject.RelationCollection();
				foreach (IRelation relation in table.Relations)
				{
					TableObject.Relation rel = new TableObject.Relation();
					foreach (IColumnConnection c in relation)
					{
						if (_columns.ContainsKey(c.Source.Id))
						{
							rel.Add(new TableObject.ColumnConnection
							{
								Source = tobj.Columns[_columns[c.Source.Id]],
								Target = c.Target,
								Operator = c.Operator,
								FullLine = c.FullLine,
								RelationType = c.RelationType,
								ExtInfo = c.ExtInfo,
								ColumnExtInfo = c.ColumnExtInfo,
								RelationId = c.RelationId,
								UserDefined = c.UserDefined
							});
							rel.RelationId = c.RelationId;
						}
					}
					relations.Add(rel);
				}
				tobj.Relations = relations;
				tobj.Id = tableId;
				tableId--;
				return tobj;
			}
		}

		public void CopyColumns(IFullColumnCollection fromcolumns, out IFullColumnCollection toColumns)
		{
			toColumns = new FullColumnCollection();
			foreach (IColumn col in fromcolumns)
			{
				(toColumns as FullColumnCollection).Add(col);
			}
		}

		public void CopyTableObjects(ITableObjectCollection tableObjects, out ITableObjectCollection toTables)
		{
			toTables = new TableObjectCollection();
			foreach (ITableObject tableObject in tableObjects)
			{
				(toTables as TableObjectCollection).Add(tableObject);
			}
		}

		public void CopyColumnsToUserObjects(IFullColumnCollection fromColumns, IUserObjects userObjects)
		{
			foreach (IColumn fromColumn in fromColumns)
			{
				if (!(userObjects.Columns as FullColumnCollection).ContainsKey(fromColumn.Id))
				{
					(userObjects.Columns as FullColumnCollection).Add(fromColumn);
				}
			}
		}

		public void CopyTableObjectsToUserObjects(ITableObjectCollection tableObjects, IUserObjects userObjects)
		{
			foreach (ITableObject tableObject in tableObjects)
			{
				TableObjectCollection to = userObjects.TableObjects as TableObjectCollection;
				if (!to.Contains(tableObject.Id))
				{
					to.Add(tableObject);
				}
			}
		}

		public TableCounts GetObjectCounts(TableType type, IUser user, string system, string search, ILanguage language, IOptimization optimization, int? objectTypeFilter = null, int? extendedObjectTypeFilter = null)
		{
			TableCounts tableCount = default(TableCounts);
			if (system == null)
			{
				return tableCount;
			}
			IOptimizationCollection allOptimizations = GetOptimizations(user);
			IEnumerable<ITableObject> visibleTables = Objects.Where((ITableObject tobj) => (search == null || (tobj.Descriptions[language ?? DefaultLanguage] != null && tobj.Descriptions[language ?? DefaultLanguage].ToLower().Contains(search.ToLower())) || (tobj.Descriptions[language ?? DefaultLanguage] == null && tobj.TableName.ToLower().Contains(search.ToLower())) || (tobj.TransactionNumber != null && tobj.TransactionNumber.Contains(search))) && tobj.Database.ToLower() == system.ToLower() && tobj.Type == type && (user.IsSuper || (allOptimizations.Count > 0 && GetUserRightToTableObject(tobj, user) > RightType.None)) && (type != TableType.Issue || user.IsSuper || (allOptimizations.Count > 0 && ((tobj as IIssue).Flag > 0 || GetUserRightToTableObject(tobj, user) > RightType.None))) && (type == TableType.Issue || GetDataCount(tobj, optimization, tobj, user, MultiOptimizations: false, null, writeToDatabase: false) > 0) && (type == TableType.Issue || !tobj.IsArchived) && GetIsVisibleOfTableByUser(tobj, user) && (!objectTypeFilter.HasValue || !objectTypeFilter.HasValue || objectTypeFilter.Value == ((TableObject)tobj).ObjectTypeCode) && (!extendedObjectTypeFilter.HasValue || !extendedObjectTypeFilter.HasValue || ((TableObject)tobj).ExtendedObjectType.Contains(extendedObjectTypeFilter.Value)));
			tableCount.VisibleTableCount = visibleTables.Count();
			tableCount.VisibleTables = visibleTables;
			IEnumerable<ITableObject> invisibleTables = Objects.Where((ITableObject tobj) => (search == null || (tobj.Descriptions[language ?? DefaultLanguage] != null && tobj.Descriptions[language ?? DefaultLanguage].ToLower().Contains(search.ToLower())) || (tobj.Descriptions[language ?? DefaultLanguage] == null && tobj.TableName.ToLower().Contains(search.ToLower())) || (tobj.TransactionNumber != null && tobj.TransactionNumber.Contains(search))) && tobj.Database.ToLower() == system.ToLower() && tobj.Type == type && (user.IsSuper || (allOptimizations.Count > 0 && GetUserRightToTableObject(tobj, user) > RightType.None)) && (type != TableType.Issue || user.IsSuper || (allOptimizations.Count > 0 && ((tobj as IIssue).Flag > 0 || GetUserRightToTableObject(tobj, user) > RightType.None))) && (type == TableType.Issue || GetDataCount(tobj, optimization, tobj, user, MultiOptimizations: false, null, writeToDatabase: false) > 0) && (type == TableType.Issue || !tobj.IsArchived) && !GetIsVisibleOfTableByUser(tobj, user) && (!objectTypeFilter.HasValue || !objectTypeFilter.HasValue || objectTypeFilter.Value == ((TableObject)tobj).ObjectTypeCode) && (!extendedObjectTypeFilter.HasValue || !extendedObjectTypeFilter.HasValue || ((TableObject)tobj).ExtendedObjectType.Contains(extendedObjectTypeFilter.Value)));
			tableCount.InVisibleTableCount = invisibleTables.Count();
			tableCount.InVisibleTables = invisibleTables;
			tableCount.EmptyTableCount = 0;
			if (type != TableType.Issue)
			{
				IEnumerable<ITableObject> emptyTables = Objects.Where((ITableObject tobj) => (search == null || (tobj.Descriptions[language ?? DefaultLanguage] != null && tobj.Descriptions[language ?? DefaultLanguage].ToLower().Contains(search.ToLower())) || (tobj.Descriptions[language ?? DefaultLanguage] == null && tobj.TableName.ToLower().Contains(search.ToLower())) || (tobj.TransactionNumber != null && tobj.TransactionNumber.Contains(search))) && tobj.Database.ToLower() == system.ToLower() && tobj.Type == type && (user.IsSuper || (allOptimizations.Count > 0 && GetUserRightToTableObject(tobj, user) > RightType.None)) && (type != TableType.Issue || user.IsSuper || (allOptimizations.Count > 0 && ((tobj as IIssue).Flag > 0 || GetUserRightToTableObject(tobj, user) > RightType.None))) && GetDataCount(tobj, optimization, tobj, user, MultiOptimizations: false, null, writeToDatabase: false) < 1 && !tobj.IsArchived && GetIsVisibleOfTableByUser(tobj, user) && (!objectTypeFilter.HasValue || !objectTypeFilter.HasValue || objectTypeFilter.Value == ((TableObject)tobj).ObjectTypeCode) && (!extendedObjectTypeFilter.HasValue || !extendedObjectTypeFilter.HasValue || ((TableObject)tobj).ExtendedObjectType.Contains(extendedObjectTypeFilter.Value)));
				tableCount.EmptyTableCount = emptyTables.Count();
				tableCount.EmptyTables = emptyTables;
				IEnumerable<ITableObject> emptyInVisibleTables = Objects.Where((ITableObject tobj) => (search == null || (tobj.Descriptions[language ?? DefaultLanguage] != null && tobj.Descriptions[language ?? DefaultLanguage].ToLower().Contains(search.ToLower())) || (tobj.Descriptions[language ?? DefaultLanguage] == null && tobj.TableName.ToLower().Contains(search.ToLower())) || (tobj.TransactionNumber != null && tobj.TransactionNumber.Contains(search))) && tobj.Database.ToLower() == system.ToLower() && tobj.Type == type && (user.IsSuper || (allOptimizations.Count > 0 && GetUserRightToTableObject(tobj, user) > RightType.None)) && (type != TableType.Issue || user.IsSuper || (allOptimizations.Count > 0 && ((tobj as IIssue).Flag > 0 || GetUserRightToTableObject(tobj, user) > RightType.None))) && (type == TableType.Issue || !GetIsVisibleOfTableByUser(tobj, user)) && (type == TableType.Issue || GetDataCount(tobj, optimization, tobj, user, MultiOptimizations: false, null, writeToDatabase: false) <= 0) && !tobj.IsArchived && (!objectTypeFilter.HasValue || !objectTypeFilter.HasValue || objectTypeFilter.Value == ((TableObject)tobj).ObjectTypeCode) && (!extendedObjectTypeFilter.HasValue || !extendedObjectTypeFilter.HasValue || ((TableObject)tobj).ExtendedObjectType.Contains(extendedObjectTypeFilter.Value)));
				tableCount.EmptyInVisibleTableCount = emptyInVisibleTables.Count();
				tableCount.EmptyInVisibleTables = emptyInVisibleTables;
				IEnumerable<ITableObject> archivedTables = from tobj in Objects
					let getObjectType = tobj.ObjectTypes[language ?? DefaultLanguage]
					where (search == null || (tobj.Descriptions[language ?? DefaultLanguage] != null && tobj.Descriptions[language ?? DefaultLanguage].ToLower().Contains(search.ToLower())) || (tobj.Descriptions[language ?? DefaultLanguage] == null && tobj.TableName.ToLower().Contains(search.ToLower())) || (tobj.TransactionNumber != null && tobj.TransactionNumber.ToString().Contains(search))) && tobj.Database.ToLower() == system.ToLower() && tobj.Type == type && (user.IsSuper || (allOptimizations.Count > 0 && GetUserRightToTableObject(tobj, user) > RightType.None)) && (type != TableType.Issue || user.IsSuper || (allOptimizations.Count > 0 && ((tobj as IIssue).Flag > 0 || GetUserRightToTableObject(tobj, user) > RightType.None))) && tobj.IsArchived && (!objectTypeFilter.HasValue || !objectTypeFilter.HasValue || objectTypeFilter.Value == ((TableObject)tobj).ObjectTypeCode) && (!extendedObjectTypeFilter.HasValue || !extendedObjectTypeFilter.HasValue || ((TableObject)tobj).ExtendedObjectType.Contains(extendedObjectTypeFilter.Value))
					select tobj;
				tableCount.ArchivedTableCount = archivedTables.Count();
				tableCount.ArchivedTables = archivedTables;
			}
			return tableCount;
		}

		public long GetDataCount(ITableObject table, IOptimization opt, ITableObject original = null, IUser user = null, bool MultiOptimizations = false, IDictionary<int, Tuple<int, int>> OptimizationSelected = null, bool writeToDatabase = true)
		{
			if (table.RowCount < 0)
			{
				return -1L;
			}
			if (table.RowCount == 0L)
			{
				return 0L;
			}
			List<IOrderArea> orderArealist = new List<IOrderArea>();
			long count = 0L;
			string indexValue = opt.FindValue(OptimizationType.IndexTable);
			string splitValue = opt.FindValue(OptimizationType.SplitTable);
			string sortValue = opt.FindValue(OptimizationType.SortColumn);
			string languageValue = ((user == null) ? Languages.FirstOrDefault().CountryCode : user.CurrentLanguage);
			orderArealist.AddRange(table.OrderAreas.GetOrderAreas(languageValue, indexValue, splitValue, sortValue));
			foreach (IOrderArea o in orderArealist)
			{
				count += o.End - o.Start + 1;
			}
			if (table.OrderAreas.Count == 0)
			{
				count = table.RowCount;
			}
			if (table.PageSystem != null)
			{
				table.PageSystem.Conn = ConnectionManager.CreateConnection(_cmanager.DbConfig);
				table.PageSystem.UpdateSQL(table, original, orderArealist, user, _optimizations, MultiOptimizations, OptimizationSelected);
				table.PageSystem.CountTable(_cmanager.GetConnection(), table);
				return table.PageSystem.Count;
			}
			if (original != null && original.RoleBasedFilters != null)
			{
				PageSystem pageSystem = new PageSystem("", opt.Id, "");
				pageSystem.UpdateSQL(table, original, orderArealist, user, _optimizations, MultiOptimizations, OptimizationSelected);
				pageSystem.CountTable(_cmanager.GetConnection(), table);
				return pageSystem.Count;
			}
			return count;
		}

		public long GetSubtotalDataCount(string sql)
		{
			long count = 0L;
			using DatabaseBase conn = _cmanager.GetConnection();
			return long.Parse(conn.ExecuteScalar(sql).ToString());
		}

		public IColumnCollection GetColumnList(ITableObject table, IOptimization opt, int tableId, IUser user = null)
		{
			IColumnCollection columns = table.Columns;
			using DatabaseBase conn = _cmanager.GetConnection();
			string sql2 = "SELECT is_visible,table_id,data_type,original_name,optimization_type,is_empty,max_length,id,name,user_defined,const_value,ordinal,";
			sql2 += string.Format("from_column,from_column_format, flag, param_operator FROM `{0}`.`{1}` where table_id = {2};", conn.DbConfig.DbName, "columns", table.Id);
			using IDataReader reader = conn.ExecuteReader(sql2);
			while (reader.Read())
			{
				global::SystemDb.Internal.Column column = new global::SystemDb.Internal.Column
				{
					IsVisible = (!reader.IsDBNull(0) && Convert.ToBoolean(reader[0])),
					IsTempedHidden = false,
					TableId = ((!reader.IsDBNull(1)) ? reader.GetInt32(1) : 0),
					DataType = ((!reader.IsDBNull(2)) ? ((SqlType)Enum.Parse(typeof(SqlType), Convert.ToInt32(reader[2]).ToString(CultureInfo.InvariantCulture))) : SqlType.String),
					OriginalName = (reader.IsDBNull(3) ? null : reader.GetString(3)),
					OptimizationType = ((!reader.IsDBNull(4)) ? ((OptimizationType)Enum.Parse(typeof(OptimizationType), Convert.ToInt32(reader[4]).ToString(CultureInfo.InvariantCulture))) : OptimizationType.NotSet),
					IsEmpty = (!reader.IsDBNull(5) && Convert.ToBoolean(reader[5])),
					MaxLength = ((!reader.IsDBNull(6)) ? reader.GetInt32(6) : 0),
					Id = ((!reader.IsDBNull(7)) ? reader.GetInt32(7) : 0),
					Name = (reader.IsDBNull(8) ? null : reader.GetString(8)),
					UserDefined = (!reader.IsDBNull(9) && Convert.ToBoolean(reader[9])),
					ConstValue = (reader.IsDBNull(10) ? null : reader.GetString(10)),
					Ordinal = ((!reader.IsDBNull(11)) ? reader.GetInt32(11) : 0),
					FromColumn = (reader.IsDBNull(12) ? null : reader.GetString(12)),
					FromColumnFormat = (reader.IsDBNull(13) ? null : reader.GetString(13)),
					Flag = (short)((!reader.IsDBNull(14)) ? reader.GetInt16(14) : 0),
					ParamOperator = ((!reader.IsDBNull(15)) ? reader.GetInt32(15) : 0)
				};
				columns.Add(column);
			}
			return columns;
		}

		public void AddTableObjectsForExport(TableType type, IUser user, ITableCollection tables, IViewCollection views, ITableObjectCollection tableObjectCollection, string database, string search, int page, int size, int sortColumn, bool direction, ILanguage language, IOptimization optimization, out int count)
		{
			count = 0;
			if (database == null)
			{
				return;
			}
			IOptimizationCollection AllOptimizations = GetOptimizations(user);
			switch (type)
			{
			case TableType.Table:
				(tables as TableCollection).Clear();
				break;
			case TableType.View:
				(views as ViewCollection).Clear();
				break;
			}
			IUserTableObjectSettingsCollection userTableObjectSettings = UserTableObjectSettings;
			IEnumerable<ITableObject> tobjs = Objects.Where((ITableObject tobj) => !tobj.IsArchived && tobj.Type == type && tobj.Database.Equals(database, StringComparison.OrdinalIgnoreCase) && (search == null || (type == TableType.View && tobj.Descriptions[language ?? DefaultLanguage] != null && tobj.Descriptions[language ?? DefaultLanguage].IndexOf(search, StringComparison.OrdinalIgnoreCase) != -1) || (type == TableType.Table && tobj.TableName.IndexOf(search, StringComparison.OrdinalIgnoreCase) != -1)) && ((userTableObjectSettings[user, tobj] != null) ? userTableObjectSettings[user, tobj].IsVisible : tobj.IsVisible) && (user.IsSuper || (AllOptimizations.Count > 0 && GetUserRightToTableObject(tobj, user) > RightType.None)));
			tobjs = sortColumn switch
			{
				1 => (!direction) ? tobjs.OrderByDescending((ITableObject t) => t.Descriptions[language ?? DefaultLanguage] ?? t.TableName) : tobjs.OrderBy((ITableObject t) => t.Descriptions[language ?? DefaultLanguage] ?? t.TableName), 
				2 => (!direction) ? tobjs.OrderByDescending((ITableObject t) => GetDataCount(t, optimization, t, user)) : tobjs.OrderBy((ITableObject t) => GetDataCount(t, optimization, t, user)), 
				_ => tobjs.OrderBy((ITableObject t) => t.Descriptions[language ?? DefaultLanguage] ?? t.TableName), 
			};
			count = tobjs.Count();
			tobjs = tobjs.Skip(page * size).Take(size);
			foreach (ITableObject tableObject in tobjs)
			{
				ITableObject tableObjectClone = tableObject.Clone() as ITableObject;
				tableObjectClone.IsVisible = GetIsVisibleOfTableByUser(tableObject, user);
				switch (type)
				{
				case TableType.Table:
					(tables as TableCollection).Add(tableObjectClone as ITable);
					break;
				case TableType.View:
					(views as ViewCollection).Add(tableObjectClone as IView);
					break;
				}
				if (!tableObjectCollection.Contains(tableObjectClone.Id))
				{
					tableObjectCollection.Add(tableObjectClone);
				}
			}
		}

		public void AddTableObjects(TableType type, IUser user, ITableCollection tables, IViewCollection views, IIssueCollection issues, ITableObjectCollection tableObjects, string system, string search, int page, int size, bool showEmpty, bool showHidden, bool showArchived, int sortColumn, bool direction, ILanguage language, IOptimization optimization, out int fullTableListCount, IEnumerable<int> exclude = null, int? objectTypeFilter = null, int? extendedObjectTypeFilter = null, bool showEmptyHidden = false)
		{
			fullTableListCount = 0;
			if (system == null)
			{
				return;
			}
			IOptimizationCollection allOptimizations = GetOptimizations(user);
			switch (type)
			{
			case TableType.Table:
				(tables as TableCollection).Clear();
				break;
			case TableType.View:
				(views as ViewCollection).Clear();
				break;
			case TableType.Issue:
				(issues as IssueCollection).Clear();
				break;
			}
			IEnumerable<ITableObject> temptobjs = Objects.Where((ITableObject tobj) => (search == null || ((tobj.Descriptions[language ?? DefaultLanguage] != null) ? tobj.Descriptions[language ?? DefaultLanguage].ToLower().Contains(search.ToLower()) : tobj.TableName.ToLower().Contains(search.ToLower())) || tobj.TransactionNumber.ToLower().Contains(search.ToLower())) && tobj.Database.ToLower() == system.ToLower() && tobj.Type == type && (user.IsSuper || (allOptimizations.Count > 0 && GetUserRightToTableObject(tobj, user) > RightType.None)) && (type != TableType.Issue || user.IsSuper || (allOptimizations.Count > 0 && ((tobj as IIssue).Flag > 0 || GetUserRightToTableObject(tobj, user) > RightType.None))) && (type == TableType.Issue || GetDataCount(tobj, optimization, tobj, user, MultiOptimizations: false, null, writeToDatabase: false) > 0 || showEmpty || showEmptyHidden) && (exclude == null || !exclude.Contains(tobj.Id)) && (!objectTypeFilter.HasValue || !objectTypeFilter.HasValue || objectTypeFilter.Value == ((TableObject)tobj).ObjectTypeCode) && (!extendedObjectTypeFilter.HasValue || !extendedObjectTypeFilter.HasValue || ((TableObject)tobj).ExtendedObjectType.Contains(extendedObjectTypeFilter.Value)));
			IEnumerable<ITableObject> tobjs = (showEmptyHidden ? temptobjs.Where((ITableObject t) => (type == TableType.Issue || GetDataCount(t, optimization, t, user, MultiOptimizations: false, null, writeToDatabase: false) <= 0) && !t.IsArchived && !GetIsVisibleOfTableByUser(t, user)) : (showEmpty ? temptobjs.Where((ITableObject t) => (type == TableType.Issue || GetDataCount(t, optimization, t, user, MultiOptimizations: false, null, writeToDatabase: false) < 1) && !t.IsArchived && GetIsVisibleOfTableByUser(t, user)) : (showHidden ? temptobjs.Where((ITableObject t) => !t.IsArchived && !GetIsVisibleOfTableByUser(t, user)) : ((!showArchived) ? temptobjs.Where((ITableObject t) => (type == TableType.Issue || GetDataCount(t, optimization, t, user, MultiOptimizations: false, null, writeToDatabase: false) > 0) && !t.IsArchived && GetIsVisibleOfTableByUser(t, user)) : temptobjs.Where((ITableObject t) => t.IsArchived)))));
			foreach (ITableObject tableObject in tobjs)
			{
				if (_userTableTransactionIdSettings[tableObject.Id, user.Id] != null)
				{
					(tableObject as TableObject).TransactionNumber = _userTableTransactionIdSettings[tableObject.Id, user.Id].TransactionId;
				}
			}
			if (type == TableType.Issue)
			{
				foreach (ITableObject item in tobjs)
				{
					Issue t_asIssue2 = item as Issue;
					if (t_asIssue2 == null)
					{
						continue;
					}
					foreach (IParameter p2 in t_asIssue2.Parameters)
					{
						if (p2.HistoryValues != null)
						{
							p2.HistoryValues.RemoveHandler();
						}
						p2.HistoryValues = new HistoryParameterValueCollection();
						foreach (HistoryParameterValue historyParameterValue in _userHistoryIssueParameterCollection)
						{
							if (historyParameterValue.UserId == user.Id && historyParameterValue.ParameterId == p2.Id && !(p2.HistoryValues as HistoryParameterValueCollection).Contains(historyParameterValue.Name) && !(p2.HistoryValues as HistoryParameterValueCollection).Contains(historyParameterValue.Id))
							{
								(p2.HistoryValues as HistoryParameterValueCollection).Add(historyParameterValue);
							}
						}
					}
				}
				foreach (ITableObject item2 in tobjs)
				{
					Issue t_asIssue = item2 as Issue;
					if (t_asIssue == null)
					{
						continue;
					}
					foreach (IParameter p in t_asIssue.Parameters)
					{
						p.HistoryFreeSelectionValues = new List<IHistoryParameterValueFreeSelection>();
						foreach (HistoryParameterValueFreeSelection historyParameterValue2 in _userHistoryFreeSelectionIssueParameterCollection)
						{
							if (historyParameterValue2.UserId == user.Id && historyParameterValue2.ParameterId == p.Id && p.HistoryFreeSelectionValues.Count((IHistoryParameterValueFreeSelection h) => h.Id == historyParameterValue2.Id) <= 0)
							{
								p.HistoryFreeSelectionValues.Add(historyParameterValue2);
							}
						}
					}
				}
			}
			fullTableListCount = tobjs.Count();
			switch (sortColumn)
			{
			case 1:
				tobjs = ((!direction) ? tobjs.OrderByDescending((ITableObject t) => t.TransactionNumber) : tobjs.OrderBy((ITableObject t) => t.TransactionNumber));
				break;
			case 2:
				tobjs = ((!direction) ? tobjs.OrderByDescending((ITableObject t) => t.Descriptions[language ?? DefaultLanguage] ?? t.TableName) : tobjs.OrderBy((ITableObject t) => t.Descriptions[language ?? DefaultLanguage] ?? t.TableName));
				break;
			case 3:
				tobjs = ((!direction) ? tobjs.OrderByDescending((ITableObject t) => GetDataCount(t, optimization, t, user, MultiOptimizations: false, null, writeToDatabase: false)) : tobjs.OrderBy((ITableObject t) => GetDataCount(t, optimization, t, user, MultiOptimizations: false, null, writeToDatabase: false)));
				break;
			case 4:
				tobjs = ((!direction) ? (from t in tobjs
					orderby t.ObjectTypes[language ?? DefaultLanguage] ?? t.ObjectTypes[DefaultLanguage] ?? string.Empty descending, t.Descriptions[language ?? DefaultLanguage] ?? t.TableName
					select t) : (from t in tobjs
					orderby t.ObjectTypes[language ?? DefaultLanguage] ?? t.ObjectTypes[DefaultLanguage] ?? string.Empty, t.Descriptions[language ?? DefaultLanguage] ?? t.TableName
					select t));
				break;
			default:
			{
				bool ordered = false;
				IProperty prop = GetProperties(user).FirstOrDefault((IProperty item) => item.Key == "table_order");
				if (prop != null && prop.Value.ToLower() == "true")
				{
					Dictionary<int, int> tableOrder = GetTableObjectOrder(user, type);
					if (tableOrder != null)
					{
						ordered = true;
						tobjs = tobjs.OrderBy((ITableObject t) => (!tableOrder.ContainsKey(t.Id)) ? (t.Ordinal + tableOrder.Count) : tableOrder[t.Id]);
					}
				}
				if (!ordered)
				{
					tobjs = tobjs.OrderBy((ITableObject t) => t.Ordinal);
				}
				break;
			}
			}
			if (size > 0)
			{
				tobjs = tobjs.Skip(page * size).Take(size);
			}
			foreach (ITableObject t2 in tobjs)
			{
				ITableObject t_clone = t2.Clone() as ITableObject;
				t_clone.IsVisible = GetIsVisibleOfTableByUser(t2, user);
				switch (type)
				{
				case TableType.Table:
					(tables as TableCollection).Add(t_clone as ITable);
					break;
				case TableType.View:
					(views as ViewCollection).Add(t_clone as IView);
					break;
				case TableType.Issue:
					(issues as IssueCollection).Add(t_clone as IIssue);
					break;
				}
				if (!tableObjects.Contains(t_clone.Id))
				{
					tableObjects.Add(t_clone);
				}
			}
		}

		public void AddTableToTableObjects(IUser user, ITableObjectCollection tableObjects, int id)
		{
			ITableObject obj_clone = Objects[id].Clone() as ITableObject;
			obj_clone.IsVisible = GetIsVisibleOfTableByUser(Objects[id], user);
			if (user.IsSuper)
			{
				tableObjects.Add(obj_clone);
			}
			else if (GetOptimizations(user).Count > 0 && GetUserRightToTableObject(Objects[id], user) > RightType.None)
			{
				tableObjects.Add(obj_clone);
			}
		}

		public void AddTableColumns(IUser user, IFullColumnCollection columns, ITableObjectCollection tableObjects, ITableCollection tables, IViewCollection views, IOptimization optimization, bool useDistinctInfo, int id)
		{
			ITableObject tobj = Objects[id];
			if (tobj == null)
			{
				return;
			}
			int objId = ((id == 0) ? tobj.Id : id);
			if (tobj is Issue)
			{
				Issue tableIssue = tobj as Issue;
				if (tableIssue.OriginalId > 0)
				{
					objId = tableIssue.OriginalId;
				}
			}
			if (_userColumnOrderSettings[user.Id, objId] == null)
			{
				ITableObject orTable = tobj;
				ResetColumnOrder(tobj, user, orTable, id);
			}
			FullColumnCollection cols = columns as FullColumnCollection;
			ITableObject obj_clone = tobj.Clone() as ITableObject;
			Dictionary<int, int> columnOrder = GetColumnOrder(user, tobj);
			Dictionary<int, EmptyDistinctColumn> emptyDistinctColumns = new Dictionary<int, EmptyDistinctColumn>();
			if (tobj.Columns.Count > 0)
			{
				emptyDistinctColumns = LoadDistinctColumnInformation(tobj.Columns.ElementAt(0).Table.Id, optimization);
			}
			if (user.IsSuper)
			{
				foreach (IColumn col2 in tobj.Columns)
				{
					IColumn col_clone2 = col2.Clone() as IColumn;
					col_clone2.IsVisible = ((_userColumnSettings[user, col2] != null) ? _userColumnSettings[user, col2].IsVisible : col2.IsVisible);
					col_clone2.MinValue = (emptyDistinctColumns.ContainsKey(col_clone2.Id) ? emptyDistinctColumns[col_clone2.Id].MinValue : "");
					col_clone2.MaxValue = (emptyDistinctColumns.ContainsKey(col_clone2.Id) ? emptyDistinctColumns[col_clone2.Id].MaxValue : "");
					col_clone2.IsOneDistinct = emptyDistinctColumns.ContainsKey(col_clone2.Id) && emptyDistinctColumns[col_clone2.Id].OneDistinct;
					col_clone2.IsOptEmpty = emptyDistinctColumns.ContainsKey(col_clone2.Id) && emptyDistinctColumns[col_clone2.Id].IsEmpty;
					if (_userColumnSettings[user, col2] == null && !tobj.UserDefined)
					{
						col_clone2.IsVisible = ((!useDistinctInfo) ? ((col2 as global::SystemDb.Internal.Column).OptimizationType != OptimizationType.IndexTable && (col2 as global::SystemDb.Internal.Column).OptimizationType != OptimizationType.SplitTable) : (!col_clone2.IsOneDistinct && !col_clone2.IsOptEmpty)) && col_clone2.IsVisible;
					}
					if (columnOrder != null && columnOrder.ContainsKey(col_clone2.Id) && columnOrder[col_clone2.Id] != col_clone2.Ordinal)
					{
						col_clone2.Ordinal = columnOrder[col_clone2.Id];
					}
					if ((tobj is IIssue && (tobj as IIssue).IssueType != IssueType.Filter) || tobj.Type != TableType.Issue)
					{
						if (cols.ContainsKey(col_clone2.Id))
						{
							col_clone2.ExactMatchUnchecked = cols[col_clone2.Id].ExactMatchUnchecked;
							(columns as FullColumnCollection).Remove(col_clone2.Id);
						}
						if (!cols.ContainsKey(col_clone2.Id))
						{
							(columns as FullColumnCollection).Add(col_clone2);
						}
					}
					if (tableObjects.Contains(id))
					{
						if (!tables.Contains(id) && tableObjects[id].Type == TableType.Table)
						{
							(tables as TableCollection).Add(tableObjects[id] as global::SystemDb.Internal.Table);
						}
						if (!views.Contains(id) && tableObjects[id].Type == TableType.View)
						{
							(views as ViewCollection).Add(tableObjects[id] as View);
						}
						col_clone2.Table = tableObjects[id];
						if (!tableObjects[id].Columns.Contains(col_clone2.Id))
						{
							tableObjects[id].Columns.Add(col_clone2);
						}
					}
					UpdateRelations(tableObjects[id], col_clone2);
				}
			}
			else
			{
				if (GetOptimizations(user).Count <= 0)
				{
					return;
				}
				RightType cat_right = RightType.None;
				foreach (ICategory c in _categories)
				{
					cat_right = _userCategoryRights[user, c];
					foreach (IRole role3 in user.Roles)
					{
						if (RightType.Inherit < cat_right)
						{
							break;
						}
						cat_right = _roleCategoryRights[role3, c];
					}
				}
				if (cat_right == RightType.Inherit)
				{
					cat_right = RightType.None;
				}
				RightType obj_right = _userTableObjectRights[user, tobj];
				foreach (IRole role2 in user.Roles)
				{
					if (RightType.Inherit < obj_right && obj_right != 0)
					{
						break;
					}
					obj_right = _roleTableObjectRights[role2, tobj];
				}
				if (obj_right == RightType.Inherit)
				{
					obj_right = cat_right;
				}
				foreach (IColumn col in tobj.Columns)
				{
					RightType col_right = _userColumnRights[user, col];
					foreach (IRole role in user.Roles)
					{
						if (RightType.Inherit < col_right)
						{
							break;
						}
						col_right = _roleColumnRights[role, col];
					}
					if (col_right == RightType.Inherit)
					{
						col_right = obj_right;
					}
					if (col_right <= RightType.None)
					{
						continue;
					}
					IColumn col_clone = col.Clone() as IColumn;
					col_clone.IsVisible = ((_userColumnSettings[user, col] != null) ? _userColumnSettings[user, col].IsVisible : col.IsVisible);
					col_clone.MinValue = (emptyDistinctColumns.ContainsKey(col_clone.Id) ? emptyDistinctColumns[col_clone.Id].MinValue : "");
					col_clone.MaxValue = (emptyDistinctColumns.ContainsKey(col_clone.Id) ? emptyDistinctColumns[col_clone.Id].MaxValue : "");
					col_clone.IsOneDistinct = emptyDistinctColumns.ContainsKey(col_clone.Id) && emptyDistinctColumns[col_clone.Id].OneDistinct;
					col_clone.IsOptEmpty = emptyDistinctColumns.ContainsKey(col_clone.Id) && emptyDistinctColumns[col_clone.Id].IsEmpty;
					if (columnOrder != null && columnOrder.ContainsKey(col_clone.Id) && columnOrder[col_clone.Id] != col_clone.Ordinal)
					{
						col_clone.Ordinal = columnOrder[col_clone.Id];
					}
					if (((obj_clone is IIssue && (obj_clone as IIssue).IssueType != IssueType.Filter) || obj_clone.Type != TableType.Issue) && !cols.ContainsKey(col_clone.Id))
					{
						(columns as FullColumnCollection).Add(col_clone);
					}
					if (tableObjects.Contains(id))
					{
						if (!tables.Contains(id) && tableObjects[id].Type == TableType.Table)
						{
							(tables as TableCollection).Add(tableObjects[id] as global::SystemDb.Internal.Table);
						}
						if (!views.Contains(id) && tableObjects[id].Type == TableType.View)
						{
							(views as ViewCollection).Add(tableObjects[id] as View);
						}
						col_clone.Table = tableObjects[id];
						if (!tableObjects[id].Columns.Contains(col_clone.Id))
						{
							tableObjects[id].Columns.Add(col_clone);
						}
					}
					UpdateRelations(tableObjects[id], col_clone);
				}
				CleanUpTableObjectsRelations(user, tableObjects);
			}
		}

		public bool GetRoleRightsToColumn(int roleId, int columnId)
		{
			return GetRoleRightsToColumnObject(_roles[roleId], _columns[columnId]) > RightType.None;
		}

		public bool GetUserRightsToColumn(int userId, RightObjectNode node)
		{
			if (node.Parent.Type != UpdateRightType.TableObject)
			{
				return false;
			}
			return GetUserRightsToColumnObject(Objects[node.Parent.Id], _columns[node.Id], _users[userId]) > RightType.None;
		}

		public bool GetUserRightsToColumn(int userId, int columnId)
		{
			return GetUserRightsToColumnObject(_columns[columnId], _users[userId]) > RightType.None;
		}

		public int[] ReadTableTypeIds(TableType type, string system, int userId)
		{
			return (from tobj in Objects
				where tobj.Database.ToLower() == system.ToLower() && tobj.Type == type && GetUserRightToTableObject(tobj, _users[userId]) > RightType.None
				select tobj.Id).ToArray();
		}

		public bool GetUserRightToTableType(TableType type, string system, int userId)
		{
			return Objects.Where((ITableObject tobj) => tobj.Database.ToLower() == system.ToLower() && tobj.Type == type && GetUserRightToTableObject(tobj, _users[userId]) > RightType.None).Any();
		}

		public bool GetRoleRightToTableType(TableType type, string system, int roleId)
		{
			return Objects.Where((ITableObject tobj) => tobj.Database.ToLower() == system.ToLower() && tobj.Type == type && GetRoleRightsToTableObject(tobj, _roles[roleId]) > RightType.None).Any();
		}

		public bool GetRoleRightsToTable(int tableId, int roleId)
		{
			return GetRoleRightsToTableObject(Objects[tableId], _roles[roleId]) > RightType.None;
		}

		public bool GetUserRightsToTable(int tableId, int userId)
		{
			return GetUserRightToTableObject(Objects[tableId], _users[userId]) > RightType.None;
		}

		public bool GetIsVisibleOfTableByUser(ITableObject tobj, IUser user)
		{
			if (tobj.IsVisible && _userTableObjectSettings[user, tobj] != null)
			{
				return _userTableObjectSettings[user, tobj].IsVisible;
			}
			return tobj.IsVisible;
		}

		public IRoleSetting GetRoleSettingOnSpecificType(RoleSettingsType type, int roleId)
		{
			return Roles[roleId].Settings.Get(type, roleId);
		}

		public IUserObjects GetUserObjects(IUser user)
		{
			UserObjects objects = new UserObjects();
			objects.Optimizations = GetOptimizations(user);
			if (user.IsSuper)
			{
				objects.OptimizationGroups = _optimizationGroups;
				foreach (ICategory category in _categories)
				{
					Category cat_clone = category.Clone() as Category;
					Dictionary<int, int> issueOrder2 = GetTableObjectOrder(user, TableType.Issue);
					Dictionary<int, int> viewOrder2 = GetTableObjectOrder(user, TableType.View);
					foreach (ITableObject t2 in category.TableObjects.ToList())
					{
						ITableObject obj_clone2 = t2.Clone() as ITableObject;
						obj_clone2.IsVisible = GetIsVisibleOfTableByUser(t2, user);
						if (obj_clone2.Type == TableType.Issue && issueOrder2 != null && issueOrder2.ContainsKey(obj_clone2.Id) && issueOrder2[obj_clone2.Id] != obj_clone2.Ordinal)
						{
							obj_clone2.Ordinal = issueOrder2[obj_clone2.Id];
						}
						if (obj_clone2.Type == TableType.View && viewOrder2 != null && viewOrder2.ContainsKey(obj_clone2.Id) && viewOrder2[obj_clone2.Id] != obj_clone2.Ordinal)
						{
							obj_clone2.Ordinal = viewOrder2[obj_clone2.Id];
						}
						Dictionary<int, int> columnOrder2 = GetColumnOrder(user, t2);
						foreach (IColumn col2 in t2.Columns)
						{
							IColumn col_clone2 = col2.Clone() as IColumn;
							col_clone2.IsVisible = ((_userColumnSettings[user, col2] != null) ? _userColumnSettings[user, col2].IsVisible : col2.IsVisible);
							if (columnOrder2 != null && columnOrder2.ContainsKey(col_clone2.Id) && columnOrder2[col_clone2.Id] != col_clone2.Ordinal)
							{
								col_clone2.Ordinal = columnOrder2[col_clone2.Id];
							}
							if (obj_clone2.Type == TableType.Archive)
							{
								(obj_clone2.Columns as ColumnCollection).Add(col_clone2);
								(col_clone2 as global::SystemDb.Internal.Column).Table = obj_clone2;
								UpdateRelations(obj_clone2, col_clone2);
							}
						}
						switch (obj_clone2.Type)
						{
						case TableType.Issue:
							objects.IssueCount++;
							break;
						case TableType.Table:
							objects.TableCount++;
							break;
						case TableType.View:
							objects.ViewCount++;
							break;
						case TableType.Archive:
							(objects.Archives as ArchiveCollection).Add(obj_clone2 as IArchive);
							break;
						case TableType.ArchiveDocument:
							(objects.ArchiveDocuments as ArchiveDocumentCollection).Add(obj_clone2 as IArchiveDocument);
							break;
						}
					}
					(objects.Categories as CategoryCollection).Add(cat_clone);
				}
			}
			else if (objects.Optimizations.Count > 0)
			{
				foreach (IOptimization opt in objects.Optimizations)
				{
					if (!objects.OptimizationGroups.Contains(opt.Group))
					{
						(objects.OptimizationGroups as OptimizationGroupCollection).Add(opt.Group);
					}
				}
				foreach (ICategory c in _categories)
				{
					RightType cat_right = _userCategoryRights[user, c];
					foreach (IRole role3 in user.Roles)
					{
						if (RightType.Inherit < cat_right)
						{
							break;
						}
						cat_right = _roleCategoryRights[role3, c];
					}
					if (cat_right == RightType.Inherit)
					{
						cat_right = RightType.None;
					}
					Category cat_clone2 = c.Clone() as Category;
					Dictionary<int, int> issueOrder = GetTableObjectOrder(user, TableType.Issue);
					Dictionary<int, int> viewOrder = GetTableObjectOrder(user, TableType.View);
					foreach (ITableObject t in c.TableObjects)
					{
						RightType obj_right = _userTableObjectRights[user, t];
						if (obj_right == RightType.Inherit)
						{
							bool done = false;
							foreach (IRole role2 in user.Roles)
							{
								RightType previous_obj_right = obj_right;
								obj_right = _roleTableObjectRights[role2, t];
								switch (obj_right)
								{
								case RightType.Write:
									done = true;
									break;
								case RightType.Read:
									if (previous_obj_right == RightType.Write)
									{
										obj_right = previous_obj_right;
									}
									break;
								case RightType.None:
									if (previous_obj_right == RightType.Write || previous_obj_right == RightType.Read)
									{
										obj_right = previous_obj_right;
									}
									break;
								case RightType.Inherit:
									obj_right = previous_obj_right;
									break;
								}
								if (done)
								{
									break;
								}
							}
						}
						if (obj_right == RightType.Inherit)
						{
							obj_right = cat_right;
						}
						ITableObject obj_clone = t.Clone() as ITableObject;
						obj_clone.IsVisible = GetIsVisibleOfTableByUser(t, user);
						if (obj_clone.Type == TableType.Issue && issueOrder != null && issueOrder.ContainsKey(obj_clone.Id) && issueOrder[obj_clone.Id] != obj_clone.Ordinal)
						{
							obj_clone.Ordinal = issueOrder[obj_clone.Id];
						}
						if (obj_clone.Type == TableType.View && viewOrder != null && viewOrder.ContainsKey(obj_clone.Id) && viewOrder[obj_clone.Id] != obj_clone.Ordinal)
						{
							obj_clone.Ordinal = viewOrder[obj_clone.Id];
						}
						Dictionary<int, int> columnOrder = GetColumnOrder(user, t);
						foreach (IColumn col in t.Columns)
						{
							RightType col_right = _userColumnRights[user, col];
							foreach (IRole role in user.Roles)
							{
								if (RightType.Inherit < col_right)
								{
									break;
								}
								col_right = _roleColumnRights[role, col];
							}
							if (col_right == RightType.Inherit)
							{
								col_right = obj_right;
							}
							if (col_right > RightType.None)
							{
								IColumn col_clone = col.Clone() as IColumn;
								col_clone.IsVisible = ((_userColumnSettings[user, col] != null) ? _userColumnSettings[user, col].IsVisible : col.IsVisible);
								if (columnOrder != null && columnOrder.ContainsKey(col_clone.Id) && columnOrder[col_clone.Id] != col_clone.Ordinal)
								{
									col_clone.Ordinal = columnOrder[col_clone.Id];
								}
								(obj_clone.Columns as ColumnCollection).Add(col_clone);
								(col_clone as global::SystemDb.Internal.Column).Table = obj_clone;
								UpdateRelations(obj_clone, col_clone);
							}
						}
						if (obj_clone.Columns.Count > 0 || (obj_clone.Type == TableType.Issue && obj_right > RightType.None))
						{
							switch (obj_clone.Type)
							{
							case TableType.Issue:
								objects.IssueCount++;
								break;
							case TableType.Table:
								objects.TableCount++;
								break;
							case TableType.View:
								objects.ViewCount++;
								break;
							case TableType.Archive:
								(objects.Archives as ArchiveCollection).Add(obj_clone as IArchive);
								break;
							case TableType.ArchiveDocument:
								(objects.ArchiveDocuments as ArchiveDocumentCollection).Add(obj_clone as IArchiveDocument);
								break;
							}
						}
					}
					if (objects.TableCount + objects.ViewCount + objects.IssueCount + objects.Archives.Count > 0)
					{
						(objects.Categories as CategoryCollection).Add(cat_clone2);
					}
					CleanUpRelations(cat_clone2);
				}
			}
			if (objects.IssueCount > 0)
			{
				(objects.IssueCategories as CategoryCollection).Add(objects.Categories[0]);
			}
			if (objects.TableCount > 0)
			{
				(objects.TableCategories as CategoryCollection).Add(objects.Categories[0]);
			}
			if (objects.ViewCount > 0)
			{
				(objects.ViewCategories as CategoryCollection).Add(objects.Categories[0]);
			}
			return objects;
		}

		public void SaveColumnWidthSizes(int id, string widths, IUser user)
		{
			IUserTableColumnWidthsSettings widthSettings = _userTableColumnWidthSettings[user.Id, id];
			if (widthSettings == null)
			{
				widthSettings = new UserTableColumnWidthsSettings
				{
					TableId = id,
					ColumnWidths = widths,
					TableObject = Objects[id],
					UserId = user.Id,
					User = user
				};
			}
			else
			{
				(widthSettings as UserTableColumnWidthsSettings).ColumnWidths = widths;
			}
			using (DatabaseBase db = _cmanager.GetConnection())
			{
				db.DbMapping.Save(widthSettings);
			}
			_userTableColumnWidthSettings.Add(widthSettings);
		}

		public void SaveTransactionNumber(int id, int userId, string transactionNumber)
		{
			IUserTableTransactionIdSettings settings = _userTableTransactionIdSettings[id, userId];
			if (settings == null)
			{
				settings = new UserTableTransactionIdSettings
				{
					TransactionId = transactionNumber,
					TableId = id,
					Table = Objects[id],
					UserId = userId,
					User = Users[userId]
				};
			}
			else
			{
				(settings as UserTableTransactionIdSettings).TransactionId = transactionNumber;
			}
			using (DatabaseBase databaseBase = _cmanager.GetConnection())
			{
				databaseBase.DbMapping.Save(settings);
			}
			_userTableTransactionIdSettings.Add(settings);
			TableTransactions tableTransaction = _tableTransactions.FirstOrDefault((TableTransactions x) => x.TableId == id);
			if (tableTransaction != null)
			{
				tableTransaction.TransactionNumber = transactionNumber;
			}
			else
			{
				tableTransaction = new TableTransactions();
				tableTransaction.TableId = id;
				tableTransaction.TransactionNumber = transactionNumber;
				_tableTransactions.Add(tableTransaction);
			}
			using DatabaseBase db = _cmanager.GetConnection();
			db.DbMapping.Save(tableTransaction);
		}

		public void RemoveColumns(IFullColumnCollection userColumns, IColumnCollection cols)
		{
			foreach (IColumn column in cols)
			{
				if ((userColumns as FullColumnCollection).ContainsKey(column.Id))
				{
					(userColumns as FullColumnCollection).Remove(column.Id);
				}
			}
		}

		public void AddIssues(IEnumerable<int> favIds, IUser user, IIssueCollection issues, ITableObjectCollection tableObjects, string database)
		{
			IEnumerable<ITableObject> temptobjs = Objects.Where((ITableObject tobj) => favIds.Contains(tobj.Id) && tobj.Database.ToLower() == database.ToLower());
			foreach (ITableObject tableObject in temptobjs)
			{
				if (_userTableTransactionIdSettings[tableObject.Id, user.Id] != null)
				{
					(tableObject as TableObject).TransactionNumber = _userTableTransactionIdSettings[tableObject.Id, user.Id].TransactionId;
				}
			}
			using (DatabaseBase db = _cmanager.GetConnection())
			{
				List<HistoryParameterValue> list = new List<HistoryParameterValue>();
				try
				{
					list = db.DbMapping.Load<HistoryParameterValue>("user_id = " + user.Id);
				}
				catch (Exception)
				{
					list = null;
				}
				if (list != null)
				{
					foreach (ITableObject item in temptobjs)
					{
						Issue t_asIssue = item as Issue;
						if (t_asIssue == null)
						{
							continue;
						}
						foreach (IParameter p in t_asIssue.Parameters)
						{
							p.HistoryValues = new HistoryParameterValueCollection();
							HistoryParameterValueCollection collection = p.HistoryValues as HistoryParameterValueCollection;
							foreach (HistoryParameterValue historyParameterValue in from w in list
								where w.ParameterId == p.Id
								orderby w.Ordinal
								select w)
							{
								if (!collection.Any((IHistoryParameterValue item) => string.Compare(item.Name, historyParameterValue.Name, StringComparison.InvariantCultureIgnoreCase) == 0) && !collection.Contains(historyParameterValue.Id))
								{
									collection.Add(historyParameterValue);
								}
							}
						}
					}
				}
			}
			foreach (ITableObject item2 in temptobjs)
			{
				ITableObject t_clone = item2.Clone() as ITableObject;
				if (!issues.Contains(t_clone.Id))
				{
					(issues as IssueCollection).Add(t_clone as IIssue);
				}
				if (!tableObjects.Contains(t_clone.Id))
				{
					tableObjects.Add(t_clone);
				}
			}
		}

		public bool DeleteParameterFreeSelectionHistroy(int userId, int issueId)
		{
			bool success = true;
			try
			{
				using DatabaseBase db = _cmanager.GetConnection();
				db.DbMapping.Delete("user_free_selection_parameter_history", userId, issueId);
				return success;
			}
			catch (Exception ex)
			{
				_log.Error(ex.Message, ex);
				return false;
			}
		}

		public bool DeleteParameterFreeSelectionHistory(int userId, int issueId)
		{
			bool success = true;
			try
			{
				using DatabaseBase db = _cmanager.GetConnection();
				db.DbMapping.Delete("user_free_selection_parameter_history", userId, issueId);
				return success;
			}
			catch (Exception ex)
			{
				_log.Error(ex.Message, ex);
				return false;
			}
		}

		public void ClearPreviousFreeSelectionParameters(int issueId, int userId)
		{
			try
			{
				int[] array = (from h in _userHistoryFreeSelectionIssueParameterCollection.Values
					where h.IssueId == issueId && h.UserId == userId
					select h.Id).ToArray();
				foreach (int r in array)
				{
					_userHistoryFreeSelectionIssueParameterCollection.TryRemove(r, out var _);
				}
			}
			catch (Exception)
			{
			}
		}

		public IHistoryParameterValueFreeSelection AddParameterFreeSelectionHistory(int userId, int parameterId, int issueId, string value, int selectionType)
		{
			using (NDC.Push(LogHelper.GetNamespaceContext()))
			{
				IHistoryParameterValueFreeSelection historyParamter = null;
				try
				{
					historyParamter = new HistoryParameterValueFreeSelection
					{
						UserId = userId,
						IssueId = issueId,
						ParameterId = parameterId,
						SelectionType = selectionType,
						Value = value
					};
					using (DatabaseBase db = _cmanager.GetConnection())
					{
						db.DbMapping.Save(historyParamter);
					}
					_userHistoryFreeSelectionIssueParameterCollection.Add(historyParamter);
				}
				catch (Exception ex)
				{
					_log.Error(ex.Message, ex);
				}
				return historyParamter;
			}
		}

		public bool AddParameterHistory(IHistoryParameterValueCollection history, int userId, int parameterId, string value)
		{
			bool success = true;
			using (NDC.Push(LogHelper.GetNamespaceContext()))
			{
				try
				{
					HistoryParameterValue historyParameterValue = new HistoryParameterValue();
					historyParameterValue.UserId = userId;
					historyParameterValue.Value = value;
					historyParameterValue.ParameterId = parameterId;
					historyParameterValue.UserDefined = true;
					historyParameterValue.Ordinal = history.Count;
					historyParameterValue.Name = userId + "_" + parameterId + "_" + value;
					historyParameterValue.SelectionType = 0;
					IHistoryParameterValue historyParamter = historyParameterValue;
					using (DatabaseBase db = _cmanager.GetConnection())
					{
						db.DeleteParameterHistory(_cmanager.DbConfig.DbName, "user_issueparameter_history", userId, parameterId, historyParamter.Name);
						db.DbMapping.Save(historyParamter);
						(history as HistoryParameterValueCollection).Clear();
						db.OrdinalOrderBy(_cmanager.DbConfig.DbName, "user_issueparameter_history", userId, parameterId);
						foreach (HistoryValues item2 in db.HistoryWriter(_cmanager.DbConfig.DbName, "user_issueparameter_history", userId, parameterId))
						{
							(history as HistoryParameterValueCollection).Add(new HistoryParameterValue
							{
								Id = item2.Id,
								UserId = item2.UserId,
								Value = item2.Value,
								ParameterId = item2.ParameterId,
								UserDefined = item2.UserDefined,
								Ordinal = item2.Ordinal,
								Name = item2.UserId + "_" + item2.ParameterId + "_" + item2.Value,
								SelectionType = item2.SelectionType
							});
						}
					}
					int[] array = (from h in _userHistoryIssueParameterCollection.Values
						where h.ParameterId == parameterId && h.UserId == userId
						select h.Id).ToArray();
					foreach (int r in array)
					{
						_userHistoryIssueParameterCollection.TryRemove(r, out var _);
					}
					foreach (IHistoryParameterValue item in history)
					{
						Queue.AddSave(item);
						_userHistoryIssueParameterCollection.Add(item);
					}
					return success;
				}
				catch (Exception ex)
				{
					_log.Error(ex.Message, ex);
					return false;
				}
			}
		}

		public void AddTableToIssue(IUser user, ITableObjectCollection tables, IIssueCollection issues, int id)
		{
			ITableObject tObject = Objects[id];
			if (tObject == null)
			{
				return;
			}
			IIssue obj_clone = tObject.Clone() as IIssue;
			obj_clone.IsVisible = GetIsVisibleOfTableByUser(tObject, user);
			if (user.IsSuper)
			{
				(issues as IssueCollection).Add(obj_clone);
				if (!(tables as TableObjectCollection).Contains(id))
				{
					(tables as TableObjectCollection).Add(obj_clone);
				}
			}
			else if (GetOptimizations(user).Count > 0 && GetUserRightToTableObject(Objects[id], user) > RightType.None)
			{
				(issues as IssueCollection).Add(obj_clone);
			}
		}

		private Dictionary<int, EmptyDistinctColumn> LoadDistinctColumnInformation(int tableId, IOptimization opt)
		{
			Dictionary<int, EmptyDistinctColumn> emptyDistinctDict = new Dictionary<int, EmptyDistinctColumn>();
			using DatabaseBase db = _cmanager.GetConnection();
			if (!db.TableExists("empty_distinct_columns"))
			{
				return emptyDistinctDict;
			}
			StringBuilder selectionString = new StringBuilder();
			selectionString.AppendFormat("table_id = {0}", tableId);
			selectionString.AppendFormat(" AND (index_value IS NULL OR index_value = '{0}')", opt.FindValue(OptimizationType.IndexTable));
			selectionString.AppendFormat(" AND (split_value IS NULL OR split_value = '{0}')", opt.FindValue(OptimizationType.SplitTable));
			selectionString.AppendFormat(" AND (sort_value = '{0}')", opt.FindValue(OptimizationType.SortColumn));
			List<EmptyDistinctColumn> list = db.DbMapping.Load<EmptyDistinctColumn>(selectionString.ToString());
			if (list.Count == 0)
			{
				selectionString.Clear();
				selectionString.AppendFormat("table_id = {0}", tableId);
				selectionString.AppendFormat(" AND (index_value IS NULL OR index_value = '{0}')", opt.FindValue(OptimizationType.IndexTable));
				selectionString.AppendFormat(" AND (split_value IS NULL OR split_value = '{0}')", opt.FindValue(OptimizationType.SplitTable));
				selectionString.Append(" AND (sort_value IS NULL)");
				list = db.DbMapping.Load<EmptyDistinctColumn>(selectionString.ToString());
			}
			foreach (EmptyDistinctColumn emptyDistinctColumn in list)
			{
				if (emptyDistinctDict.ContainsKey(emptyDistinctColumn.ColumnId))
				{
					emptyDistinctDict.Remove(emptyDistinctColumn.ColumnId);
				}
				emptyDistinctDict.Add(emptyDistinctColumn.ColumnId, emptyDistinctColumn);
			}
			return emptyDistinctDict;
		}

		private RightType GetRoleRightsToColumnObject(IRole role, IColumn column)
		{
			RightType col_right = RightType.None;
			foreach (ICategory c in _categories)
			{
				RightType catRight = _roleCategoryRights[role, c];
				if (catRight == RightType.Inherit)
				{
					catRight = RightType.None;
				}
				foreach (ITableObject table in c.TableObjects)
				{
					if (table.Columns.Contains(column))
					{
						RightType obj_right = _roleTableObjectRights[role, table];
						if (obj_right == RightType.Inherit)
						{
							obj_right = catRight;
						}
						col_right = _roleColumnRights[role, column];
						if (col_right == RightType.Inherit)
						{
							col_right = obj_right;
						}
						break;
					}
				}
			}
			return col_right;
		}

		private RightType GetUserRightsToColumnObject(IColumn column, IUser user)
		{
			if (user.IsSuper)
			{
				return RightType.Read;
			}
			RightType col_right = RightType.None;
			foreach (ICategory c in _categories)
			{
				RightType cat_right = _userCategoryRights[user, c];
				foreach (IRole role3 in user.Roles)
				{
					if (RightType.Inherit < cat_right)
					{
						break;
					}
					cat_right = _roleCategoryRights[role3, c];
				}
				if (cat_right == RightType.Inherit)
				{
					cat_right = RightType.None;
				}
				foreach (ITableObject t in c.TableObjects)
				{
					if (!t.Columns.Contains(column))
					{
						continue;
					}
					RightType obj_right = _userTableObjectRights[user, t];
					foreach (IRole role2 in user.Roles)
					{
						if (RightType.Inherit < obj_right)
						{
							break;
						}
						obj_right = _roleTableObjectRights[role2, t];
					}
					if (obj_right == RightType.Inherit)
					{
						obj_right = cat_right;
					}
					col_right = _userColumnRights[user, column];
					foreach (IRole role in user.Roles)
					{
						if (RightType.Inherit < col_right)
						{
							break;
						}
						col_right = _roleColumnRights[role, column];
					}
					if (col_right == RightType.Inherit)
					{
						col_right = obj_right;
					}
					break;
				}
			}
			return col_right;
		}

		private RightType GetUserRightsToColumnObject(ITableObject table, IColumn column, IUser user)
		{
			if (user.IsSuper)
			{
				return RightType.Read;
			}
			RightType obj_right = RightType.None;
			foreach (ICategory category in _categories)
			{
				RightType cat_right = _userCategoryRights[user, category];
				foreach (IRole role3 in user.Roles)
				{
					if (RightType.Inherit < cat_right)
					{
						break;
					}
					cat_right = _roleCategoryRights[role3, category];
				}
				if (cat_right == RightType.Inherit)
				{
					cat_right = RightType.None;
				}
				obj_right = _userTableObjectRights[user, table];
				foreach (IRole role2 in user.Roles)
				{
					if (RightType.Inherit < obj_right)
					{
						break;
					}
					obj_right = _roleTableObjectRights[role2, table];
				}
				if (obj_right == RightType.Inherit)
				{
					obj_right = cat_right;
				}
				RightType col_right = _userColumnRights[user, column];
				foreach (IRole role in user.Roles)
				{
					if (RightType.Inherit < col_right)
					{
						break;
					}
					col_right = _roleColumnRights[role, column];
				}
				if (col_right == RightType.Inherit)
				{
					col_right = obj_right;
				}
				if (col_right > RightType.None)
				{
					return col_right;
				}
			}
			return obj_right;
		}

		private RightType GetRoleRightsToTableObject(ITableObject table, IRole role)
		{
			RightType obj_right = RightType.None;
			foreach (ICategory c in _categories)
			{
				RightType cat_right = _roleCategoryRights[role, c];
				if (cat_right == RightType.Inherit)
				{
					cat_right = RightType.None;
				}
				obj_right = _roleTableObjectRights[role, table];
				if (obj_right == RightType.Inherit)
				{
					obj_right = cat_right;
				}
				foreach (IColumn columns in table.Columns)
				{
					RightType col_right = _roleColumnRights[role, columns];
					if (col_right == RightType.Inherit)
					{
						col_right = obj_right;
					}
					if (col_right > RightType.None)
					{
						return col_right;
					}
				}
			}
			return obj_right;
		}

		public bool GetUserRightsToUserLogs(int credentialId, IUser user)
		{
			return _userLogRights[user, _users[credentialId]] > RightType.None;
		}

		public bool GetRoleRightsToUserLogs(int credentialId, IUser user)
		{
			return _userLogRights[user, _users[credentialId]] > RightType.None;
		}

		private RightType GetUserRightToTableObject(ITableObject table, IUser user)
		{
			if (user.IsSuper)
			{
				return RightType.Read;
			}
			RightType obj_right = RightType.None;
			foreach (ICategory category in _categories)
			{
				RightType cat_right = _userCategoryRights[user, category];
				foreach (IRole role3 in user.Roles)
				{
					if (RightType.Inherit < cat_right)
					{
						break;
					}
					cat_right = _roleCategoryRights[role3, category];
				}
				if (cat_right == RightType.Inherit)
				{
					cat_right = RightType.None;
				}
				obj_right = _userTableObjectRights[user, table];
				if (obj_right == RightType.Inherit)
				{
					bool done = false;
					foreach (IRole role2 in user.Roles)
					{
						RightType previous_obj_right = obj_right;
						obj_right = _roleTableObjectRights[role2, table];
						switch (obj_right)
						{
						case RightType.Write:
							done = true;
							break;
						case RightType.Read:
							if (previous_obj_right == RightType.Write)
							{
								obj_right = previous_obj_right;
							}
							break;
						case RightType.None:
							if (previous_obj_right == RightType.Write || previous_obj_right == RightType.Read)
							{
								obj_right = previous_obj_right;
							}
							break;
						case RightType.Inherit:
							obj_right = previous_obj_right;
							break;
						}
						if (done)
						{
							break;
						}
					}
				}
				if (obj_right == RightType.Inherit)
				{
					obj_right = cat_right;
				}
				foreach (IColumn column in table.Columns)
				{
					RightType col_right = _userColumnRights[user, column];
					foreach (IRole role in user.Roles)
					{
						if (RightType.Inherit < col_right)
						{
							break;
						}
						col_right = _roleColumnRights[role, column];
					}
					if (col_right == RightType.Inherit)
					{
						col_right = obj_right;
					}
					if (col_right > RightType.None)
					{
						return col_right;
					}
				}
			}
			return obj_right;
		}

		public IUserObjects GetRoleObjects(IRole role)
		{
			UserObjects objects = new UserObjects();
			objects.Optimizations = GetOptimizations(role);
			lock (RoleTableObjectRights)
			{
				objects.RightObjectTree.FillRightTree(role, this);
			}
			if (role.IsSuper)
			{
				objects.Categories = _categories;
				objects.OptimizationGroups = _optimizationGroups;
				foreach (ICategory category in objects.Categories)
				{
					foreach (ITableObject t in category.TableObjects)
					{
						switch (t.Type)
						{
						case TableType.Issue:
							objects.IssueCount++;
							break;
						case TableType.Table:
							objects.TableCount++;
							break;
						case TableType.View:
							objects.ViewCount++;
							break;
						case TableType.Archive:
							(objects.Archives as ArchiveCollection).Add(t as IArchive);
							break;
						case TableType.ArchiveDocument:
							(objects.ArchiveDocuments as ArchiveDocumentCollection).Add(t as IArchiveDocument);
							break;
						}
					}
				}
			}
			else if (objects.Optimizations.Count > 0)
			{
				foreach (IOptimization opt in objects.Optimizations)
				{
					if (!objects.OptimizationGroups.Contains(opt.Group))
					{
						(objects.OptimizationGroups as OptimizationGroupCollection).Add(opt.Group);
					}
				}
				foreach (ICategory c in _categories)
				{
					RightType cat_right = _roleCategoryRights[role, c];
					if (cat_right == RightType.Inherit)
					{
						cat_right = RightType.None;
					}
					Category cat_clone = c.Clone() as Category;
					foreach (ITableObject t2 in c.TableObjects)
					{
						RightType obj_right = _roleTableObjectRights[role, t2];
						if (obj_right == RightType.Inherit)
						{
							obj_right = cat_right;
						}
						ITableObject obj_clone = t2.Clone() as ITableObject;
						foreach (IColumn col in t2.Columns)
						{
							RightType col_right = _roleColumnRights[role, col];
							if (col_right == RightType.Inherit)
							{
								col_right = obj_right;
							}
							if (col_right > RightType.None)
							{
								IColumn col_clone = col.Clone() as IColumn;
								(obj_clone.Columns as ColumnCollection).Add(col_clone);
							}
						}
						if (obj_clone.Columns.Count > 0 || (obj_clone.Type == TableType.Issue && obj_right > RightType.None))
						{
							switch (obj_clone.Type)
							{
							case TableType.Issue:
								objects.IssueCount++;
								break;
							case TableType.Table:
								objects.TableCount++;
								break;
							case TableType.View:
								objects.ViewCount++;
								break;
							case TableType.Archive:
								(objects.Archives as ArchiveCollection).Add(obj_clone as IArchive);
								break;
							case TableType.ArchiveDocument:
								(objects.ArchiveDocuments as ArchiveDocumentCollection).Add(obj_clone as IArchiveDocument);
								break;
							}
						}
					}
					if (objects.TableCount + objects.ViewCount + objects.IssueCount > 0)
					{
						(objects.Categories as CategoryCollection).Add(cat_clone);
					}
				}
			}
			if (objects.IssueCount > 0)
			{
				(objects.IssueCategories as CategoryCollection).Add(objects.Categories[0]);
			}
			if (objects.TableCount > 0)
			{
				(objects.TableCategories as CategoryCollection).Add(objects.Categories[0]);
			}
			if (objects.ViewCount > 0)
			{
				(objects.ViewCategories as CategoryCollection).Add(objects.Categories[0]);
			}
			return objects;
		}

		public bool LoadInFileSystem(string database)
		{
			if (_fileSystems.Count != 0 && _fileSystems.Any((IFileSys x) => x.Database == database))
			{
				Task.WaitAll(new List<Task>
				{
					Task.Factory.StartNew(delegate
					{
						using DatabaseBase databaseBase2 = _cmanager.GetConnection();
						string sql2 = "SELECT `id`, `parent_id`, `name` FROM `" + database + "`.`filesystem_directories`";
						_directoriesObjects = new DirectoryObjectCollection();
						foreach (DirectoryObject current2 in databaseBase2.DbMapping.LoadBySQL<DirectoryObject>(sql2))
						{
							_directoriesObjects.Add(current2);
						}
					}),
					Task.Factory.StartNew(delegate
					{
						using DatabaseBase databaseBase = _cmanager.GetConnection();
						string sql = "SELECT `id`, `directory_id`, `file_name`, `file_size`, `file_date` FROM `" + database + "`.`filesystem_files`";
						_filesObjects = new FileObjectCollection();
						foreach (FileObject current in databaseBase.DbMapping.LoadBySQL<FileObject>(sql))
						{
							_filesObjects.Add(current);
						}
					})
				}.ToArray());
				_directoriesObjects.First().Children = new List<IDirectoryObject>();
				foreach (IGrouping<int, IDirectoryObject> dir in from c in _directoriesObjects
					where c.ParentId != 0
					select c into x
					group x by x.ParentId)
				{
					_directoriesObjects[dir.Key].Children = new List<IDirectoryObject>();
				}
				foreach (IDirectoryObject item in _directoriesObjects)
				{
					if (item.ParentId != 0)
					{
						_directoriesObjects[item.ParentId].Children.Add(item.Clone());
					}
				}
				return true;
			}
			return false;
		}

		public void UpdateStartScreensList()
		{
			_startscreens.Clear();
			using DatabaseBase db = ConnectionManager.GetConnection();
			foreach (StartScreen item in db.DbMapping.Load<StartScreen>())
			{
				_startscreens.Add(item);
			}
		}

		public void SetTableEngineForTableObject(ITableObject table)
		{
			table.EngineType = DB.GetTableEngineByName(table.Database, table.TableName);
		}

		private void OnError(Exception ex)
		{
			if (this.Error != null)
			{
				this.Error(this, new System.IO.ErrorEventArgs(ex));
			}
		}

		public void LoadTables(DatabaseBase db, bool useNewIssueMethod = false)
		{
			try
			{
				LoadTablesImpl(db, useNewIssueMethod);
			}
			catch (Exception ex)
			{
				using (NDC.Push(LogHelper.GetNamespaceContext()))
				{
					_log.Error(ex.Message, ex);
				}
				OnError(ex);
				throw;
			}
		}

		public void CheckTables(DatabaseBase db)
		{
			try
			{
				CheckTablesImpl(db);
			}
			catch (Exception ex)
			{
				using (NDC.Push(LogHelper.GetNamespaceContext()))
				{
					_log.Error(ex.Message, ex);
				}
				OnError(ex);
				throw;
			}
		}

		private void SetObjectType(TableObject tobj, DatabaseBase db)
		{
			if (tobj == null)
			{
				return;
			}
			string prefix = string.Empty;
			if (tobj.ObjectTypeCode == 0)
			{
				if (tobj.TableName.IndexOf("_") == 2)
				{
					prefix = tobj.TableName.Substring(0, 2);
				}
				if (!string.IsNullOrEmpty(prefix) && _objectTypesCollection.ContainsValue(prefix))
				{
					tobj.ObjectTypeCode = _objectTypesCollection.FirstOrDefault((KeyValuePair<int, string> x) => x.Value == prefix).Key;
					db.DbMapping.Save(tobj);
				}
			}
			RefreshTableObjectType(tobj, prefix);
		}

		public void RefreshTableObjectType(TableObject tobj, string tablePrefix = null)
		{
			string objectName = string.Empty;
			foreach (Language language in _languages)
			{
				IObjectTypeText temp = ObjectTypeTextCollection.FirstOrDefault((IObjectTypeText t) => t.CountryCode == language.CountryCode && t.RefId == tobj.ObjectTypeCode);
				if (temp == null)
				{
					if (tobj.TableName.IndexOf("_") == 2)
					{
						tablePrefix = tobj.TableName.Substring(0, 2);
						objectName = GetObjectTypeName(tablePrefix, language.CultureInfo);
					}
					else
					{
						objectName = global::SystemDb.Resources.Resources.ResourceManager.GetString("other", language.CultureInfo);
					}
				}
				else
				{
					objectName = temp.Text;
				}
				tobj.SetObjectType(objectName, language);
			}
		}

		public static string GetObjectTypeName(string prefix, CultureInfo language)
		{
			return global::SystemDb.Resources.Resources.ResourceManager.GetString(prefix.ToLower(), language) ?? global::SystemDb.Resources.Resources.ResourceManager.GetString("other", language);
		}

		public ITableObjectCollection ModifyIssue(DatabaseBase db, IIssue issue, ITableObjectCollection tableObjectsForModifying)
		{
			if (tableObjectsForModifying == null)
			{
				tableObjectsForModifying = new TableObjectCollection();
				foreach (FakeTable fakeTable in db.DbMapping.Load<FakeTable>("type = " + 6))
				{
					tableObjectsForModifying.Add(fakeTable);
				}
				foreach (FakeProcedure fakeProc in db.DbMapping.Load<FakeProcedure>("type = " + 7))
				{
					tableObjectsForModifying.Add(fakeProc);
				}
				tableObjectsForModifying.AddRange(_objects);
			}
			ModifyIssueImpl(db, (Issue)issue, (TableObjectCollection)tableObjectsForModifying);
			return tableObjectsForModifying;
		}

		private void ModifyIssueImpl(DatabaseBase conn, Issue issue, TableObjectCollection tableObjectsForModifying)
		{
			if (issue.IssueType != IssueType.StoredProcedure)
			{
				return;
			}
			string proc = conn.GetProcedureDefinition(issue.Database, issue.Command);
			string modifiedProc = ModifyProcedure(proc, tableObjectsForModifying, issue.Database);
			if (!(proc != modifiedProc))
			{
				return;
			}
			DbConfig obj = (DbConfig)conn.DbConfig.Clone();
			obj.DbName = issue.Database;
			using DatabaseBase baseConn = ConnectionManager.CreateConnection(obj);
			baseConn.Open();
			baseConn.ExecuteNonQuery("DROP PROCEDURE IF EXISTS " + baseConn.Enquote(issue.Command));
			try
			{
				baseConn.ExecuteNonQuery(modifiedProc);
			}
			catch (Exception)
			{
				baseConn.ExecuteNonQuery(proc);
			}
		}

		internal string ModifyProcedure(string proc, ITableObjectCollection tableObjectsForModifying, string database)
		{
			StringBuilder builder = new StringBuilder(proc.Length);
			Regex tableRegEx = GetRegexForSkriptParsing();
			string[] splitLines = proc.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
			string[] array = splitLines;
			foreach (string line in array)
			{
				string currentLine = line;
				List<ReplaceOccurence> replaceStrings = new List<ReplaceOccurence>();
				Match match = tableRegEx.Match(currentLine);
				while (match.Success)
				{
					Group tableGroup = match.Groups["Retrieve"];
					if (tableGroup.Success)
					{
						int indexOfDot = tableGroup.Value.IndexOf('.');
						string table = tableGroup.Value;
						if (indexOfDot != -1)
						{
							table = tableGroup.Value.Substring(indexOfDot + 1);
						}
						table = table.Replace("`", "");
						foreach (ITableObject tableObject in tableObjectsForModifying)
						{
							if ((tableObject.Type == TableType.Table || tableObject.Type == TableType.View || tableObject.Type == TableType.FakeTable || tableObject.Type == TableType.FakeProcedure) && !(tableObject.Database != database) && table.ToLower() == tableObject.TableName.ToLower())
							{
								replaceStrings.Add(new ReplaceOccurence
								{
									Group = tableGroup,
									TableObject = tableObject
								});
							}
						}
					}
					match = match.NextMatch();
				}
				int pos = 0;
				foreach (ReplaceOccurence replacement in replaceStrings)
				{
					builder.Append(currentLine.Substring(pos, replacement.Group.Index - pos));
					pos = replacement.Group.Index + replacement.Group.Length;
					builder.Append('`').Append(replacement.TableObject.Database).Append("`.`")
						.Append(replacement.TableObject.TableName)
						.Append('`');
				}
				if (pos != 0)
				{
					builder.Append(currentLine.Substring(pos));
				}
				else
				{
					builder.Append(currentLine);
				}
				if (line != splitLines[splitLines.Length - 1])
				{
					builder.Append(Environment.NewLine);
				}
			}
			return builder.ToString();
		}

		public Dictionary<string, long> GetTableNamesAndComplexity(string statement, Dictionary<string, long> tableObjectsNamesAndComplexity)
		{
			Dictionary<string, long> tableInfos = new Dictionary<string, long>();
			string[] array = statement.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				string currentLine;
				string currentLineLowered = (currentLine = array[i]).ToLower();
				foreach (KeyValuePair<string, long> tableObjectName in tableObjectsNamesAndComplexity)
				{
					int index = -1;
					while (index + 1 < currentLine.Length && (index = currentLineLowered.IndexOf(tableObjectName.Key, index + 1)) != -1)
					{
						int start = currentLineLowered.LastIndexOfAny(new char[5] { ' ', '`', '.', '(', '\t' }, index) + 1;
						int stop = currentLineLowered.IndexOfAny(new char[5] { ' ', '`', ';', ')', '(' }, index);
						if (stop == -1)
						{
							stop = currentLine.Length;
						}
						string tableName = currentLineLowered.Substring(start, stop - start);
						if (!(tableName.ToLower() != tableObjectName.Key.ToLower()))
						{
							if (!tableInfos.ContainsKey(tableName))
							{
								tableInfos.Add(tableName, tableObjectName.Value);
							}
							break;
						}
					}
				}
			}
			return tableInfos;
		}

		private Regex GetRegexForSkriptParsing()
		{
			return _regexForSkriptParsing ?? (_regexForSkriptParsing = new Regex("\\bjoin\\s+[\\(\\s]*`(?<Retrieve>[ a-zA-Z\\._$\\d]+)|\\bjoin\\s+[\\(\\s]*(?<Retrieve>[`a-zA-Z\\._$\\d]+)|\\bfrom\\s+[\\(\\s]*`(?<Retrieve>[ a-zA-Z\\._$\\d]+)`|\\bfrom\\s+[\\(\\s]*(?<Retrieve>[`a-zA-Z\\._$\\d]+)|\\bupdate\\s+(?<Update>[`a-zA-Z\\._$\\d]+)\\b|\\binsert\\s+(?:\\binto\\b)?\\s+(?<Insert>[`a-zA-Z\\._$\\d]+)\\b|\\btruncate\\s+table\\s+(?<Delete>[`a-zA-Z\\._$\\d]+)\\b|\\bdelete\\s+(?:\\bfrom\\b)?\\s+(?<Delete>[`a-zA-Z\\._$\\d]+)\\b", RegexOptions.IgnoreCase));
		}

		public Dictionary<string, long> GetTableNamesAndComplexityNew(string statement, Dictionary<string, long> tableObjectsNamesAndComplexity)
		{
			Dictionary<string, long> tableInfos = new Dictionary<string, long>();
			Regex tableRegEx = GetRegexForSkriptParsing();
			string[] array = statement.Split(new string[1] { ";" }, StringSplitOptions.None);
			foreach (string currentLine in array)
			{
				Match match = tableRegEx.Match(currentLine);
				while (match.Success)
				{
					Group tableGroup = match.Groups["Retrieve"];
					if (tableGroup.Success)
					{
						int indexOfDot = tableGroup.Value.IndexOf('.');
						string table = tableGroup.Value;
						if (indexOfDot != -1)
						{
							table = tableGroup.Value.Substring(indexOfDot + 1);
						}
						table = table.Replace("`", "");
						table = table.ToLower();
						if (tableInfos.ContainsKey(table))
						{
							match = match.NextMatch();
							continue;
						}
						foreach (KeyValuePair<string, long> tableObject2 in tableObjectsNamesAndComplexity.Where((KeyValuePair<string, long> tableObject) => table == tableObject.Key.ToLower()))
						{
							if (!tableInfos.ContainsKey(table))
							{
								tableInfos.Add(table, tableObject2.Value);
							}
						}
					}
					match = match.NextMatch();
				}
			}
			return tableInfos;
		}

		private void FillOptimizationTree(List<Optimization> optList, List<OptimizationText> optTexts)
		{
			if (optList.Count == 0)
			{
				return;
			}
			bool inserted = false;
			int i;
			for (i = optList.Count - 1; i >= 0; i--)
			{
				IOptimization opt;
				if ((opt = _optimizations.FirstOrDefault((IOptimization o) => optList[i].ParentId == o.Id)) == null)
				{
					continue;
				}
				optList[i].Parent = opt;
				optList[i].Group = _optimizationGroups[optList[i].OptimizationGroupId];
				foreach (ILanguage j in _languages)
				{
					OptimizationText textObject = optTexts.FirstOrDefault((OptimizationText o) => optList[i].Id == o.RefId && o.CountryCode == j.CountryCode);
					if (textObject != null)
					{
						optList[i].SetDescription(textObject.Text, j);
					}
				}
				(opt.Children as OptimizationCollection).Add(optList[i]);
				_optimizations.Add(optList[i]);
				optList.RemoveAt(i);
				inserted = true;
			}
			OptimizationCollection removeList = new OptimizationCollection();
			foreach (IOptimization opItem in _optimizations)
			{
				if (opItem.Value != null && opItem.Value.Trim().ToLower().EndsWith("_dynamic"))
				{
					removeList.Add(opItem);
				}
			}
			foreach (IOptimization removeItem in removeList)
			{
				_optimizations.Remove(removeItem.Id);
			}
			if (!inserted)
			{
				throw new FormatException("wrong parent id");
			}
			FillOptimizationTree(optList, optTexts);
		}

		public DatabaseBaseOutOfDateInformation HasDatabaseUpgrade(DatabaseBase conn)
		{
			return new DatabaseUpgrade(conn).HasUpgrade();
		}

		public void CreateInfoTable(DatabaseBase conn)
		{
			conn.DbMapping.CreateTableIfNotExists<Info>();
			new Info(conn).DbVersion = VersionInfo.CurrentDbVersion;
		}

		private void AddProperty(DatabaseBase db, string key, string value, PropertyType type, string descrDe, string descrEn, string nameDe, string nameEn)
		{
			ILanguage de = Languages["de"];
			ILanguage en = Languages["en"];
			Property prop = new Property
			{
				Key = key,
				Value = value,
				Type = type
			};
			db.DbMapping.Save(prop);
			if (de != null)
			{
				PropertyText propertyText2 = new PropertyText
				{
					CountryCode = de.CountryCode,
					RefId = prop.Id,
					Text = descrDe,
					Name = nameDe
				};
				prop.Descriptions[de] = descrDe;
				prop.Names[de] = nameDe;
				db.DbMapping.Save(propertyText2);
			}
			if (en != null)
			{
				PropertyText propertyText = new PropertyText
				{
					CountryCode = en.CountryCode,
					RefId = prop.Id,
					Text = descrEn,
					Name = nameEn
				};
				prop.Descriptions[en] = descrEn;
				prop.Names[en] = nameEn;
				db.DbMapping.Save(propertyText);
			}
			_properties.Add(prop);
		}

		private void FillDefaultsIfEmpty(DatabaseBase db)
		{
			int thousand = 0;
			int optimization = 0;
			int enlarged_datagrid = 0;
			int table_order = 0;
			int export_description = 0;
			int indexerrorprop = 0;
			int defaultPDFfontSize = 0;
			int showNullValue = 0;
			thousand = Properties.Where((IProperty item) => item.Key.Equals("thousand")).Count();
			optimization = Properties.Where((IProperty item) => item.Key.Equals("optimization")).Count();
			enlarged_datagrid = Properties.Where((IProperty item) => item.Key.Equals("enlarged_datagrid")).Count();
			table_order = Properties.Where((IProperty item) => item.Key.Equals("table_order")).Count();
			export_description = Properties.Where((IProperty item) => item.Key.Equals("export_description")).Count();
			indexerrorprop = Properties.Where((IProperty item) => item.Key.Equals("indexerrorprop")).Count();
			defaultPDFfontSize = Properties.Where((IProperty item) => item.Key.Equals("defaultPDFfontSize")).Count();
			showNullValue = Properties.Where((IProperty item) => item.Key.Equals("shownullvalue")).Count();
			if (thousand > 1 || optimization > 1 || enlarged_datagrid > 1 || table_order > 1 || export_description > 1 || indexerrorprop > 1 || defaultPDFfontSize > 1 || showNullValue > 1)
			{
				db.ExecuteNonQuery("DELETE FROM property_texts;");
				foreach (UserPropertySettings setting in _userPropertySettings.GetAllSettings)
				{
					db.DbMapping.Delete(setting);
				}
				_userPropertySettings.Clear();
				db.ExecuteNonQuery("DELETE FROM user_property_settings;");
				foreach (Property prop in Properties)
				{
					db.DbMapping.Delete(prop);
				}
				_properties.Clear();
				thousand = 0;
				optimization = 0;
				enlarged_datagrid = 0;
				table_order = 0;
				export_description = 0;
				indexerrorprop = 0;
				defaultPDFfontSize = 0;
				showNullValue = 0;
			}
			else if (showNullValue != 0)
			{
				IProperty prop2 = Properties.FirstOrDefault((IProperty item) => item.Key.Equals("shownullvalue"));
				string newNameValue = "Show null values";
				ILanguage en2 = Languages["en"];
				if (en2 != null && prop2.Names[en2] != newNameValue)
				{
					prop2.Names[en2] = newNameValue;
					db.ExecuteNonQuery("UPDATE property_texts SET `NAME` = '" + newNameValue + "' WHERE `REF_ID` = " + prop2.Id + " AND `COUNTRY_CODE` = 'en';");
				}
			}
			if (thousand == 0)
			{
				AddProperty(db, "thousand", "false", PropertyType.Bool, "Soll das Tausender-Trennzeichen in der Tabellenansicht angezeigt werden?", "Should there be a display of thousand separators within table view?", "Tausender-Trennzeichen", "Thousand-separators");
			}
			if (optimization == 0)
			{
				AddProperty(db, "optimization", "true", PropertyType.Bool, "Soll die Beschreibung der Optimierungskriterien angezeigt werden?", "Show the description of the optimization criteria?", "Darstellung der Optimierungskriterien", "Presentation of optimization criteria");
			}
			if (enlarged_datagrid == 0)
			{
				AddProperty(db, "enlarged_datagrid", "false", PropertyType.Bool, "Soll die Detailansicht immer vergrößert dargestellt werden?", "Should the details always be displayed in an enlarged fashion?", "Darstellung der Detailansicht", "Show details");
			}
			if (table_order == 0)
			{
				AddProperty(db, "table_order", "false", PropertyType.Bool, "Wollen Sie, dass zuletzt angeklickte Sachverhalte/Sichten in den Übersichtslisten auf die oberste Position rutschen?", "Do you want the last clicked reports/views to be placed at the top of the list?", "Tabellen-Reihenfolge nach Aufruf", "Table order");
			}
			if (export_description == 0)
			{
				AddProperty(db, "export_description", "false", PropertyType.Bool, "Wollen Sie, dass die Beschreibung des Exports in der Excel-Datei sichtbar ist?", "Do you want the description of an export to be displayed inside the excel file?", "Export-Beschreibung in Excel-Datei", "Export description in excel-file");
			}
			if (indexerrorprop == 0)
			{
				AddProperty(db, "indexerrorprop", "false", PropertyType.Bool, "Soll die falsche index informationen angezeigt werden?", "Do you want to the wrong index information to be displayed?", "Index fehler anzeigen", "Show Index Error");
			}
			if (defaultPDFfontSize == 0)
			{
				AddProperty(db, "defaultPDFfontSize", "12", PropertyType.Integer, "Hier können Sie die default Schriftgröße einstellen (Zwischen 5 und 20)", "Here you can set the default font size (Between 5 and 20)", "PDF Schriftgröße", "PDF font size");
			}
			if (showNullValue == 0)
			{
				AddProperty(db, "shownullvalue", "false", PropertyType.Bool, "Soll der Nullwert angezeigt werden?", "Do you want the null values to be displayed?", "Nullwert anzeigen", "Show null values");
			}
			if (Categories.Count == 1)
			{
				ICategory cat = Categories.First();
				ILanguage de2 = Languages["de"];
				if (de2 != null && cat.Names[de2] == null)
				{
					cat.SetName("Allgemein", de2);
					CategoryText text = new CategoryText
					{
						CountryCode = "de",
						RefId = cat.Id,
						Text = "Allgemein"
					};
					db.DbMapping.Save(text);
				}
			}
			if (Roles.Count == 0)
			{
				global::SystemDb.Internal.Role role = new global::SystemDb.Internal.Role
				{
					Flags = SpecialRights.Super,
					Name = "Admin"
				};
				db.DbMapping.Save(role);
				Roles.Add(role);
			}
			if (Users.Count == 0)
			{
				global::SystemDb.Internal.User user = new global::SystemDb.Internal.User
				{
					UserName = "avendata_admin",
					Password = "avendata_admin",
					Flags = SpecialRights.Super,
					Name = "Admin Avendata"
				};
				db.DbMapping.Save(user);
			}
			MySqlConnection conn = new MySqlConnection(db.ConnectionString);
			if (conn.State != ConnectionState.Open)
			{
				conn.Open();
			}
			try
			{
				if (Users.Where((IUser x) => x.UserName != null && x.UserName.ToLower() == "avendata_qs").Count() == 0)
				{
					global::SystemDb.Internal.User avendata_qs = new global::SystemDb.Internal.User
					{
						UserName = "avendata_qs",
						Password = "avendata_qs",
						Flags = SpecialRights.Super,
						Name = "Avendata QS"
					};
					db.DbMapping.Save(avendata_qs);
					UsersByUserName.Add(avendata_qs);
				}
			}
			catch
			{
				_log.ContextLog(LogLevelEnum.Info, "Error while creating Avendata QS user!");
			}
			finally
			{
				conn.Close();
			}
			foreach (ITableObject system in Objects.Distinct(new Utils.EqualityComparer<ITableObject>((ITableObject o) => o.Database.ToLower().GetHashCode())))
			{
				if (!Optimizations.Where((IOptimization o) => o.Group.Type == OptimizationType.System).All((IOptimization o) => o.Value.ToLower() != system.Database.ToLower()))
				{
					continue;
				}
				IOptimizationGroup optGroup = null;
				foreach (IOptimizationGroup group in _optimizationGroups)
				{
					if (group.Type == OptimizationType.System)
					{
						optGroup = group;
						break;
					}
				}
				if (optGroup == null)
				{
					ILanguage de = Languages["de"];
					ILanguage en = Languages["en"];
					optGroup = new OptimizationGroup
					{
						Type = OptimizationType.System
					};
					db.DbMapping.Save(optGroup);
					if (de != null)
					{
						optGroup.Names[de] = "System";
						db.DbMapping.Save(new OptimizationGroupText
						{
							CountryCode = "de",
							RefId = optGroup.Id,
							Text = "System"
						});
					}
					if (en != null)
					{
						optGroup.Names[en] = "System";
						db.DbMapping.Save(new OptimizationGroupText
						{
							CountryCode = "en",
							RefId = optGroup.Id,
							Text = "System"
						});
					}
				}
				Optimization opt = new Optimization
				{
					Value = system.Database,
					Parent = _rootOptimization,
					Group = optGroup,
					OptimizationGroupId = optGroup.Id,
					ParentId = _rootOptimization.ParentId
				};
				if (!opt.Value.ToLower().EndsWith("_dynamic"))
				{
					db.DbMapping.Save(opt);
					_optimizations.Add(opt);
					((OptimizationCollection)_rootOptimization.Children).Add(opt);
				}
			}
		}

		public void ImportLoginDb(string login_db_name)
		{
			if (_cmanager == null || _cmanager.ConnectionState != ConnectionStates.Online)
			{
				throw new InvalidOperationException();
			}
			using (DatabaseBase viewbox_db = _cmanager.GetConnection())
			{
				try
				{
					viewbox_db.BeginTransaction();
					using (DatabaseBase login_db = ConnectionManager.CreateConnection(new DbConfig
					{
						ConnectionString = ConnectionString,
						DbName = login_db_name
					}))
					{
						login_db.Open();
						Dictionary<Tuple<int, OptimizationType>, OptimizationGroup> cat = new Dictionary<Tuple<int, OptimizationType>, OptimizationGroup>();
						if (login_db.TableExists("cat"))
						{
							foreach (Catalog c in login_db.DbMapping.Load<Catalog>())
							{
								OptimizationGroup ogroup = new OptimizationGroup();
								ogroup.SetName(c.Name, DefaultLanguage);
								cat.Add(new Tuple<int, OptimizationType>(c.Id, OptimizationType.NotSet), ogroup);
							}
						}
						Dictionary<int, int?> old_id = new Dictionary<int, int?>();
						old_id.Add(0, null);
						if (login_db.TableExists("filter"))
						{
							List<Filter> i = login_db.DbMapping.Load<Filter>();
							FillOptimizationTree(i, cat, old_id, viewbox_db);
						}
					}
					viewbox_db.CommitTransaction();
				}
				catch (Exception ex)
				{
					viewbox_db.RollbackTransaction();
					throw ex;
				}
			}
			foreach (IOptimization o in Optimizations)
			{
				if (o.Group.Type == OptimizationType.System)
				{
					ImportSystemDb(o.Value + "_system");
				}
			}
		}

		public void ImportSystemDb(string system_db_name)
		{
			if (_cmanager == null || _cmanager.ConnectionState != ConnectionStates.Online)
			{
				throw new InvalidOperationException();
			}
			using DatabaseBase viewbox_db = _cmanager.GetConnection();
			try
			{
				viewbox_db.BeginTransaction();
				using (DatabaseBase system_db = ConnectionManager.CreateConnection(new DbConfig
				{
					ConnectionString = ConnectionString,
					DbName = system_db_name
				}))
				{
					system_db.Open();
					string database = system_db.DbConfig.DbName.Replace("_system", string.Empty);
					ISet<int> procs = new HashSet<int>();
					Dictionary<int, Procedures> procByTableId = new Dictionary<int, Procedures>();
					Dictionary<int, int> procById = new Dictionary<int, int>();
					Dictionary<int, List<int>> procAndParams = new Dictionary<int, List<int>>();
					Dictionary<int, Parameter> procParams = new Dictionary<int, Parameter>();
					if (system_db.TableExists("procedures"))
					{
						foreach (Procedures p2 in system_db.DbMapping.Load<Procedures>())
						{
							procs.Add(p2.TableId);
							procByTableId.Add(p2.TableId, p2);
							procById.Add(p2.TableId, p2.Id);
							procAndParams.Add(p2.Id, new List<int>());
						}
					}
					if (system_db.TableExists("procedure_params"))
					{
						foreach (ProcedureParameter p in system_db.DbMapping.LoadSorted<ProcedureParameter>("ordinal"))
						{
							SqlType type2 = p.Type.ToLower() switch
							{
								"string" => SqlType.String, 
								"char" => SqlType.String, 
								"char(10)" => SqlType.String, 
								"natural number" => SqlType.Integer, 
								"numeric" => SqlType.Decimal, 
								"date" => SqlType.Date, 
								"time" => SqlType.Time, 
								"datetime" => SqlType.DateTime, 
								"integer" => SqlType.Integer, 
								"date as integer yyyymmdd" => SqlType.Date, 
								"time as integer hhmmss" => SqlType.Time, 
								"datetime as integer yyyymmddhhmmss" => SqlType.DateTime, 
								"float" => SqlType.Numeric, 
								"int" => SqlType.Integer, 
								"number" => SqlType.Integer, 
								"int(2) unsigned" => SqlType.Integer, 
								"int(4) unsigned" => SqlType.Integer, 
								"tinyint(3) unsigned" => SqlType.Integer, 
								"tinyint(2) unsigned" => SqlType.Integer, 
								"smallint(4) unsigned" => SqlType.Integer, 
								"varchar(1)" => SqlType.String, 
								"char(16)" => SqlType.String, 
								"bool" => SqlType.Boolean, 
								"boolean" => SqlType.Boolean, 
								_ => throw new FormatException($"unknown column type '{p.Type}'"), 
							};
							Parameter par2 = new Parameter
							{
								Name = p.Name,
								Ordinal = p.Ordinal,
								DataType = type2,
								UserDefined = false
							};
							par2.SetDescription(p.Comment, DefaultLanguage);
							procParams.Add(p.Id, par2);
							procAndParams[p.ProcedureId].Add(p.Id);
						}
					}
					Dictionary<int, global::SystemDb.Compatibility.Table> tables = new Dictionary<int, global::SystemDb.Compatibility.Table>();
					Dictionary<int, TableObject> old_id = new Dictionary<int, TableObject>();
					Category generalCategory = new Category
					{
						Id = 0
					};
					string checkDbSql = $"select count(*) from tables where `database` = '{database}'";
					bool mergeDatabase = (long)viewbox_db.ExecuteScalar(checkDbSql) > 0;
					foreach (global::SystemDb.Compatibility.Table t in system_db.DbMapping.Load<global::SystemDb.Compatibility.Table>())
					{
						if (mergeDatabase)
						{
							string checkTableSql = $"select count(*) from tables where `database` = '{database}' and name = '{t.Name}'";
							if ((long)viewbox_db.ExecuteScalar(checkTableSql) != 0L)
							{
								continue;
							}
						}
						tables.Add(t.Id, t);
						TableObject tobj2;
						if (t.SystemId == 1)
						{
							tobj2 = new global::SystemDb.Internal.Table();
							tobj2.Ordinal = Tables.Count;
							tobj2.TransactionNumber = (tobj2.Ordinal + 10001).ToString();
						}
						else
						{
							if (t.SystemId != 2)
							{
								throw new FormatException($"unknown system_id '{t.SystemId}'");
							}
							if (procs.Contains(t.Id))
							{
								tobj2 = new Issue();
								tobj2.Ordinal = Issues.Count;
								tobj2.TransactionNumber = (tobj2.Ordinal + 1001).ToString();
							}
							else
							{
								tobj2 = new View();
								tobj2.Ordinal = Views.Count;
								tobj2.TransactionNumber = (tobj2.Ordinal + 2001).ToString();
							}
						}
						long count = -1L;
						if (tobj2.Type == TableType.View || tobj2.Type == TableType.Table)
						{
							try
							{
								count = system_db.CountTable(database, t.Name);
							}
							catch
							{
								continue;
							}
						}
						tobj2.Database = database;
						tobj2.TableName = t.Name;
						tobj2.RowCount = count;
						tobj2.UserDefined = t.UserDefined;
						tobj2.Category = generalCategory;
						tobj2.IsVisible = true;
						viewbox_db.DbMapping.Save(tobj2);
						if (!string.IsNullOrEmpty(t.Comment))
						{
							tobj2.SetDescription(t.Comment, DefaultLanguage);
							viewbox_db.DbMapping.Save(new TableObjectText
							{
								RefId = tobj2.Id,
								CountryCode = DefaultLanguage.CountryCode,
								Text = t.Comment
							});
						}
						old_id.Add(t.Id, tobj2);
						_objects.Add(tobj2);
						switch (tobj2.Type)
						{
						case TableType.Issue:
							if (procById.ContainsKey(t.Id))
							{
								foreach (int paramsId in procAndParams[procById[t.Id]])
								{
									if (!procParams.ContainsKey(paramsId))
									{
										continue;
									}
									Parameter par = procParams[paramsId];
									par.Issue = tobj2 as Issue;
									viewbox_db.DbMapping.Save(par);
									((tobj2 as Issue).Parameters as ParameterCollection).Add(par);
									foreach (ILanguage i in _languages)
									{
										if (par.Descriptions[i] != null)
										{
											ParameterText par_text = new ParameterText
											{
												RefId = par.Id,
												Text = par.Descriptions[i],
												CountryCode = i.CountryCode
											};
											viewbox_db.DbMapping.Save(par_text);
										}
									}
								}
								(tobj2 as Issue).Command = procByTableId[t.Id].Name;
								(tobj2 as Issue).UseIndexValue = procByTableId[t.Id].ClientSplitting;
								(tobj2 as Issue).UseSplitValue = procByTableId[t.Id].CompanyCodeSplitting;
								(tobj2 as Issue).UseSortValue = procByTableId[t.Id].FinancialYearSplitting;
								IssueExtension ie = new IssueExtension();
								ie.Command = (tobj2 as Issue).Command;
								ie.RefId = tobj2.Id;
								ie.UseIndexValue = (tobj2 as Issue).UseIndexValue;
								ie.UseSortValue = (tobj2 as Issue).UseSortValue;
								ie.UseSplitValue = (tobj2 as Issue).UseSplitValue;
								viewbox_db.DbMapping.Save(ie);
							}
							_issues.Add(tobj2 as Issue);
							break;
						case TableType.View:
							_views.Add(tobj2 as View);
							break;
						case TableType.Table:
							_tables.Add(tobj2 as global::SystemDb.Internal.Table);
							break;
						}
					}
					foreach (global::SystemDb.Compatibility.Column c in system_db.DbMapping.Load<global::SystemDb.Compatibility.Column>())
					{
						if (old_id.ContainsKey(c.TableId))
						{
							TableObject tobj = old_id[c.TableId];
							SqlType type;
							switch (c.Type.Trim().ToLower())
							{
							case "string":
								type = SqlType.String;
								break;
							case "integer":
							case "binary":
							case "natural number":
								type = SqlType.Integer;
								break;
							case "float":
							case "numeric":
								type = SqlType.Decimal;
								break;
							case "date":
							case "date dd/mm/yy":
							case "date yyyy-mm-dd":
							case "date dd.mm.yyyy":
							case "date dd.mm.yy":
							case "date as integer yyyymmdd":
								type = SqlType.Date;
								break;
							case "time":
							case "time as integer hhmmss":
								type = SqlType.Time;
								break;
							case "datetime":
							case "datetime as integer yyyymmddhhmmss":
								type = SqlType.DateTime;
								break;
							case "datetime dd.mm.yyyy hh:mm:ss":
								type = SqlType.DateTime;
								break;
							default:
								throw new FormatException($"unknown column type '{c.Type}'");
							}
							global::SystemDb.Internal.Column column = new global::SystemDb.Internal.Column
							{
								Id = 0,
								Table = tobj,
								Name = c.Name,
								DataType = type,
								IsVisible = c.IsSelected,
								IsEmpty = c.IsEmpty,
								Ordinal = tobj.ColumnCount,
								UserDefined = tobj.UserDefined,
								ConstValue = c.ConstantValue
							};
							if (c.Type.Trim().ToLower() == "float" || c.Type.Trim().ToLower() == "numeric")
							{
								column.MaxLength = ((c.Length > 0) ? c.Length : 2);
							}
							global::SystemDb.Compatibility.Table old_t = tables[c.TableId];
							if (old_t.ClientColumnName != null && old_t.ClientColumnName.ToLower() == c.Name.ToLower())
							{
								column.OptimizationType = OptimizationType.IndexTable;
								tobj.IndexTableColumn = column;
							}
							else if (old_t.CompanyCodeColumnName != null && old_t.CompanyCodeColumnName.ToLower() == c.Name.ToLower())
							{
								column.OptimizationType = OptimizationType.SplitTable;
								tobj.SplitTableColumn = column;
							}
							else if (old_t.FinancialYearColumnName != null && old_t.FinancialYearColumnName.ToLower() == c.Name.ToLower())
							{
								column.OptimizationType = OptimizationType.SortColumn;
								tobj.SortColumn = column;
							}
							viewbox_db.DbMapping.Save(column);
							if (!string.IsNullOrEmpty(c.Comment))
							{
								column.SetDescription(c.Comment, DefaultLanguage);
								viewbox_db.DbMapping.Save(new ColumnText
								{
									RefId = column.Id,
									CountryCode = DefaultLanguage.CountryCode,
									Text = column.Descriptions[DefaultLanguage]
								});
							}
							(tobj.Columns as ColumnCollection).Add(column);
						}
					}
				}
				viewbox_db.CommitTransaction();
			}
			catch (Exception)
			{
				viewbox_db.RollbackTransaction();
				throw;
			}
		}

		public void CheckDatatypes(int fromId = 0)
		{
			if (_cmanager == null || _cmanager.ConnectionState != ConnectionStates.Online)
			{
				throw new InvalidOperationException();
			}
			using DatabaseBase viewbox_db = _cmanager.GetConnection();
			try
			{
				viewbox_db.BeginTransaction();
				CheckDatatypes(viewbox_db, fromId);
				viewbox_db.CommitTransaction();
			}
			catch (Exception)
			{
				viewbox_db.RollbackTransaction();
				throw;
			}
		}

		public void ReorderTables(int fromId = 0)
		{
			if (_cmanager == null || _cmanager.ConnectionState != ConnectionStates.Online)
			{
				throw new InvalidOperationException();
			}
			using DatabaseBase viewbox_db = _cmanager.GetConnection();
			new MySqlCommand("set net_write_timeout=99999; set net_read_timeout=99999", (MySqlConnection)viewbox_db.Connection).ExecuteNonQuery();
			ReorderTables(viewbox_db, fromId);
		}

		public void CalculateOrderArea(ITableObject tobj, string orderCols, DatabaseBase viewboxDb)
		{
			try
			{
				List<global::SystemDb.Internal.OrderArea> list = new List<global::SystemDb.Internal.OrderArea>();
				string select = $"SELECT _row_no_, {orderCols} FROM {viewboxDb.Enquote(tobj.Database, tobj.TableName)} ORDER BY _row_no_";
				using (IDataReader reader = viewboxDb.ExecuteReader(select))
				{
					global::SystemDb.Internal.OrderArea oarea = null;
					while (reader.Read())
					{
						int i = 0;
						string index_value = null;
						if (tobj.IndexTableColumn != null && !tobj.IndexTableColumn.IsEmpty && !reader.IsDBNull(++i))
						{
							index_value = reader[i].ToString();
						}
						string split_value = null;
						if (tobj.SplitTableColumn != null && !tobj.SplitTableColumn.IsEmpty && !reader.IsDBNull(++i))
						{
							split_value = reader[i].ToString();
						}
						string sort_value = null;
						if (tobj.SortColumn != null && !tobj.SortColumn.IsEmpty && !reader.IsDBNull(++i))
						{
							sort_value = reader[i].ToString();
						}
						if (oarea == null)
						{
							global::SystemDb.Internal.OrderArea obj = new global::SystemDb.Internal.OrderArea
							{
								TableId = tobj.Id,
								Start = 1L,
								IndexValue = index_value,
								SplitValue = split_value,
								SortValue = sort_value
							};
							oarea = obj;
							list.Add(obj);
							(tobj.OrderAreas as OrderAreaCollection).Add(oarea);
						}
						else if (index_value != oarea.IndexValue || split_value != oarea.SplitValue || sort_value != oarea.SortValue)
						{
							long current = Convert.ToInt64(reader.GetValue(0));
							oarea.End = current - 1;
							global::SystemDb.Internal.OrderArea obj2 = new global::SystemDb.Internal.OrderArea
							{
								TableId = tobj.Id,
								Start = current,
								IndexValue = index_value,
								SplitValue = split_value,
								SortValue = sort_value
							};
							oarea = obj2;
							list.Add(obj2);
							(tobj.OrderAreas as OrderAreaCollection).Add(oarea);
						}
					}
					oarea.End = tobj.RowCount;
				}
				viewboxDb.DbMapping.Save(typeof(global::SystemDb.Internal.OrderArea), list);
			}
			catch (Exception ex)
			{
				_log.Warn($"Calculate order areas for table [{tobj.Name}] failed", ex);
			}
		}

		public void FixSqlServerTypes()
		{
			if (_cmanager == null || _cmanager.ConnectionState != ConnectionStates.Online)
			{
				throw new InvalidOperationException();
			}
			using DatabaseBase viewbox_db = _cmanager.GetConnection();
			try
			{
				viewbox_db.BeginTransaction();
				FixSqlServerTypes(viewbox_db);
				viewbox_db.CommitTransaction();
			}
			catch (Exception)
			{
				viewbox_db.RollbackTransaction();
				throw;
			}
		}

		public void UpgradeDatabase(DatabaseBase conn)
		{
			DatabaseUpgrade upgrader = new DatabaseUpgrade(conn);
			if (upgrader.HasUpgrade() != null)
			{
				upgrader.UpgradeDatabase(conn);
				DatabaseOutOfDateInformation = null;
			}
		}

		public void CheckCounts(DatabaseBase conn)
		{
			foreach (ITableObject o in Objects)
			{
				if (o.Type == TableType.View || o.Type == TableType.Table)
				{
					long count = conn.CountTable(o.Database, o.TableName);
					if (count != o.RowCount)
					{
						(o as TableObject).RowCount = count;
						conn.DbMapping.Save(o);
					}
				}
			}
		}

		public void RemoveNotExistingTables(DatabaseBase conn)
		{
			foreach (ITableObject o in Objects)
			{
				if (o.Type != TableType.Table || conn.TableExists(o.Database, o.TableName))
				{
					continue;
				}
				foreach (IColumn c in o.Columns)
				{
					conn.DbMapping.Delete<ColumnText>("ref_id=" + c.Id);
					conn.DbMapping.Delete(c);
				}
				foreach (IOrderArea a in o.OrderAreas)
				{
					conn.DbMapping.Delete(a);
				}
				conn.DbMapping.Delete<TableObjectText>("ref_id=" + o.Id);
				conn.DbMapping.Delete(o);
			}
		}

		public void ImportDescriptionsFromSystemDb(string system_db_name)
		{
			using DatabaseBase viewbox_db = _cmanager.GetConnection();
			using DatabaseBase system_db = ConnectionManager.CreateConnection(new DbConfig
			{
				ConnectionString = ConnectionString,
				DbName = system_db_name
			});
			system_db.Open();
			string database = system_db.DbConfig.DbName.Replace("_system", string.Empty);
			foreach (global::SystemDb.Compatibility.Table t in system_db.DbMapping.Load<global::SystemDb.Compatibility.Table>())
			{
				ITableObject tobj = Objects[database + "." + t.Name];
				if (tobj == null)
				{
					continue;
				}
				if (!string.IsNullOrEmpty(t.Comment) && tobj.Descriptions[DefaultLanguage] == null)
				{
					tobj.SetDescription(t.Comment, DefaultLanguage);
					viewbox_db.DbMapping.Save(new TableObjectText
					{
						CountryCode = DefaultLanguage.CountryCode,
						RefId = tobj.Id,
						Text = t.Comment
					});
				}
				foreach (global::SystemDb.Compatibility.Column c in system_db.DbMapping.Load<global::SystemDb.Compatibility.Column>("table_id = " + t.Id))
				{
					IColumn column = tobj.Columns[c.Name];
					if (column != null && !string.IsNullOrEmpty(c.Comment) && (column.Descriptions[DefaultLanguage] == null || string.IsNullOrEmpty(column.Descriptions[DefaultLanguage])))
					{
						column.SetDescription(c.Comment, DefaultLanguage);
						viewbox_db.DbMapping.Save(new ColumnText
						{
							CountryCode = DefaultLanguage.CountryCode,
							RefId = column.Id,
							Text = c.Comment
						});
					}
				}
			}
		}

		private void FillOptimizationTree(List<Filter> optList, Dictionary<Tuple<int, OptimizationType>, OptimizationGroup> cat, Dictionary<int, int?> old_id, DatabaseBase viewbox_db)
		{
			if (optList.Count == 0)
			{
				foreach (Optimization item in new List<Optimization>(from o in Optimizations
					where o.Group != null && o.Group.Type == OptimizationType.None
					select o as Optimization))
				{
					item.Value = null;
				}
				return;
			}
			bool inserted = false;
			int i;
			for (i = optList.Count - 1; i >= 0; i--)
			{
				IOptimization parent = Optimizations.FirstOrDefault((IOptimization o) => optList[i].ParentId == old_id[o.Id]);
				if (parent == null)
				{
					continue;
				}
				OptimizationGroup optGroup = null;
				OptimizationType original_opt_id = ((optList[i].OptimizationId == OptimizationType.NotSet) ? OptimizationType.None : optList[i].OptimizationId);
				Tuple<int, OptimizationType> key = Tuple.Create(optList[i].CatalogId, original_opt_id);
				if (!cat.ContainsKey(key))
				{
					optGroup = cat[Tuple.Create(optList[i].CatalogId, OptimizationType.NotSet)].Clone();
					optGroup.Type = original_opt_id;
					viewbox_db.DbMapping.Save(optGroup);
					viewbox_db.DbMapping.Save(new OptimizationGroupText
					{
						RefId = optGroup.Id,
						CountryCode = DefaultLanguage.CountryCode,
						Text = optGroup.Names[DefaultLanguage]
					});
					cat.Add(new Tuple<int, OptimizationType>(optList[i].CatalogId, original_opt_id), optGroup);
					_optimizationGroups.Add(optGroup);
				}
				else
				{
					optGroup = cat[key];
				}
				string value = ((optList[i].Value.ToLower() == "alle") ? null : optList[i].Value);
				if (optGroup.Type == OptimizationType.System)
				{
					IOptimization current = parent;
					while (current.Parent != null && current.Group.Type == OptimizationType.None && !string.IsNullOrEmpty(current.Value))
					{
						value = $"{current.Value}_{value}";
						current = current.Parent;
					}
				}
				Optimization newOpt = new Optimization
				{
					Group = optGroup,
					OptimizationGroupId = optGroup.Id,
					Parent = parent,
					ParentId = parent.Id
				};
				if (optList[i].Description != null)
				{
					newOpt.SetDescription(optList[i].Description.Trim(), DefaultLanguage);
				}
				if (optGroup.Type != OptimizationType.None)
				{
					newOpt.Value = value;
				}
				viewbox_db.DbMapping.Save(newOpt);
				if (optGroup.Type == OptimizationType.None)
				{
					newOpt.Value = value;
				}
				viewbox_db.DbMapping.Save(new OptimizationText
				{
					RefId = newOpt.Id,
					CountryCode = DefaultLanguage.CountryCode,
					Text = newOpt.Descriptions[DefaultLanguage]
				});
				_optimizations.Add(newOpt);
				(parent.Children as OptimizationCollection).Add(newOpt);
				old_id.Add(newOpt.Id, optList[i].Id);
				optList.RemoveAt(i);
				inserted = true;
			}
			if (!inserted)
			{
				throw new FormatException("wrong parent id");
			}
			FillOptimizationTree(optList, cat, old_id, viewbox_db);
		}

		private void CheckDatatypes(DatabaseBase viewbox_db, int fromId = 0)
		{
			foreach (ITableObject tobj in Objects)
			{
				if (tobj.Id >= fromId + 1)
				{
					CheckDatatypes(viewbox_db, tobj);
				}
			}
		}

		private void CheckDatatypes(DatabaseBase db, ITableObject tobject, ITableObject parentObj = null)
		{
			ITableObject tobj = parentObj ?? tobject;
			foreach (DbColumnInfo cinfo in db.GetColumnInfos(tobj.Database, tobj.TableName))
			{
				if (tobj.Columns.Contains(cinfo.Name))
				{
					global::SystemDb.Internal.Column col = tobj.Columns[cinfo.Name] as global::SystemDb.Internal.Column;
					switch (cinfo.Type)
					{
					case DbColumnTypes.DbInt:
					case DbColumnTypes.DbBigInt:
					case DbColumnTypes.DbBool:
						col.DataType = SqlType.Integer;
						break;
					case DbColumnTypes.DbNumeric:
						col.MaxLength = cinfo.NumericScale;
						col.DataType = SqlType.Decimal;
						break;
					case DbColumnTypes.DbDate:
						col.DataType = SqlType.Date;
						break;
					case DbColumnTypes.DbTime:
						col.DataType = SqlType.Time;
						break;
					case DbColumnTypes.DbDateTime:
						col.DataType = SqlType.DateTime;
						break;
					case DbColumnTypes.DbText:
					case DbColumnTypes.DbLongText:
					case DbColumnTypes.DbBinary:
					case DbColumnTypes.DbUnknown:
						col.MaxLength = cinfo.MaxLength;
						col.DataType = SqlType.String;
						break;
					}
					db.DbMapping.Save(col);
				}
			}
		}

		private void ReorderTables(DatabaseBase viewbox_db, int fromId = 0)
		{
			try
			{
				foreach (ITableObject obj in _objects)
				{
					if (obj.Id < fromId + 1 || (obj.Type != TableType.Table && obj.Type != TableType.View))
					{
						continue;
					}
					TableObject tobj = obj as TableObject;
					if (tobj.RowCount < 0)
					{
						tobj.RowCount = viewbox_db.CountTable(tobj.Database, tobj.TableName);
						viewbox_db.DbMapping.Save(tobj);
					}
					List<string> columns = (from c in viewbox_db.GetColumnNames(tobj.Database, tobj.TableName)
						select c.ToLower()).ToList();
					if (tobj.RowCount == 0L)
					{
						continue;
					}
					string order_cols = string.Empty;
					if (tobj.IndexTableColumn != null && !tobj.IndexTableColumn.IsEmpty)
					{
						if (!columns.Contains(tobj.IndexTableColumn.Name.ToLower()))
						{
							continue;
						}
						order_cols += viewbox_db.Enquote(tobj.IndexTableColumn.Name);
					}
					if (tobj.SplitTableColumn != null && !tobj.SplitTableColumn.IsEmpty)
					{
						if (!columns.Contains(tobj.SplitTableColumn.Name.ToLower()))
						{
							continue;
						}
						if (order_cols.Length > 0)
						{
							order_cols += ", ";
						}
						order_cols += viewbox_db.Enquote(tobj.SplitTableColumn.Name);
					}
					if (tobj.SortColumn != null && !tobj.SortColumn.IsEmpty)
					{
						if (!columns.Contains(tobj.SortColumn.Name.ToLower()))
						{
							continue;
						}
						if (order_cols.Length > 0)
						{
							order_cols += ", ";
						}
						order_cols += viewbox_db.Enquote(tobj.SortColumn.Name);
					}
					if (order_cols.Length != 0)
					{
						CalculateOrderArea(tobj, order_cols, viewbox_db);
					}
				}
			}
			catch (Exception)
			{
			}
		}

		public void CalculateOrderArea2(ITableObject tobj, string orderCols)
		{
			try
			{
				using DatabaseBase conn = ConnectionManager.GetConnection();
				conn.DbMapping.Delete<global::SystemDb.Internal.OrderArea>("table_id = " + tobj.Id);
				if (string.IsNullOrEmpty(orderCols))
				{
					return;
				}
				List<global::SystemDb.Internal.OrderArea> list = new List<global::SystemDb.Internal.OrderArea>();
				string select = string.Format("SELECT  MIN(_row_no_),MAX(_row_no_), {0} FROM {1} GROUP BY {0}", orderCols, conn.Enquote(tobj.Database, tobj.TableName));
				using (IDataReader reader = conn.ExecuteReader(select))
				{
					while (reader.Read())
					{
						long start = Convert.ToInt64(reader.GetValue(0));
						long end = Convert.ToInt64(reader.GetValue(1));
						int i = 1;
						string indexValue = null;
						if (tobj.IndexTableColumn != null && !tobj.IndexTableColumn.IsEmpty && !reader.IsDBNull(++i))
						{
							indexValue = reader[i].ToString();
						}
						string splitValue = null;
						if (tobj.SplitTableColumn != null && !tobj.SplitTableColumn.IsEmpty && !reader.IsDBNull(++i))
						{
							splitValue = reader[i].ToString();
						}
						string sortValue = null;
						if (tobj.SortColumn != null && !tobj.SortColumn.IsEmpty && !reader.IsDBNull(++i))
						{
							sortValue = reader[i].ToString();
						}
						list.Add(new global::SystemDb.Internal.OrderArea
						{
							TableId = tobj.Id,
							Start = start,
							End = end,
							IndexValue = indexValue,
							SplitValue = splitValue,
							SortValue = sortValue
						});
					}
				}
				conn.DbMapping.Save(typeof(global::SystemDb.Internal.OrderArea), list);
			}
			catch (Exception ex)
			{
				_log.Error($"Calculate order areas for table [{tobj.Name}] failed", ex);
				throw new Exception($"Calculate order areas for table [{tobj.Name}] failed", ex);
			}
		}

		private void FixSqlServerTypes(DatabaseBase viewbox_db)
		{
			foreach (ITableObject obj in _objects)
			{
				if (obj.Type != TableType.Table && obj.Type != TableType.View)
				{
					continue;
				}
				using IDataReader reader = viewbox_db.ExecuteReader("SELECT * FROM " + viewbox_db.Enquote(obj.Database, obj.TableName), CommandBehavior.SchemaOnly);
				foreach (DataRow row in reader.GetSchemaTable().Rows)
				{
					if (obj.Columns.Contains(row["ColumnName"].ToString()))
					{
						IColumn col = obj.Columns[row["ColumnName"].ToString()];
						string datatype = row["DataTypeName"].ToString();
						string cmd = $"SELECT MAX(DATALENGTH ({viewbox_db.Enquote(col.Name)})) FROM {viewbox_db.Enquote(obj.Database, obj.TableName)}";
						long sz = Convert.ToInt64(viewbox_db.ExecuteScalar(cmd));
						if (sz <= 8000 && datatype.ToLower().Equals("text"))
						{
							cmd = $"ALTER TABLE {viewbox_db.Enquote(obj.Database, obj.TableName)} ALTER COLUMN {viewbox_db.Enquote(col.Name)} VARCHAR({sz})";
							viewbox_db.ExecuteNonQuery(cmd);
						}
						cmd = string.Format("INSERT INTO datatypes (column_id, datatype, length, precision) VALUES ({0}, '{1}', {2}, {3})", col.Id, row["DataTypeName"], sz, row["NumericPrecision"]);
						viewbox_db.ExecuteNonQuery(cmd);
					}
				}
			}
		}
	}
}
