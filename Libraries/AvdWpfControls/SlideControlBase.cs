using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AvdWpfControls
{
    public enum SlideDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    public class SlideControlBase : ItemsControl, INotifyPropertyChanged
    {
        static SlideControlBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (SlideControlBase),
                                                     new FrameworkPropertyMetadata(typeof (SlideControlBase)));
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        //protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e) {
        //    if (e.Key == System.Windows.Input.Key.Tab) {
        //        //var c = Items[0] as FrameworkElement;
        //        //var x = VisualTreeHelper.GetChild(c, 0);
        //        //if (c != null) {
        //        //    e.Handled = true;
        //        //    c.MoveFocus(new TraversalRequest(FocusNavigationDirection.NavigateNext));
        //        //}
        //    } else {
        //        base.OnKeyDown(e);
        //    }
        //}
        //protected override void OnKeyUp(System.Windows.Input.KeyEventArgs e) {
        //    if (e.Key == System.Windows.Input.Key.Tab) {
        //        e.Handled = true;
        //    } else {
        //        base.OnKeyDown(e);
        //    }
        //}
        //protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e) {
        //    if (e.Key == System.Windows.Input.Key.Tab) {
        //    } else {
        //        base.OnKeyDown(e);
        //    }
        //}
        //protected override void OnPreviewKeyUp(System.Windows.Input.KeyEventArgs e) {
        //    if (e.Key == System.Windows.Input.Key.Tab) {
        //    } else {
        //        base.OnKeyDown(e);
        //    }
        //}
        private void show_child_completed(object sender, EventArgs e)
        {
            (Content as FrameworkElement).Visibility = Visibility.Collapsed;
            UseClipping = false;
        }

        private void hide_children_completed(object sender, EventArgs e)
        {
            foreach (FrameworkElement item in Items) item.Visibility = Visibility.Collapsed;
            UseClipping = false;
        }

        #region Headers

        public IEnumerable<SlideControlHeader> Headers
        {
            get
            {
                var headers = new List<SlideControlHeader>();
                if (!string.IsNullOrEmpty(Caption) || ImageSource != null)
                {
                    var header = new SlideControlHeader {Caption = Caption, ImageSource = ImageSource};
                    headers.Insert(0, header);
                    var parent = Parent as SlideControlBase;
                    while (parent != null)
                    {
                        if (!string.IsNullOrEmpty(parent.Caption) || parent.ImageSource != null)
                        {
                            header = new SlideControlHeader {Caption = parent.Caption, ImageSource = parent.ImageSource};
                            header.Opacity = 0.5;
                            headers.Insert(0, header);
                        }
                        parent = parent.Parent as SlideControlBase;
                    }
                }
                return headers;
            }
        }

        #endregion Headers

        #region HideChildren

        //--------------------------------------------------------------------------------
        public void HideChildren()
        {
            // show content panel
            switch (SlideDirection)
            {
                case SlideDirection.Left:
                    ContentTransformY = 0;
                    ItemsTransformY = 0;
                    ContentTransformX = -ActualWidth;
                    break;
                case SlideDirection.Right:
                    ContentTransformY = 0;
                    ItemsTransformY = 0;
                    ContentTransformX = ActualWidth;
                    break;
                case SlideDirection.Up:
                    ContentTransformX = 0;
                    ItemsTransformX = 0;
                    ContentTransformY = -ActualHeight;
                    break;
                case SlideDirection.Down:
                    ContentTransformX = 0;
                    ItemsTransformX = 0;
                    ContentTransformY = ActualHeight;
                    break;
                default:
                    break;
            }
            (Content as FrameworkElement).Visibility = Visibility.Visible;
            var sb = new Storyboard();
            // add animation to slide in the content panel
            var slideInAnim = new DoubleAnimation {Duration = AnimDuration, From = -ActualHeight, To = 0};
            sb.Children.Add(slideInAnim);
            Storyboard.SetTarget(slideInAnim, this);
            // add animation to slide out the items panel
            var slideOutAnim = new DoubleAnimation {Duration = AnimDuration, From = 0, To = ActualHeight};
            sb.Children.Add(slideOutAnim);
            Storyboard.SetTarget(slideOutAnim, this);
            // add clip animation to slide in the content panel
            var slideInClipAnim = new RectAnimation {Duration = AnimDuration};
            sb.Children.Add(slideInClipAnim);
            Storyboard.SetTarget(slideInClipAnim, this);
            Storyboard.SetTargetProperty(slideInClipAnim, new PropertyPath(ContentClippingRectProperty));
            // add clip animation to slide out the items panel
            var slideOutClipAnim = new RectAnimation {Duration = AnimDuration};
            sb.Children.Add(slideOutClipAnim);
            Storyboard.SetTarget(slideOutClipAnim, this);
            Storyboard.SetTargetProperty(slideOutClipAnim, new PropertyPath(ItemsClippingRectProperty));
            switch (SlideDirection)
            {
                case SlideDirection.Left:
                    slideOutAnim.From = 0;
                    slideOutAnim.To = ActualWidth;
                    Storyboard.SetTargetProperty(slideInAnim, new PropertyPath(ContentTransformXProperty));
                    slideInAnim.From = -ActualWidth;
                    slideInAnim.To = 0;
                    Storyboard.SetTargetProperty(slideOutAnim, new PropertyPath(ItemsTransformXProperty));
                    slideInClipAnim.From = new Rect(ActualWidth, 0, 0, ActualHeight);
                    slideInClipAnim.To = new Rect(0, 0, ActualWidth, ActualHeight);
                    slideOutClipAnim.From = new Rect(0, 0, ActualWidth, ActualHeight);
                    slideOutClipAnim.To = new Rect(0, 0, 0, ActualHeight);
                    break;
                case SlideDirection.Right:
                    slideOutAnim.From = 0;
                    slideOutAnim.To = -ActualWidth;
                    Storyboard.SetTargetProperty(slideInAnim, new PropertyPath(ContentTransformXProperty));
                    slideInAnim.From = ActualWidth;
                    slideInAnim.To = 0;
                    Storyboard.SetTargetProperty(slideOutAnim, new PropertyPath(ItemsTransformXProperty));
                    slideInClipAnim.From = new Rect(0, 0, 0, ActualHeight);
                    slideInClipAnim.To = new Rect(0, 0, ActualWidth, ActualHeight);
                    slideOutClipAnim.From = new Rect(0, 0, 0, ActualHeight);
                    slideOutClipAnim.To = new Rect(ActualWidth, 0, ActualWidth, ActualHeight);
                    break;
                case SlideDirection.Up:
                    slideOutAnim.From = 0;
                    slideOutAnim.To = ActualHeight;
                    Storyboard.SetTargetProperty(slideInAnim, new PropertyPath(ContentTransformYProperty));
                    slideInAnim.From = -ActualHeight;
                    slideInAnim.To = 0;
                    Storyboard.SetTargetProperty(slideOutAnim, new PropertyPath(ItemsTransformYProperty));
                    slideInClipAnim.From = new Rect(0, ActualHeight, ActualWidth, ActualHeight);
                    slideInClipAnim.To = new Rect(0, 0, ActualWidth, ActualHeight);
                    slideOutClipAnim.From = new Rect(0, 0, ActualWidth, ActualHeight);
                    slideOutClipAnim.To = new Rect(0, 0, ActualWidth, 0);
                    break;
                case SlideDirection.Down:
                    slideOutAnim.From = 0;
                    slideOutAnim.To = -ActualHeight;
                    Storyboard.SetTargetProperty(slideInAnim, new PropertyPath(ContentTransformYProperty));
                    slideInAnim.From = ActualHeight;
                    slideInAnim.To = 0;
                    Storyboard.SetTargetProperty(slideOutAnim, new PropertyPath(ItemsTransformYProperty));
                    slideInClipAnim.From = new Rect(0, 0, ActualWidth, 0);
                    slideInClipAnim.To = new Rect(0, 0, ActualWidth, ActualHeight);
                    slideOutClipAnim.From = new Rect(0, 0, ActualWidth, ActualHeight);
                    slideOutClipAnim.To = new Rect(0, ActualHeight, ActualWidth, 0);
                    break;
                default:
                    break;
            }
            // hide items after animation finished
            sb.Completed += hide_children_completed;
            // update selected content
            DependencyObject root = this;
            while (root != null && !(root is SlideControl)) root = VisualTreeHelper.GetParent(root);
            var sc = root as SlideControl;
            sc.SelectedContent = Content;
            // begin animation
            UseClipping = true;
            sb.Begin(this);
        }

        //--------------------------------------------------------------------------------

        #endregion HideChildren

        #region Hide

        public void Hide()
        {
            (Parent as SlideControlBase).HideChildren();
        }

        #endregion

        #region Content

        //--------------------------------------------------------------------------------
        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof (object), typeof (SlideControlBase),
                                        new UIPropertyMetadata(null));

        public object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        //--------------------------------------------------------------------------------

        #endregion Content

        #region ImageSource

        //--------------------------------------------------------------------------------
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof (ImageSource), typeof (SlideControlBase),
                                        new UIPropertyMetadata(null));

        public ImageSource ImageSource
        {
            get { return (ImageSource) GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        //--------------------------------------------------------------------------------

        #endregion ImageSource

        #region Caption

        //--------------------------------------------------------------------------------
        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof (string), typeof (SlideControlBase),
                                        new UIPropertyMetadata(string.Empty));

        public string Caption
        {
            get { return (string) GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        //--------------------------------------------------------------------------------

        #endregion Caption

        #region ContentTransformX

        //--------------------------------------------------------------------------------
        public static readonly DependencyProperty ContentTransformXProperty =
            DependencyProperty.Register("ContentTransformX", typeof (double), typeof (SlideControlBase),
                                        new UIPropertyMetadata(0.0));

        public double ContentTransformX
        {
            get { return (double) GetValue(ContentTransformXProperty); }
            set { SetValue(ContentTransformXProperty, value); }
        }

        //--------------------------------------------------------------------------------

        #endregion ContentTransformY

        #region ContentTransformY

        //--------------------------------------------------------------------------------
        public static readonly DependencyProperty ContentTransformYProperty =
            DependencyProperty.Register("ContentTransformY", typeof (double), typeof (SlideControlBase),
                                        new UIPropertyMetadata(0.0));

        public double ContentTransformY
        {
            get { return (double) GetValue(ContentTransformYProperty); }
            set { SetValue(ContentTransformYProperty, value); }
        }

        //--------------------------------------------------------------------------------

        #endregion ContentTransformY

        #region ItemsTransformX

        //--------------------------------------------------------------------------------
        // Using a DependencyProperty as the backing store for ItemsTransformY.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsTransformXProperty =
            DependencyProperty.Register("ItemsTransformX", typeof (double), typeof (SlideControlBase),
                                        new UIPropertyMetadata(0.0));

        public double ItemsTransformX
        {
            get { return (double) GetValue(ItemsTransformXProperty); }
            set { SetValue(ItemsTransformXProperty, value); }
        }

        //--------------------------------------------------------------------------------

        #endregion ItemsTransformX

        #region ItemsTransformY

        //--------------------------------------------------------------------------------
        // Using a DependencyProperty as the backing store for ItemsTransformY.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsTransformYProperty =
            DependencyProperty.Register("ItemsTransformY", typeof (double), typeof (SlideControlBase),
                                        new UIPropertyMetadata(0.0));

        public double ItemsTransformY
        {
            get { return (double) GetValue(ItemsTransformYProperty); }
            set { SetValue(ItemsTransformYProperty, value); }
        }

        //--------------------------------------------------------------------------------

        #endregion ItemsTransformY

        #region AnimDuration

        //--------------------------------------------------------------------------------
        public static readonly DependencyProperty AnimDurationProperty =
            DependencyProperty.Register("AnimDuration", typeof (Duration), typeof (SlideControlBase),
                                        new UIPropertyMetadata(
                                            new Duration(TimeSpan.FromMilliseconds(220))));

        public Duration AnimDuration
        {
            get { return (Duration) GetValue(AnimDurationProperty); }
            set { SetValue(AnimDurationProperty, value); }
        }

        //--------------------------------------------------------------------------------

        #endregion AnimDuration

        #region SlideDirection

        //--------------------------------------------------------------------------------
        // Using a DependencyProperty as the backing store for SlideDirection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SlideDirectionProperty =
            DependencyProperty.Register("SlideDirection", typeof (SlideDirection), typeof (SlideControlBase),
                                        new UIPropertyMetadata(SlideDirection.Left));

        public SlideDirection SlideDirection
        {
            get { return (SlideDirection) GetValue(SlideDirectionProperty); }
            set { SetValue(SlideDirectionProperty, value); }
        }

        //--------------------------------------------------------------------------------

        #endregion SlideDirection

        #region UseClipping

        public static readonly DependencyProperty UseClippingProperty =
            DependencyProperty.Register("UseClipping", typeof (bool), typeof (SlideControlBase),
                                        new UIPropertyMetadata(false));

        public bool UseClipping
        {
            get { return (bool) GetValue(UseClippingProperty); }
            set { SetValue(UseClippingProperty, value); }
        }

        #endregion UseClipping

        #region ContentClippingRect

        public static readonly DependencyProperty ContentClippingRectProperty =
            DependencyProperty.Register("ContentClippingRect", typeof (Rect), typeof (SlideControlBase),
                                        new UIPropertyMetadata(new Rect()));

        public Rect ContentClippingRect
        {
            get { return (Rect) GetValue(ContentClippingRectProperty); }
            set { SetValue(ContentClippingRectProperty, value); }
        }

        #endregion ContentClippingRect

        #region ItemsClippingRect

        public static readonly DependencyProperty ItemsClippingRectProperty =
            DependencyProperty.Register("ItemsClippingRect", typeof (Rect), typeof (SlideControlBase),
                                        new UIPropertyMetadata(new Rect()));

        public Rect ItemsClippingRect
        {
            get { return (Rect) GetValue(ItemsClippingRectProperty); }
            set { SetValue(ItemsClippingRectProperty, value); }
        }

        #endregion ItemsClippingRect

        #region ShowChild

        //--------------------------------------------------------------------------------
        public void ShowChild(int index, object dataContext = null)
        {
            // hide all items except the item with the specified index
            foreach (FrameworkElement it in Items) it.Visibility = Visibility.Collapsed;
            ItemsTransformY = ActualHeight;
            var item = Items[index] as SlideControlBase;
            item.Visibility = Visibility.Visible;
            if (dataContext != null) (item.Content as FrameworkElement).DataContext = dataContext;
            item.OnPropertyChanged("Headers");
            var sb = new Storyboard();
            // add animation to slide out the content panel
            var slideOutAnim = new DoubleAnimation {Duration = AnimDuration};
            sb.Children.Add(slideOutAnim);
            Storyboard.SetTarget(slideOutAnim, this);
            // add animation to slide in the items panel
            var slideInAnim = new DoubleAnimation {Duration = AnimDuration};
            sb.Children.Add(slideInAnim);
            Storyboard.SetTarget(slideInAnim, this);
            // add clip animation to slide out the content panel
            var slideOutClipAnim = new RectAnimation {Duration = AnimDuration};
            sb.Children.Add(slideOutClipAnim);
            Storyboard.SetTarget(slideOutClipAnim, this);
            Storyboard.SetTargetProperty(slideOutClipAnim, new PropertyPath(ContentClippingRectProperty));
            // add clip animation to slide in the items panel
            var slideInClipAnim = new RectAnimation {Duration = AnimDuration};
            sb.Children.Add(slideInClipAnim);
            Storyboard.SetTarget(slideInClipAnim, this);
            Storyboard.SetTargetProperty(slideInClipAnim, new PropertyPath(ItemsClippingRectProperty));
            switch (item.SlideDirection)
            {
                case SlideDirection.Left:
                    slideOutAnim.From = 0;
                    slideOutAnim.To = -ActualWidth;
                    slideOutClipAnim.From = new Rect(0, 0, ActualWidth, ActualHeight);
                    slideOutClipAnim.To = new Rect(ActualWidth, 0, 0, ActualHeight);
                    slideInClipAnim.From = new Rect(0, 0, 0, ActualHeight);
                    slideInClipAnim.To = new Rect(0, 0, ActualWidth, ActualHeight);
                    Storyboard.SetTargetProperty(slideOutAnim, new PropertyPath(ContentTransformXProperty));
                    slideInAnim.From = ActualWidth;
                    slideInAnim.To = 0;
                    Storyboard.SetTargetProperty(slideInAnim, new PropertyPath(ItemsTransformXProperty));
                    ContentTransformY = 0;
                    ItemsTransformY = 0;
                    break;
                case SlideDirection.Right:
                    slideOutAnim.From = 0;
                    slideOutAnim.To = ActualWidth;
                    slideOutClipAnim.From = new Rect(0, 0, ActualWidth, ActualHeight);
                    slideOutClipAnim.To = new Rect(0, 0, 0, ActualHeight);
                    slideInClipAnim.From = new Rect(0, ActualWidth, 0, ActualHeight);
                    slideInClipAnim.To = new Rect(0, 0, ActualWidth, ActualHeight);
                    Storyboard.SetTargetProperty(slideOutAnim, new PropertyPath(ContentTransformXProperty));
                    slideInAnim.From = -ActualWidth;
                    slideInAnim.To = 0;
                    Storyboard.SetTargetProperty(slideInAnim, new PropertyPath(ItemsTransformXProperty));
                    ContentTransformY = 0;
                    ItemsTransformY = 0;
                    break;
                case SlideDirection.Up:
                    slideOutAnim.From = 0;
                    slideOutAnim.To = -ActualHeight;
                    slideOutClipAnim.From = new Rect(0, 0, ActualWidth, ActualHeight);
                    slideOutClipAnim.To = new Rect(0, ActualHeight, ActualWidth, 0);
                    slideInClipAnim.From = new Rect(0, 0, ActualWidth, ActualHeight);
                    slideInClipAnim.To = new Rect(0, 0, ActualWidth, ActualHeight);
                    Storyboard.SetTargetProperty(slideOutAnim, new PropertyPath(ContentTransformYProperty));
                    slideInAnim.From = ActualHeight;
                    slideInAnim.To = 0;
                    Storyboard.SetTargetProperty(slideInAnim, new PropertyPath(ItemsTransformYProperty));
                    ContentTransformX = 0;
                    ItemsTransformX = 0;
                    break;
                case SlideDirection.Down:
                    slideOutAnim.From = 0;
                    slideOutAnim.To = ActualHeight;
                    slideOutClipAnim.From = new Rect(0, 0, ActualWidth, ActualHeight);
                    slideOutClipAnim.To = new Rect(0, 0, ActualWidth, 0);
                    slideInClipAnim.From = new Rect(0, ActualHeight, ActualWidth, ActualHeight);
                    slideInClipAnim.To = new Rect(0, 0, ActualWidth, ActualHeight);
                    Storyboard.SetTargetProperty(slideOutAnim, new PropertyPath(ContentTransformYProperty));
                    slideInAnim.From = -ActualHeight;
                    slideInAnim.To = 0;
                    Storyboard.SetTargetProperty(slideInAnim, new PropertyPath(ItemsTransformYProperty));
                    ContentTransformX = 0;
                    ItemsTransformX = 0;
                    break;
                default:
                    break;
            }
            // hide content panel after animation finished
            sb.Completed += show_child_completed;
            // update selected content
            DependencyObject root = this;
            while (root != null && !(root is SlideControl)) root = VisualTreeHelper.GetParent(root);
            var sc = root as SlideControl;
            sc.SelectedContent = item;
            // begin animation
            UseClipping = true;
            sb.Begin(this);
        }

        //--------------------------------------------------------------------------------

        #endregion ShowChild
    }
}