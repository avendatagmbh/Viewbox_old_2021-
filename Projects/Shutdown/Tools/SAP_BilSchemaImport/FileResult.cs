using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SAP_BilSchemaImport {
    public class FileResult {
        #region Constructor
        public FileResult(string path) {
            Filename = path;
            AccountStructures = new HashSet<string>();
        }
        #endregion Constructor

        #region Properties

        public string Filename { get; set; }
        public HashSet<string> AccountStructures { get; set; }
        public string Error { get; set; }
        #endregion Properties

        #region Methods
        #endregion Methods
    }
}
