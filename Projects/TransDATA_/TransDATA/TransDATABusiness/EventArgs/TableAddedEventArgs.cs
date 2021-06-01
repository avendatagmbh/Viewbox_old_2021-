using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TransDATABusiness.ConfigDb.Tables;


namespace TransDATABusiness.EventArgs {
 
    public class TableAddedEventArgs : System.EventArgs {

        public Table Table { get; set; }
    }
}
