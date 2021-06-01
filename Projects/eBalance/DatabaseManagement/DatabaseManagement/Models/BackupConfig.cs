// --------------------------------------------------------------------------------
// author: Benjamin held
// since:  2011-07-26
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using DatabaseManagement.DbUpgrade;
using Utils;

namespace DatabaseManagement.Models {
    public class BackupConfig : NotifyPropertyChangedBase {

        private string _backupDirectory;
        private FileSystemWatcher watcher;

        public BackupConfig(Window owner, string configPath) {
            BackupDirectory = configPath;
            Owner = owner;
            SelectedFileUserInfo = new eBalanceBackup.UserInfo();
        }

        public string BackupDirectory {
            get { return _backupDirectory; }
            set {
                if (_backupDirectory != value && _backupDirectory != value + "\\") {
                    _backupDirectory = value;
                    if (!string.IsNullOrEmpty(_backupDirectory) && _backupDirectory.Last() != '\\')
                        _backupDirectory += "\\";
                    ListFilesOfDirectory();
                    OnPropertyChanged("BackupDirectory");

                    if (_backupDirectory == null || !Directory.Exists(_backupDirectory)) {
                        return;
                    }

                    try {
                        watcher = new FileSystemWatcher();
                        watcher.Path = _backupDirectory;
                        watcher.Filter = "*.bak";
                        watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName;
                        watcher.Created += OnDirectoryContentsChanged;
                        watcher.Deleted += OnDirectoryContentsChanged;
                        watcher.Renamed += OnDirectoryContentsChanged;
                        watcher.EnableRaisingEvents = true;
                    } catch (Exception) {
                        throw new Exception("Das Backup-Verzeichnis \"" + _backupDirectory +
                                            "\" existiert nicht oder ist nicht lesbar, bitte ändern Sie es im Backup Reiter.");
                    }
                }
            }
        }

        public eBalanceBackup.UserInfo SelectedFileUserInfo { get; set; }
        public Window Owner { get; set; }

        private void OnDirectoryContentsChanged(object sender, FileSystemEventArgs e) {
            Owner.Dispatcher.Invoke(
                DispatcherPriority.Background,
                new Action(delegate { ListFilesOfDirectory(); }));
        }

        private void OnDirectoryContentsChanged(object sender, RenamedEventArgs e) {
            Owner.Dispatcher.Invoke(
                DispatcherPriority.Background,
                new Action(delegate { ListFilesOfDirectory(); }));
        }

        private void ListFilesOfDirectory() {
            var directoryInfo = new DirectoryInfo(BackupDirectory);
            if (directoryInfo.Exists) {
                FileInfo[] bakFiles = directoryInfo.GetFiles("*.bak");
                Files.Clear();
                foreach (FileInfo fi in bakFiles) {
                    Files.Add(fi.Name);
                }
            }
        }

        private void GetUserInfoOfFile(string filename) {
            if (string.IsNullOrEmpty(filename)) {
                SelectedFileUserInfo = new eBalanceBackup.UserInfo();
                return;
            }
            SelectedFileUserInfo = new eBalanceBackup.UserInfo();
            var backup = new eBalanceBackup();
            SelectedFileUserInfo = backup.ReadUserInfoFromFile(BackupDirectory + filename);
            OnPropertyChanged("SelectedFileUserInfo");
        }

        #region Files
        private ObservableCollection<string> _files = new ObservableCollection<string>();

        public ObservableCollection<string> Files {
            get { return _files; }
            set {
                _files = value;
                OnPropertyChanged("Files");
            }
        }
        #endregion

        #region SelectedFile
        private object _selectedFile;

        public object SelectedFile {
            get { return _selectedFile; }
            set {
                _selectedFile = value;
                GetUserInfoOfFile(value as string);
                OnPropertyChanged("SelectedItem");
            }
        }
        #endregion
    }
}