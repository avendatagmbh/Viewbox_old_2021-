using System.Windows;
using System.Windows.Controls;
using AvdCommon.Rules.Gui.Models;
using AvdCommon.Rules.Gui.TreeNodes;
using Utils;

namespace AvdCommon.Rules.Gui.ResourceDictionaries
{
    public partial class RuleTreeViewResources
    {
        public void btnEditRule_Click(object sender, RoutedEventArgs e)
        {
            DlgEditRule dialog = new DlgEditRule();
            Button b = (Button) sender;
            dialog.Owner = UIHelpers.TryFindParent<Window>(b);
            Rule rule = b.DataContext as Rule;
            if (rule == null) rule = (b.DataContext as RuleTreeNode).Rule;
            EditRuleModel editRuleModel = new EditRuleModel {Rule = rule, RuleWithParameters = (Rule) rule.Clone()};
            dialog.DataContext = editRuleModel;
            if (dialog.ShowDialog().Value)
            {
                editRuleModel.RuleWithParameters.CopyParametersTo(rule);
            }
        }
    }
}