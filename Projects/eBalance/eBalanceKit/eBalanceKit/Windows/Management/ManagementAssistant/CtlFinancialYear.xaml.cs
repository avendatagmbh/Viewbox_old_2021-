using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using eBalanceKitBusiness.Structures;

namespace eBalanceKit.Windows.Management.ManagementAssistant {
    /// <summary>
    /// Interaktionslogik für CtlFinancialYear.xaml
    /// </summary>
    public partial class CtlFinancialYear : UserControl {
        public CtlFinancialYear() {
            InitializeComponent();
            dpBalSheetClosingDate.Language = XmlLanguage.GetLanguage(AppConfig.SelectedLanguage.Culture.Name);
            dpBalSheetClosingDatePreviousYear.Language = XmlLanguage.GetLanguage(AppConfig.SelectedLanguage.Culture.Name);
            dpFiscalYearBegin.Language = XmlLanguage.GetLanguage(AppConfig.SelectedLanguage.Culture.Name);
            dpFiscalYearBeginPrevious.Language = XmlLanguage.GetLanguage(AppConfig.SelectedLanguage.Culture.Name);
            dpFiscalYearEnd.Language = XmlLanguage.GetLanguage(AppConfig.SelectedLanguage.Culture.Name);
            dpFiscalYearEndPrevious.Language = XmlLanguage.GetLanguage(AppConfig.SelectedLanguage.Culture.Name);
        }
    }
}
