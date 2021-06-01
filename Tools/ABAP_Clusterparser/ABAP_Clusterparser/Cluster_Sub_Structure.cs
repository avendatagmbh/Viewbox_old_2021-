using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABAP_Clusterparser {
    public class Cluster_Sub_Structure {
        public Cluster_Sub_Structure() { Occurs = -1; }
        public string Name { get; set; }
        public string Table { get; set; }
        public string Addition { get; set; }
        public int Occurs { get; set; }
        public string Header { get; set; }
    }
}
