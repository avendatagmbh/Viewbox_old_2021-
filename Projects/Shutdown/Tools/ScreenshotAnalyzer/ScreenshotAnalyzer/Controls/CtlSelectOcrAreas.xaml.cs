using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ScreenshotAnalyzer.Models;
using ScreenshotAnalyzerBusiness.Structures;
using Utils;
using ViewValidator.Windows;

namespace ScreenshotAnalyzer.Controls {
    /// <summary>
    /// Interaktionslogik für CtlSelectOcrAreas.xaml
    /// </summary>
    public partial class CtlSelectOcrAreas : UserControl {
        public CtlSelectOcrAreas() {
            InitializeComponent();
            ((Image)imageArea.Children[0]).SizeChanged += new SizeChangedEventHandler(CtlSelectOcrAreas_SizeChanged);
        }


        #region Fields
        SelectOcrAreasModel Model { get { return DataContext as SelectOcrAreasModel; } }
        #endregion Fields

        #region Mouseactions
        private void imageArea_MouseWheel(object sender, MouseWheelEventArgs e) {
            Model.CurrentEditModeInstance.MouseWheel(e);
        }

        private void ctlSelectOcrAreas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            Model.CurrentEditModeInstance.LeftMouseDown(e);
        }

        private void ctlSelectOcrAreas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Model.CurrentEditModeInstance.LeftMouseUp(e);
        }


        private void ctlSelectOcrAreas_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            Model.CurrentEditModeInstance.RightMouseDown(e);
        }

        private void ctlSelectOcrAreas_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            Model.CurrentEditModeInstance.RightMouseUp(e);
        }

        private void ctlSelectOcrAreas_MouseMove(object sender, MouseEventArgs e) {
            Model.CurrentEditModeInstance.MouseMove(e);
        }

        #endregion Mouseactions

        #region EventHandler
        void CtlSelectOcrAreas_SizeChanged(object sender, SizeChangedEventArgs e) {
            ResizeRectangles();
        }


        private void ctlSelectOcrAreas_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (Model != null) {
                Model.SetControls(imageArea, (Image)imageArea.Children[0], zoomAndPanControl);
            }
        }
        #endregion EventHandler

        #region Buttonactions

        private void rbSelectAnchor_Checked(object sender, RoutedEventArgs e) {
            Model.CurrentEditMode = CurrentEditMode.SelectAnchor;
        }

        private void rbSelectOcrArea_Checked(object sender, RoutedEventArgs e) {
            Model.CurrentEditMode = CurrentEditMode.SelectArea;
        }

        private void rbSelectRectangle_Checked(object sender, RoutedEventArgs e) {
            Model.CurrentEditMode = CurrentEditMode.SelectRectangle;
        }

        private void ctlSelectOcrAreas_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.D1)
                rbSelectAnchor.IsChecked = true;
            if (e.Key == Key.D2)
                rbSelectOcrArea.IsChecked = true;
            if (e.Key == Key.D3)
                rbSelectRectangle.IsChecked = true;
            if (e.Key == Key.Delete) {
                Model.CurrentEditModeInstance.DeleteKeyPressed();
            }
        }

        #endregion Buttonactions

        #region Methods

        private void ResizeRectangles() {
            foreach (var child in imageArea.Children) {
                Rectangle rect = child as Rectangle;
                if (rect != null) {
                    Model.SetRectanglePositionFromOcrRectangle(rect, Model.RectangleToOcrRectangle[rect]);
                }
            }
        }
        #endregion Methods


    }
}
