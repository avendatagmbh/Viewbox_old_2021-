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
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Reconciliation.Converters {
    public class ReconciliationTransactionTypeToImageSourceConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is IReconciliationTransaction) {
                var transaction = value as IReconciliationTransaction;
                switch (transaction.Reconciliation.ReconciliationType) {
                    case ReconciliationTypes.Reclassification:
                        switch (transaction.TransactionType) {
                            case TransactionTypes.Source:
                                return "/eBalanceKitResources;component/Resources/ReconciliationReclassificationSource.png";

                            case TransactionTypes.Destination:
                                return "/eBalanceKitResources;component/Resources/ReconciliationReclassificationDest.png";

                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                    case ReconciliationTypes.ValueChange:
                        return "/eBalanceKitResources;component/Resources/ReconciliationValueChange.png";

                    case ReconciliationTypes.Delta:
                        return "/eBalanceKitResources;component/Resources/ReconciliationDelta.png";

                    case ReconciliationTypes.PreviousYearValues:
                        return "/eBalanceKitResources;component/Resources/ReconciliationPreviousYear.png";

                    case ReconciliationTypes.ImportedValues:
                        return "/eBalanceKitResources;component/Resources/ReconciliationImport.png";

                    case ReconciliationTypes.AuditCorrection:
                        return "/eBalanceKitResources;component/Resources/AuditCorrection24.png";

                    case ReconciliationTypes.AuditCorrectionPreviousYear:
                        return "/eBalanceKitResources;component/Resources/AuditCorrection24.png";

                    case ReconciliationTypes.TaxBalanceValue:
                        return "/eBalanceKitResources;component/Resources/ReconciliationDelta.png";

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