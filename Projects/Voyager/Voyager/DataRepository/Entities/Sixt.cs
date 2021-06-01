using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataRepository.Entities {
   public class Sixt {

       private const string TABLE_NAME = "SIXT";
       private const string ID = "ID";
       private const string STATIONNUMBER = "STATIONNUMBER";
       private const string GDSCODE = "GDSCODE";

       /**********************************************************************/
       #region properties

       public uint SixtID { get; set; }         //PK+FK(address-class)
       public uint StationNumber { get; set; }
       public string GDS_Code { get; set; }     //varchar(10)

       #endregion properties
   }
}
