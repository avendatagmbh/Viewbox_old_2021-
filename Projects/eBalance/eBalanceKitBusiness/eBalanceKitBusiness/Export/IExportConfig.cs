// --------------------------------------------------------------------------------
// author: sev
// since: 2012-02-10
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.ComponentModel;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Export {
    public interface IExportConfig : INotifyPropertyChanged {

        Document Document { get; }
        
        ExportTypes ExportType { get; set; }
        ConfigExport.ExportTypeInfo ExportInfo { get; }
        string Filename { get; set; }
        string FilePath { get; set; }
        bool FileExists { get; }
        
        bool ExportAsXbrl { get; set; }

        bool ExportCompanyInformation { get; set; }
        
        bool ExportReportInformation { get; set; }
        
        bool ExportDocumentInformation { get; set; }
        
        bool ExportAccountBalances { get; set; }
        
        bool ExportUnassignedAccounts { get; set; }
        
        bool ExportBalanceSheet { get; set; }               
        bool ShowBalanceSheet { get; set; }
        
        bool ExportIncomeStatement { get; set; }
        bool ShowIncomeStatement { get; set; }
        
        bool ExportAppropriationProfit { get; set; }
        bool ShowAppropriationProfit { get; set; }
        
        bool ShowContingentLiabilities { get; set; }
        bool ExportContingentLiabilities { get; set; }
        
        bool ShowCashFlowStatement { get; set; }
        bool ExportCashFlowStatement { get; set; }
        
        bool ShowManagementReport { get; set; }        
        bool ExportManagementReport { get; set; }
        
        bool ExportAdjustmentOfIncome { get; set; }
        bool ShowAdjustmentOfIncome { get; set; }
        
        bool ExportDeterminationOfTaxableIncome { get; set; }
        bool ShowDeterminationOfTaxableIncome { get; set; }
        
        bool ExportDeterminationOfTaxableIncomeBusinessPartnership { get; set; }
        bool ShowDeterminationOfTaxableIncomeBusinessPartnership { get; set; }
        
        bool ExportDeterminationOfTaxableIncomeSpecialCases { get; set; }
        bool ShowDeterminationOfTaxableIncomeSpecialCases { get; set; }
        
        bool ExportNtAssNet { get; set; }
        bool ShowNtAssNet { get; set; }
        
        bool ExportNtAssGrossShort { get; set; }
        bool ShowNtAssGrossShort { get; set; }
        
        bool ExportNtAssGross { get; set; }
        bool ShowNtAssGross { get; set; }
        
        bool ExportEquityStatement { get; set; }
        bool ShowEquityStatement { get; set; }
        
        bool ExportComments { get; set; }
        bool ExportNILValues { get; set; }        
        bool ExportMandatoryOnly { get; set; }
        bool ExportReconciliationInfo { get; set; }        
        bool ExportAccounts { get; set; }
        bool ExportNotes { get; set; }
        bool ExportOtherReportElements { get; set; }

        bool IsCommercialBalanceSheet { get; }
        bool HasHypercubes { get; }

    }
}