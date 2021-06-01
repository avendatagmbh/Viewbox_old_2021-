using System;
using System.ComponentModel;
using System.Threading;
using SystemDb;
using AV.Log;
using DbAccess;
using DbAccess.Enums;
using DbAccess.Structures;
using IndexDb.IndexJob;
using log4net;
using Utils;

namespace IndexDb
{
	public class IndexDb : IDisposable, INotifyPropertyChanged
	{
		private const int ConnectFailCount = 100;

		private const string DbSuffix = "_index";

		private static IndexDb _cannonSingletonInstance;

		private static readonly object Padlock = new object();

		internal readonly ILog Log = LogHelper.GetLogger();

		private ConnectionManager _indexDbConnection;

		private bool? _isInitialized;

		private bool _isInitializedIndexDb;

		private bool _isDisposed;

		public global::SystemDb.SystemDb ViewboxDb { get; private set; }

		public ConnectionManager IndexDbConnection
		{
			get
			{
				return _indexDbConnection;
			}
			private set
			{
				if (_indexDbConnection != value)
				{
					_indexDbConnection = value;
					OnPropertyChanged("Db");
				}
			}
		}

		public bool? IsInitialized
		{
			get
			{
				return _isInitialized;
			}
			private set
			{
				if (_isInitialized != value)
				{
					_isInitialized = value;
					OnPropertyChanged("IsInitialized");
				}
			}
		}

		public bool IsDisposed
		{
			get
			{
				return _isDisposed;
			}
			set
			{
				_isDisposed = value;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string property)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		public static IndexDb GetInstance()
		{
			lock (Padlock)
			{
				return _cannonSingletonInstance ?? (_cannonSingletonInstance = new IndexDb());
			}
		}

		public void Init(global::SystemDb.SystemDb viewboxDb, int maxWorkThreads)
		{
			if (_isInitializedIndexDb)
			{
				return;
			}
			ViewboxDb = viewboxDb;
			GlobalSettings.ThreadCount = maxWorkThreads;
			using (NDC.Push(LogHelper.GetNamespaceContext()))
			{
				Log.Info("Init started");
			}
			if (viewboxDb == null)
			{
				NullReferenceException ex3 = new NullReferenceException("viewboxDbConfig must have value");
				Log.Error(ex3);
				throw ex3;
			}
			if (viewboxDb.ConnectionManager == null)
			{
				return;
			}
			DbConfig indexDbConfig = viewboxDb.ConnectionManager.DbConfig.Clone() as DbConfig;
			if (indexDbConfig == null)
			{
				return;
			}
			using (DatabaseBase databaseBase = ConnectionManager.CreateConnection(indexDbConfig))
			{
				databaseBase.Open();
				string IdxDbName = viewboxDb.ConnectionManager.DbConfig.DbName + "_index";
				databaseBase.CreateDatabaseIfNotExists(IdxDbName);
				if (!databaseBase.DatabaseExists(IdxDbName))
				{
					Exception ex2 = new Exception($"Can not create index database: {IdxDbName}");
					Log.Error(ex2);
					throw ex2;
				}
			}
			indexDbConfig.DbName = viewboxDb.ConnectionManager.DbConfig.DbName + "_index";
			using (DatabaseBase indexDbTestConnectoin = ConnectionManager.CreateConnection(indexDbConfig))
			{
				indexDbTestConnectoin.Open();
				if (!indexDbTestConnectoin.IsOpen)
				{
					Exception ex = new Exception($"Can not connect to Index server: {indexDbConfig.Hostname}");
					Log.Error(ex);
					throw ex;
				}
			}
			IndexDbConnection = new ConnectionManager(indexDbConfig, GlobalSettings.ThreadCount + 1);
			IndexDbConnection.PropertyChanged += IndexDbConnectionManagerPropertyChanged;
			if (!IndexDbConnection.IsInitialized)
			{
				IndexDbConnection.Init();
			}
			using (NDC.Push(LogHelper.GetNamespaceContext()))
			{
				Log.Info("Init ended");
			}
			Thread.Sleep(100);
			_isInitializedIndexDb = true;
		}

		public static string DoJob(global::SystemDb.SystemDb viewboxDb, ProgressCalculator progress, int maxWorkThreads, int columnId = 0, int parameterId = 0, bool recreateIfExists = true)
		{
			progress.Description = "Initializing...";
			using IndexDb target = GetInstance();
			GlobalSettings.ColumnCount = 0;
			target.Init(viewboxDb, maxWorkThreads);
			if (!target.IndexDbConnection.DbConfig.DbName.Contains(viewboxDb.ConnectionManager.DbConfig.DbName + "_index"))
			{
				return "Index database does not exists: " + viewboxDb.ConnectionManager.DbConfig.DbName + "_index";
			}
			int attempts = 0;
			while (target.IndexDbConnection.ConnectionState == ConnectionStates.Connecting)
			{
				Thread.Sleep(100);
				attempts++;
				if (100 < attempts)
				{
					break;
				}
			}
			if (target.IndexDbConnection.ConnectionState != ConnectionStates.Online)
			{
				return "Could not connect to index database: " + viewboxDb.ConnectionManager.DbConfig.DbName + "_index";
			}
			attempts = 0;
			while (!target.IsInitialized.HasValue)
			{
				Thread.Sleep(100);
				attempts++;
				if (100 < attempts)
				{
					break;
				}
			}
			if (!target.IsInitialized.HasValue || !target.IsInitialized.Value)
			{
				return "Could not initialize index database: " + viewboxDb.ConnectionManager.DbConfig.DbName + "_index";
			}
			JobQueue jobQueue = new JobQueue();
			jobQueue.QueueJobs(progress, columnId, parameterId, recreateIfExists);
			jobQueue.StartJobs(progress, maxWorkThreads);
			return "";
		}

		private void IndexDbConnectionManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			using (NDC.Push(LogHelper.GetNamespaceContext()))
			{
				Log.Info("IndexDbConnectionManagerPropertyChanged: " + e.PropertyName + " (" + ((IndexDbConnection == null) ? "" : (IndexDbConnection.ConnectionState.ToString() + ")")));
			}
			if (IndexDbConnection == null)
			{
				return;
			}
			string propertyName = e.PropertyName;
			if (!(propertyName == "ConnectionState"))
			{
				return;
			}
			ConnectionStates connectionState = IndexDbConnection.ConnectionState;
			if (connectionState != ConnectionStates.Online || (IsInitialized.HasValue && IsInitialized.Value))
			{
				return;
			}
			try
			{
				using (IndexDbConnection.GetConnection())
				{
					IsInitialized = true;
				}
			}
			catch (Exception ex)
			{
				IsInitialized = false;
				using (NDC.Push(LogHelper.GetNamespaceContext()))
				{
					Log.Error("Error in initialize index db", ex);
				}
			}
		}

		public void DeleteColumnData(int columnId)
		{
			using DatabaseBase connection = IndexDbConnection.GetConnection();
			connection.DropTableIfExists("index_" + columnId);
			connection.DropTableIfExists("value_" + columnId);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				if (disposing && IndexDbConnection != null)
				{
					IndexDbConnection.Dispose();
				}
				IndexDbConnection = null;
				_isDisposed = true;
				IsInitialized = false;
				_cannonSingletonInstance = null;
			}
		}
	}
}
