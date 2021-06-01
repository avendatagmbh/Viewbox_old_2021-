using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.Algorithm.Interfaces;
using eBalanceKitBusiness.Structures;

namespace eBalanceKitBusiness.Algorithm {
    public class FilterManager {
        #region GetVisibility

        /// <param name="showHiddenEntriesAnyway">For example assigned accounts should be shown even if they were invisble before.</param>
        public static bool GetVisibility(
            IListEntry item,
            BalanceListFilterOptions filterOptions,
            bool showHiddenEntriesAnyway = false) {
            if (!showHiddenEntriesAnyway) {
                // do not show hidden items visible, if the respective balance list option is disabled
                if (!filterOptions.ShowHiddenItems && item.IsHidden) {
                    return false;
                }
            }
            if (filterOptions.FilterType == BalanceListFilterType.Value && string.IsNullOrEmpty(filterOptions.Filter)) {
                return true;
            }

            if (filterOptions.FilterType == BalanceListFilterType.Value) {
                // ignore leading zeros
                string f = filterOptions.Filter.ToLower();
                if (!filterOptions.ExactSearch) while (f.StartsWith("0")) f = f.Remove(0, 1);

                string number = item.Number.ToLower();
                if (!filterOptions.ExactSearch) while (number.StartsWith("0")) number = number.Remove(0, 1);

                if (filterOptions.ExactSearch) {
                    if ((filterOptions.SearchAccountNumbers && number.StartsWith(f)) ||
                        (filterOptions.SearchAccountNames && item.Name.ToLower().StartsWith(f))) {
                        return true;
                    }
                } else {
                    if ((filterOptions.SearchAccountNumbers && number.Contains(f)) ||
                        (filterOptions.SearchAccountNames && item.Name.ToLower().Contains(f))) {
                        return true;
                    }
                }
            } else if (filterOptions.FilterType == BalanceListFilterType.Area) {
                // ignore leading zeros
                string f1 = filterOptions.FilterAreaFrom != null ? filterOptions.FilterAreaFrom.ToLower() : null;
                string f2 = filterOptions.FilterAreaTo != null ? filterOptions.FilterAreaTo.ToLower() : null;
                string number = item.Number.ToLower();

                Int64 number1, number2, value;

                //Enable smaller and greater searches
                if (string.IsNullOrEmpty(f1) && Int64.TryParse(f2, out number2) && Int64.TryParse(number, out value))
                    return value <= number2;
                if (string.IsNullOrEmpty(f2) && Int64.TryParse(f1, out number1) && Int64.TryParse(number, out value))
                    return value >= number1;
                if(Int64.TryParse(f1, out number1) && Int64.TryParse(f2, out number2) && Int64.TryParse(number, out value)) {
                    return (value >= number1 && value <= number2);
                } else {
                    return (f1 == null || number.CompareTo(f1) >= 0 || number.StartsWith(f1)) &&
                       (f2 == null || number.CompareTo(f2) <= 0 || number.StartsWith(f2));    
                }
            }

            return false;
        }
        #endregion GetVisibility
    }
}
