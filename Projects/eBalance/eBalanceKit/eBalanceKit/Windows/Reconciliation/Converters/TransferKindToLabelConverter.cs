// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-04-05
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Globalization;
using System.Windows.Data;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Reconciliation.Converters {
    public class TransferKindToLabelConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is TransferKinds) {
                switch ((TransferKinds)value) {
                    case TransferKinds.Reclassification:
                        return ResourcesReconciliation.TransferKindReclassification;

                    case TransferKinds.ValueChange:
                        return ResourcesReconciliation.TransferKindChangeVlaue;
                        
                    case TransferKinds.ReclassificationWithValueChange:
                        return ResourcesReconciliation.TransferKindReclassificationChangeValue;

                    default:
                        throw new ArgumentOutOfRangeException("value");
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { throw new NotImplementedException(); }
    }
}