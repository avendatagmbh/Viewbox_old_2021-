using System.Windows;

namespace AvdWpfControls
{
    public class MonetaryTextBox : NumericTextbox
    {
        static MonetaryTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (MonetaryTextBox),
                                                     new FrameworkPropertyMetadata(typeof (MonetaryTextBox)));
        }

        #region CurrencyString

        // Using a DependencyProperty as the backing store for CurrencyString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrencyStringProperty =
            DependencyProperty.Register("CurrencyString", typeof (string), typeof (MonetaryTextBox),
                                        new UIPropertyMetadata("€"));

        public string CurrencyString
        {
            get { return (string) GetValue(CurrencyStringProperty); }
            set { SetValue(CurrencyStringProperty, value); }
        }

        #endregion CurrencyString
    }
}