using System;
using System.Windows;
using System.Windows.Controls;
using AvdCommon.Rules.ExecuteRules;
using AvdCommon.Rules.Gui.TreeNodes;

namespace AvdCommon.Rules.Gui.TemplateSelectors
{
    public class RuleToColumnTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var cp = container as TreeViewItem;
            var cp2 = container as ContentPresenter;
            if (item is RuleTreeNode)
            {
                RuleTreeNode node = (RuleTreeNode) item;
                string resourceName = node.Rule is ExecuteRule ? "RuleListItem" : "SortRuleListItem";
                if (cp != null) return cp.FindResource(resourceName) as DataTemplate;
                return cp2.FindResource(resourceName) as DataTemplate;
            }
            if (item is ColumnTreeNode)
            {
                if (cp != null) return cp.FindResource("ColumnListItem") as DataTemplate;
                return cp2.FindResource("ColumnListItem") as DataTemplate;
            }
            // should not happen
            throw new Exception("RuleToColumn entry type is not implemented: " + item.GetType().Name);
        }
    }
}