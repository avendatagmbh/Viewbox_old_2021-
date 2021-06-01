using System.ComponentModel;
using eBalanceKitBusiness.Interfaces.PresentationTree;
using eBalanceKitBusiness.Reconciliation;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures.DbMapping;
using System.Collections.Generic;
using eBalanceKitBusiness.Structures.ValueTree;
using eBalanceKitBusiness.Structures.XbrlElementValue;

namespace eBalanceKitBusiness {

    /// <summary>
    /// Interface for all xbrl value classes.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2010-11-04</since>
    public interface IValueTreeEntry : INotifyPropertyChanged {

        IValueMapping DbValue { get; }
        ReconciliationInfo ReconciliationInfo { get; }

        object Value { get; set; }
        string DisplayString { get; }
        ComputedValue MonetaryValue { get; }
        bool HasValue { get; }
        bool IsNumeric { get; }
        decimal DecimalValue { get; }
        Taxonomy.IElement Element { get; }

        bool AutoComputeAllowed { get; }
        bool AutoComputeEnabled { get; set; }
        bool SupressWarningMessages { get; set; }
        bool SendAccountBalances { get; set; }
        bool SendAccountBalancesRecursive { get; set; }
        void SetSendAccountBalancesRecursive(bool setValue);

        bool ValidationWarning { get; set; }
        string ValidationWarningMessage { get; set; }

        bool ValidationError { get; set; }
        string ValidationErrorMessage { get; set; }

        bool IsReportable { get; set; }

        bool HasComputedValue { get; }
        bool HasManualValue { get; }
        bool IsEnabled { get; }

        void AddSummationTarget(IValueTreeEntry Value, decimal Weight);
        void AddSummationSource(IValueTreeEntry value);

        void AddUserSummationTarget(IValueTreeEntry Value, decimal Weight);
        void AddUserSummationSource(IValueTreeEntry value);

        void UpdateComputedValue(IPresentationTreeNode pTreeNode = null, bool sumAll = false);
        void SetFlags(bool autoComputeEnabled, bool supressWarningMessages, bool sendAccountBalances);

        bool HasSummationSource(string id);

        bool IsComputationOrphanedNode { get; }
        bool HasMonetaryParent { get; }

        ValueTreeNode Parent { get; }

        bool IsValueVisible { get; }
        bool IsEditAllowed { get; }
        bool IsTransferValueVisible { get; }
        bool IsEditTransferValueAllowed { get; }

        ReportRights ReportRights { get; }
        bool IsCompanyEditAllowed { get; }

        void UserOptionChanged();
    }
}
