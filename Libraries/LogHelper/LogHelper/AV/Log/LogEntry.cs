using System;

namespace AV.Log
{
	public class LogEntry
	{
		private LogLevelEnum LogType { get; set; }

		private string Message { get; set; }

		private DateTime TimeStamp { get; set; }

		public Exception Exception { get; set; }

		public string DisplayString => TimeStampString + ": " + Message;

		private string TimeStampString => TimeStamp.ToString("HH:mm:ss");

		public LogEntry(LogLevelEnum logType, string message)
		{
			LogType = logType;
			Message = message;
			TimeStamp = DateTime.Now;
		}
	}
}
