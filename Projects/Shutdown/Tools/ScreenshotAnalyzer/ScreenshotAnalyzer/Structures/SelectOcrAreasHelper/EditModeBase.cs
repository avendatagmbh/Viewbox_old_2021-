using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using ScreenshotAnalyzer.Models;
using ZoomAndPan;

namespace ScreenshotAnalyzer.Structures.SelectOcrAreasHelper {
    public abstract class EditModeBase {
        public EditModeBase(UIElement control, ZoomAndPanControl zoomAndPanControl, SelectOcrAreasModel model, Canvas canvas) {
            Control = control;
            Model = model;
            ZoomAndPanControl = zoomAndPanControl;
            Canvas = canvas;
        }


        #region Properties
        protected ZoomAndPanControl ZoomAndPanControl { get; private set; }
        protected UIElement Control { get; private set; }
        protected Canvas Canvas { get; private set; }
        protected Point _panStartPosition = new Point(0, 0);
        protected bool _leftMouseDown, _rightMouseDown;
        protected Point _dragStartPosition;
        protected Rectangle _currentDragRectangle;
        protected SelectOcrAreasModel Model { get; private set; }
        #endregion Properties

        public virtual void LeftMouseDown(MouseButtonEventArgs e) {
            _leftMouseDown = true;
            _dragStartPosition = Model.CanvasToImage(e.GetPosition(Control));
            Control.CaptureMouse();
        }

        public virtual void LeftMouseUp(MouseButtonEventArgs e) {
            _leftMouseDown = false;
            Control.ReleaseMouseCapture();

        }

        public virtual void RightMouseDown(MouseButtonEventArgs e) {
            _rightMouseDown = true;
            _panStartPosition = e.GetPosition(ZoomAndPanControl);
            Control.CaptureMouse();
        }

        public virtual void RightMouseUp(MouseButtonEventArgs e) {
            _rightMouseDown = false;
            Control.ReleaseMouseCapture();

        }

        public virtual void MouseWheel(MouseWheelEventArgs e) {
            ZoomAndPanControl.ContentScale += e.Delta > 0 ? 0.1 : -0.1;
        }

        public virtual void MouseMove(MouseEventArgs e) {
            ZoomAndPanControl.Cursor = Cursors.Arrow;
            if (_rightMouseDown) {
                Point curContentMousePoint = e.GetPosition(ZoomAndPanControl);
                Vector dragOffset = curContentMousePoint - _panStartPosition;
                
                ZoomAndPanControl.ContentOffsetX -= dragOffset.X;
                ZoomAndPanControl.ContentOffsetY -= dragOffset.Y;
                _panStartPosition = curContentMousePoint;
            }
            
        }

        public virtual void DeleteKeyPressed() {
            
        }
    }
}
