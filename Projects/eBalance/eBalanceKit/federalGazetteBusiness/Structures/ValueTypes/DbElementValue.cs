// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-12-15
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using DbAccess;

namespace federalGazetteBusiness.Structures.ValueTypes {
    [DbTable("federalGazetteElementValues", ForceInnoDb = true)]
    public class DbElementValue {
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public int Id { get; set; }

        [DbColumn("document_id", AllowDbNull = false)]
        public int DocumentId { get; set; }

        [DbColumn("name", AllowDbNull = false)]
        public string ElementName { get; set; }
        
        [DbColumn("value", AllowDbNull = true)]
        public string ElementValue { get; set; }

        [DbColumn("parameter_type", AllowDbNull = true)]
        public string ParameterType { get; set; }
    }
}