using System;

namespace AvdCommon.Logging
{
    public class LogEntry
    {
        public LogEntry(LogType logType, string message)
        {
            LogType = logType;
            Message = message;
            TimeStamp = DateTime.Now;
        }

        public LogType LogType { get; set; }
        public string Message { get; set; }
        public DateTime TimeStamp { get; private set; }
        public Exception Exception { get; set; }

        public string DisplayString
        {
            get { return TimeStampString + ": " + Message; }
        }

        public string TimeStampString
        {
            get { return TimeStamp.ToString("HH:mm:ss"); }
        }
    }
}