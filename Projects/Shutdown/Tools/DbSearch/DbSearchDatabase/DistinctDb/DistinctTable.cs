using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbSearchDatabase.DistinctDb {
    public class DistinctTable {
        #region Constructor

        public DistinctTable(string table, int id = 0) {
            Name = table;
            Id = id;
        }
        #endregion Constructor

        #region Properties
        public int Id { get; set; }
        public string Name { get; set; }
        #endregion Properties

        #region Methods
        public override bool Equals(object obj) {
            if (obj == null) return false;
            DistinctTable other = obj as DistinctTable;
            if (other == null) return false;
            return other.Name.ToLower() == Name.ToLower();
        }

        public override int GetHashCode() {
            return Name.ToLower().GetHashCode();
        }
        #endregion Methods
    }
}
