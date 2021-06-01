using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using AvdCommon.Rules.Interfaces;

namespace AvdCommon.Rules.Gui.TreeNodes
{
    public class ColumnTreeNode : TreeNodeBase
    {
        private readonly IColumn _column;
        private readonly string _header;

        public ColumnTreeNode(IColumn column, Window window)
            : base(null, window)
        {
            _header = column.Name;
            _column = column;
            foreach (var rule in column.Rules.AllRules)
                Children.Add(new RuleTreeNode(this, rule, OwnerWindow));
            _column.Rules.AllRules.CollectionChanged += AllRules_CollectionChanged;
            //foreach (var rule in column.Rules.SortRules)
            //    Children.Add(new RuleTreeNode(this, (Rule)rule, OwnerWindow));
            //foreach (var rule in column.Rules.ExecuteRules) {
            //    Children.Add(new RuleTreeNode(this, (Rule)rule, OwnerWindow));
            //}
            //_column.Rules.ExecuteRules.CollectionChanged += UsedRules_CollectionChanged;
        }

        public override string Name
        {
            get { return _header; }
        }

        private void AllRules_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var newRule in e.NewItems)
                    {
                        Children.Insert(e.NewStartingIndex, new RuleTreeNode(this, (Rule) newRule, OwnerWindow));
                    }
                    IsExpanded = true;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var deletedRule in e.OldItems)
                    {
                        for (int i = Children.Count - 1; i >= 0; --i)
                            if (((RuleTreeNode) Children[i]).Rule == (Rule) deletedRule)
                                Children.RemoveAt(i);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    Children.Move(e.OldStartingIndex, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Children.Clear();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void RemoveRule(RuleTreeNode ruleTreeNode)
        {
            _column.Rules.RemoveRule(ruleTreeNode.Rule);
        }

        public void AddRules(List<Rule> rules, TreeNodeBase afterNode)
        {
            if (afterNode == null || afterNode is ColumnTreeNode)
                //Insert at beginning
                _column.Rules.AddRules(rules, Children.Count == 0 ? null : _column.Rules.AllRules[0]);
            else if (afterNode is RuleTreeNode)
            {
                //AddRules expects the node before the insertion
                int index = Children.IndexOf(afterNode);
                if (index >= 0 && index < Children.Count - 1)
                    _column.Rules.AddRules(rules, ((RuleTreeNode) Children[index + 1]).Rule);
                else
                    _column.Rules.AddRules(rules, null);
            }
            IsExpanded = true;
        }

        public override void AddRules(List<Rule> rules)
        {
            AddRules(rules, this);
        }

        public void MoveRule(RuleTreeNode ruleTreeNode, TreeNodeBase destinationNode)
        {
            int indexDestination = Children.IndexOf(destinationNode);
            if (indexDestination == -1)
            {
                //Put rule at beginning
                _column.Rules.MoveRule(ruleTreeNode.Rule,
                                       _column.Rules.AllRules.Count == 0 ? null : _column.Rules.AllRules[0]);
            }
            else
            {
                _column.Rules.MoveRule(ruleTreeNode.Rule, ((RuleTreeNode) destinationNode).Rule);
            }
        }

        internal void DeleteRule(RuleTreeNode ruleTreeNode)
        {
            _column.Rules.RemoveRule(ruleTreeNode.Rule);
        }
    }
}