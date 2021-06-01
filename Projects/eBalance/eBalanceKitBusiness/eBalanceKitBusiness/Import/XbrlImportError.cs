using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBalanceKitBusiness.Import
{
    class XbrlImportError
    {
        #region Properties
        public string FilePath { get; set; }
        public string MethodName { get; set; }
        public string ErrorDescription { get; set; }
        public string FileName {
            get {
                if (FilePath == null) return null;
                return System.IO.Path.GetFileName(FilePath);
            }
        }
        public string Position { get; set; }
        #endregion Properties
    }
}
