// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using eBalanceKitBusiness.Structures.GlobalSearch;

namespace eBalanceKit.TemplateSelectors {
    internal class GlobalSearchTemplateSelector : DataTemplateSelector {

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            var element = container as FrameworkElement;
            if (element != null) {
                if (item is SearchResultItem) return element.FindResource("ResultItem") as HierarchicalDataTemplate;
                else if (item is IGlobalSearcherTreeNode) return element.FindResource("NodeItem") as HierarchicalDataTemplate;
            }
            return null;
        }

    }
}
