using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitTestExample {

    /* Golden rules for unit tests
     * 1. A unit test should test exactly one function of a class
     * 2. Structure of a unit test: AAA, arrange, act, assert
     * 3. If possible, define interface of a class upfront, write unit tests (which of course fail as no code is written), 
     *      then implement until all tests are satisfied
    */
    class Program {
        static void Main(string[] args) {
            ExportLog log = new ExportLog();
            log.ReadFile("export.log");


            const string tableName = "allazymt";
            var logEntry = log.GetEntry(tableName);
            if(logEntry == null)
                Console.WriteLine(string.Format("Could not find table {0}",tableName));
            else {
                Console.WriteLine(string.Format("Table {0} has count before of {1} and count after of {2}", tableName, logEntry.CountBefore, logEntry.CountAfter));
            }
            Console.ReadKey();
        }
    }
}
