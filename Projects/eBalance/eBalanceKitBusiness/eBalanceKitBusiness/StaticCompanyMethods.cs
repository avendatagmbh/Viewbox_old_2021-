// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-03-27
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.XbrlElementValue;

namespace eBalanceKitBusiness {
    public static class StaticCompanyMethods {

        /// <summary>
        /// Bankentaxonomie - Checks if the current report is RechKredV (Branche) - [de-gcd_genInfo.report.id.specialAccountingStandard as XbrlElementValue_SingleChoice]
        /// </summary>
        public static bool IsRKV(Document document) { return document.IsRKV; }

        /// <summary>
        /// Versicherungstaxonomie - Checks if the current report is RechVersV (Branche) - [de-gcd_genInfo.report.id.specialAccountingStandard as XbrlElementValue_SingleChoice]
        /// </summary>
        public static bool IsRVV(Document document) { return document.IsRVV; }

        /// <summary>
        /// Checks if the company is a so called "Personengesellschaft" (OHG, KG, GKG, AGKG, EWI, GBR, PG, MUN)
        /// </summary>
        public static bool IsPersonenGesellschaft(Document document) { return document.IsPartnership; }

        /// <summary>
        /// Checks if the company is a so called "Einzelunternehmer" 
        /// </summary>
        /// <ToDo>Apply the new specification (available 11/2012)</ToDo>
        public static bool IsEinzelunternehmer(Document document) { return document.IsSoleProprietorship; }
        
        /// <summary>
        /// Checks if the company is a so called "Körperschaft" 
        /// </summary>
        public static bool IsKoerperschaft(Document document) { return document.IsCorporation; }

        /// <summary>
        /// Checks if the income statement format is GKV (de-gcd_genInfo.report.id.incomeStatementFormat)
        /// </summary>
        public static bool IsTypeOperatingResultGKV(Document document) { return document.IsTypeOperatingResultTC; }
        
        /// <summary>
        /// Checks if the income statement format is UKV (de-gcd_genInfo.report.id.incomeStatementFormat)
        /// </summary>
        public static bool IsTypeOperatingResultUKV(Document document) { return document.IsTypeOperatingResultCoS; }

        /// <summary>
        /// Checks if the income statement format is not set (de-gcd_genInfo.report.id.incomeStatementFormat)
        /// </summary>
        public static bool IsTypeOperatingResultNotSelected(Document document) { return document.IsTypeOperatingResultNotSelected; }

    }
}