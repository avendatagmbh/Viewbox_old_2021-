using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ScreenshotAnalyzerBusiness.Structures;
using ScreenshotAnalyzerDatabase.Config;
using ScreenshotAnalyzerDatabase.Factories;
using ScreenshotAnalyzerDatabase.Interfaces;
using Utils;

namespace ScreenshotAnalyzerBusiness.Manager {
    public static class OcrRectangleRegistry {
        static OcrRectangleRegistry() {
            ScreenshotToRects = new Dictionary<Screenshot, ScreenshotRectangles>();
        }

        public class ScreenshotRectangles {
            public ScreenshotRectangles() {
                RectToDbRect = new Dictionary<OcrRectangle, IDbOcrRectangle>();
            }

            public IDbOcrRectangle Anchor { get; set; }
            public Dictionary<OcrRectangle, IDbOcrRectangle> RectToDbRect { get; set; }
            //public bool DisableUpdate { get; set; }
        }

        #region Properties
        private static Dictionary<Screenshot, ScreenshotRectangles> ScreenshotToRects { get; set; }
        private static readonly Dictionary<ObservableCollectionAsync<OcrRectangle>, Screenshot> CollectionToScreenshot = new Dictionary<ObservableCollectionAsync<OcrRectangle>, Screenshot>();
        
        #endregion Properties

        #region Methods
        public static void RegisterScreenshot(Screenshot screenshot, bool loadRectangles) {
            ScreenshotRectangles screenshotRectangles = new ScreenshotRectangles();
            ScreenshotToRects[screenshot] = screenshotRectangles;

            //Load rectangles from database and create the appropriate OcrRectangles in the domain
            if (loadRectangles) {
                IEnumerable<IDbOcrRectangle> dbRectangles = DatabaseObjectFactory.LoadRectangles(screenshot.DbScreenshot);
                foreach (var dbRect in dbRectangles) {
                    switch(dbRect.Type) {
                        case RectType.Text:
                            OcrRectangle ocrRect = new OcrRectangle(dbRect.UpperLeft, dbRect.LowerRight);
                            screenshot.Rectangles.Add(ocrRect);
                            screenshotRectangles.RectToDbRect[ocrRect] = dbRect;
                            break;
                        case RectType.Anchor:
                            screenshot.Anchor = new OcrRectangle(dbRect.UpperLeft, dbRect.LowerRight);
                            screenshotRectangles.Anchor = dbRect;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            screenshot.PropertyChanged += screenshot_PropertyChanged;
            CollectionToScreenshot[screenshot.Rectangles] = screenshot;
            screenshot.Rectangles.CollectionChanged += Rectangles_CollectionChanged;
        }

        static void Rectangles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            Screenshot screenshot = CollectionToScreenshot[(ObservableCollectionAsync<OcrRectangle>) sender];
            ScreenshotRectangles rectangles = ScreenshotToRects[screenshot];
            //if (rectangles.DisableUpdate) return;
            switch(e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (OcrRectangle addedRect in e.NewItems) {
                        rectangles.RectToDbRect[addedRect] = DatabaseObjectFactory.CreateRectangle(screenshot.DbScreenshot, addedRect.UpperLeft, addedRect.LowerRight, RectType.Text );
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (OcrRectangle deletedRect in e.OldItems) {
                        rectangles.RectToDbRect[deletedRect].Delete();
                        rectangles.RectToDbRect.Remove(deletedRect);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var deletedRect in rectangles.RectToDbRect.Values)
                        deletedRect.Delete();
                    rectangles.RectToDbRect.Clear();
                    break;
                //case NotifyCollectionChangedAction.Replace:
                //case NotifyCollectionChangedAction.Move:
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        static void screenshot_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "Anchor") {
                Screenshot screenshot = (Screenshot) sender;
                ScreenshotRectangles screenshotRectangles = ScreenshotToRects[screenshot];

                if (screenshot.Anchor == null) {
                    //Need to delete this anchor from the database
                    if (screenshotRectangles.Anchor != null) screenshotRectangles.Anchor.Delete();
                }
                else {
                    if (screenshotRectangles.Anchor == null) {
                        screenshotRectangles.Anchor = DatabaseObjectFactory.CreateRectangle(screenshot.DbScreenshot,
                                                                                            screenshot.Anchor.UpperLeft,
                                                                                            screenshot.Anchor.LowerRight,
                                                                                            RectType.Anchor);
                    }
                    else {
                        try {
                            screenshotRectangles.Anchor.DisableSaving = true;
                            screenshotRectangles.Anchor.UpperLeft = screenshot.Anchor.UpperLeft;
                            screenshotRectangles.Anchor.DisableSaving = false;
                            screenshotRectangles.Anchor.LowerRight = screenshot.Anchor.LowerRight;
                        } finally {
                            screenshotRectangles.Anchor.DisableSaving = false;
                        }
                    }
                }
            }
        }

        public static Tuple<OcrRectangle,IDbOcrRectangle> GetRectangle(Screenshot screenshot, int ocrRectangleId) {
            ScreenshotRectangles screenshotRectangles = ScreenshotToRects[screenshot];
            foreach (var rectPair in screenshotRectangles.RectToDbRect)
                if (rectPair.Value.Id == ocrRectangleId)
                    return new Tuple<OcrRectangle,IDbOcrRectangle>(rectPair.Key, rectPair.Value);
            return null;
        }

        public static IDbOcrRectangle GetRectangle(Screenshot screenshot, OcrRectangle ocrRectangle) {
            ScreenshotRectangles screenshotRectangles = ScreenshotToRects[screenshot];
            return screenshotRectangles.RectToDbRect[ocrRectangle];
        }
        #endregion Methods

        public static void UpdateRectangle(Screenshot screenshot, OcrRectangle rect) {
            ScreenshotRectangles screenshotRectangles = ScreenshotToRects[screenshot];
            screenshotRectangles.RectToDbRect[rect].UpperLeft = rect.UpperLeft;
            screenshotRectangles.RectToDbRect[rect].LowerRight = rect.LowerRight;
            screenshotRectangles.RectToDbRect[rect].Save();
        }
    }
}
