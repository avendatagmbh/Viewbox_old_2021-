using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using DbSearchBase.Interfaces;
using DbSearchLogic.SearchCore.Keys;

namespace DbSearch.TemplateSelectors {
    public class KeyTemplateSelector : DataTemplateSelector {
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            FrameworkElement element = container as FrameworkElement;
            if (element == null) return null;
            if (item is KeyTable) return element.FindResource("keyTableNode") as DataTemplate;
            if (item is IDisplayKey) { 
                IDisplayKey key = item as IDisplayKey;
                if (key.Key is IDisplayForeignKey)
                    if (key.Parent != null && key.Parent.Key is IDisplayPrimaryKey)
                        return element.FindResource("foreignKeyNodeForPrimaryKey") as DataTemplate;
                    else
                        return element.FindResource("foreignKeyNode") as DataTemplate;
                else
                    return element.FindResource("keyNode") as DataTemplate;
            }
            return null;
        }
    }
}
