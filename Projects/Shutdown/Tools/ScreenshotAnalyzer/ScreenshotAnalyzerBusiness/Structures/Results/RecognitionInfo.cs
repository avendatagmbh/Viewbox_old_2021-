// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-08
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;

namespace ScreenshotAnalyzerBusiness.Structures.Results {
    public class RecognitionInfo {
        public RecognitionInfo(ResultRowEntry resultRowEntry, Image fullImage) {
            ResultRowEntry = resultRowEntry;
            double margin = 0;
            Image = Images.ImageFromRectangle(fullImage, 
                new OcrRectangle(
                    new Point(resultRowEntry.Rectangle.UpperLeft.X-margin, resultRowEntry.Rectangle.UpperLeft.Y-margin),
                    new Point(resultRowEntry.Rectangle.LowerRight.X + margin, resultRowEntry.Rectangle.LowerRight.Y + margin)
                    )
                ).ToBitmapSource();
        }

        #region Properties
        public ResultRowEntry ResultRowEntry { get; set; }
        public BitmapSource Image { get; set; }
        #endregion
    }
}
