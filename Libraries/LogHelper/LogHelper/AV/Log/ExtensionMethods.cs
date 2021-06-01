using System;
using log4net;

namespace AV.Log
{
	public static class ExtensionMethods
	{
		public static void ContextLog(this ILog log, LogLevelEnum logLevel, string message)
		{
			switch (logLevel)
			{
			case LogLevelEnum.Debug:
				if (log.IsDebugEnabled)
				{
					DoLogWithContext(LogContextEnum.ClassMethod, delegate
					{
						log.DebugFormat(message);
					});
				}
				break;
			case LogLevelEnum.Info:
				if (log.IsInfoEnabled)
				{
					DoLogWithContext(LogContextEnum.ClassMethod, delegate
					{
						log.InfoFormat(message);
					});
				}
				break;
			case LogLevelEnum.Warn:
				if (log.IsWarnEnabled)
				{
					DoLogWithContext(LogContextEnum.ClassMethod, delegate
					{
						log.WarnFormat(message);
					});
				}
				break;
			case LogLevelEnum.Error:
				if (log.IsErrorEnabled)
				{
					DoLogWithContext(LogContextEnum.ClassMethod, delegate
					{
						log.ErrorFormat(message);
					});
				}
				break;
			case LogLevelEnum.Fatal:
				if (log.IsFatalEnabled)
				{
					DoLogWithContext(LogContextEnum.ClassMethod, delegate
					{
						log.FatalFormat(message);
					});
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		public static void ContextLog(this ILog log, LogLevelEnum logLevel, string format, params object[] args)
		{
			switch (logLevel)
			{
			case LogLevelEnum.Debug:
				if (log.IsDebugEnabled)
				{
					DoLogWithContext(LogContextEnum.ClassMethod, delegate
					{
						log.DebugFormat(format, args);
					});
				}
				break;
			case LogLevelEnum.Info:
				if (log.IsInfoEnabled)
				{
					DoLogWithContext(LogContextEnum.ClassMethod, delegate
					{
						log.InfoFormat(format, args);
					});
				}
				break;
			case LogLevelEnum.Warn:
				if (log.IsWarnEnabled)
				{
					DoLogWithContext(LogContextEnum.ClassMethod, delegate
					{
						log.WarnFormat(format, args);
					});
				}
				break;
			case LogLevelEnum.Error:
				if (log.IsErrorEnabled)
				{
					DoLogWithContext(LogContextEnum.ClassMethod, delegate
					{
						log.ErrorFormat(format, args);
					});
				}
				break;
			case LogLevelEnum.Fatal:
				if (log.IsFatalEnabled)
				{
					DoLogWithContext(LogContextEnum.ClassMethod, delegate
					{
						log.FatalFormat(format, args);
					});
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		public static void Log(this ILog log, LogLevelEnum logLevel, string message)
		{
			log.Log(logLevel, message, LogContextEnum.None, collectEntry: false);
		}

		public static void Log(this ILog log, LogLevelEnum logLevel, string message, bool collectEntry = false)
		{
			log.Log(logLevel, message, LogContextEnum.None, collectEntry);
		}

		private static void Log(this ILog log, LogLevelEnum logLevel, string message, LogContextEnum withContext = LogContextEnum.None, bool collectEntry = false)
		{
			switch (logLevel)
			{
			case LogLevelEnum.Debug:
				if (log.IsDebugEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.Debug(message);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			case LogLevelEnum.Info:
				if (log.IsInfoEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.Info(message);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			case LogLevelEnum.Warn:
				if (log.IsWarnEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.Warn(message);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			case LogLevelEnum.Error:
				if (log.IsErrorEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.Error(message);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			case LogLevelEnum.Fatal:
				if (log.IsFatalEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.Fatal(message);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (collectEntry)
			{
				LogHelper.AddLogEntry(new LogEntry(logLevel, message)
				{
					Exception = null
				});
			}
		}

		public static void Log(this ILog log, LogLevelEnum logLevel, string message, Exception exception, bool collectEntry = false)
		{
			log.Log(logLevel, message, exception, LogContextEnum.None, collectEntry);
		}

		private static void Log(this ILog log, LogLevelEnum logLevel, string message, Exception exception, LogContextEnum withContext = LogContextEnum.None, bool collectEntry = false)
		{
			switch (logLevel)
			{
			case LogLevelEnum.Debug:
				if (log.IsDebugEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.Debug(message, exception);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			case LogLevelEnum.Info:
				if (log.IsInfoEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.Info(message, exception);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			case LogLevelEnum.Warn:
				if (log.IsWarnEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.Warn(message, exception);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			case LogLevelEnum.Error:
				if (log.IsErrorEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.Error(message, exception);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			case LogLevelEnum.Fatal:
				if (log.IsFatalEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.Fatal(message, exception);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (collectEntry)
			{
				LogHelper.AddLogEntry(new LogEntry(logLevel, message)
				{
					Exception = exception
				});
			}
		}

		public static void Log(this ILog log, LogLevelEnum logLevel, string format, LogContextEnum withContext = LogContextEnum.None, bool collectEntry = false, params object[] args)
		{
			switch (logLevel)
			{
			case LogLevelEnum.Debug:
				if (log.IsDebugEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.DebugFormat(format, args);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			case LogLevelEnum.Info:
				if (log.IsInfoEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.InfoFormat(format, args);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			case LogLevelEnum.Warn:
				if (log.IsWarnEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.WarnFormat(format, args);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			case LogLevelEnum.Error:
				if (log.IsErrorEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.ErrorFormat(format, args);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			case LogLevelEnum.Fatal:
				if (log.IsFatalEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.FatalFormat(format, args);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (collectEntry)
			{
				LogHelper.AddLogEntry(new LogEntry(logLevel, string.Format(format, args))
				{
					Exception = null
				});
			}
		}

		public static void Log(this ILog log, LogLevelEnum logLevel, string format, Exception exception, LogContextEnum withContext = LogContextEnum.None, bool collectEntry = false, params object[] args)
		{
			switch (logLevel)
			{
			case LogLevelEnum.Debug:
				if (log.IsDebugEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.DebugFormat(format, args);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			case LogLevelEnum.Info:
				if (log.IsInfoEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.InfoFormat(format, args);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			case LogLevelEnum.Warn:
				if (log.IsWarnEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.WarnFormat(format, args);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			case LogLevelEnum.Error:
				if (log.IsErrorEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.ErrorFormat(format, args);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			case LogLevelEnum.Fatal:
				if (log.IsFatalEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.FatalFormat(format, args);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (collectEntry)
			{
				LogHelper.AddLogEntry(new LogEntry(logLevel, string.Format(format, args))
				{
					Exception = exception
				});
			}
		}

		public static void Log(this ILog log, LogLevelEnum logLevel, IFormatProvider provider, string format, LogContextEnum withContext = LogContextEnum.None, bool collectEntry = false, params object[] args)
		{
			switch (logLevel)
			{
			case LogLevelEnum.Debug:
				if (log.IsDebugEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.DebugFormat(provider, format, args);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			case LogLevelEnum.Info:
				if (log.IsInfoEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.InfoFormat(provider, format, args);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			case LogLevelEnum.Warn:
				if (log.IsWarnEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.WarnFormat(provider, format, args);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			case LogLevelEnum.Error:
				if (log.IsErrorEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.ErrorFormat(provider, format, args);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			case LogLevelEnum.Fatal:
				if (log.IsFatalEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.FatalFormat(provider, format, args);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (collectEntry)
			{
				LogHelper.AddLogEntry(new LogEntry(logLevel, string.Format(provider, format, args))
				{
					Exception = null
				});
			}
		}

		public static void Log(this ILog log, LogLevelEnum logLevel, IFormatProvider provider, string format, Exception exception, LogContextEnum withContext = LogContextEnum.None, bool collectEntry = false, params object[] args)
		{
			switch (logLevel)
			{
			case LogLevelEnum.Debug:
				if (log.IsDebugEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.DebugFormat(provider, format, args);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			case LogLevelEnum.Info:
				if (log.IsInfoEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.InfoFormat(provider, format, args);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			case LogLevelEnum.Warn:
				if (log.IsWarnEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.WarnFormat(provider, format, args);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			case LogLevelEnum.Error:
				if (log.IsErrorEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.ErrorFormat(provider, format, args);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			case LogLevelEnum.Fatal:
				if (log.IsFatalEnabled)
				{
					DoLogWithContext(withContext, delegate
					{
						log.FatalFormat(provider, format, args);
					});
				}
				else
				{
					collectEntry = false;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (collectEntry)
			{
				LogHelper.AddLogEntry(new LogEntry(logLevel, string.Format(provider, format, args))
				{
					Exception = exception
				});
			}
		}

		public static void DebugWithCheck(this ILog log, string message)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug(message);
			}
		}

		public static void DebugWithCheck(this ILog log, string message, Exception exception)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug(message, exception);
			}
		}

		public static void DebugFormatWithCheck(this ILog log, string format, params object[] args)
		{
			if (log.IsDebugEnabled)
			{
				log.DebugFormat(format, args);
			}
		}

		public static void DebugFormatWithCheck(this ILog log, IFormatProvider provider, string format, params object[] args)
		{
			if (log.IsDebugEnabled)
			{
				log.DebugFormat(provider, format, args);
			}
		}

		public static void InfoWithCheck(this ILog log, string message)
		{
			if (log.IsInfoEnabled)
			{
				log.Info(message);
			}
		}

		public static void InfoWithCheck(this ILog log, string message, Exception exception)
		{
			if (log.IsInfoEnabled)
			{
				log.Info(message, exception);
			}
		}

		public static void InfoFormatWithCheck(this ILog log, string format, params object[] args)
		{
			if (log.IsInfoEnabled)
			{
				log.InfoFormat(format, args);
			}
		}

		public static void InfoFormatWithCheck(this ILog log, IFormatProvider provider, string format, params object[] args)
		{
			if (log.IsInfoEnabled)
			{
				log.InfoFormat(provider, format, args);
			}
		}

		public static void WarnWithCheck(this ILog log, string message)
		{
			if (log.IsWarnEnabled)
			{
				log.Warn(message);
			}
		}

		public static void WarnWithCheck(this ILog log, string message, Exception exception)
		{
			if (log.IsWarnEnabled)
			{
				log.Warn(message, exception);
			}
		}

		public static void WarnFormatWithCheck(this ILog log, string format, params object[] args)
		{
			if (log.IsWarnEnabled)
			{
				log.WarnFormat(format, args);
			}
		}

		public static void WarnFormatWithCheck(this ILog log, IFormatProvider provider, string format, params object[] args)
		{
			if (log.IsWarnEnabled)
			{
				log.WarnFormat(provider, format, args);
			}
		}

		public static void ErrorWithCheck(this ILog log, string message)
		{
			if (log.IsErrorEnabled)
			{
				log.Error(message);
			}
		}

		public static void ErrorWithCheck(this ILog log, string message, Exception exception)
		{
			if (log.IsErrorEnabled)
			{
				log.Error(message, exception);
			}
		}

		public static void ErrorFormatWithCheck(this ILog log, string format, params object[] args)
		{
			if (log.IsErrorEnabled)
			{
				log.ErrorFormat(format, args);
			}
		}

		public static void ErrorFormatWithCheck(this ILog log, IFormatProvider provider, string format, params object[] args)
		{
			if (log.IsErrorEnabled)
			{
				log.ErrorFormat(provider, format, args);
			}
		}

		public static void FatalWithCheck(this ILog log, string message)
		{
			if (log.IsFatalEnabled)
			{
				log.Fatal(message);
			}
		}

		public static void FatalWithCheck(this ILog log, string message, Exception exception)
		{
			if (log.IsFatalEnabled)
			{
				log.Fatal(message, exception);
			}
		}

		public static void FatalFormatWithCheck(this ILog log, string format, params object[] args)
		{
			if (log.IsFatalEnabled)
			{
				log.FatalFormat(format, args);
			}
		}

		public static void FatalFormatWithCheck(this ILog log, IFormatProvider provider, string format, params object[] args)
		{
			if (log.IsFatalEnabled)
			{
				log.FatalFormat(provider, format, args);
			}
		}

		private static void DoLogWithContext(LogContextEnum withContext, Action action)
		{
			try
			{
				switch (withContext)
				{
				case LogContextEnum.Method:
					NDC.Push(LogHelper.GetCallerMethodContext());
					break;
				case LogContextEnum.ClassMethod:
					NDC.Push(LogHelper.GetCallerClassContext());
					break;
				case LogContextEnum.NamespaceClassMethod:
					NDC.Push(LogHelper.GetCallerNamespaceContext());
					break;
				default:
					throw new ArgumentOutOfRangeException("withContext");
				case LogContextEnum.None:
					break;
				}
				action();
			}
			finally
			{
				switch (withContext)
				{
				case LogContextEnum.Method:
				case LogContextEnum.ClassMethod:
				case LogContextEnum.NamespaceClassMethod:
					NDC.Pop();
					break;
				default:
					throw new ArgumentOutOfRangeException("withContext");
				case LogContextEnum.None:
					break;
				}
			}
		}
	}
}
