// --------------------------------------------------------------------------------
// author: Solueman Hussain
// since: 2012-01-06
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Text;

namespace eBalanceKitBusiness.FederalGazette.Model {
    public class FederalGazetteFiles
    {
        #region properties
        
        public string FileContentDescription { get; set; }
        public int? FileAnnualContentType { get; set; }
        public string FileName { get; set; }
        public StringBuilder FileContent { get; set; }
        #endregion
    }
}