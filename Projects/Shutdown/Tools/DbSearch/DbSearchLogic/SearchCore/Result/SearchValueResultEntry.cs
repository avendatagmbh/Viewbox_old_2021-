using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbSearchLogic.SearchCore.Result {
    public class SearchValueResultEntry {

        #region Properties
        public uint Id { get; set; }
        public SearchValueResultEntryType Type { get; set; }
        #endregion Properties

        #region Constructor
        public SearchValueResultEntry(uint id, SearchValueResultEntryType type = SearchValueResultEntryType.Exact) {
            Id = id;
            Type = type;
        }
        #endregion
    }
}
