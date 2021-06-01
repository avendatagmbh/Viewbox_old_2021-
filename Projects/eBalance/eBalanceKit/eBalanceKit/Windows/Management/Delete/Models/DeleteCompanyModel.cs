// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Utils;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Management.Delete.Models {
    public class DeleteCompanyModel : NotifyPropertyChangedBase {
        public DeleteCompanyModel(Window owner) {
            Owner = owner;
            _items = new ObservableCollectionAsync<CheckableItemWrapper<Company>>();
            foreach (var item in
                (from c in CompanyManager.Instance.AllowedCompanies
                  where !c.HasAnyAssignedDocument
                  select new CheckableItemWrapper<Company>(c))) _items.Add(item);

            foreach (var item in Items)
                item.PropertyChanged += (sender, args) => OnPropertyChanged("DeleteButtonEnabled");
        }

        private Window Owner { get; set; }

        #region Items
        private readonly ObservableCollectionAsync<CheckableItemWrapper<Company>> _items;
        public IEnumerable<CheckableItemWrapper<Company>> Items { get { return _items; } }
        #endregion // Items

        public void DeleteSelectedItems() {
            if (
                MessageBox.Show(Owner, ResourcesManamgement.RequestDeleteAllSelectedCompanies, string.Empty, MessageBoxButton.YesNo,
                                MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes) return;

            try {
                foreach (var company in SelectedItems.ToList()) {
                    CompanyManager.Instance.DeleteCompany(company.Item);
                    _items.Remove(company);
                }
            } catch (Exception ex) {
                throw new Exception(ResourcesManamgement.DeleteAllSelectedCompaniesFailed + ex.Message);
            }

            MessageBox.Show(Owner, ResourcesManamgement.DeleteAllSelectedCompaniesSucceed);
        }

        public IEnumerable<CheckableItemWrapper<Company>> SelectedItems { get { return from c in Items where c.IsChecked select c; } }

        #region DeleteButtonEnabled
        public bool DeleteButtonEnabled { get { return SelectedItems.Any(); } }
        #endregion // DeleteButtonEnabled
    }
}