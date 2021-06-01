using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Config.Interfaces.DbStructure;

namespace Business.CsvImporter.Structures {
    //Collects information about a table which may have been separated into different csv files
    public class CsvTableInfo {
        #region Constructor
        private void Init(string tableName, IEnumerable<string> filenames) {
            TableName = tableName;
            FolderName = new FileInfo(filenames.First()).DirectoryName;
            if (FolderName != null && FolderName[FolderName.Length - 1] != '\\' && FolderName[FolderName.Length - 1] != '/')
                FolderName = FolderName + "\\";
            Filenames = new List<string>();
            foreach(var filename in filenames)
                Filenames.Add(filename);
        }

        internal CsvTableInfo(ITable table) {
            Init(table.Name, table.FileNames);
            Table = table;
        }
 
        #endregion Constructor

        #region Properties
        public string TableName { get; set; }
        public string FolderName { get; set; }
        public List<string> Filenames { get; set; }
        public ITable Table { get; private set; }
        #endregion Properties

        #region Methods
        public string GetPath(string filename) {
            return filename;
        }
        #endregion Methods
    }
}
