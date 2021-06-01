using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using AV.Log;
using Business.CsvImporter.Structures;
using Business.Interfaces;
using Config.DbStructure;
using Config.Interfaces.Config;
using Config.Interfaces.DbStructure;
using DbAccess.Structures;
using DbImport.CsvImport;
using Ionic.Zip;
using log4net;
using File = System.IO.File;
using Logging;
using Base.Exceptions;
using AV.Log;

namespace Business.Structures.MetadataAgents
{
    internal class MetadataAgentCsv : IMetadataAgent {
        
        private ILog _log = LogHelper.GetLogger();

        // results for 4939K filesize
        //128K  115.68 sec
        //64K    49.78 sec
        //32K     7.70 sec
        //16K     3.68 sec
        //8K      2.21 sec
        //4K      1.18 sec
        //2K      0.70 sec
        //1K      0.43 sec
        //512     0.31 sec
        //128     0.21 sec
        //64      0.20 sec => local read
        //32      0.20 sec
        //1       1.27 sec
        private const int _bufferSizeNetwork = 1024 * 60; //60K bytes
        private const int _bufferSizeLocal = 64; //64 bytes

        #region Constructor
        internal MetadataAgentCsv(ICsvInputConfig inputConfig)
        {
            InputConfig = inputConfig;
            LoggingTables = new Dictionary<string, List<XMLLoggingTable>>();
        }
        #endregion Constructor

        #region Properties
        private ICsvInputConfig InputConfig { get; set; }

        private Dictionary<string, List<XMLLoggingTable>> LoggingTables { get; set; }
        #endregion Properties

        #region Methods

        public void AddTables(IInputAgent importAgent, List<DbTableInfo> tables, IImportDbStructureProgress importProgress)
        {
            _log.ContextLog( LogLevelEnum.Debug,"Folder: {0}", InputConfig.Folder);
            
            HashSet<string> baseFilenames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            //Stopwatch sw = Stopwatch.StartNew();
            IEnumerable<FileInfo> files = new DirectoryInfo(InputConfig.Folder).EnumerateFiles("*.*",
                                                                     InputConfig.ImportSubDirectories
                                                                         ? SearchOption.AllDirectories
                                                                         : SearchOption.TopDirectoryOnly).Where(
                                                                             s =>
                                                                             s.FullName.ToLower().EndsWith(".csv") ||
                                                                             s.FullName.ToLower().EndsWith(".txt") ||
                                                                             s.FullName.ToLower().EndsWith(".csv.zip")).OrderBy((file) => file.FullName.ToLower());
            //sw.Stop();

            foreach (FileInfo filename in files)
            {
                _log.ContextLog( LogLevelEnum.Debug,"Listed filename: {0}", filename.Name);
                try
                {

                    //Do not import export_log files
                    if (InputConfig.IsSapCsv && Regex.IsMatch(filename.Name, @"^export_log_\d{8}_\d{6}.csv$")) {
                        _log.ContextLog( LogLevelEnum.Debug,"Skiped (reason #1) filename: {0}", filename.Name);
                        continue;
                    }

                    if (Regex.IsMatch(filename.Name, @"^transdata_export_\d{4}-\d{2}-\d{2} \d{2}-\d{2}-\d{2}.xml$")){
                        _log.ContextLog(LogLevelEnum.Debug,"Skiped (reason #2) filename: {0}", filename.Name);
                        continue;
                    }

                    if (Regex.IsMatch(filename.Name, @"^TransDATA_Documentation.xml$")){
                        _log.ContextLog( LogLevelEnum.Debug,"Skiped (reason #3) filename: {0}", filename.Name);
                        continue;
                    }
                    
                    _log.ContextLog( LogLevelEnum.Info,"Processed filename: {0}", filename.Name);

                    string baseFilename = filename.FullName;

                    if (InputConfig.TableCanHaveMultipleParts)
                    {
                        baseFilename = GetBaseFilename(filename.FullName);
                        if (baseFilenames.Contains(baseFilename))
                            continue;
                    }
                    
                    DbTableInfo table = null;
                    bool newtable = true;

                    // BAAN base filename
                    if (InputConfig.IsBaanCsv)
                    {
                        baseFilename = BaanGetBaseFilename(filename.FullName, InputConfig.BaanCompanyIdLength).ToLower();

                        if (!baseFilenames.Contains(baseFilename))
                        {
                            baseFilenames.Add(baseFilename);
                            table = new DbTableInfo("", "", filename.FullName, "", "", "");
                        }
                        else
                        {
                            table = tables.Find(t => BaanGetBaseFilename(t.Name, InputConfig.BaanCompanyIdLength) == BaanGetBaseFilename(filename.FullName, InputConfig.BaanCompanyIdLength));
                            newtable = false;
                        }
                    }
                    else
                    {
                        baseFilenames.Add(baseFilename);
                        table = new DbTableInfo("", "", filename.FullName, "", "", "");
                    }
                    
                    int ordinal = 0;
                    foreach (var colName in CsvImporterBase.GetFieldNamesFromCsv(filename.FullName, InputConfig))
                    {

                        bool addCol = true;
                        if (InputConfig.IsBaanCsv)
                            addCol = (table.Columns.Find(i => i.Name == colName) == null);

                        if (addCol)
                            table.Columns.Add(new DbColumnInfo()
                            {
                                Name = colName,
                                AllowDBNull = true,
                                Type = DbColumnTypes.DbText,
                                OriginalType = "Text",
                                OrdinalPosition = ordinal
                            });
                        ordinal++;
                    }
                    
                    //TODO: move to good place
                    // *************
                    // check maximum row size for MySql (65535 bytes)
                    // info at: http://dev.mysql.com/doc/refman/5.5/en/column-count-limit.html
                    int maxRowSize = 1 + table.Columns.Sum(col => (10 + col.Name.Length));
                    if (maxRowSize > 65535) {
                        _log.ContextLog( LogLevelEnum.Error, "Error: maximum row size for MySql is too much: {0}. Maximum is 65535", maxRowSize);
                    }
                    // check maximum row count for MsSql (4000 columns)
                    //info at: http://msdn.microsoft.com/en-us/library/ms143432.aspx
                    if (table.Columns.Count > 4000) {
                        _log.ContextLog(LogLevelEnum.Error, "Error: maximum row count for MsSql is too much: {0}. Maximum is 4000", table.Columns.Count);
                    }
                    // *************

                    _log.ContextLog( LogLevelEnum.Debug,"{0}", newtable ? "It was a new table" : "It was a part of table");

                    if (newtable)
                    {
                        importProgress.TablesTotal++;
                        tables.Add(table);
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = string.Format("{0} \"{1}\":{2}", Base.Localisation.ResourcesCommon.MetaDataAgentCsvErrorInTheFile, filename.Name,
                                                        Environment.NewLine + ex.Message);

                    _log.ContextLog(LogLevelEnum.Error, "Error: {0}", errorMessage);

                    //TODO: Do some proper handling: log all
                    importProgress.AddErrorMessage(errorMessage);
                }
                //tableInfos.Add(new CsvTableInfo(basename, filename.FullName));
            }

            ReadLogFiles();

            _log.ContextLog( LogLevelEnum.Debug,"Done");
        }

        private void ReadLogFiles()
        {
            if (string.IsNullOrEmpty(InputConfig.FolderLog)) return;

            _log.ContextLog( LogLevelEnum.Debug,"");
            Stopwatch sw = new Stopwatch();
            
            sw.Start();

            foreach (var file in Directory.GetFiles(InputConfig.FolderLog))
            {
                var fi = new FileInfo(file);


                if (Regex.IsMatch(fi.Name, @"^export_log_\d{8}_\d{6}.csv$"))
                    ReadSAPLog(fi);

                if (Regex.IsMatch(fi.Name, @"^transdata_export_\d{4}-\d{2}-\d{2} \d{2}-\d{2}-\d{2}.xml$"))
                    ReadTDLog(fi);

                if (Regex.IsMatch(fi.Name, @"^TransDATA_Documentation.xml$"))
                    ReadTDLog(fi);

                if (Regex.IsMatch(fi.Name, @"^" + Base.Localisation.ResourcesCommon.LogFile + @"\d{2}-\d{2}-\d{4} \d{2}-\d{2}-\d{2}.xml$"))
                    ReadTDLog(fi);
            }

            sw.Stop();

            _log.ContextLog( LogLevelEnum.Info,"Time: {0}", sw.Elapsed.ToString());
        }

        private void ReadTDLog(FileInfo fi)
        {
            _log.ContextLog(LogLevelEnum.Debug, "");
            var sb = new StringBuilder();
            using (var reader = new StreamReader(fi.FullName))
            {
                while (!reader.EndOfStream)
                {
                    sb.Append(reader.ReadLine());
                }
                reader.Close();
            }

            var doc = new XmlDocument();
            doc.LoadXml(sb.ToString());
            foreach (XmlNode child in doc.DocumentElement)
            {
                switch (child.Name)
                {
                    case "Table":
                        var xmlLoggingTable = new XMLLoggingTable();
                        xmlLoggingTable.Load(child);
                        if (!LoggingTables.ContainsKey(xmlLoggingTable.Name.ToLower()))
                        {
                            LoggingTables.Add(xmlLoggingTable.Name.ToLower(), new List<XMLLoggingTable>());
                        }
                        LoggingTables[xmlLoggingTable.Name.ToLower()].Add(xmlLoggingTable);
                        break;
                }
            }
        }

        private void ReadSAPLog(FileInfo fi)
        {
            _log.ContextLog(LogLevelEnum.Debug, "");
            string line;
            bool firstLine = true;

            using (var reader = new StreamReader(fi.FullName))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (firstLine)
                    {
                        firstLine = false;
                        continue;
                    }
                    if (string.IsNullOrEmpty(line))
                        continue;

                    ReadSAPLogLine(line);
                }
            }
        }

        private void ReadSAPLogLine(string line)
        {
            var xmlLoggingTable = new XMLLoggingTable();

            string[] fields = line.Split(';');
            if (fields.Length != 11) throw new InvalidOperationException("Folgende Zeile im Exportlog ist ungültig, da sie nicht 11 Einträge besitzt: " + Environment.NewLine + line);
            xmlLoggingTable.Name = fields[1];

            int countBefore, countAfter;
            if (!Int32.TryParse(fields[6], out countBefore)) throw new InvalidOperationException("Countbefore ist keine Zahl in der Zeile " + Environment.NewLine + line);
            xmlLoggingTable.Count = countBefore;
            xmlLoggingTable.LogCount = countBefore;

            if (!Int32.TryParse(fields[9], out countAfter))
                throw new InvalidOperationException("Countafter ist keine Zahl in der Zeile " + Environment.NewLine + line);
            xmlLoggingTable.LogCountDest = countAfter;

            // fields[4] DATE "20120123"
            // fields[5] TIME "145906"
            xmlLoggingTable.LogTimestamp = DateTime.ParseExact(fields[4] + " " + fields[5], "yyyyMMdd HHmmss",
                                                               CultureInfo.InvariantCulture);

            if (!LoggingTables.ContainsKey(xmlLoggingTable.Name.ToLower()))
            {
                LoggingTables.Add(xmlLoggingTable.Name.ToLower(), new List<XMLLoggingTable>());
            }
            LoggingTables[xmlLoggingTable.Name.ToLower()].Add(xmlLoggingTable);
        }

        internal static string GetBaseFilename(string fullName)
        {
            Regex regex = new Regex(@"(?<name>.*?)_\d+.csv$|(?<name>.*?).csv$", RegexOptions.IgnoreCase);
            var match = regex.Match(fullName);
            if (match.Success)
                return match.Groups["name"].Value + ".csv";
            return fullName;
        }

        internal static int ExtractPartNumber(string name)
        {
            var match = Regex.Match(name, @".*?_(?<number>\d+).csv$", RegexOptions.IgnoreCase);
            if (match.Success)
                return Convert.ToInt32(match.Groups["number"].Value);
            return -1;
        }

        internal static string ReplacePartNumber(string name, int replaceWith)
        {
            return Regex.Replace(name, @"(.*?)_\d+.csv$", string.Format("$1_{0}.csv", replaceWith), RegexOptions.IgnoreCase);
        }


        internal static string BaanGetBaseFilename(string fullName, int CompanyIdLength)
        {
            Regex regex = new Regex(@"(?<name>.*?)\d{" + CompanyIdLength + @"}.csv$|(?<name>.*?).csv$", RegexOptions.IgnoreCase);
            var match = regex.Match(fullName);
            if (match.Success)
                return match.Groups["name"].Value + ".csv";
            return fullName;
        }

        internal static string BaanExtractPartNumber(string name, int CompanyIdLength)
        {
            var match = Regex.Match(name, @".*(?<number>\d{" + CompanyIdLength + @"}).csv$", RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups["number"].Value;
            return "";
        }

        internal static string BaanReplacePartNumber(string name, int CompanyIdLength, string replaceWith)
        {
            return Regex.Replace(name, @"(.*?)\d{" + CompanyIdLength + @"}.csv$", "${1}" + string.Format("{0}.csv", replaceWith), RegexOptions.IgnoreCase);
        }

        internal static string BaanSearch(string name, int CompanyIdLength)
        {
            string xx = Regex.Replace(name, @"(.*?)\d{" + CompanyIdLength + @"}.csv$", "${1}*.csv", RegexOptions.IgnoreCase);
            return xx;
        }


        public void ImportTable(DbTableInfo table, IProfile profile, IImportDbStructureProgress importProgress)
        {
            _log.ContextLog(LogLevelEnum.Debug, "table.Catalog: {0} table.Schema: {1}", table.Catalog, table.Schema);
            Stopwatch sw= new Stopwatch();

            ITable newTable = profile.CreateTable();

            //t.Count = tableInfo.Item2;
            newTable.Catalog = table.Catalog;
            newTable.Schema = table.Schema;

            if (InputConfig.IsBaanCsv)
                newTable.Name = new FileInfo(BaanGetBaseFilename(table.Name, InputConfig.BaanCompanyIdLength)).Name;
            else if (InputConfig.TableCanHaveMultipleParts)
                newTable.Name = new FileInfo(GetBaseFilename(table.Name)).Name;
            else
                newTable.Name = new FileInfo(table.Name).Name;
            //Delete .csv from name
            newTable.Name = newTable.Name.Substring(0, newTable.Name.Length - 4);

            newTable.Type = table.Type;
            newTable.Comment = table.Remarks;
            newTable.DoExport = true;
            newTable.FileNames.Add(table.Name);

            if (InputConfig.IsBaanCsv)
            {
                sw.Reset();
                sw.Start();

                IEnumerable<FileInfo> files = new DirectoryInfo(InputConfig.Folder).EnumerateFiles("*.*",
                                                                                                   InputConfig.
                                                                                                       ImportSubDirectories
                                                                                                       ? SearchOption.
                                                                                                             AllDirectories
                                                                                                       : SearchOption.
                                                                                                             TopDirectoryOnly)
                    .Where(
                        s =>
                        s.Name.ToLower().StartsWith(newTable.Name.ToLower()) && (
                                                                             s.FullName.ToLower().EndsWith(".csv") ||
                                                                             s.FullName.ToLower().EndsWith(".txt") ||
                                                                             s.FullName.ToLower().EndsWith(".csv.zip"))
                    );

                foreach (FileInfo filename in files)
                    if (filename.FullName.ToLower() != table.Name.ToLower())
                        newTable.FileNames.Add(filename.FullName);


                sw.Stop();
                _log.ContextLog(LogLevelEnum.Debug, "InputConfig.IsBaanCsv: yes t.FileNames.Count: {0} Time: {1}", newTable.FileNames.Count, sw.Elapsed.ToString());
            }
            else if (InputConfig.TableCanHaveMultipleParts)
            {
                sw.Reset();
                int currentNumber = ExtractPartNumber(table.Name);
                if (currentNumber != -1)
                {
                    int i = 0;

                    while (File.Exists(ReplacePartNumber(table.Name, i)))
                    {
                        if (i != currentNumber)
                        {
                            newTable.FileNames.Add(ReplacePartNumber(table.Name, i));
                        }
                        ++i;
                    }
                }
                sw.Stop();

                _log.ContextLog(LogLevelEnum.Debug, "InputConfig.TableCanHaveMultipleParts: yes t.FileNames.Count: {0} Time: {1}", newTable.FileNames.Count, sw.Elapsed.ToString());
            }

            lock (importProgress) { importProgress.AddProcessedTable(newTable); }

            foreach (var column in table.Columns)
            {
                var c = newTable.CreateColumn();
                c.Name = column.Name;
                c.OrdinalPosition = column.OrdinalPosition;
                c.DoExport = true;
                c.TypeName = column.OriginalType;
                c.Type = ColumnTypes.Text;
                newTable.AddColumn(c);
            }


            #region Metadata from log
            _log.ContextLog(LogLevelEnum.Debug, "Metadata from log");
            if (string.IsNullOrEmpty(InputConfig.FolderLog))
            {
                newTable.Count = CountTable(newTable.FileNames);
            }
            else
            {
                try
                {
                    if (!AssignTableToLog(newTable, GetLastLog(newTable.Name.ToLower().Replace(".csv", string.Empty), new FileInfo(table.Name).Directory.Name)))
                    {
                        newTable.Count = CountTable(newTable.FileNames);
                    }
                }
                catch (DataCountNotMatchException)
                {
                    lock (profile) { profile.AddTable(newTable); }
                    lock (importProgress) { importProgress.RemoveProcessedTable(newTable); }
                    throw;
                }
            }
            #endregion

            lock (profile) { profile.AddTable(newTable); }

            lock (importProgress) { importProgress.RemoveProcessedTable(newTable); }
        }

        private XMLLoggingTable GetLastLog(string tableName, string schemaName)
        {
            _log.ContextLog(LogLevelEnum.Debug, "");
            if (!LoggingTables.ContainsKey(tableName.ToLower().Replace("___slash___", "/"))) return null;

            XMLLoggingTable result = null;
            foreach (var xmlLoggingTable in LoggingTables[tableName.Replace("___slash___", "/")])
            {
                if (string.IsNullOrEmpty(xmlLoggingTable.Schema) && string.IsNullOrEmpty(xmlLoggingTable.Catalog))
                {
                    if (result == null || xmlLoggingTable.LogTimestamp > result.LogTimestamp) result = xmlLoggingTable;
                }
                else
                {
                    if (xmlLoggingTable.Schema.Equals(schemaName, StringComparison.InvariantCultureIgnoreCase) || xmlLoggingTable.Catalog.Equals(schemaName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (result == null || xmlLoggingTable.LogTimestamp > result.LogTimestamp) result = xmlLoggingTable;
                    }
                }
            }

            return result;
        }

        private static bool AssignTableToLog(ITable t, XMLLoggingTable logTable)
        {
            LogHelper.GetLogger().ContextLog(LogLevelEnum.Debug, "");
            if (logTable == null)
                return false;
            t.Catalog = logTable.Catalog;
            t.Comment = logTable.Comment;
            t.Count = logTable.LogCountDest;
            t.Filter = logTable.Filter;
            t.Schema = logTable.Schema;

            foreach (var col in t.Columns)
            {
                foreach (var logCol in logTable.Columns)
                {
                    if (col.Name.Equals(logCol.Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        col.AllowDBNull = logCol.AllowDBNull;
                        col.AutoIncrement = logCol.AutoIncrement;
                        col.Comment = logCol.Comment;
                        col.DefaultValue = logCol.DefaultValue;
                        col.IsIdentity = logCol.IsIdentity;
                        col.IsPrimaryKey = logCol.IsPrimaryKey;
                        col.IsUnsigned = logCol.IsUnsigned;
                        col.MaxLength = logCol.MaxLength;
                        col.NumericScale = logCol.NumericScale;
                        col.OrdinalPosition = logCol.OrdinalPosition;
                        col.TypeName = logCol.TypeName;
                    }
                }
            }

            return true;
            //if(t.Count != logTable.LogCountDest) {
            //    throw new DataCountNotMatchException("Source(log): " + logTable.LogCountDest + "/" + "File(current): " +
            //                                         t.Count, null);
            //}
        }

        private bool IsOnNetwork(Uri uri) {

            if (uri == null) return false;

            #region [ if network drive ]

            DriveInfo drive = DriveInfo.GetDrives().FirstOrDefault(d => (uri.Segments.Length > 1) ? (d.Name == uri.Segments[0].Replace("/", "\\") || d.Name == uri.Segments[1].Replace("/", "\\")) : (d.Name == uri.Segments[0].Replace("/", "\\")));
            if (drive != null) {
                DriveType driveType = drive.DriveType;
                switch (driveType) {
                    case System.IO.DriveType.CDRom:
                        break;
                    case System.IO.DriveType.Fixed:
                        // Local Drive
                        break;
                    case System.IO.DriveType.Network:
                        // Mapped Drive
                        return true;
                        break;
                    case System.IO.DriveType.NoRootDirectory:
                        break;
                    case System.IO.DriveType.Ram:
                        break;
                    case System.IO.DriveType.Removable:
                        // Usually a USB Drive
                        break;
                    case System.IO.DriveType.Unknown:
                        break;
                }
            }

            #endregion [ if network drive ]

            #region [ if not network drive ]

            IPAddress[] host;
            IPAddress[] local;
            bool isLocal = false;

            host = Dns.GetHostAddresses(uri.Host);
            local = Dns.GetHostAddresses(Dns.GetHostName());

            foreach (IPAddress hostAddress in host) {
                if (IPAddress.IsLoopback(hostAddress)) {
                    isLocal = true;
                    break;
                } else {
                    foreach (IPAddress localAddress in local) {
                        if (hostAddress.Equals(localAddress)) {
                            isLocal = true;
                            break;
                        }
                    }

                    if (isLocal) {
                        break;
                    }
                }
            }
            return isLocal;

            #endregion [ if not network drive ]
        }

        private long CountTable(IEnumerable<string> filenames) {
            _log.ContextLog(LogLevelEnum.Debug, "");

            try {
                _log.ContextLog(LogLevelEnum.Debug, "Method start");

                bool endSeperatorIsNewLine = InputConfig.GetLineEndSeperator().EndsWith("\r\n") ||
                                                InputConfig.GetLineEndSeperator().EndsWith("\n");
                string endSeparator = InputConfig.GetLineEndSeperator();
                bool endSeparatorIsEmpty = string.IsNullOrEmpty(endSeparator);
                string endSeparatorWithoutNewLine = InputConfig.GetLineEndSeperator().Replace("\r", "").Replace("\n", "");
                bool endSeparatorWithoutNewLineIsEmpty = string.IsNullOrEmpty(endSeparatorWithoutNewLine);

                Stopwatch swatch;
                string removeFile = string.Empty;
                long count = 0;
                foreach (string filename in filenames) {
                    int bufferSize = _bufferSizeLocal;
                    bool isOnNetwork = IsOnNetwork(new Uri(filename));
                    if (isOnNetwork) bufferSize = _bufferSizeNetwork;

                    removeFile = string.Empty;
                    if (filename.ToLower().EndsWith(".zip")) {
                        _log.ContextLog(LogLevelEnum.Debug, "Extracting the content of [{0}] to [{1}] started", filename);
                        ZipFile zip = new ZipFile(filename);
                        zip.ExtractAll(new FileInfo(filename).Directory.FullName);
                        removeFile = filename.Substring(0, filename.Length - 4);
                    }

                    _log.ContextLog(LogLevelEnum.Debug, "Counting records started for file [{0}]", filename);
                    swatch = Stopwatch.StartNew();

                    StringBuilder stringBuilder = new StringBuilder();
                    FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                    using (StreamReader streamReader = new StreamReader(fileStream)) {
                        if (endSeperatorIsNewLine) {
                            string line;
                            while ((line = streamReader.ReadLine()) != null) {
                                if (endSeparatorWithoutNewLineIsEmpty) {
                                    count++;
                                } else if (line.EndsWith(endSeparatorWithoutNewLine)) {
                                    count++;
                                }
                            }
                        } else {
                            char[] fileContents = new char[bufferSize];
                            int charsRead = streamReader.ReadBlock(fileContents, 0, bufferSize);
                            int charsReadBefore = 0;

                            if (endSeparatorIsEmpty) {
                                if (charsRead > 0) count++;
                            }
                            else {
                                while (charsRead > 0) {
                                    stringBuilder.Append(fileContents);
                                    if (charsReadBefore > 0) {
                                        while (DetectEndSeparator(stringBuilder, ref count, endSeparator));
                                    }
                                    charsReadBefore = charsRead;
                                    charsRead = streamReader.Read(fileContents, 0, bufferSize);
                                }
                                DetectEndSeparator(stringBuilder, ref count, endSeparator);
                            }
                        }
                    }

                    swatch.Stop();
                    _log.ContextLog(LogLevelEnum.Debug, "Counting records finished for file [{0}], record count: {2}, processing time: {1}", filename, swatch.Elapsed.ToString(), count);

                    _log.ContextLog(LogLevelEnum.Debug, "Deleting file [{0}]", removeFile);
                    if (!string.IsNullOrEmpty(removeFile)) File.Delete(removeFile);
                    _log.ContextLog(LogLevelEnum.Debug, "File deleted [{0}]", removeFile);
                }

                if (InputConfig.IsBaanCsv)
                {
                    if (count > 0)
                        count-= filenames.Count();
                }else{
                    //Subtract one because of the header line
                    if (count > 0 && InputConfig.HeadlineInFirstLine)
                        count--;
                    if (count > 0 && InputConfig.HeadlineInEachFileFirstLine)
                        count -= filenames.Count();
                }

                _log.ContextLog(LogLevelEnum.Debug, "Method end");

                return count;
            }
            catch (Exception ex)
            {
                _log.ContextLog(LogLevelEnum.Error, ex.Message, ex);
                throw;
            }
        }

        private bool DetectEndSeparator(StringBuilder stringBuilder, ref long count, string endSeparator)
        {
            int index = stringBuilder.ToString().IndexOf(endSeparator, StringComparison.InvariantCulture);
            if (index >= 0) {
                count++;
                //stringBuilder.Remove(0, index + 1);
                stringBuilder.Remove(0, index + endSeparator.Length);
                return true;
            }
            return false;
        }
        
        public void Cleanup() { }
        #endregion Methods
    }
}
