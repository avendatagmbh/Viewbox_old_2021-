// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-07-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using eBalanceKitBusiness.Reconciliation.ViewModels;

namespace eBalanceKit.Windows.Reconciliation.TemplateSelectors {
    public class ReconciliationTypeTemplateSelector : DataTemplateSelector {
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            var element = container as FrameworkElement;
            if (element == null) return null;
            if (item is NavTreeNode) return element.FindResource("NavTreeNodeTemplate") as DataTemplate;
            return element.FindResource("DefaultNodeTemplate") as DataTemplate;
        }
    }
}