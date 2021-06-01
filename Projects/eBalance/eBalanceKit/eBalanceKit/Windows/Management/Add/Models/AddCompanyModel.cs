// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Windows;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Management.Add.Models {
    public class AddCompanyModel {
        public AddCompanyModel(Window owner) {
            Owner = owner;

            try {
                Company = new Company();
                CompanyManager.Instance.AddCompany(Company);
                Company.SetFinancialYearIntervall(2009, 2030);
                Company.SetVisibleFinancialYearIntervall(DateTime.Now.Year, DateTime.Now.Year);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, ResourcesCommon.ErrorsCaption, MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        public Company Company { get; private set; }
        public Window Owner { get; private set; }

        public void Cancel() { CompanyManager.Instance.DeleteCompany(Company); }
    }
}