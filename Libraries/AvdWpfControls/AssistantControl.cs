using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AvdWpfControls
{
    public class AssistantControl : AssistantControlTabPanel
    {
        public static readonly DependencyProperty CloseOnCancelProperty =
            DependencyProperty.Register("CloseOnCancel", typeof (bool), typeof (AssistantControl),
                                        new PropertyMetadata(true));

        static AssistantControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (AssistantControl),
                                                     new FrameworkPropertyMetadata(typeof (AssistantControl)));
        }

        public AssistantControl()
        {
            Buttons = new ObservableCollection<Button>();
            ReplaceFooter = new ObservableCollection<Border>();
        }

        #region Ok

        public static readonly RoutedEvent OkEvent = EventManager.RegisterRoutedEvent(
            "Ok", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (AssistantControl));

        public event RoutedEventHandler Ok
        {
            add { AddHandler(OkEvent, value); }
            remove { RemoveHandler(OkEvent, value); }
        }

        internal void OnOk()
        {
            RaiseEvent(new RoutedEventArgs(OkEvent));
        }

        #endregion Ok

        #region Cancel

        public static readonly RoutedEvent CancelEvent = EventManager.RegisterRoutedEvent(
            "Cancel", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (AssistantControl));

        public event RoutedEventHandler Cancel
        {
            add { AddHandler(CancelEvent, value); }
            remove { RemoveHandler(CancelEvent, value); }
        }

        internal void OnCancel()
        {
            RaiseEvent(new RoutedEventArgs(CancelEvent));
        }

        #endregion Cancel

        #region OkButtonCaption

        public static readonly DependencyProperty OkButtonCaptionProperty =
            DependencyProperty.Register("OkButtonCaption", typeof (string), typeof (AssistantControl),
                                        new UIPropertyMetadata(null));

        public string OkButtonCaption
        {
            get { return (string) GetValue(OkButtonCaptionProperty); }
            set { SetValue(OkButtonCaptionProperty, value); }
        }

        #endregion OkButtonCaption

        #region OkButtonEnabled

        public static readonly DependencyProperty OkButtonEnabledProperty =
            DependencyProperty.Register("OkButtonEnabled", typeof (bool), typeof (AssistantControl),
                                        new UIPropertyMetadata(false));

        public bool OkButtonEnabled
        {
            get { return (bool) GetValue(OkButtonEnabledProperty); }
            set { SetValue(OkButtonEnabledProperty, value); }
        }

        #endregion OkButtonEnabled

        #region CancelButtonEnabled

        public static readonly DependencyProperty CancelButtonEnabledProperty =
            DependencyProperty.Register("CancelButtonEnabled", typeof (bool), typeof (AssistantControl),
                                        new UIPropertyMetadata(true));

        public bool CancelButtonEnabled
        {
            get { return (bool) GetValue(CancelButtonEnabledProperty); }
            set { SetValue(CancelButtonEnabledProperty, value); }
        }

        #endregion CancelButtonEnabled

        #region CancelButtonVisibility

        public static readonly DependencyProperty CancelButtonVisibilityProperty =
            DependencyProperty.Register("CancelButtonVisibility", typeof (Visibility), typeof (AssistantControl),
                                        new UIPropertyMetadata(Visibility.Visible));

        public Visibility CancelButtonVisibility
        {
            get { return (Visibility) GetValue(CancelButtonVisibilityProperty); }
            set { SetValue(CancelButtonVisibilityProperty, value); }
        }

        #endregion CancelButtonVisibility

        #region IsNavigationButtonVisible

        public static readonly DependencyProperty IsNavigationButtonVisibleProperty =
            DependencyProperty.Register("IsNavigationButtonVisible", typeof (bool), typeof (AssistantControl),
                                        new UIPropertyMetadata(true));

        public bool IsNavigationButtonVisible
        {
            get { return (bool) GetValue(IsNavigationButtonVisibleProperty); }
            set { SetValue(IsNavigationButtonVisibleProperty, value); }
        }

        #endregion IsNavigationButtonVisible

        #region ButtonBorderStyle

        public static readonly DependencyProperty ButtonBorderStyleProperty =
            DependencyProperty.Register("ButtonBorderStyle", typeof (Style), typeof (AssistantControl),
                                        new UIPropertyMetadata(new Border().Style));

        public Style ButtonBorderStyle
        {
            get { return (Style) GetValue(ButtonBorderStyleProperty); }
            set { SetValue(ButtonBorderStyleProperty, value); }
        }

        #endregion ButtonBorderStyle       

        #region Buttons

        // Using a DependencyProperty as the backing store for Buttons.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonsProperty =
            DependencyProperty.Register("Buttons", typeof (ObservableCollection<Button>), typeof (AssistantControl));

        public ObservableCollection<Button> Buttons
        {
            get { return (ObservableCollection<Button>) GetValue(ButtonsProperty); }
            set { SetValue(ButtonsProperty, value); }
        }

        #endregion Buttons

        #region ReplaceFooter

        // Using a DependencyProperty as the backing store for ReplaceFooter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReplaceFooterProperty =
            DependencyProperty.Register("ReplaceFooter", typeof (ObservableCollection<Border>),
                                        typeof (AssistantControl));

        public ObservableCollection<Border> ReplaceFooter
        {
            get { return (ObservableCollection<Border>) GetValue(ReplaceFooterProperty); }
            set { SetValue(ReplaceFooterProperty, value); }
        }

        #endregion ReplaceFooter

        #region DetailHeaderBackground

        // Using a DependencyProperty as the backing store for DetailHeaderBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DetailHeaderBackgroundProperty =
            DependencyProperty.Register("DetailHeaderBackground", typeof (Brush), typeof (AssistantControl),
                                        new UIPropertyMetadata(new SolidColorBrush(Color.FromRgb(224, 224, 224))));

        public Brush DetailHeaderBackground
        {
            get { return (Brush) GetValue(DetailHeaderBackgroundProperty); }
            set { SetValue(DetailHeaderBackgroundProperty, value); }
        }

        #endregion DetailHeaderBackground

        #region ShowSteps

        public static readonly DependencyProperty ShowStepsProperty =
            DependencyProperty.Register("ShowSteps", typeof (bool), typeof (AssistantControl),
                                        new UIPropertyMetadata(false));

        public bool ShowSteps
        {
            get { return (bool) GetValue(ShowStepsProperty); }
            set { SetValue(ShowStepsProperty, value); }
        }

        #endregion ShowSteps

        public bool CloseOnCancel
        {
            get { return (bool) GetValue(CloseOnCancelProperty); }
            set { SetValue(CloseOnCancelProperty, value); }
        }
    }
}