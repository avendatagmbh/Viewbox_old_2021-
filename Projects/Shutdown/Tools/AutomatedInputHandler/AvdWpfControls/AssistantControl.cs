// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-08
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AvdWpfControls {
    public class AssistantControl : AssistantControlTabPanel {
        static AssistantControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (AssistantControl),
                                                     new FrameworkPropertyMetadata(typeof (AssistantControl)));            
        }

        public AssistantControl() {
            Buttons = new ObservableCollection<Button>();
            ReplaceFooter = new ObservableCollection<Border>();
            AdditionalHeaderContent = new StackPanel();
        }

        #region Ok
        public static readonly RoutedEvent OkEvent = EventManager.RegisterRoutedEvent(
            "Ok", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (AssistantControl));

        public event RoutedEventHandler Ok { add { AddHandler(OkEvent, value); } remove { RemoveHandler(OkEvent, value); } }
        internal void OnOk() { RaiseEvent(new RoutedEventArgs(OkEvent)); }

        #endregion Ok

        #region Finish Event
        public static readonly RoutedEvent FinishEvent = EventManager.RegisterRoutedEvent(
            "Finish", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (AssistantControl));

        public event RoutedEventHandler Finish { add { AddHandler(FinishEvent, value); } remove { RemoveHandler(FinishEvent, value); } }

        internal void OnFinish() {
            RaiseEvent(new RoutedEventArgs(FinishEvent));
        }

        #endregion Finish
        

        #region Cancel
        public static readonly RoutedEvent CancelEvent = EventManager.RegisterRoutedEvent(
            "Cancel", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AssistantControl));

        public event RoutedEventHandler Cancel { add { AddHandler(CancelEvent, value); } remove { RemoveHandler(CancelEvent, value); } }
        internal void OnCancel() { RaiseEvent(new RoutedEventArgs(CancelEvent)); }

        #endregion Cancel
       
        #region OkButtonCaption
        public string OkButtonCaption { get { return (string)GetValue(OkButtonCaptionProperty); } set { SetValue(OkButtonCaptionProperty, value); } }
        public static readonly DependencyProperty OkButtonCaptionProperty =
            DependencyProperty.Register("OkButtonCaption", typeof(string), typeof(AssistantControl), new UIPropertyMetadata(null));
        #endregion OkButtonCaption

        #region OkButtonEnabled
        public bool OkButtonEnabled { get { return (bool)GetValue(OkButtonEnabledProperty); } set { SetValue(OkButtonEnabledProperty, value); } }
        public static readonly DependencyProperty OkButtonEnabledProperty =
            DependencyProperty.Register("OkButtonEnabled", typeof(bool), typeof(AssistantControl), new UIPropertyMetadata(false));
        #endregion OkButtonEnabled

        #region CancelButtonEnabled
        public bool CancelButtonEnabled { get { return (bool)GetValue(CancelButtonEnabledProperty); } set { SetValue(CancelButtonEnabledProperty, value); } }
        public static readonly DependencyProperty CancelButtonEnabledProperty =
            DependencyProperty.Register("CancelButtonEnabled", typeof(bool), typeof(AssistantControl), new UIPropertyMetadata(true));
        #endregion CancelButtonEnabled

        #region CancelButtonVisibility
        public Visibility CancelButtonVisibility { get { return (Visibility)GetValue(CancelButtonVisibilityProperty); } set { SetValue(CancelButtonVisibilityProperty, value); } }
        public static readonly DependencyProperty CancelButtonVisibilityProperty =
            DependencyProperty.Register("CancelButtonVisibility", typeof(Visibility), typeof(AssistantControl), new UIPropertyMetadata(Visibility.Visible));
        #endregion CancelButtonVisibility

        #region IsNavigationButtonVisible
        public bool IsNavigationButtonVisible { get { return (bool)GetValue(IsNavigationButtonVisibleProperty); } set { SetValue(IsNavigationButtonVisibleProperty, value); } }
        public static readonly DependencyProperty IsNavigationButtonVisibleProperty =
            DependencyProperty.Register("IsNavigationButtonVisible", typeof(bool), typeof(AssistantControl), new UIPropertyMetadata(true));
        #endregion IsNavigationButtonVisible

        #region IsButtonBorderVisible
        public bool IsButtonBorderVisible { get { return (bool)GetValue(IsButtonBorderVisibleProperty); } set { SetValue(IsButtonBorderVisibleProperty, value); } }
        public static readonly DependencyProperty IsButtonBorderVisibleProperty =
            DependencyProperty.Register("IsButtonBorderVisible", typeof(bool), typeof(AssistantControl), new UIPropertyMetadata(true));
        #endregion IsButtonBorderVisible

        #region ButtonBorderStyle
        public Style ButtonBorderStyle { get { return (Style)GetValue(ButtonBorderStyleProperty); } 
            set { SetValue(ButtonBorderStyleProperty, value); } }

        public static readonly DependencyProperty ButtonBorderStyleProperty =
            DependencyProperty.Register("ButtonBorderStyle", typeof (Style), typeof (AssistantControl),
                                        new UIPropertyMetadata(new Border().Style));
        #endregion ButtonBorderStyle   

        #region HeaderBorderStyle
        public Style HeaderBorderStyle { get { return (Style)GetValue(HeaderBorderStyleProperty); } set { SetValue(HeaderBorderStyleProperty, value); } }

        public static readonly DependencyProperty HeaderBorderStyleProperty =
            DependencyProperty.Register("HeaderBorderStyle", typeof(Style), typeof(AssistantControl),
                                        new UIPropertyMetadata(new Border().Style));
        #endregion HeaderBorderStyle       

        #region Buttons
        public ObservableCollection<Button> Buttons { get { return (ObservableCollection<Button>)GetValue(ButtonsProperty); } set { SetValue(ButtonsProperty, value); } }

        // Using a DependencyProperty as the backing store for Buttons.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonsProperty =
            DependencyProperty.Register("Buttons", typeof(ObservableCollection<Button>), typeof(AssistantControl));
        #endregion Buttons

        #region ReplaceFooter
        public ObservableCollection<Border> ReplaceFooter {
            get { return (ObservableCollection<Border>) GetValue(ReplaceFooterProperty); }
            set {
                SetValue(ReplaceFooterProperty, value);
                if (value != null && value.Count > 0) {
                    IsButtonBorderVisible = false;
                }
            }
        }

        // Using a DependencyProperty as the backing store for ReplaceFooter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReplaceFooterProperty =
            DependencyProperty.Register("ReplaceFooter", typeof(ObservableCollection<Border>), typeof(AssistantControl));
        #endregion ReplaceFooter

        #region DetailHeaderBackground
        public Brush DetailHeaderBackground {
            get { return (Brush)GetValue(DetailHeaderBackgroundProperty); }
            set { SetValue(DetailHeaderBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DetailHeaderBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DetailHeaderBackgroundProperty =
            DependencyProperty.Register("DetailHeaderBackground", typeof(Brush), typeof(AssistantControl),
                                        new UIPropertyMetadata(new SolidColorBrush(Color.FromRgb(224, 224, 224))));


        #endregion DetailHeaderBackground


        #region AdditionalHeaderContent
        public StackPanel AdditionalHeaderContent {
            get { return (StackPanel)GetValue(AdditionalHeaderContentProperty); }
            set { SetValue(AdditionalHeaderContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DetailHeaderBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AdditionalHeaderContentProperty =
            DependencyProperty.Register("AdditionalHeaderContent", typeof(StackPanel), typeof(AssistantControl));


        #endregion AdditionalHeaderContent

        #region ShowSteps
        public bool ShowSteps {
            get { return (bool)GetValue(ShowStepsProperty); }
            set { SetValue(ShowStepsProperty, value); }
        }

        public static readonly DependencyProperty ShowStepsProperty =
            DependencyProperty.Register("ShowSteps", typeof(bool), typeof(AssistantControl), new UIPropertyMetadata(false));

        
        #endregion ShowSteps

        #region CloseOnCancelProperty
        public static readonly DependencyProperty CloseOnCancelProperty =
            DependencyProperty.Register("CloseOnCancel", typeof (bool), typeof (AssistantControl),
                                        new PropertyMetadata(true));

        public bool CloseOnCancel { get { return (bool) GetValue(CloseOnCancelProperty); } set { SetValue(CloseOnCancelProperty, value); } }

        #endregion CloseOnCancelProperty

        #region NextButtonCaptionLastPage
        public static readonly DependencyProperty NextButtonCaptionLastPageProperty =
            DependencyProperty.Register("NextButtonCaptionLastPage", typeof (string), typeof (AssistantControl),
                                        new PropertyMetadata(default(string)));

        public string NextButtonCaptionLastPage { get { return (string) GetValue(NextButtonCaptionLastPageProperty); } set { SetValue(NextButtonCaptionLastPageProperty, value); } }

        #endregion NextButtonCaptionLastPage
    }
}
