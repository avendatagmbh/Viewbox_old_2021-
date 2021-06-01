// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-13
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScreenshotAnalyzerBusiness.Structures.History {
    public interface IRectangleAction {
        void RollbackAction(List<OcrRectangle> rectangles);
    }
}
