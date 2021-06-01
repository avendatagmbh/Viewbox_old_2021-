using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbSearchLogic.Structures.TableRelated {
    public class ColumnInfo {
        public ColumnInfo(string name) {
            Name = name;
        }

        #region Properties
        public string Name { get; set; }
        public string Comment { get; set; }
        #endregion

        public override string ToString() {
            return Name + "(" + Comment + ")";
        }
    }
}
