using System.Windows;
using System.Windows.Controls;
using Taxonomy;
using Taxonomy.Enums;
using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKitBusiness.Reconciliation.Interfaces;

namespace eBalanceKit.Windows.Reconciliation.TemplateSelectors {
    class SimplifiedTreeViewItemTemplateSelector : DataTemplateSelector {
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            var element = container as FrameworkElement;
            if (element == null) return null;
            IElement xbrlElement = null;

            IReconciliationTransaction maybeReconciliationTransaction = item as IReconciliationTransaction;
            if (maybeReconciliationTransaction != null) {
                return element.FindResource("SimpleReconciliationTransaction") as DataTemplate; }

            IReconciliationTreeNode maybeReconciliationTreeNode = (item as IReconciliationTreeNode);
            if (maybeReconciliationTreeNode != null) {
                xbrlElement = maybeReconciliationTreeNode.Element;
                //maybeReconciliationTreeNode.ScrollIntoViewRequested += path => {
                //    element.BringIntoView();
                //    ((TreeViewItem) element).IsSelected = true;
                //};
            }

            IPresentationTreeNode maybePresentationTreeNode = (item as IPresentationTreeNode);
            if (maybePresentationTreeNode != null) {
                xbrlElement = maybePresentationTreeNode.Element;
                //maybePresentationTreeNode.ScrollIntoViewRequested += path => {
                //    element.BringIntoView();
                //    ((TreeViewItem) element).IsSelected = true;
                //};
            }
            if (xbrlElement == null) return null;

            switch (xbrlElement.ValueType) {
                case XbrlElementValueTypes.Monetary:
                    return element.FindResource("SimpleMonetaryNode") as DataTemplate;

                default:
                    return element.FindResource("DefaultNode") as DataTemplate;
            }
        }
    }
}
