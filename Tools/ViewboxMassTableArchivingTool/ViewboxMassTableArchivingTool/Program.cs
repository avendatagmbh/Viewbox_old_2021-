using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SystemDb;
using AV.Log;
using CommandLineParser.Exceptions;
using ViewboxDb;
using log4net;


namespace ViewboxMassTableArchivingTool {
    internal class Program {
        private static MiniViewboxWrapper _vb;
        private static bool _viewboxDatabaseInitialized = false;
        private static IEnumerable<ITableObject> objs;


        private static UTF8Encoding utf8 = new UTF8Encoding();
        private static Options _options;

        private static void Main(string[] args) {
            LogHelper.GetLogger().InfoFormat("Applicaton start");

            Console.WriteLine();
            Console.WriteLine("Applicaton start {0}", Assembly.GetExecutingAssembly().GetName().Version);

            Console.CancelKeyPress += new ConsoleCancelEventHandler(CtrlCHandler);


            CommandLineParser.CommandLineParser parser = new CommandLineParser.CommandLineParser();
            _options = new Options();
            parser.ExtractArgumentAttributes(_options);
            try {
                parser.ParseCommandLine(args);

                if (string.IsNullOrWhiteSpace(_options.System)) {
                    LogHelper.GetLogger().InfoFormat("System to be archived / restored needs to be set");
                    Console.WriteLine("System to be archived / restored  needs to be set");
                    ByeBye("");
                }

                if (_options.List || _options.Archive || _options.Restore || _options.Export || _options.FullExport) {
                    ViewboxDatabaseInit();
                } else {
                    ByeBye("");
                }
            } catch (CommandLineException e) {
                parser.ShowUsage();
                ByeBye("");
            }
        }

        protected static void CtrlCHandler(object sender, ConsoleCancelEventArgs args) { ByeBye("Forced application exit"); }

        private static void ByeBye(string message) {
            if (string.IsNullOrEmpty(message))
                message = "Application exit";

            if (_vb != null) {
                _vb.Dispose();
                _vb = null;
            }

            LogHelper.GetLogger().InfoFormat(message);
            Console.WriteLine();
            Console.WriteLine(message);
            Environment.Exit(0);
        }


        private static void ViewboxDatabaseInit() {
            LogHelper.GetLogger().InfoFormat("Start viewbox database initialization");
            Console.WriteLine("Start viewbox database initialization");

            _vb = new MiniViewboxWrapper();
            _vb.ViewboxDbInitialized += new EventHandler(ViewboxDatabaseInitialized);
            _vb.ConnectionString = "server=" + _options.Server;

            if (!String.IsNullOrEmpty(_options.Port) && !String.IsNullOrWhiteSpace(_options.Port))
            {
                _vb.ConnectionString = _vb.ConnectionString + ";port=" + _options.Port;
            }

            _vb.ConnectionString = _vb.ConnectionString +
                                   ";User Id=" + _options.User +
                                   ";password=" + _options.Password +
                                   ";database=" + _options.Database +
                                   ";Connect Timeout=1000;Default Command Timeout=0;Allow Zero Datetime=True";

            _vb.TempDBName = _options.Database + "_temp";
            _vb.IndexDBName = _options.Database + "_index";

            LogHelper.GetLogger().InfoFormat("ConnectionString: {0}", _vb.ConnectionString);
            Console.WriteLine();
            Console.WriteLine("Server: {0}", _options.Server);
            Console.WriteLine("Port: {0}", _options.Port);
            Console.WriteLine("Database: {0}", _options.Database);
            Console.WriteLine("User: {0}", _options.User);
            Console.WriteLine("Password: {0}", _options.Password);
            Console.WriteLine();

            _vb.Init();
        }

        private static void ViewboxDatabaseInitialized(object sender, EventArgs args) {
            LogHelper.GetLogger().InfoFormat("Viewbox database initialized");

            Console.WriteLine();
            Console.WriteLine("Viewbox database initialized");

            string curVer = "";
            string insVer = "";
            if (_vb.Database.SystemDb.DatabaseOutOfDateInformation != null)
            {
                curVer = _vb.Database.SystemDb.DatabaseOutOfDateInformation.CurrentDbVersion;
                insVer = _vb.Database.SystemDb.DatabaseOutOfDateInformation.InstalledDbVersion;
            }



            if (curVer == insVer)
            {
                objs = _vb.GetTablesFromQuery();
                if (objs != null && objs.Any()) {
                    _viewboxDatabaseInitialized = true;

                    LogHelper.GetLogger().InfoFormat("Tables found: {0}", objs.Count());
                    Console.WriteLine("Tables found: {0}", objs.Count());
                    
                    // Officialy AvenDATA SAP check
                    bool isSap = objs.Any() && objs.Any(obj => obj.TableName.ToLower() == "dd03l");
                    LogHelper.GetLogger().InfoFormat("Is SAP: {0}", isSap ? "True" : "False");
                    Console.WriteLine("Is SAP: {0}", isSap ? "True" : "False");

                    if (!_options.SkipSAPCheck) {
                        if (!isSap) {
                            ByeBye("");
                        }
                    }

                    Console.WriteLine();

                    if (_options.Export || _options.FullExport) {
                        if (string.IsNullOrEmpty(_options.FilePath)) {
                            _options.FilePath = "tables.csv";
                        }
                        Export(_options.FilePath, _options.FullExport);
                    }
                    else if (_options.Archive)
                    {
                        if (string.IsNullOrEmpty(_options.FilePath))
                            _options.FilePath = "tables.csv";
                        
                        ArchiveRestore(_options.FilePath, ArchiveType.Archive, _options, objs, _vb);                        
                    }
                    else if (_options.Restore)
                    {
                        if (string.IsNullOrEmpty(_options.FilePath)) {
                            _options.FilePath = "tables.csv";
                        }

                        ArchiveRestore(_options.FilePath, ArchiveType.Restore, _options, objs, _vb);
                    } else if (_options.List) {
                        ListTables();
                    }
                } else {
                    LogHelper.GetLogger().WarnFormat("There is no user tables in viewbox database");
                    Console.WriteLine("There is no user tables in viewbox database");

                    ByeBye("");
                }
            }else {
                LogHelper.GetLogger().WarnFormat("Please upgrade viewbox database from {0} to {1}", insVer, curVer);
                Console.WriteLine("Please upgrade viewbox database from {0} to {1}", insVer, curVer);
                ByeBye("");
            }
        }

        private static void Export(string filepath, bool fullExport) {
            if (File.Exists(filepath)) {
                File.Delete(filepath);
            }

            try {
                System.IO.StreamWriter fs = new System.IO.StreamWriter(filepath, false, utf8);

                if (fullExport) {
                    fs.WriteLine("Database,TableName,RowCount,Type,IsArchived");
                }

                foreach (ITableObject obj in objs) {
                    if (fullExport) {
                        fs.WriteLine("{0},{1},{2},{3},{4}", obj.Database, obj.TableName, obj.RowCount,
                                     obj.Type.ToString(), obj.IsArchived ? "Archived" : "");
                    } else {
                        fs.WriteLine("{0}", obj.TableName);
                    }
                }
                fs.Close();
                LogHelper.GetLogger().InfoFormat("Export done: {0}", filepath);
                Console.WriteLine("Export done: {0}", filepath);

            } catch (Exception ex) {
                LogHelper.GetLogger().Error(ex);
                Console.WriteLine("Error: {0}", ex.Message);
                throw;
            }

            ByeBye("");
        }

        private static void ListTables() {
            foreach (ITableObject obj in objs) {
                Console.WriteLine("{0,-30} {1,9} {2,13}", obj.TableName, obj.RowCount, obj.Type.ToString());
            }
            ByeBye("");
        }

        private static void ArchiveRestore(
            string p_FilePath, ArchiveType p_Type, Options p_Options, IEnumerable<ITableObject> p_Objs,
            MiniViewboxWrapper p_MVBH)
        {
            using (StreamReader m_Reader = GetStream(p_Options.FilePath)) {
                if (m_Reader != null) {
                    ArchiveRestoreLogic.ArchiveRestore(m_Reader, p_Type, p_Options, p_Objs, p_MVBH);
                    m_Reader.Close();
                    m_Reader.Dispose();
                }
            }
        }

        private static StreamReader GetStream(string p_FileNameWithPath)
        {
            if (File.Exists(p_FileNameWithPath))
                return new StreamReader(p_FileNameWithPath, true);

            return null;
        }
    }
}