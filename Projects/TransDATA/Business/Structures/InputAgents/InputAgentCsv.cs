// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using AV.Log;
using Config;
using Config.Interfaces.Config;
using Config.Interfaces.DbStructure;
using Ionic.Zip;
using Ude;
using Utils;
using log4net;
using IDataReader = Business.Interfaces.IDataReader;

namespace Business.Structures.InputAgents {
    public class InputAgentCsv : InputAgentBase {
        internal ILog _log = LogHelper.GetLogger();

        public InputAgentCsv(IInputConfig config)
            : base(config) { }

        public override IDataReader GetDataReader(ITransferEntity entity, bool useAdo = false) {
            _log.ContextLog(LogLevelEnum.Debug, "NotImplementedException");
            throw new NotImplementedException();
            //TODO: implement GetDataReader
        }

        private ICsvInputConfig CsvConfig { get { return Config.Config as ICsvInputConfig; } }

        public override DataTable GetPreview(ITransferEntity entity, long count) {
            _log.ContextLog(LogLevelEnum.Debug, "");
            var table = entity as ITable;
            if (table == null || table.FileNames.Count == 0) {
                _log.ContextLog(LogLevelEnum.Debug, "Config not set or no files");
                return new DataTable();
            }
            //CsvReader reader = new CsvReader(table.FileNames[0]){Separator = CsvConfig.FieldSeperator};

            //reader.GetCsvData(count, Encoding.GetEncoding(CsvConfig.FileEncoding));
            return DataTableFromFilename(count, table.FileNames[0], CsvConfig);
        }

        public static DataTable DataTableFromFilename(long count, string filename, ICsvInputConfig csvConfig) {
            LogHelper.GetLogger().ContextLog(LogLevelEnum.Debug, "");
            Stopwatch sw = new Stopwatch();

            // for configuration backward compatibility: read and transform Config.FileEncoding
            Encoding encoding = Encoding.UTF8;
            try {
                if (csvConfig.FileEncoding == "utf8") encoding = Encoding.UTF8;
                else if (csvConfig.FileEncoding == "latin1") encoding = Encoding.GetEncoding(1252);
                else encoding = Encoding.GetEncoding(csvConfig.FileEncoding);
            } catch (Exception ex) {
                LogHelper.GetLogger().ContextLog(LogLevelEnum.Error,
                                                 "User interaction needed! Select right FileEncoding in profile! Error: {0}",
                                                 ex.Message);

                throw new Exception(
                    "Select right FileEncoding in profile! Error: "+ ex.Message);
            }


            EnhancedCsvReader csvReader = new EnhancedCsvReader(filename) {
                Separator = csvConfig.GetFieldSeperator(),
                EndOfLine = csvConfig.GetLineEndSeperator()
            };

            if (csvConfig.OptionallyEnclosedBy.Length == 0)
                csvReader.StringsOptionallyEnclosedBy = null;
            else if (csvConfig.OptionallyEnclosedBy.Length == 1)
                csvReader.StringsOptionallyEnclosedBy = csvConfig.OptionallyEnclosedBy[0];
            else {
                LogHelper.GetLogger().ContextLog(LogLevelEnum.Error,
                                                 "User interaction needed! Preview data from CSV files with OptionallyEnclosedBy with more than one character is not possible.");
                throw new Exception(
                    "Datenvorschau von CSV Dateien mit OptionallyEnclosedBy mit mehr als einem Zeichen ist nicht möglich.");
            }

            return csvReader.GetCsvData((int) count, encoding);
        }

        public override long GetCount(ITransferEntity entity) {
            _log.ContextLog(LogLevelEnum.Debug, "");
            Stopwatch sw =new Stopwatch();
            sw.Restart();

            var removeFile = string.Empty;
            var table = entity as ITable;

            long count = 0;
            foreach (string filename in table.FileNames) {
                var fi = filename;

                var endSeperatorIsNewLine = CsvConfig.GetLineEndSeperator().EndsWith("\r\n") ||
                                            CsvConfig.GetLineEndSeperator().EndsWith("\n");
                var endSeparator = CsvConfig.GetLineEndSeperator().Replace("\r", "").Replace("\n", "");

                if (fi.ToLower().EndsWith(".zip")) {
                    var file = new FileInfo(fi);
                    ZipFile zip = new ZipFile(fi);
                    //zip.ExtractAll(new FileInfo(fi).Directory.FullName);
                    zip.ExtractAll(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
                    //fi = fi.Substring(0, fi.Length - 4);
                    fi = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" +
                         file.Name.Substring(0, file.Name.Length - 4);
                    ;
                    removeFile = fi;
                }

                using (var sReader = new StreamReader(fi)) {
                    string line = string.Empty;
                    if (endSeperatorIsNewLine) {
                        while ((line = sReader.ReadLine()) != null) {
                            if (line.EndsWith(endSeparator)) count++;
                        }
                    } else {
                        int searchStart = 0;
                        while ((line = sReader.ReadLine()) != null) {
                            while (
                                (searchStart =
                                 line.IndexOf(endSeparator, searchStart, StringComparison.InvariantCulture) + 1) != 0) {
                                count++;
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(removeFile)) File.Delete(removeFile);
            }


            if (CsvConfig.IsBaanCsv) {
                // in each of baan files are column names
                count -= table.FileNames.Count;
            } else {
                // Subtract one because of the header line
                if (CsvConfig.HeadlineInFirstLine)
                    count--;
                if (CsvConfig.HeadlineInEachFileFirstLine)
                    count-= table.FileNames.Count;
            }

            sw.Stop();
            _log.ContextLog(LogLevelEnum.Debug, "Count: {0} Time: {1}", count, sw.Elapsed.ToString());


            return count;
        }

        public override bool CheckDataAccess() {
            _log.ContextLog(LogLevelEnum.Debug, "");

            return true;
            //TODO: implement CheckDataAccess
        }

        public override string GetDescription()
        {
            _log.ContextLog(LogLevelEnum.Debug, "");
            return "Csv" + Environment.NewLine + CsvConfig.Folder;
        }
    }
}