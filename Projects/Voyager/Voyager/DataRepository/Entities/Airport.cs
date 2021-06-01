using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessDomain.Entities.Tables;

namespace DataRepository.Entities {
    public class Airport {

        private const string TABLE_NAME = "AIRPORT";
        private const string ID = "ID";
        private const string NAME = "NAME";
        private const string IATACODE = "IATACODE";
        private const string INFO = "INFO";


        /**************************************************************************/
        #region properties

        public uint AirportID { get; set; }   // PK+FK(address-class)
        public string Name { get; set; }        //varchar(100)
        public string IATACode { get; set; }    //varchar(5)
        public string Info { get; set; }        // varchar(255)

        #endregion properties
    }
}
