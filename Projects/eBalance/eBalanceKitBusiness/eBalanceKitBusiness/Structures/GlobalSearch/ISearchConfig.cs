// --------------------------------------------------------------------------------
// author: Márton Garai
// since: 2012-07-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace eBalanceKitBusiness.Structures.GlobalSearch
{
    public interface ISearchConfig
    {
        bool SearchInGeneral { get; set; }
        bool SearchInBalanceSheet { get; set; }
        bool SearchInContingentLiabilities { get; set; }
        bool SearchInIncomeStatement { get; set; }
        bool SearchInAppropriationProfit { get; set; }
        bool SearchInManagementReport { get; set; }
        bool SearchInCashFlowStatement { get; set; }
        bool SearchInNotes { get; set; }
        bool SearchInOtherReportElements { get; set; }
        bool SearchInAdjustmentOfIncome { get; set; }
        bool SearchInDeterminationOfTaxableIncome { get; set; }
        bool SearchInDeterminationOfTaxableIncomeBusinessPartnership { get; set; }
        bool SearchInDeterminationOfTaxableIncomeSpecialCases { get; set; }
        bool SearchInAssets { get; set; }
        bool SearchInLiabilities { get; set; }
        //bool SearchInEquityStatement { get; set; }
    }
}
