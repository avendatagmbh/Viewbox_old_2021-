using System;
using System.Windows;
using System.Windows.Controls;
using AvdCommon.Rules.ExecuteRules;
using AvdCommon.Rules.SortRules;

namespace AvdCommon.Rules.Gui.TemplateSelectors
{
    public class RuleTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var cp = container as TreeViewItem;
            var cp2 = container as ContentPresenter;
            if (item is ExecuteRule)
            {
                if (cp != null) return cp.FindResource("RuleListItem") as DataTemplate;
                return cp2.FindResource("RuleListItem") as DataTemplate;
            }
            if (item is SortRule)
            {
                if (cp != null) return cp.FindResource("SortRuleListItem") as DataTemplate;
                return cp2.FindResource("SortRuleListItem") as DataTemplate;
            }
            // should not happen
            throw new Exception("RuleTemplateSelector entry type is not implemented: " + item.GetType().Name);
        }
    }
}