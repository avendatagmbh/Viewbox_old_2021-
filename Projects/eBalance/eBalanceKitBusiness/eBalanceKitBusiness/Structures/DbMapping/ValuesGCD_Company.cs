/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2011-02-09      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using XbrlProcessor.Taxonomy;
using eBalanceKitBusiness.Structures.Interfaces;

namespace eBalanceKitBusiness.Structures.DbMapping {

    [DbTable("values_gcd_company")]
    public class ValuesGCD_Company {

        [DbColumn("id", AllowDbNull = false, AutoIncrement = true)]
        [DbPrimaryKey]
        public int Id { get; set; }

        [DbColumn("company_id", AllowDbNull = true, IsInverseMapping = true)]
        public Document Company { get; set; }

        [DbColumn("parent_id", AllowDbNull = true, IsInverseMapping = true)]
        public ValuesGCD_Company Parent { get; set; }

        [DbColumn("xbrl_elem_id", AllowDbNull = true, Length = 128)]
        public string XbrlElementId { get; set; }

        [DbColumn("value", AllowDbNull = true, Length = 4096)]
        public string Value { get; set; }

        [DbCollection("Parent")]
        public List<ValuesGCD_Company> Children {
            get {
                if (_children == null) _children = new List<ValuesGCD_Company>();
                return _children;
            }
            set { _children = value; }
        }
        private List<ValuesGCD_Company> _children;
    }
}
