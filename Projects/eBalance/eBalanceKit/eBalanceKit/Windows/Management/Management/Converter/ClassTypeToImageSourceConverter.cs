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

namespace eBalanceKit.Windows.Management.Management.Converter {
    public class ClassTypeToImageSourceConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

            if (value is ReportTreeNode) {
                value = (value as ReportTreeNode).Item;
                if (value is eBalanceKitBusiness.Structures.DbMapping.System) return "/eBalanceKitResources;component/Resources/System48.png";
                if (value is Company) return "/eBalanceKitResources;component/Resources/Company48.png";
                if (value is FinancialYear) return "/eBalanceKitResources;component/Resources/FinancialYears48.png";
                if (value is Document) return "/eBalanceKitResources;component/Resources/ReportManagement48.png";
            } else {
                if (value is eBalanceKitBusiness.Structures.DbMapping.System)
                    return "/eBalanceKitResources;component/Resources/SystemManagement48.png";
                if (value is Company) return "/eBalanceKitResources;component/Resources/CompanyManagement48.png";
                if (value is Document) return "/eBalanceKitResources;component/Resources/ReportManagement48.png";
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }
}