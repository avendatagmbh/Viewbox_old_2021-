using System;
using System.IO;
using Utils;

namespace AvdCommon.Logging
{
    //[Obsolete("This class is obsolete, LogHelper instead", true)]
    public static class LogManager
    {
        private const int bufferSize = 100;

        #region Constructor

        static LogManager()
        {
            //LogEntries = new ObservableCollectionAsync<LogEntry>();
            //Status("Programm gestartet");
        }

        #endregion Constructor

        #region Properties

        private static readonly object _lock = new object();

        private static StreamWriter _fileWriter;

        [Obsolete("This property is obsolete, use LogHelper.LogEntries instead", true)]
        public static ObservableCollectionAsync<LogEntry> LogEntries { get; set; }

        #endregion Properties

        private static void AddEntry(LogEntry entry)
        {
            lock (_lock)
            {
                //Write to file
                try
                {
                    if (_fileWriter == null)
                    {
                        _fileWriter =
                            new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                             "\\AvenDATA\\" + Global.ApplicationName + "\\" + Global.ApplicationName +
                                             "_" +
                                             DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".log");
                        _fileWriter.AutoFlush = true;
                    }
                    _fileWriter.Write(entry.TimeStamp.ToString("yyyy-MM-dd_HH-mm-ss") + ": " + entry.Message +
                                      (entry.Exception != null ? ", Stacktrace: " + entry.Exception.StackTrace : "") +
                                      Environment.NewLine);
                    //if (entry.Exception != null && entry.Exception is OutOfMemoryException) {
                    //    PerformanceCounter ramPerformanceCounter = new PerformanceCounter("Memory", "Available MBytes"); 
                    //    _fileWriter.Write("Freier Speicher: " + );
                    //}
                }
                catch (Exception ex)
                {
                }
                //LogEntries.Insert(0, entry);
                //if(LogEntries.Count > bufferSize)
                //    LogEntries.RemoveAt(LogEntries.Count - 1);
            }
        }

        /// <summary>
        ///   Write a message to a file
        /// </summary>
        /// <param name="Message"> The writed message </param>
        public static void AddEntryToAFile(string Message)
        {
            lock (_lock)
            {
                //Write to file
                try
                {
                    if (_fileWriter == null)
                    {
                        _fileWriter =
                            new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                             "\\AvenDATA\\" + Global.ApplicationName + "\\" + Global.ApplicationName +
                                             "_" +
                                             DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".log");
                        _fileWriter.AutoFlush = true;
                    }
                    _fileWriter.Write(DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ": " + Message +
                                      Environment.NewLine);
                }
                catch (Exception)
                {
                }
            }
        }

        [Obsolete("This method is obsolete, please use LogHelper.Warn or LogHelper.Log(LogLevelEnum.Warn ...) instead",
            true)]
        public static void Warning(string message)
        {
            AddEntry(new LogEntry(LogType.Warning, message));
        }

        [Obsolete("This method is obsolete, please use LogHelper.Error or LogHelper.Log(LogLevelEnum.Error ...) instead"
            , true)]
        public static void Error(string message)
        {
            AddEntry(new LogEntry(LogType.Error, message));
        }

        [Obsolete("This method is obsolete, please use LogHelper.Info or LogHelper.Log(LogLevelEnum.Info ...) instead",
            true)]
        public static void Status(string message)
        {
            AddEntry(new LogEntry(LogType.Status, message));
        }

        [Obsolete("This method is obsolete, please use LogHelper.Error or LogHelper.Log(LogLevelEnum.Error ...) instead"
            , true)]
        public static void Error(string message, Exception exception)
        {
            AddEntry(new LogEntry(LogType.Status, message) {Exception = exception});
        }
    }
}