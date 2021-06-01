using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ScreenshotAnalyzer.Models;
using ScreenshotAnalyzerBusiness.Structures;
using ZoomAndPan;

namespace ScreenshotAnalyzer.Structures.SelectOcrAreasHelper {
    class EditModeSelectAndResizeRectangle : EditModeBase {
        public EditModeSelectAndResizeRectangle(UIElement control, ZoomAndPanControl zoomAndPanControl, SelectOcrAreasModel model, Canvas canvas) : base(control, zoomAndPanControl, model, canvas) {
        }

        private OcrRectangle _currentResizeRectangle = null;
        private OcrRectangle.NearestSegment _currentDragSegment;
        private Point _currentMovePoint;
        //private double _currentMovePointX, _currentMovePointY;
        private int _minimumDistance = 10;

        private OcrRectangle _selectedRect = null;
        private Brush _lastHighlightedBrush = null;

        public override void LeftMouseDown(MouseButtonEventArgs e) {
            base.LeftMouseDown(e);
            if (Model.CurrentEditMode == CurrentEditMode.SelectRectangle) {
                OcrRectangle minRect;
                OcrRectangle.NearestSegment minNearestSegment;
                var minDist = GetNearestRectangle(e, out minRect, out minNearestSegment);
                if (minRect != null && minRect.IsInside(Model.CanvasToImage(e.GetPosition(Canvas)))) {
                    Model.CurrentEditMode = CurrentEditMode.MoveRectangle;
                    _currentResizeRectangle = minRect;
                    _currentMovePoint = Model.CanvasToImage(e.GetPosition(Canvas));
                }
                else if (minDist < _minimumDistance) {
                    Model.CurrentEditMode = CurrentEditMode.ResizeRectangle;
                    _currentDragSegment = minNearestSegment;
                    _currentResizeRectangle = minRect;
                }
            }
        }

        public override void LeftMouseUp(MouseButtonEventArgs e) {
            base.LeftMouseUp(e);


            if (Model.CurrentEditMode == CurrentEditMode.ResizeRectangle || Model.CurrentEditMode == CurrentEditMode.MoveRectangle) {
                //Save new position
                Model.EditingFinished(_currentResizeRectangle);
                Model.CurrentEditMode = CurrentEditMode.SelectRectangle;
                SetCursor(e);
            }
            _currentDragRectangle = null;
        }

        public override void MouseMove(MouseEventArgs e) {
            base.MouseMove(e);
            SetCursor(e);

            //Highlighting of currectly nearest rectangle (if in range)
            #region Highlighting
            OcrRectangle minRect;
            OcrRectangle.NearestSegment minNearestSegment;
            var minDist = GetNearestRectangle(e, out minRect, out minNearestSegment);
            if (minDist < _minimumDistance || (minRect != null && minRect.IsInside(Model.CanvasToImage(e.GetPosition(Canvas))))) {
                UnhighlightLastSelectedRect();
                _selectedRect = minRect;
                _lastHighlightedBrush = Model.OcrRectangleToRectangle[minRect].Stroke;
                Model.OcrRectangleToRectangle[minRect].Stroke = Brushes.Green;
            }else if (_selectedRect != null) {
                UnhighlightLastSelectedRect();
            }

            #endregion Highlighting


            if (_leftMouseDown && Model.CurrentEditMode == CurrentEditMode.ResizeRectangle && _currentResizeRectangle != null) {
                _currentResizeRectangle.Resize(Model.CanvasToImage(e.GetPosition(Control)), _currentDragSegment);
                Model.SetRectanglePositionFromOcrRectangle(Model.OcrRectangleToRectangle[_currentResizeRectangle], _currentResizeRectangle);
            }
            if (_leftMouseDown && Model.CurrentEditMode == CurrentEditMode.MoveRectangle && _currentResizeRectangle != null) {
                _currentResizeRectangle.Move(Model.CanvasToImage(e.GetPosition(Control))-_currentMovePoint);
                _currentMovePoint = Model.CanvasToImage(e.GetPosition(Control));
                Model.SetRectanglePositionFromOcrRectangle(Model.OcrRectangleToRectangle[_currentResizeRectangle], _currentResizeRectangle);
            }

        }

        private void UnhighlightLastSelectedRect() {
            if (_selectedRect != null) {
                Rectangle visualRect;
                //Rectangle may have been deleted in the meantime
                if (Model.OcrRectangleToRectangle.TryGetValue(_selectedRect, out visualRect))
                    visualRect.Stroke = _lastHighlightedBrush;
            }
        }

        public override void DeleteKeyPressed() {
            if (_selectedRect != null) {
                Model.DeleteRectangle(_selectedRect);
            }
        }

        private void SetCursor(MouseEventArgs e) {
            if (Model.CurrentEditMode == CurrentEditMode.SelectRectangle && Model.Screenshot != null) {
                OcrRectangle minRect;
                OcrRectangle.NearestSegment minNearestSegment;
                var minDist = GetNearestRectangle(e, out minRect, out minNearestSegment);
                if (minRect != null && minRect.IsInside(Model.CanvasToImage(e.GetPosition(Canvas)))) {
                    ZoomAndPanControl.Cursor = Cursors.SizeAll;
                }
                else if (minDist < _minimumDistance) {
                    ZoomAndPanControl.Cursor = NearestSegmentToCursor(minNearestSegment);
                }
            } else if (Model.CurrentEditMode == CurrentEditMode.ResizeRectangle && Model.Screenshot != null) {
                ZoomAndPanControl.Cursor = NearestSegmentToCursor(_currentDragSegment);
            } else if (Model.CurrentEditMode == CurrentEditMode.MoveRectangle && Model.Screenshot != null) {
                ZoomAndPanControl.Cursor = Cursors.SizeAll;
            }
        }

        private Cursor NearestSegmentToCursor(OcrRectangle.NearestSegment segment) {
            switch (segment) {
                case OcrRectangle.NearestSegment.NorthWest:
                case OcrRectangle.NearestSegment.SouthEast:
                    return Cursors.SizeNWSE;
                case OcrRectangle.NearestSegment.North:
                case OcrRectangle.NearestSegment.South:
                    return Cursors.SizeNS;
                case OcrRectangle.NearestSegment.NorthEast:
                case OcrRectangle.NearestSegment.SouthWest:
                    return Cursors.SizeNESW;
                case OcrRectangle.NearestSegment.East:
                case OcrRectangle.NearestSegment.West:
                    return Cursors.SizeWE;
                default:
                    throw new ArgumentOutOfRangeException("Unknown segment");
            }
        }

        private double GetNearestRectangle(MouseEventArgs e, out OcrRectangle minRect, out OcrRectangle.NearestSegment minNearestSegment) {
            double minDist = double.MaxValue;
            minRect = null;
            minNearestSegment = OcrRectangle.NearestSegment.East;
            if (Model.Screenshot == null) {
                return minDist;
            }

            foreach (var rect in Model.Screenshot.Rectangles) {
                OcrRectangle.NearestSegment nearestSegment;
                double dist = rect.GetDistance(Model.CanvasToImage(e.GetPosition(Canvas)), out nearestSegment);
                if (dist < minDist) {
                    minDist = dist;
                    minRect = rect;
                    minNearestSegment = nearestSegment;
                }
            }
            return minDist;
        }
    }
}
