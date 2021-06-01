using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewValidatorLogic.Structures.InitialSetup;
using DbAccess.Structures;
using ViewValidatorLogic.Logic;

namespace ViewValidatorConsole {
    class Program {
        static void Main(string[] args) {
            ValidationSetup setup = new ValidationSetup();

            setup.DbConfigValidation = new DbConfig("Access") { Hostname = "Q:\\Großprojekte\\Josef Möbius Bau-AG\\05 Verprobungsdaten\\FiBu\\Verprobung.mdb" };
            setup.DbConfigView = new DbConfig("MySQL") { Password = "avendata", Username = "root", Port = 3306, Hostname = "localhost", DbName = "fibu_1000" };
            TableMapping tableMapping = new TableMapping();
            tableMapping.TableValidation = new Table("BilanzenDruckAbgrenzungen");
            tableMapping.TableView = new Table("bilsta_abgrenzungen_view");

            tableMapping.ColumnMappings.Add(new ColumnMapping("KONTO", "Konto"));
            tableMapping.ColumnMappings.Add(new ColumnMapping("BEZEICHNUNG", "Konto_Bez"));
            setup.TableMappings.Add(tableMapping);

            ViewValidator validator = new ViewValidator(setup);

            try {
                validator.StartValidation();

                foreach (var result in validator.Results) {
                    Console.WriteLine("Rows equal: " + result.RowsEqual);
                    Console.WriteLine("Row Differences: " + result.RowDifferenceCount);
                    Console.WriteLine("Row Missing in Validation: " + result.ResultPerTable[0].MissingRowsCount);
                    Console.WriteLine("Row Missing in View: " + result.ResultPerTable[1].MissingRowsCount);

                    for(int i = 0; i < 2; ++i){
                        Console.WriteLine("---------------------");
                        Console.WriteLine("Missing Rows in " + i);
                        foreach (var missing in result.ResultPerTable[i].MissingRows)
                            Console.WriteLine(missing);
                        Console.WriteLine("---------------------");
                    }

                    Console.WriteLine("---------------------");
                    Console.WriteLine("Row differences");
                    foreach (var rowDifference in result.RowDifferences) {
                        Console.WriteLine(rowDifference.Rows[0] + " - " + rowDifference.Rows[1]);
                    }
                }
                Console.ReadKey();
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }

        }
    }
}
