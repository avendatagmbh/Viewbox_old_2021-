using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AV.Log;
using CommandLineParser.Exceptions;
using log4net;

namespace ViewRelevantChecker
{
    class Program
    {
        private static Options _options;
        private static ILog log = LogHelper.GetLogger();

        /// <summary>
        /// Defines the program entry point. 
        /// </summary>
        /// <param name="args">An array of <see cref="T:System.String"/> containing command line parameters.</param>
        private static void Main(string[] args)
        {
            CommandLineParser.CommandLineParser parser = new CommandLineParser.CommandLineParser();
            _options = new Options();
            parser.ExtractArgumentAttributes(_options);
            try
            {
                parser.ParseCommandLine(args);

                try
                {

                    string logPath = Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, "log");
                    LogHelper.ConfigureLogger(LogHelper.GetLogger(), logPath);
                }
                catch (Exception ex)
                {
                }

                DoWork();
            }
            catch (CommandLineException)
            {
                parser.ShowUsage();
            }
        }

        private static void DoWork()
        {
            try {
                if (!File.Exists(_options.FilePath)) {
                    LogInfoMessage(string.Format("File does not exists [{0}]", _options.FilePath));
                    return;
                }

                List<string> tables = File.ReadAllLines(_options.FilePath).Select(t => t.ToLower()).ToList();
                LogInfoMessage(string.Format("Tables are read from file, table count: {0}", tables.Count));

                List<string> tablesFound = new List<string>();

                if (Directory.Exists(_options.Directory)) {
                    //new DirectoryInfo(_options.Directory)
                    foreach (FileInfo file in new DirectoryInfo(_options.Directory).GetFiles()) {
                        if (!_options.CsvInput) {
                            string nameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name).ToLower();
                            if (tables.Contains(nameWithoutExtension)) {
                                // multiple files for a table in sql
                                if (!tablesFound.Contains(nameWithoutExtension)) tablesFound.Add(nameWithoutExtension);
                            }
                        } else {
                            string name = Path.GetFileName(file.Name).ToLower();
                            name = name.Substring(0, )
                        }
                    }
                } else
                {
                    LogInfoMessage(string.Format("Directory does not exists [{0}]", _options.Directory));
                    return;
                }

                LogInfoMessage(string.Format("Tables found [{0}]", tablesFound.Count));
                //if (tablesFound.Count == 0) return;

                LogInfoMessage(string.Format("Saving result to file [{0}] started", _options.OutputFilePath));
                using (StreamWriter tablesFoundFile = new StreamWriter(_options.OutputFilePath, true, Encoding.UTF8))
                {
                    tablesFoundFile.WriteLine("{1} tables found in directory [{0}]", _options.FilePath, tablesFound.Count);

                    foreach (string table in tablesFound) {
                        tablesFoundFile.WriteLine(table);
                    }
                }

                LogInfoMessage(string.Format("Saving result to file [{0}] finished", _options.OutputFilePath));
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private static void LogInfoMessage(string logMsg)
        {
            Console.WriteLine(logMsg);
            log.Info(logMsg);
        }
    }
}
