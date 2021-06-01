// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-27
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Globalization;
using System.Windows.Data;
using eBalanceKit.Windows.Management.Delete.Models;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKit.Windows.Management.Delete.Converter {
    public class ClassTypeToImageSourceConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is CheckableItemWrapper<eBalanceKitBusiness.Structures.DbMapping.System>) return "/eBalanceKitResources;component/Resources/DeleteSystem48.png";
            if (value is CheckableItemWrapper<Company>) return "/eBalanceKitResources;component/Resources/DeleteCompany48.png";
            if (value is CheckableItemWrapper<Document>) return "/eBalanceKitResources;component/Resources/DeleteReport48.png";
            if (value is CheckableItemWrapper<IReconciliation>) {
                var recon = ((CheckableItemWrapper<IReconciliation>) value).Item;
                switch(recon.ReconciliationType) {
                    //case ReconciliationTypes.PreviousYearValues:
                    //    return "/eBalanceKitResources;component/Resources/ReconciliationPreviousYear.png";
                    //case ReconciliationTypes.ImportedValues:
                    //    break;
                    case ReconciliationTypes.Reclassification:
                        return "/eBalanceKitResources;component/Resources/ReconciliationReclassification.png";
                    case ReconciliationTypes.ValueChange:
                        return "/eBalanceKitResources;component/Resources/ReconciliationValueChange.png";
                    case ReconciliationTypes.Delta:
                        return "/eBalanceKitResources;component/Resources/ReconciliationDelta.png";
                    case ReconciliationTypes.TaxBalanceValue:
                        return "/eBalanceKitResources;component/Resources/ReconciliationDelta.png";
                    //case ReconciliationTypes.AuditCorrection:
                    //    break;
                    //case ReconciliationTypes.AuditCorrectionPreviousYear:
                    //    break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }
}