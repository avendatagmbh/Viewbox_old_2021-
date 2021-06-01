using System;
using System.ComponentModel;
using System.Xml;
using AvdCommon.Rules.Factories;

/*
 * Dies bildet die Basisklasse für alle Regeln, um eine neue Regel zu erstellen, muss davon abgeleitet werden und
 * 1. Im RuleManager die neue Regel hinzugefügt werden
 * 2. Um das Speichern/Laden zu unterstützen, muss in der RuleFactory der UniqueName ins case eingefügt werden
 * 3. Für parametrisierte Regeln muss ein entsprechendes Control hinzugefügt werden (utner RuleSpecific) und das Control in die RuleControlFactory eingetragen werden
 */

namespace AvdCommon.Rules
{
    public abstract class Rule : ICloneable, INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Constructor

        public Rule()
        {
            Name = "TestRule";
            UniqueName = "TestRule";
        }

        #endregion

        #region Properties

        #region Name

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        #endregion Name

        public string UniqueName { get; set; }
        public abstract bool HasParameter { get; }

        #region IsSelected

        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        #endregion

        #region Comment

        private string _comment = "";

        public string Comment
        {
            get { return _comment; }
            set
            {
                if (_comment != value)
                {
                    _comment = value;
                    OnPropertyChanged("Comment");
                }
            }
        }

        #endregion Comment

        #region ShowCommentOnly

        private bool _showCommentOnly;

        public bool ShowCommentOnly
        {
            get { return _showCommentOnly; }
            set
            {
                if (_showCommentOnly != value)
                {
                    _showCommentOnly = value;
                    OnPropertyChanged("ShowCommentOnly");
                }
            }
        }

        #endregion

        #endregion

        #region Methods

        /*        public virtual string Execute(string value) {
            return value;
        }*/

        public virtual object Clone()
        {
            Rule result = RuleFactory.RuleFromUniqueName(UniqueName);
            CopyParametersTo(result);
            return result;
        }

        public virtual int Sort(string v1, string v2)
        {
            return v1.CompareTo(v2);
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }
            Rule rule = obj as Rule;
            if (rule == null)
            {
                return false;
            }
            return Equals(rule);
        }

        public virtual bool Equals(Rule rule)
        {
            if ((object) rule == null)
            {
                return false;
            }
            return UniqueName == rule.UniqueName && Comment == rule.Comment && ShowCommentOnly == rule.ShowCommentOnly &&
                   Name == rule.Name;
        }

        public override int GetHashCode()
        {
            return UniqueName.GetHashCode() ^ (Comment == null ? 0 : Comment.GetHashCode()) ^
                   ShowCommentOnly.GetHashCode() ^ Name.GetHashCode();
        }

        public static bool operator ==(Rule a, Rule b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }
            // If one is null, but not both, return false.
            if (((object) a == null) || ((object) b == null))
            {
                return false;
            }
            return a.Equals(b);
        }

        public static bool operator !=(Rule a, Rule b)
        {
            return !(a == b);
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Name", UniqueName);
            writer.WriteAttributeString("Comment", Comment);
            writer.WriteAttributeString("ShowCommentOnly", ShowCommentOnly.ToString());
        }

        //For simple rules there is nothing to read, parameterized rules can read their parameters
        public virtual void ReadXml(XmlNode node)
        {
            if (node.Attributes["Comment"] != null) Comment = node.Attributes["Comment"].Value;
            if (node.Attributes["ShowCommentOnly"] != null)
                ShowCommentOnly = Convert.ToBoolean(node.Attributes["ShowCommentOnly"].Value);
        }

        public virtual void CopyParametersTo(Rule rule)
        {
            rule.IsSelected = IsSelected;
            rule.Comment = Comment;
            rule.ShowCommentOnly = ShowCommentOnly;
        }

        #endregion Methods
    }
}