// --------------------------------------------------------------------------------
// author: Márton Garai
// since: 2012-08-06
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;

namespace eBalanceKitBusiness.MappingTemplate
{
    public class SplittedChildInfo
    {
        public SplittedChildInfo() { 
            IsSelected = false;
        }

        public decimal? AmountPercent { get; set; }

        public string Name { get; set; }

        public string Number { get; set; }

        public bool IsSelected { get; set; }

        public ValueInputMode ValueInputMode { get; set; }

        public string Comment { get; set; }

        // used in TaxonomyAndBalanceListBase.xaml
        public virtual string ValueDisplayString { 
            get {
                if (ValueInputMode == ValueInputMode.Relative) {
                    return AmountPercent.HasValue ? AmountPercent + " %" : "-";
                }
                return string.Empty;
            }
        }

        public string AccountDisplayString { get { return (Number != null ? Number + " - " : "") + Name; } }
    }
}
