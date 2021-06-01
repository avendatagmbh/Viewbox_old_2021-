/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-11-04      initial implementation
 *************************************************************************************************************/

using System;
using System.Diagnostics;
using System.Globalization;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Structures.ValueTree;

namespace eBalanceKitBusiness.Structures.XbrlElementValue {

    #region XblrElementValue_NullableValueBase
    public abstract class XblrElementValue_NullableValueBase<T> : XbrlElementValueBase where T : struct {
        protected XblrElementValue_NullableValueBase(Taxonomy.IElement element, ValueTreeNode parent, IValueMapping dbValue, bool isNumeric) :
            base(element, parent, dbValue) {
            _isNumeric = isNumeric;
        }
        
        public virtual string DisplayString { get { return HasValue ? _value.Value.ToString() : string.Empty; } }
        protected T? _value;
        public override bool HasValue { get { return _value.HasValue; } }

        private bool _isNumeric;
        public override bool IsNumeric { get { return _isNumeric; } }
        
        protected override object GetValue() { return _value; }
    }
    #endregion XblrElementValue_NullableValueBase

    #region Boolean
    public class XbrlElementValue_Boolean : XblrElementValue_NullableValueBase<bool>, IValueTreeEntry {
        internal XbrlElementValue_Boolean(Taxonomy.IElement element, ValueTreeNode parent, IValueMapping dbValue) : base(element, parent, dbValue, false) { }
        
        public bool? BoolValue { 
            get { return _value; }
            set {
                _value = value;
                OnPropertyChanged("BoolValue");
                OnPropertyChanged("DisplayString");
            }
        }
        
        protected override void SetValue(object value) {
            if (value == null) BoolValue = null;
            else BoolValue = Convert.ToBoolean(value); 
        }
    }
    #endregion Boolean

    #region Date
    public class XbrlElementValue_Date : XblrElementValue_NullableValueBase<DateTime>, IValueTreeEntry {
        internal XbrlElementValue_Date(Taxonomy.IElement element, ValueTreeNode parent, IValueMapping dbValue) : base(element, parent, dbValue, false) { }

        public override string DisplayString { 
            get { 
                if (DateValue.HasValue) return DateValue.Value.ToShortDateString(); 
                else return string.Empty; 
            } 
        }
        public DateTime? DateValue {
            get { return _value; }
            set {
                _value = value;
                OnPropertyChanged("DateValue");
                OnPropertyChanged("DisplayString");
            }
        }
        
        protected override void SetValue(object value) {
            DateValue = null;
            if (value == null) return;
            if (value.GetType() == typeof(string)) {
                string valueAsString = value as string;
                if (!string.IsNullOrEmpty(valueAsString)) {
                    DateTime date;
                    if (!DateTime.TryParse(valueAsString, out date)) {
                        if (!DateTime.TryParse(valueAsString, LocalisationUtils.GermanCulture, DateTimeStyles.None, out date)) {
                            Debug.Fail(string.Format("DateTime {0} could not be parse from string - this should not happen", valueAsString));
                        }
                    }
                    DateValue = date;
                    //DateValue = Convert.ToDateTime(value);
                }
            } else if (value is DateTime)
                DateValue = (DateTime) value;
            else
                DateValue = Convert.ToDateTime(value);
        }
    }
    #endregion Date

    #region Int
    public class XbrlElementValue_Int : XblrElementValue_NullableValueBase<Int64>, IValueTreeEntry {
        internal XbrlElementValue_Int(Taxonomy.IElement element, ValueTreeNode parent, IValueMapping dbValue) : base(element, parent, dbValue, true) { }

        public Int64? IntValue {
            get { return _value; }
            set {
                _value = value;
                OnPropertyChanged("IntValue");
                OnPropertyChanged("DisplayString");
            }
        }

        protected override void SetValue(object value) {
            if (value == null || string.IsNullOrEmpty(value.ToString())) IntValue = null;
            else IntValue = Convert.ToInt64(value); 
        }
    }
    #endregion Int

    #region Numeric
    public class XbrlElementValue_Numeric : XblrElementValue_NullableValueBase<decimal>, IValueTreeEntry {
        internal XbrlElementValue_Numeric(Taxonomy.IElement element, ValueTreeNode parent, IValueMapping dbValue) : base(element, parent, dbValue, true) { }

        public decimal? NumericValue {
            get { return _value; }
            set {
                _value = value;
                OnPropertyChanged("NumericValue");
                OnPropertyChanged("DisplayString");
            }
        }

        protected override void SetValue(object value) { 
            if (value == null) NumericValue = null; 
            else NumericValue = Convert.ToDecimal(value); 
        }
    }
    #endregion Numeric

    #region String
    public class XbrlElementValue_String : XbrlElementValueBase, IValueTreeEntry {
        internal XbrlElementValue_String(Taxonomy.IElement element, ValueTreeNode parent, IValueMapping dbValue) : base(element, parent, dbValue) { }
        public string DisplayString {
            get {
                if (_value == null) return "-";

                int maxChars = 60;
                string result;

                if (_value.Contains(System.Environment.NewLine)) {
                    result = _value.Substring(0, _value.IndexOf(System.Environment.NewLine));
                    if (result.Length + 3 > maxChars) {
                        result = result.Substring(0, maxChars - 3);
                    }
                    result = result + "...";
                } else {
                    result = _value;
                    if (result.Length + 3 > maxChars) {
                        result = result.Substring(0, maxChars - 3);
                        result = result + "...";
                    }
                }

                return result;
            }
        }
        private string _value;
        public string StringValue { get { return _value; } }
        public override bool HasValue { get { return !string.IsNullOrEmpty(this.StringValue); } }
        public override bool IsNumeric { get { return false; } }
        
        protected override void SetValue(object value) {
            if (value == null) _value = null;
            else if (string.IsNullOrWhiteSpace(value.ToString())) _value = null;
            else _value = value.ToString();
        }
        
        protected override object GetValue() {
            if (_value == null) return null;
            else return _value;
        }
    }
    #endregion String
}
