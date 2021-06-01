// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-09
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;

namespace ScreenshotAnalyzerDatabase.Interfaces {
    public interface IDbScreenshotGroup {
        string Name { get; set; }
        string Comment { get; set; }
        List<IDbScreenshot> Screenshots { get; }
        void Delete();
    }

}
