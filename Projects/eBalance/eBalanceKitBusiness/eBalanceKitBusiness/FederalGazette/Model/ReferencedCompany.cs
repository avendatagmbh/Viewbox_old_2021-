// --------------------------------------------------------------------------------
// author: Solueman Hussain
// since: 2012-01-09
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace eBalanceKitBusiness.FederalGazette.Model {
    public class ReferencedCompany
    {
        #region properties
        public string Companytype { get; set; }
        public string CompanyName { get; set; }
        public string CompanyLocation { get; set; }
        public string LegalFormForeign { get; set; }
        public string Country { get; set; }
        public int? CompanyId { get; set; }
        #endregion
    }
}