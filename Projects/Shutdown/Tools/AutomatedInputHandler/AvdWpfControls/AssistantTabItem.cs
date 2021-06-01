// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-03-09
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Utils.Commands;

namespace AvdWpfControls {
    public class AssistantTabItem : TabItem {
        static AssistantTabItem() {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof (AssistantTabItem),
            //                                         new FrameworkPropertyMetadata(typeof (AssistantTabItem)));
        }

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion

        #region NextAllowed
        public bool NextAllowed { get { return (bool) GetValue(NextAllowedProperty); } set { SetValue(NextAllowedProperty, value); OnPropertyChanged("NextAllowed"); } }

        public static readonly DependencyProperty NextAllowedProperty =
            DependencyProperty.Register("NextAllowed", typeof (bool), typeof (AssistantTabItem),
                                        new UIPropertyMetadata(true));
        #endregion NextAllowed

        #region Command

        #region OnSelected
        public DelegateCommand CommandOnSelected { get { return (DelegateCommand)GetValue(CommandOnSelectedProperty); } set { SetValue(CommandOnSelectedProperty, value); OnPropertyChanged("CommandOnSelected"); } }

        public static readonly DependencyProperty CommandOnSelectedProperty =
            DependencyProperty.Register("CommandOnSelected", typeof(DelegateCommand), typeof(AssistantTabItem));

        protected override void OnSelected(RoutedEventArgs e) {
            if (CommandOnSelected != null) {
                CommandOnSelected.Execute(CommandOnSelectedParameter ?? e);
            }
            base.OnSelected(e);
        }

        #region CommandOnSelectedParameter
        public object CommandOnSelectedParameter { get { return GetValue(CommandOnSelectedParameterProperty); } set { SetValue(CommandOnSelectedParameterProperty, value); } }

        public static readonly DependencyProperty CommandOnSelectedParameterProperty =
            DependencyProperty.Register("CommandOnSelectedParameter", typeof(object), typeof(AssistantTabItem),
                                        new UIPropertyMetadata(null));
        #endregion CommandOnSelectedParameter

        #endregion OnSelected



        #endregion Command

        #region BackAllowed
        public bool BackAllowed {
            get { return (bool)GetValue(BackAllowedProperty); }
            set { SetValue(BackAllowedProperty, value); }
        }

        public static readonly DependencyProperty BackAllowedProperty =
            DependencyProperty.Register("BackAllowed", typeof (bool), typeof (AssistantTabItem),
                                        new UIPropertyMetadata(true));

        
        #endregion BackAllowed

        #region IsSummaryPage
        public bool IsSummaryPage {
            get { return (bool)GetValue(IsSummaryPageProperty); }
            set { SetValue(IsSummaryPageProperty, value); }
        }

        public static readonly DependencyProperty IsSummaryPageProperty =
            DependencyProperty.Register("IsSummaryPage", typeof(bool), typeof(AssistantTabItem), new UIPropertyMetadata(false));
        #endregion IsSummaryPage

        #region DetailHeader
        public string DetailHeader { get { return (string) GetValue(DetailHeaderProperty); } set { SetValue(DetailHeaderProperty, value); } }

        // Using a DependencyProperty as the backing store for DetailHeader.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DetailHeaderProperty =
            DependencyProperty.Register("DetailHeader", typeof (string), typeof (AssistantTabItem),
                                        new UIPropertyMetadata(null));
        #endregion DetailHeader

        #region DetailHeaderImageSource
        public string DetailHeaderImageSource { get { return (string)GetValue(DetailHeaderImageSourceProperty); } set { SetValue(DetailHeaderImageSourceProperty, value); } }

        // Using a DependencyProperty as the backing store for DetailHeaderImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DetailHeaderImageSourceProperty =
            DependencyProperty.Register("DetailHeaderImageSource", typeof(string), typeof(AssistantTabItem),
                                        new UIPropertyMetadata(null));
        #endregion DetailHeader
        
        #region DetailHeaderImageHeight
        public double DetailHeaderImageHeight { get { return (double) GetValue(DetailHeaderImageHeightProperty); } set { SetValue(DetailHeaderImageHeightProperty, value); } }

        // Using a DependencyProperty as the backing store for DetailHeaderImageHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DetailHeaderImageHeightProperty =
            DependencyProperty.Register("DetailHeaderImageHeight", typeof (double), typeof (AssistantTabItem),
                                        new UIPropertyMetadata((double)24));

        #endregion DetailHeaderImageHeight
    }
}