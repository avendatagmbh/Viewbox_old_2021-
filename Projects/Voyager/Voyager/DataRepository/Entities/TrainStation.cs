using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataRepository.Entities {
   public class TrainStation {

       private const string TABLE_NAME = "TRAINSTATION";
       private const string ID = "ID";
       private const string NAME = "NAME";
       private const string INFO = "INFO";

       /***********************************************************************/
       #region properties

       public int TrainStationID { get; set; }      //PK+FK(address
       public string Name { get; set; }         //varchar (100)
       public string Info { get; set; }         // varchar(255)

       #endregion properties
   }
}
