// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-03-13
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using Taxonomy;
using Taxonomy.Enums;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKitBusiness.Reconciliation.Interfaces;

namespace eBalanceKit.Windows.Reconciliation.TemplateSelectors {
    public class TreeViewItemTemplateSelector : DataTemplateSelector {
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            var element = container as FrameworkElement;
            if (element == null) return null;
            IElement xbrlElement = null;

            IReconciliationTransaction maybeReconciliationTransaction = item as IReconciliationTransaction;
            if (maybeReconciliationTransaction != null) {
                if (element is TreeViewItem) {
                    maybeReconciliationTransaction.ScrollIntoViewRequested += path => {
                        element.BringIntoView();
                        element.SetValue(TreeViewItem.IsSelectedProperty, true);
                    };
                }
                return element.FindResource("ReconciliationTransaction") as DataTemplate;
            }

            //if (item is IReconciliationTreeNode) xbrlElement = (item as IReconciliationTreeNode).Element;
            IReconciliationTreeNode maybeReconciliationTreeNode = (item as IReconciliationTreeNode);
            if (maybeReconciliationTreeNode != null) {
                xbrlElement = maybeReconciliationTreeNode.Element;
                if (element is TreeViewItem) {
                    maybeReconciliationTreeNode.ScrollIntoViewRequested += path => {
                        element.BringIntoView();
                        element.SetValue(TreeViewItem.IsSelectedProperty, true);
                    };
                }
            }

            //if (item is IPresentationTreeNode) xbrlElement = (item as IPresentationTreeNode).Element;
            IPresentationTreeNode maybePresentationTreeNode = (item as IPresentationTreeNode);
            if (maybePresentationTreeNode != null) {
                xbrlElement = maybePresentationTreeNode.Element;
                if (element is TreeViewItem) {
                    maybePresentationTreeNode.ScrollIntoViewRequested += path => {
                        element.BringIntoView();
                        element.SetValue(TreeViewItem.IsSelectedProperty, true);
                    };
                }
            }
            if (xbrlElement == null) return null;

            switch (xbrlElement.ValueType) {
                case XbrlElementValueTypes.Monetary:
                    return element.FindResource("MonetaryNode") as DataTemplate;

                default:
                    return element.FindResource("DefaultNode") as DataTemplate;
            }
        }
    }
}