using System.Collections.Generic;
using System.ComponentModel;
using eBalanceKitBusiness.Structures.XbrlElementValue;

namespace eBalanceKitBusiness.Reconciliation.Interfaces {
    public interface IReconciliationInfo {
        
        ComputedValue ComputedValueTransfer { get; set; }
        ComputedValue ComputedValueTransferPreviousYear { get; set; }
        
        XbrlElementValueBase XbrlElementValue { get; }

        decimal? TransferValue { get; }
        string TransferValueDisplayString { get; }

        decimal? TransferValueInputPreviousYear { get; set; }
        decimal? TransferValuePreviousYear { get; }
        string TransferValuePreviousYearDisplayString { get; }

        bool HasTransferValue { get; }

        decimal? HBValue { get; }
        string HBValueDisplayString { get; }
        
        decimal? STValue { get; }
        string STValueDisplayString { get; }
        
        bool HasComputedValue { get; }
        string GetDisplayString(decimal? value);

        IEnumerable<IReconciliationTransaction> Transactions { get; }

        bool HasSelectedTransaction { get; }
        bool IsAssignedToSelectedReconciliation { get; }

        void RefreshValues();
    }
}