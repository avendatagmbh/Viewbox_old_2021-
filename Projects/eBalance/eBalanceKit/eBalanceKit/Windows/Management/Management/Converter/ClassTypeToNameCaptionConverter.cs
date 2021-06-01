// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-27
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Globalization;
using System.Windows.Data;
using eBalanceKit.Windows.Management.Management.Models;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Management.Management.Converter {
    public class ClassTypeToNameCaptionConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

            if (value is ReportTreeNode) {
                value = (value as ReportTreeNode).Item;
                if (value is eBalanceKitBusiness.Structures.DbMapping.System) return ResourcesCommon.Name;
                if (value is Company) return ResourcesCommon.Name;
                if (value is FinancialYear) return ResourcesMain.FinancialYear;
                if (value is Document) return ResourcesCommon.Name;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }
}