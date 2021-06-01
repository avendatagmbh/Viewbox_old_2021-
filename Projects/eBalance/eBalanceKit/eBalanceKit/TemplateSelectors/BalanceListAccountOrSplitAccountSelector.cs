// --------------------------------------------------------------------------------
// author: Márton Garai
// since: 2012-08-06
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows.Controls;
using System.Windows;
using eBalanceKitBusiness.Interfaces.BalanceList;
using eBalanceKitBusiness.MappingTemplate;

namespace eBalanceKit.TemplateSelectors {
    internal class BalanceListAccountOrSplitAccountTemplateSelector : DataTemplateSelector {
        public override DataTemplate SelectTemplate(object item, DependencyObject container) { 
            ContentPresenter element = container as ContentPresenter;
            if (element != null) {
                SplitAccountGroupInfo splitAccountGroupInfo = element.Content as SplitAccountGroupInfo;
                if (splitAccountGroupInfo != null) {
                    return element.FindResource("SplitAccountGroupInfoDataTemplate") as DataTemplate;
                }
                IAccount accountElement = element.Content as IAccount;
                if (accountElement != null) {
                    return element.FindResource("BalanceListAccount") as DataTemplate;
                }
            }
            return null;
        }
    }
}
