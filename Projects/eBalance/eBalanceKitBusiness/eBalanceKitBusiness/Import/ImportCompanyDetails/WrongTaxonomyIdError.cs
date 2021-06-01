using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBalanceKitBusiness.Import.ImportCompanyDetails {
    public class WrongTaxonomyIdError {
        #region Properties
        public string FilePath { get; set; }
        public string TaxonomyName { get; set; }
        public string TaxonomyId { get; set; }
        public int LineNumber { get; set; }
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
