// --------------------------------------------------------------------------------
// author: Solueman Hussain
// since: 2012-01-09
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace eBalanceKitBusiness.FederalGazette.Model {
    public class FederalGazetteOrder
    {
        #region properties
        
        public string ClientNumber { get; set; }
        public string CompanyName { get; set; }
        public DateTime CreateDate { get; set; }
        public string OrderNumber { get; set; }
        public int PublicationCategory { get; set; }
        public int PublicationType { get; set; }
        public string Sign { get; set; }
        public string OrderStatus { get; set; }
        public List<DateEntry>DateList { get; set; }
        #endregion

    }
    public class DateEntry
    {
        #region properties
        
        public DateTime Date { get; set; }
        public int DateType { get; set; }
        #endregion
    }
}