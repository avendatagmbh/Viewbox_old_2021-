// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-15
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ScreenshotAnalyzerBusiness.Structures.History {
    class RectangleMapping {
        private RectangleMapping() {
            _mappingTo = new List<int>();
            _mappingFrom = new List<int>();
        }

        public RectangleMapping(List<OcrRectangle> rectangles, RectangleHistory rectangleHistory) {
            _mappingTo = new List<int>(rectangles.Count);
            List<OcrRectangle> historyBeginning = RollbackHistory(rectangles, rectangleHistory);
            _mappingFrom = new int[historyBeginning.Count].ToList();
            for(int i = 0; i < _mappingFrom.Count; ++i) _mappingFrom[i] = -1;

            for (int i = 0; i < rectangles.Count; ++i ) {
                int found = historyBeginning.IndexOf(rectangles[i]);
                _mappingTo.Add(found);
                if (found != -1) _mappingFrom[found] = i;
            }
        }

        private readonly List<int> _mappingTo;
        private readonly List<int> _mappingFrom; 

        #region Methods
        private List<OcrRectangle> RollbackHistory(IEnumerable<OcrRectangle> rectangles, RectangleHistory rectangleHistory) {
            List<OcrRectangle> result = rectangles.ToList();
            for (int i = rectangleHistory.Actions.Count - 1; i >= 0; --i) {
                IRectangleAction action = rectangleHistory.Actions[i];
                action.RollbackAction(result);
            }
            return result;
        }

        private delegate int Mapping(int index);

        public RectangleMapping Combine(RectangleMapping rectangleMapping, bool otherMappingTo) {
            RectangleMapping mapping = new RectangleMapping();
            Mapping otherMappingFunc = rectangleMapping.MappingTo;
            if (!otherMappingTo) otherMappingFunc = rectangleMapping.MappingFrom;

            for (int i = 0; i < (otherMappingTo ? rectangleMapping._mappingFrom.Count : rectangleMapping._mappingTo.Count); ++i)
                mapping._mappingFrom.Add(-1);

            for (int i = 0; i < _mappingFrom.Count; ++i) {
                if (MappingTo(i) == -1) mapping._mappingTo.Add(-1);
                else {
                    int index = otherMappingFunc(MappingTo(i));
                    mapping._mappingTo.Add(index);
                    if (index != -1) mapping._mappingFrom[index] = i;
                }
            }

            return mapping;
        }

        public int MappingTo(int index) {
            if (index < 0 || index >= _mappingTo.Count) {
                return -1;
            }
            return _mappingTo[index];
        }

        public int MappingFrom(int index) {
            if (index < 0 || index >= _mappingFrom.Count) {
                return -1;
            }
            return _mappingFrom[index];
        }
        #endregion Methods
    }
}
