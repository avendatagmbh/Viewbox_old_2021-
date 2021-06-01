using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using SystemDb;
using SystemDb.Compatibility;
using AV.Log;
using DbAccess;
using DbAccess.Structures;
using ProjectDb.Tables;
using Utils;
using log4net;
using ErrorEventArgs = Utils.ErrorEventArgs;

namespace ViewBuilderBusiness.Structures.Config
{
    /// <summary>
    ///   Helper class / needed for GUI data binding reasons.
    /// </summary>
    public class MaxWorkerThreads
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="MaxWorkerThreads" /> class.
        /// </summary>
        /// <param name="value"> The value. </param>
        public MaxWorkerThreads(int value)
        {
            Value = value;
        }

        /// <summary>
        ///   Gets or sets the value.
        /// </summary>
        /// <value> The value. </value>
        public int Value { get; set; }

        /// <summary>
        ///   Gets the display.
        /// </summary>
        /// <value> The display. </value>
        public string DisplayString
        {
            get
            {
                if (Value == 1) return "1 Thread verwenden";
                else return Value.ToString() + " Threads verwenden";
            }
        }
    }

    /// <summary>
    ///   Profile configuration.
    /// </summary>
    public class ProfileConfig : INotifyPropertyChanged, IDisposable
    {
        private static readonly ILog logg = LogHelper.GetLogger();

        private static int _physicalProcessors = -1;
        private static int _physicalCores = -1;
        private readonly ILog log = LogHelper.GetLogger();

        /// <summary>
        ///   Initializes a new instance of the <see cref="ProfileConfig" /> class.
        /// </summary>
        public ProfileConfig()
        {
            Name = "<Name>";
            Description = "keine Profilbeschreibung eingegeben";
            ViewboxDbName = "";
            AutoGenerateIndex = false;

            DbConfig = new DbConfig("MySQL");

            ScriptSource = new ConfigScriptSource();

            Mail = new MailConfig();

            _viewscriptDict = new Dictionary<string, Viewscript>(StringComparer.OrdinalIgnoreCase);
            Viewscripts = new ObservableCollectionAsync<Viewscript>();

            _viewDict = new Dictionary<string, View>(StringComparer.OrdinalIgnoreCase);
            Views = new ObservableCollectionAsync<View>();
            ViewsHistory = new ObservableCollectionAsync<View>();
            AllowedMaxWorkerThreads = new List<MaxWorkerThreads>();

            int coreNumber = GetProcessorCoreNumber();
            log.Info(_physicalProcessors);
            log.Info(_physicalCores);

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
                    _physicalProcessors =
                        new ManagementObjectSearcher("Select NumberOfProcessors from Win32_ComputerSystem").Get().Count;
                }
                catch (Exception e)
                {
                    logg.Error(e);
                }
                try
                {
                    foreach (var item in new ManagementObjectSearcher("Select NumberOfCores from Win32_Processor").Get()
                        )
                    {
                        _physicalCores += int.Parse(item["NumberOfCores"].ToString());
                    }
                }
                catch (Exception e)
                {
                    logg.Error(e);
                }
            }

            var val = _physicalProcessors >= _physicalCores ? _physicalProcessors : _physicalCores;
            return val == 0 ? 4 : val;
        }

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        public void RemoveAllViewsWithName(string name)
        {
            List<View> removeViews = Views.Where(viewTemp => viewTemp.Name.ToLower() == name.ToLower()).ToList();
            foreach (var oldView in removeViews)
                Views.Remove(oldView);
            _viewDict.Remove(name);
        }

        #region events

        /// <summary>
        ///   Occurs when a property changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///   Occurs when an error occured.
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;

        #endregion events

        #region eventTrigger

        /// <summary>
        ///   Called when an error occured.
        /// </summary>
        private void OnError(string message)
        {
            if (Error != null) Error(this, new ErrorEventArgs(message));
        }

        /// <summary>
        ///   Called when a property changed.
        /// </summary>
        /// <param name="property"> The property. </param>
        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion eventTrigger

        #region fields

        /// <summary>
        ///   Help object.
        /// </summary>
        private readonly object _lock;

        /// <summary>
        ///   View dictionary.
        /// </summary>
        private readonly Dictionary<string, View> _viewDict;

        /// <summary>
        ///   Viewscript dictionary.
        /// </summary>
        private readonly Dictionary<string, Viewscript> _viewscriptDict;

        /// <summary>
        ///   See property Name.
        /// </summary>
        private bool _autoGenerateIndex;

        /// <summary>
        ///   See property ConnectionManager.
        /// </summary>
        private ConnectionManager _connectionManager;

        /// <summary>
        ///   See property Description.
        /// </summary>
        private string _description;

        /// <summary>
        ///   See property IndexDb.
        /// </summary>
        private IndexDb.IndexDb _indexDb;

        /// <summary>
        ///   See property IsIndexInitialized.
        /// </summary>
        private bool _isIndexInitialized;

        /// <summary>
        ///   See property IsInitialized.
        /// </summary>
        private bool _isInitialized;

        /// <summary>
        ///   See property Name.
        /// </summary>
        private string _name;

        /// <summary>
        ///   See property ProjectDb.
        /// </summary>
        private ProjectDb.ProjectDb _projectDb;

        /// <summary>
        ///   See property SysDb.
        /// </summary>
        private SystemDb.SystemDb _sysDb;

        private SystemDb.SystemDb _viewboxDb;

        #endregion fields

        #region persistent properties

        /// <summary>
        ///   Gets or sets the name.
        /// </summary>
        /// <value> The name. </value>
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the description.
        /// </summary>
        /// <value> The description. </value>
        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged("Description");
                }
            }
        }

        /// <summary>
        ///   Gets or sets AutoGenerateIndex.
        /// </summary>
        /// <value> The AutoGenerateIndex. </value>
        public bool AutoGenerateIndex
        {
            get { return _autoGenerateIndex; }
            set
            {
                if (_autoGenerateIndex != value)
                {
                    _autoGenerateIndex = value;
                    OnPropertyChanged("AutoGenerateIndex");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the allowed max worker threads.
        /// </summary>
        /// <value> The allowed max worker threads. </value>
        public List<MaxWorkerThreads> AllowedMaxWorkerThreads { get; set; }

        /// <summary>
        ///   Gets or sets the max worker threads.
        /// </summary>
        /// <value> The max worker threads. </value>
        public MaxWorkerThreads MaxWorkerThreads { get; set; }

        /// <summary>
        ///   Gets or sets the database config.
        /// </summary>
        /// <value> The db config. </value>
        public DbConfig DbConfig { get; set; }

        public string ViewboxDbName { get; set; }

        /// <summary>
        ///   Gets or sets the script source.
        /// </summary>
        /// <value> The script source. </value>
        public ConfigScriptSource ScriptSource { get; set; }

        /// <summary>
        ///   Gets or sets the mail config.
        /// </summary>
        /// <value> The config mail. </value>
        public MailConfig Mail { get; set; }

        /// <summary>
        ///   Gets or sets the viewscripts.
        /// </summary>
        /// <value> The viewscripts. </value>
        public ObservableCollectionAsync<Viewscript> Viewscripts { get; private set; }

        /// <summary>
        ///   Gets or sets the views.
        /// </summary>
        /// <value> The views. </value>
        public ObservableCollectionAsync<View> Views { get; private set; }

        /// <summary>
        ///   Gets or sets the ViewsHistory.
        /// </summary>
        /// <value> The views. </value>
        public ObservableCollectionAsync<View> ViewsHistory { get; private set; }

        public ObservableCollectionAsync<ITable> TableList { get; set; }

        #endregion persistent properties

        #region non persistent properties

        /// <summary>
        ///   Gets or sets the assigned user.
        /// </summary>
        /// <value> The assigned user. </value>
        public UserConfig AssignedUser { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value> <c>true</c> if this instance is initialized; otherwise, <c>false</c> . </value>
        public bool IsInitialized
        {
            get { return _isInitialized; }
            set
            {
                if (_isInitialized != value)
                {
                    _isInitialized = value;
                    OnPropertyChanged("IsInitialized");
                }
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether this index instance is initialized.
        /// </summary>
        /// <value> <c>true</c> if this instance is initialized; otherwise, <c>false</c> . </value>
        public bool IsIndexInitialized
        {
            get { return _isIndexInitialized; }
            set
            {
                if (_isIndexInitialized != value)
                {
                    _isIndexInitialized = value;
                    OnPropertyChanged("IsIndexInitialized");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the system db interface.
        /// </summary>
        /// <value> The sys db. </value>
        public SystemDb.SystemDb SysDb
        {
            get { return _sysDb; }
            private set
            {
                if (_sysDb != value)
                {
                    _sysDb = value;
                    OnPropertyChanged("SysDb");
                }
            }
        }

        public SystemDb.SystemDb ViewboxDb
        {
            get { return _viewboxDb; }
            private set
            {
                if (_viewboxDb != value)
                {
                    _viewboxDb = value;
                    OnPropertyChanged("ViewboxDb");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the project db interface.
        /// </summary>
        /// <value> The project db. </value>
        public ProjectDb.ProjectDb ProjectDb
        {
            get { return _projectDb; }
            set
            {
                if (_projectDb != value)
                {
                    _projectDb = value;
                    OnPropertyChanged("ProjectDb");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the index db interface.
        /// </summary>
        /// <value> The index db. </value>
        public IndexDb.IndexDb IndexDb
        {
            get { return _indexDb; }
            set
            {
                if (_indexDb != value)
                {
                    _indexDb = value;
                    OnPropertyChanged("IndexDb");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the connection manager.
        /// </summary>
        /// <value> The connection manager. </value>
        public ConnectionManager ConnectionManager
        {
            get { return _connectionManager; }
            private set
            {
                if (_connectionManager != value)
                {
                    _connectionManager = value;
                    OnPropertyChanged("ConnectionManager");
                }
            }
        }

        #endregion non persistent properties

        #region methods

        private List<string> _tableNames;

        /// <summary>
        ///   Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            // dispose system-db interface
            if (SysDb != null)
            {
                SysDb.Dispose();
                SysDb.ConnectionManager.Dispose();
            }
            if (ViewboxDb != null)
            {
                ViewboxDb.Dispose();
                if (ViewboxDb.ConnectionManager != null)
                    ViewboxDb.ConnectionManager.Dispose();
            }
            // dispose project-db interface
            if (ProjectDb != null)
            {
                ProjectDb.Dispose();
                ProjectDb.ConnectionManager.Dispose();
            }
            // dispose index db
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
            // dispose connection manager
            if (ConnectionManager != null)
            {
                ConnectionManager.Dispose();
                ConnectionManager = null;
            }
        }

        /// <summary>
        ///   Inits the sys db.
        /// </summary>
        public void Init()
        {
            //new Thread(InitThread).Start();
            Task task = Task.Factory.StartNew(InitThread);
            task.Wait();
        }

        /// <summary>
        ///   Initialization thread method.
        /// </summary>
        private void InitThread()
        {
            lock (_lock)
            {
                try
                {
                    IsInitialized = false;
                    IsIndexInitialized = false;
                    // remove existing connection manager and system-db instance
                    if (ConnectionManager != null) ConnectionManager.Dispose();
                    if (SysDb != null) SysDb.Dispose();
                    if (ViewboxDb != null) ViewboxDb.Dispose();
                    if (ProjectDb != null) ProjectDb.Dispose();
                    if (IndexDb != null) IndexDb.Dispose();
                    // init new connection manager
                    ConnectionManager = new ConnectionManager(DbConfig, MaxWorkerThreads.Value);
                    ConnectionManager.Init();
                    // create new project-db instance
                    ProjectDb = new ProjectDb.ProjectDb();
                    ProjectDb.PropertyChanged += ProjectDb_PropertyChanged;
                    ProjectDb.Init(DbConfig);
                }
                catch (Exception ex)
                {
                    log.Error("ProfileConfig.InitThread(): " + ex.Message, ex);
                }
            }
        }

        private void ProjectDb_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsInitialized":
                    if (IsInitialized) break;
                    try
                    {
                        // load view list

                        using (var connection = ProjectDb.ConnectionManager.GetConnection())
                            foreach (Viewscript viewscript in ProjectDb.LoadViewscripts())
                            {
                                AddViewscript(viewscript, connection);
                            }
                        List<View> views = ProjectDb.LoadViews();
                        foreach (var view in views)
                            Views.Add(view);
                        //List<View> viewsHistory = ProjectDb.LoadViewsHistory();
                        ProjectDb.LoadViewsHistory().ForEach(vh => ViewsHistory.Add(vh));
                        // create new system-db instance
                        SysDb = new SystemDb.SystemDb();
                        //this.SysDb.PropertyChanged += new PropertyChangedEventHandler(SysDb_PropertyChanged);
                        //this.SysDb.Init(this.DbConfig);
                        var newSysDbConfig = DbConfig.Clone() as DbConfig;
                        newSysDbConfig.DbName = newSysDbConfig.DbName + "_system";
                        SysDb.Connect(newSysDbConfig.DbType, newSysDbConfig.ConnectionString, MaxWorkerThreads.Value);
                        SysDb.ConnectionManager.PropertyChanged += SysDb_PropertyChanged;
                        var newViewboxDbConfig = DbConfig.Clone() as DbConfig;
                        newViewboxDbConfig.DbName = ViewboxDbName;
                        ViewboxDb = new SystemDb.SystemDb();
                        ViewboxDb.SystemDbInitialized += ViewboxDbOnSystemDbInitialized;
                        ViewboxDb.Connect(DbConfig.DbType, newViewboxDbConfig.ConnectionString, MaxWorkerThreads.Value);
                        IndexDb = new IndexDb.IndexDb();
                        IndexDb.PropertyChanged += IndexDb_PropertyChanged;
                        //if (!(IndexDb.IsInitialized.HasValue && IndexDb.IsInitialized.Value))
                        //    throw new Exception("IndexDb not initialized!");
                        //if (!(IndexDb.IsInitializedIndex.HasValue && IndexDb.IsInitializedIndex.Value))
                        //    throw new Exception("IndexDb not initialized!");
                    }
                    catch (Exception ex)
                    {
                        IsInitialized = false;
                        OnError("Failed to initialize project database connection: " + ex.Message + "\r\n" +
                                ex.StackTrace);
                    }
                    break;
            }
        }

        /// <summary>
        ///   Handles the PropertyChanged event of the SysDb control.
        /// </summary>
        /// <param name="sender"> The source of the event. </param>
        /// <param name="e"> The <see cref="System.ComponentModel.PropertyChangedEventArgs" /> instance containing the event data. </param>
        private void SysDb_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ConnectionState" &&
                (sender as ConnectionManager).ConnectionState == ConnectionStates.Online)
            {
                if (IsInitialized) return;
            }
        }

        private void ViewboxDbOnSystemDbInitialized(object sender, System.EventArgs e)
        {
            IsInitialized = true;
            try
            {
                var viewsInViewboxDb = (from obj in ViewboxDb.Objects
                                        where (obj.Type == TableType.View ||
                                               (obj.Type == TableType.Issue &&
                                                ((IIssue) obj).IssueType == IssueType.StoredProcedure))
                                              && obj.Database.ToLower() == DbConfig.DbName.ToLower()
                                        select obj).ToList();
                List<View> deletedViews = new List<View>();
                // search deleted views
                foreach (View view in Views)
                {
                    if (!viewsInViewboxDb.Exists(obj => obj.TableName.ToLower() == view.Name.ToLower()))
                        deletedViews.Add(view);
                }
                foreach (var view in deletedViews)
                {
                    //view.Delete();
                    Views.Remove(view);
                }
                // search missing views
                foreach (var viewInViewboxDb in viewsInViewboxDb)
                {
                    if (!Views.Any(view => view.Name.ToLower() == viewInViewboxDb.TableName.ToLower()))
                    {
                        View view = new View
                                        {
                                            Name = viewInViewboxDb.TableName,
                                            Description = viewInViewboxDb.Descriptions[ViewboxDb.DefaultLanguage],
                                            Agent = "Nicht mit Viewbuilder eingespielt",
                                            ProjectDb = ProjectDb
                                        };
                        Views.Add(view);
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

        /// <summary>
        ///   Handles the PropertyChanged event of the SysDb control.
        /// </summary>
        /// <param name="sender"> The source of the event. </param>
        /// <param name="e"> The <see cref="System.ComponentModel.PropertyChangedEventArgs" /> instance containing the event data. </param>
        private void IndexDb_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Initialized")
            {
                IsIndexInitialized = true;
            }
        }

        /// <summary>
        ///   Determines whether the specified viewscript does exist.
        /// </summary>
        /// <param name="view"> The viewscript. </param>
        /// <returns> <c>true</c> if the specified viewscript does exist; otherwise, <c>false</c> . </returns>
        public bool IsViewscriptExisting(Viewscript viewscript)
        {
            return _viewscriptDict.ContainsKey(viewscript.Name);
        }

        /// <summary>
        ///   Determines whether the specified view does exists.
        /// </summary>
        /// <param name="view"> The view. </param>
        /// <returns> <c>true</c> if the specified view does exist; otherwise, <c>false</c> . </returns>
        public bool IsViewExisting(string name)
        {
            return _viewDict.ContainsKey(name);
        }

        /// <summary>
        ///   Adds the specified viewscript.
        /// </summary>
        /// <param name="viewscript"> The viewscript. </param>
        /// <param name="conn"> The project db connection. </param>
        public void AddViewscript(Viewscript viewscript, IDatabase conn)
        {
            viewscript.FileName = Path.ChangeExtension(Path.Combine(ScriptSource.Directory, viewscript.Name), ".sql");
            if (viewscript.ProjectDb == null)
            {
                viewscript.ProjectDb = ProjectDb;
                viewscript.SaveOrUpdate(conn);
            }

            if (!_viewscriptDict.ContainsKey(viewscript.Name))
            {
                _viewscriptDict.Add(viewscript.Name, viewscript);
                lock (Viewscripts)
                    lock (viewscript)
                    {
                        Viewscripts.Add(viewscript);
                    }
            }
        }

        /// <summary>
        ///   Removes the specified viewscript.
        /// </summary>
        /// <param name="viewName"> Name of the viewscript. </param>
        public void RemoveViewscript(string viewName)
        {
            Viewscript viewscript = _viewscriptDict[viewName];
            lock (Viewscripts)
                lock (viewscript)
                {
                    viewscript.Delete();
                    Viewscripts.Remove(viewscript);
                    _viewscriptDict.Remove(viewscript.Name);
                }
        }

        /// <summary>
        ///   Gets the viewscript.
        /// </summary>
        /// <param name="viewName"> Name of the viewscript. </param>
        public Viewscript GetViewscript(string viewName)
        {
            if (_viewscriptDict.ContainsKey(viewName))
            {
                return _viewscriptDict[viewName];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///   Adds the specified view.
        /// </summary>
        /// <param name="view"> The view. </param>
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

        /// <summary>
        ///   Removes the specified view.
        /// </summary>
        /// <param name="viewName"> Name of the view. </param>
        public void RemoveView(string viewName)
        {
            View view = _viewDict[viewName];
            view.Delete();
            Views.Remove(view);
            _viewscriptDict.Remove(view.Name);
        }

        /// <summary>
        ///   Gets the view.
        /// </summary>
        /// <param name="viewName"> Name of the view. </param>
        public View GetView(string viewName)
        {
            if (_viewDict.ContainsKey(viewName))
            {
                return _viewDict[viewName];
            }
            else
            {
                return null;
            }
        }

        public void GetTableNames(bool forced = false)
        {
            if (forced)
                _tableNames = null;
            if (_tableNames == null || _tableNames.Count == 0)
            {
                using (var db = ConnectionManager.GetConnection())
                {
                    _tableNames = db.GetTableList();
                }
            }
        }

        public Dictionary<string, long> GetCorrectTableNames(Dictionary<string, long> tableInfos)
        {
            var correctTableInfos = new Dictionary<string, long>();
            GetTableNames();
            using (var db = ConnectionManager.GetConnection())
            {
                foreach (var t in tableInfos)
                {
                    if (_tableNames.Contains(t.Key)) correctTableInfos[t.Key] = t.Value;
                    else if (t.Key.StartsWith("_") && !correctTableInfos.ContainsKey(t.Key.Substring(1)) &&
                             _tableNames.Contains(t.Key.Substring(1)))
                        correctTableInfos.Add(t.Key.Substring(1), t.Value);
                }
            }
            return correctTableInfos;
        }

        #endregion methods
    }
}