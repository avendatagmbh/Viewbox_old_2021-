using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Xml;
using AvdCommon.Rules.ExecuteRules;
using AvdCommon.Rules.Factories;
using AvdCommon.Rules.SortRules;
using Utils;

namespace AvdCommon.Rules
{
    public class RuleSet : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion

        #region Constructor

        public RuleSet()
        {
            ExecuteRules = new ObservableCollectionAsync<ExecuteRule>();
            SortRules = new ObservableCollectionAsync<SortRule>();
            AllRules = new ObservableCollectionAsync<Rule>();
        }

        #endregion

        #region Properties

        public ObservableCollectionAsync<ExecuteRule> ExecuteRules { get; set; }
        public ObservableCollectionAsync<SortRule> SortRules { get; set; }
        public ObservableCollectionAsync<Rule> AllRules { get; set; }

        #endregion

        #region Methods

        private void AddRule<T>(T rule, T beforeRule, bool addDuplicate, ObservableCollection<T> rules) where T : Rule
        {
            if (addDuplicate || !rules.Contains(rule))
            {
                int index = rules.IndexOf(beforeRule);
                if (index != -1)
                    rules.Insert(index, rule);
                else
                    rules.Add(rule);
            }

            if ((IEnumerable<Rule>) rules != AllRules)
                //Always add sort rules before execute rules
                AddRule(rule, (rule is SortRule && AllRules.Count > 0) ? AllRules[0] : beforeRule, addDuplicate,
                        AllRules);
        }

        public void MoveRule<T>(T rule, T beforeRule, ObservableCollection<T> rules) where T : Rule
        {
            int oldIndex = rules.IndexOf(rule);
            int moveToIndex = rules.IndexOf(beforeRule);
            if (moveToIndex == -1) rules.Move(oldIndex, 0);
            else rules.Move(oldIndex, moveToIndex);
            if ((IEnumerable<Rule>) rules != AllRules) MoveRule(rule, beforeRule, AllRules);
        }

        public void AddRule(Rule rule, Rule beforeRule = null, bool addDuplicate = false)
        {
            if (rule is ExecuteRule)
                AddRule(rule as ExecuteRule, beforeRule as ExecuteRule, addDuplicate, ExecuteRules);
            else if (rule is SortRule)
                AddRule(rule as SortRule, beforeRule as SortRule, addDuplicate, SortRules);
        }

        public void RemoveRule(Rule rule)
        {
            if (rule is ExecuteRule)
                ExecuteRules.Remove((ExecuteRule) rule);
            else if (rule is SortRule)
                SortRules.Remove((SortRule) rule);
            AllRules.Remove(rule);
        }

        public void AddRules(IEnumerable<Rule> rules, Rule beforeRule = null)
        {
            foreach (var rule in rules)
                AddRule(rule, beforeRule);
        }

        public void MoveRule(Rule rule, Rule beforeRule)
        {
            if (!(rule is ExecuteRule && beforeRule is ExecuteRule) && !(rule is SortRule && beforeRule is SortRule))
                return;
            if (rule is ExecuteRule)
                MoveRule((ExecuteRule) rule, (ExecuteRule) beforeRule, ExecuteRules);
            else if (rule is SortRule)
                MoveRule((SortRule) rule, (SortRule) beforeRule, SortRules);
        }

        #region XmlStuff

        public string ToXml()
        {
            StringWriter stringWriter = new StringWriter();
            using (XmlWriter writer = XmlWriter.Create(stringWriter))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Rules");
                foreach (var executeRule in ExecuteRules)
                {
                    writer.WriteStartElement("EX");
                    executeRule.WriteXml(writer);
                    writer.WriteEndElement();
                }
                foreach (var sortRule in SortRules)
                {
                    writer.WriteStartElement("SO");
                    sortRule.WriteXml(writer);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            return stringWriter.ToString();
        }

        public void FromXml(string xmlString)
        {
            if (string.IsNullOrEmpty(xmlString)) return;
            AllRules.Clear();
            ExecuteRules.Clear();
            SortRules.Clear();
            XmlDocument doc = new XmlDocument();
            doc.Load(new StringReader(xmlString));
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node.Name == "EX" || node.Name == "SO")
                {
                    Rule rule = RuleFactory.RuleFromNode(node);
                    AddRule(rule);
                }
            }
        }

        #endregion XmlStuff

        #endregion Methods
    }
}