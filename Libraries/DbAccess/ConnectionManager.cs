using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using DbAccess.Enums;
using DbAccess.Structures;

namespace DbAccess
{
	public class ConnectionManager : IDisposable, INotifyPropertyChanged
	{
		private class PooledConnection : IDisposable
		{
			public DatabaseBase Database { get; private set; }

			public DateTime LastAccess { get; set; }

			public PooledConnection(DatabaseBase database)
			{
				Database = database;
				LastAccess = DateTime.Now;
				if (Database != null)
				{
					Database.IsManaged = true;
				}
			}

			public void Dispose()
			{
				Database.IsManaged = false;
				Database.Dispose();
			}
		}

		private CancellationTokenSource _cleanUpThreadCt = new CancellationTokenSource();

		private CancellationTokenSource _connectionWatchdogThreadCt = new CancellationTokenSource();

		private CancellationTokenSource _connectionResetThreadCt = new CancellationTokenSource();

		private readonly DbConfig _dbConfig;

		private readonly int _maxConnections;

		private ConnectionStates _connectionState;

		private bool _isInitialized;

		private bool _isDisposed;

		public object _initLocker = new object();

		public static List<string> DbTypeNames { get; private set; }

		public ConnectionStates ConnectionState
		{
			get
			{
				return _connectionState;
			}
			private set
			{
				if (_connectionState != value)
				{
					_connectionState = value;
					OnPropertyChanged("ConnectionState");
				}
			}
		}

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

		public bool IsDisposed
		{
			get
			{
				return _isDisposed;
			}
			private set
			{
				if (_isDisposed != value)
				{
					_isDisposed = value;
					OnPropertyChanged("IsDisposed");
				}
			}
		}

		public DbConfig DbConfig => _dbConfig;

		public event PropertyChangedEventHandler PropertyChanged;

		public ConnectionManager(string connectionString, int maxConnections, DbTemplate dbTemplate = null)
			: this(new DbConfig
			{
				ConnectionString = connectionString
			}, maxConnections, dbTemplate)
		{
		}

		public ConnectionManager(DbConfig dbConfig, int maxConnections, DbTemplate dbTemplate = null)
		{
			_dbConfig = dbConfig;
			_maxConnections = maxConnections;
			IsDisposed = false;
			IsInitialized = false;
			_dbConfig.PropertyChanged += _dbConfig_PropertyChanged;
			if (dbConfig.DbTemplate == null)
			{
				_dbConfig.DbTemplate = dbTemplate;
			}
		}

		private void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private void _dbConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
		}

		public void Notify()
		{
			OnPropertyChanged("ConnectionState");
		}

		public static DatabaseBase CreateConnection(string dbType, string host, string user, string password, DbTemplate dbTemplate = null)
		{
			return CreateConnection(dbType, host, user, password, 3306, dbTemplate);
		}

		public static DatabaseBase CreateConnection(string dbType, string host, string user, string password, int port, DbTemplate dbTemplate = null)
		{
			return CreateConnection(new DbConfig
			{
				Hostname = host,
				Username = user,
				Password = password,
				Port = port,
				DbTemplate = dbTemplate
			});
		}

		public static DatabaseBase CreateConnection(DbConfig dbConfig)
		{
			return new DatabaseBase(dbConfig);
		}

		public static DatabaseBase CreateConnection(string provider, string connectionString, DbTemplate dbTemplate = null)
		{
			return CreateConnection(new DbConfig
			{
				ConnectionString = connectionString,
				DbTemplate = dbTemplate
			});
		}

		public void Init()
		{
			if (IsInitialized)
			{
				return;
			}
			lock (_initLocker)
			{
				Task.Factory.StartNew(delegate
				{
					ConnectionWatchdog();
				}, _connectionWatchdogThreadCt.Token);
				IsInitialized = true;
			}
		}

		public DatabaseBase GetConnection()
		{
			DatabaseBase databaseBase = CreateConnection(_dbConfig);
			databaseBase.Open();
			return databaseBase;
		}

		public void Dispose()
		{
			Task.Factory.StartNew(delegate
			{
				DoDispose();
			});
		}

		private void DoDispose()
		{
			IsDisposed = true;
			ConnectionState = ConnectionStates.Offline;
			_connectionWatchdogThreadCt.Cancel();
		}

		private void ConnectionWatchdog()
		{
			try
			{
				while (!IsDisposed)
				{
					UpdateConnectionState();
					Thread.Sleep((ConnectionState == ConnectionStates.Online) ? 30000 : 10000);
				}
			}
			catch (ThreadAbortException)
			{
			}
			catch (Exception)
			{
			}
		}

		public void UpdateConnectionState()
		{
			if (ConnectionState != ConnectionStates.Online)
			{
				ConnectionState = ConnectionStates.Connecting;
			}
			try
			{
				using DatabaseBase conn = CreateConnection(_dbConfig);
				ConnectionState = (conn.TestConnection() ? ConnectionStates.Online : ConnectionStates.Offline);
			}
			catch
			{
				ConnectionState = ConnectionStates.Offline;
			}
		}

		public static string GetDatabase(string provider, string connectionString)
		{
			return new DbConfig
			{
				ConnectionString = connectionString
			}.DbName;
		}

		public void SetupTimeOuts(int timeOut)
		{
			using DatabaseBase conn = GetConnection();
			string timeOutQuery = string.Format("SET GLOBAL interactive_timeout = {0};SET GLOBAL wait_timeout = {0};", timeOut);
			conn.ExecuteNonQuery(timeOutQuery);
		}
	}
}
