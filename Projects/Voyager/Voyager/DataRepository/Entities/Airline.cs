using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataRepository.Entities {
   public class Airline {

       private const string TABLE_NAME = "AIRLINE";
       private const string ID = "ID";
       private const string NAME = "NAME";

       /********************************************************************/
       #region properties

       public int AirlineID { get; private set; }
       public string Name { get; private set; }

       #endregion properties

       /*******************************************************************/

   }
}
