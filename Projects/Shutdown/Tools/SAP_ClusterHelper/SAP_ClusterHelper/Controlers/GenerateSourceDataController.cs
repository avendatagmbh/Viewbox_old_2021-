// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-10
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using DbAccess;
using DbAccess.Structures;
using SAP_ClusterHelper.Models;
using SAP_ClusterHelper.Windows;

namespace SAP_ClusterHelper.Controlers {
    public static class GenerateSourceDataController {

        public static void GenerateSourceData(GenerateSourceDataModel config, ProgressInfo progressInfo) {
            if (!Directory.Exists(config.ScriptFolder)) Directory.CreateDirectory(config.ScriptFolder);
            if (!Directory.Exists(config.DataFolder)) Directory.CreateDirectory(config.DataFolder);

            var dbConfig = new DbConfig("MySQL") {
                Hostname = config.DbHostname,
                Username = config.DbUser,
                Password = config.DbPassword,
                DbName = config.DbDatabase
            };

            using (var conn = DbAccess.ConnectionManager.CreateConnection(dbConfig)) {
                conn.Open();

                if (config.GenerateSTXL) GenerateSTXLData(conn, config, progressInfo);
                if (config.GeneratePCL1) GeneratePCL1Data(conn, config, progressInfo);
                if (config.GeneratePCL2) GeneratePCL2Data(conn, config, progressInfo);
                if (config.GeneratePCL3) GeneratePCL3Data(conn, config, progressInfo);
                if (config.GeneratePCL4) GeneratePCL4Data(conn, config, progressInfo);
            }
        }

        private static void GenerateSTXLData(IDatabase conn, GenerateSourceDataModel config, ProgressInfo progressInfo) {

            try {
                const string tablename = "stxl";
                string baseFolder = config.DataFolder + "\\" + tablename + "\\";

                var columnString = string.Join(",", new[] {
                    "MANDT", // 0
                    "RELID", // 1
                    "TDOBJECT", // 2
                    "TDNAME", // 3
                    "TDID", // 4
                    "TDSPRAS", // 5
                    "SRTF2", // 6
                    "CLUSTR", // 7
                    "CLUSTD" // 8
                });

                Dictionary<string, BinaryWriter> writers = new Dictionary<string, BinaryWriter>();
                Dictionary<string, StreamWriter> writersAsc = new Dictionary<string, StreamWriter>();
                Dictionary<string, long> size = new Dictionary<string, long>();
                Dictionary<string, int> partNumbers = new Dictionary<string, int>();

                progressInfo.Caption = "exporting data for table STXL";
                progressInfo.Maximum = conn.CountTable(tablename);
                progressInfo.Value = 0;
                using (var reader = conn.ExecuteReader("SELECT " + columnString + " FROM " + tablename)) {
                    while (reader.Read()) {
                        var key = reader.GetString(1);

                        // create new writer
                        if (!writers.ContainsKey(key)) {
                            if (!Directory.Exists(config.DataFolder + "\\" + tablename))
                                Directory.CreateDirectory(config.DataFolder + "\\" + tablename);

                            writers[key] = new BinaryWriter(new FileStream(baseFolder + key + ".csv", FileMode.Create));
                            writersAsc[key] =
                                new StreamWriter(new FileStream(baseFolder + key + "_asc.csv", FileMode.Create));

                            size[key] = 0;
                            partNumbers[key] = 0;
                        }

                        // create writer for next part if part > 500 Mb
                        if (size[key] > 1024 * 1024 * 512 && reader["SRTF2"].ToString() == "0") {
                            writers[key].Close();
                            writersAsc[key].Close();

                            partNumbers[key]++;
                            size[key] = 0;

                            writers[key] =
                                new BinaryWriter(new FileStream(baseFolder + key + partNumbers[key] + ".csv",
                                                                FileMode.Create));

                            writersAsc[key] =
                                new StreamWriter(new FileStream(baseFolder + key + partNumbers[key] + "_asc.csv",
                                                                FileMode.Create));
                        }

                        // write data
                        var tmp = StringToByteArray(reader["CLUSTD"].ToString());
                        size[key] += tmp.Length;
                        writers[key].Write(tmp);

                        writersAsc[key].WriteLine(
                            reader["MANDT"] + ";" +
                            reader["TDOBJECT"] + ";" +
                            reader["TDNAME"] + ";" +
                            reader["TDID"] + ";" +
                            reader["TDSPRAS"] + ";" +
                            reader["CLUSTR"] + ";" +
                            reader["SRTF2"]);
                        progressInfo.Value += 1;
                        progressInfo.Caption = "exporting data for table STXL (" + progressInfo.Value.ToString("0,0") +
                                               " / " + progressInfo.Maximum.ToString("0,0") + ")";
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show("Data generation failed: " + ex.Message);
            }
        }

        private static void GeneratePCL1Data(IDatabase conn, GenerateSourceDataModel config, ProgressInfo progressInfo) {
            // TODO
        }

        private static void GeneratePCL2Data(IDatabase conn, GenerateSourceDataModel config, ProgressInfo progressInfo) {
            // TODO
        }

        private static void GeneratePCL3Data(IDatabase conn, GenerateSourceDataModel config, ProgressInfo progressInfo) {
            // TODO
        }

        private static void GeneratePCL4Data(IDatabase conn, GenerateSourceDataModel config, ProgressInfo progressInfo) {
            // TODO
        }

        private static byte[] StringToByteArray(string hex) {
            return Enumerable.Range(0, hex.Length)
                .Where(x => 0 == x%2)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }
    }
}