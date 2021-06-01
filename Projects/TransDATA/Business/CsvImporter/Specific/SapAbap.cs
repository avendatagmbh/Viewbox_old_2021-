using System;
using System.Text.RegularExpressions;
using Business.CsvImporter;
using Business.CsvImporter.Events;
using Business.CsvImporter.ExportLogs;
using Business.CsvImporter.Structures;
using Config.Config;
using Config.Interfaces.Config;
using Config.Interfaces.DbStructure;
using Config.Structures;
using DbAccess;
using System.IO;

namespace DbImport.CsvImport.CsvImporter {

    internal class SapAbap : CsvImporterBase, ICsvImporter {

        public SapAbap(StateCsvImport state, CsvInputConfig csvInputConfig, IDatabaseOutputConfig outputConfig)
            : base(state, csvInputConfig, outputConfig) { }

        private readonly ExportLog _exportLog = new ExportLog();

        public override void Init() {
            base.Init();

            //Read export_logs 
            foreach (
                var file in
                    Directory.EnumerateFiles(CsvInputConfig.Folder, "export_log_*.csv",
                                             CsvInputConfig.ImportSubDirectories
                                                 ? SearchOption.AllDirectories
                                                 : SearchOption.TopDirectoryOnly)) {
                if (!Regex.IsMatch(new FileInfo(file).Name, @"^export_log_\d{8}_\d{6}.csv$"))
                    continue;
                _exportLog.ReadFile(file);
            }
        }

        protected override bool Validate(long destCount, ITable table) {
            ExportLogEntry logEntry = _exportLog.GetEntry(table.Name);
            if(logEntry == null) {
                table.TransferState.State = TransferStates.TransferedCountDifference;
                table.TransferState.Message =
                    "Die Datensatz Anzahl der Tabelle konnte nicht validiert werden, da sie nicht im Exportlog verzeichnet ist.";
                return false;
            }
            else if(logEntry.CountAfter != destCount) {
                table.TransferState.State = TransferStates.TransferedCountDifference;
                table.TransferState.Message =
                    string.Format(
                        "Die Datensatz Anzahl stimmt nicht überein. Im SAP waren CountBefore:{0}, CountAfter:{1}. In der Datenbank ist der Count:{2}",
                        logEntry.CountBefore, logEntry.CountAfter, destCount);
                return false;
            }
            return true;
            //if (destCount != table.Count) {
            //    table.TransferState.State = TransferStates.TransferedCountDifference;
            //    table.TransferState.Message =
            //        string.Format(
            //            "Anzahl der Zeilen weicht. Die CSV Datei hat {0} Einträge, die Datenbank-Tabelle hat {1} Einträge.",
            //            table.Count, destCount);
            //}

        }
    }
}
