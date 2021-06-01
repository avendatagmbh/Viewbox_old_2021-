// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-04-24
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Taxonomy;
using eBalanceKitBase.Structures;

namespace eBalanceKitBusiness.Reconciliation.Import {
    public class PreviousYearValue {

        internal PreviousYearValue(decimal value, IElement element) {
            Value = value;
            Element = element;
        }

        public decimal Value { get; set; }
        public IElement Element { get; set; }
        public string ValueDisplayString { get { return LocalisationUtils.CurrencyToString(Value); } }
    }
}