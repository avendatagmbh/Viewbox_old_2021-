// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-13
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using ScreenshotAnalyzerDatabase.Config;

namespace ScreenshotAnalyzerDatabase.Interfaces {
    public interface IDbResult {
        DateTime CreatedOn { get; set; }
        List<IDbResultColumn> ResultColumns { get; }
        List<IDbResultEntry> ResultEntries { get; }
        void Save();
    }
}
