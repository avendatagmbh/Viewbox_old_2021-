using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using AV.Log;
using CommandLineParser.Exceptions;
using MySql.Data.MySqlClient;
using log4net;

namespace ViewboxArchiveConverter
{
    class Program
    {
        private static Options _options;
        private static ILog log = LogHelper.GetLogger();

        [STAThread]
        static void Main(string[] args)
        {
            CommandLineParser.CommandLineParser parser = new CommandLineParser.CommandLineParser();
            _options = new Options();
            parser.ExtractArgumentAttributes(_options);
            try {
                bool continueProcessing = true;
                bool reorderDirectoryStructure = false;

                try {
                    parser.ParseCommandLine(args);
                    continueProcessing = Convert.ToBoolean(_options.Continue);
                    reorderDirectoryStructure = Convert.ToBoolean(_options.ReorderFolders);
                }
                catch (Exception ex) {
                    Console.WriteLine();
                    Console.WriteLine(ex.Message);
                    parser.ShowUsage();
                    return;
                }

                try {
                    string logPath = null;
                    if (!string.IsNullOrWhiteSpace(_options.LogFile)) 
                        logPath = _options.LogFile;
                    else
                        logPath = Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, "log");
                    LogHelper.ConfigureLogger(LogHelper.GetLogger(), logPath);
                    log = LogHelper.GetLogger();
                }
                catch (Exception ex)
                {
                }

                if (!reorderDirectoryStructure)
                    DoConversion();
                else
                    DoReorderDirectoryStructure();
            }
            catch (CommandLineException)
            {
                parser.ShowUsage();
            }
        }

        private static void DoConversion()
        {
            try
            {
                bool continueProcessing = Convert.ToBoolean(_options.Continue);

                #region [ Check ]
                switch (_options.Input.ToLower())
                {
                    case "*.*":
                    case "*.pdf":
                    case "*.fax":
                    case "*.xls":
                    case "*.xlsx":
                    case "*.doc":
                    case "*.docx":
                    case "*.md":
                        break;
                    default:
                        throw new NotSupportedException(string.Format("[{0}] files are not supported", _options.Input));
                }

                switch (_options.Format.ToLower())
                {
                    case ".jpg":
                    case ".pdf":
                        break;
                    default:
                        throw new NotSupportedException(string.Format("Conversion to [{0}] is not supported", _options.Format));
                }

                if (_options.Format.ToLower() == ".pdf" && !(_options.Input.ToLower() == "*.fax" || _options.Input.ToLower() == "*.md"))
                    throw new NotSupportedException(string.Format("Only .fax files are supported to be converted to [{0}]", _options.Format));
                #endregion [ Check ]

                #region [ Directory check ]
                if (!Directory.Exists(_options.ArchiveDirectory)) {
                    LogInfoMessage(string.Format("Directory does not exists [{0}]", _options.ArchiveDirectory));
                    return;
                }

                if (!Directory.Exists(_options.ThumbnailDirectory)) {
                    LogInfoMessage(string.Format("Directory does not exists [{0}], creating ...", _options.ThumbnailDirectory));
                    Directory.CreateDirectory(_options.ThumbnailDirectory);
                    LogInfoMessage(string.Format("Directory [{0}] created.", _options.ThumbnailDirectory));
                }
                #endregion [ Directory check ]

                DirectoryInfo thumbnailsDirectory = new DirectoryInfo(_options.ThumbnailDirectory);
                DirectoryInfo pdfDirectory = new DirectoryInfo(_options.PdfDirectory);
                DirectoryInfo archiveDirectory = new DirectoryInfo(_options.ArchiveDirectory);

                int count = 0;

                DirectoryInfo[] diArr = GetAllDirectory();

                foreach (DirectoryInfo info in diArr) {
                    foreach (string archiveFileName in Directory.EnumerateFiles(info.FullName, _options.Input))
                    {
                        count++;
                        string archiveFileNameLower = archiveFileName.ToLower();
                        FileInfo file = new FileInfo(archiveFileNameLower);
                        string relativePath = info.FullName.Replace(archiveDirectory.FullName + "\\", "");
                        FileConvertOptions fileConverterOptions = new FileConvertOptions()
                        {
                            ArchiveDirectory = archiveDirectory,
                            ThumbnailDirectory = thumbnailsDirectory,
                            PdfDirectory =  pdfDirectory,
                            ContinueProcessing = continueProcessing,
                            ArchiveFileName = file.Name,
                            ConvertFrom = file.Extension,
                            //ConvertTo = ".jpg"
                            ConvertTo = _options.Format,
                            IgnoreFormats = Convert.ToBoolean(_options.IgnoreFormats),
                            CurrentDirectory = info.FullName,
                            CurrentThumbnailDirectory = Path.Combine(thumbnailsDirectory.FullName, relativePath),
                            CurrentPdfDirectory = Path.Combine(pdfDirectory.FullName, relativePath)
                        };
                        switch (file.Extension.ToLower())
                        {
                            case ".fax":
                            case ".pdf":
                            case ".xls":
                            case ".xlsx":
                            case ".doc":
                            case ".docx":
                            case ".txt":
                            case ".md":
                                LogProcessedMessage(ConverterFactory.Instance.GetConverterInstance(file.Extension, fileConverterOptions).Convert(fileConverterOptions), archiveFileName);
                                break;
                            default:
                                if(!CheckFileMetaInfoFile(file))
                                    log.InfoFormat("Unsupported file found. [{0}]", archiveFileName);
                                break;
                        }
                    }                    
                }
                
                LogInfoMessage(string.Format("Processing of files in folder [{0}] has finished finished", _options.ArchiveDirectory));
            }
            catch(IOException ioex) {
                log.Error(ioex.Message, ioex);
                string data = "Exception data:\r\n";
                foreach (DictionaryEntry dictionaryEntry in ioex.Data) {
                    data += "key: " + dictionaryEntry.Key.ToString() + "\tvalue: " + dictionaryEntry.Value.ToString() + "\r\n";
                }
                log.Error(data);
                Console.WriteLine(ioex.Message);
                throw;
            } 
            catch (Exception ex) {
                log.Error(ex.Message, ex);
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private static bool CheckFileMetaInfoFile(FileInfo file)
        {
            try {
                if (file.Extension == "") {
                    FileInfo fi = new FileInfo(file.FullName +".md");
                    return fi.Exists;
                }
                return false;
            } catch {
                return false;
            }
        }

        private static DirectoryInfo[] GetAllDirectory()
        {
            try {
                LogInfoMessage(string.Format("Get directory list [{0}]", _options.ArchiveDirectory));

                DirectoryInfo di = new DirectoryInfo(_options.ArchiveDirectory);
                DirectoryInfo[] diArr = null;
                if (Convert.ToBoolean(_options.Recursive))
                {                    
                    diArr = di.GetDirectories("*.*", SearchOption.AllDirectories);                    
                } else {
                    diArr = new DirectoryInfo[1];
                    diArr[0] = di;
                }

                LogInfoMessage(string.Format("Get directory list done [{0}]", _options.ArchiveDirectory));
                return diArr;
            }
            catch(Exception ex) {
                LogInfoMessage(string.Format("Get directory list error [{0}]", ex.Message));
                return null;
            }
        }

        private static void DoReorderDirectoryStructure() {
            try {
                #region [ Directory check ]
                if (!Directory.Exists(_options.ArchiveDirectory)) {
                    LogInfoMessage(string.Format("Directory does not exists [{0}]", _options.ArchiveDirectory));
                    return;
                }
                #endregion [ Directory check ]

                if (string.IsNullOrWhiteSpace(_options.System))
                    throw new ArgumentException("Argument is null or empty.", "System");

                DirectoryInfo archiveDirectory = new DirectoryInfo(_options.ArchiveDirectory);
                DirectoryInfo thumbnailDirectory = GetDirectory(archiveDirectory, "Thumbnails");
                DirectoryInfo pdfDirectory = GetDirectory(archiveDirectory, "Pdfs");

                List<string> folderStructureExists = new List<string>();

                using (MySqlConnection conn = new MySqlConnection(string.Format("Server={0};Database={1};Uid={2};Pwd={3};", _options.Server, _options.Database, _options.User, _options.Password)))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand()) {
                        cmd.Connection = conn;

                        FileInfo thumbnailFile = null;
                        FileInfo pdfFile = null;
                        string newPath = null;
                        string newFileName = null;
                        string mandt = null;
                        string year = null;
                        string month = null;
                        string type = null;
                        DateTime date;
                        int count = 0;
                        foreach (string archiveFileName in Directory.EnumerateFiles(_options.ArchiveDirectory, _options.Input)) {
                            count++;
                            string archiveFileNameLower = archiveFileName.ToLower();
                            FileInfo file = new FileInfo(archiveFileNameLower);

                            // select file from belegarchiv table
                            //cmd.CommandText = string.Format("SELECT MANDT, AR_DATE, RESERVE FROM {0}.{1} LIMIT 1", _options.System, "belegarchiv", file.Name);
                            cmd.CommandText = string.Format("SELECT MANDT, AR_DATE, RESERVE FROM {0}.{1} WHERE FileName = '{2}'", _options.System, "belegarchiv", file.Name);
                            using(MySqlDataReader rdr = cmd.ExecuteReader()) {
                                if (rdr.Read()) {
                                    mandt = rdr[0].ToString();
                                    date = Convert.ToDateTime(rdr[1]);
                                    year = date.Year.ToString();
                                    month = date.Month.ToString();
                                    type = rdr[2].ToString();

                                    ////**************
                                    //type = file.Extension.Substring(1);
                                    ////**************

                                    // move the archive document to the new folder structure
                                    if (archiveDirectory != null) {
                                        if (File.Exists(file.FullName)) {
                                            newPath = EnsureNewFolderStructure(archiveDirectory, type, mandt, year, month, folderStructureExists);
                                            if (string.IsNullOrEmpty(newPath))
                                                throw new Exception(string.Format("This should not happen, serious problem occurred! ..."));
                                            newFileName = Path.Combine(newPath, file.Name);
                                            if (File.Exists(newFileName)) File.Delete(newFileName);
                                            File.Move(file.FullName, newFileName);
                                        }
                                    }

                                    // move the thumbnail when available to the new folder structure
                                    if (thumbnailDirectory != null) {
                                        thumbnailFile = new FileInfo(Path.Combine(thumbnailDirectory.FullName, file.Name.Replace(file.Extension, "") + ".jpg"));
                                        if (File.Exists(thumbnailFile.FullName)) {
                                            newPath = EnsureNewFolderStructure(thumbnailDirectory, type, mandt, year, month, folderStructureExists);
                                            if (string.IsNullOrEmpty(newPath))
                                                throw new Exception(string.Format("This should not happen, serious problem occurred! ..."));
                                            newFileName = Path.Combine(newPath, thumbnailFile.Name);
                                            if (File.Exists(newFileName)) File.Delete(newFileName);
                                            File.Move(thumbnailFile.FullName, newFileName);
                                        }
                                    }

                                    // move the pdf when available to the new folder structure
                                    if (pdfDirectory != null) {
                                        pdfFile = new FileInfo(Path.Combine(pdfDirectory.FullName, file.Name.Replace(file.Extension, "") + ".pdf"));
                                        if (File.Exists(pdfFile.FullName)) {
                                            newPath = EnsureNewFolderStructure(pdfDirectory, type, mandt, year, month, folderStructureExists);
                                            if (string.IsNullOrEmpty(newPath))
                                                throw new Exception(string.Format("This should not happen, serious problem occurred! ..."));
                                            newFileName = Path.Combine(newPath, pdfFile.Name);
                                            if (File.Exists(newFileName)) File.Delete(newFileName);
                                            File.Move(pdfFile.FullName, newFileName);
                                        }
                                    }
                                }
                            }
                            // move file to new folder
                        }
                    }
                }

            } catch (Exception ex) {
                log.Error(ex.Message, ex);
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private static string EnsureNewFolderStructure(DirectoryInfo directory, string type, string mandt, string year, string month, List<string> folderStructureExists)
        {
            string fullPath = Path.Combine(directory.FullName, type, mandt, year, month).ToLower();
            if (folderStructureExists.Contains(fullPath))
                return fullPath;

            string path = Path.Combine(directory.FullName, type);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path = Path.Combine(path, mandt);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path = Path.Combine(path, year);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path = Path.Combine(path, month);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            folderStructureExists.Add(path.ToLower());
            return path;
        }
        
        private static DirectoryInfo GetDirectory(DirectoryInfo archiveDirectory, string suggestedName) {
            string path = Path.Combine(archiveDirectory.FullName, suggestedName);
            if (Directory.Exists(path))
                return new DirectoryInfo(path);

            if (path.ToLower().EndsWith("s"))
                path = path.Substring(0, path.Length - 1);
            else
                return null;

            if (Directory.Exists(path))
                return new DirectoryInfo(path);

            return null;
        }

        private static void LogProcessedMessage(bool result, string fileName)
        {
            if (result)
                LogInfoMessage(string.Format("File [{0}] processed", fileName));
            else
                LogInfoMessage(string.Format("File processing [{0}] failed", fileName));
        }

        private static void LogInfoMessage(string logMsg)
        {
            Console.WriteLine(logMsg);
            log.Info(logMsg);
        }
    }
}
