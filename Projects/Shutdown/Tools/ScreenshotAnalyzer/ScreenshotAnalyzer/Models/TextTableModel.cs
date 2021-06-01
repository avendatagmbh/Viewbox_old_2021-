using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScreenshotAnalyzerBusiness.Structures;
using ScreenshotAnalyzerBusiness.Structures.Results;

namespace ScreenshotAnalyzer.Models {
    public class TextTableModel {
        public TextTableModel(RecognitionResult recognitionResult) {
            RecognitionResult = recognitionResult;
        }

        #region Properties
        public RecognitionResult RecognitionResult { get; set; }
        #endregion Properties
    }
}
