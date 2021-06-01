using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using DatabaseManagement.Structures;
using Utils.Commands;
using eBalanceKitBase.Structures;
using eBalanceKitBase.Structures.Backup;
using eBalanceKitResources.Localisation;
using AutomaticBackupConfig = eBalanceKitBase.Structures.Backup.AutomaticBackupConfig;
using ServiceInstaller = DatabaseManagement.Structures.ServiceInstaller;

namespace DatabaseManagement.Models {
    public class AutomaticBackupModel : Utils.NotifyPropertyChangedBase {
        #region Constructor
        public AutomaticBackupModel() {
            _saveCommand = new DelegateCommand((obj) => true, SaveOptions);
            ServiceStopCommand = new DelegateCommand((obj) => true, StopSerivce);
            ServiceStartCommand = new DelegateCommand((obj) => true, StartSerivce);
            ServiceRestartCommand = new DelegateCommand((obj) => true, RestartSerivce);
            ServiceInstallCommand = new DelegateCommand((obj) => true, InstallSerivce);
            if(Config == null) {
                Config = new eBalanceKitBase.Structures.Backup.AutomaticBackupConfig();
            }
            WeekDayChecker = InitWeekDayDict();
            // transform the time to local time because the user would like to see and enter the StartTime depending on his local time.
            StartDate = Config.StartTime.ToLocalTime();
            StartTime = StartDate.Hour;

            
            InitService();
        }


        #endregion Constructor

        #region Properties
        public Window Owner { get; set; }
        /// <summary>
        /// Name of the Service to create the automatic backups.
        /// </summary>
        public string ServiceName { get { return "EBilanzBackupService"; } }
        /// <summary>
        /// Path incl. name of the Installer for the Service to create the automatic backups.
        /// </summary>
        private string InstallerName {
            get {
                return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
                    "\\"+ ServiceName + "Setup.msi";
            }
        }
        /// <summary>
        /// The Configuration (Manager.DatabaseManager.DatabaseConfig.AutomaticBackupConfig)
        /// </summary>
        public AutomaticBackupConfig Config { get { return Manager.DatabaseManager.DatabaseConfig.AutomaticBackupConfig; } set { Manager.DatabaseManager.DatabaseConfig.AutomaticBackupConfig = value; } }

        /// <summary>
        /// Dictionary that contains all weekday and the information if the day was selected.
        /// Used only for <see cref="eBalanceKitBase.Structures.Backup.AutomaticBackupConfig.BackupType.Weekly"/>.
        /// </summary>
        public Dictionary<DayOfWeek, bool> WeekDayChecker { get; set; }

        #region StartTime
        private int _startTime;

        public int StartTime {
            get { return _startTime; }
            set {
                if (_startTime != value) {
                    _startTime = value;
                    OnPropertyChanged("StartTime");
                }
            }
        }
        #endregion StartTime

        #region StartDate
        private DateTime _startDate;

        public DateTime StartDate {
            get { return _startDate; }
            set {
                if (_startDate != value) {
                    _startDate = value;
                    OnPropertyChanged("StartDate");
                }
            }
        }
        #endregion StartDate
        
        #endregion Properties


        #region Service
        private ServiceController _service;

        private ServiceController Service {
            get { return _service; }
            set {
                _service = value;
                OnPropertyChanged("Service");
                OnPropertyChanged("ServiceStarted");
                OnPropertyChanged("ServiceInstalled");
                OnPropertyChanged("ServiceInstalledNotStarted");
            }
        }

        
        public bool ServiceInstalled { get { return Service != null; } }

        public bool ServiceStarted { get { return Service != null && Service.Status == ServiceControllerStatus.Running; } }
        public bool ServiceInstalledStarted { get { return Service != null && Service.Status == ServiceControllerStatus.Running; } }

        public ServiceControllerStatus? ServiceStatus { get { return Service != null ? Service.Status : (ServiceControllerStatus?) null; } }

        /// <summary>
        /// Only limited used at the moment because it would be required to allow execute as different user if the user has no admin rights and all this stuff.
        /// Starting is used after installation (but fails if non admin and produces a MessageBox).
        /// </summary>
        #region service handling

        #region start service

        public ICommand ServiceStartCommand { get; private set; }

        /// <summary>
        /// Starts the service for the automatic backup.
        /// </summary>
        /// <param name="obj"></param>
        private void StartSerivce(Object obj) {
            if (Service == null) {
                return;
            }

            try {
                Service.Start();
            } catch (InvalidOperationException e) {
                // ToDo show message that no rights for starting
                System.Windows.MessageBox.Show(ResourcesExternalTools.SerivceCouldNotStart + Environment.NewLine +
                                               ResourcesCommon.InsufficientRights, ResourcesCommon.Error);
            } catch (Exception e) {
                ExceptionLogging.LogException(e);
                System.Windows.MessageBox.Show(ResourcesExternalTools.SerivceCouldNotStart + Environment.NewLine + e.Message, ResourcesCommon.Error);
            }
        }
        #endregion

        #region restart service

        public ICommand ServiceRestartCommand { get; private set; }

        /// <summary>
        /// Restarts the service for the automatic backup.
        /// Waits after calling stop service 500ms while Service.Status != ServiceControllerStatus.Stopped
        /// </summary>
        /// <param name="obj"></param>
        private void RestartSerivce(Object obj) {
            if (Service == null) {
                return;
            }

            try {
                StopSerivce(obj);
                while (Service.Status != ServiceControllerStatus.Stopped) {
                    Thread.Sleep(500);
                }
                StartSerivce(obj);
            } catch (Exception e) {
                ExceptionLogging.LogException(e);
            }
        }
        #endregion

        #region stop service

        public ICommand ServiceStopCommand { get; private set; }

        /// <summary>
        /// Stops the service for the automatic backup.
        /// </summary>
        /// <param name="obj"></param>
        private void StopSerivce(Object obj) {
            if (Service == null) {
                return;
            }

            try {
                Service.Stop();
            } catch (Exception e) {
                ExceptionLogging.LogException(e);
            }
        }

        #endregion

        #endregion

        #region install service

        public ICommand ServiceInstallCommand { get; private set; }

        /// <summary>
        /// Calls the <see cref="InstallerName"/> to install the service.
        /// </summary>
        private void InstallSerivce(object obj) {

            try {
                ProcessStartInfo pInfo = new ProcessStartInfo();
                //Set the file name member of the process info structure.
                pInfo.FileName = InstallerName;
                //Start the process.
                Process p = Process.Start(pInfo);
                //Wait for the window to finish loading.
                p.WaitForInputIdle();
                //Wait for the process to end.
                p.WaitForExit();

                InitService();
            } catch (Exception e) {
                ExceptionLogging.LogException(e);
                System.Windows.MessageBox.Show(ResourcesExternalTools.StartingProcessFailed + Environment.NewLine +
                                               e.Message, ResourcesCommon.Installation);
            }

            StartSerivce(null);

            OnPropertyChanged("Service");
            OnPropertyChanged("ServiceStarted");
            OnPropertyChanged("ServiceInstalled");
            OnPropertyChanged("ServiceInstalledNotStarted");
        
        }

        #region InstallerExisting
        public bool InstallerExisting {
            get {
                try {
                    return System.IO.File.Exists(InstallerName);
                } catch (Exception e) {
                    ExceptionLogging.LogException(e);
                    return false;
                }
            }
        }
        #endregion InstallerExisting

        #endregion
        /// <summary>
        /// Checks if the specified service is installed on the specified machine.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="machineName">Name of the computer. If default (NULL) it will be searched on the local machine </param>
        /// <returns>Serivce is exisiting?</returns>
        private bool CheckServiceExist(string serviceName, string machineName = null) {
            ServiceController[] services = machineName == null ? ServiceController.GetServices() : ServiceController.GetServices(machineName);
            var service = services.FirstOrDefault(s => s.ServiceName == serviceName);
            return service != null;
        }

        /// <summary>
        /// Initialize the service if it is exisiting. Existence will be checked by calling <see cref="CheckServiceExist"/>.
        /// </summary>
        private void InitService() {

            try {
                if (CheckServiceExist(ServiceName)) {
                    Service = new ServiceController(ServiceName);
                }
            }
            catch (Exception e) {
                ExceptionLogging.LogException(e);
            }
        }

        #endregion Service


        #region Save
        private readonly ICommand _saveCommand;

        public ICommand SaveCommand { get { return _saveCommand; } }


        #region SaveFolder

        public string SaveFolder {
            get { return Config.File; }
            set {
                if (Config.File != value) {
                    Config.File = value;
                    OnPropertyChanged("SaveFolder");
                }
            }
        }
        #endregion SaveFolder

        private void SaveOptions(object obj) {
            //if (string.IsNullOrEmpty(StartDate)) {
            //    return;
            //}
            //if (string.IsNullOrEmpty(StartTime)) {
            //    StartTime = "0";
            //}
            Config.Weekdays.Clear();
            Config.Weekdays.AddRange(from entry in WeekDayChecker where entry.Value select entry.Key);
            Config.StartTime = new DateTime(StartDate.Year, StartDate.Month, StartDate.Day, StartTime, 0, 0, DateTimeKind.Local).ToUniversalTime();
            //Config.Save();
            Manager.DatabaseManager.DatabaseConfig.Save();
            MessageBox.Show("Einstellungen wurden erfolgreich gespeichert.", "", MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }
        #endregion Save


        #region Methods
        /// <summary>
        /// Initialze a Dictionary that can be used for <see cref="WeekDayChecker"/> by checking Config.
        /// </summary>
        /// <returns>New Dictionary based on the Config.</returns>
        private Dictionary<DayOfWeek, bool> InitWeekDayDict() {
            var result = new Dictionary<DayOfWeek, bool>();
            DayOfWeek firstDay = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            for (int dayIndex = 0; dayIndex < 7; dayIndex++) {
                var currentDay = (DayOfWeek)(((int)firstDay + dayIndex) % 7);

                result.Add(currentDay, Config.Type == AutomaticBackupConfig.BackupType.Weekly ? Config.Weekdays.Contains(currentDay) : false);
            }
            return result;
        }
        #endregion Methods
    }
}
