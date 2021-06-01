using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBalanceKitBase.Structures.Backup {
    /// <summary>
    /// Class for configuration of the automatic backup service.
    /// All <see cref="DateTime"/> information are stored in UTC (<see cref="DateTime.KindUtc"/>) to be universal.
    /// </summary>
    public class AutomaticBackupConfig {
        #region Constructor
        public AutomaticBackupConfig() {
            ConfigTime = DateTime.UtcNow; 
            Weekdays = new List<DayOfWeek>();
            StartTime = DateTime.UtcNow;
        }
        #endregion Constructor

        #region Properties

        #region const
        /// <summary>
        /// Identifies which string is used to delimit the entries of <see cref="Weekdays"/> in the String representation.
        /// </summary>
        private static string DelimiterWeekDay { get { return ";"; } }

        /// <summary>
        /// Identifies which string is used to delimit the different properties in the String representation.
        /// </summary>
        private static string DelimiterConfig { get { return "|^|"; } }

        #endregion

        /// <summary>
        /// Identification what time the config was created / saved the last time.
        /// Not used at the moment.
        /// </summary>
        private DateTime ConfigTime { get; set; }
        /// <summary>
        /// The destination where the backup will be saved to.
        /// </summary>
        public string File { get; set; }
        /// <summary>
        /// Get or set the type of backup plan (daily, weekly, monthly).
        /// </summary>
        public BackupType Type { get; set; }
        /// <summary>
        /// Contains <see cref="DayOfWeek"/> the backup has to be done.
        /// Used only if <see cref="Type"/> = <see cref="eBalanceKitBase.Structures.Backup.AutomaticBackupConfig.BackupType.Weekly"/>. 
        /// </summary>
        public List<DayOfWeek> Weekdays { get; set; }
        /// <summary>
        /// The <see cref="DateTime"/> the first backup has to be done and base for every following backup. Stored as <see cref="DateTime.KindUtc"/>.
        /// </summary>
        public DateTime StartTime { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Load a configuration from the given string.
        /// </summary>
        /// <param name="info">A string in a format like "(int) <see cref="Type"/>|^|week_(int)<see cref="Weekdays"/>;_week|^|<see cref="StartTime"/>.ToBinary()|^|<see cref="File"/>|^|"</param>
        /// <returns>A AutomaticBackupConfig with the information contained in the <see cref="info"/> parameter.</returns>
        public static AutomaticBackupConfig Load(string info) {
            if (string.IsNullOrEmpty(info)) {
                return null;
            }

            var result = new AutomaticBackupConfig();

            var infoArray = info.Split(new[] { DelimiterConfig }, StringSplitOptions.None);


            int currentStep = 0;
            {
                if (infoArray.Length < currentStep) {
                    return result;
                }
                // Step 0 = Type
                int type;

                if (int.TryParse(infoArray[currentStep], out type)) {
                    result.Type = (BackupType)type;
                }
                currentStep++;
            }
            {
                if (infoArray.Length < currentStep) {
                    return result;
                }

                // Step 1 = WeekDays
                if (infoArray[currentStep].StartsWith("week_") && infoArray[currentStep].EndsWith("_week")) {
                    if (result.Type == BackupType.Weekly) {
                        var weekdayList =
                            infoArray[currentStep].Replace("week_", string.Empty).Replace("_week", string.Empty).Split(
                                new[] {DelimiterWeekDay}, StringSplitOptions.None);

                        foreach (var weekday in weekdayList) {
                            int weekdayNo;
                            if (int.TryParse(weekday, out weekdayNo)) {
                                result.Weekdays.Add((DayOfWeek) weekdayNo);
                            }
                        }
                    }
                    currentStep++;
                }
            } 
            {
                {
                    if (infoArray.Length < currentStep) {
                        return result;
                    }

                    // Step 2 = StartTime
                    long startTime;
                    
                    if (long.TryParse(infoArray[currentStep], out startTime)) {
                        result.StartTime = DateTime.FromBinary(startTime);
                    }
                    currentStep++;
                }
            }
            {
                if (infoArray.Length < currentStep) {
                    return result;
                }

                // Step 3 = File
                result.File = infoArray[currentStep];
            }

            return result;

        }

        /// <summary>
        /// Representation of this config that is stored in the ConfigFile
        /// </summary>
        /// <returns>(int) <see cref="Type"/>|^|week_(int)<see cref="Weekdays"/>;_week|^|<see cref="StartTime"/>.ToBinary()|^|<see cref="File"/>|^|</returns>
        public override string ToString() {
            //return base.ToString();
            StringBuilder result = new StringBuilder();
            result.Append((int) Type);
            result.Append(DelimiterConfig);
            result.Append("week_");
            foreach (var weekday in Weekdays) {
                result.Append((int)weekday);
                result.Append(DelimiterWeekDay);
            }
            result.Append("_week");
            result.Append(DelimiterConfig);
            result.Append(StartTime.ToBinary());
            result.Append(DelimiterConfig);
            result.Append(File);
            result.Append(DelimiterConfig);


            return result.ToString();
        }

        #endregion Methods


        public enum BackupType {
            /// <summary>
            /// A backup has to be generated every day.
            /// </summary>
            Daily,
            /// <summary>
            /// A backup has to be generated at the in <see cref="AutomaticBackupConfig.Weekdays"/> specified days.
            /// </summary>
            Weekly,
            /// <summary>
            /// A backup has to be generated once in a month.
            /// </summary>
            Monthly
        }
    }
}
