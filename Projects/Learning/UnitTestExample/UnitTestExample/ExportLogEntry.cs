using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitTestExample {
    class ExportLogEntry {
        #region Properties
        public string TableName { get; set; }
        public long CountBefore { get; set; }
        public long CountAfter { get; set; }
        #endregion Properties

        #region Methods

        //Structure is: USER;TableName;FileName;Filter;DateBefore;TimeBefore;CountBeforeStr;DateAfter;TimeAfter;CountAfterStr;Duration
        //Example:      BEH; allazymt ;allazymt;      ;20120113  ;174549    ;62            ;20120113 ;174850   ;62           ;181
        public static ExportLogEntry ReadFromLine(string line) {
            ExportLogEntry result = new ExportLogEntry();
            string[] fields = line.Split(';');
            if (fields.Length != 11) throw new InvalidOperationException("Folgende Zeile im Exportlog ist ungültig, da sie nicht 11 Einträge besitzt: " + Environment.NewLine + line);
            result.TableName = fields[1];
            long countBefore, countAfter;
            if (!Int64.TryParse(fields[6], out countBefore)) throw new InvalidOperationException("Countbefore ist keine Zahl in der Zeile " + Environment.NewLine + line);
            result.CountBefore = countBefore;

            if (!Int64.TryParse(fields[9], out countAfter))
                throw new InvalidOperationException("Countafter ist keine Zahl in der Zeile " + Environment.NewLine + line);
            result.CountAfter = countAfter;

            return result;
        }
        #endregion Methods
    }
}
