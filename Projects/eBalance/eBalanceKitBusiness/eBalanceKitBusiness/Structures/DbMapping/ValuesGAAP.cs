/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2011-02-09      initial implementation
 *************************************************************************************************************/

using System;
using DbAccess;
using eBalanceKitBusiness.Structures.Interfaces;
using XbrlProcessor.Taxonomy;
using XbrlProcessor.Taxonomy.XbrlElementValue;

namespace eBalanceKitBusiness.Structures.DbMapping {

    [DbTable("values_gaap")]
    public class ValuesGAAP : ValuesBase<ValuesGAAP>, IValueMapping {

        /// <summary>
        /// Inits the taxonomy entries.
        /// </summary>
        public static void InitTaxonomyEntries(Taxonomy taxonomy) {
            try {
                taxonomy.DefineAsList("kke.unlimitedPartners");
                taxonomy.DefineAsList("kke.unlimitedPartners.sumEquityAccounts.kinds");
                taxonomy.SetElementType("kke.unlimitedPartners.sumEquityAccounts.kinds.name", XbrlElementValueTypes.SingleChoice);

                taxonomy.DefineAsList("kke.limitedPartners");
                taxonomy.DefineAsList("kke.limitedPartners.sumEquityAccounts.kinds");

                taxonomy.DefineAsList("nt.ass.DRS12_24");
                
                taxonomy.DefineAsList("nt.particip.listRow");
                
                taxonomy.DefineAsList("nt.genInfo.DRS13_29");
                taxonomy.DefineAsList("nt.genInfo.DRS13_32");
                taxonomy.DefineAsList("nt.genInfo.DRS10_41");
                taxonomy.DefineAsList("nt.genInfo.derivatetFinancialinstruments");
                taxonomy.DefineAsList("nt.genInfo.FinancialInstrFinAssMas");

                taxonomy.DefineAsList("nt.relationships.salariesActiveMembers.BoardsSpecification.personal");
                taxonomy.DefineAsList("nt.relationships.salariesFormerMembers.BoardsSpecification.personal");
                taxonomy.DefineAsList("nt.relationships.ShareholdersKapCoSpec");

                taxonomy.DefineAsList("nt.segmGeographical");
                taxonomy.DefineAsList("nt.segmGeographical.OtherImportant");
                taxonomy.DefineAsList("nt.segmGeographical.Business");
                taxonomy.DefineAsList("nt.segmGeographical.ratio");

                taxonomy.DefineAsList("nt.segmBusiness");
                taxonomy.DefineAsList("nt.segmBusiness.OtherImportant");
                taxonomy.DefineAsList("nt.segmBusiness.geographical");
                taxonomy.DefineAsList("nt.segmBusiness.ratio");

                taxonomy.DefineAsList("hbst.transfer");
                taxonomy.DefineAsList("hbst.transfer.bsAss");
                taxonomy.DefineAsList("hbst.transfer.bsEqLiab");
                taxonomy.DefineAsList("hbst.transfer.isChangeNetIncome");
                taxonomy.DefineAsList("hbst.accountbalances");

                taxonomy.DefineAsList("bs.eqLiab.accruals.pensions.ast");
                taxonomy.DefineAsList("bs.eqLiab.equity.subscribed.limitedLiablePartners.details");
                taxonomy.DefineAsList("bs.eqLiab.equity.subscribed.unlimitedLiablePartners.details");

                taxonomy.SetElementType("hbst.transfer.bsAss.name", XbrlElementValueTypes.HbstElementName);
                taxonomy.SetElementType("hbst.transfer.bsEqLiab.name", XbrlElementValueTypes.HbstElementName);
                taxonomy.SetElementType("hbst.transfer.isChangeNetIncome.name", XbrlElementValueTypes.HbstElementName);

            } catch (Exception ex) {
                global::System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}
