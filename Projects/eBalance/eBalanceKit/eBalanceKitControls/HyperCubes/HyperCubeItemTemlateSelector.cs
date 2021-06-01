// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-24
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Windows;
using System.Windows.Controls;
using Taxonomy.Enums;
using eBalanceKitBusiness.HyperCubes.Interfaces;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;

namespace eBalanceKitControls.HyperCubes {
    public class HyperCubeItemTemlateSelector : DataTemplateSelector {
        public override DataTemplate SelectTemplate(object obj, DependencyObject container) {
            var element = container as FrameworkElement;
            if (element != null) {
                if (obj is IHyperCubeTableEntry) {
                    var item = obj as IHyperCubeTableEntry;
                    switch (item.Item.PrimaryDimensionValue.ValueType) {
                        case XbrlElementValueTypes.Abstract:
                            return element.FindResource("HyperCubeTableEntryAbstract") as DataTemplate;

                        case XbrlElementValueTypes.Monetary:
                            return element.FindResource("HyperCubeTableEntryMonetary") as DataTemplate;

                        case XbrlElementValueTypes.String:
                            return element.FindResource("HyperCubeTableEntryString") as DataTemplate;

                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            return null;
        }
    }
}