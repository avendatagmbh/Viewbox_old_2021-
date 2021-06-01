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
    public class RectangleDeleteAction : IRectangleAction {
        public RectangleDeleteAction(OcrRectangle ocrRect, int index) {
            _deletedAt = index;
            _ocrRectangle = ocrRect;
        }

        private int _deletedAt;
        private OcrRectangle _ocrRectangle;

        public void RollbackAction(List<OcrRectangle> rectangles) {
            rectangles.Insert(_deletedAt, _ocrRectangle);
        }
    }
}
