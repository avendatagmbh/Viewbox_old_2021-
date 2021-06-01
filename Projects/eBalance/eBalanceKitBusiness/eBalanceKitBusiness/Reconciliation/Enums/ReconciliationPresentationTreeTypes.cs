using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBalanceKitBusiness.Reconciliation.Enums {
    public enum ReconciliationPresentationTreeTypes {

        /// <summary>
        /// Assets tab in Steuerliche Modifikationen / Überleitung Handelsbilanzposten auf Steuerbilanzposten
        /// same as public IReconciliationTree PresentationTreeBalanceSheetTotalAssets { get; private set; } in ReconciliationModel.cs
        /// </summary>
        BalanceSheetTotalAssets,

        /// <summary>
        /// Liabilites tab in Steuerliche Modifikationen / Überleitung Handelsbilanzposten auf Steuerbilanzposten
        /// same as public IReconciliationTree PresentationTreeBalanceSheetLiabilities { get; private set; } in ReconciliationModel.cs
        /// </summary>
        BalanceSheetLiabilities,

        /// <summary>
        /// Income Statement tab in Steuerliche Modifikationen / Überleitung Handelsbilanzposten auf Steuerbilanzposten
        /// same as public IReconciliationTree PresentationTreeIncomeStatement { get; private set; } in ReconciliationModel.cs
        /// </summary>
        IncomeStatement
    }
}
