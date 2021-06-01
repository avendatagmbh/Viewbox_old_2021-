// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-11-22
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ScreenshotAnalyzerBusiness.Manager;
using ScreenshotAnalyzerBusiness.Structures.History;
using ScreenshotAnalyzerDatabase.Interfaces;
using Utils;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace ScreenshotAnalyzerBusiness.Structures {
    public class Screenshot : NotifyPropertyChangedBase{
        public Screenshot(string path, IDbScreenshot dbScreenshot, bool loadRectangles) {
            DbScreenshot = dbScreenshot;
            Path = path;
            Rectangles = new ObservableCollectionAsync<OcrRectangle>();
            OcrRectangleRegistry.RegisterScreenshot(this, loadRectangles);
            RectangleHistory = new RectangleHistory(this);
        }

        #region Properties

        internal IDbScreenshot DbScreenshot { get; set; }
        public string Path {
            get { return DbScreenshot.Path; }
            set { DbScreenshot.Path = value; }
        }

        public string DisplayString { get { return new FileInfo(Path).Name; } }
        public ObservableCollectionAsync<OcrRectangle> Rectangles { get; private set; }

        #region Anchor
        private OcrRectangle _anchor;
        public OcrRectangle Anchor {
            get { return _anchor; }
            set {
                if (_anchor != value) {
                    _anchor = value;
                    AnchorRemained = false;
                    OnPropertyChanged("Anchor");
                }
            }
        }
        #endregion Anchor

        public bool AnchorRemained { get; set; }

        private Size _imageSize;
        private bool _imageSizeInitialized = false;
        public Size ImageSize {
            get {
                if (!_imageSizeInitialized) {
                    Image temp = Bitmap.FromFile(Path);
                    _imageSize = new Size(temp.Width, temp.Height);
                    _imageSizeInitialized = true;
                }
                return _imageSize;
            }
        }

        public string Filename {
            get { return new FileInfo(Path).Name; }
        }

        public RectangleHistory RectangleHistory { get; private set; }
        #endregion

        public void AddRectangle(Point dragStartPosition, Point dragEndPosition) {
            OcrRectangle ocrRect = new OcrRectangle(dragStartPosition, dragEndPosition);
            Rectangles.Add(ocrRect);
            RectangleHistory.RectangleAdded(ocrRect);
            //String result = Marshal.PtrToStringAnsi(OCRpart(Path, -1, (int) ocrRect.UpperLeft.X, (int) ocrRect.UpperLeft.Y, (int)ocrRect.Width, (int)ocrRect.Height));
            //Console.WriteLine("non-self: " + Environment.NewLine + result);
        }

        public void SetAnchor(Point dragStartPosition, Point dragEndPosition) {
            Anchor = new OcrRectangle(dragStartPosition, dragEndPosition);
        }

        public void DeleteRectangle(OcrRectangle rect) {
            RectangleHistory.DeletePending(rect);
            Rectangles.Remove(rect);
        }
    }
}
