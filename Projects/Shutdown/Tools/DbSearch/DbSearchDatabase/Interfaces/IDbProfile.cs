// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-08
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using DbAccess.Structures;
using System.Collections.Generic;

namespace DbSearchDatabase.Interfaces {
    public interface IDbProfile  {
        string Name { get; set; }
        string Description { get; set; }
        DbConfig DbConfigView { get;  }
        bool IsLoaded { get; }
        string CustomRules { get; set; }
        bool DatabaseTooOld { get; }
        bool DatabaseTooNew { get; }

        DbConfig GetProfileConfig();
                
        void Save();
        void UpdateDatabase();
    }
}
