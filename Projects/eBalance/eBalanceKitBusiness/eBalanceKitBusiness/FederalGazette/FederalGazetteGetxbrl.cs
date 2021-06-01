// --------------------------------------------------------------------------------
// author: Solueman Hussain
// since: 2012-01-13
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Text;
using eBalanceKitBusiness.Export;
using eBalanceKitBusiness.FederalGazette.Model;
using eFederalGazette;

namespace eBalanceKitBusiness.FederalGazette {
    public class FederalGazetteGetXbrl {
        public FederalGazetteGetXbrl(FederalGazetteMainModel model) {
            Model = model;
            _xbrlExportAnnual = new FederalGazetteXbrlExport(Model);
        }


        #region Locals
        private FederalGazetteMainModel Model { get; set; }
        private FederalGazetteXbrlExport _xbrlExportAnnual;

        #endregion

        #region GetBalanceSheet
        public StringBuilder GetBalanceSheet() { return _xbrlExportAnnual.ExportXmlAnnualStatement(null, true, "bs"); }
        #endregion

        #region GetIncomeStatement
        public StringBuilder GetIncomeStatement() { return _xbrlExportAnnual.ExportXmlAnnualStatement(null, true, "is"); }
        #endregion

        #region GetNetProfit
        public StringBuilder GetNetProfit() { return _xbrlExportAnnual.ExportXmlAnnualStatement(null, true, "incomeUse"); }
        #endregion

        #region GetNotes
        public StringBuilder GetNotes() { return _xbrlExportAnnual.ExportXmlAnnualStatement(null, true, "nt"); }
        #endregion

        #region GetFixedAssets
        public StringBuilder GetFixedAssets() { return _xbrlExportAnnual.ExportXmlAnnualStatement(null, true, "nt.ass"); }
        #endregion

        #region GetManagementReport
        public StringBuilder GetManagementReport() { return _xbrlExportAnnual.ExportXmlAnnualStatement(null, true, "mgmtRep"); }
        #endregion

        #region GetAllAccounts
        public StringBuilder GetAllAccounts() {
            var accounts = new List<string>();
            accounts.Add("bs");
            accounts.Add("is");
            accounts.Add("incomeUse");
            accounts.Add("nt");
            accounts.Add("nt.ass");
            accounts.Add("mgmtRep");
            return _xbrlExportAnnual.ExportXmlAnnualStatement(null, true, accounts);
        }
        #endregion
    }
}