using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Config;
using System.Configuration;
using Moq;

namespace EasyDocExtraction.Helper
{
    /// <summary>
    /// Thread safe logging class helper.
    /// </summary>
    public class Logger
    {
        Logger() { }
        private class LogCreator 
        {

            internal static log4net.ILog _uniqueLogInstance;
            static LogCreator() { }
        }
        public class LoggingConfigurer
        {
            public static readonly string DefaultLayoutPattern = "%-5p%d{yyyy-MM-dd hh:mm:ss} – %m%n";
            public static string DefaultFileName = "default.log";
            public static readonly string FileAppendarName = "XXXRFA";

            // Configure the logger programmatically. 
            public static void ConfigureLogging(ILog log, string logFileName)
            {
                bool isConfigured = log.Logger.Repository.Configured;
                //if (!isConfigured)
                {
                    // Setup RollingFileAppender
                    RollingFileAppender rollingFileAppender = new RollingFileAppender();
                    rollingFileAppender.Layout = new PatternLayout(DefaultLayoutPattern);
                    rollingFileAppender.MaximumFileSize = "5MB";
                    rollingFileAppender.MaxSizeRollBackups = 50;
                    rollingFileAppender.RollingStyle = RollingFileAppender.RollingMode.Size;
                    rollingFileAppender.AppendToFile = true;
                    rollingFileAppender.File = ConfigurationManager.AppSettings["logFolder"] + logFileName + ".log";
                    rollingFileAppender.ActivateOptions(); // IMPORTANT, creates the file
                    BasicConfigurator.Configure(rollingFileAppender);
                    //}
                }
            }
            public static void RenameLogFile(string newLogFileName)
            {
                var rfa = LogManager.GetRepository().GetAppenders().
                     OfType<RollingFileAppender>().
                     FirstOrDefault();
                if (rfa != null)
                {
                    string newFileName = ConfigurationManager.AppSettings["logFolder"] + newLogFileName + ".log";
                    if (rfa.File != newFileName)
                    {
                        rfa.File = newFileName;
                        rfa.AppendToFile = true;
                    }
                    rfa.ActivateOptions();
                }
            }

        }
        private static ILog GetMock()
        {
            return new Mock<log4net.ILog>().Object;
        }
        public static log4net.ILog Log
        {
            get {
                // test case only
                if (LogCreator._uniqueLogInstance == null) LogCreator._uniqueLogInstance = GetMock();
                return LogCreator._uniqueLogInstance;
            }
        }
        public static void Write(string message)
        {
            Console.WriteLine("> " + message);
            Log.Info(message);
        }
        public static void Write(string format, params object[] args)
        {
            Console.WriteLine("> " + format, args);
            Log.Info(string.Format(format, args));
        }
        public static void WriteError(string format, params object[] args)
        {
            Console.WriteLine("ERROR:" + format, args);
            Log.Error(string.Format(format, args));
        }
        public static log4net.ILog Instance
        {
            get { return LogCreator._uniqueLogInstance; }
            set { LogCreator._uniqueLogInstance = value; }
        }
        public static void WriteError(string message, Exception ex)
        {
            Console.WriteLine("ERROR:" + message + " Exception:" + ex.ToString());
            Log.Error(message, ex);
            
            #if DEBUG
                throw ex;
            #endif
        }
    }
}
