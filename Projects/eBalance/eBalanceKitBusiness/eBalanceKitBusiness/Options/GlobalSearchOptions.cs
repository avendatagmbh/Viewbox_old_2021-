// --------------------------------------------------------------------------------
// author: Márton Garai
// since: 2012-07-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using eBalanceKitBusiness.Structures.GlobalSearch;

namespace eBalanceKitBusiness.Options
{
    internal class GlobalSearchOptions
    {
        internal GlobalSearchOptions(AvailableOptions options) { _options = options; }

        private readonly AvailableOptions _options;

        public void GetGlobalSearchOptionsFromDatabase(ISearchConfig config) { 
            config.SearchInGeneral = _options.GetBoolValue("SearchInGeneral") ?? true;
            config.SearchInBalanceSheet = _options.GetBoolValue("SearchInBalanceSheet") ?? true;
            config.SearchInContingentLiabilities = _options.GetBoolValue("SearchInContingentLiabilities") ?? true;
            config.SearchInIncomeStatement = _options.GetBoolValue("SearchInIncomeStatement") ?? true;
            config.SearchInAppropriationProfit = _options.GetBoolValue("SearchInAppropriationProfit") ?? true;
            //config.SearchInEquityStatement = _options.GetBoolValue("SearchInEquityStatement") ?? true;
            config.SearchInManagementReport = _options.GetBoolValue("SearchInManagementReport") ?? true;
            config.SearchInCashFlowStatement = _options.GetBoolValue("SearchInCashFlowStatement") ?? true;
            config.SearchInNotes = _options.GetBoolValue("SearchInNotes") ?? true;
            config.SearchInOtherReportElements = _options.GetBoolValue("SearchInOtherReportElements") ?? true;
            config.SearchInAdjustmentOfIncome = _options.GetBoolValue("SearchInAdjustmentOfIncome") ?? true;
            config.SearchInDeterminationOfTaxableIncome = _options.GetBoolValue("SearchInDeterminationOfTaxableIncome") ?? true;
            config.SearchInDeterminationOfTaxableIncomeBusinessPartnership = _options.GetBoolValue("SearchInDeterminationOfTaxableIncomeBusinessPartnership") ?? true;
            config.SearchInDeterminationOfTaxableIncomeSpecialCases = _options.GetBoolValue("SearchInDeterminationOfTaxableIncomeSpecialCases") ?? true;
            config.SearchInAssets = _options.GetBoolValue("SearchInAssets") ?? true;
            config.SearchInLiabilities = _options.GetBoolValue("SearchInLiabilities") ?? true;
        }

        public void SetGlobalSearchOptionsValue(ISearchConfig config) { 
            _options.SetValue("SearchInGeneral", config.SearchInGeneral);
            _options.SetValue("SearchInBalanceSheet", config.SearchInBalanceSheet);
            _options.SetValue("SearchInContingentLiabilities", config.SearchInContingentLiabilities);
            _options.SetValue("SearchInIncomeStatement", config.SearchInIncomeStatement);
            _options.SetValue("SearchInAppropriationProfit", config.SearchInAppropriationProfit);
            //_options.SetValue("SearchInEquityStatement", config.SearchInEquityStatement);
            _options.SetValue("SearchInManagementReport", config.SearchInManagementReport);
            _options.SetValue("SearchInCashFlowStatement", config.SearchInCashFlowStatement);
            _options.SetValue("SearchInNotes", config.SearchInNotes);
            _options.SetValue("SearchInOtherReportElements", config.SearchInOtherReportElements);
            _options.SetValue("SearchInAdjustmentOfIncome", config.SearchInAdjustmentOfIncome);
            _options.SetValue("SearchInDeterminationOfTaxableIncome", config.SearchInDeterminationOfTaxableIncome);
            _options.SetValue("SearchInDeterminationOfTaxableIncomeBusinessPartnership", config.SearchInDeterminationOfTaxableIncomeBusinessPartnership);
            _options.SetValue("SearchInDeterminationOfTaxableIncomeSpecialCases", config.SearchInDeterminationOfTaxableIncomeSpecialCases);
            _options.SetValue("SearchInAssets", config.SearchInAssets);
            _options.SetValue("SearchInLiabilities", config.SearchInLiabilities);
            _options.SaveConfiguration();
        }
    }
}
