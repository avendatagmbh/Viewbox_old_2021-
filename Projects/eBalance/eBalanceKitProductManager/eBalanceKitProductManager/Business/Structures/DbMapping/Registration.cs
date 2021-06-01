using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;

namespace eBalanceKitProductManager.Business.Structures.DbMapping {

    [DbTable("registrations")]
    public class Registration {

        [DbColumn("id", AllowDbNull = false, AutoIncrement = true)]
        [DbPrimaryKey]
        public int Id { get; set; }

        [DbColumn("serial_number_id")]
        public Instance SerialNumber { get; set; }

        [DbColumn("key", AllowDbNull = false, Length = 25)]
        public string Key { get; set; }

        [DbColumn("registrator", AllowDbNull = false, Length = 5)]
        public string Registrator { get; set; }

        [DbColumn("timestamp", AllowDbNull = false)]
        public DateTime Timestamp { get; set; }
    }
}
