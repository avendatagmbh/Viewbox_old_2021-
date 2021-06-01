// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2010-11-07
// copyright 2010-2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using Taxonomy.Enums;
using Taxonomy.PresentationTree;
using eBalanceKitBusiness.MappingTemplate;

namespace eBalanceKit.Windows.MappingTemplates.TemplateSelectors {
    internal class TemplateTreeTemplateSelector : DataTemplateSelector {
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            FrameworkElement element = container as FrameworkElement;

            if (element == null) return null;

            if (container.DependencyObjectType.Name == "TreeViewItem" && ((TreeViewItem) container).IsSelected) {
                return element.FindResource("PresentationTreeNodeDefault") as DataTemplate;
            }
            
            if (item is PresentationTreeNode) {
                PresentationTreeNode ptNode = item as PresentationTreeNode;

                if (ptNode.Element.IsList)
                    return element.FindResource("PresentationTreeNodeList") as DataTemplate;                      

                switch (ptNode.Element.ValueType) {

                    case XbrlElementValueTypes.Monetary:
                        return element.FindResource("PresentationTreeNodeMonetary") as DataTemplate;                      

                    default:
                        return element.FindResource("PresentationTreeNodeDefault") as DataTemplate;
                }
            }

            if (item is MappingLineGui) {
                return element.FindResource("PresentationTreeNodeAssignmentLineGui") as DataTemplate;
            }

            return null;
        }
    }
}
