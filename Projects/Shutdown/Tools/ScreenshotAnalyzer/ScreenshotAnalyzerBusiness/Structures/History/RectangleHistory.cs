// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-13
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;

namespace ScreenshotAnalyzerBusiness.Structures.History {
    public class RectangleHistory {
        public RectangleHistory(Screenshot screenshot) {
            Screenshot = screenshot;
        }
        #region Properties



        internal Screenshot Screenshot { get; private set; }

        private List<IRectangleAction> _actions = new List<IRectangleAction>();
        public List<IRectangleAction> Actions {
            get { return _actions; }
        }

        #endregion Properties

        #region Methods
        public void SetRectangles(Screenshot refScreenshot, Screenshot screenshot, double correctionX, double correctionY) {
            
        }
        #endregion Methods

        public void RectangleAdded(OcrRectangle ocrRect) {
            _actions.Add(new RectangleAddAction(ocrRect, Screenshot.Rectangles.Count));
        }

        public void DeletePending(OcrRectangle ocrRect) {
            _actions.Add(new RectangleDeleteAction(ocrRect, Screenshot.Rectangles.IndexOf(ocrRect)));
        }

        public void Clear() {
            _actions.Clear();
        }
    }
}
