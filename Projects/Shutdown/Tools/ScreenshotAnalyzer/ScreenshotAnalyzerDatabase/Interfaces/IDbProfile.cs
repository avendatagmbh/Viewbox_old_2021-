// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-08
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using DbAccess;
using DbAccess.Structures;

namespace ScreenshotAnalyzerDatabase.Interfaces {
    public interface IDbProfile  {
        string Name { get; set; }
        string Description { get; set; }
        DbConfig DbConfig { get;  }
        bool DatabaseTooOld { get; }
        bool DatabaseTooNew { get; }
        List<IDbTable> Tables { get; }
        string AccessPath { get; set; }

        void Save();
        void UpdateDatabase();
        IDatabase GetOpenConnection();
        void Load();
    }
}
