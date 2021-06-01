// --------------------------------------------------------------------------------
// author: Solueman Hussain
// since: 2012-01-06
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace eBalanceKitBusiness.FederalGazette.Model {
    public class CompanyList
    {
        #region properties
        
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public bool Delete { get; set; }
        public string Domicile { get; set; }
        public string LegalForm { get; set; }
        public string RegisterCourt { get; set; }
        public string RegisterType { get; set; }
        public string RegisterNumber { get; set; }
        #endregion
    }
}