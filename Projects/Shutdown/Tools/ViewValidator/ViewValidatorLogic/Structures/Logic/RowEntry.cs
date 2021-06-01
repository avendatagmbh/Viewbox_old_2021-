// -----------------------------------------------------------
// Created by Benjamin Held - 15.08.2011 11:41:00
// Copyright AvenDATA 2011
// -----------------------------------------------------------

using System;
using System.Collections.Generic;
using AvdCommon.Rules;
using DbAccess.Structures;
using ViewValidatorLogic.Interfaces;

namespace ViewValidatorLogic.Structures.Logic {
    public class RowEntry : IRowEntry{
        #region Constructor
        public RowEntry(RuleSet ruleSet) {
            this.RuleSet = ruleSet;
            RowEntryType = RowEntryType.Normal;
        }
        #endregion

        #region Properties

        #region Value
        private object _value;
        public object Value {
            get { return _value; }
            set {
                _value = value;
                ApplyRules();
            }
        }
        #endregion

        public DbColumnTypes ColumnType { get; set; }
        private RuleSet RuleSet { get; set; }
        public string DisplayString { get { return ToString(); } }

        private string _ruleDisplayString;
        public string RuleDisplayString { get { return _ruleDisplayString; } }
        public RowEntryType RowEntryType { get; set; }
        //If the IsEqualTo method should always return true, set _doNotCompare to true
        //This is used by RuleComparisonTrueForValue
        private bool _doNotCompare = false;

        #endregion

        #region Methods
        string ValueToString(object value) {
            if (value == null) return "DBNULL";
            if (value is Byte[]) {
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                return enc.GetString(value as Byte[]);
            }
            return value.ToString();
        }

        private void ApplyRules()
        {
            _ruleDisplayString = Value == null ? "DBNULL" : ValueToString(Value);

            foreach (var rule in RuleSet.ExecuteRules)
            {
                if (rule.IsSpecialRule)
                {
                    if (rule.SpecialValue == _ruleDisplayString) _doNotCompare = true;
                }
                else
                    _ruleDisplayString = rule.Execute(_ruleDisplayString);
            }
        }


        private bool IsNumber(DbColumnTypes columnType) {
            return columnType == DbColumnTypes.DbBigInt || columnType == DbColumnTypes.DbInt || columnType == DbColumnTypes.DbNumeric;
        }

        private bool IsDate(DbColumnTypes columnType) {
            return columnType == DbColumnTypes.DbDate || columnType == DbColumnTypes.DbDateTime ||
                   columnType == DbColumnTypes.DbTime;
        }

        internal int CompareTo(RowEntry rowEntry) {
            //string v1 = Value == null ? "DBNULL" : ValueToString(Value);
            //string v2 = rowEntry.Value == null ? "DBNULL" : ValueToString(rowEntry.Value);
            //foreach (var rule in RuleSet)
            //    v1 = rule.Execute(v1);
            //foreach (var rule in rowEntry.RuleSet)
            //    v2 = rule.Execute(v2);

            //if (v1 == "DBNULL" || v2 == "DBNULL") return v1 == v2 ? 0 : (v1 == "DBNULL" ? -1 : 1);

            //double value1, value2;
            //if (Double.TryParse(ValueToString(v1), out value1) && Double.TryParse(ValueToString(v2), out value2)) {
            //    if (value1 < value2) return -1;
            //    else if (value1 > value2) return 1;
            //    else return 0;
            //}           

            //return v1.CompareTo(v2);
            string v1 = _ruleDisplayString;
            string v2 = rowEntry._ruleDisplayString;

            if (v1 == "DBNULL" || v2 == "DBNULL") return v1 == v2 ? 0 : (v1 == "DBNULL" ? -1 : 1);

            //If sort rules are present, use the first for comparison (at the moment only one makes sense)
            if (RuleSet.SortRules.Count > 0) {
                return RuleSet.SortRules[0].Sort(v1, v2);
            }
            if (rowEntry.RuleSet.SortRules.Count > 0) {
                return rowEntry.RuleSet.SortRules[0].Sort(v1, v2);
            }

            if (IsNumber(ColumnType) && IsNumber(rowEntry.ColumnType)) {
                double value1, value2;
                if (Double.TryParse(ValueToString(v1), out value1) && Double.TryParse(ValueToString(v2), out value2)) {
                    if (value1 < value2) return -1;
                    if (value1 > value2) return 1;
                    return 0;
                }
            }

            if (IsDate(ColumnType) && IsDate(rowEntry.ColumnType)) {
                DateTime dateTime1, dateTime2;
                if (DateTime.TryParse(v1, out dateTime1) && DateTime.TryParse(v2, out dateTime2)) {
                    return dateTime1.CompareTo(dateTime2);
                }
            }

            return v1.CompareTo(v2);
        }

        internal bool IsEqualTo(RowEntry rowEntry) {
            return _ruleDisplayString == rowEntry._ruleDisplayString || _doNotCompare || rowEntry._doNotCompare;
        }

        public override string ToString() {
            return ValueToString(Value);
        }



        internal RowEntry Clone() {
            RowEntry result = new RowEntry(RuleSet);
            result.Value = Value;
            result.ColumnType = ColumnType;
            result.RowEntryType = RowEntryType;
            return result;
        }
        #endregion
    }
}
