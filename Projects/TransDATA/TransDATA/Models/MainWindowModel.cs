// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-08-28
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml;
using AV.Log;
using Base.Localisation;
using Business;
using Business.Events;
using Business.Structures.Pdf;
using Config;
using Config.Interfaces.DbStructure;
using Config.Interfaces.Mail;
using Config.Manager;
using Logging;
using TransDATA.Windows;
using Utils;
using log4net;
using MessageBox = System.Windows.MessageBox;
using System.Diagnostics;
using Config.Interfaces.Config;

namespace TransDATA.Models
{
    /// <summary>
    /// Model for the main window.
    /// </summary>
    internal class MainWindowModel : NotifyPropertyChangedBase
    {
        internal ILog _log = LogHelper.GetLogger();

        public MainWindowModel(Window owner, IUser user)
        {
            Owner = owner;
            User = user;
            AppController.ProfileManager.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ProfileManager_PropertyChanged);
        }

        private void ProfileManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LastProfile")
                OnPropertyChanged("SelectedProfile");
        }

        #region Owner
        private Window Owner { get; set; }
        #endregion

        #region Profiles
        public IEnumerable<IProfile> Profiles { get { return AppController.ProfileManager.VisibleProfiles; } }
        #endregion

        #region SelectedProfile
        private IProfile _selectedProfile;

        public IProfile SelectedProfile
        {
            get
            {
                if (_selectedProfile == null)
                {
                    _selectedProfile = AppController.ProfileManager.LastProfile;
                    SelectedProfileModel = (_selectedProfile == null
                                            ? null
                                            : SelectedProfileModel = new SelectedProfileModel(_selectedProfile));
                }
                return _selectedProfile;
            }
            set
            {
                _selectedProfile = value;
                SelectedProfileModel = (_selectedProfile == null
                                            ? null
                                            : SelectedProfileModel = new SelectedProfileModel(value));

                OnPropertyChanged("SelectedProfile");
            }
        }

        private void CheckDataAccess()
        {
            if (_selectedProfile == null) return;

            var transferAgent = AppController.GetDataTransferAgent(SelectedProfile);
            SelectedProfile.DataSourceAvailable = transferAgent.CheckDataAccessSource();
            SelectedProfile.DataDestinationAvailable = transferAgent.CheckDataAccessDestination();
            SelectedProfile.DataSourceTooltip = transferAgent.GetSourceTooltip();
            SelectedProfile.DataDestinationTooltip = transferAgent.GetDestinationTooltip();
        }
        #endregion

        #region SelectedProfileModel
        private SelectedProfileModel _selectedProfileModel;

        public SelectedProfileModel SelectedProfileModel
        {
            get { return _selectedProfileModel; }
            private set
            {
                _selectedProfileModel = value;
                OnPropertyChanged("SelectedProfileModel");
                CheckDataAccess();
            }
        }
        #endregion

        #region EditProfile
        public void EditProfile()
        {
            if (HandleSelectedProfileNull())
                return;
            var dlgEditProfile = new DlgEditProfile() { Owner = Owner };
            var model = new EditProfileModel(SelectedProfile, dlgEditProfile);
            dlgEditProfile.DataContext = model;
            dlgEditProfile.ShowDialog();

            CheckDataAccess();
        }
        #endregion EditProfile

        #region HandleSelectedProfileNull
        private bool HandleSelectedProfileNull()
        {
            if (SelectedProfile == null)
            {
                MessageBox.Show(Owner, ResourcesCommon.NoProfileSelected, ResourcesCommon.NoProfileSelectedCaption,
                                MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
                return true;
            }
            return false;
        }
        #endregion HandleSelectedProfileNull

        #region ReadDatabaseStructure
        public void ReadDatabaseStructure()
        {
            if (HandleSelectedProfileNull())
                return;
            string error;
            if (!SelectedProfile.InputConfig.Config.Validate(out error))
            {
                _log.ContextLog( LogLevelEnum.Error, "SelectedProfile.InputConfig.Config.Validate: {0}", error);
                MessageBox.Show(Owner, ResourcesCommon.MainWindowErrorReadingDBStructure + Environment.NewLine + error, "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (MessageBox.Show(Owner, ResourcesCommon.MainWindowLongTimeRunning, "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;
            var dlgImportdbStructure = new DlgImportDbStructure
            {
                Owner = Owner
            };
            var model = new ImportDbStructureModel(dlgImportdbStructure);
            //var selectedProfile = SelectedProfile;
            model.StartImportDbStructure(SelectedProfile);
            dlgImportdbStructure.DataContext = model;
            dlgImportdbStructure.ShowDialog();
            //Force gui update
            SelectedProfile = SelectedProfile;

            SelectedProfile.IsDatabaseAnalysed = true;
            SelectedProfile.Save();
        }
        #endregion ReadDatabaseStructure

        #region EditMailSettings
        public void EditMailSettings()
        {
            var model = new EditMailModel(MailManager.GlobalMailConfig.Clone());
            var dlgEditMail = new DlgEditMail()
            {
                Owner = Owner,
                DataContext = model
            };

            dlgEditMail.ShowDialog();
            if (model.Saved)
                MailManager.GlobalMailConfig = model.MailConfig;
        }
        #endregion EditMailSettings

        #region User
        private IUser _user;
        public IUser User
        {
            get { return _user; }
            set
            {
                if (_user != value)
                {
                    _user = value;
                    OnPropertyChanged("User");
                }
            }
        }
        #endregion User

        public void StartTransfer()
        {
            if (HandleSelectedProfileNull())
                return;
            var dlgTransfer = new DlgTransfer() { Owner = Owner };
            var model = new TransferModel(SelectedProfile, dlgTransfer);
            dlgTransfer.DataContext = model;
            if (model.StartExport())
            {
                model.TransferAgent.Finished += TransferFinished;
                dlgTransfer.ShowDialog();
            }
        }



        private void TransferFinished(object sender, EventArgs e)
        {
            if (Owner.Dispatcher.CheckAccess())
            {
                // default document path.
                string exportDocumentPath = AppController.GetDataTransferAgent(SelectedProfile).GetLogDirectory();

                

                //// set documentation path to "%DestinationPath%\Documentation"
                //if (SelectedProfile.OutputConfig.Config is ICsvOutputConfig)
                //    exportDocumentPath = ((ICsvOutputConfig)SelectedProfile.OutputConfig.Config).Folder + "\\log" ;

                //if (SelectedProfile.OutputConfig.Config is IGdpduConfig)
                //    exportDocumentPath = ((IGdpduConfig)SelectedProfile.OutputConfig.Config).Folder + "\\log";

                //// create if is not exists
                //if (!Directory.Exists(exportDocumentPath))
                //    Directory.CreateDirectory(exportDocumentPath);


                // generating log-files (xml, csv)
                GenerateCsvXmlFiles(exportDocumentPath);

                // generating log-file (pdf)
                FileInfo pdgFileInfo = GeneratePDFFile(exportDocumentPath);

                

                // sending logfile (pdf)
                if (pdgFileInfo != null)
                    SendMail(string.Format(Base.Localisation.ResourcesCommon.MailSubjectFinished, SelectedProfile.Name), pdgFileInfo);

            }
            else
            {
                Owner.Dispatcher.Invoke(new EventHandler(TransferFinished), new[] { sender, e });
            }
        }

        private bool SendMail(string subject, FileInfo attachement)
        {
            return MailManager.SendMailMessage(SelectedProfile.MailConfig, subject,
                                        Base.Localisation.ResourcesCommon.MailFileInAttachement,
                                        SelectedProfile.MailConfig.Host.ToLower() == "smtp.mail.yahoo.de" ? "rsadata7@yahoo.de" : "Transdata@avendata.de",
                //"rsadata7@yahoo.de",
                                        new List<FileInfo>() { attachement });
        }



        public void ShowProfileManagement()
        {
            var dlg = new DlgProfileManagement { Owner = Owner };
            var model = new ProfileManagementModel(dlg);
            dlg.DataContext = model;
            PropertyChangedEventHandler selectProfileDelegate = delegate(object sender, PropertyChangedEventArgs args)
            {
                if (args.PropertyName == "SelectedItem")
                    SelectedProfile = model.SelectedItem;

            };

            EventHandler modelOnClosed = delegate(object sender, EventArgs args)
            {
                var mod = sender as ProfileManagementModel;
                foreach (var profile in mod.Items)
                {
                    if (!profile.IsDatabaseAnalysed)
                    {
                        ReadDatabaseStructure();
                    }
                }
            };

            model.Closed += modelOnClosed;
            model.PropertyChanged += selectProfileDelegate;
            dlg.ShowDialog();
            model.PropertyChanged -= selectProfileDelegate;
            model.Closed -= modelOnClosed;
        }

        internal void EditUserSettings()
        {
            new DlgEditUser { DataContext = new EditUserModel(User) }.ShowDialog();
        }

        internal void CreateDocumentation()
        {
            if (HandleSelectedProfileNull())
                return;

            // choosing export-path for log
            var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() != DialogResult.OK) return;

            // generating log-files (xml, csv)
            GenerateCsvXmlFiles(dlg.SelectedPath);

            // generating log-file (pdf)
            GeneratePDFFile(dlg.SelectedPath);

            MessageBox.Show(Base.Localisation.ResourcesCommon.DocumentationCreated);
        }

        private FileInfo GeneratePDFFile(string path)
        {
            // generating pdf
            string destFullPath = path + "\\" +
                Base.Localisation.ResourcesCommon.LogFile +
                              DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss") + ".pdf";


            var loggingDb = new LoggingDb(Config.ConfigDb.ConnectionManager);
            var logFileGenerator = new LogfileGenerator(null, SelectedProfile.Name);
            var tableList = new List<ITransferEntity>();
            foreach (var table in SelectedProfile.Tables)
            {
                tableList.Add(table);
            }
            var file = logFileGenerator.GenerateLogFile(loggingDb, tableList);
            File.Move(file.FullName, destFullPath);

            return new FileInfo(destFullPath);
        }

        private void GenerateCsvXmlFiles(string path)
        {
            // variables for xml
            var xmlSettings = new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineChars = Environment.NewLine };
            var xmlSb = new StringBuilder();
            var xmlWriter = XmlWriter.Create(xmlSb, xmlSettings);
            var xmlFileWriter = new StreamWriter(path + "\\" +
                Base.Localisation.ResourcesCommon.LogFile +
                              DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss") + ".xml");

            // variables for csv
            var csvHeader = "Tabellenname;Count;CountLog;CountDest;Error";
            var csvFileWriter = new StreamWriter(path + "\\" +
                Base.Localisation.ResourcesCommon.LogFile +
                              DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss") + ".csv");

            // variables for logging-database
            var loggingDb = new LoggingDb(Config.ConfigDb.ConnectionManager);

            // begin of xml
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Documentation");
            xmlWriter.WriteElementString("DateTime", DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));

            // begin of csv
            csvFileWriter.WriteLine(csvHeader);

            // itterate over all tables in profile
            foreach (var table in SelectedProfile.Tables)
            {
                // get last log-entry for specific table
                var log = loggingDb.GetLogTables(table.Id);

                if (log.Count == 0)
                {
                    HandleNoLogXml(table, xmlWriter);
                    HandleNoLogCsv(table, csvFileWriter);
                }
                else
                {
                    var logItem = log[log.Count - 1];

                    HandleLogXml(table, logItem, xmlWriter);
                    HandleLogCsv(table, logItem, csvFileWriter);
                }
            }

            // end of xml
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            xmlFileWriter.Write(xmlSb.ToString());
            xmlFileWriter.Flush();
            xmlFileWriter.Close();

            // end of csv
            csvFileWriter.Flush();
            csvFileWriter.Close();
        }

        private void HandleLogCsv(ITable table, Logging.Interfaces.DbStructure.ITable logItem, StreamWriter csvFileWriter)
        {
            csvFileWriter.WriteLine(table.Name + ";" + table.Count + ";" + logItem.Count + ";" + logItem.CountDest + ";" +
                                    (string.IsNullOrEmpty(logItem.Error)
                                         ? "-"
                                         : logItem.Error.Replace("\r", string.Empty).Replace("\n", string.Empty)));
        }

        private void HandleLogXml(ITable table, Logging.Interfaces.DbStructure.ITable logItem, XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("Table");
            new XMLLoggingTable()
            {
                Catalog = table.Catalog,
                Schema = table.Schema,
                Name = table.Name,
                Comment = table.Comment,
                Count = table.Count,
                Filter = table.Filter,
                TransferStateState = (int)table.TransferState.State,
                TransferStateMessage = table.TransferState.Message,
                LogCount = logItem.Count,
                LogCountDest = logItem.CountDest,
                LogDuration = logItem.Duration,
                LogError = logItem.Error,
                LogFilter = logItem.Filter,
                LogTimestamp = logItem.Timestamp,
                LogUsername = logItem.Username,
                LogState = logItem.State.ToString()
            }.WriteToXML(xmlWriter);

            foreach (var col in logItem.Columns)
            {
                foreach (var column in table.Columns)
                {
                    if (col.ColumnId != column.Id) continue;
                    xmlWriter.WriteStartElement("Column");
                    new XMLLoggingColumn()
                    {
                        Name = column.Name,
                        Comment = column.Comment,
                        Type = column.Type.ToString(),
                        TypeName = column.TypeName,
                        DbType = column.DbType.ToString(),
                        MaxLength = column.MaxLength,
                        DefaultValue = column.DefaultValue,
                        AllowDBNull = column.AllowDBNull,
                        AutoIncrement = column.AutoIncrement,
                        NumericScale = column.NumericScale,
                        IsPrimaryKey = column.IsPrimaryKey,
                        IsUnsigned = column.IsUnsigned,
                        IsIdentity = column.IsIdentity,
                        OrdinalPosition = column.OrdinalPosition
                    }.WriteToXML(xmlWriter);
                    xmlWriter.WriteEndElement();
                }
            }
            xmlWriter.WriteEndElement();
        }

        private void HandleNoLogCsv(ITable table, StreamWriter csvFileWriter)
        {
            csvFileWriter.WriteLine(table.Name + ";-;-;-;-");
        }

        private void HandleNoLogXml(ITable table, XmlWriter xmlWriter)
        {
            // doing nothing
        }

        public void OpenLogDirectory()
        {
            if (_selectedProfile == null) return;
            Process.Start(AppController.GetDataTransferAgent(SelectedProfile).GetLogDirectory());
        }
    }
}