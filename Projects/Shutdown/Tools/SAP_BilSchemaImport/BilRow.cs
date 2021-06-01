using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SAP_BilSchemaImport {
    
    public class BilRow {
        public int Id { get; set; }
        public int Parent { get; set; }
        public string ShortName { get; set; }
        public string Name { get; set; }
        public string Account { get; set; }
        public bool Debit { get; set; }
        public bool Credit { get; set; }
        public int Level { get; set; }
        public string AccountFrom { get; set; }
        public string AccountTo { get; set; }
        public string Type { get; set; }
        public int HighestGroupId { get; set; }
        public string AdditionalInformation { get; set; }
        public string AccountStructure { get; set; }
    }
}
