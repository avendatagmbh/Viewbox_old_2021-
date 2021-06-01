using System;
using System.Collections.Generic;
using Business.CsvImporter.Events;
using Business.CsvImporter.Structures;
using Config.Config;
using Config.Interfaces.Config;
using DbAccess;
using System.IO;
using DbImport.CsvImport;
using System.Text;

namespace Business.CsvImporter.Specific {
    
    internal class Default : CsvImporterBase, ICsvImporter {

        public Default(StateCsvImport state, CsvInputConfig csvInputConfig, IDatabaseOutputConfig outputConfig)
            : base(state, csvInputConfig, outputConfig) { }

        /// <summary>
        /// Returns the filename of the first csv-part.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        //protected override string GetMainFilename (string fileName) {
        //    return fileName;
        //}

        //protected override void LoadData(IDatabase db, string tableName, string fileName, int ignoreLines) {
        //    string cmd = "";
        //    try
        //    {
        //        string enc = "utf8";
        //        if (!string.IsNullOrEmpty(CsvInputConfig.FileEncoding)) enc = CsvInputConfig.FileEncoding;

        //        cmd =
        //            "LOAD DATA LOCAL INFILE '" + (fileName).Replace("\\", "/") + "'" +
        //            " INTO TABLE " + tableName +
        //            " CHARACTER SET " + enc + 
        //            " COLUMNS TERMINATED BY '" + CsvInputConfig.GetFieldSeperator() + "'" +
        //            " OPTIONALLY ENCLOSED BY '" + "\"" + "'" +
        //            " ESCAPED BY ''" +
        //            " LINES TERMINATED BY '" + CsvInputConfig.GetLineEndSeperator() + "'" +
        //            " IGNORE " + ignoreLines + " LINES";
        //        db.ExecuteNonQuery(cmd);
        //    } catch (Exception ex) {
        //        throw new Exception("Fehler beim Absetzen des Befehls: " + cmd + Environment.NewLine + ex.Message);
        //    }
        //}


        //protected override void CreateTable(IDatabase db, string tableName, string fileName) {
        //    // Get column names and types (+lengths)

        //    try {
        //        IEnumerable<string> arrFieldNames = GetFieldNamesFromCsv(fileName, CsvInputConfig);


        //        // create table
        //        string sCommand = "CREATE TABLE IF NOT EXISTS " + tableName + " (";
        //        foreach (string sField in arrFieldNames) {
        //            sCommand += db.Enquote(sField) + " TEXT,";
        //        }
        //        sCommand = sCommand.Remove(sCommand.Length - 1);
        //        sCommand += ") ENGINE = MyISAM;";

        //        db.ExecuteNonQuery(sCommand);

        //    } catch (Exception ex) {
        //        throw new Exception("Fehler beim Anlegen der Tabelle: " + Environment.NewLine + ex.Message);
        //    }
        //}

        //public event EventHandler<CsvImportStateEventArgs> UpdateState;
    }
}
