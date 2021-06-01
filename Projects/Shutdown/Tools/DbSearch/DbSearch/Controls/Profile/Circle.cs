using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Utils;

namespace DbSearch.Controls.Profile {
    class Circle : Canvas {
        public Circle() {
            //Fill = Brushes.Transparent;
        }

        protected override void OnRender(DrawingContext drawingContext) {
            drawingContext.DrawEllipse(Brushes.Transparent, new Pen(Fill, 3), new Point(Width * 0.5, Height * 0.5), Width * 0.5-5, Height * 0.5-5);
            //drawingContext.DrawEllipse(Brushes.Transparent, null, new Point(Width * 0.5, Height * 0.5), Width * 0.25, Height * 0.25);
            //drawingContext.DrawEllipse(new SolidColorBrush(Colors.White) { Opacity = 0.5 }, null, new Point(Width * 0.5, Height * 0.5), Width * 0.25, Height * 0.25);
            
        }

        #region Fill
        public static readonly DependencyProperty FillProperty =
             DependencyProperty.Register("Fill", typeof(Brush),
             typeof(Circle), new FrameworkPropertyMetadata(Brushes.Transparent, OnFillPropertyChanged));

        public Brush Fill {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        private static void OnFillPropertyChanged(DependencyObject source,
                DependencyPropertyChangedEventArgs e) {

            Circle control = source as Circle;
            control.InvalidateVisual();
            control.InvalidateMeasure();
            
        }
        #endregion Fill

    }
}
