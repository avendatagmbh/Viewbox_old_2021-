// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Business.Interfaces;
using Config.Interfaces.DbStructure;
using System.IO;
using Config.Config;
using System.Text;
using System;
using Config.Structures;
using Ionic.Zip;
using Logging;
using System.Linq;
using System.Windows.Forms;

namespace Business.Structures.OutputAgents {
    internal class OutputAgentCsv : OutputAgentBase {
        public OutputAgentCsv(IProfile profile, IInputAgent inputAgent, IOutputConfig config)
            : base(profile, inputAgent, config) { }

        private CsvOutputConfig CsvOutputConfig { get { return (CsvOutputConfig)Config.Config; } }
        private IDataReader Reader { get; set; }

        private static string GetCsvFileName(string tableName) {
            var invalidChars = new[] {
                (char) 34, // "
                (char) 42, // *
                (char) 47, // /
                (char) 58, // :
                (char) 60, // <
                (char) 62, // >
                (char) 63, // ?
                (char) 92, // \
                (char) 124 // | 
            };

            string tmp = invalidChars.Aggregate(tableName,
                                                (current, invalidChar) =>
                                                current.Replace(invalidChar.ToString(), "%" + ((int)invalidChar) + "%"));
            for (int i = 0; i <= 31; i++)
                tmp = tmp.Replace(((char)i).ToString(), "%" + i + "%");
            return tmp + ".csv";
        }

        public override void InitTransfer() {
            if (!Directory.Exists(CsvOutputConfig.Folder))
                Directory.CreateDirectory(CsvOutputConfig.Folder);
        }

        public override void ProcessEntity(ITransferEntity entity, TransferTableProgress progress, Logging.LoggingDb loggingDb, bool useAdo = false) {
            int datasetCount = 0;
            Logging.Interfaces.DbStructure.ITable logEntity = loggingDb.CreateLogEntity((ITable) entity, Profile);
            logEntity.Timestamp = progress.StartTime;

            try {
                logEntity.Count = InputAgent.GetCount(entity);

                bool hasColumns = false;
                foreach (var column in entity.Columns) {
                    if (column.DoExport) hasColumns = true;
                }

                if (hasColumns) {
                    Encoding enc;
                    try {
                        enc = Encoding.GetEncoding(CsvOutputConfig.FileEncoding);
                    } catch (Exception) {
                        enc = Encoding.UTF8;
                    }

                    string outputFolder = CsvOutputConfig.Folder + "\\";
                    if (entity is Config.Interfaces.DbStructure.ITable)
                    {
                        var tab = entity as Config.Interfaces.DbStructure.ITable;
                        if (!string.IsNullOrEmpty(tab.Catalog)) outputFolder += tab.Catalog.Replace("*","_STAR_") + "\\";
                        if (!string.IsNullOrEmpty(tab.Schema)) outputFolder += tab.Schema.Replace("*", "_STAR_") + "\\";
                    }
                    if(!Directory.Exists(outputFolder))
                    {
                        Directory.CreateDirectory(outputFolder);
                    }

                    using (var w = new StreamWriter(
                        new FileStream(outputFolder + (string.IsNullOrEmpty(entity.Filter)? string.Empty : entity.Filter.Replace("<", "SMALLER").Replace(">","BIGGER")) + GetCsvFileName(entity.Name),
                                       FileMode.Create),
                                       enc))
                    {
                        // writing header
                        var sb = new StringBuilder();
                        foreach (var col in entity.Columns) {
                            if (!col.DoExport) continue;
                            if (sb.Length > 0) sb.Append(CsvOutputConfig.FieldSeperator);
                            sb.Append(col.Name);
                        }
                        w.Write(sb.ToString() + CsvOutputConfig.LineEndSeperator + System.Environment.NewLine);

                        using (Reader = InputAgent.GetDataReader(entity, useAdo)) {
                            Reader.Load();

                            while (!Cancelled && Reader.Read()) {
                                object[] data = Reader.GetData();
                                string[] result = new string[Reader.FieldCount];

                                for (int i = 0; i < Reader.FieldCount; i++) {
                                    if (data[i] == null || data[i] is DBNull)
                                    {
                                        result[i] = "NULL";
                                        continue;
                                    }
                                    if (data[i].GetType().Name.ToLower().Contains("Byte[]".ToLower()))
                                    {
                                        try {
                                            result[i] = "\"" + BitConverter.ToString((byte[])data[i]).Replace("-",
                                                                                                           string.Empty) +
                                                    "\"";
                                        } catch (OutOfMemoryException) {
                                            var tmpFile = Application.StartupPath + "\\" + new Guid().ToString() + ".csv";
                                            try {
                                                using (var tempWriter = new StreamWriter(tmpFile, false)) {
                                                    foreach (var b in (byte[])data[i]) {
                                                        tempWriter.Write(BitConverter.ToString(new Byte[] { b }));
                                                        tempWriter.Flush();
                                                    }
                                                    tempWriter.Close();
                                                }
                                                data[i] = null;
                                                GC.Collect();
                                                GC.WaitForPendingFinalizers();

                                                using (var tempReader = new StreamReader(tmpFile)) {
                                                    result[i] = tempReader.ReadToEnd();
                                                }
                                            } catch (Exception ex) {
                                                MessageBox.Show("Error in converting Byte[]-data" + System.Environment.NewLine + ex.Message + System.Environment.NewLine + ex.StackTrace);
                                                throw;
                                            } finally {
                                                File.Delete(tmpFile);
                                            }
                                        }
                                    } else
                                    {
                                        result[i] = "\"" + data[i].ToString().Replace("\"", "\"\"") + "\"";
                                    }
                                }

                                w.Write(string.Join(CsvOutputConfig.FieldSeperator, result));
                                w.Write(CsvOutputConfig.LineEndSeperator + System.Environment.NewLine);

                                datasetCount++;
                                progress.DataSetsProcessed++;
                                w.Flush();
                            }
                            Reader.Close();
                        }
                        w.Flush();
                        w.Close();

                        if (CsvOutputConfig.CompressAfterExport) {
                            CompressFile(new FileInfo(outputFolder + GetCsvFileName(entity.Name)));
                        }

                        logEntity.State = ExportStates.Ok;
                    }
                } else {
                    logEntity.State = ExportStates.NoColumns;
                }
            } catch (Exception ex) {
                logEntity.Error = ex.Message;
                logEntity.State = ExportStates.Error;
                entity.TransferState.State = TransferStates.TransferedError;
                entity.TransferState.Message = ex.Message;
                throw;
            } finally {
                logEntity.CountDest = datasetCount;
                logEntity.Duration = DateTime.Now - progress.StartTime;
                loggingDb.Save(logEntity);

                if (string.IsNullOrEmpty(logEntity.Error)) {
                    if(logEntity.Count != logEntity.CountDest) {
                        entity.TransferState.State = TransferStates.TransferedCountDifference;
                        entity.TransferState.Message = string.Format(
                        "Anzahl der Zeilen weicht ab. Die Quelle hat {0} Einträge, die CSV-Datei hat {1} Einträge.",
                        logEntity.Count, logEntity.CountDest);

                    }
                    else {
                        entity.TransferState.State = TransferStates.TransferedOk;
                        entity.TransferState.Message = "Tabelle wurde korrekt übertragen";
                    }
                }

                if(useAdo) {
                    entity.TransferState.Message += System.Environment.NewLine + "ADO wurde erzwungen";
                }
                entity.Save();
            }
        }

        private void CompressFile(FileInfo fi) {
            try {
                using (var zip = new ZipFile())
                {
                    zip.UseZip64WhenSaving = Zip64Option.Always;
                    zip.AddFile(fi.FullName, string.Empty);
                    zip.Save(fi.FullName + ".zip");
                }
                System.IO.File.Delete(fi.FullName);
            } catch (Exception) {
                //TODO: Do proper handling
            }
        }

        public override void CompleteTransfer() { /* nothing to do */ }

        public override void Cancel() {
            Cancelled = true;
            Reader.Cancel();
            Reader.Close();
        }

        public override void SaveLogFile(FileInfo fi)
        {
            if (!Directory.Exists(CsvOutputConfig.Folder + "\\log\\"))
                Directory.CreateDirectory(CsvOutputConfig.Folder + "\\log\\");
            File.Copy(fi.FullName, CsvOutputConfig.Folder + "\\log\\" + fi.Name, true);
        }

        public override void SaveXMLMetaData(string XMLString)
        {
            using (var writer = new StreamWriter(CsvOutputConfig.Folder + "\\transdata_export_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".xml"))
            {
                writer.Write(XMLString);
                writer.Close();
            }
        }

        public override bool CheckDataAccess() {
            return true;
            // TODO
        }

        public override string GetLogDirectory()
        {
            if (!Directory.Exists(CsvOutputConfig.Folder + "\\log\\"))
                Directory.CreateDirectory(CsvOutputConfig.Folder + "\\log\\");
            return CsvOutputConfig.Folder + "\\log\\";
        }

        public override string GetDescription() {
            return "Csv" + Environment.NewLine + CsvOutputConfig.Folder;
        }
    }
}