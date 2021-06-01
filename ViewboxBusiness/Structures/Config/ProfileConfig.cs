using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using SystemDb;
using AV.Log;
using DbAccess;
using DbAccess.Enums;
using DbAccess.Structures;
using IndexDb;
using log4net;
using Utils;
using ViewboxBusiness.ProfileDb;
using ViewboxBusiness.ProfileDb.Tables;

namespace ViewboxBusiness.Structures.Config
{
	public class ProfileConfig : INotifyPropertyChanged, IDisposable
	{
		private static readonly ILog Log = LogHelper.GetLogger();

		private static int _physicalProcessors = -1;

		private static int _physicalCores = -1;

		private readonly ILog _log = LogHelper.GetLogger();

		private readonly object _lock;

		private readonly Dictionary<string, View> _viewDict;

		private readonly Dictionary<string, Viewscript> _viewscriptDict;

		private bool _autoGenerateIndex;

		private ConnectionManager _connectionManager;

		private string _description;

		private global::IndexDb.IndexDb _indexDb;

		private bool _isIndexInitialized;

		private bool _isInitialized;

		private string _name;

		private ProjectDb _projectDb;

		private global::SystemDb.SystemDb _sysDb;

		private global::SystemDb.SystemDb _viewboxDb;

		private List<string> _tableNames;

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if (_name != value)
				{
					_name = value;
					OnPropertyChanged("Name");
				}
			}
		}

		public string Description
		{
			get
			{
				return _description;
			}
			set
			{
				if (_description != value)
				{
					_description = value;
					OnPropertyChanged("Description");
				}
			}
		}

		public bool AutoGenerateIndex
		{
			get
			{
				return _autoGenerateIndex;
			}
			set
			{
				if (_autoGenerateIndex != value)
				{
					_autoGenerateIndex = value;
					OnPropertyChanged("AutoGenerateIndex");
				}
			}
		}

		public List<MaxWorkerThreads> AllowedMaxWorkerThreads { get; set; }

		public MaxWorkerThreads MaxWorkerThreads { get; set; }

		public DbConfig DbConfig { get; set; }

		public string ViewboxDbName { get; set; }

		public ConfigScriptSource ScriptSource { get; set; }

		public MailConfig Mail { get; set; }

		public ObservableCollectionAsync<Viewscript> Viewscripts { get; private set; }

		public ObservableCollectionAsync<View> Views { get; private set; }

		public ObservableCollectionAsync<View> ViewsHistory { get; private set; }

		public ObservableCollectionAsync<ITable> TableList { get; set; }

		public UserConfig AssignedUser { get; set; }

		public bool IsInitialized
		{
			get
			{
				return _isInitialized;
			}
			set
			{
				if (_isInitialized != value)
				{
					_isInitialized = value;
					OnPropertyChanged("IsInitialized");
				}
			}
		}

		public bool IsIndexInitialized
		{
			get
			{
				return _isIndexInitialized;
			}
			set
			{
				if (_isIndexInitialized != value)
				{
					_isIndexInitialized = value;
					OnPropertyChanged("IsIndexInitialized");
				}
			}
		}

		public global::SystemDb.SystemDb SysDb
		{
			get
			{
				return _sysDb;
			}
			private set
			{
				if (_sysDb != value)
				{
					_sysDb = value;
					OnPropertyChanged("SysDb");
				}
			}
		}

		public global::SystemDb.SystemDb ViewboxDb
		{
			get
			{
				return _viewboxDb;
			}
			private set
			{
				if (_viewboxDb != value)
				{
					_viewboxDb = value;
					OnPropertyChanged("ViewboxDb");
				}
			}
		}

		public ProjectDb ProjectDb
		{
			get
			{
				return _projectDb;
			}
			set
			{
				if (_projectDb != value)
				{
					_projectDb = value;
					OnPropertyChanged("ProjectDb");
				}
			}
		}

		public global::IndexDb.IndexDb IndexDb
		{
			get
			{
				return _indexDb;
			}
			set
			{
				if (_indexDb != value)
				{
					_indexDb = value;
					OnPropertyChanged("IndexDb");
				}
			}
		}

		public ConnectionManager ConnectionManager
		{
			get
			{
				return _connectionManager;
			}
			private set
			{
				if (_connectionManager != value)
				{
					_connectionManager = value;
					OnPropertyChanged("ConnectionManager");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public event EventHandler<Utils.ErrorEventArgs> Error;

		public ProfileConfig()
		{
			Name = "<Name>";
			Description = "keine Profilbeschreibung eingegeben";
			ViewboxDbName = "";
			AutoGenerateIndex = false;
			DbConfig = new DbConfig();
			ScriptSource = new ConfigScriptSource();
			Mail = new MailConfig();
			_viewscriptDict = new Dictionary<string, Viewscript>(StringComparer.OrdinalIgnoreCase);
			Viewscripts = new ObservableCollectionAsync<Viewscript>();
			_viewDict = new Dictionary<string, View>(StringComparer.OrdinalIgnoreCase);
			Views = new ObservableCollectionAsync<View>();
			ViewsHistory = new ObservableCollectionAsync<View>();
			AllowedMaxWorkerThreads = new List<MaxWorkerThreads>();
			int coreNumber = GetProcessorCoreNumber();
			_log.Info(_physicalProcessors);
			_log.Info(_physicalCores);
			for (int i = 2; i <= coreNumber; i++)
			{
				AllowedMaxWorkerThreads.Add(new MaxWorkerThreads(i));
			}
			MaxWorkerThreads = AllowedMaxWorkerThreads[coreNumber - 2];
			IsInitialized = false;
			IsIndexInitialized = false;
			_lock = new object();
		}

		public static int GetProcessorCoreNumber()
		{
			if (_physicalProcessors == -1 || _physicalCores == -1)
			{
				_physicalProcessors = 0;
				_physicalCores = 0;
				try
				{
					_physicalProcessors = new ManagementObjectSearcher("Select NumberOfProcessors from Win32_ComputerSystem").Get().Count;
				}
				catch (Exception e2)
				{
					Log.Error(e2);
				}
				try
				{
					foreach (ManagementBaseObject item in new ManagementObjectSearcher("Select NumberOfCores from Win32_Processor").Get())
					{
						_physicalCores += int.Parse(item["NumberOfCores"].ToString());
					}
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
			int val = ((_physicalProcessors >= _physicalCores) ? _physicalProcessors : _physicalCores);
			if (val != 0)
			{
				return val;
			}
			return 4;
		}

		public void RemoveAllViewsWithName(string name)
		{
			foreach (View oldView in Views.Where((View viewTemp) => viewTemp.Name.ToLower() == name.ToLower()).ToList())
			{
				Views.Remove(oldView);
			}
			_viewDict.Remove(name);
		}

		private void OnError(string message)
		{
			if (this.Error != null)
			{
				this.Error(this, new Utils.ErrorEventArgs(message));
			}
		}

		private void OnPropertyChanged(string property)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		public void Dispose()
		{
			if (SysDb != null)
			{
				SysDb.Dispose();
				SysDb.ConnectionManager.Dispose();
			}
			if (ViewboxDb != null)
			{
				ViewboxDb.Dispose();
				if (ViewboxDb.ConnectionManager != null)
				{
					ViewboxDb.ConnectionManager.Dispose();
				}
			}
			if (ProjectDb != null)
			{
				ProjectDb.Dispose();
				ProjectDb.ConnectionManager.Dispose();
			}
			if (IndexDb != null)
			{
				IndexDb.Dispose();
			}
			if (SysDb != null)
			{
				SysDb.ConnectionManager = null;
			}
			if (ViewboxDb != null)
			{
				ViewboxDb.ConnectionManager = null;
			}
			if (ProjectDb != null)
			{
				ProjectDb.ConnectionManager = null;
			}
			if (ConnectionManager != null)
			{
				ConnectionManager.Dispose();
				ConnectionManager = null;
			}
		}

		public void Init()
		{
			Task.Factory.StartNew(InitThread).Wait();
		}

		private void InitThread()
		{
			lock (_lock)
			{
				try
				{
					IsInitialized = false;
					IsIndexInitialized = false;
					if (ConnectionManager != null)
					{
						ConnectionManager.Dispose();
					}
					if (SysDb != null)
					{
						SysDb.Dispose();
					}
					if (ViewboxDb != null)
					{
						ViewboxDb.Dispose();
					}
					if (ProjectDb != null)
					{
						ProjectDb.Dispose();
					}
					if (IndexDb != null)
					{
						IndexDb.Dispose();
					}
					ConnectionManager = new ConnectionManager(DbConfig, MaxWorkerThreads.Value);
					if (!ConnectionManager.IsInitialized)
					{
						ConnectionManager.Init();
					}
					ProjectDb = new ProjectDb();
					ProjectDb.PropertyChanged += ProjectDbPropertyChanged;
					ProjectDb.Init(DbConfig);
				}
				catch (Exception ex)
				{
					_log.Error("ProfileConfig.InitThread(): " + ex.Message, ex);
				}
			}
		}

		private void ProjectDbPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			string propertyName = e.PropertyName;
			if (!(propertyName == "IsInitialized") || IsInitialized)
			{
				return;
			}
			try
			{
				using (DatabaseBase connection = ProjectDb.ConnectionManager.GetConnection())
				{
					foreach (Viewscript viewscript in ProjectDb.LoadViewscripts())
					{
						AddViewscript(viewscript, connection);
					}
				}
				foreach (View view in ProjectDb.LoadViews())
				{
					Views.Add(view);
				}
				ProjectDb.LoadViewsHistory().ForEach(delegate(View vh)
				{
					ViewsHistory.Add(vh);
				});
				SysDb = new global::SystemDb.SystemDb();
				DbConfig newSysDbConfig = DbConfig.Clone() as DbConfig;
				if (newSysDbConfig != null)
				{
					newSysDbConfig.DbName += "_system";
					SysDb.Connect(newSysDbConfig.ConnectionString, MaxWorkerThreads.Value);
				}
				SysDb.ConnectionManager.PropertyChanged += SysDbPropertyChanged;
				DbConfig newViewboxDbConfig = DbConfig.Clone() as DbConfig;
				if (newViewboxDbConfig != null)
				{
					newViewboxDbConfig.DbName = ViewboxDbName;
				}
				ViewboxDb = new global::SystemDb.SystemDb();
				ViewboxDb.SystemDbInitialized += ViewboxDbOnSystemDbInitialized;
				if (newViewboxDbConfig != null)
				{
					ViewboxDb.Connect(newViewboxDbConfig.ConnectionString, MaxWorkerThreads.Value);
				}
				IndexDb = new global::IndexDb.IndexDb();
				IndexDb.PropertyChanged += IndexDbPropertyChanged;
			}
			catch (Exception ex)
			{
				IsInitialized = false;
				OnError("Failed to initialize project database connection: " + ex.Message + "\r\n" + ex.StackTrace);
			}
		}

		private void SysDbPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ConnectionManager connectionManager = sender as ConnectionManager;
			if (connectionManager != null && e.PropertyName == "ConnectionState" && connectionManager.ConnectionState == ConnectionStates.Online)
			{
				_ = IsInitialized;
			}
		}

		private void ViewboxDbOnSystemDbInitialized(object sender, EventArgs e)
		{
			IsInitialized = true;
			try
			{
				List<ITableObject> viewsInViewboxDb = ViewboxDb.Objects.Where((ITableObject obj) => (obj.Type == TableType.View || (obj.Type == TableType.Issue && ((IIssue)obj).IssueType == IssueType.StoredProcedure)) && obj.Database.ToLower() == DbConfig.DbName.ToLower()).ToList();
				foreach (View view3 in Views.Where((View view) => !viewsInViewboxDb.Exists((ITableObject obj) => obj.TableName.ToLower() == view.Name.ToLower())).ToList())
				{
					Views.Remove(view3);
				}
				foreach (ITableObject viewInViewboxDb in viewsInViewboxDb)
				{
					if (!Views.Any((View view) => view.Name.ToLower() == viewInViewboxDb.TableName.ToLower()))
					{
						View view2 = new View
						{
							Name = viewInViewboxDb.TableName,
							Description = viewInViewboxDb.Descriptions[ViewboxDb.DefaultLanguage],
							Agent = "Nicht mit Viewbuilder eingespielt",
							ProjectDb = ProjectDb
						};
						Views.Add(view2);
					}
				}
				try
				{
					IndexDb.Init(ViewboxDb, 4);
				}
				catch
				{
				}
				ConnectionManager.Notify();
				ProjectDb.Notify();
			}
			catch (Exception ex)
			{
				IsInitialized = false;
				OnError("Failed to initialize system database connection: " + ex.Message);
			}
		}

		private void IndexDbPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Initialized")
			{
				IsIndexInitialized = true;
			}
		}

		public bool IsViewscriptExisting(Viewscript viewscript)
		{
			return _viewscriptDict.ContainsKey(viewscript.Name);
		}

		public bool IsViewExisting(string name)
		{
			return _viewDict.ContainsKey(name);
		}

		public void AddViewscript(Viewscript viewscript, DatabaseBase conn)
		{
			viewscript.FileName = Path.ChangeExtension(Path.Combine(ScriptSource.Directory, viewscript.Name), ".sql");
			if (viewscript.ProjectDb == null)
			{
				viewscript.ProjectDb = ProjectDb;
				viewscript.SaveOrUpdate(conn);
			}
			if (_viewscriptDict.ContainsKey(viewscript.Name))
			{
				return;
			}
			_viewscriptDict.Add(viewscript.Name, viewscript);
			lock (Viewscripts)
			{
				lock (viewscript)
				{
					Viewscripts.Add(viewscript);
				}
			}
		}

		public void RemoveViewscript(string viewName)
		{
			Viewscript viewscript = _viewscriptDict[viewName];
			lock (Viewscripts)
			{
				lock (viewscript)
				{
					viewscript.Delete();
					Viewscripts.Remove(viewscript);
					_viewscriptDict.Remove(viewscript.Name);
				}
			}
		}

		public Viewscript GetViewscript(string viewName)
		{
			if (_viewscriptDict.ContainsKey(viewName))
			{
				return _viewscriptDict[viewName];
			}
			return null;
		}

		public void AddView(View view)
		{
			_viewDict.Add(view.Name, view);
			Views.Add(view);
			if (view.ProjectDb == null)
			{
				view.ProjectDb = ProjectDb;
				view.SaveOrUpdate();
			}
		}

		public void RemoveView(string viewName)
		{
			View view = _viewDict[viewName];
			view.Delete();
			Views.Remove(view);
			_viewscriptDict.Remove(view.Name);
		}

		public View GetView(string viewName)
		{
			if (_viewDict.ContainsKey(viewName))
			{
				return _viewDict[viewName];
			}
			return null;
		}

		public void GetTableNames(bool forced = false)
		{
			if (forced)
			{
				_tableNames = null;
			}
			if (_tableNames == null || _tableNames.Count == 0)
			{
				using DatabaseBase db = ConnectionManager.GetConnection();
				_tableNames = db.GetTableList();
			}
		}

		public Dictionary<string, long> GetCorrectTableNames(Dictionary<string, long> tableInfos)
		{
			Dictionary<string, long> correctTableInfos = new Dictionary<string, long>();
			GetTableNames();
			foreach (KeyValuePair<string, long> t in tableInfos)
			{
				if (_tableNames.Contains(t.Key))
				{
					correctTableInfos[t.Key] = t.Value;
				}
				else if (t.Key.StartsWith("_") && !correctTableInfos.ContainsKey(t.Key.Substring(1)) && _tableNames.Contains(t.Key.Substring(1)))
				{
					correctTableInfos.Add(t.Key.Substring(1), t.Value);
				}
			}
			return correctTableInfos;
		}
	}
}
