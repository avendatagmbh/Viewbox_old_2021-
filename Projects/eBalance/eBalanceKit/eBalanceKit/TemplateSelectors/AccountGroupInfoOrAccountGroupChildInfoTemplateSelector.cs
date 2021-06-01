// --------------------------------------------------------------------------------
// author: Márton Garai
// since: 2012-09-18
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Windows;
using System.Windows.Controls;
using eBalanceKitBusiness.MappingTemplate;

namespace eBalanceKit.TemplateSelectors
{
    class AccountGroupInfoOrAccountGroupChildInfoTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if (container is TreeViewItem) {
                var c = container as TreeViewItem;
                if (c.Header is AccountGroupInfo) 
                    return c.FindResource("AccountGroupDataTemplate") as DataTemplate;
                if (c.Header is AccountGroupChildInfo)
                    return c.FindResource("BalanceListAccountDataTemplate") as DataTemplate;
                throw new Exception("Not implemented: " + c.Header.GetType().Name);
            } 
            if (container is ContentPresenter) {
                var c = container as ContentPresenter;
                if (c.Content is AccountGroupInfo)
                    return c.FindResource("AccountGroupDataTemplate") as DataTemplate;
                if (c.Content is AccountGroupChildInfo)
                    return c.FindResource("BalanceListAccountDataTemplate") as DataTemplate;
                throw new Exception("Not implemented: " + c.Content.GetType().Name);
            }
            throw new Exception("Not implemented exception.");
        }

    }
}
