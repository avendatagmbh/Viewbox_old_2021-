// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-09
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScreenshotAnalyzerDatabase.Interfaces {
    public interface IDbScreenshot {
        string Path { get; set; }
        int Id { get; }
        void Save();
        void Delete();
    }
}
