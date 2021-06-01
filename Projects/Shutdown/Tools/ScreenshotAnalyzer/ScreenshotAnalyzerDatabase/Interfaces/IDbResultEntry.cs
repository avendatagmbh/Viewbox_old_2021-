// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-13
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScreenshotAnalyzerDatabase.Interfaces {
    public interface IDbResultEntry {
        string EditedValue { get; set; }
        string RecognizedText { get; set; }
        int OcrRectangleId { get; }
        int ScreenshotId { get; }
    }
}
