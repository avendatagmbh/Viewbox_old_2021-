/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2011-02-13      initial implementation
 *************************************************************************************************************/

using System;
using DbAccess;
using eBalanceKitBusiness.Structures.Interfaces;
using XbrlProcessor.Taxonomy;
using XbrlProcessor.Taxonomy.PresentationTree;
using XbrlProcessor.Taxonomy.XbrlElementValue;

namespace eBalanceKitBusiness.Structures.DbMapping {

    [DbTable("values_gcd")]
    public class ValuesGCD : ValuesBase<ValuesGCD>, IValueMapping {

        /// <summary>
        /// Inits the taxonomy entries.
        /// </summary>
        public static void InitTaxonomyEntries(Taxonomy taxonomy) {
            try {
                taxonomy.DefineAsList("genInfo.doc.author");
                taxonomy.SetElementType("genInfo.report.id.reportElement", XbrlElementValueTypes.MultipleChoice);
                taxonomy.SetElementType("genInfo.report.preparationAttestation.attester.type", XbrlElementValueTypes.SingleChoice);

                taxonomy.DefineAsList("genInfo.company.id.contactAddress");
                taxonomy.DefineAsList("genInfo.company.id.shareholder");

                SetNonPersistantValues(taxonomy.ElementsByName["genInfo.company"].PresentationTreeNodes[0], typeof(ValuesGCD_Company));

                // The following elements and subelements of these are not included in the value tree.
                // The period values could be received from the Document.FinancialYear property.
                taxonomy.ElementsByName["genInfo.report.period.fiscalYearBegin"].IsPersistant = false;
                taxonomy.ElementsByName["genInfo.report.period.fiscalYearEnd"].IsPersistant = false;
                taxonomy.ElementsByName["genInfo.report.period.balSheetClosingDate"].IsPersistant = false;
                taxonomy.ElementsByName["genInfo.report.period.fiscalPreciousYearBegin"].IsPersistant = false;
                taxonomy.ElementsByName["genInfo.report.period.fiscalPreciousYearEnd"].IsPersistant = false;
                taxonomy.ElementsByName["genInfo.report.period.balSheetClosingDatePreviousYear"].IsPersistant = false;


            } catch (Exception ex) {
                throw new Exception("Init values gcd: " + ex.Message);
            }
        }

        private static void SetNonPersistantValues(IPresentationTreeNode root, Type tableType) {

            if (root is PresentationTreeNode) {
                (root as PresentationTreeNode).Element.TableType = tableType;
                foreach (var child in root.Children) {
                    SetNonPersistantValues(child, tableType);
                }
            }
        }
    }
}
