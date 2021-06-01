using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Business.CsvImporter.ExportLogs {
    public class ExportLog {
        #region Constructor
        public ExportLog() {
        }
        #endregion Constructor

        #region Properties
        private readonly Dictionary<string, ExportLogEntry> _exportLogs = new Dictionary<string, ExportLogEntry>(StringComparer.InvariantCultureIgnoreCase);
        #endregion Properties

        #region Methods
        #region ReadFile
        public void ReadFile(string filename) {
            ReadFile(new StreamReader(filename));
        }

        internal void ReadFile(TextReader reader) {
            string line;
            bool firstLine = true;
            while ((line=reader.ReadLine()) != null) {
                if (firstLine) {
                    firstLine = false;
                    continue;
                }
                if (string.IsNullOrEmpty(line))
                    continue;

                ExportLogEntry logEntry = new ExportLogEntry();
                logEntry.ReadFromLine(line);
                _exportLogs[logEntry.TableName] = logEntry;
            }
        }
        #endregion ReadFile

        #region GetEntry
        public ExportLogEntry GetEntry(string table) {
            ExportLogEntry result;
            if (!_exportLogs.TryGetValue(table, out result))
                return null;
            return result;
        }
        #endregion GetEntry

        public int Count() {
            return _exportLogs.Count;
        }
        #endregion Methods
    }
}
