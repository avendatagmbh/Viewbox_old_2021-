// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-09-12
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Structures.ValueTree;

namespace eBalanceKitBusiness.Structures.XbrlElementValue {
    public class XbrlElementValue_Monetary : XbrlElementValueBase, IValueTreeEntry {
        internal XbrlElementValue_Monetary(Taxonomy.IElement element, ValueTreeNode parent, IValueMapping dbValue)
            : base(element, parent, dbValue) {
            PropertyChanged += VirtualBalanceListAndAccountManager.MonetaryValuePropertyChanged;
        }

        public string DisplayString {
            get { return MonetaryValue.Value.HasValue ? LocalisationUtils.CurrencyToString(MonetaryValue.Value.Value) : "-"; }
        }

        protected override void SetValue(object value) {
            if (value == null || value.ToString().Length == 0) MonetaryValue.ManualValue = null;
            else MonetaryValue.ManualValue = Math.Round(Convert.ToDecimal(value), 2, MidpointRounding.AwayFromZero);
        }

        public override bool HasValue {
            get { return MonetaryValue.Value.HasValue; }
        }

        public override bool IsNumeric {
            get { return true; }
        }

        protected override object GetValue() { return MonetaryValue.ManualValue; }
    }
}