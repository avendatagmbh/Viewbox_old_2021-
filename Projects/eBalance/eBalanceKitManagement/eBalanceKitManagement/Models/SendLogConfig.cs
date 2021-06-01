using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utils;
using eBalanceKitBusiness.Logs;
using System.Collections.ObjectModel;
using eBalanceKitBusiness.Manager;
using eBalanceKitResources.Localisation;

namespace eBalanceKitManagement.Models {
    internal class SendLogConfig : LogConfig {
        public override void ShowLogs() {
            LogFilter filter = new LogFilter();

            LogFiles.LogList.Clear();
            LogFiles.ReadSendLogs(filter);
            Items = new ObservableCollection<LogEntryBase>();

            base.ShowLogs();
        }

        /// <summary>
        /// Export the report logs into the given file.
        /// </summary>
        internal override string ExportCsv() {
            try {
                LogFilter filter = new LogFilter();

                LogFiles.LogList.Clear();
                LogFiles.ReadSendLogs(filter);
            

                string filename = "Report_log.csv";
                SaveFileDialog dlgSaveFile = new SaveFileDialog {
                    FileName = filename,
                    DefaultExt = ".csv",
                    Filter = "csv " + ResourcesCommon.Files + " (*.csv)|*.csv"
                };
                DialogResult result = dlgSaveFile.ShowDialog();
                if (result != DialogResult.OK && result != DialogResult.Yes) return "cancelled";
                filename = dlgSaveFile.FileName;

                using (CsvWriter writer = new CsvWriter(filename, Encoding.ASCII)) {

                    writer.WriteCsvData(new[] { ResourcesLogging.Time, ResourcesCommon.User, ResourcesLogging.Event });

                    foreach (LogEntryBase logEntry in LogFiles.LogList) {
                        // write a line into the file.
                        writer.WriteCsvData(new[] {
                            logEntry.TimestampString,
                            UserManager.Instance.GetUser(logEntry.UserId()).DisplayString,
                            logEntry.Message 
                        });
                    }
                }
                return string.Empty;
            } catch (Exception e) {
                return e.Message;
            }
        }
    }
}
