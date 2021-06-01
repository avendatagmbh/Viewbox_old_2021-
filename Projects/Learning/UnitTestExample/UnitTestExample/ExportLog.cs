using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UnitTestExample {
    class ExportLog {
        internal ExportLog() {
            GetDataSource = filename => new StreamReader(filename);
            //GetDataSource = delegate(string filename) { return new StreamReader(filename); };
            //GetDataSource = GetDataSourceImpl;
        }


        #region Properties
        public delegate TextReader GetDataSourceFunction(string filename);
        public GetDataSourceFunction GetDataSource { get; set; }

        private readonly Dictionary<string, ExportLogEntry> _exportLogs = new Dictionary<string, ExportLogEntry>(StringComparer.InvariantCultureIgnoreCase);
        #endregion Properties

        #region Methods
        #region ReadFile
        public void ReadFile(string filename) {
            //ReadFile(new StreamReader(filename));
            ReadFile(GetDataSource(filename));
        }

        internal void ReadFile(TextReader reader) {
            string line;
            bool firstLine = true;
            while ((line = reader.ReadLine()) != null) {
                if (firstLine) {
                    firstLine = false;
                    continue;
                }
                if (string.IsNullOrEmpty(line))
                    continue;

                ExportLogEntry logEntry = ExportLogEntry.ReadFromLine(line);
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
