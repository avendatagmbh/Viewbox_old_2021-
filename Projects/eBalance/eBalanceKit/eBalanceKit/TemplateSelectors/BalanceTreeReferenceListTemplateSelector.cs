using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Taxonomy.Interfaces.PresentationTree;
using Taxonomy.Enums;
using eBalanceKitBusiness;

namespace eBalanceKit.TemplateSelectors {
    internal class BalanceTreeReferenceListTemplateSelector : DataTemplateSelector {
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            var element = container as FrameworkElement;
            if (element != null) {
                if (item is IPresentationTreeNode) {
                    var node = item as IPresentationTreeNode;

                    if (node.IsHypercubeContainer) {
                        return element.FindResource("PresentationTreeNodeHyperCube") as DataTemplate;
                    }

                    if (node.Element.IsList) {
                        // unhandled list - should not happen
                        return element.FindResource("PresentationTreeNodeList") as DataTemplate;
                    }

                    switch (node.Element.ValueType) {
                        case XbrlElementValueTypes.Boolean:
                            return element.FindResource("PresentationTreeNodeBoolean") as DataTemplate;

                        case XbrlElementValueTypes.Abstract:
                            return element.FindResource("PresentationTreeNodeAbstract") as DataTemplate;

                        case XbrlElementValueTypes.Tuple:
                            return element.FindResource("PresentationTreeNodeTuple") as DataTemplate;

                        case XbrlElementValueTypes.Date:
                            return element.FindResource("PresentationTreeNodeDate") as DataTemplate;

                        case XbrlElementValueTypes.String:
                            return element.FindResource("PresentationTreeNodeText") as DataTemplate;

                        case XbrlElementValueTypes.Monetary:
                            return element.FindResource("PresentationTreeNodeMonetaryReferenceList") as DataTemplate;

                        case XbrlElementValueTypes.SingleChoice:
                            return element.FindResource("PresentationTreeNodeSingleChoice") as DataTemplate;

                        case XbrlElementValueTypes.Numeric:
                        case XbrlElementValueTypes.Int:
                            return element.FindResource("PresentationTreeNodeNumeric") as DataTemplate;

                        default:
                            return null;
                    }
                }

                if (item is IAccount) {
                    return element.FindResource("PresentationTreeNodeAccountSimple") as DataTemplate;
                }

                if (item is ISplittedAccount) {
                    return element.FindResource("PresentationTreeNodeSplittedAccountSimple") as DataTemplate;
                }

                if (item is IAccountGroup) {
                    return element.FindResource("PresentationTreeNodeAccountGroupSimple") as DataTemplate;
                }

                throw new Exception("Found unknown element type in presentation tree.");
            }

            return null;
        }
    }
}
