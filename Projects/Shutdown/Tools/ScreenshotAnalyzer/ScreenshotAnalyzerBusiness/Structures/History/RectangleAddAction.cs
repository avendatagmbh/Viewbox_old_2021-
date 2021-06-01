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
    class RectangleAddAction : IRectangleAction{
        public RectangleAddAction(OcrRectangle ocrRect, int count) {
            _ocrRectangle = ocrRect;
            _position = count - 1;
        }

        #region Properties
        private int _position;
        private OcrRectangle _ocrRectangle;

        #endregion Properties

        #region Implementation of IRectangleAction

        public void RollbackAction(List<OcrRectangle> rectangles) {
            rectangles.RemoveAt(_position);
        }

        #endregion
    }
}
