using System;
using System.Collections.Generic;
using System.Diagnostics;
using DbAccess;
using Taxonomy;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Structures.ValueTree;

namespace eBalanceKitBusiness.Structures.XbrlElementValue {

    /// <summary>
    /// Presentation tree value class for single choice elements.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-01-26</since>
    public class XbrlElementValue_SingleChoice : XbrlElementValueBase, IValueTreeEntry {

        internal XbrlElementValue_SingleChoice(Taxonomy.IElement element, ValueTreeNode parent, IValueMapping dbValue, Type valueType)
            : base(element, parent, dbValue) {

            // init value list
            if (element.Children.Count > 0) {
                this.Elements = element.Children[0].Children;
            }

            // select "Kerntaxonomie" as default value for new reports
            if (element.Id == "de-gcd_genInfo.report.id.specialAccountingStandard" && dbValue.Value == null) {
                dbValue.Value = "genInfo.report.id.specialAccountingStandard.K";
            }

            // select previously selected value
            if (dbValue != null && dbValue.Value != null) {
                foreach (var elem in this.Elements) {
                    if (elem.Name.Equals(dbValue.Value)) {
                        this.SelectedValue = elem;
                    }
                }
            }
        }

        private string _value;

        private List<IElement> _elements;
        public List<Taxonomy.IElement> Elements { get { return _elements; } set { _elements = value; } }

        #region SelectedValue
        private Taxonomy.IElement _selectedValue;
        public Taxonomy.IElement SelectedValue {
            get { return _selectedValue; }
            set { 
                _selectedValue = value;
                if (_selectedValue != null) {
                    Value = _selectedValue.Name;
                } else {
                    Value = null;
                }
                OnPropertyChanged("DisplayString");
                OnPropertyChanged("SelectedValue");
            }
        }
        #endregion SelectedValue

        #region DisplayString
        public string DisplayString {
            get {
                return _selectedValue == null ? "<keine Bezeichnung>" : SelectedValue.Label;
            }
        }
        #endregion DisplayString

        public override bool IsNumeric { get { return false; } }
        public override bool HasValue { get { return this.SelectedValue != null; } }

        protected override void SetValue(object value) {
            if (value == null) _value = null;
            else _value = value.ToString();
        }

        protected override object GetValue() { return _value; }

    }
}
