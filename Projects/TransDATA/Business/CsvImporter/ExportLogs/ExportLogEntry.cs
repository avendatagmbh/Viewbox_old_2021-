using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.CsvImporter.ExportLogs {
    public class ExportLogEntry {
        #region Constructor
        public ExportLogEntry() {
        }
        #endregion Constructor

        #region Properties
        public string TableName { get; set; }
        public int CountBefore { get; set; }
        public int CountAfter { get; set; }
        #endregion Properties

        #region Methods
        #endregion Methods

        public void ReadFromLine(string line) {
            string[] fields = line.Split(';');
            if(fields.Length != 11) throw new InvalidOperationException("Folgende Zeile im Exportlog ist ungültig, da sie nicht 11 Einträge besitzt: " + Environment.NewLine + line);
            TableName = fields[1];
            int countBefore, countAfter;
            if(!Int32.TryParse(fields[6], out countBefore)) throw new InvalidOperationException("Countbefore ist keine Zahl in der Zeile " + Environment.NewLine + line);
            CountBefore = countBefore;

            if (!Int32.TryParse(fields[9], out countAfter))
                throw new InvalidOperationException("Countafter ist keine Zahl in der Zeile " + Environment.NewLine + line);
            CountAfter = countAfter;
        }
    }
}
