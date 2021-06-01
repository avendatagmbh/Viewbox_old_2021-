using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess.Structures;

namespace DbSearch.Models.Profile {
    public class DatabaseModel {
        public DatabaseModel(DbConfig dbConfig) {
            DbConfig = dbConfig;
            IsReadOnly = false;
        }

        #region Properties
        public DbConfig DbConfig { get; set; }
        public bool IsReadOnly { get; set; }

        #endregion Properties
    }
}
