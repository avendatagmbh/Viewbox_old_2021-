// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-01-24
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;

namespace SchemaAnalyzerLib {
    internal class KeyInfo {
        #region Columns
        private readonly List<RelevantColumnInfo> _columns = new List<RelevantColumnInfo>();
        public List<RelevantColumnInfo> Columns { get { return _columns; } }
        #endregion
    }
}