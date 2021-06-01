using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace AvdCommon.Rules.Gui.TreeNodes
{
    public class RuleTreeNode : TreeNodeBase
    {
        public RuleTreeNode(TreeNodeBase parent, Rule rule, Window window)
            : base(parent, window)
        {
            _header = rule.Name;
            Rule = rule;
        }

        #region Properties

        private Rule _rule;

        public Rule Rule
        {
            get { return _rule; }
            set
            {
                if (_rule != value)
                {
                    _rule = value;
                    _rule.PropertyChanged += _rule_PropertyChanged;
                }
            }
        }

        private void _rule_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        #region Header

        private readonly string _header;

        public override string Name
        {
            get { return _header; }
        }

        public string Comment
        {
            get { return Rule.Comment; }
        }

        public bool ShowCommentOnly
        {
            get { return Rule.ShowCommentOnly; }
        }

        #endregion

        #endregion

        #region Methods

        public override void AddRules(List<Rule> rules)
        {
            ((ColumnTreeNode) Parent).AddRules(rules, this);
        }

        public void MoveRule(TreeNodeBase treeNode)
        {
            ((ColumnTreeNode) Parent).MoveRule(this, treeNode);
        }

        public void DeleteRule()
        {
            ((ColumnTreeNode) Parent).DeleteRule(this);
        }

        #endregion
    }
}