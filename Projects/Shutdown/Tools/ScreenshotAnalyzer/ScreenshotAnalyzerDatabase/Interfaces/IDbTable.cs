// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-07
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using ScreenshotAnalyzerDatabase.Config;

namespace ScreenshotAnalyzerDatabase.Interfaces {
    public interface IDbTable {
        string Name { get; set; }
        string TableName { get; set; }
        string Comment { get; set; }
        List<IDbScreenshotGroup> ScreenshotGroups { get; }
        void Delete();
    }
}
