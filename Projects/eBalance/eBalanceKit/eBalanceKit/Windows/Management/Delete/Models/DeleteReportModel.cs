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
    public class DeleteReportModel : NotifyPropertyChangedBase {
        public DeleteReportModel(Window owner) {
            Owner = owner;
            _items =
                new ObservableCollectionAsync<CheckableItemWrapper<Document>>();
            foreach (var item in
                (from d in DocumentManager.Instance.AllowedDocuments
                 orderby d.System, d.Company, d.FinancialYear, d.Name
                 select new CheckableItemWrapper<Document>(d))) _items.Add(item);

            foreach (var item in Items)
                item.PropertyChanged += (sender, args) => OnPropertyChanged("DeleteButtonEnabled");
        }

        private Window Owner { get; set; }

        #region Items
        private readonly
            ObservableCollectionAsync<CheckableItemWrapper<Document>> _items;

        public IEnumerable<CheckableItemWrapper<Document>> Items { get { return _items; } }
        #endregion // Items

        public void DeleteSelectedItems() {
            if (
                MessageBox.Show(Owner, ResourcesManamgement.RequestDeleteAllSelectedReports, string.Empty,
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes) return;

            try {
                foreach (var document in SelectedItems.ToList()) {
                    DocumentManager.Instance.DeleteDocument(document.Item);
                    _items.Remove(document);
                }
            } catch (Exception ex) {
                throw new Exception(ResourcesManamgement.DeleteAllSelectedReportsFailed + ex.Message);
            }

            MessageBox.Show(Owner, ResourcesManamgement.DeleteAllSelectedReportsSucceed);

        }

        public IEnumerable<CheckableItemWrapper<Document>> SelectedItems { get { return from d in Items where d.IsChecked select d; } }

        #region DeleteButtonEnabled
        public bool DeleteButtonEnabled { get { return SelectedItems.Any(); } }
        #endregion // DeleteButtonEnabled
    }
}