using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.Collections;

namespace DocumentationGeneratorBusiness {
    public class Test {
        public SystemDb.SystemDb SystemDb;

        //example, how to connect to viewbox-db and get alle data as Objects
        public void Init() {
            SystemDb = new SystemDb.SystemDb();
            SystemDb.Connect("MySQL", "server=viewbox;User Id=root;password=avendata;database=viewbox;Connect Timeout=1000;Default Command Timeout=0;Allow Zero Datetime=True", 20);
        }

        //example, how to use DBAccess
        public long NumberOfTablesInScheme() {
            using(var db = SystemDb.ConnectionManager.GetConnection()) {
                return db.GetTableList().Count;
            }
        }

        
    }
}
