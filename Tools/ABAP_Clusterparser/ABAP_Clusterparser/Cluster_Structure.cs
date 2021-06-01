using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABAP_Clusterparser {
    public class Cluster_Structure {
        public Cluster_Structure() {
            SubStructures = new List<Cluster_Sub_Structure>();
        }
        public string Name { get; set; }
        public List<Cluster_Sub_Structure> SubStructures { get; set; }
        public string Addition { get; set; }
    }
}
