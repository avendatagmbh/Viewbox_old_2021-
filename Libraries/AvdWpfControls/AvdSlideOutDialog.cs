using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace AvdWpfControls
{
    public enum HeaderPositions
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public class AvdSlideOutDialog : Control
    {
        private bool _animationRunning;
        private bool _cancelExpansion;
        private bool _isCollapsed = true;
        private bool _isExpanded;
        private bool _waitingForExpansion;

        static AvdSlideOutDialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof (AvdSlideOutDialog), new FrameworkPropertyMetadata(typeof (AvdSlideOutDialog)));
        }

        public AvdSlideOutDialog()
        {
            ClipToBounds = true;
        }

        #region events

        #region Expanded

        public static readonly RoutedEvent ExpandedEvent = EventManager.RegisterRoutedEvent(
            "Expanded", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (AvdSlideOutDialog));

        public event RoutedEventHandler Expanded
        {
            add { AddHandler(ExpandedEvent, value); }
            remove { RemoveHandler(ExpandedEvent, value); }
        }

        private bool OnExpanded()
        {
            var args = new RoutedEventArgs(ExpandedEvent);
            RaiseEvent(args);
            return args.Handled;
        }

        #endregion // Expanded

        #region Collapsed

        public static readonly RoutedEvent CollapsedEvent = EventManager.RegisterRoutedEvent(
            "Collapsed", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (AvdSlideOutDialog));

        public event RoutedEventHandler Collapsed
        {
            add { AddHandler(CollapsedEvent, value); }
            remove { RemoveHandler(CollapsedEvent, value); }
        }

        private bool OnCollapsed()
        {
            var args = new RoutedEventArgs(CollapsedEvent);
            RaiseEvent(args);
            return args.Handled;
        }

        #endregion // Collapsed

        #endregion // events

        #region properties

        #region HeaderPosition

        // Using a DependencyProperty as the backing store for HeaderPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderPositionProperty =
            DependencyProperty.Register(
                "HeaderPosition",
                typeof (HeaderPositions),
                typeof (AvdSlideOutDialog),
                new UIPropertyMetadata(HeaderPositions.TopRight, (o, args) =>
                                                                     {
                                                                         //var item = (AvdSlideOutDialog) o;
                                                                         //switch (item.HeaderPosition) {
                                                                         //    case HeaderPositions.TopLeft:
                                                                         //        item.HeaderCornerRadius = new CornerRadius(0, 0, 10, 0);
                                                                         //        break;
                                                                         //    case HeaderPositions.TopRight:
                                                                         //        item.HeaderCornerRadius = new CornerRadius(0, 0, 0, 10);
                                                                         //        break;

                                                                         //    case HeaderPositions.BottomLeft:
                                                                         //        item.HeaderCornerRadius = new CornerRadius(10, 0, 0, 0);
                                                                         //        break;

                                                                         //    case HeaderPositions.BottomRight:
                                                                         //        item.HeaderCornerRadius = new CornerRadius(0, 10, 0, 0);
                                                                         //        break;
                                                                         //}
                                                                     }));

        public HeaderPositions HeaderPosition
        {
            get { return (HeaderPositions) GetValue(HeaderPositionProperty); }
            set { SetValue(HeaderPositionProperty, value); }
        }

        #endregion // HeaderPosition

        #region Content

        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof (object), typeof (AvdSlideOutDialog),
                                        new UIPropertyMetadata(null));

        public object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        #endregion // Content

        #region HeaderBackground

        // Using a DependencyProperty as the backing store for HeaderBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderBackgroundProperty =
            DependencyProperty.Register("HeaderBackground", typeof (Brush), typeof (AvdSlideOutDialog),
                                        new UIPropertyMetadata(Brushes.White));

        public Brush HeaderBackground
        {
            get { return (Brush) GetValue(HeaderBackgroundProperty); }
            set { SetValue(HeaderBackgroundProperty, value); }
        }

        #endregion HeaderBackground

        #region HeaderBorderBrush

        // Using a DependencyProperty as the backing store for HeaderBorderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderBorderBrushProperty =
            DependencyProperty.Register("HeaderBorderBrush", typeof (Brush), typeof (AvdSlideOutDialog),
                                        new UIPropertyMetadata(Brushes.Silver));

        public Brush HeaderBorderBrush
        {
            get { return (Brush) GetValue(HeaderBorderBrushProperty); }
            set { SetValue(HeaderBorderBrushProperty, value); }
        }

        #endregion HeaderBorderBrush

        #region HeaderForeground

        // Using a DependencyProperty as the backing store for HeaderForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderForegroundProperty =
            DependencyProperty.Register("HeaderForeground", typeof (Brush), typeof (AvdSlideOutDialog),
                                        new UIPropertyMetadata(Brushes.Black));

        public Brush HeaderForeground
        {
            get { return (Brush) GetValue(HeaderForegroundProperty); }
            set { SetValue(HeaderForegroundProperty, value); }
        }

        #endregion // HeaderForeground

        #region ImageSource

        // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof (ImageSource), typeof (AvdSlideOutDialog),
                                        new UIPropertyMetadata(null));

        public ImageSource ImageSource
        {
            get { return (ImageSource) GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        #endregion ImageSource

        #region ImageHeight

        // Using a DependencyProperty as the backing store for ImageHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageHeightProperty =
            DependencyProperty.Register("ImageHeight", typeof (double), typeof (AvdSlideOutDialog),
                                        new UIPropertyMetadata((double) 16));

        public double ImageHeight
        {
            get { return (double) GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }

        #endregion ImageHeight

        #region Caption

        // Using a DependencyProperty as the backing store for Caption.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register(
                "Caption", typeof (string), typeof (AvdSlideOutDialog), new UIPropertyMetadata(string.Empty));

        public string Caption
        {
            get { return (string) GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        #endregion // Caption

        #region ExpandDuration

        // Using a DependencyProperty as the backing store for ExpandDuration.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExpandDurationProperty =
            DependencyProperty.Register(
                "ExpandDuration",
                typeof (TimeSpan),
                typeof (AvdSlideOutDialog),
                new UIPropertyMetadata(new TimeSpan(0, 0, 0, 0, 250)));

        public TimeSpan ExpandDuration
        {
            get { return (TimeSpan) GetValue(ExpandDurationProperty); }
            set { SetValue(ExpandDurationProperty, value); }
        }

        #endregion // ExpandDuration

        #region CollapseDuration

        // Using a DependencyProperty as the backing store for CollapseDuration.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CollapseDurationProperty =
            DependencyProperty.Register(
                "CollapseDuration",
                typeof (TimeSpan),
                typeof (AvdSlideOutDialog),
                new UIPropertyMetadata(new TimeSpan(0, 0, 0, 0, 150)));

        public TimeSpan CollapseDuration
        {
            get { return (TimeSpan) GetValue(CollapseDurationProperty); }
            set { SetValue(CollapseDurationProperty, value); }
        }

        #endregion // CollapseDuration

        #region CanvasLeft

        // Using a DependencyProperty as the backing store for CanvasLeft.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanvasLeftProperty =
            DependencyProperty.Register("CanvasLeft", typeof (double), typeof (AvdSlideOutDialog),
                                        new UIPropertyMetadata(0.0));

        public double CanvasLeft
        {
            get { return (double) GetValue(CanvasLeftProperty); }
            set { SetValue(CanvasLeftProperty, value); }
        }

        #endregion // CanvasLeft

        #region CanvasTop

        // Using a DependencyProperty as the backing store for CanvasTop.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanvasTopProperty =
            DependencyProperty.Register("CanvasTop", typeof (double), typeof (AvdSlideOutDialog),
                                        new UIPropertyMetadata(0.0));

        public double CanvasTop
        {
            get { return (double) GetValue(CanvasTopProperty); }
            set { SetValue(CanvasTopProperty, value); }
        }

        #endregion // CanvasTop

        #region HeaderCornerRadius

        // Using a DependencyProperty as the backing store for HeaderCornerRadius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderCornerRadiusProperty =
            DependencyProperty.Register("HeaderCornerRadius", typeof (CornerRadius), typeof (AvdSlideOutDialog),
                                        new UIPropertyMetadata(new CornerRadius()));

        public CornerRadius HeaderCornerRadius
        {
            get { return (CornerRadius) GetValue(HeaderCornerRadiusProperty); }
            set { SetValue(HeaderCornerRadiusProperty, value); }
        }

        #endregion // HeaderCornerRadius

        #region HeaderHeight

        // Using a DependencyProperty as the backing store for HeaderHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderHeightProperty =
            DependencyProperty.Register("HeaderHeight", typeof (double), typeof (AvdSlideOutDialog),
                                        new UIPropertyMetadata(20.0));

        public double HeaderHeight
        {
            get { return (double) GetValue(HeaderHeightProperty); }
            set { SetValue(HeaderHeightProperty, value); }
        }

        #endregion // HeaderHeight

        #region ContentWidth

        // Using a DependencyProperty as the backing store for ContentWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentWidthProperty =
            DependencyProperty.Register(
                "ContentWidth", typeof (double), typeof (AvdSlideOutDialog), new UIPropertyMetadata(150.0));

        public double ContentWidth
        {
            get { return (double) GetValue(ContentWidthProperty); }
            set { SetValue(ContentWidthProperty, value); }
        }

        #endregion // ContentWidth

        #region DefaultContentWidth

        // Using a DependencyProperty as the backing store for DefaultContentWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultContentWidthProperty =
            DependencyProperty.Register(
                "DefaultContentWidth",
                typeof (double),
                typeof (AvdSlideOutDialog),
                new UIPropertyMetadata(
                    150.0,
                    (o, args) =>
                        {
                            var dlg = ((AvdSlideOutDialog) o);
                            dlg.ContentWidth = dlg.DefaultContentWidth;
                        }));

        public double DefaultContentWidth
        {
            get { return (double) GetValue(DefaultContentWidthProperty); }
            set { SetValue(DefaultContentWidthProperty, value); }
        }

        #endregion // DefaultContentWidth

        #region ContentHeight

        // Using a DependencyProperty as the backing store for ContentHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentHeightProperty =
            DependencyProperty.Register("ContentHeight", typeof (double), typeof (AvdSlideOutDialog),
                                        new UIPropertyMetadata(0.0));

        public double ContentHeight
        {
            get { return (double) GetValue(ContentHeightProperty); }
            set { SetValue(ContentHeightProperty, value); }
        }

        #endregion // ContentHeight

        #region FullVerticalExpansion

        // Using a DependencyProperty as the backing store for FullVerticalExpansion.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FullVerticalExpansionProperty =
            DependencyProperty.Register("FullVerticalExpansion", typeof (bool), typeof (AvdSlideOutDialog),
                                        new UIPropertyMetadata(false));

        /// <summary>
        ///   If true, the content will be expanded to full control size, otherwhise a small gap is left to display the shadow.
        /// </summary>
        public bool FullVerticalExpansion
        {
            get { return (bool) GetValue(FullVerticalExpansionProperty); }
            set { SetValue(FullVerticalExpansionProperty, value); }
        }

        #endregion // FullVerticalExpansion

        #region AnimationTriggerEnabled

        private bool _animationTriggerEnabled = true;

        public bool AnimationTriggerEnabled
        {
            get { return _animationTriggerEnabled; }
            set { _animationTriggerEnabled = value; }
        }

        #endregion // AnimationTriggerEnabled

        #endregion // properties

        public void Expand()
        {
            if (_isExpanded) return;
            if (_animationRunning) return;
            _cancelExpansion = false;
            _waitingForExpansion = true;
            Thread.Sleep(200);
            if (_cancelExpansion)
            {
                _waitingForExpansion = false;
                return;
            }
            _waitingForExpansion = false;
            _animationRunning = true;
            RunExpandAnimation();
        }

        private void RunExpandAnimation()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(RunExpandAnimation), DispatcherPriority.Normal);
                return;
            }
            var width = ActualWidth;
            var sb = new Storyboard();
            int animMs = (int) ExpandDuration.TotalMilliseconds;
            AddHeaderCornerRadiusAnimation(sb, animMs, 10.0, 0.0);
            switch (HeaderPosition)
            {
                case HeaderPositions.TopLeft:
                    AddXAnimation(sb, animMs, -width + DefaultContentWidth - 20, 0);
                    AddXAnimation(sb, animMs, -width + DefaultContentWidth, 0);
                    break;

                case HeaderPositions.TopRight:
                    AddXAnimation(sb, animMs, width - DefaultContentWidth, 20);
                    AddYAnimation(sb, animMs, (-1)*(ActualHeight), 0);
                    break;

                case HeaderPositions.BottomLeft:
                    AddXAnimation(sb, animMs, -width + DefaultContentWidth - 20, 0);
                    AddYAnimation(sb, animMs, ActualHeight, (FullVerticalExpansion ? 0 : 20));
                    break;

                case HeaderPositions.BottomRight:
                    AddXAnimation(sb, animMs, width - DefaultContentWidth, 20);
                    AddYAnimation(sb, animMs, ActualHeight, (FullVerticalExpansion ? 0 : 20));
                    break;
            }
            sb.Completed += (sender, args) =>
                                {
                                    _animationRunning = false;
                                    _isExpanded = true;
                                    _isCollapsed = false;
                                    OnExpanded();
                                };
            UpdatePositions();
            ContentWidth = width - 20;
            ContentHeight = ActualHeight - HeaderHeight - (FullVerticalExpansion ? 0 : 20);
            sb.Begin();
        }

        public void Collapse()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(Collapse), DispatcherPriority.Normal);
                return;
            }
            if (_waitingForExpansion)
            {
                _cancelExpansion = true;
                return;
            }
            if (_isCollapsed) return;
            if (_animationRunning) return;
            _animationRunning = true;
            var width = Math.Min(600, ActualWidth);
            var sb = new Storyboard();
            int animMs = (int) CollapseDuration.TotalMilliseconds;
            AddHeaderCornerRadiusAnimation(sb, animMs, 0, 10);
            switch (HeaderPosition)
            {
                case HeaderPositions.TopLeft:
                    AddXAnimation(sb, animMs, 0, -width + DefaultContentWidth + 20);
                    AddYAnimation(sb, animMs, 0, (-1)*(ActualHeight));
                    break;
                case HeaderPositions.TopRight:
                    AddXAnimation(sb, animMs, 20, width - DefaultContentWidth);
                    AddYAnimation(sb, animMs, 0, (-1)*(ActualHeight));
                    break;
                case HeaderPositions.BottomLeft:
                    AddXAnimation(sb, animMs, 0, -width + DefaultContentWidth + 20);
                    AddYAnimation(sb, animMs, (FullVerticalExpansion ? 0 : 20), ActualHeight);
                    break;
                case HeaderPositions.BottomRight:
                    AddXAnimation(sb, animMs, 20, width - DefaultContentWidth);
                    AddYAnimation(sb, animMs, (FullVerticalExpansion ? 0 : 20), ActualHeight);
                    break;
            }
            sb.Completed += (sender, args) =>
                                {
                                    _animationRunning = false;
                                    _isExpanded = false;
                                    _isCollapsed = true;
                                    OnCollapsed();
                                };
            sb.Begin();
        }

        private void AddXAnimation(Storyboard sb, int animMs, double from, double to)
        {
            var animX = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(animMs));
            Storyboard.SetTarget(animX, this);
            Storyboard.SetTargetProperty(animX, new PropertyPath(CanvasLeftProperty));
            sb.Children.Add(animX);
        }

        private void AddYAnimation(Storyboard sb, int animMs, double from, double to)
        {
            var animY = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(animMs));
            Storyboard.SetTarget(animY, this);
            Storyboard.SetTargetProperty(animY, new PropertyPath(CanvasTopProperty));
            sb.Children.Add(animY);
        }

        private void AddHeaderCornerRadiusAnimation(Storyboard sb, int animMs, double from, double to)
        {
            var animHeaderCornerRadius = new ObjectAnimationUsingKeyFrames();
            animHeaderCornerRadius.KeyFrames.Add(
                new DiscreteObjectKeyFrame(
                    new CornerRadius(0, 0, 0, from),
                    KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0))));
            var animStepMs = animMs/10;
            var step = (to - from)/10;
            for (int i = 1; i <= 10; i++)
            {
                switch (HeaderPosition)
                {
                    case HeaderPositions.TopLeft:
                        animHeaderCornerRadius.KeyFrames.Add(
                            new DiscreteObjectKeyFrame(
                                new CornerRadius(0, 0, from + (i*step), 0),
                                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(animStepMs*i))));
                        break;
                    case HeaderPositions.TopRight:
                        animHeaderCornerRadius.KeyFrames.Add(
                            new DiscreteObjectKeyFrame(
                                new CornerRadius(0, 0, 0, from + (i*step)),
                                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(animStepMs*i))));
                        break;
                    case HeaderPositions.BottomLeft:
                        animHeaderCornerRadius.KeyFrames.Add(
                            new DiscreteObjectKeyFrame(
                                new CornerRadius(0, from + (i*step), 0, 0),
                                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(animStepMs*i))));
                        break;
                    case HeaderPositions.BottomRight:
                        animHeaderCornerRadius.KeyFrames.Add(
                            new DiscreteObjectKeyFrame(
                                new CornerRadius(from + (i*step), 0, 0, 0),
                                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(animStepMs*i))));
                        break;
                }
            }
            Storyboard.SetTarget(animHeaderCornerRadius, this);
            Storyboard.SetTargetProperty(animHeaderCornerRadius, new PropertyPath(HeaderCornerRadiusProperty));
            sb.Children.Add(animHeaderCornerRadius);
        }

        private void UpdatePositions()
        {
            CanvasLeft = Math.Min(600, ActualWidth) - DefaultContentWidth;
            CanvasTop = (-1)*ActualHeight;
        }

        public void DisableAnimationTrigger()
        {
            AnimationTriggerEnabled = false;
        }

        public void EnableAnimationTrigger()
        {
            AnimationTriggerEnabled = true;
        }
    }
}