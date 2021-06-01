using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewBuilder.Structures
{
    public class ExtendedColumnInformationCsvItem
    {
        public string SourceTable { get; set; }
        public string SourceColumn { get; set; }
        public string TargetTable { get; set; }
        public string TargetColumn { get; set; }
        public string Information { get; set; }
        public string Information2 { get; set; }
        public string Type { get; set; }

        public override string ToString()
        {
            return SourceTable + ";" +
                   SourceColumn + ";" +
                   TargetTable + ";" +
                   TargetColumn + ";" +
                   Information + ";" +
                   Information2 + ";" + 
                   Type + ";";
        }
    }
}
