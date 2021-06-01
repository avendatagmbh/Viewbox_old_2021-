using System;
using System.Collections.Generic;
using System.ComponentModel;
using AV.Log;
using DbAccess;
using DbAccess.Enums;
using DbAccess.Structures;
using log4net;
using Utils;
using ViewboxBusiness.ProfileDb.Tables;

namespace ViewboxBusiness.ProfileDb
{
	public class ProjectDb : IDisposable, INotifyPropertyChanged
	{
		public const string DbSuffix = "_project";

		private ConnectionManager _connectionManager;

		private string _dbVersion;

		private bool _isInitialized;

		private DbConfig _prjDbConfig;

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

		public ConnectionManager ConnectionManager
		{
			get
			{
				return _connectionManager;
			}
			set
			{
				if (_connectionManager != value)
				{
					_connectionManager = value;
					OnPropertyChanged("ConnectionManager");
				}
			}
		}

		public string DbVersion
		{
			get
			{
				return _dbVersion;
			}
			private set
			{
				if (_dbVersion != value)
				{
					_dbVersion = value;
					OnPropertyChanged("DbVersion");
				}
			}
		}

		public string DataDbName { get; private set; }

		public string RelationsFilePath
		{
			get
			{
				using DatabaseBase conn = ConnectionManager.GetConnection();
				return Info.GetValue(conn, "RelationsCsvPath");
			}
			set
			{
				using DatabaseBase conn = ConnectionManager.GetConnection();
				Info.SetValue(conn, "RelationsCsvPath", value);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public event EventHandler<ErrorEventArgs> Error;

		private void OnError(string message)
		{
			if (this.Error != null)
			{
				this.Error(this, new ErrorEventArgs(message));
			}
		}

		private void OnPropertyChanged(string property)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		private void ConnectionManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (ConnectionManager == null)
			{
				return;
			}
			string propertyName = e.PropertyName;
			if (!(propertyName == "ConnectionState"))
			{
				return;
			}
			ConnectionStates connectionState = ConnectionManager.ConnectionState;
			if (connectionState != ConnectionStates.Online || IsInitialized)
			{
				return;
			}
			try
			{
				using (DatabaseBase conn = ConnectionManager.GetConnection())
				{
					Info.CreateTable(conn);
					conn.DbMapping.CreateTableIfNotExists<Viewscript>();
					conn.DbMapping.CreateTableIfNotExists<View>();
					DetectDbVersion(conn);
					if (DbVersion == "1.0.0")
					{
						conn.DropTableIfExists("info");
						Info.CreateTable(conn);
					}
					new DbUpgrader().UpgradeDb(_prjDbConfig);
				}
				IsInitialized = true;
			}
			catch (Exception ex)
			{
				using (NDC.Push(LogHelper.GetNamespaceContext()))
				{
					LogHelper.GetLogger().Error("Error while initializing ProjectDb", ex);
				}
				IsInitialized = false;
				OnError("Failed to initialize the project database interface: " + ex.Message);
			}
		}

		public void Dispose()
		{
			if (_connectionManager != null)
			{
				_connectionManager.Dispose();
			}
		}

		public void Notify()
		{
			OnPropertyChanged("ConnectionState");
		}

		public void Init(DbConfig dbConfig)
		{
			try
			{
				using DatabaseBase conn = ConnectionManager.CreateConnection(dbConfig);
				conn.Open();
				conn.CreateDatabaseIfNotExists(dbConfig.DbName + "_project");
			}
			catch (Exception ex)
			{
				OnError("Error when creating the project database: " + ex.Message);
			}
			_prjDbConfig = dbConfig.Clone() as DbConfig;
			if (_prjDbConfig != null)
			{
				_prjDbConfig.DbName = dbConfig.DbName + "_project";
			}
			DataDbName = dbConfig.DbName;
			ConnectionManager = new ConnectionManager(_prjDbConfig, 2);
			ConnectionManager.PropertyChanged += ConnectionManagerPropertyChanged;
			if (!ConnectionManager.IsInitialized)
			{
				ConnectionManager.Init();
			}
		}

		public List<Viewscript> LoadViewscripts()
		{
			using DatabaseBase conn = ConnectionManager.GetConnection();
			List<Viewscript> viewscripts = Viewscript.Load(conn);
			foreach (Viewscript item in viewscripts)
			{
				item.ProjectDb = this;
			}
			return viewscripts;
		}

		public List<View> LoadViews()
		{
			using DatabaseBase conn = ConnectionManager.GetConnection();
			List<View> views = View.LoadLatestRecords(conn);
			foreach (View item in views)
			{
				item.ProjectDb = this;
			}
			return views;
		}

		public List<View> LoadViewsHistory()
		{
			using DatabaseBase conn = ConnectionManager.GetConnection();
			List<View> views = View.Load(conn);
			foreach (View item in views)
			{
				item.ProjectDb = this;
			}
			return views;
		}

		private void DetectDbVersion(DatabaseBase conn)
		{
			if (!conn.GetTableList().Contains("info"))
			{
				throw new Exception("Missing Table 'info' in project database.");
			}
			DbVersion = Info.GetValue(conn, Info.KeyVersion);
		}
	}
}
