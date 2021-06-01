// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-04-24
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using Taxonomy;

namespace eBalanceKitBusiness.Reconciliation.Import {
    /// <summary>
    /// Model for previous year values import.
    /// </summary>
    public class PreviousYearValues {
               
        #region Values
        private readonly List<PreviousYearValue> _values = new List<PreviousYearValue>();
        public IEnumerable<PreviousYearValue> Values { get { return _values; } }
        #endregion // Values     

        public IEnumerable<PreviousYearValue> ValuesAssets { get { return _values.Where(t => t.Element.IsBalanceSheetAssetsPosition); } }
        public IEnumerable<PreviousYearValue> ValuesLiabilities { get { return _values.Where(t => t.Element.IsBalanceSheetLiabilitiesPosition); } }
        public IEnumerable<PreviousYearValue> ValuesIncomeStatement { get { return _values.Where(t => t.Element.IsIncomeStatementPosition); } }

        public bool HasTransactions { get { return _values.Count > 0; } }

        internal  void AddValue(decimal value, IElement element) { _values.Add(new PreviousYearValue(value, element)); }
    }
}