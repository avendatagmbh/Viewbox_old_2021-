// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-06-21
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using eBalanceKit.Models;
using eBalanceKitBusiness.Manager;

namespace eBalanceKit.Controls {
    /// <summary>
    /// Interaktionslogik für CtlTaxonomyTreeViewConfig.xaml
    /// </summary>
    public partial class CtlTaxonomyTreeViewConfig : UserControl {
        public CtlTaxonomyTreeViewConfig() {
            InitializeComponent();
        }

        private TaxonomyViewModel Model {
            get { return DataContext as TaxonomyViewModel; }
        }

        private void btnSendAllAccountBalances_Click(object sender, RoutedEventArgs e) {
            if (Model == null) return;
            if (!RightManager.WriteRestDocumentAllowed(Model.Document)) {
                return;
            }
            Model.Document.ValueTreeMain.SetSendAccountBalancesFlag(true, Model.PresentationTree);
        }

        private void btnSendNoAccountBalances_Click(object sender, RoutedEventArgs e) {
            if (Model == null) return;
            if (!RightManager.WriteRestDocumentAllowed(Model.Document)) {
                return;
            }
            Model.Document.ValueTreeMain.SetSendAccountBalancesFlag(false, Model.PresentationTree);
        }

        private void btnSendWishedAccountBalances_Click(object sender, RoutedEventArgs e) {
            if (Model == null) return;
            if (!RightManager.WriteRestDocumentAllowed(Model.Document)) {
                return;
            }
            Model.Document.ValueTreeMain.SetSendAccountBalancesFlagIfWished(Model.PresentationTree);
        }
    }
}