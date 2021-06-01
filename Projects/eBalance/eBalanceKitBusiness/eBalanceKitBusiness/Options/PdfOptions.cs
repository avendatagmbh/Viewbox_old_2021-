using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using DbAccess;
using eBalanceKitBusiness.Export;

namespace eBalanceKitBusiness.Options
{
    internal class PdfOptions
    {
        internal PdfOptions(AvailableOptions options) {
            _options = options;
        }

        private readonly AvailableOptions _options;
        /// <summary>
        /// Check the database if a value is set in the pdf export options. If something is set, the given config will be
        /// modified.
        /// </summary>
        /// <param name="config">the config that have to be modified</param>
        public void GetPdfExportOptionsFromDatabase(IExportConfig config) {
            bool? boolValue = _options.GetBoolValue("ExportNotes");
            if (boolValue != null)
                config.ExportNotes = boolValue.Value;
            //boolValue = _options.GetBoolValue("ExportAsXbrl");
            //if (boolValue != null)
            //    config.ExportAsXbrl = boolValue.Value;
            boolValue = _options.GetBoolValue("ExportAccountBalances");
            if (boolValue != null)
                config.ExportAccountBalances = boolValue.Value;
            boolValue = _options.GetBoolValue("ExportAccounts");
            if (boolValue != null)
                config.ExportAccounts = boolValue.Value;
            boolValue = _options.GetBoolValue("ExportAdjustmentOfIncome");
            if (boolValue != null)
                config.ExportAdjustmentOfIncome = boolValue.Value;
            boolValue = _options.GetBoolValue("ExportAppropriationProfit");
            if (boolValue != null)
                config.ExportAppropriationProfit = boolValue.Value;
            boolValue = _options.GetBoolValue("ExportBalanceSheet");
            if (boolValue != null)
                config.ExportBalanceSheet = boolValue.Value;
            boolValue = _options.GetBoolValue("ExportCashFlowStatement");
            if (boolValue != null)
                config.ExportCashFlowStatement = boolValue.Value;
            boolValue = _options.GetBoolValue("ExportComments");
            if (boolValue != null)
                config.ExportComments = boolValue.Value;
            boolValue = _options.GetBoolValue("ExportCompanyInformation");
            if (boolValue != null)
                config.ExportCompanyInformation = boolValue.Value;
            boolValue = _options.GetBoolValue("ExportContingentLiabilities");
            if (boolValue != null)
                config.ExportContingentLiabilities = boolValue.Value;
            boolValue = _options.GetBoolValue("ExportDeterminationOfTaxableIncome");
            if (boolValue != null)
                config.ExportDeterminationOfTaxableIncome = boolValue.Value;
            boolValue = _options.GetBoolValue("ExportDeterminationOfTaxableIncomeBusinessPartnership");
            if (boolValue != null)
                config.ExportDeterminationOfTaxableIncomeBusinessPartnership = boolValue.Value;
            boolValue = _options.GetBoolValue("ExportDeterminationOfTaxableIncomeSpecialCases");
            if (boolValue != null)
                config.ExportDeterminationOfTaxableIncomeSpecialCases = boolValue.Value;
            boolValue = _options.GetBoolValue("ExportDocumentInformation");
            if (boolValue != null)
                config.ExportDocumentInformation = boolValue.Value;
            boolValue = _options.GetBoolValue("ExportEquityStatement");
            if (boolValue != null)
                config.ExportEquityStatement = boolValue.Value;
            boolValue = _options.GetBoolValue("ExportIncomeStatement");
            if (boolValue != null)
                config.ExportIncomeStatement = boolValue.Value;
            boolValue = _options.GetBoolValue("ExportManagementReport");
            if (boolValue != null)
                config.ExportManagementReport = boolValue.Value;
            boolValue = _options.GetBoolValue("ExportMandatoryOnly");
            if (boolValue != null)
                config.ExportMandatoryOnly = boolValue.Value;
            boolValue = _options.GetBoolValue("ExportNILValues");
            if (boolValue != null)
                config.ExportNILValues = boolValue.Value;
            boolValue = _options.GetBoolValue("ExportNtAssGross");
            if (boolValue != null)
                config.ExportNtAssGross = boolValue.Value;
            boolValue = _options.GetBoolValue("ExportNtAssGrossShort");
            if (boolValue != null)
                config.ExportNtAssGrossShort = boolValue.Value;
            boolValue = _options.GetBoolValue("ExportNtAssNet");
            if (boolValue != null)
                config.ExportNtAssNet = boolValue.Value;
            boolValue = _options.GetBoolValue("ExportReconciliationInfo");
            if (boolValue != null)
                config.ExportReconciliationInfo = boolValue.Value;
            boolValue = _options.GetBoolValue("ExportReportInformation");
            if (boolValue != null)
                config.ExportReportInformation = boolValue.Value;
            boolValue = _options.GetBoolValue("ExportUnassignedAccounts");
            if (boolValue != null)
                config.ExportUnassignedAccounts = boolValue.Value;
            boolValue = _options.GetBoolValue("ExportOtherReportElements");
            if (boolValue != null)
                config.ExportOtherReportElements = boolValue.Value;
        }

        /// <summary>
        /// Set the value of the pdf export option.
        /// Only accept changes from the implemented options.
        /// </summary>
        /// <param name="config">the saved config</param>
        public void SetPdfExportOptionsValue(IExportConfig config) {
            //_options.SetValue("ExportAsXbrl", config.ExportAsXbrl);
            _options.SetValue("ExportNILValues", config.ExportNILValues);
            _options.SetValue("ExportAccountBalances", config.ExportAccountBalances);
            _options.SetValue("ExportAccounts", config.ExportAccounts);
            _options.SetValue("ExportAdjustmentOfIncome", config.ExportAdjustmentOfIncome);
            _options.SetValue("ExportAppropriationProfit", config.ExportAppropriationProfit);
            _options.SetValue("ExportBalanceSheet", config.ExportBalanceSheet);
            _options.SetValue("ExportCashFlowStatement", config.ExportCashFlowStatement);
            _options.SetValue("ExportComments", config.ExportComments);
            _options.SetValue("ExportCompanyInformation", config.ExportCompanyInformation);
            _options.SetValue("ExportContingentLiabilities", config.ExportContingentLiabilities);
            _options.SetValue("ExportDeterminationOfTaxableIncome", config.ExportDeterminationOfTaxableIncome);
            _options.SetValue("ExportDeterminationOfTaxableIncomeBusinessPartnership",
                     config.ExportDeterminationOfTaxableIncomeBusinessPartnership);
            _options.SetValue("ExportDeterminationOfTaxableIncomeSpecialCases",
                     config.ExportDeterminationOfTaxableIncomeSpecialCases);
            _options.SetValue("ExportDocumentInformation", config.ExportDocumentInformation);
            _options.SetValue("ExportEquityStatement", config.ExportEquityStatement);
            _options.SetValue("ExportIncomeStatement", config.ExportIncomeStatement);
            _options.SetValue("ExportManagementReport", config.ExportManagementReport);
            _options.SetValue("ExportMandatoryOnly", config.ExportMandatoryOnly);
            _options.SetValue("ExportNotes", config.ExportNotes);
            _options.SetValue("ExportNtAssGross", config.ExportNtAssGross);
            _options.SetValue("ExportNtAssGrossShort", config.ExportNtAssGrossShort);
            _options.SetValue("ExportNtAssNet", config.ExportNtAssNet);
            _options.SetValue("ExportReconciliationInfo", config.ExportReconciliationInfo);
            _options.SetValue("ExportReportInformation", config.ExportReportInformation);
            _options.SetValue("ExportUnassignedAccounts", config.ExportUnassignedAccounts);
            _options.SetValue("ExportOtherReportElements", config.ExportOtherReportElements);
            _options.SaveConfiguration();
        }
    }
}
