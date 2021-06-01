using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using System.Web.Configuration;

namespace Viewbox
{

    /// <summary>
    /// Class for logging exception info to text file 
    /// </summary>
    internal class FileLogger : ILogger
    {

        #region contructors

        private FileLogger() {
            _logFile = WebConfigurationManager.AppSettings["LogPath"];

            if (_logFile == null){
                throw new ApplicationException("There is no path specified in web.config for log file"); // this should never happen anyway
            }

            if (!File.Exists(_logFile)){
                using (System.IO.FileStream fs = System.IO.File.Create(_logFile))
                {
                    ;
                }
            }
        }
        /// <summary>
        /// ensure singleton pattern
        /// </summary>
        static FileLogger() {
            _fileLogger = new FileLogger();
        }


        #endregion

        #region public methods

        /// <summary>
        /// global access point to a filelogger object
        /// </summary>
        /// <returns></returns>
        public static FileLogger GetInstance() {
            return _fileLogger;
        }

        /// <summary>
        /// write exception details to the top of the specified textfile. Performance can be degraded if the file grows large.
        /// </summary>
        /// <param name="e">exception to log</param>
        public void LogException(Exception e) {
            string logmessage = FormatExceptionToSave(e);
            lock (_fileWriterLock)
            {
                string oldLog = File.ReadAllText(_logFile);
                System.IO.File.WriteAllText(_logFile,logmessage + Environment.NewLine + oldLog); // can hurt performance
            }
        }

        /// <summary>
        /// append exception info to end of the file. If any performance related issue appears, one can use this method
        /// </summary>
        /// <param name="e">exception to log</param>
        private void LogExceptionAppend(Exception e) {
            string logmessage = FormatExceptionToSave(e);
            lock (_fileWriterLock)
            {
                using (StreamWriter logwriter = new StreamWriter(_logFile, true))
                {
                    logwriter.Write(logmessage);
                }
            }
        }

        #endregion

        #region private methods
        /// <summary>
        /// Helper method to create a formatted string for exception
        /// </summary>
        /// <param name="e"> The exception to log</param>
        /// <returns></returns>
        private string FormatExceptionToSave(Exception e) {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("TIME: ");
            sb.AppendLine(DateTime.Now.ToString());
            sb.AppendLine("");
            sb.AppendLine("MESSAGE: ");
            sb.AppendLine(e.Message.ToString());
            sb.AppendLine("");
            if (e.InnerException != null)
            {
                sb.AppendLine("INNER EXCEPTION: ");
                sb.AppendLine(e.InnerException.ToString());
                sb.AppendLine("");
            }
            sb.AppendLine("STACK TRACE: ");
            sb.AppendLine(e.StackTrace.ToString());
            sb.AppendLine("");
            string terminatorLine = "__________________________________________________________________";
            sb.AppendLine(terminatorLine);
            return sb.ToString();
        }

        #endregion

        #region private fields

        private object _fileWriterLock = new Object();

        private readonly string _logFile = string.Empty;

        private static FileLogger _fileLogger;

        #endregion

    }
}