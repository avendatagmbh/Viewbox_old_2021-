using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;

namespace eBalanceKitProductManager.Business.Structures.DbMapping {

    [DbTable("versions")]
    public class Version {

        [DbColumn("id", AllowDbNull = false, AutoIncrement = true)]
        [DbPrimaryKey]
        public int Id { get; set; }

        [DbColumn("version", AllowDbNull = false, Length = 16)]
        public string VersionId { get; set; }

        [DbColumn("since", AllowDbNull = false)]
        public DateTime Since { get; set; }

        [DbColumn("changelog", AllowDbNull = true, Length = 4096)]
        public string Changelog { get; set; }

        public string DisplayString {
            get { return "Version " + this.VersionId + " (seit " + Since.ToShortDateString() + ")"; }
        }
    }
}
