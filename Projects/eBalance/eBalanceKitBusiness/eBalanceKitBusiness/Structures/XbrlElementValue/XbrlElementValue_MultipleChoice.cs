using System;
using System.Collections.Generic;
using System.ComponentModel;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Structures.ValueTree;
using eBalanceKitBusiness.Structures.DbMapping.ValueMappings;

namespace eBalanceKitBusiness.Structures.XbrlElementValue {

    #region class BoolWithNotifyChanged
    public class BoolWithNotifyChanged : INotifyPropertyChanged {
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        private bool? _boolValue;

        public bool? BoolValue {
            get { return _boolValue; }
            set {
                if (_boolValue != value) {
                    _boolValue = value;
                    OnPropertyChanged("BoolValue");
                }
            }
        }


        bool _validationError;
        public bool ValidationError {
            get { return _validationError; }
            set {
                if (_validationError != value) {
                    _validationError = value;
                    OnPropertyChanged("ValidationError");
                }
            }
        }

        string _validationErrorMessage;
        public string ValidationErrorMessage {
            get { return _validationErrorMessage; }
            set {
                if (_validationErrorMessage != value) {
                    _validationErrorMessage = value;
                    OnPropertyChanged("ValidationErrorMessage");
                }
            }
        }

        bool _validationWarning;
        public bool ValidationWarning {
            get { return _validationWarning; }
            set {
                if (_validationWarning != value) {
                    _validationWarning = value;
                    OnPropertyChanged("ValidationWarning");
                }
            }
        }

        string _validationWarningMessage;
        public string ValidationWarningMessage {
            get { return _validationWarningMessage; }
            set {
                if (_validationWarningMessage != value) {
                    _validationWarningMessage = value;
                    OnPropertyChanged("ValidationWarningMessage");
                }
            }
        }

    }
    #endregion class BoolWithNotifyChanged

    /// <summary>
    /// Presentation tree value class for multiple elements.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-01-26</since>
    public class XbrlElementValue_MultipleChoice: XbrlElementValueBase, IValueTreeEntry {

        #region constructor
        internal XbrlElementValue_MultipleChoice(Taxonomy.ITaxonomy taxonomy, Taxonomy.IElement element, ValueTreeNode parent, IValueMapping dbValue, Type valueType)
            : base(element, parent, dbValue) {

            IsChecked = new Dictionary<string, BoolWithNotifyChanged>();

            // init value list
            this.Elements = new List<Taxonomy.IElement>();
            if (element.Children.Count > 0) {
                string substitutionGroup = element.Children[0].Name;
                foreach (Taxonomy.IElement elem in taxonomy.Elements.Values) {
                    if (elem.SubstitutionGroup.EndsWith(substitutionGroup)) {
                        this.Elements.Add(elem);
                        BoolWithNotifyChanged newValue = new BoolWithNotifyChanged();
                        IsChecked[elem.Id] = newValue;
                        newValue.PropertyChanged += new PropertyChangedEventHandler(newValue_PropertyChanged);
                    }
                }
            }

            // init IsChecked states
            if (this.Value != null && this.Value.ToString().Length > 0) {
                foreach (string elemName in this.Value.ToString().Split('|')) {
                    if (this.IsChecked.ContainsKey(elemName.Substring(2))) {
                        if (elemName.Substring(0, 1) == "T") {
                            this.IsChecked[elemName.Substring(2)].BoolValue = true;
                        } else {
                            this.IsChecked[elemName.Substring(2)].BoolValue = false;
                        }
                    }
                }
            }
        }
        #endregion constructor

        void newValue_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            string newValue = string.Empty;
            foreach (string elemName in this.IsChecked.Keys) {
                if (IsChecked[elemName].BoolValue.HasValue) {
                    if (newValue.Length > 0) newValue += "|";
                    if (IsChecked[elemName].BoolValue.Value == true) {
                        newValue += "T#" + elemName;
                    } else {
                        newValue += "F#" + elemName;
                    }
                }
            }

            if (newValue.Length == 0) newValue = null;

            this.Value = newValue;
        }

        private string _value;

        public List<Taxonomy.IElement> Elements { get; set; }       
        private Dictionary<string, bool> Values { get; set; }
        public string DisplayString { get { return string.Empty; } }
        public override bool IsNumeric { get { return false; } }
        public override bool HasValue { get { return true; } }
        public Dictionary<string, BoolWithNotifyChanged> IsChecked { get; private set; }
        public void SetValue(string xbrlId) { this.Values[xbrlId] = true; }
        public void ResetValue(string xbrlId) { this.Values[xbrlId] = false; }
        protected override void SetValue(object value) { if (value == null) _value = null; else _value = value.ToString(); }
        protected override object GetValue() { return _value; }
    }
}
