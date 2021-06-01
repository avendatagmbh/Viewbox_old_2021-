using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AV.Log;
using Base.Localisation;
using DbAccess.Structures;
using Microsoft.Win32;
using Utils;
using System.IO;
using Microsoft.Office.Interop.Access;
using ViewAssistant.Controls;
using log4net;
using Application = Microsoft.Office.Interop.Access.Application;

namespace ViewAssistantBusiness.Models
{
    public delegate void MessageShownHandler(String message);

    public class AccessMergerModel : NotifyPropertyChangedBase
    {
        public AccessMergerModel()
        {
            OutputTablesWithLabels = true;
            WhiteSpaceNotSupportedInTables = false;
            OutputFileLabel = "_final";

            MergeFileCommand = new RelayCommand(
                x =>
                    {
                        Task.Factory.StartNew(MergeFileClick);
                    });
            CopyAccessFilesCommand = new RelayCommand(
                x =>
                    {
                        Task.Factory.StartNew(CopyAccessFilesClick);
                    });
            LinkingFileCommand = new RelayCommand(
                x =>
                    {
                        Task.Factory.StartNew(LinkingFileClick);
                    });

            TotalProgressNumber = 0;
            IsStarted = false;
        }

        #region Events 

        #region StartUpEvent 

        public event EventHandler StartUp;

        public void OnStartUp()
        {
            EventHandler handler = StartUp;
            if (handler != null) handler(this, null);
        }

        #endregion StartUpEvent

        #region InfoShownEvent

        public event MessageShownHandler InfoShownEvent;

        public void OnInfoShownEvent(String e)
        {
            if (InfoShownEvent != null) InfoShownEvent(e);
        }

        #endregion InfoShownEvent

        #region ErrorShownEvent

        public event MessageShownHandler ErrorShownEvent;

        public void OnErrorShownEvent(String e)
        {
            if (ErrorShownEvent != null) ErrorShownEvent(e);
        }

        #endregion ErrorShownEvent

        #endregion Events

        #region Properties

        private readonly ILog _logger = LogHelper.GetLogger();

        #region IsStarted

        private bool _isStarted;
        public Boolean IsStarted
        {
            get
            {
                return _isStarted;
            }
            set
            {
                _isStarted = value;
                OnPropertyChanged("IsStarted");
            }
        }

        #endregion IsStarted

        #region InputDirectory

        private String _inputDirectory;

        public String InputDirectory
        {
            get { return _inputDirectory; }
            set
            {
                if (_inputDirectory != value)
                {
                    _inputDirectory = value;
                    OnPropertyChanged("InputDirectory");
                    OnPropertyChanged("InputFiles");
                }
            }
        }

        #endregion InputDirectory

        #region InputFiles

        public List<FileInfo> InputFiles
        {
            get
            {
                var dir = new DirectoryInfo(InputDirectory);
                var fileList = dir.GetFiles("*.mdb").ToList();
                fileList.AddRange(dir.GetFiles("*.accdb"));
                return fileList;
            }
        }

        #endregion InputFiles

        #region TotalProgressNumber

        private int _totalProgressNumber;
        public int TotalProgressNumber
        {
            get
            {
                return _totalProgressNumber;
            }
            set
            {
                _totalProgressNumber = value;
                OnPropertyChanged("TotalProgressNumber");
            }
        }

        #endregion TotalProgressNumber

        #region ActualProgressNumber

        private int _actualProgressNumber;
        public int ActualProgressNumber
        {
            get
            {
                return _actualProgressNumber;
            }
            set
            {
                _actualProgressNumber = value;
                OnPropertyChanged("ActualProgressNumber");
            }
        }

        #endregion ActualProgressNumber

        #region OutputFileName

        private String _outputFileName;

        public String OutputFileName
        {
            get { return _outputFileName; }
            set
            {
                if (_outputFileName != value)
                {
                    _outputFileName = value;
                    OnPropertyChanged("OutputFileName");
                }
            }
        }

        #endregion OutputFileName

        #region OutputTablesWithLabels

        private Boolean _outputTablesWithLabels;

        public Boolean OutputTablesWithLabels
        {
            get { return _outputTablesWithLabels; }
            set
            {
                if (_outputTablesWithLabels != value)
                {
                    _outputTablesWithLabels = value;
                    OnPropertyChanged("OutputTablesWithLabels");
                }
            }
        }

        #endregion OutputTablesWithLabels 

        #region WhiteSpaceNotSupportedInTables

        private Boolean _whiteSpaceNotSupportedInTables;

        public Boolean WhiteSpaceNotSupportedInTables
        {
            get { return _whiteSpaceNotSupportedInTables; }
            set
            {
                if (_whiteSpaceNotSupportedInTables != value)
                {
                    _whiteSpaceNotSupportedInTables = value;
                    OnPropertyChanged("WhiteSpaceNotSupportedInTables");
                }
            }
        }

        #endregion WhiteSpaceNotSupportedInTables

        #region OutputFileLabel

        private String _outputFileLabel;

        public String OutputFileLabel
        {
            get { return _outputFileLabel; }
            set
            {
                if (_outputFileLabel != value)
                {
                    _outputFileLabel = value;
                    OnPropertyChanged("OutputFileLabel");
                }
            }
        }

        #endregion OutputFileLabel

        #region OutputDirectory

        private String _outputDirectory;

        public String OutputDirectory
        {
            get { return _outputDirectory; }
            set
            {
                if (_outputDirectory != value)
                {
                    _outputDirectory = value;
                    OnPropertyChanged("OutputDirectory");
                }
            }
        }

        #endregion OutputDirectory

        #region ExpectedFileSize

        public long ExpectedFileSize
        {
            get { return InputFiles.Sum(x => x.Length); }
        }

        #endregion ExpectedFileSize

        public static Boolean IsSuitableAccessInstalled
        {
            get
            {
                var rootKey = Registry.ClassesRoot.OpenSubKey(@"Access.Application\CurVer", false);

                if (rootKey == null)
                {
                    return false;
                }

                var value = rootKey.GetValue("").ToString();

                Int32 verNum;
                var good = Int32.TryParse(value.Substring(19), out verNum);

                return good && value.StartsWith("Access.Application.") && verNum >= 12;
            }
        }

        #endregion Properties
        
        #region private methods

        private void ShowErrorMessage(String message)
        {
            OnErrorShownEvent(message);
            _logger.Error(message);
        }

        private void ShowInfoMessage(String message)
        {
            OnInfoShownEvent(message);
            _logger.Info(message);
        }

        private void InitOperation()
        {
            if(!IsStarted)
            {
                OnStartUp();
                TotalProgressNumber = 0;
            }
            foreach (var file in InputFiles)
            {
                var input = new DbConfig { DbType = "Access", Hostname = file.FullName };

                using (var iConn = input.CreateConnection())
                {
                    if (!iConn.IsOpen)
                    {
                        iConn.Open();
                    }
                    TotalProgressNumber += iConn.GetTableList().Count;
                    iConn.Close();
                }
            }
            ActualProgressNumber = 0;
            IsStarted = true;
            OnPropertyChanged("TotalProgressNumber");
        }

        #endregion private methods

        #region Commands

        #region MergeFileCommand

        private bool MergeOrLinkingInputFault()
        {
            if (String.IsNullOrEmpty(OutputFileName) || String.IsNullOrEmpty(InputDirectory))
            {
                OnErrorShownEvent(ResourcesCommon.FillingEveryFieldCompulsory);
                return true;
            }
            return false;
        }

        private void MergeFileClick()
        {
            if(MergeOrLinkingInputFault())
            {
                return;
            }

            InitOperation();

            var fileSizeInGB = ExpectedFileSize / (1024 * 1024 * 1024);
            if (fileSizeInGB >= 2)
            {
                ShowErrorMessage(ResourcesCommon.BigFileSizeMergingException);
                return;
            }

            var accessOutput = new Application();
            if (File.Exists(OutputFileName))
            {
                File.Delete(OutputFileName);
            }
            accessOutput.NewCurrentDatabase(OutputFileName);

            try
            {
                foreach (var file in InputFiles)
                {
                    var input = new DbConfig { DbType = "Access", Hostname = file.FullName };
                    List<string> tables;

                    using (var iConn = input.CreateConnection())
                    {
                        if (!iConn.IsOpen)
                        {
                            iConn.Open();
                        }
                        tables = iConn.GetTableList();
                        iConn.Close();
                    }

                    foreach (var table in tables)
                    {
                        var newTablename = 
                            (OutputTablesWithLabels ? (Path.GetFileNameWithoutExtension(file.Name) + "_")  : "") + table;
                        if (WhiteSpaceNotSupportedInTables)
                        {
                            newTablename =
                                newTablename.Replace(" ", "_").Replace("\n", "_").Replace("\r", "_").Replace("\t", "_");
                        }
                        try
                        {
                            accessOutput.DoCmd.TransferDatabase(AcDataTransferType.acImport, "Microsoft Access",
                                                                file.FullName,
                                                                AcObjectType.acTable, table, newTablename);
                        }
                        finally
                        {
                            accessOutput.DoCmd.Close(AcObjectType.acTable, newTablename, AcCloseSave.acSaveYes);
                        }
                        ++ActualProgressNumber;
                    }
                }
                ShowInfoMessage(ResourcesCommon.MergingFilesSuccessful);
            }
            catch
            {
                accessOutput.CloseCurrentDatabase();
                accessOutput.Quit();
                if (File.Exists(OutputFileName))
                {
                    File.Delete(OutputFileName);
                }
                ShowErrorMessage(ResourcesCommon.MergingFilesFailed);  
            }
            accessOutput.CloseCurrentDatabase();
            accessOutput.Quit();
        }

        public ICommand MergeFileCommand { get; set; }
        #endregion MergeFileCommand

        #region CopyAccessFilesCommand

        private bool CopyInputFault()
        {
            if (String.IsNullOrEmpty(OutputDirectory) || String.IsNullOrEmpty(InputDirectory))
            {
                OnErrorShownEvent(ResourcesCommon.FillingEveryFieldCompulsory);
                return true;
            }
            return false;
        }

        private void CopyAccessFilesClick()
        {
            if (CopyInputFault())
            {
                return;
            }

            InitOperation();

            var sb = new StringBuilder();

            Parallel.ForEach(InputFiles,
                             file =>
                                 {
                                     var input = new DbConfig {DbType = "Access", Hostname = file.FullName};
                                     List<string> tables;

                                     using (var iConn = input.CreateConnection())
                                     {
                                         if (!iConn.IsOpen)
                                         {
                                             iConn.Open();
                                         }
                                         tables = iConn.GetTableList();
                                         iConn.Close();
                                     }

                                     var onfb = new StringBuilder();
                                     onfb.Append(OutputDirectory);
                                     onfb.Append("\\");
                                     onfb.Append(Path.GetFileNameWithoutExtension(file.Name));
                                     onfb.Append(OutputFileLabel);
                                     onfb.Append(file.Extension);

                                     var output = new DbConfig {DbType = "Access", Hostname = onfb.ToString()};

                                     var accessCopy = new Application();
                                     if (File.Exists(output.Hostname))
                                     {
                                         File.Delete(output.Hostname);
                                     }
                                     accessCopy.NewCurrentDatabase(output.Hostname);

                                     try
                                     {
                                         foreach (var table in tables)
                                         {
                                             var newTablename =
                                                 (OutputTablesWithLabels
                                                      ? (Path.GetFileNameWithoutExtension(file.Name) + "_")
                                                      : "") + table;
                                             if (WhiteSpaceNotSupportedInTables)
                                             {
                                                 newTablename =
                                                     newTablename.Replace(" ", "_").Replace("\n", "_").
                                                         Replace("\r", "_").Replace("\t", "_");
                                             }
                                             try
                                             {
                                                 accessCopy.DoCmd.TransferDatabase(AcDataTransferType.acImport,
                                                                                   "Microsoft Access",
                                                                                   file.FullName,
                                                                                   AcObjectType.acTable, table,
                                                                                   newTablename);
                                             }
                                             finally
                                             {
                                                 accessCopy.DoCmd.Close(AcObjectType.acTable, newTablename,
                                                                        AcCloseSave.acSaveYes);
                                             }
                                             ++ActualProgressNumber;
                                         }
                                     }
                                     catch
                                     {
                                         accessCopy.CloseCurrentDatabase();
                                         accessCopy.Quit();
                                         if (File.Exists(output.Hostname))
                                         {
                                             File.Delete(output.Hostname);
                                         }
                                         sb.Append(Environment.NewLine);
                                         sb.Append(file.FullName);
                                     }
                                     accessCopy.CloseCurrentDatabase();
                                     accessCopy.Quit();
                                 });
            if(sb.Length != 0)
            {
                sb.Insert(0, ResourcesCommon.CopyFileError);
                OnErrorShownEvent(sb.ToString());
            }
            else
            {
                OnInfoShownEvent(ResourcesCommon.CopyFileFinishedSuccessfully);
            }
        }

        public ICommand CopyAccessFilesCommand { get; set; }

        #endregion CopyAccessFilesCommand

        #region LinkingFileCommand

        private void LinkingFileClick()
        {
            if (MergeOrLinkingInputFault())
            {
                return;
            }

            InitOperation();

            var accessOutput = new Application();
            if (File.Exists(OutputFileName))
            {
                File.Delete(OutputFileName);
            }
            accessOutput.NewCurrentDatabase(OutputFileName);

            try
            {
                foreach (var file in InputFiles)
                {
                    var input = new DbConfig { DbType = "Access", Hostname = file.FullName };
                    List<string> tables;

                    using (var iConn = input.CreateConnection())
                    {
                        if (!iConn.IsOpen)
                        {
                            iConn.Open();
                        }
                        tables = iConn.GetTableList();
                        iConn.Close();
                    }

                    foreach (var table in tables)
                    {
                        var newTablename =
                            (OutputTablesWithLabels ? (Path.GetFileNameWithoutExtension(file.Name) + "_") : "") + table;
                        if (WhiteSpaceNotSupportedInTables)
                        {
                            newTablename =
                                newTablename.Replace(" ", "_").Replace("\n", "_").Replace("\r", "_").Replace("\t", "_");
                        }
                        try
                        {
                            accessOutput.DoCmd.TransferDatabase(AcDataTransferType.acLink, "Microsoft Access",
                                                                file.FullName,
                                                                AcObjectType.acTable, table, newTablename);
                        }
                        finally
                        {
                            accessOutput.DoCmd.Close(AcObjectType.acTable, newTablename, AcCloseSave.acSaveYes);
                        }
                        ++ActualProgressNumber;
                    }
                }
                ShowInfoMessage(ResourcesCommon.LinkingFilesSuccessful);
            }
            catch
            {
                accessOutput.CloseCurrentDatabase();
                accessOutput.Quit();
                if (File.Exists(OutputFileName))
                {
                    File.Delete(OutputFileName);
                }
                ShowErrorMessage(ResourcesCommon.LinkingFilesFailed);
            }
            accessOutput.CloseCurrentDatabase();
            accessOutput.Quit();
        }

        public ICommand LinkingFileCommand { get; set; }

        #endregion LinkingFileCommand

        #endregion Commands
    }
}
