// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-08
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;

namespace DbSearchDatabase.Interfaces {
    public interface IDatabaseObject<T> {
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        T Id { get; }
    }
}
