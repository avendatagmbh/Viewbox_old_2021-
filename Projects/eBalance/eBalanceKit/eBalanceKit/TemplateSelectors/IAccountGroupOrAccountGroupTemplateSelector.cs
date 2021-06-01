// --------------------------------------------------------------------------------
// author: Márton Garai
// since: 2012-08-09
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using eBalanceKitBusiness.MappingTemplate;

namespace eBalanceKit.TemplateSelectors
{
    internal class IAccountGroupOrAccountGroupTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container) { 
            TreeViewItem element = container as TreeViewItem;
            if (element != null) {
                AccountGroupInfo  accountGroupInfo = element.Header as AccountGroupInfo;
                if (accountGroupInfo != null) {
                    return element.FindResource("AccountGroupDataTemplate") as DataTemplate;
                }
                AccountGroupChildInfo accountGroup = element.Header as AccountGroupChildInfo ;
                if (accountGroup != null) {
                    return element.FindResource("BalanceListAccountDataTemplate") as DataTemplate;
                }
            }
            ContentPresenter presenter = container as ContentPresenter;
            if (presenter != null) {
                AccountGroupInfo  accountGroupInfo = presenter.Content as AccountGroupInfo;
                if (accountGroupInfo != null) {
                    return presenter.FindResource("AccountGroupDataTemplate") as DataTemplate;
                }
                AccountGroupChildInfo accountGroup = presenter.Content as AccountGroupChildInfo ;
                if (accountGroup != null) {
                    return presenter.FindResource("BalanceListAccountDataTemplate") as DataTemplate;
                }
            }
            return null;
        }
    }
}
