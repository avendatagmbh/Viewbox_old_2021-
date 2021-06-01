/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2011-4-26      initial implementation (based on DbAccess 1.0)
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBalanceKitBusiness.Structures {
    
    public class EricResultMessage {

        public string Id { get; set; }
        public string FK { get; set; }
        public string USB { get; set; }
        public string MZI { get; set; }
        public string VD { get; set; }
        public string PK { get; set; }
        public string RId { get; set; }
        public string EId { get; set; }
        public string Text { get; set; }
    }
}
