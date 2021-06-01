// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-08
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.ComponentModel;
using System.Runtime.Serialization;
using DbAccess;

namespace DbSearchDatabase.Structures {
    [DataContract]
    public class DatabaseObjectBase<T> {
        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        [DataMember(Name="Id")]
        public T Id { get; set; }
        #endregion
    }
}
