// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-04-03
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Globalization;
using System.Windows.Data;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Reconciliation.Converters {
    public class ReconciliationTransactionTypeToTooltipTextConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is IReconciliationTransaction) {
                var transaction = value as IReconciliationTransaction;
                switch (transaction.Reconciliation.ReconciliationType) {
                    case ReconciliationTypes.Reclassification:
                        switch (transaction.TransactionType) {
                            case TransactionTypes.Source:
                                return ResourcesReconciliation.ReconciliationReclassificationSourceTooltip;

                            case TransactionTypes.Destination:
                                return ResourcesReconciliation.ReconciliationReclassificationDestTooltip;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                    case ReconciliationTypes.ValueChange:
                        return ResourcesReconciliation.ReconciliationValueChangeTooltip;

                    case ReconciliationTypes.Delta:
                        return ResourcesReconciliation.ReconciliationDeltaTooltip;

                    case ReconciliationTypes.PreviousYearValues:
                        return ResourcesReconciliation.ReconciliationPreviousYearValuesTooltip;

                    case ReconciliationTypes.ImportedValues:
                        return ResourcesReconciliation.ReconciliationImportedValuesTooltip;

                    case ReconciliationTypes.AuditCorrection:
                        return ResourcesReconciliation.ReconciliationAuditCorrectionTooltip;

                    case ReconciliationTypes.AuditCorrectionPreviousYear:
                        return ResourcesReconciliation.ReconciliationAuditCorrectionPreviousYearTooltip;

                    case ReconciliationTypes.TaxBalanceValue:
                        return ResourcesReconciliation.ReconciliationTaxBalanceValueTooltip;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}