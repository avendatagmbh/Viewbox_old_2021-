using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace MappingTemplateUpgrade {
    internal class Program {

        private static List<Assignments> ParseValues(List<string> values) {

            var assigments = new List<Assignments>();
            foreach (var value in values) {
                var assign = new Assignments();
                string tmpStr = value.Replace("\"", "");
                ;

                var parts = tmpStr.Split(';');
                if (parts.Count() < 4) {}

                assign.Number = parts[0];
                assign.Name = parts[1];
                assign.DebitId = parts[2];
                assign.CreditId = parts[3];

                assigments.Add(assign);
            }

            return assigments;
        }

        internal static void Main(string[] args) {

            string inputFile = null;
            Console.WriteLine("eBilanz-Kit - Vorlagen Upgrade Tool");
            Console.WriteLine("-----------------------------------");
            if (args.Length == 0) {
                Console.WriteLine("Syntax: MappingTemplateUpgrade <Dateiname>.csv");
                return;
            }

            inputFile = args[0];
            if (!File.Exists(inputFile)) {
                Console.WriteLine("Die Datei " + inputFile + " existiert nicht.");
                return;
            }

            var values = new List<string>();

            try {
                using (var reader = new StreamReader(Environment.CurrentDirectory + "\\" + inputFile, Encoding.UTF8)) {
                    int counter = 0;
                    while (!reader.EndOfStream) {
                        counter++;
                        string line = reader.ReadLine();
                        if (counter > 1 && line != null) {
                            values.Add(line);

                        }
                    }
                }

                string xmlFileName = inputFile.Substring(0, inputFile.IndexOf(".")) + ".xml";
                var assignments = ParseValues(values);

                using (var writer = new XmlTextWriter(xmlFileName, null)) {
                    writer.Formatting = Formatting.Indented;
                    writer.WriteStartDocument();
                    writer.WriteComment("Konvertierung der CSV-Datei abgeschlossen. Dateiname (XML): " +
                                        Environment.NewLine + xmlFileName);
              
                    writer.WriteStartElement("e_balance_kit_mapping_template");
                    writer.WriteStartElement("taxonomy");
                    writer.WriteString("de-gaap-2010-12-16");
                    writer.WriteEndElement();

                    writer.WriteStartElement("name");
                    writer.WriteString(inputFile.Substring(0, inputFile.IndexOf(".")));
                    writer.WriteEndElement();

                    writer.WriteStartElement("assignments");

                    foreach (var value in assignments) {
                        writer.WriteStartElement("assignment");
                        writer.WriteAttributeString("number", value.Number);
                        writer.WriteAttributeString("name", value.Name);
                        if (!string.IsNullOrEmpty(value.DebitId))
                            writer.WriteAttributeString("debit_element_id", value.DebitId);
                        if (!string.IsNullOrEmpty(value.CreditId))
                            writer.WriteAttributeString("credit_element_id", value.CreditId);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndElement();

                }

                Console.WriteLine("Konvertierung erfolgreich abgeschlossen.");

            }
            catch (Exception ex) {
                Console.WriteLine("Fehler bei der Konvertierung: " + ex.Message);
            }
        }
    }

    internal class Assignments {
        public string Name { get; set; }
        public string Number { get; set; }
        public string DebitId { get; set; }
        public string CreditId { get; set; }

    }
}
