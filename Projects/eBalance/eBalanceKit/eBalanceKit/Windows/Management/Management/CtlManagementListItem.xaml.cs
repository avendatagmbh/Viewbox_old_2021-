// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-21
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using eBalanceKit.Windows.Management.Edit;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Management.Management {
    public partial class CtlManagementListItem {
        public CtlManagementListItem() {
            InitializeComponent();
            DataContextChanged += (sender, args) => {
                var type = DataContext.GetType();
                if (type.GetProperty("Comment") == null)
                    txtComment.Text = null;
            };
        }

        //private void BtnDeleteClick(object sender, RoutedEventArgs e) {
        //    if (DataContext is eBalanceKitBusiness.Structures.DbMapping.System) {
        //        var system = (eBalanceKitBusiness.Structures.DbMapping.System) DataContext;
        //        if (
        //            MessageBox.Show(string.Format(ResourcesManamgement.RequestDeleteSystem, system.Name), string.Empty,
        //                            MessageBoxButton.YesNo,
        //                            MessageBoxImage.Question,
        //                            MessageBoxResult.No) == MessageBoxResult.Yes) {
        //                SystemManager.Instance.DeleteSystem(system);
        //        }
        //    } else if (DataContext is Company) {
        //        var company = (Company) DataContext;
        //        if (
        //            MessageBox.Show(string.Format(ResourcesManamgement.RequestDeleteCompany, company.Name), string.Empty,
        //                            MessageBoxButton.YesNo,
        //                            MessageBoxImage.Question,
        //                            MessageBoxResult.No) == MessageBoxResult.Yes) {
        //                                CompanyManager.Instance.DeleteCompany(company);
        //        }
        //    }
        //}

        private void Edit() {
            if (DataContext is eBalanceKitBusiness.Structures.DbMapping.System) {
                var system = (eBalanceKitBusiness.Structures.DbMapping.System)DataContext;
                new DlgEditSystem(system) { Owner = UIHelpers.TryFindParent<Window>(this) }.ShowDialog();
            } else if (DataContext is Company) {
                var company = (Company)DataContext;
                new DlgEditCompany(company) { Owner = UIHelpers.TryFindParent<Window>(this) }.ShowDialog();
            }
        }

        private void ClickableControlMouseClick(object sender, RoutedEventArgs e) { Edit(); }

    }
}