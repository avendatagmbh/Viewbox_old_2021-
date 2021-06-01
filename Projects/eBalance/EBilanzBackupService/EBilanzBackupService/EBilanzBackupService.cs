using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using DatabaseManagement.DbUpgrade;
using DbAccess;
using eBalanceKitBase;
using System.Threading.Tasks;
using System.IO;
using eBalanceKitBase.Structures.Backup;

namespace EBilanzBackupService {
    public partial class EBilanzBackupService : ServiceBase {

        
        #region DatabaseConfig
        private DatabaseConfig _databaseConfig;

        public DatabaseConfig DatabaseConfig {
            get {
                if (_databaseConfig == null) {
                    LoadConfiguration();
                }
                return _databaseConfig;
            }
            set { _databaseConfig = value; 
            }
        }
        #endregion DatabaseConfig

#if DEBUG
        private const string Logdestination = @"C:\ebk\";
#endif



        private bool Running { get; set; }

        private TimeSpan SleepingTimespan { get; set; }
        private DateTime LastExecuted { get; set; }
        private readonly TimeSpan DefaultSleepingTime;

        private bool ExecuteBackup {
            get {
                var xx = IsBefore(Execute);
                LogManager.Log("ExecuteBackup after IsBefore(Execute):" + xx);
                xx &= !IsToday(LastExecuted);
                LogManager.Log("ExecuteBackup after & !IsToday(LastExecuted:" + xx);
                LogManager.Log("ExecuteBackup:" + Execute);
                return xx;
            }
        }

        public EBilanzBackupService() {
            DefaultSleepingTime = new TimeSpan(1, 0, 0);
            SleepingTimespan = new TimeSpan(0, 1, 0);

            InitializeComponent();
            LogManager.Log("Initialized");
#if DEBUG

            eBalanceKitBase.Structures.ExceptionLogging.LogFilename = Logdestination + @"eBalanceKitBackup.exception.log";
            if (!Directory.Exists(Logdestination)) Directory.CreateDirectory(Logdestination);
#endif

        }

        protected override void OnStart(string[] args) {
            LogManager.Log("Started");
            Running = true;
            DatabaseConfig = new DatabaseConfig();
            DatabaseConfig.LoadConfig();
            Task.Factory.StartNew(Perfrom);
        }

        protected override void OnStop() {
            LogManager.Log("Stopped");
            Running = false;
        }

        private void LoadConfiguration() {
            _databaseConfig = new DatabaseConfig();
            _databaseConfig.LoadConfig();
        }

        protected override void OnContinue() { LogManager.Log("Continued"); }

        private void Perfrom() {
                LogManager.Log("Running called " + Running);
            while (Running) {
                LogManager.Log("Running:" + Running);
                if (ExecuteBackup) {
                    LogManager.Log("ExecuteBackup:" + ExecuteBackup);
                    DoBackup();
                }

                CalculateNewExecutionTime();

                var difference = Execute.Subtract(DateTime.UtcNow);
                
                //if difference between next execution time and default sleeping time + now is less than default sleeping time than sleep only for difference time
                System.Threading.Thread.Sleep(difference < DefaultSleepingTime ? difference : DefaultSleepingTime);


                LogManager.Log("ExecuteBackup at " + Execute);
                LogManager.Log("now it's " + DateTime.UtcNow);
            }
        }

        private void DoBackup() {
            LogManager.Log("EBilanzBackupService beginnt Arbeit");

            var filename = (DatabaseConfig.AutomaticBackupConfig.File.EndsWith("\\") ? DatabaseConfig.AutomaticBackupConfig.File : DatabaseConfig.AutomaticBackupConfig.File + "\\") +
                                @"eBalanceKitBackup_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".bak";
            var backup = new eBalanceBackup();
            var userInfo = new eBalanceBackup.UserInfo {Comment = "Automatic Backup " + ServiceName};

            try {
                using (IDatabase conn = ConnectionManager.CreateConnection(DatabaseConfig.DbConfig)) {
                    if (conn.IsOpen) {
                        LogManager.Log("conn.IsOpen");
                    } else {
                        LogManager.Log("!conn.IsOpen");
                        conn.Open();
                    }
                    backup.ExportDatabase(conn, filename, userInfo, true);
                }
                LogManager.Log("EBilanzBackupService backup abgeschlossen");
            } catch (Exception ex) {
                eBalanceKitBase.Structures.ExceptionLogging.LogException(ex);
            } finally {
                CalculateNewExecutionTime();
                LastExecuted = DateTime.UtcNow;
            }
        }

        private DateTime Start { get { return DatabaseConfig.AutomaticBackupConfig.StartTime; } }
        private DateTime Execute { get; set; }


        private void CalculateNewExecutionTime() {
            var x = 0;
            LogManager.Log("in CalculateNewExecutionTime");

            LoadConfiguration();

                        var newTime = Start;

            try {
                LogManager.Log("DatabaseConfig.AutomaticBackupConfig.Type = " + DatabaseConfig.AutomaticBackupConfig.Type);
                switch (DatabaseConfig.AutomaticBackupConfig.Type) {
                    case AutomaticBackupConfig.BackupType.Daily:
                        LogManager.Log("Daily");
                        while (IsBefore(newTime)) {
                            LogManager.Log(newTime.ToString());
                            newTime = newTime.AddDays(1);
                        }

                            LogManager.Log(newTime.ToString());
                        break;

                    case AutomaticBackupConfig.BackupType.Weekly:
                        LogManager.Log("Weekly");

                        while (IsBefore(newTime)) {
                            LogManager.Log("first loop " + newTime.ToString());
                            newTime = newTime.AddDays(1);
                        }

                        while (!DatabaseConfig.AutomaticBackupConfig.Weekdays.Contains(newTime.DayOfWeek)) {
                            LogManager.Log("2nd loop" + newTime.ToString());
                            newTime = newTime.AddDays(1);
                        }

                            LogManager.Log(newTime.ToString());

                        break;

                    case AutomaticBackupConfig.BackupType.Monthly:
                        LogManager.Log("Monthly");
                        while (IsBefore(newTime)) {
                            newTime = newTime.AddMonths(1);
                        }
                            LogManager.Log(newTime.ToString());
                        break;
                }

                LogManager.Log("end of switch in CalculateNewExecutionTime with " + newTime);

                var daysInMonth = DateTime.DaysInMonth(newTime.Year, newTime.Month);

                int executionDay = newTime.Day > daysInMonth ? daysInMonth : newTime.Day;

                Execute = new DateTime(newTime.Year, newTime.Month, executionDay, Start.Hour, Start.Minute, Start.Second,
                                       DateTimeKind.Utc);

                LogManager.Log("end of CalculateNewExecutionTime with " + Execute);
            } catch (Exception e) {
                eBalanceKitBase.Structures.ExceptionLogging.LogException(e);
            }
        }

        private bool IsBefore(DateTime compareTime) {
            LogManager.Log("IsBefore " + DateTime.UtcNow + " started to compared to " + compareTime);

            var baseTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
                                       DateTime.UtcNow.Hour, 0, 0);

            compareTime = new DateTime(compareTime.Year, compareTime.Month, compareTime.Day, compareTime.Hour, 0, 0);

            LogManager.Log("IsBefore " + baseTime + " compared to " + compareTime + " is " + (compareTime.CompareTo(baseTime) < 0));

            return compareTime.CompareTo(baseTime) < 0;
        }

        private bool IsToday(DateTime compareTime) {
            LogManager.Log("IsToday " + DateTime.UtcNow + " started to compared to " + compareTime);

            var baseTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
                                       0, 0, 0);

            compareTime = new DateTime(compareTime.Year, compareTime.Month, compareTime.Day, 0, 0, 0);

            LogManager.Log("IsToday " + baseTime + " compared to " + compareTime + " is " + (compareTime.CompareTo(baseTime) < 0));

            return compareTime.CompareTo(baseTime) < 0;
        }


        private static class LogManager {

#if DEBUG
            private static string file = Logdestination + @"logFile.csv";
#endif

            public static void Log(string logText) {
#if DEBUG
                using (var writer = new StreamWriter(file, true)) {
                    writer.WriteLine(DateTime.Now.ToString(CultureInfo.InvariantCulture) + ";" + logText);
                }
#endif
            }
        }
    }

}