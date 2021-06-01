using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbSearchDatabase.DistinctDb {
    public class DistinctColumn {
        #region Constructor

        public DistinctColumn(DistinctTable table, string name) {
            Table = table;
            Name = name;
        }
        #endregion Constructor

        #region Properties

        public DistinctTable Table { get; set; }
        public string Name { get; set; }
        #endregion Properties

        #region Methods
        #endregion Methods
    }
}
