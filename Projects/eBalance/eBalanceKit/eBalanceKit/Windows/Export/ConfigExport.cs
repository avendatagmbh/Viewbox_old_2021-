using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.Export;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKit.Windows.Export
{
    public class ConfigExport {

        public ConfigExport(Document document) {

            StatusBarText = "Dies ist ein Test";

            //

            


        }

        
        #region props

        public string StatusBarText { get; set; }



        public Document Document { get; set; }
        public string Filename { get; set; }

        public bool ExportBalanceSheet { get; set; }
        public bool ExportIncomeStatement { get; set; }
        public bool ExportAccountBalances { get; set; }

        public bool ExportNILValues { get; set; }
        public bool ExportTransferHBST { get; set; }
        public bool ExportAccounts { get; set; }

        public bool ExportDocumentInformation { get; set; }
        public bool ExportReportInformation { get; set; }
        public bool ExportCompanyInformation { get; set; }

        public bool ExportComments { get; set; }

        public bool GermanBalance { get; set; }


        public bool ExportMandatoryOnly { get; set; }
        public bool ExportAppropriationProfit { get; set; }
        public bool ExportContingentLiabilities { get; set; }
        //public bool ExportChangesEquityAccounts { get; set; }
        //public bool ExportChangesEquityStatement { get; set; }
        public bool ExportCashFlowStatement { get; set; }
        //public bool ExportNotes { get; set; }
        //public bool ExportManagementReport { get; set; }
        //public bool ExportTransfersCommercialCodeToTax { get; set; }
        //public bool ExportOtherReportElements { get; set; }
        //public bool ExportDetailedInformation { get; set; }
        public bool ExportAdjustmentOfIncome { get; set; }
        public bool ExportDeterminationOfTaxableIncome { get; set; }
        public bool ExportDeterminationOfTaxableIncomeBusinessPartnership { get; set; }
        public bool ExportDeterminationOfTaxableIncomeSpecialCases { get; set; }
        public bool ExportNotes { get; set; }
        public bool ExportManagementReport { get; set; }



        public object ShowBalanceSheet { get; set; }
        public object ShowAppropriationProfit { get; set; }
        public object ShowContingentLiabilities { get; set; }
        //public object ShowChangesEquityAccounts { get; set; }
        //public object ShowChangesEquityStatement { get; set; }
        public object ShowCashFlowStatement { get; set; }
        //public object ShowNotes { get; set; }
        //public object ShowManagementReport { get; set; }
        //public object ShowTransfersCommercialCodeToTax { get; set; }
        //public object ShowReportElements { get; set; }
        //public object ShowDetailedInformation { get; set; }
        public object ShowAdjustmentOfIncome { get; set; }
        public object ShowDeterminationOfTaxableIncome { get; set; }
        public object ShowDeterminationOfTaxableIncomeBusinessPartnership { get; set; }
        public object ShowDeterminationOfTaxableIncomeSpecialCases { get; set; }
        public object ShowNotes { get; set; }
        public object ShowManagementReport { get; set; }
        public object ShowIncomeStatement { get; set; }


        public object EnableBalanceSheet { get; set; }
        public object EnableAppropriationProfit { get; set; }
        public object EnableContingentLiabilities { get; set; }
        //public object EnableChangesEquityAccounts { get; set; }
        //public object EnableChangesEquityStatement { get; set; }
        public object EnableCashFlowStatement { get; set; }
        //public object EnableNotes { get; set; }
        //public object EnableManagementReport { get; set; }
        //public object EnableTransfersCommercialCodeToTax { get; set; }
        //public object EnableReportElements { get; set; }
        //public object EnableDetailedInformation { get; set; }
        public object EnableAdjustmentOfIncome { get; set; }
        public object EnableDeterminationOfTaxableIncome { get; set; }
        public object EnableDeterminationOfTaxableIncomeBusinessPartnership { get; set; }
        public object EnableDeterminationOfTaxableIncomeSpecialCases { get; set; }
        public object EnableNotes { get; set; }
        public object EnableManagementReport { get; set; }
        public object EnableIncomeStatement { get; set; }

        


        #endregion


    }

}
