using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DocumentationGeneratorBusiness;

namespace DocumentationGeneratorConsole {
    class Program {
        static void Main(string[] args) { 
            var test = new Test();
            test.Init();

            test.SystemDb.SystemDbInitialized += new EventHandler(SystemDb_SystemDbInitialized);

            Console.WriteLine(test.NumberOfTablesInScheme()); //gets number of tables from viewbox-db via dbaccess 
        }

        static void SystemDb_SystemDbInitialized(object sender, EventArgs e) {
            Console.WriteLine((sender as SystemDb.SystemDb).Objects.Count); //gets number of tables, that are listed in viewbox-db tables
        }
    }
}
