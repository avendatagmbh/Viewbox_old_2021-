using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ScreenshotAnalyzer.Models;
using ScreenshotAnalyzerBusiness.Structures;
using ZoomAndPan;

namespace ScreenshotAnalyzer.Structures.SelectOcrAreasHelper {
    public class EditModeSelectAreaOrAnchor : EditModeBase{
        public EditModeSelectAreaOrAnchor(UIElement control, ZoomAndPanControl zoomAndPanControl, SelectOcrAreasModel model, Canvas canvas)
            : base(control, zoomAndPanControl, model, canvas) {
            
        }

        public override void LeftMouseUp(MouseButtonEventArgs e) {
            base.LeftMouseUp(e);
            if (HasMinRectangleSize(_dragStartPosition, e.GetPosition(Canvas)))
                Model.AddRectangle(_dragStartPosition, Model.CanvasToImage(e.GetPosition(Canvas)));
            if (_currentDragRectangle != null) {
                Canvas.Children.Remove(_currentDragRectangle);
                _currentDragRectangle = null;
            }
        }

        public override void MouseMove(MouseEventArgs e) {
            base.MouseMove(e);
            //if(Model.CurrentEditMode == CurrentEditMode.SelectAnchor)
            //    ZoomAndPanControl.Cursor = Cursors.UpArrow;
            //else
                ZoomAndPanControl.Cursor = Cursors.Cross;
            
            if (_leftMouseDown) {
                Point currentPos = Model.CanvasToImage(e.GetPosition(Canvas));
                if (_currentDragRectangle == null && HasMinRectangleSize(_dragStartPosition, currentPos)) {
                    _currentDragRectangle = Model.CreateRectangle(Canvas, new OcrRectangle(_dragStartPosition, currentPos), Model.CurrentEditMode);
                }
                if (_currentDragRectangle != null) {
                    Model.SetRectanglePositionFromOcrRectangle(_currentDragRectangle, new OcrRectangle(_dragStartPosition, currentPos));
                }

            }
        }

        private bool HasMinRectangleSize(Point start, Point end) {
            return Math.Abs(end.X - start.X) + Math.Abs(end.Y - start.Y) > 40;
        }
    }
}
