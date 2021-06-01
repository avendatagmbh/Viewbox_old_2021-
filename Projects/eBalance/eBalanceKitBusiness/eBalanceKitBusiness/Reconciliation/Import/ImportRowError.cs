using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBalanceKitBusiness.Reconciliation.Import {
    public class ImportRowError {
        #region Properties
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string Description { get; set; }
        public string FileName { 
            get {
                if (FilePath == null) return null;
                return System.IO.Path.GetFileName(FilePath);
            } 
        }
        public string ReconciliationName { get; set; }
        #endregion Properties
    }
}
