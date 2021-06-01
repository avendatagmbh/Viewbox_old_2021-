// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-02-26
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;

namespace AvdWpfControls {
    public class MonetaryTextBox : NumericTextbox {
        static MonetaryTextBox() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (MonetaryTextBox),
                                                     new FrameworkPropertyMetadata(typeof (MonetaryTextBox)));
        }

        #region CurrencyString
        public string CurrencyString { get { return (string) GetValue(CurrencyStringProperty); } set { SetValue(CurrencyStringProperty, value); } }

        // Using a DependencyProperty as the backing store for CurrencyString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrencyStringProperty =
            DependencyProperty.Register("CurrencyString", typeof (string), typeof (MonetaryTextBox),
                                        new UIPropertyMetadata("€"));

        #endregion CurrencyString


    }
}