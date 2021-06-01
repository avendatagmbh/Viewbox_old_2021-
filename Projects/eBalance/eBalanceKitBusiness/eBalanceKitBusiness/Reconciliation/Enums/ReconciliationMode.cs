using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBalanceKitBusiness.Reconciliation.Enums {
    public enum ReconciliationMode {

        /// <summary>
        /// Difference between trade balance and fiscal balance
        /// Differenzwerte zwischen Handelsbilanz und Steuerbilanz
        /// </summary>
        General = 0,

        /// <summary>
        /// Tax balance sheet values
        /// Steuerbilanzwerte
        /// </summary>
        TaxBalanceSheetValues = 1,
    }
}
