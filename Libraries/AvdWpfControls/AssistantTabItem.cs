using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Utils.Commands;

namespace AvdWpfControls
{
    public class AssistantTabItem : TabItem
    {
        static AssistantTabItem()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof (AssistantTabItem),
            //                                         new FrameworkPropertyMetadata(typeof (AssistantTabItem)));
        }

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region NextAllowed

        public static readonly DependencyProperty NextAllowedProperty =
            DependencyProperty.Register("NextAllowed", typeof (bool), typeof (AssistantTabItem),
                                        new UIPropertyMetadata(true));

        public bool NextAllowed
        {
            get { return (bool) GetValue(NextAllowedProperty); }
            set
            {
                SetValue(NextAllowedProperty, value);
                OnPropertyChanged("NextAllowed");
            }
        }

        #endregion NextAllowed

        #region Command

        #region OnSelected

        public static readonly DependencyProperty CommandOnSelectedProperty =
            DependencyProperty.Register("CommandOnSelected", typeof (DelegateCommand), typeof (AssistantTabItem));

        public DelegateCommand CommandOnSelected
        {
            get { return (DelegateCommand) GetValue(CommandOnSelectedProperty); }
            set
            {
                SetValue(CommandOnSelectedProperty, value);
                OnPropertyChanged("CommandOnSelected");
            }
        }

        protected override void OnSelected(RoutedEventArgs e)
        {
            if (CommandOnSelected != null)
            {
                CommandOnSelected.Execute(CommandOnSelectedParameter ?? e);
            }
            base.OnSelected(e);
        }

        #region CommandOnSelectedParameter

        public static readonly DependencyProperty CommandOnSelectedParameterProperty =
            DependencyProperty.Register("CommandOnSelectedParameter", typeof (object), typeof (AssistantTabItem),
                                        new UIPropertyMetadata(null));

        public object CommandOnSelectedParameter
        {
            get { return GetValue(CommandOnSelectedParameterProperty); }
            set { SetValue(CommandOnSelectedParameterProperty, value); }
        }

        #endregion CommandOnSelectedParameter

        #endregion OnSelected

        #endregion Command

        #region BackAllowed

        public static readonly DependencyProperty BackAllowedProperty =
            DependencyProperty.Register("BackAllowed", typeof (bool), typeof (AssistantTabItem),
                                        new UIPropertyMetadata(true));

        public bool BackAllowed
        {
            get { return (bool) GetValue(BackAllowedProperty); }
            set { SetValue(BackAllowedProperty, value); }
        }

        #endregion BackAllowed

        #region IsSummaryPage

        public static readonly DependencyProperty IsSummaryPageProperty =
            DependencyProperty.Register("IsSummaryPage", typeof (bool), typeof (AssistantTabItem),
                                        new UIPropertyMetadata(false));

        public bool IsSummaryPage
        {
            get { return (bool) GetValue(IsSummaryPageProperty); }
            set { SetValue(IsSummaryPageProperty, value); }
        }

        #endregion IsSummaryPage

        #region DetailHeader

        // Using a DependencyProperty as the backing store for DetailHeader.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DetailHeaderProperty =
            DependencyProperty.Register("DetailHeader", typeof (string), typeof (AssistantTabItem),
                                        new UIPropertyMetadata(null));

        public string DetailHeader
        {
            get { return (string) GetValue(DetailHeaderProperty); }
            set { SetValue(DetailHeaderProperty, value); }
        }

        #endregion DetailHeader

        #region DetailHeaderImageSource

        // Using a DependencyProperty as the backing store for DetailHeaderImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DetailHeaderImageSourceProperty =
            DependencyProperty.Register("DetailHeaderImageSource", typeof (string), typeof (AssistantTabItem),
                                        new UIPropertyMetadata(null));

        public string DetailHeaderImageSource
        {
            get { return (string) GetValue(DetailHeaderImageSourceProperty); }
            set { SetValue(DetailHeaderImageSourceProperty, value); }
        }

        #endregion DetailHeader

        #region DetailHeaderImageHeight

        // Using a DependencyProperty as the backing store for DetailHeaderImageHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DetailHeaderImageHeightProperty =
            DependencyProperty.Register("DetailHeaderImageHeight", typeof (double), typeof (AssistantTabItem),
                                        new UIPropertyMetadata((double) 24));

        public double DetailHeaderImageHeight
        {
            get { return (double) GetValue(DetailHeaderImageHeightProperty); }
            set { SetValue(DetailHeaderImageHeightProperty, value); }
        }

        #endregion DetailHeaderImageHeight
    }
}