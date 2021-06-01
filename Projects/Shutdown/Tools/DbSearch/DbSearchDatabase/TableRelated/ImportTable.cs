using System.IO;

namespace DbSearchDatabase.TableRelated {
    public class ImportTable {
        public ImportTable(string name, string validationPath) {
            Name = name;
            Use = true;
            ValidationPath = validationPath;
        }

        public string ValidationPath { get; set; }
        public string ValidationFilename { get { return new FileInfo(ValidationPath).Name; } }
        public string Name { get; set; }
        public bool Use { get; set; }
    }
}
