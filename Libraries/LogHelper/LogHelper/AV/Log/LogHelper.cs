using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using Utils;

namespace AV.Log
{
	public sealed class LogHelper
	{
		private sealed class DynamicLogger : Logger
		{
			internal DynamicLogger(string name)
				: base(name)
			{
			}
		}

		private class NestedSingleton
		{
			internal static readonly LogHelper singleton;

			static NestedSingleton()
			{
				singleton = new LogHelper();
			}
		}

		private const int bufferSize = 100;

		private const string logExtension = ".log";

		private const string configExtension = ".exe.config";

		private const string defaultRollingSize = "15MB";

		private const int defaultRollingCount = 10;

		private const string defaultLogPattern = "%d [%t] %-5p %c [%x] - %m%n";

		private const string defaultRepositoryName = "AvenDATA.Log";

		private const string defaultLoggerName = "logHelper";

		private static readonly SemaphoreSlim semaphoreLoggers = new SemaphoreSlim(1);

		private static readonly SemaphoreSlim semaphoreAddLogger = new SemaphoreSlim(1);

		private static readonly SemaphoreSlim semaphoreLoggerName = new SemaphoreSlim(1);

		private static readonly SemaphoreSlim semaphoreLogFilePath = new SemaphoreSlim(1);

		private static readonly SemaphoreSlim semaphoreLogEntries = new SemaphoreSlim(1);

		private static readonly SemaphoreSlim semaphoreAddLogEntry = new SemaphoreSlim(1);

		private ObservableCollectionAsync<LogEntry> _logEntries;

		private string _logFilePath;

		private string _loggerName;

		private Dictionary<string, ILog> _loggers;

		private static LogHelper Instance => NestedSingleton.singleton;

		private ObservableCollectionAsync<LogEntry> InstanceLogEntries
		{
			get
			{
				if (_logEntries == null)
				{
					semaphoreLogEntries.Wait();
					try
					{
						if (_logEntries == null)
						{
							_logEntries = new ObservableCollectionAsync<LogEntry>();
						}
					}
					finally
					{
						semaphoreLogEntries.Release();
					}
				}
				return _logEntries;
			}
		}

		private Dictionary<string, ILog> Loggers
		{
			get
			{
				if (_loggers == null)
				{
					semaphoreLoggers.Wait();
					try
					{
						if (_loggers == null)
						{
							_loggers = new Dictionary<string, ILog>();
						}
					}
					finally
					{
						semaphoreLoggers.Release();
					}
				}
				return _loggers;
			}
		}

		private static ObservableCollectionAsync<LogEntry> LogEntries => Instance.InstanceLogEntries;

		private string LoggerName
		{
			get
			{
				if (_loggerName == null)
				{
					semaphoreLoggerName.Wait();
					try
					{
						if (_loggerName == null)
						{
							Assembly assembly = Assembly.GetEntryAssembly();
							if (assembly == null)
							{
								assembly = Assembly.GetExecutingAssembly();
							}
							if (assembly != null)
							{
								_loggerName = assembly.GetName().Name;
							}
							else
							{
								_loggerName = "logHelper";
							}
						}
					}
					finally
					{
						semaphoreLoggerName.Release();
					}
				}
				return _loggerName;
			}
		}

		private string DefaultLogFilePath
		{
			get
			{
				if (_logFilePath == null)
				{
					semaphoreLogFilePath.Wait();
					try
					{
						if (_logFilePath == null)
						{
							_logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\AvenDATA\\" + LoggerName + "\\";
						}
					}
					finally
					{
						semaphoreLogFilePath.Release();
					}
				}
				return _logFilePath;
			}
		}

		public static bool PerformanceLogging { get; set; }

		private LogHelper()
		{
			XmlConfigurator.Configure();
		}

		public static ILog ConfigureLogger(ILog logger, string logFilePath)
		{
			return Instance.ConfigureAndSetLogger(logger, logFilePath);
		}

		public static ILog GetLogger()
		{
			return GetLogger(Instance.LoggerName);
		}

		private static ILog GetLogger(string loggerName)
		{
			return Instance.GetNamedLogger(null, loggerName);
		}

		public static ILog GetLogger(string repositoryName, string loggerName)
		{
			return Instance.GetNamedLogger(repositoryName, loggerName);
		}

		public static string GetMethodContext()
		{
			return new StackTrace().GetFrame(1).GetMethod().Name;
		}

		public static string GetClassContext()
		{
			MethodBase method = new StackTrace().GetFrame(1).GetMethod();
			return method.DeclaringType.Name + "." + method.Name;
		}

		public static string GetNamespaceContext()
		{
			MethodBase method = new StackTrace().GetFrame(1).GetMethod();
			return method.DeclaringType.FullName + "." + method.Name;
		}

		internal static string GetCallerMethodContext()
		{
			return new StackTrace().GetFrame(3).GetMethod().Name;
		}

		internal static string GetCallerClassContext()
		{
			MethodBase method = new StackTrace().GetFrame(3).GetMethod();
			return method.DeclaringType.Name + "." + method.Name;
		}

		internal static string GetCallerNamespaceContext()
		{
			MethodBase method = new StackTrace().GetFrame(3).GetMethod();
			return method.DeclaringType.FullName + "." + method.Name;
		}

		private ILog GetNamedLogger(string repositoryName, string loggerName)
		{
			string tmpRepositoryName = (string.IsNullOrEmpty(repositoryName) ? "AvenDATA.Log" : repositoryName);
			string tmpFullLoggerName = GetLoggerKey(tmpRepositoryName, loggerName);
			ILog logger = null;
			if (!Loggers.TryGetValue(tmpFullLoggerName, out logger))
			{
				semaphoreAddLogger.Wait();
				try
				{
					if (!Loggers.TryGetValue(tmpFullLoggerName, out logger))
					{
						if (LogManager.GetAllRepositories().All((ILoggerRepository r) => r.Name != tmpRepositoryName))
						{
							CreateRepository(tmpRepositoryName);
						}
						logger = LogManager.GetLogger(tmpRepositoryName, loggerName);
						if (!logger.Logger.Repository.Configured)
						{
							logger = ConfigureLoggerInternal(logger, null);
						}
						Loggers.Add(tmpFullLoggerName, logger);
						return logger;
					}
					return logger;
				}
				finally
				{
					semaphoreAddLogger.Release();
				}
			}
			return logger;
		}

		private void EnsureLogPath(string logPath)
		{
			string path = Path.GetDirectoryName(logPath);
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}

		private ILog ConfigureAndSetLogger(ILog logger, string logFilePath)
		{
			ILog log = Instance.ConfigureLoggerInternal(logger, logFilePath);
			semaphoreAddLogger.Wait();
			try
			{
				string loggerKey = GetLoggerKey(log.Logger.Repository.Name, log.Logger.Name);
				if (!string.IsNullOrWhiteSpace(loggerKey))
				{
					Loggers[loggerKey] = log;
					return log;
				}
				return log;
			}
			finally
			{
				semaphoreAddLogger.Release();
			}
		}

		private static ILoggerRepository CreateRepository(string repositoryName)
		{
			ILoggerRepository loggerRepository = LogManager.CreateRepository(repositoryName);
			XmlConfigurator.Configure(loggerRepository);
			return loggerRepository;
		}

		private ILog ConfigureLoggerInternal(ILog logger, string logFilePath)
		{
			string logPath = Path.Combine(DefaultLogFilePath, LoggerName + ".log");
			if (!string.IsNullOrWhiteSpace(logFilePath))
			{
				logPath = logFilePath;
			}
			EnsureLogPath(logPath);
			RollingFileAppender rollingFileAppender = new RollingFileAppender();
			rollingFileAppender.LockingModel = new FileAppender.MinimalLock();
			rollingFileAppender.AppendToFile = true;
			rollingFileAppender.RollingStyle = RollingFileAppender.RollingMode.Composite;
			rollingFileAppender.MaxSizeRollBackups = 10;
			rollingFileAppender.MaximumFileSize = "15MB";
			rollingFileAppender.DatePattern = "yyyyMMdd";
			rollingFileAppender.Layout = new PatternLayout("%d [%t] %-5p %c [%x] - %m%n");
			rollingFileAppender.File = Path.Combine(logPath);
			rollingFileAppender.StaticLogFileName = true;
			RollingFileAppender roller = rollingFileAppender;
			roller.ActivateOptions();
			Hierarchy hierarchy = (Hierarchy)logger.Logger.Repository;
			hierarchy.Root.AddAppender(roller);
			hierarchy.Root.Level = Level.All;
			hierarchy.Configured = true;
			DynamicLogger dynamicLogger = new DynamicLogger(logger.Logger.Name);
			dynamicLogger.Hierarchy = hierarchy;
			dynamicLogger.Level = Level.All;
			dynamicLogger.AddAppender(roller);
			return new LogImpl(dynamicLogger);
		}

		private static string GetLoggerKey(string repositoryName, string loggerName)
		{
			return repositoryName + "_" + loggerName;
		}

		internal static void AddLogEntry(LogEntry entry)
		{
			semaphoreAddLogEntry.Wait();
			try
			{
				((Collection<LogEntry>)(object)LogEntries).Insert(0, entry);
				if (((Collection<LogEntry>)(object)LogEntries).Count > 100)
				{
					((Collection<LogEntry>)(object)LogEntries).RemoveAt(((Collection<LogEntry>)(object)LogEntries).Count - 1);
				}
			}
			finally
			{
				semaphoreAddLogEntry.Release();
			}
		}
	}
}
