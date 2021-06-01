using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbConsolidateBusiness {
    internal class Entry {
        public string SourceDatabase { get; set; }
        public string OriginalName { get; set; }
        public string Name { get; set; }
        public long CountOriginal { get; set; }
        public long CountDestinationTable { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Exception { get; set; }
        public string Warning { get; set; }

        public void Read(string line) {
            var entries = line.Split(';');
            if (entries.Length != 9) {
                throw new Exception("Zeile ist ungültig: " + line);
            }
            SourceDatabase = entries[0];
            OriginalName = entries[1];
            Name = entries[2];
            CountOriginal = Convert.ToInt64(entries[3]);
            CountDestinationTable = Convert.ToInt64(entries[4]);
            Start = Convert.ToDateTime(entries[5]);
            End = Convert.ToDateTime(entries[6]);
            Exception = entries[7];
            Warning = entries[8];
        }

        public string ToCsvLine() {
            return SourceDatabase + ";" + OriginalName + ";" + Name + ";" + CountOriginal + ";" + CountDestinationTable + ";" + Start.ToString() + ";" + End.ToString() + ";" + Exception + ";" + Warning;
        }
    }
}
