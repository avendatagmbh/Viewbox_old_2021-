// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-10-30
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using Taxonomy;
using Taxonomy.Enums;
using eBalanceKitBusiness.Interfaces.PresentationTree;
using federalGazetteBusiness.Structures;
using federalGazetteBusiness.Structures.ValueTypes;

namespace eBalanceKit.TemplateSelectors {
    public class FederalGazetteTemplateSelector : DataTemplateSelector {
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            var element = container as FrameworkElement;
            if (element == null) return null;
            IElement xbrlElement = null;

            var fedGazElement = GetTemplateForFederalGazettElement(item, element);
            if (fedGazElement != null) {
                return fedGazElement;
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
                case XbrlElementValueTypes.String:
                    return element.FindResource("TextNode") as DataTemplate;

                default:
                    return element.FindResource("DefaultNode") as DataTemplate;
            }
        }

        private DataTemplate GetTemplateForFederalGazettElement(object item, FrameworkElement element) {
            if (item is IFederalGazetteElementInfo) {
                switch ((item as IFederalGazetteElementInfo).Type) {
                    case  FederalGazetteElementType.String:
                        return element.FindResource("FederalGazetteElementInfoString") as DataTemplate;
                    case  FederalGazetteElementType.Date:
                        return element.FindResource("FederalGazetteElementInfoDate") as DataTemplate;
                    case  FederalGazetteElementType.SelectionList:
                        return element.FindResource("FederalGazetteElementListBox") as DataTemplate;
                    case  FederalGazetteElementType.SelectionOption:
                        return element.FindResource("FederalGazetteElementRadioOptions") as DataTemplate;
                    case  FederalGazetteElementType.Bool:
                        return element.FindResource("FederalGazetteElementBool") as DataTemplate;
                    case  FederalGazetteElementType.Text:
                        return element.FindResource("FederalGazetteElementText") as DataTemplate;
                }
            }
            if (item is FederalGazetteElementList) {
                return element.FindResource("FederalGazetteElementList") as DataTemplate;
            }

            //if (item is FederalGazetteElementOptions) {

            //    return element.FindResource("FederalGazetteElementOptions") as DataTemplate;
                
            //}
            return null;
        }
    }
}