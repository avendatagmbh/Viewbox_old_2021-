using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess.Structures;

namespace DbConsolidate.Models {
    public class MainWindowModel {
        public MainWindowModel() {
            DatabaseModel = new DatabaseModel(new DbConfig("MySQL"));
        }

        public DatabaseModel DatabaseModel { get; set; }
    }
}
